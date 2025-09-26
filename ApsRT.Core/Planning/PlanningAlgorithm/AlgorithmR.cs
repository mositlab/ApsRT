using ApsRT.Core.Leftovers;
using ApsRT.Core.Planning.ParallelPlanning;
using ApsRT.Core.PlanSimulator;
using ApsRT.Core.PlanSimulator.PlanSimulatorEngine;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Taras.TaraSearchEngine;
using Azure;
using Google.OrTools.Sat;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UniversalPack.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;

namespace ApsRT.Core.Planning.PlanningAlgorithm
{
    internal class AlgorithmR
    {

        public PlanningAlgorithm basisLink;

        public AlgorithmR(PlanningAlgorithm basisLink)
        {
            this.basisLink = basisLink;
        }


        List<PlanSimulatorBlock> OpersCopy;

        FrozenDictionary<Uco, List<PlanSimulatorBlock>> BlocksDependingOnCenter;

        List<PlanSimulatorBlock> LinearPlan = new();

        HashSet<PlanSimulatorBlock> ExistingBlocks = new();

        // Словарик для хранения выбранных тар для правильного алгоритма
        Dictionary<int, Uco> PrevsInfoForR = new ();

        void UpdateTaraOnMod(PlanSimulatorMod mod, Uco tara)
        {
            foreach (var ooop in mod.StagesAsNormal)
            {
                ooop.CurrentTara = tara;
                PrevsInfoForR.Add(ooop.Index, ooop.CurrentTara);
            }
        }

        void AddBlockToPlan(PlanSimulatorBlock block, Uco center, Uco tara)
        {
            UpdateTaraOnMod(block.Lists[0], tara);
            block.SetCenter(center, 0);
            BlocksDependingOnCenter[center].Add(block);
            LinearPlan.Add(block);
            ExistingBlocks.Add(block);
            OpersCopy.Remove(block);
        }

        void AddBlockParallel(PlanSimulatorBlock block, Uco center, Uco tara, PlanSimulatorBlock mainBlock)
        {
            block.SetCenter(center, 0);

            var firstMod = block.Lists[0];

            firstMod.BlockLink = mainBlock;

            UpdateTaraOnMod(firstMod, tara);

            //var tara = firstMod.FirstOperJust.CurrentTara;

            mainBlock.Lists.Add(firstMod); // Добавляем мод
                                           //lastBlock.FillKoeff += 0.25f; // TODO: другое значение вставить, мб передать в виде аргумента
            //if (ThisTaraMustBeLast) lastBlock.FillKoeff = 1;
            if (!mainBlock.ExistingTaraHere.ContainsKey(tara))
            {
                mainBlock.ExistingTaraHere.Add(tara, 0);
            }

            mainBlock.ExistingTaraHere[tara]++;

            mainBlock.UpdatePossibleRCsHere(); // Обновляем инфу по центрам



            OpersCopy.Remove(block);


            Queue<PlanSimulatorBlock> blocks = new();
            blocks.Enqueue(mainBlock);

            while (blocks.Count > 0)
            {
                var currentBlock = blocks.Dequeue();

                if (!LinearPlan.Contains(currentBlock)) continue;

                currentBlock.BlockIndex[0] = Math.Max(currentBlock.PreviousBlockOnThisRC[0] == null ? -1 : currentBlock.PreviousBlockOnThisRC[0].BlockIndex[0], currentBlock.Lists.Max(list => list.PrevMods.Count == 0 ? -1 : list.PrevMods.Max(prevMod => prevMod.BlockLink.BlockIndex[0]))) + 1;

                if (currentBlock.NextBlockOnThisRC[0] != null && !blocks.Contains(currentBlock.NextBlockOnThisRC[0])) blocks.Enqueue(currentBlock.NextBlockOnThisRC[0]);


                var blocksToAdd = currentBlock.Lists
                    .Where(x => x != firstMod)
                    .SelectMany(x => x.NextMods)
                    .Distinct()
                    .Select(x => x.BlockLink)
                    .Distinct()
                    .Where(x => x != currentBlock.NextBlockOnThisRC[0]);

                foreach (var toAdd in blocksToAdd) if (!blocks.Contains(toAdd)) blocks.Enqueue(toAdd);

            }



            LinearPlan = LinearPlan.OrderBy(block => block.BlockIndex[0]).ToList();
        }

        class RFirstStageResult
        {
            public PlanSimulatorBlock Block;
            public Uco RC, Tara;
            public double Max;
            public bool ItsBadResult;
            public double Cost;

            public RFirstStageResult(PlanSimulatorBlock block, Uco rC, Uco tara, double max, bool itsBadResult, float cost)
            {
                Block = block;
                RC = rC;
                Tara = tara;
                Max = max;
                ItsBadResult = itsBadResult;
                Cost = cost;
            }
        }

        List<RFirstStageResult> Step1_1_PrepareFirstStages(PlanSimulatorBlock[] OpersForFirstLevelArray)
        {
            List<RFirstStageResult> firstResults = new();

            for (int i = 0; i < OpersForFirstLevelArray.Length; i++)
            {
                var mod = OpersForFirstLevelArray[i].Lists[0];

                TemporaryTaraSearchData data = null;

                bool Can1 = TaraSearchEngine.GetAllVariantsForThese(mod, out data, new SumIsPossibleOptions(false)
                {
                    PrevsInfo = PrevsInfoForR,
                    ModToTaraInfoLink = basisLink.ModToTaraInfo
                });

                bool anythingWas = false;
                if (Can1)
                {
                    for (int j = 0; j < data.PossibleCentersAndTaras.Length; j++)
                    {
                        Uco currentCenter = data.PossibleCentersAndTaras[j].RC;
                        Uco currentTara = data.PossibleCentersAndTaras[j].Tara;
                        if (TaraSearchEngine.FilterAllVariants(mod, data, out var tara, out var rc, out var MaxNamot, out _, out _, new SumIsPossibleOptions(false)
                        {
                            LogSomeErrors = false,
                            PrevsInfo = PrevsInfoForR,
                            RCMustBeThis = new HashSet<Uco>() { currentCenter },
                            TaraMustBeThis = new HashSet<Uco>() { currentTara },
                            ModToTaraInfoLink = basisLink.ModToTaraInfo
                        }))
                        {
                            firstResults.Add(new RFirstStageResult(mod.BlockLink, rc, tara, MaxNamot, false, 0));
                            anythingWas = true;
                        }
                    }
                }

                // Придётся задействовать самую простую технику, но именно что ошибочную.
                if (!Can1 || !anythingWas)
                {
                    foreach (var rc in mod.PossibleRCsHere)
                    {
                        firstResults.Add(new RFirstStageResult(mod.BlockLink, rc, mod.FirstOperJust.CurrentTara, mod.FirstOperJust.CurrentTaraMaxCapacity, true, 0));
                    }
                }


            }

            return firstResults;
        }

        void Step1_2_PrepareValues(List<RFirstStageResult> firstResults)
        {
            double minDelta = double.MaxValue;
            foreach (var link in firstResults)
            {
                var minDeadline = link.Block.Lists[0].StagesAsNormal.Min(x => x.OrderLink.Deadline);
                var desiredRC = link.RC;
                var blockDuration = link.Block.MaxTime(desiredRC);
                DateTime blockEnd = basisLink.StartOfPlanningDate + blockDuration;
                link.Cost = (blockEnd - minDeadline).TotalDays; // Получаем дельту. Если дедлайн был меньше, то и дельта будет положительная.
                if (minDelta > link.Cost) minDelta = link.Cost;
            }
            foreach (var link in firstResults)
            {
                if (minDelta < 0) link.Cost -= minDelta; // Увеличиваем на эту самую дельту, чтобы избавиться от нулей.
                link.Cost = (link.Cost * 1000) + 1; // Ну это чтобы не было 0, если пришёл в эту точку.
            }
        }

        void Step1_FirstStages()
        {
            Console.WriteLine("R: getting first ops...");
            var OpersForFirstLevelArray = OpersCopy.Where(block => block.Lists[0].FirstOperJust.Prevs.Count == 0).ToArray(); // Получаем те этапы, которые подходят именно в качестве первых.

            List<RFirstStageResult> firstResults = Step1_1_PrepareFirstStages(OpersForFirstLevelArray);

            Step1_2_PrepareValues(firstResults);

            HashSet<Uco> passedCenters = new();
            HashSet<PlanSimulatorBlock> passedBlocks = new();

            var OpersForFirstLevelForSort = firstResults
                .OrderBy(x => (x.ItsBadResult ? 1 : 0)) // Сначала уберем плохие результаты наверх
                .ThenBy(x => x.Cost) // А теперь уже и по цене отсортируем
                .ToArray();

            for (int i = 0; i < OpersForFirstLevelForSort.Length; i++)
            {
                var element = OpersForFirstLevelForSort[i];
                var center = element.RC;
                var tara = element.Tara;
                if (passedCenters.Contains(center) || passedBlocks.Contains(element.Block)) continue;

                AddBlockToPlan(element.Block, center, tara);

                passedCenters.Add(center);
                passedBlocks.Add(element.Block);
            }
        }

        public void Start(PlanningAlgorithmWorkflow workflow)
        {
            var start = DateTime.Now;

            OpersCopy = basisLink.UsedPool.CopyBlocksDeep(basisLink, basisLink.UsedPool.BestPoolIndex, true);

            Console.WriteLine("R, beg, " + OpersCopy.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Count());


            //.Where(x => x.Lists[0].FirstOperJust.Center.Label.Contains("EEL-40")).ToList();
            var poolCopy = new PlanSimulatorPool(basisLink.SessionLink, OpersCopy, basisLink.MinAcceptableDate, basisLink.UsedPool.PoolSize);
            poolCopy.PrepareOnOneRCData();
            poolCopy.PrepareAllRCsOnBlocks();
            BlocksDependingOnCenter = poolCopy.CentersData.ToFrozenDictionary(x => x.Key, x => new List<PlanSimulatorBlock>());

            // Сейчас лучший индекс - 0.

            // Получаем самый максимальный уровень, по которому только можно ходить.

            Step1_FirstStages(); // Поставим сначала первые этапы

            bool success = Step2_Process();

            if (success)
            {
                Console.WriteLine("R: successfully ending...");
                poolCopy.Blocks = LinearPlan;
                var ALL_COUNT = basisLink.UsedPool.Blocks.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Count();
                Console.WriteLine("R, end, " + ALL_COUNT);
                ALL_COUNT = basisLink.UsedPool.Blocks.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Distinct().Count();
                Console.WriteLine("R, end, " + ALL_COUNT);
                ;
            }
            else
            {
                ;
            }

            var end = DateTime.Now;

            Console.WriteLine($"R total time = {end - start}");
        }

        object[] lockers;
        ConcurrentQueue<int> AvailableIndexes;
        public object locker = new object();

        class RSearcher
        {
            public float minResult = float.MaxValue;
            public Uco ChoosenCenter = Uco.EmptyObject;
            public Uco ChoosenTara = Uco.EmptyObject;
            public double MaxNamotka = 0;
            public bool ThisResultIsBad = true;
            public PlanSimulatorMod ChoosenMod = null;
            public bool WillBeParr = false;
            public PlanSimulatorBlock parrBlock = null;
        }

        void Step2_1_Parallel(PlanSimulatorBlock[] PossibleOpersHere, RSearcher searcherClass)
        {
            var process = Parallel.For(0, PossibleOpersHere.Length, i =>
            {
                var mod = PossibleOpersHere[i].Lists[0];

                TemporaryTaraSearchData data = null;

                bool Can1 = TaraSearchEngine.GetAllVariantsForThese(mod, out data, new SumIsPossibleOptions(false) { PrevsInfo = PrevsInfoForR, ModToTaraInfoLink = basisLink.ModToTaraInfo });


                //if (Can1 && !mod.FirstOperJust.NoCheckNamotka && data.PossibleCentersAndTaras.Any(x => x.Tara.IsEmpty))
                //{
                //    ;
                //}

                bool anythingWas = false;
                if (Can1) anythingWas |= Step2_1_1_Check(mod, searcherClass, data);

                if (!Can1 || !anythingWas) Step2_1_2_Errored(mod, searcherClass);

            });
            while (!process.IsCompleted) ;
        }

        bool ParallelCheck(PlanSimulatorMod mod, RSearcher searcherClass, Uco currentCenter, Uco currentTara, double maxNamot, bool ForceThisTara)
        {
            bool result = false;
            if (!mod.FirstOperJust.CurrentTara.IsEmpty && mod.FirstOperJust.ParallelPlanningIDs.TryGetValue(currentCenter, out var parrID) && parrID != -1
            && basisLink.SessionLink.MasterDataLinks.ParallelPlanningMasterData.ParIDParams.TryGetValue(parrID, out var paramsHere)
            )
            {
                var possibleParallelOpers = BlocksDependingOnCenter[currentCenter]
                .Where(block =>

                block.Lists[0].FirstOperJust.ParallelPlanningIDs.TryGetValue(currentCenter, out var parrIDToCheck) && parrIDToCheck == parrID // Сначала проверим, чтоб у блока был тот же айдишник, что и нужен нам

                &&

                /*
                 a = block.Lists[0].AllTime[currentCenter]
                 b = mod.AllTime[currentCenter]
                 c = block.MaxTime(currentCenter)
                 Нужно проверить эти условия:
                 c - b >= 0 - это значит, что текущее моделирование по длительности не уступает тому, что уже там есть, и его можно юзать
                Чтобы использовать вместе с дельтой, нужно сделать так:
                c - b <= paramsHere.Delta
                А чтобы правило работало в обе стороны, юзаем math.abs:
                */
                Math.Abs((block.MaxTime(currentCenter) - mod.AllTime[currentCenter]).TotalSeconds) <= paramsHere.Delta
                //paramsHere.Delta

                &&

                // Надо ещё, чтобы сюда влазила тара вся.
                basisLink.SessionLink.MasterDataLinks.ParallelPlanningMasterData.GetMostFreeTara(
                currentCenter,
                mod.FirstOperJust.OperationVid,
                parrID,
                mod.BlockLink.Lists.Count, //TODO: у общих этапов их количество может быть больше
                block.ExistingTaraHere,
                currentTara,//mod.FirstOperJust.CurrentTara,
                out var resultat
                ) >= 0


                ).ToArray();

                if (possibleParallelOpers.Length > 0)
                {
                    (PlanSimulatorBlock block, double crit1, double crit2, float TotalCost)[] variants = possibleParallelOpers.Select(block =>
                    {
                        var deltaBetweenInnerOps = Math.Abs((block.MaxTime(currentCenter) - mod.AllTime[currentCenter]).TotalSeconds); // Приходится заюзать то, что я описал выше, потому что близость операций можно определить по дельте между ними
                        deltaBetweenInnerOps = Math.Round(deltaBetweenInnerOps); // На будущее - уберу дробную часть


                        var dateDeltaNoAbs = (mod.MinDatesData[0].MinValue - block.Lists[0].DateOfStart[0]).TotalSeconds;
                        var dateDelta = Math.Abs(dateDeltaNoAbs);
                        dateDelta = Math.Round(dateDelta);

                        var prostoi = dateDeltaNoAbs < 0 ? -dateDeltaNoAbs : 0;

                        return (block, crit1: dateDelta, crit2: deltaBetweenInnerOps, TotalCost: (float)(dateDelta + deltaBetweenInnerOps / 10000f));
                    })
                    .OrderBy(x => x.crit1)
                    .ThenBy(y => y.crit2)
                    .ToArray();

                    result = true;
                    var parrBlockVariant = variants.First();
                    float localResult = parrBlockVariant.TotalCost;
                    lock (locker)
                    {
                        bool ThisWillBrokeMyResult = ForceThisTara && !searcherClass.ThisResultIsBad;
                        bool IneedItBcsItsGood = !ForceThisTara && searcherClass.ThisResultIsBad;

                        if ((searcherClass.minResult > localResult || IneedItBcsItsGood)
                           &&
                           !ThisWillBrokeMyResult
                           )
                        {
                            searcherClass.ThisResultIsBad = ForceThisTara;
                            searcherClass.WillBeParr = true;
                            searcherClass.parrBlock = parrBlockVariant.block;
                            searcherClass.minResult = localResult;
                            searcherClass.ChoosenMod = mod;
                            searcherClass.MaxNamotka = ForceThisTara ? mod.FirstOperJust.CurrentTaraMaxCapacity : maxNamot;
                            searcherClass.ChoosenCenter = currentCenter;
                            searcherClass.ChoosenTara = currentTara;
                        }
                    }
                }
            }
            return result;
        }

        bool Step2_1_1_Check(PlanSimulatorMod mod, RSearcher searcherClass, TemporaryTaraSearchData data)
        {
            bool result = false;
            var possibleRCs = data.PossibleCentersAndTaras;
            for (int j = 0; j < possibleRCs.Length; j++)
            {
                var currentCenter = possibleRCs[j].RC;
                var currentTara = possibleRCs[j].Tara;
                if (TaraSearchEngine.FilterAllVariants(mod, data, out var tara, out var rc, out var MaxNamot, out _, out _, new SumIsPossibleOptions(false)
                {
                    LogSomeErrors = false,
                    PrevsInfo = PrevsInfoForR,
                    RCMustBeThis = new HashSet<Uco>() { currentCenter },
                    TaraMustBeThis = new HashSet<Uco>() { currentTara },
                    ModToTaraInfoLink = basisLink.ModToTaraInfo
                }))
                {
                    result |= ParallelCheck(mod, searcherClass, currentCenter, currentTara, MaxNamot, false);
                }
            }
            return result;
        }

        void Step2_1_2_Errored(PlanSimulatorMod mod, RSearcher searcherClass)
        {
            var possibleRCs = mod.PossibleRCsHere;
            //var subProcess = Parallel.For(0, possibleRCs.Count, j =>
            for (int j = 0; j < possibleRCs.Count; j++)
            {
                var currentCenter = possibleRCs[j];
                ParallelCheck(mod, searcherClass, currentCenter, mod.FirstOperJust.CurrentTara, mod.FirstOperJust.CurrentTaraMaxCapacity, true);
            }
            //);
            //while (!subProcess.IsCompleted) ;
        }

        void Step2_2_Linear(PlanSimulatorBlock[] PossibleOpersHere, RSearcher searcherClass)
        {
            if (true || !searcherClass.WillBeParr)
            {
                var process = Parallel.For(0, PossibleOpersHere.Length, i =>
                {
                    var mod = PossibleOpersHere[i].Lists[0];

                    TemporaryTaraSearchData data = null;

                    bool Can1 = TaraSearchEngine.GetAllVariantsForThese(mod, out data, new SumIsPossibleOptions(false) {
                        PrevsInfo = PrevsInfoForR,
                        ModToTaraInfoLink = basisLink.ModToTaraInfo
                    });

                    //if (Can1 && !mod.FirstOperJust.NoCheckNamotka && data.PossibleCentersAndTaras.Any(x => x.Tara.IsEmpty))
                    //{
                    //    ;
                    //}

                    bool anythingWas = false;
                    if (Can1) anythingWas |= Step2_2_1_TaraAndRCsCheck(mod, searcherClass, data);

                    //var was = searcherClass.ChoosenCenter;
                    if (!Can1 || !anythingWas)
                    {
                        Step2_2_2_ForcedCheckNoTara(mod, searcherClass, data);
                    }
                });
                while (!process.IsCompleted) ;
            }
        }

        bool Step2_2_1_TaraAndRCsCheck(PlanSimulatorMod mod, RSearcher searcherClass, TemporaryTaraSearchData data)
        {
            bool result = false;
            var possibleRCs = data.PossibleCentersAndTaras;
            //var subProcess = 
            //    Parallel.For(0, possibleRCs.Length, j =>
            for (int j = 0; j < possibleRCs.Length; j++)
            {
                var currentCenter = possibleRCs[j].RC;
                var currentTara = possibleRCs[j].Tara;
                result |= TaraAndRCsSubchecker(mod, searcherClass, data, currentCenter, currentTara, false);
            }
            return result;
            //);
            //while (!subProcess.IsCompleted)
            //{
            //    ;
            //}
        }

        void Step2_2_2_ForcedCheckNoTara(PlanSimulatorMod mod, RSearcher searcherClass, TemporaryTaraSearchData data)
        {
            var possibleRCs = mod.PossibleRCsHere;
            //var subProcess = Parallel.For(0, possibleRCs.Count, j =>
            for (int j = 0; j < possibleRCs.Count; j++)
            {
                var currentCenter = possibleRCs[j];
                TaraAndRCsSubchecker(mod, searcherClass, data, currentCenter, mod.FirstOperJust.CurrentTara, true);
            }
            //);
            //while (!subProcess.IsCompleted)
            //{
            //    ;
            //}
        }

        bool TaraAndRCsSubchecker(PlanSimulatorMod mod, RSearcher searcherClass, TemporaryTaraSearchData data, Uco currentCenter, Uco currentTara, bool ForceThisTara)
        {
            bool result = false;
            while (AvailableIndexes.Count == 0)
            {
                ;
            }
            int IndexToWorkWith = 0;
            while (!AvailableIndexes.TryDequeue(out IndexToWorkWith))
            {
                ;
            }
            lock (lockers[IndexToWorkWith])
            {
                Uco tara = Uco.EmptyObject;
                Uco rc = Uco.EmptyObject;
                double MaxNamot = 0;
                if (ForceThisTara || TaraSearchEngine.FilterAllVariants(mod, data, out tara, out rc, out MaxNamot, out _, out _, new SumIsPossibleOptions(false)
                {
                    LogSomeErrors = false,
                    PrevsInfo = PrevsInfoForR,
                    RCMustBeThis = new HashSet<Uco>() { currentCenter },
                    TaraMustBeThis = new HashSet<Uco>() { currentTara },
                    ModToTaraInfoLink = basisLink.ModToTaraInfo
                }))
                {
                    result = true; // Значит я так могу.
                    float localResult = HeuristicCheckBlock(mod, currentCenter, IndexToWorkWith/*, list*/);
                    bool ThisWillBrokeMyResult = ForceThisTara && !searcherClass.ThisResultIsBad;
                    bool IneedItBcsItsGood = !ForceThisTara && searcherClass.ThisResultIsBad;
                    lock (locker)
                    {
                        if ((searcherClass.minResult > localResult || IneedItBcsItsGood)
                            &&
                            !ThisWillBrokeMyResult
                            )
                        {
                            searcherClass.minResult = localResult;
                            searcherClass.ChoosenMod = mod;
                            searcherClass.ChoosenCenter = ForceThisTara ? currentCenter : rc;
                            searcherClass.ChoosenTara = ForceThisTara ? currentTara : tara;
                            searcherClass.MaxNamotka = ForceThisTara ? mod.FirstOperJust.CurrentTaraMaxCapacity : MaxNamot;
                            searcherClass.WillBeParr = false;
                            searcherClass.ThisResultIsBad = ForceThisTara;
                        }
                    }
                }

                AvailableIndexes.Enqueue(IndexToWorkWith);
            }

            return result;
        }

        bool Step2_Process()
        {
            Console.WriteLine("R: get max level...");
            int MaxLevel = OpersCopy.Max(x => x.Lists.Max(y => y.StagesAsNormal.Max(z => z.Level)));


            PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngineResult LastResult = PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(LinearPlan, 0, basisLink.StartOfPlanningDate, null);

            basisLink.UpdateBcsSuccess(LastResult, LinearPlan, 0);

            int CurrentLevel = 2; // Потому что мы первый уровень уже прошли, полагается.
            //CurrentLevel = 1; // Не пытайся поставить начало на 1, потому что почему-то это ухудшает план на 1-2 дня.


            int LevelCounter = 0;
            int MaxLevelCounter = basisLink.UsedPool.Blocks.Count;

            int ErroredIterationsCount = 0;

            bool Success = false;

            object globalLocker = new();


            Console.WriteLine("R: prepare lockers...");
            lockers = new object[basisLink.UsedPool.PoolSize];
            for (int i = 0; i < lockers.Length; i++) lockers[i] = new();
            AvailableIndexes = new ConcurrentQueue<int>();
            for (int i = 1; i < basisLink.UsedPool.PoolSize - 3; i++) // -1 потому что последний пул нужен для ручного анализа.
            {
                //if (i == bestIndex) continue;
                AvailableIndexes.Enqueue(i);
            }

            Console.WriteLine("R: calculating...");
            while (OpersCopy.Count > 0)
            {
                var PossibleOpersHere = OpersCopy.Where(x => x.Lists[0].FirstOperJust.Level == CurrentLevel

                &&

                x.Lists[0].PrevMods.All(mod => ExistingBlocks.Contains(mod.BlockLink)) // Проверяем на хронологию

                &&

                !ExistingBlocks.Contains(x)

                ).ToArray();

                if (PossibleOpersHere.Length == 0)
                {
                    ErroredIterationsCount++;
                    if (ErroredIterationsCount == MaxLevel * 2)
                    {
                        break;
                    }
                }

                if (PossibleOpersHere.Length > 0)
                {
                    if (basisLink.WannaStop)
                    {
                        break;
                    }

                    RSearcher searcherClass = new();

                    Step2_1_Parallel(PossibleOpersHere, searcherClass); // Попробуем параллель поискать

                    Step2_2_Linear(PossibleOpersHere, searcherClass); // А теперь, если не было параллелизма, обычный варик

                    if (searcherClass.WillBeParr)
                    {
                        AddBlockParallel(searcherClass.ChoosenMod.BlockLink, searcherClass.ChoosenCenter, searcherClass.ChoosenTara, searcherClass.parrBlock);
                    }
                    else
                    {
                        AddBlockToPlan(searcherClass.ChoosenMod.BlockLink, searcherClass.ChoosenCenter, searcherClass.ChoosenTara);
                    }

                    if (OpersCopy.Count == 0) Success = true;

                    ErroredIterationsCount = 0;

                    LastResult = PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(LinearPlan, 0, basisLink.StartOfPlanningDate, null); // Вызывем симулятор плана
                    
                    basisLink.UpdateBcsSuccess(LastResult, LinearPlan, 0);
                    basisLink.UsedWorkflow.Generation++;

                    basisLink.AlgorithmCallback?.Invoke(basisLink.CurrentStage, LinearPlan.Count);
                }

                LevelCounter++;
                if (LevelCounter >= MaxLevelCounter || PossibleOpersHere.Length == 0)
                {
                    LevelCounter = 0;
                    CurrentLevel++; // Обновляем уровень
                    if (CurrentLevel > MaxLevel) CurrentLevel = 0;
                }
            }

            return Success;

        }

        //float Step2_1_CheckBlockParallel(PlanSimulatorMod mod, Uco choosenCenter, int poolIndex, PlanSimulatorBlock blockToInsert)
        //{

        //}

        float HeuristicCheckBlock(PlanSimulatorMod mod, Uco choosenCenter, int poolIndex/*, List<PlanSimulatorBlock> list*/)
        {
            var list = BlocksDependingOnCenter[choosenCenter];
            //var swallowCopy = PlanSimulatorPool.CopyBlocksSwallow(LinearPlan);

            /*
             new List<PlanSimulatorBlock>()
            {
                mod.BlockLink
            };
             */
            //mod.BlockLink.SetCenter(choosenCenter, poolIndex);
            //PlanSimulatorPool.MoveCentersInAllBlocks(swallowCopy, 0, poolIndex);
            //PlanSimulatorPool.InsertBlockSafe(swallowCopy, mod.BlockLink, swallowCopy.Count, poolIndex, 0);
            //PlanSimulatorPool.MoveMinDatesInAllBlocks(swallowCopy, 0, poolIndex);

            //var res = PlanSimulatorEngineResult.CreateFullInstance(basisLink.SessionLink, basisLink.StartOfPlanningDate);
            //PlanSimulatorEngineResult localResult = PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(swallowCopy, poolIndex, basisLink.StartOfPlanningDate, res, StartFrom: swallowCopy.Count - 1, LinearRC: false);

            //var res1 = (float)(mod.DateOfEnd[poolIndex] - basisLink.MinAcceptableDate).TotalSeconds;

            var lastBlock = list.Count == 0 ? null : list[^1];
            var operationA = list.Count == 0 ? null : lastBlock.Lists[0]?.FirstOper;

            var minDate = mod.PrevMods.Count == 0 ? basisLink.StartOfPlanningDate : mod.PrevMods.Max(x => x.DateOfEnd[0]);
            if (minDate == DateTime.MinValue) minDate = basisLink.StartOfPlanningDate; // Получаем минимальную дату

            var lastCenterDate = lastBlock == null ? basisLink.StartOfPlanningDate : lastBlock.Lists[0].DateOfEnd[0];

            TimeSpan prostoiDur = minDate - lastCenterDate;

            var rechangesResult = operationA == null ? null : basisLink.SessionLink.MasterDataLinks.RechangesMasterData.FindRechanges(new(lastBlock.GetCenterData(0).CenterDataLink, mod.MinDatesData[0], operationA.Stage, mod.FirstOperJust, choosenCenter));

            TimeSpan rechesDur = rechangesResult == null ? new TimeSpan() : TimeSpan.FromSeconds(rechangesResult.Duration);

            // Чтобы сделать только переналадки, хватит вот этого критерия:
            //return (float)rechesDur.TotalSeconds;

            prostoiDur -= rechesDur;

            if (prostoiDur.Ticks < 0) prostoiDur = TimeSpan.Zero;

            var time1 = lastCenterDate + prostoiDur + rechesDur;

            var lastDate = time1 + mod.AllTime[choosenCenter];

            //var res1 = (float)(time1 - minDate).TotalSeconds;

            var res2 = (float)/*rechesDur.TotalSeconds;//*/(lastDate - basisLink.MinAcceptableDate).TotalSeconds;

            return res2;

        }

    }
}

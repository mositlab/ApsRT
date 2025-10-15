using ApsRT.Core.Leftovers;
using ApsRT.Core.NaladLengths;
using ApsRT.Core.Planning.ParallelPlanning;
using ApsRT.Core.PlanSimulator;
using ApsRT.Core.PlanSimulator.PlanSimulatorEngine;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Stages;
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

        double gamma;

        List<PlanSimulatorBlock> OpersCopy;

        FrozenDictionary<Uco, List<PlanSimulatorBlock>> BlocksDependingOnCenter;

        List<PlanSimulatorBlock> LinearPlan = new();

        HashSet<PlanSimulatorBlock> ExistingBlocks = new();

        // Словарик для хранения выбранных тар для правильного алгоритма
        Dictionary<int, Uco> PrevsInfoForR = new ();


        Dictionary<int, Uco> OneRCGroupsForR = new (); // Ограничители "На Одном РЦ"

        int FixedCompleted = 0, VPZCompleted = 0;

        void AddOneRCData(PlanSimulatorMod firstMod, Uco center)
        {

            if (firstMod.FirstOperJust.OnOneRC
                && firstMod.BlockLink.PoolLink.ModIDToOnOneRCGroupID.TryGetValue(firstMod.ModID, out var groupID1))
            {
                OneRCGroupsForR.TryAdd(groupID1, center);
            }
        }

        void AddFixedOrVPZ(PlanSimulatorMod firstMod)
        {
            if (firstMod.IsFixed)
            {
                FixedCompleted++;
                VPZCompleted++;
            }
            else
            {
                if (firstMod.IsFactOrVPZ)
                {
                    VPZCompleted++;
                }
            }
            //if (firstMod.IsFixed)
            //{
            //    basisLink.GLOBAL_FIXED_MIN--;
            //    basisLink.GLOBAL_VPZ_MAX--;
            //}
            //else
            //{
            //    if (firstMod.IsFactOrVPZ)
            //    {
            //        basisLink.GLOBAL_VPZ_MAX--;
            //    }
            //}
        }

        void UpdateTaraOnMod(PlanSimulatorMod mod, Uco tara)
        {
            foreach (var ooop in mod.StagesAsNormal)
            {
                ooop.CurrentTara = tara;
                if (tara.IsEmpty) continue;
                PrevsInfoForR.Add(ooop.Index, ooop.CurrentTara);
            }
        }

        void AddBlockToPlan(PlanSimulatorBlock block, Uco center, Uco tara)
        {
            var firstMod = block.Lists[0];
            UpdateTaraOnMod(firstMod, tara);
            block.SetCenter(center, 0);
            AddOneRCData(firstMod, center);
            BlocksDependingOnCenter[center].Add(block);
            LinearPlan.Add(block);
            ExistingBlocks.Add(block);
            OpersCopy.Remove(block);
            AddFixedOrVPZ(firstMod);
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

            AddFixedOrVPZ(firstMod);

            AddOneRCData(firstMod, center);

            OpersCopy.Remove(block);


            Queue<PlanSimulatorBlock> blocks = new();
            blocks.Enqueue(mainBlock);

            HashSet<(PlanSimulatorBlock from, PlanSimulatorBlock what)> alreadyHaveIt = new();

            var list = BlocksDependingOnCenter[center];

            var point = list.IndexOf(mainBlock);
            var point2 = list.IndexOf(block);

            int startIndex = mainBlock.BlockIndex[0];

            for (int i = startIndex; i < LinearPlan.Count; i++)
            {
                var currentBlock = LinearPlan[i];

                int prevBlockIndex = currentBlock.PreviousBlockOnThisRC[0] == null ? -1 : currentBlock.PreviousBlockOnThisRC[0].BlockIndex[0];

                if (prevBlockIndex >= LinearPlan.Count - 1)
                {
                    ;
                }

                int worstPrev = currentBlock.Lists.Max(list => list.PrevMods.Count == 0 ? -1 : list.PrevMods.Max(prevMod => prevMod.BlockLink.BlockIndex[0]));

                if (worstPrev >= LinearPlan.Count - 1)
                {
                    ;
                }

                currentBlock.BlockIndex[0] = Math.Max(prevBlockIndex, worstPrev) + 1;
            }


            //Console.WriteLine("-----------------------");

            //while (blocks.Count > 0)
            //{
            //    var currentBlock = blocks.Dequeue();

            //    if (!LinearPlan.Contains(currentBlock)) continue;

            //    //Console.WriteLine(currentBlock.ToString());
            //    var listLocal = BlocksDependingOnCenter[center];

            //    int prevBlockIndex = currentBlock.PreviousBlockOnThisRC[0] == null ? -1 : currentBlock.PreviousBlockOnThisRC[0].BlockIndex[0];

            //    if (prevBlockIndex >= LinearPlan.Count - 1)
            //    {
            //        ;
            //    }

            //    int worstPrev = currentBlock.Lists.Max(list => list.PrevMods.Count == 0 ? -1 : list.PrevMods.Max(prevMod => prevMod.BlockLink.BlockIndex[0]));

            //    if (worstPrev >= LinearPlan.Count - 1)
            //    {
            //        ;
            //    }

            //    currentBlock.BlockIndex[0] = Math.Max(prevBlockIndex, worstPrev) + 1;

            //    //if (currentBlock.NextBlockOnThisRC[0] == null)
            //    //{
            //    //    ;
            //    //}

            //    if (currentBlock.NextBlockOnThisRC[0] != null && !blocks.Contains(currentBlock.NextBlockOnThisRC[0]) && LinearPlan.Contains(currentBlock.NextBlockOnThisRC[0]))
            //    {
            //        var key = (currentBlock, currentBlock.NextBlockOnThisRC[0]);
            //        if (alreadyHaveIt.Contains(key))
            //        {
            //            ;
            //        }
            //        alreadyHaveIt.Add(key);
            //        Console.WriteLine($"БЛОК [ {currentBlock} ] ---> сосед [ {currentBlock.NextBlockOnThisRC[0]} ]");
            //        blocks.Enqueue(currentBlock.NextBlockOnThisRC[0]);
            //    }

            //    var blocksToAdd = currentBlock.Lists
            //        .Where(x => x != firstMod)
            //        .SelectMany(x => x.NextMods)
            //        .Distinct()
            //        .Select(x => x.BlockLink)
            //        .Distinct()
            //        .Where(x => LinearPlan.Contains(x) && x != currentBlock.NextBlockOnThisRC[0]);

            //    foreach (var toAdd in blocksToAdd)
            //    {
            //        if (!blocks.Contains(toAdd))
            //        {
            //            var key = (currentBlock, toAdd);
            //            if (alreadyHaveIt.Contains(key))
            //            {
            //                ;
            //            }
            //            Console.WriteLine($"БЛОК [ {currentBlock} ] ---> ребёнок [ {toAdd} ]");
            //            alreadyHaveIt.Add(key);
            //            blocks.Enqueue(toAdd);
            //        }
            //    }

            //}



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
                .ThenBy(x => x.Block.Lists[0].IsFixed ? 0 : (x.Block.Lists[0].IsFactOrVPZ ? 1 : 2)) // Расставим ещё и в порядке "фикс.-впз.-свобод."
                .ThenBy(x => x.Block.Lists[0].IsFixed && x.Block.Lists[0].FirstOperJust.StartTimeFact.HasValue ? (x.Block.Lists[0].FirstOperJust.StartTimeFact.Value - basisLink.StartOfPlanningDate).TotalSeconds : x.Cost) // А теперь уже и по цене отсортируем
                .ToArray();

            for (int i = 0; i < OpersForFirstLevelForSort.Length; i++)
            {
                var element = OpersForFirstLevelForSort[i];
                var center = element.RC;
                var tara = element.Tara;
                if (passedCenters.Contains(center) || passedBlocks.Contains(element.Block)) continue;

                //if (LinearPlan.Count < basisLink.GLOBAL_FIXED_MIN && !element.Block.Lists[0].IsFixed) continue;
                //if (LinearPlan.Count < basisLink.GLOBAL_FIXED_MIN && !element.Block.Lists[0].IsFixed) continue;

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


            gamma = basisLink.SessionLink.SettingsController.CurrentSettings.TGamma;

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
                Console.WriteLine("R, count f, " + ALL_COUNT);
                ALL_COUNT = basisLink.UsedPool.Blocks.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Distinct().Count();
                Console.WriteLine("R, count f, " + ALL_COUNT);

                Step3_PostChecker();

                ;
            }
            else
            {
                ;
            }

            var end = DateTime.Now;

            Console.WriteLine($"R total time = {end - start}");
        }

        void Step3_PostChecker()
        {
            Console.WriteLine("R, chronology check... ");
            // 1-е условие выполнимости плана. Условие хронологии.
            // Индекс блока каждого этапа должен быть меньше, чем индекс блока этапа, зависимого от рассматриваемого.
            bool ChronologyCheck = basisLink.CheckChronology(LinearPlan, out _, out _);
            Console.WriteLine(ChronologyCheck);

            // 2-е условие. Условие пустых тар
            // Каждый этап, имеющий один из признаков "НеПроверятьНамотку", "ВспомогательнаяОперация", должен иметь текущую тару, равную "ПУСТО", а отстутсвие этих признаков обязывает иметь текущую тару, НЕ равную "ПУСТО".
            Console.WriteLine("R, empty taras check... ");
            bool emptyTarasCheck = EmptyTarasCheck(LinearPlan, out int errIndex);
            Console.WriteLine($"{emptyTarasCheck}, {errIndex}");

            // 3-е условие. Условие выполнения признака "НеМенятьТару"
            // Каждый этап, имеющий признак "НеМенятьТару", должен иметь ту же назначенную тару, что и его предыдущие этапы.
            Console.WriteLine("R, no change tara check... ");
            int noChangeTaraResult = NoChangeTaraCheck(LinearPlan, out int errIndex2);
            Console.WriteLine($"{noChangeTaraResult}, {errIndex2}");


            ;
        }

        bool EmptyTarasCheck(List<PlanSimulatorBlock> blocks, out int FirstErroredBlock)
        {
            FirstErroredBlock = -1;
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                foreach (var mod in block.Lists)
                {
                    foreach (var stage in mod.StagesAsNormal)
                    {
                        if (stage.IsVspom || stage.NoCheckNamotka)
                        {
                            if (!stage.CurrentTara.IsEmpty)
                            {
                                FirstErroredBlock = i;
                                return false;
                            }
                        }
                        else
                        {
                            if (stage.CurrentTara.IsEmpty)
                            {
                                FirstErroredBlock = i;
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        int NoChangeTaraCheck(List<PlanSimulatorBlock> blocks, out int FirstErroredBlock)
        {
            FirstErroredBlock = -1;
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                foreach (var mod in block.Lists)
                {
                    foreach (var stage in mod.StagesAsNormal)
                    {
                        if (stage.NoChangeTara)
                        {
                            var prevs = stage.Prevs
                                .Select(index => basisLink.SessionLink.MasterDataLinks.StagesMasterData.AllStages[index])
                                .Where(stage => !stage.IsVspom && !stage.NoCheckNamotka) // Это относится только к этапам, которые не вспомогательные, и проверяют намотку
                                .Select(stage => stage.CurrentTara).ToHashSet(); // Берём у них текущую тару

                            if (prevs.Count == 0) continue;

                            if (prevs.Count > 1)
                            {
                                if (prevs.All(prev => prev == stage.CurrentTara)) continue;

                                FirstErroredBlock = i;
                                return 2;
                            }
                            else
                            {
                                if (prevs.First() != stage.CurrentTara)
                                {
                                    FirstErroredBlock = i;
                                    return 1;
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }

        object[] lockers;
        ConcurrentQueue<int> AvailableIndexes;
        public object locker = new object();


        //class RWriterClass
        //{
        //    public int IterationIndex;
        //    public int ChoosenStageIndex;
        //    public int PreviousStageIndex;
        //    public Uco ChoosenRC;
        //    public int HowMuchLeft;
        //    public double CriteriaValue;
        //}
        //Dictionary<(int ID, Uco RC, Uco Tara, bool Parr), (float Value, bool IsGood)> RWriterElements = new();


        struct RTempResult
        {
            public TimeSpan Rechanges;
            public TimeSpan DeltaFromStart;
            public TimeSpan DeltaFromStartNoOp;
            public TimeSpan DeltaFromDeadline;
            public float TotalCost;
            public bool IsFromSameOrder;
            public int OrderPriority;
            public int Level;

            public int ErrorsCount;

            public RTempResult(TimeSpan rechanges, TimeSpan deltaFromStart, TimeSpan deltaFromStartNoOp, TimeSpan deltaFromDeadline, int OrderPriority, bool isFromSameOrder, int level, float totalCost, int errorsCount)
            {
                Rechanges = rechanges;
                DeltaFromStart = deltaFromStart;
                TotalCost = totalCost;
                DeltaFromDeadline = deltaFromDeadline;
                IsFromSameOrder = isFromSameOrder;
                this.OrderPriority = OrderPriority;
                Level = level;
                ErrorsCount = errorsCount;
                DeltaFromStartNoOp = deltaFromStartNoOp;
            }

            public bool IsBetterThanOther(RTempResult otherResult, bool NoSteps)
            {
                if (otherResult.TotalCost == float.MaxValue && TotalCost < float.MaxValue) return true;

                if (ErrorsCount > otherResult.ErrorsCount) return false;

                if (NoSteps) return DeltaFromStart < otherResult.DeltaFromStart;

                //return
                //    Rechanges < otherResult.Rechanges
                //    || (Rechanges == otherResult.Rechanges && DeltaFromStart < otherResult.DeltaFromStart);

                return
                    OrderPriority < otherResult.OrderPriority
                    || (OrderPriority == otherResult.OrderPriority && TotalCost < otherResult.TotalCost)
                    || (OrderPriority == otherResult.OrderPriority && TotalCost == otherResult.TotalCost && Rechanges < otherResult.Rechanges)
                    || (OrderPriority == otherResult.OrderPriority && TotalCost == otherResult.TotalCost && Rechanges == otherResult.Rechanges && DeltaFromDeadline < otherResult.DeltaFromDeadline)
                    || (OrderPriority == otherResult.OrderPriority && TotalCost == otherResult.TotalCost && Rechanges == otherResult.Rechanges && DeltaFromDeadline == otherResult.DeltaFromDeadline && IsFromSameOrder && !otherResult.IsFromSameOrder)
                    || (OrderPriority == otherResult.OrderPriority && TotalCost == otherResult.TotalCost && Rechanges == otherResult.Rechanges && DeltaFromDeadline == otherResult.DeltaFromDeadline && IsFromSameOrder == otherResult.IsFromSameOrder && Level < otherResult.Level)
                    ;

                //return Rechanges < otherResult.Rechanges || (Rechanges == otherResult.Rechanges && TotalCost < otherResult.TotalCost);
                
                //if (otherResult.Rechanges > Rechanges) return true;

                //return DeltaFromStart < otherResult.DeltaFromStart;
            }

        }

        class RSearcher
        {
            public RTempResult minResult = new RTempResult(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, int.MaxValue, false, int.MaxValue, float.MaxValue, int.MaxValue);
            public Uco ChoosenCenter = Uco.EmptyObject;
            public Uco ChoosenTara = Uco.EmptyObject;
            public double MaxNamotka = 0;
            public bool ThisResultIsBad = true;
            public PlanSimulatorMod ChoosenMod = null;
            public bool WillBeParr = false;
            public PlanSimulatorBlock parrBlock = null;
        }

        void Step2_1_Parallel(PlanSimulatorBlock[] PossibleOpersHere, RSearcher searcherClass, PlanSimulatorEngineResult LastResult)
        {
            var process = Parallel.For(0, PossibleOpersHere.Length, i =>
            {
                var mod = PossibleOpersHere[i].Lists[0];

                TemporaryTaraSearchData data = null;

                bool Can1 = TaraSearchEngine.GetAllVariantsForThese(mod, out data, new SumIsPossibleOptions(false) { PrevsInfo = PrevsInfoForR, ModToTaraInfoLink = basisLink.ModToTaraInfo });

                (List<Uco> allowedRC, Uco? neededThisRC) = GetOneRCInfo(mod);

                bool anythingWas = false;
                if (Can1) anythingWas |= Step2_1_1_Check(mod, searcherClass, data, neededThisRC, allowedRC, LastResult);

                if (!Can1 || !anythingWas) Step2_1_2_Errored(mod, searcherClass, LastResult);

            });
            while (!process.IsCompleted) ;
        }

        bool ParallelCheck(PlanSimulatorMod mod, RSearcher searcherClass, Uco currentCenter, Uco currentTara, double maxNamot, bool ForceThisTara, PlanSimulatorEngineResult LastResult)
        {
            bool result = false;
            if (!mod.FirstOperJust.CurrentTara.IsEmpty && mod.FirstOperJust.ParallelPlanningIDs.TryGetValue(currentCenter, out var parrID) && parrID != -1
            && basisLink.SessionLink.MasterDataLinks.ParallelPlanningMasterData.ParIDParams.TryGetValue(parrID, out var paramsHere)
            )
            {
                var NaladLengthsMasterData = basisLink.SessionLink.MasterDataLinks.NaladLengthsMasterData;
                var possibleParallelOpers = BlocksDependingOnCenter[currentCenter]
                .Where(block =>
                {
                    var rechangesResult = LastResult.RechangesBeforeFirstStages[block.Lists[0].FirstOperJust.Index].RechangeItself;
                    bool asFirst = NaladLengthsMasterData.ItsFirstLaunch(rechangesResult, block.PreviousBlockOnThisRC[0]?.Lists[0]);
                    var OperB = mod.FirstOperJust;
                    GottenNaladLengthStruct naladLengths = NaladLengthsMasterData.GetNaladLengthsForCurrentStageOnly(OperB, currentCenter, asFirst);
                    /*

            if 
            {
                itsBadResult = true;
            }
                     */

                    return

                    (!OperB.NeededNaladLength.HasValue || NaladLengthsMasterData.NaladLengthsConverted(naladLengths.NaladLengthHere, OperB.EdIzmClassificator, OperB.KgKm).Value <= OperB.NeededNaladLength.Value.Length.Value)

                    &&


                    block.Lists[0].IsFixed == mod.IsFixed
                    &&
                    block.Lists[0].IsFactOrVPZ == mod.IsFactOrVPZ
                    &&
                    block.Lists[0].FirstOperJust.StartTimeFact == mod.FirstOperJust.StartTimeFact // Ну до кучи проверим ещё и это

                    &&

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
                    ) >= 0;


                }

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
                    float localResult0 = parrBlockVariant.TotalCost;

                    var dateOfEnd = basisLink.StartOfPlanningDate + TimeSpan.FromSeconds(parrBlockVariant.crit1 + Math.Max(parrBlockVariant.block.MaxTime(currentCenter).TotalSeconds, mod.AllTime[currentCenter].TotalSeconds));

                    var minDeadline = mod.StagesAsNormal.Min(x => x.OrderLink.Deadline);
                    var minDeadline2 = parrBlockVariant.block.Lists.Min(list => list.StagesAsNormal.Min(stage => stage.OrderLink.Deadline));
                    if (minDeadline2 < minDeadline) minDeadline = minDeadline2;

                    HashSet<OrderID> orders = mod.OrdersHere;//StagesAsNormal.Select(x => x.OrderLink.OrderID).ToHashSet();
                    bool IsFromSameOrder = parrBlockVariant.block.Lists.Any(list => list.StagesAsNormal.Any(stage => orders.Contains(stage.OrderLink.OrderID)));


                    RTempResult localResult = new RTempResult(TimeSpan.Zero /*переналадки уже уплочены*/, dateOfEnd - basisLink.StartOfPlanningDate, TimeSpan.FromSeconds(parrBlockVariant.crit1), minDeadline - dateOfEnd/*parrBlockVariant.crit1*/, mod.WorstOrderPriority, IsFromSameOrder, mod.FirstOperJust.Level, localResult0, 0);

                    lock (locker)
                    {
                        bool ThisWillBrokeMyResult = ForceThisTara && !searcherClass.ThisResultIsBad;
                        bool IneedItBcsItsGood = !ForceThisTara && searcherClass.ThisResultIsBad;

                        if ((localResult.IsBetterThanOther(searcherClass.minResult, true || !basisLink.SessionLink.SettingsController.CurrentSettings.UseRStar)/*searcherClass.minResult > localResult*/ || IneedItBcsItsGood)
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
                        //RWriterElements.TryAdd((mod.FirstOperJust.Index, currentCenter, currentTara, true), (localResult, !ForceThisTara));
                    }
                }
            }
            return result;
        }

        bool Step2_1_1_Check(PlanSimulatorMod mod, RSearcher searcherClass, TemporaryTaraSearchData data, Uco? NeedThisOneRC, List<Uco> allowedRC, PlanSimulatorEngineResult LastResult)
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
                    // Поскольку фильтрашка сама учитывает "NoCheckNamotka", и конечную тару, здесь проверять уже ничего не надо.

                    bool isBadAfterAll = CheckOneRCIsBad(allowedRC, NeedThisOneRC, currentCenter);
                    result |= ParallelCheck(mod, searcherClass, rc, tara/*currentCenter, currentTara*/, MaxNamot, isBadAfterAll, LastResult);
                }
            }
            return result;
        }

        void Step2_1_2_Errored(PlanSimulatorMod mod, RSearcher searcherClass, PlanSimulatorEngineResult LastResult)
        {
            var possibleRCs = mod.PossibleRCsHere;
            //var subProcess = Parallel.For(0, possibleRCs.Count, j =>
            for (int j = 0; j < possibleRCs.Count; j++)
            {
                var currentCenter = possibleRCs[j];
                ParallelCheck(mod, searcherClass, currentCenter, mod.FirstOperJust.CurrentTara, mod.FirstOperJust.CurrentTaraMaxCapacity, true, LastResult);
            }
            //);
            //while (!subProcess.IsCompleted) ;
        }

        bool CheckOneRCIsBad(List<Uco> allowedRC, Uco? NeedThisOneRC, Uco currentCenter)
        {
            return (allowedRC != null && !allowedRC.Contains(currentCenter))
                || (NeedThisOneRC.HasValue && NeedThisOneRC.Value != currentCenter); // Если какое-то из этих условий не выполняется, то это ПЛОХОЙ рц для рассмотрения. Достаточно проверить две вещи - либо я раньше назначал РЦ, и это не тот, что нужен, ЛИБО - нет в списке РЦ подходящего.
        }

        (List<Uco> allowedRC, Uco? neededThisRC) GetOneRCInfo(PlanSimulatorMod mod)
        {
            List<Uco> allowedRC = null;
            Uco? neededThisRC = null;
            int groupID1 = -1;
            if (mod.FirstOperJust.OnOneRC && mod.BlockLink.PoolLink.ModIDToOnOneRCGroupID.TryGetValue(mod.ModID, out groupID1))
            {
                allowedRC = mod.BlockLink.PoolLink.OnOneRCGroupData[groupID1].PossibleRCs;
            }
            if (groupID1 != -1 && OneRCGroupsForR.TryGetValue(groupID1, out var oneGroupRC)) neededThisRC = oneGroupRC;
            return (allowedRC, neededThisRC);
        }

        void Step2_2_Linear(PlanSimulatorBlock[] PossibleOpersHere, RSearcher searcherClass)
        {
            if (true || !searcherClass.WillBeParr)
            {
                var process = Parallel.For(0, PossibleOpersHere.Length, i =>
                //for (int i = 0; i < PossibleOpersHere.Length; i++)
                {
                    var mod = PossibleOpersHere[i].Lists[0];

                    TemporaryTaraSearchData data = null;

                    bool Can1 = TaraSearchEngine.GetAllVariantsForThese(mod, out data, new SumIsPossibleOptions(false) {
                        PrevsInfo = PrevsInfoForR,
                        ModToTaraInfoLink = basisLink.ModToTaraInfo
                    });

                    (List<Uco> allowedRC, Uco? neededThisRC) = GetOneRCInfo(mod);

                    bool anythingWas = false;
                    if (Can1) anythingWas |= Step2_2_1_TaraAndRCsCheck(mod, searcherClass, data, neededThisRC, allowedRC);

                    //var was = searcherClass.ChoosenCenter;
                    if (!Can1 || !anythingWas)
                    {
                        Step2_2_2_ForcedCheckNoTara(mod, searcherClass, data);
                    }
                }
                );
                while (!process.IsCompleted) ;
            }
        }

        bool Step2_2_1_TaraAndRCsCheck(PlanSimulatorMod mod, RSearcher searcherClass, TemporaryTaraSearchData data, Uco? NeedThisOneRC, List<Uco> allowedRC)
        {
            bool result = false;
            var possibleRCs = data.PossibleCentersAndTaras;
            //var subProcess = 
            //    Parallel.For(0, possibleRCs.Length, j =>
            for (int j = 0; j < possibleRCs.Length; j++)
            {
                var currentCenter = possibleRCs[j].RC;
                var currentTara = possibleRCs[j].Tara;


                bool isBadAfterAll = CheckOneRCIsBad(allowedRC, NeedThisOneRC, currentCenter);

                result |= TaraAndRCsSubchecker(mod, searcherClass, data, currentCenter, currentTara, isBadAfterAll);
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
                    // Поскольку фильтрашка сама учитывает "NoCheckNamotka", и конечную тару, здесь проверять уже ничего не надо.

                    result = true; // Значит я так могу.
                    var localResultData = HeuristicCheckBlock(mod, currentCenter, IndexToWorkWith, basisLink.SessionLink.SettingsController.CurrentSettings.RGlobalTimeDelta ? basisLink.StartOfPlanningDate : mod.PossibleRCsHere.Min(x =>
                    {
                        var list = BlocksDependingOnCenter[x];
                        if (list.Count == 0) return basisLink.StartOfPlanningDate;
                        return list[^1].Lists[0].DateOfStart[0] + list[^1].MaxTime(x);
                    }),
                    out int errors);
                    //ForceThisTara |= itsBadResult; // Если это хуевый результат, то смотрим на это.

                    var dateofend = basisLink.StartOfPlanningDate + localResultData.TotalTime;

                    var mindeadline = mod.StagesAsNormal.Min(x => x.OrderLink.Deadline);

                    var delta = mindeadline - dateofend;

                    var list = BlocksDependingOnCenter[currentCenter];

                    HashSet<OrderID> orders = mod.OrdersHere;//StagesAsNormal.Select(x => x.OrderLink.OrderID).ToHashSet();

                    bool IsFromSameOrder = list.Count > 0 ? list[^1].Lists.Any(list => list.StagesAsNormal.Any(stage => orders.Contains(stage.OrderLink.OrderID))) : false;

                    RTempResult localResult = new RTempResult(
                        localResultData.Rechanges,
                        localResultData.TotalTime,
                        localResultData.TotalTimeNoOp,
                        delta,
                        mod.WorstOrderPriority,
                        IsFromSameOrder,
                        mod.FirstOperJust.Level,
                        (float)(localResultData.TotalTimeNoOp.TotalSeconds * gamma + (1 - gamma) * localResultData.Rechanges.TotalSeconds),
                        errors
                        );

                    bool ThisWillBrokeMyResult = ForceThisTara && !searcherClass.ThisResultIsBad;
                    bool IneedItBcsItsGood = !ForceThisTara && searcherClass.ThisResultIsBad;
                    lock (locker)
                    {
                        if ((localResult.IsBetterThanOther(searcherClass.minResult, !basisLink.SessionLink.SettingsController.CurrentSettings.UseRStar)/*searcherClass.minResult > localResult*/ || IneedItBcsItsGood)
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
                        //RWriterElements.TryAdd((mod.FirstOperJust.Index, ForceThisTara ? currentCenter : rc, ForceThisTara ? currentTara : tara, false), (localResult, !ForceThisTara));
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
            
            PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngineResult LastResult = PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(LinearPlan, 0, basisLink.StartOfPlanningDate, PlanSimulatorEngineResult.CreateFullInstance(basisLink.SessionLink, basisLink.StartOfPlanningDate));

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

            //StreamWriter sw = new StreamWriter("R_test1.csv", false, Encoding.UTF8);

            //sw.WriteLine(
                //"НомерИтерации;НомерВыбранногоЭтапа;НомерПредыдущегоЭтапа;РЦ;КолвоОставшихсяЭлементов;ЗначениеКритерия;КолвоЭтаповСЭтимЗначением;КоличествоЭтаповОт0До5Процентов;ИДРассмотренныхЭлементов"

                /*
                 i.	Номер итерации
    ii.	Номер выбранного этапа
    iii.	Номер предыдущего этапа
    iv.	РЦ, на который рассматривалась расстановка
    v.	Количество оставшихся элементов
    vii.	Значение критерия
    vi.	Количество этапов с таким же значением критерия
    viii.	Количество оставшихся этапов, со значением критерия, в пределах от 0% до 5% от лучшего.
    ix.	ID элементов, которые были рассмотрены (10-15 шт.)

                 */
               // );

            Console.WriteLine("R: calculating...");
            int iterationIndex = -1;
            while (OpersCopy.Count > 0)
            {
                iterationIndex++;
                if (basisLink.WannaStop)
                {
                    break;
                }

                var PossibleOpersHere = OpersCopy.Where(x =>
                
                (FixedCompleted < basisLink.GLOBAL_FIXED_MIN ? true : x.Lists[0].FirstOperJust.Level == CurrentLevel) // Если я расставляю фиксированные, там насрать на уровень, там главное дата.

                &&

                (/*FixedCompleted < basisLink.GLOBAL_FIXED_MIN ||*/ x.Lists[0].PrevMods.All(mod => ExistingBlocks.Contains(mod.BlockLink))) // Проверяем на хронологию, если конечно это не фиксированные этапы.

                &&

                !ExistingBlocks.Contains(x)

                &&

                (FixedCompleted < basisLink.GLOBAL_FIXED_MIN ? x.Lists[0].IsFixed :
                (
                    VPZCompleted < basisLink.GLOBAL_VPZ_MAX ? x.Lists[0].IsFactOrVPZ
                    : true
                ))

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

                    //RWriterElements.Clear();

                    Step2_1_Parallel(PossibleOpersHere, searcherClass, LastResult); // Попробуем параллель поискать

                    Step2_2_Linear(PossibleOpersHere, searcherClass); // А теперь, если не было параллелизма, обычный варик

                    //int prevWas = BlocksDependingOnCenter[searcherClass.ChoosenCenter].Count > 0 ? BlocksDependingOnCenter[searcherClass.ChoosenCenter][^1].Lists[0].FirstOperJust.Index : -1;

                    //var elements = RWriterElements
                    //    .Where(x => x.Value.IsGood);

                    //var elements05 = elements.Where(x =>
                    //{
                    //    var valuePercent = (x.Value.Value * 100 / searcherClass.minResult) - 100;
                    //    return Math.Abs(valuePercent) <= 5;
                    //});

                    //var elementsStrict = elements05.Where(x => x.Value.Value == searcherClass.minResult);

                    //sw.WriteLine(
                    //    $"{iterationIndex};{searcherClass.ChoosenMod.FirstOperJust.Index};{prevWas};{searcherClass.ChoosenCenter};{OpersCopy.Count};{searcherClass.minResult};{elementsStrict.Count()};{elements05.Count()};{string.Join(",", elements05.Take(10).Select(x => $"{x.Key.ID} [{x.Key.Parr}]"))}"

                        /*
                         i.	Номер итерации
            ii.	Номер выбранного этапа
            iii.	Номер предыдущего этапа
            iv.	РЦ, на который рассматривалась расстановка
            v.	Количество оставшихся элементов
            vii.	Значение критерия
            vi.	Количество этапов с таким же значением критерия
            viii.	Количество оставшихся этапов, со значением критерия, в пределах от 0% до 5% от лучшего.
            ix.	ID элементов, которые были рассмотрены (10-15 шт.)

                         */

                        //);

                    if (searcherClass.WillBeParr)
                    {
                        AddBlockParallel(searcherClass.ChoosenMod.BlockLink, searcherClass.ChoosenCenter, searcherClass.ChoosenTara, searcherClass.parrBlock);
                    }
                    else
                    {
                        AddBlockToPlan(searcherClass.ChoosenMod.BlockLink, searcherClass.ChoosenCenter, searcherClass.ChoosenTara);
                    }


                    bool ChronologyCheck = basisLink.CheckChronology(LinearPlan, out _, out _);
                    if (!ChronologyCheck)
                    {
                        ;
                    }

                    if (searcherClass.ChoosenMod.NextMods.Count == 0 && OpersCopy.Count > 0)
                    {
                        MaxLevel = OpersCopy.Max(x => x.Lists.Max(y => y.StagesAsNormal.Max(z => z.Level)));
                    }

                    if (OpersCopy.Count == 0) Success = true;

                    ErroredIterationsCount = 0;

                    LastResult = PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(LinearPlan, 0, basisLink.StartOfPlanningDate, PlanSimulatorEngineResult.CreateFullInstance(basisLink.SessionLink, basisLink.StartOfPlanningDate)); // Вызывем симулятор плана
                    
                    basisLink.UpdateBcsSuccess(LastResult, LinearPlan, 0);
                    basisLink.UsedWorkflow.Generation++;

                    basisLink.AlgorithmCallback?.Invoke(basisLink.CurrentStage, LinearPlan.Count);
                }

                if (LinearPlan.Count % 500 == 0)
                {
                    Console.WriteLine($"R: {LinearPlan.Count}");
                    basisLink.SessionLink.AssignedLogger.LogQueue.Enqueue($"step 1: {LinearPlan.Count} / {OpersCopy.Count} done\r\n");
                }

                if (FixedCompleted < basisLink.GLOBAL_FIXED_MIN) continue; // Если я ещё не дошёл до нужного, то счётчик пока не включаем.
                LevelCounter++;
                if (LevelCounter >= MaxLevelCounter || PossibleOpersHere.Length == 0)
                {
                    LevelCounter = 0;
                    CurrentLevel++; // Обновляем уровень
                    if (CurrentLevel > MaxLevel) CurrentLevel = 0;
                    Console.WriteLine($"R: switched to level {CurrentLevel}, left {OpersCopy.Count}");
                    basisLink.SessionLink.AssignedLogger.LogQueue.Enqueue($"R: switched to level {CurrentLevel}, left {OpersCopy.Count}\r\n");
                }
            }

            //sw.Close();

            return Success;

        }


        void Step2_1_AllOpersCheck(PlanSimulatorBlock[] PossibleOpersHere, RSearcher searcherClass)
        {
            
        }





        //float Step2_1_CheckBlockParallel(PlanSimulatorMod mod, Uco choosenCenter, int poolIndex, PlanSimulatorBlock blockToInsert)
        //{

        //}

        (TimeSpan Rechanges, TimeSpan TotalTime, TimeSpan TotalTimeNoOp) HeuristicCheckBlock(PlanSimulatorMod mod, Uco choosenCenter, int poolIndex/*, List<PlanSimulatorBlock> list*/, DateTime startDate, out int errorsCount)
        {
            errorsCount = 0;

            if (mod.IsFixed && mod.FirstOperJust.StartTimeFact.HasValue) return (TimeSpan.Zero, mod.FirstOperJust.StartTimeFact.Value - basisLink.MinAcceptableDate, mod.FirstOperJust.StartTimeFact.Value - basisLink.MinAcceptableDate);//(float)(mod.FirstOperJust.StartTimeFact.Value - basisLink.MinAcceptableDate).TotalSeconds;

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

            var NaladLengthsMasterData = basisLink.SessionLink.MasterDataLinks.NaladLengthsMasterData;

            bool asFirst = NaladLengthsMasterData.ItsFirstLaunch(rechangesResult, lastBlock?.Lists[0]);
            var OperB = mod.FirstOperJust;
            GottenNaladLengthStruct naladLengths = NaladLengthsMasterData.GetNaladLengthsForCurrentStageOnly(OperB, choosenCenter, asFirst);
            if (OperB.NeededNaladLength.HasValue && NaladLengthsMasterData.NaladLengthsConverted(naladLengths.NaladLengthHere, OperB.EdIzmClassificator, OperB.KgKm).Value > OperB.NeededNaladLength.Value.Length.Value)
            {
                errorsCount = 1000000;
            }

            // Чтобы сделать только переналадки, хватит вот этого критерия:
            //return (float)rechesDur.TotalSeconds;

            prostoiDur -= rechesDur;

            if (prostoiDur.Ticks < 0) prostoiDur = TimeSpan.Zero;

            var time1 = lastCenterDate + prostoiDur + rechesDur;

            var lastDate = time1 + mod.AllTime[choosenCenter];

            //var res1 = (float)(time1 - minDate).TotalSeconds;

            //var res2 = (float)/*rechesDur.TotalSeconds;//*/(lastDate - basisLink.MinAcceptableDate).TotalSeconds;

            return (rechesDur, lastDate - startDate, time1 - startDate);

        }

    }
}

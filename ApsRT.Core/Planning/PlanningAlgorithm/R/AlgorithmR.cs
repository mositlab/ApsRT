using ApsRT.Core.Leftovers;
using ApsRT.Core.NaladLengths;
using ApsRT.Core.Planning.ParallelPlanning;
using ApsRT.Core.PlanSimulator;
using ApsRT.Core.PlanSimulator.PlanSimulatorEngine;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Stages;
using ApsRT.Core.Taras.TaraSearchEngine;
using Azure;
using DocumentFormat.OpenXml.Presentation;
using Google.OrTools.Sat;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
using UniversalPack.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;

namespace ApsRT.Core.Planning.PlanningAlgorithm.R
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

        FrozenDictionary<int, List<PlanSimulatorBlock>> GroupedByLevel;

        List<PlanSimulatorBlock> LinearPlan = new();

        HashSet<PlanSimulatorBlock> ExistingBlocks = new();

        // Словарик для хранения выбранных тар для правильного алгоритма
        Dictionary<int, Uco> PrevsInfoForR = new ();


        Dictionary<int, Uco> OneRCGroupsForR = new (); // Ограничители "На Одном РЦ"

        int FixedCompleted = 0, VPZCompleted = 0;

        public int NeedsToBeFirstCounter = 0;

        void AddOneRCData(PlanSimulatorMod firstMod, Uco center)
        {

            if (firstMod.FirstOperJust.OnOneRC
                && firstMod.BlockLink.PoolLink.ModIDToOnOneRCGroupID.TryGetValue(firstMod.ModID, out var groupID1))
            {
                OneRCGroupsForR.TryAdd(groupID1, center);
            }
        }

        void AddFixedOrVPZ(PlanSimulatorMod firstMod, bool asParr)
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
            if (!asParr)
            {
                if (firstMod.IsFixed)
                {
                    basisLink.FixedBlocks++;
                    basisLink.VPZBlocks++;
                }
                else
                {
                    if (firstMod.IsFactOrVPZ)
                    {
                        basisLink.VPZBlocks++;
                    }
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

        void UpdateTaraOnMod(PlanSimulatorMod mod, Uco tara, double max, bool withRewrite)
        {
            foreach (var ooop in mod.StagesAsNormal)
            {
                ooop.CurrentTara = tara;
                ooop.CurrentTaraMaxCapacity = max;
                if (tara.EmptyCheck()) continue;
                if (withRewrite)
                    PrevsInfoForR[ooop.Index] = ooop.CurrentTara;
                else
                    PrevsInfoForR.Add(ooop.Index, ooop.CurrentTara);
            }
        }

        void AddBlockToPlan(PlanSimulatorBlock block, Uco center, Uco tara, double max)
        {
            var firstMod = block.Lists[0];
            UpdateTaraOnMod(firstMod, tara, max, false);
            block.SetCenter(center, 0);
            AddOneRCData(firstMod, center);
            BlocksDependingOnCenter[center].Add(block);
            LinearPlan.Add(block);
            ExistingBlocks.Add(block);
            OpersCopy.Remove(block);
            var level = block.Lists[0].FirstOperJust.Level;
            GroupedByLevel[level].Remove(block);
            AddFixedOrVPZ(firstMod, false);
            if (block.NeedsToBeFirst) NeedsToBeFirstCounter--;
        }

        void AddBlockParallel(PlanSimulatorBlock block, Uco center, Uco tara, double max, PlanSimulatorBlock mainBlock)
        {
            if (block.NeedsToBeFirst) NeedsToBeFirstCounter--;

            block.SetCenter(center, 0);

            var firstMod = block.Lists[0];

            firstMod.BlockLink = mainBlock;

            UpdateTaraOnMod(firstMod, tara, max, false);

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

            AddFixedOrVPZ(firstMod, true);

            AddOneRCData(firstMod, center);

            var level = block.Lists[0].FirstOperJust.Level;
            GroupedByLevel[level].Remove(block);
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
                        if (TaraSearchEngine.FilterAllVariants(mod, data, out var res, new SumIsPossibleOptions(false)
                        {
                            LogSomeErrors = false,
                            PrevsInfo = PrevsInfoForR,
                            RCMustBeThis = new HashSet<Uco>() { currentCenter },
                            TaraMustBeThis = new HashSet<Uco>() { currentTara },
                            ModToTaraInfoLink = basisLink.ModToTaraInfo
                        }))
                        {
                            firstResults.Add(new RFirstStageResult(mod.BlockLink, res.FinalRC, res.TaraName, res.Max, false, 0));
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
                link.Cost = link.Cost * 1000 + 1; // Ну это чтобы не было 0, если пришёл в эту точку.
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
                .OrderBy(x => x.ItsBadResult ? 1 : 0) // Сначала уберем плохие результаты наверх
                .ThenBy(x => x.Block.Lists[0].IsFixed ? 0 : x.Block.Lists[0].IsFactOrVPZ ? 1 : 2) // Расставим ещё и в порядке "фикс.-впз.-свобод."
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

                if (basisLink.SessionLink.SettingsController.CurrentSettings.Demos_BigPlanningLog)
                {
                    string line = $"{i + 1};Финал;{element.Block.Lists[0].FirstOperJust.GUID};{element.Block.Lists[0].FirstOperJust.Registrator};{element.RC};;{element.Tara};;{element.Block.Lists[0].FirstOperJust.CurrentModelling}";
                    planning_logger.WriteLine(line);
                }

                AddBlockToPlan(element.Block, center, tara, element.Max);

                passedCenters.Add(center);
                passedBlocks.Add(element.Block);
            }
        }


        StreamWriter planning_logger;

        ConcurrentBag<string> planLogWriterConc;

        void BigLogAdd(int iterationIndex, PlanSimulatorBlock block, Uco center, Uco tara, RTempResult localResult, RTempResult minResult, bool noStars, RSearcher searcherClass, bool itsBetter, bool parallel)
        {
            string line = $"{iterationIndex};Поиск;{block.Lists[0].FirstOperJust.GUID};{block.Lists[0].FirstOperJust.Registrator};{searcherClass.ChoosenMod?.FirstOperJust.GUID};{searcherClass.ChoosenMod?.FirstOperJust.Registrator};" +
                $"{center};{searcherClass.ChoosenCenter};" +
                $"{tara};{searcherClass.ChoosenTara};" +
                $"{block.Lists[0].FirstOperJust.CurrentModelling};{(searcherClass.ChoosenMod?.FirstOperJust.CurrentModelling.ToString() ?? "")};" +
                $"{localResult.ErrorsCount};{minResult.ErrorsCount};" +
                $"{localResult.TransferTime};{minResult.TransferTime};" +
                $"{noStars};" +
                $"{parallel};{searcherClass.WillBeParr};" +
                $"{localResult.DeltaFromStart};{minResult.DeltaFromStart};" +
                $"{localResult.DeltaFromStartNoOp};{minResult.DeltaFromStartNoOp};" +
                $"{localResult.TotalCost};{minResult.TotalCost};" +
                $"{localResult.Rechanges};{minResult.Rechanges};" +
                $"{localResult.Prostoi};{minResult.Prostoi};" +
                $"{localResult.DeltaFromDeadline};{minResult.DeltaFromDeadline};" +
                $"{localResult.OperCount};{minResult.OperCount};" +
                $"{localResult.IsFromSameOrder};{minResult.IsFromSameOrder};" +
                $"{localResult.Level};{minResult.Level};" +
                $"{(itsBetter ? "Да" : "Нет")}";
            planning_logger.WriteLine(line);
            //planLogWriterConc.Add(line);
            //BigLogAdd(localResult, searcherClass.minResult, !basisLink.SessionLink.SettingsController.CurrentSettings.UseRStar, searcherClass);
        }

        public void Start(PlanningAlgorithmWorkflow workflow)
        {
            var start = DateTime.Now;

            OpersCopy = basisLink.UsedPool.CopyBlocksDeep(basisLink, basisLink.UsedPool.BestPoolIndex, true);

            Console.WriteLine("R, beg, " + OpersCopy.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Count());

            NeedsToBeFirstCounter = OpersCopy.Where(x => x.NeedsToBeFirst).Count();

            planning_logger = new("planAnalysis.csv", false, Encoding.UTF8);
            planning_logger.WriteLine("Итерация;Тип итерации;" +
                "Рассматриваемый этап;Рассматриваемый этап регистратор;" +
                "Предыдущий этап;Предыдущий этап регистратор;" +
                "Текущий РЦ;Предыдущий РЦ;" +
                "Текущая тара;Предыдущая тара;" +
                "Текущий мод;Предыдущий мод;" +
                "R_ErrorsCount Текущий;R_ErrorsCount Предыдущий;" +
                "R_TransferTime Текущий;R_TransferTime Предыдущий;" +
                "NoSteps;" +
                "Параллель;Параллель была;" +
                "R_DeltaFromStart Текущий;R_DeltaFromStart Предыдущий;" +
                "R_DeltaFromStartNoOp Текущий;R_DeltaFromStartNoOp Предыдущий;" +
                "R_TotalCost Текущий;R_TotalCost Предыдущий;" +
                "R_Rechanges Текущий;R_Rechanges Предыдущий;" +
                "R_Prostoi Текущий;R_Prostoi Предыдущий;" +
                "R_DeltaFromDeadline Текущий;R_DeltaFromDeadline Предыдущий;" +
                "R_OperCount Текущий;R_OperCount Предыдущий;" +
                "R_IsFromSameOrder Текущий;R_IsFromSameOrder Предыдущий;" +
                "R_Level Текущий;R_Level Предыдущий;" +
                "Выбран?");
            planLogWriterConc = new();

            gamma = basisLink.SessionLink.SettingsController.CurrentSettings.TGamma;

            //.Where(x => x.Lists[0].FirstOperJust.Center.Label.Contains("EEL-40")).ToList();
            var poolCopy = new PlanSimulatorPool(basisLink.SessionLink, OpersCopy, basisLink.MinAcceptableDate, basisLink.UsedPool.PoolSize);
            //poolCopy.PrepareOnOneRCData();
            //poolCopy.PrepareRecommendedTara();
            //poolCopy.PrepareAllRCsOnBlocks();
            BlocksDependingOnCenter = poolCopy.CentersData.ToFrozenDictionary(x => x.Key, x => new List<PlanSimulatorBlock>());

            GroupedByLevel = OpersCopy.GroupBy(block => block.Lists[0].FirstOperJust.Level).ToFrozenDictionary(x => x.Key, x => x.ToList());

            // Сейчас лучший индекс - 0.

            // Получаем самый максимальный уровень, по которому только можно ходить.

            Step1_FirstStages(); // Поставим сначала первые этапы

            bool success = Step2_Process();

            if (success)
            {
                Console.WriteLine("R: successfully created plan...");
                poolCopy.Blocks = LinearPlan;
                var ALL_COUNT = basisLink.UsedPool.Blocks.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Count();
                Console.WriteLine("R, count f, " + ALL_COUNT);
                ALL_COUNT = basisLink.UsedPool.Blocks.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Distinct().Count();
                Console.WriteLine("R, count f, " + ALL_COUNT);

                Console.WriteLine("R: durations...");
                Step3_DurationsProcessor();

                Console.WriteLine("R: post checker...");
                Step4_PostChecker();

                ;
            }
            else
            {
                ;
            }

            var end = DateTime.Now;

            Console.WriteLine($"R total time = {end - start}");

            foreach (var line in planLogWriterConc) planning_logger.WriteLine(line);

            planning_logger.Close();
        }

        void Step3_DurationsProcessor()
        {
            PlanSimulatorEngineResult LastResult = PlanSimulatorEngine.CalculatePlan(LinearPlan, 0, basisLink.StartOfPlanningDate, null);

            //for (int blockIndex = LinearPlan.Count - 1; blockIndex >= 0; blockIndex--)
                for (int blockIndex = 0; blockIndex < LinearPlan.Count; blockIndex++)
                {
                if (basisLink.WannaStop) break;
                var block = LinearPlan[blockIndex];
                var mod = block.Lists[0];
                //if (mod.AllTime.Any(x => x.Value != mod.AllTime.First().Value))
                //{
                //    ;
                //}
                var actualRC = LinearPlan[blockIndex].GetCenterData(0);
                var currentDuration = LinearPlan[blockIndex].MaxTime(actualRC.Center);
                var possibleBetterRCs = LinearPlan[blockIndex].PossibleRCsHere.Where(center => currentDuration > LinearPlan[blockIndex].MaxTime(center)).ToArray();
                // Впилить условие
                //TaraSearchEngine.GetAllVariantsForThese();
                //AcceptableFilterAllVariants
                if (possibleBetterRCs.Length > 0)
                {
                    ;

                    var res0 = Parallel.For(0, possibleBetterRCs.Length, rcIndex =>
                    {
                        if (basisLink.WannaStop) return;

                        var rc = possibleBetterRCs[rcIndex];

                        Uco choosenTara = Uco.EmptyObject;
                        double maxNamotka = 0;
                        if (mod.EasyLifeData == null)
                        {
                            if (!TaraSearchEngine.SumIsPossible(mod, out _, out _, new SumIsPossibleOptions(false)
                            {
                                LogSomeErrors = false,
                                PrevsInfo = PrevsInfoForR,
                                RCMustBeThis = new HashSet<Uco>() { rc },
                                ModToTaraInfoLink = basisLink.ModToTaraInfo,
                                NextRCsMustBeThese = TaraSearchEngine.FindNeededNextRCs(mod.StagesAsNormal, (index) => mod.BlockLink.PoolLink.StagesLinks.TryGetValue(index, out var s) ? s.ModLink.BlockLink.GetCenterData(0).Center : null),
                                NaladLength = mod.NaladLengthData[0].Data.NaladLengthHere.Value + mod.NaladLengthData[0].Data.NaladLengthNexts.Value // Текущую налад длину берём
                            }))
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (!mod.EasyLifeData.All(easyMod =>
                            {
                                var localMod = easyMod.Value.AsMod;
                                bool result = TaraSearchEngine.SumIsPossible(localMod, out var res, out _, new SumIsPossibleOptions(false)
                                {
                                    LogSomeErrors = false,
                                    PrevsInfo = PrevsInfoForR,
                                    RCMustBeThis = new HashSet<Uco>() { rc },
                                    ModToTaraInfoLink = basisLink.ModToTaraInfo,
                                    NextRCsMustBeThese = TaraSearchEngine.FindNeededNextRCs(localMod.AllOpers, (index) => mod.BlockLink.PoolLink.StagesLinks.TryGetValue(index, out var s) ? s.ModLink.BlockLink.GetCenterData(0).Center : null),
                                    NaladLength = easyMod.Value.NaladLengthData[0].Data.NaladLengthHere.Value + easyMod.Value.NaladLengthData[0].Data.NaladLengthNexts.Value // Текущую налад длину берём
                                });

                                if (result && res.Max > maxNamotka)
                                {
                                    maxNamotka = res.Max;
                                    choosenTara = res.TaraName;
                                }

                                return result;
                            }))
                            {
                                return;
                            }
                        }

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
                            // Выполним условие "НаОдномРЦ"

                            PlanSimulatorPool.MoveCentersInAllBlocks(LinearPlan, 0, IndexToWorkWith);
                            PlanSimulatorPool.MovePassedInfoInAllBlocks(LinearPlan, 0, IndexToWorkWith);
                            LinearPlan[blockIndex].SetCenter(rc, IndexToWorkWith);
                            block.PoolLink.UpdateOpsOnOneRC(block, IndexToWorkWith); // Реализуем ограничение "на одном РЦ"

                            var localResult = PlanSimulatorEngine.CalculatePlan(LinearPlan, IndexToWorkWith, basisLink.StartOfPlanningDate, null);
                            lock (locker)
                            {
                                if (
                                (localResult.CatchedNaladLengthsErrorsCount <= LastResult.CatchedNaladLengthsErrorsCount) // обязательные условия
                                &&
                                (
                                localResult.GlobalPlanEndDate < LastResult.GlobalPlanEndDate
                                || (localResult.GlobalPlanEndDate == LastResult.GlobalPlanEndDate && localResult.DaysRuinedValue < LastResult.DaysRuinedValue)
                                )
                                )
                                {
                                    LinearPlan[blockIndex].SetCenter(rc, 0);
                                    block.PoolLink.UpdateOpsOnOneRC(block, 0); // Реализуем ограничение "на одном РЦ"
                                    UpdateTaraOnMod(mod, choosenTara, maxNamotka, true); // Перезаписываем тару
                                    LastResult = localResult;
                                    basisLink.UpdateBcsSuccess(localResult, LinearPlan, 0);
                                }
                            }
    
                        AvailableIndexes.Enqueue(IndexToWorkWith);
                        }
                    });

                    while (!res0.IsCompleted) ;

                }
                basisLink.AlgorithmCallback?.Invoke(basisLink.CurrentStage, LinearPlan.Count - blockIndex);
            }
        }


        void Step4_PostChecker()
        {
            Console.WriteLine("R, chronology check... ");
            // 1-е условие выполнимости плана. Условие хронологии.
            // Индекс блока каждого этапа должен быть меньше, чем индекс блока этапа, зависимого от рассматриваемого.
            bool ChronologyCheck = basisLink.CheckChronology(LinearPlan, out _, out _);
            Console.WriteLine(ChronologyCheck);

            // 2-е условие. Условие пустых тар
            // Каждый этап, имеющий один из признаков "НеПроверятьНамотку", "ВспомогательнаяОперация", должен иметь текущую тару, равную "ПУСТО", а отстутсвие этих признаков обязывает иметь текущую тару, НЕ равную "ПУСТО".
            Console.WriteLine("R, empty taras check... ");
            bool emptyTarasCheck = EmptyTarasCheck(LinearPlan/*, out int errIndex*/);
            Console.WriteLine($"{emptyTarasCheck}");

            // 3-е условие. Условие выполнения признака "НеМенятьТару"
            // Каждый этап, имеющий признак "НеМенятьТару", должен иметь ту же назначенную тару, что и его предыдущие этапы.
            Console.WriteLine("R, no change tara check... ");
            bool noChangeTaraResult = NoChangeTaraCheck(LinearPlan/*, out int errIndex2*/);
            Console.WriteLine($"{noChangeTaraResult}");

            // 4-е условие.
            Console.WriteLine("R, tara check... ");
            bool taraResult = TaraCheck(LinearPlan/*, out int errIndex2*/);
            Console.WriteLine($"{taraResult}");
            ;
        }

        bool EmptyTarasCheck(List<PlanSimulatorBlock> blocks/*, out int FirstErroredBlock*/)
        {
            var FirstErroredBlock = -1;
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
                                Console.WriteLine($"{i}: {stage} [{mod}]");
                                //return false;
                            }
                        }
                        else
                        {
                            if (stage.CurrentTara.IsEmpty)
                            {
                                FirstErroredBlock = i;
                                Console.WriteLine($"{i}: {stage} [{mod}]");
                                //return false;
                            }
                        }
                    }
                }
            }
            return FirstErroredBlock == -1;
        }

        bool NoChangeTaraCheck(List<PlanSimulatorBlock> blocks/*, out int FirstErroredBlock*/)
        {
            var FirstErroredBlock = -1;
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
                                Console.WriteLine($"{i}: no all same taras {stage} [{mod}]");
                            }
                            else
                            {
                                if (prevs.First() != stage.CurrentTara)
                                {
                                    FirstErroredBlock = i;
                                    Console.WriteLine($"{i}: no same tara {stage} [{mod}]");
                                }
                            }
                        }
                    }
                }
            }
            return FirstErroredBlock == -1;
        }


        bool TaraCheck(List<PlanSimulatorBlock> blocks/*, out int FirstErroredBlock*/)
        {
            var TaraDataSet = basisLink.SessionLink.MasterDataLinks.TarasMasterData;
            var FirstErroredBlock = -1;
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                var center = block.GetCenterData(0).Center;
                foreach (var mod in block.Lists)
                {
                    var taraToCheck = mod.FirstOperJust.CurrentTara;

                    if (taraToCheck.IsEmpty) continue;

                    bool PriemTaraError = false;
                    if (TaraDataSet.GetNeededTaras(center, mod.FirstOperJust.OperationVid, null).priem.TryGetValue(false, out var has) && has.Contains(taraToCheck))
                    {
                        // Всё ок
                    }
                    else
                    {
                        PriemTaraError = true;
                    }

                    if (PriemTaraError)
                    {
                        Console.WriteLine($"{i}: {(PriemTaraError ? "P" : "")} {taraToCheck} {mod.FirstOperJust} [{mod}] {{{mod.ModType}}}, prevs are: {string.Join(",", mod.PrevMods.Select(x => $"{x} and his tara was {x.FirstOperJust.CurrentTara}"))}");
                        Console.WriteLine("trying to fix...");

                        var prevsIds = mod.PrevMods.ToList(); // toLIst - чтоб копия образовалась

                        for (int j = 0; j < prevsIds.Count; j++)
                        {
                            if (prevsIds[j].FirstOperJust.NoChangeTara)
                            {
                                prevsIds.AddRange(prevsIds[j].PrevMods);
                                prevsIds.RemoveAt(j);
                                j--;
                                continue;
                            }
                            if (prevsIds[j].FirstOperJust.IsVspom || prevsIds[j].FirstOperJust.NoCheckNamotka/* || finalOps[i].UsesVspomTara || IDtoSdvig[finalOps[i].ID].ZadelCode != 0*/)
                            {
                                prevsIds.RemoveAt(j);
                                j--;
                            }
                        }

                        var possibleRCs = mod.FirstOperJust.PossibleCenters ?? new() { mod.FirstOperJust.OriginalCenter };
                        foreach (var rc in possibleRCs) // пройдемся по каждому РЦ и найдём вариант, при котором все prevsSdvigs смогут найти себе пару.
                        {
                            List<(bool Can, Uco RC, Uco Tara, double Max, PlanSimulatorMod sdvig)> analysis = prevsIds.Select(sdvigToCheck =>
                            {
                                bool can = Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(sdvigToCheck, out var res, out _, new SumIsPossibleOptions(false)
                                { 
                                    PrevsInfo = PrevsInfoForR,
                                    LogSomeErrors = false, 
                                    NextRCsExcludeNoChangeTara = true, 
                                    SearchForMaxTara = true,
                                    NextRCsMustBeThese = new() { rc } 
                                });
                                return (can, res.FinalRC, res.TaraName, res.Max, sdvigToCheck);
                            }).ToList();

                            if (analysis.All(x => x.Can) && TaraSearchEngine.SumIsPossible(mod, out var res, out _, new SumIsPossibleOptions(false)
                            {
                                PrevsInfo = PrevsInfoForR,
                                LogSomeErrors = false,
                                RCMustBeThis = new() { rc }
                            }))
                            {
                                Console.WriteLine("fixed!");
                                mod.BlockLink.SetCenter(res.FinalRC, 0); // Обновим этому сдвигу РЦ и прочее
                                UpdateTaraOnMod(mod, res.TaraName, res.Max, true);
                                foreach (var an in analysis)
                                {
                                    if (!an.sdvig.FirstOperJust.NoChangeTara && !an.sdvig.FirstOperJust.NoCheckNamotka
                                        && an.Tara != an.sdvig.FirstOperJust.CurrentTara) // Имеет смысл перезначать?
                                    {
                                        an.sdvig.BlockLink.SetCenter(an.RC, 0); // Обновим этому сдвигу РЦ и прочее
                                        UpdateTaraOnMod(an.sdvig, an.Tara, an.Max, true);
                                        var nextMods = new Queue<PlanSimulatorMod>(an.sdvig.NextMods);
                                        while (nextMods.Count > 0) // Пока они есть?
                                        {
                                            var modToReset = nextMods.Dequeue();
                                            if (modToReset.FirstOperJust.NoChangeTara && modToReset != mod)
                                            {
                                                if (TaraSearchEngine.SumIsPossible(modToReset, out var res2, out _, new SumIsPossibleOptions(false)
                                                {
                                                    PrevsInfo = PrevsInfoForR,
                                                    LogSomeErrors = false,
                                                    NextRCsExcludeNoChangeTara = true,
                                                    SearchForMaxTara = true,
                                                    NextRCsMustBeThese = new() { rc }
                                                }))
                                                {
                                                    modToReset.BlockLink.SetCenter(res2.FinalRC, 0);
                                                    UpdateTaraOnMod(modToReset, res2.TaraName, res2.Max, true);
                                                    foreach (var next in modToReset.NextMods) nextMods.Enqueue(next);
                                                }
                                                else
                                                {
                                                    Console.WriteLine("but failed then...");
                                                }
                                            }
                                        }

                                    }
                                }
                                var LastResult = PlanSimulatorEngine.CalculatePlan(blocks, 0, basisLink.StartOfPlanningDate, null); // Пересчитываем.
                                basisLink.UpdateBcsSuccess(LastResult, blocks, 0);

                                //i = -1; // Начнём затем сначала всё
                                break; // Ну и уходим отсюда тут же
                            }
                        }
                        //public void PrepareRecommendedTara()
                        //{
                        //    var partias = Blocks.GroupBy(x => x.Lists[0].FirstOperJust.GetOrderOrPartiaCode());
                        //    foreach (var group in partias)
                        //    {
                        //        //if (group.Key.ToString() == "ea3e2b21-7e56-11f0-be4a-3cecef1c38f6")
                        //        //{
                        //        //    ;
                        //        //}
                        //        var modsHere = group.Select(x => x.Lists[0]);
                        //        var modsToCheck = modsHere
                        //            //.Where(mod => mod.PrevMods.Count > 1)
                        //            ;
                        //        foreach (var sdvig in modsToCheck)
                        //        {



                        //            if (prevsIds.Count < 2 || prevsIds.All(x => x.FirstOperJust.Level == prevsIds[0].FirstOperJust.Level && x.FirstOperJust.OperationVid == prevsIds[0].FirstOperJust.OperationVid)) continue;

                        //            if (prevsIds.FindIndex(x => x.FirstOperJust.Index == 14434) > -1)
                        //            {
                        //                ;//ApsRT.Core.Taras.TaraSearchEngine.IAbleForTaraCalcInterface.FirstOperLink = {Оп. :14434 [9c64b7b6-6d03-11f0-be4a-3cecef1c38f6]}
                        //            }

                        //        }
                        //    }
                        //}
                    }

                    /*
                     .GetNeededTaras(center, ) && TaraNeedsToBeHere.All(x => x.IsEmpty || has.Contains(x))
                     */

                }
            }
            return FirstErroredBlock == -1;
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

        void Step2_1_Parallel(int iterationIndex, PlanSimulatorBlock[] PossibleOpersHere, RSearcher searcherClass, PlanSimulatorEngineResult LastResult)
        {
            var process = Parallel.For(0, PossibleOpersHere.Length, i =>
            {
                var mod = PossibleOpersHere[i].Lists[0];

                TemporaryTaraSearchData data = null;

                bool Can1 = TaraSearchEngine.GetAllVariantsForThese(mod, out data, new SumIsPossibleOptions(false) { PrevsInfo = PrevsInfoForR, ModToTaraInfoLink = basisLink.ModToTaraInfo });

                (List<Uco> allowedRC, Uco? neededThisRC) = GetOneRCInfo(mod);

                bool anythingWas = false;
                if (Can1) anythingWas |= Step2_1_1_Check(iterationIndex, mod, searcherClass, data, neededThisRC, allowedRC, LastResult);

                //if (!Can1 || !anythingWas) Step2_1_2_Errored(mod, searcherClass, LastResult);

            });
            while (!process.IsCompleted) ;
        }

        bool ParallelCheck(int iterationIndex, PlanSimulatorMod mod, RSearcher searcherClass, Uco currentCenter, Uco currentTara, double maxNamot, bool ForceThisTara, PlanSimulatorEngineResult LastResult)
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
                    (PlanSimulatorBlock block, TimeSpan prostoi, double crit1, double crit2, float TotalCost)[] variants = possibleParallelOpers.Select(block =>
                    {
                        var deltaBetweenInnerOps = Math.Abs((block.MaxTime(currentCenter) - mod.AllTime[currentCenter]).TotalSeconds); // Приходится заюзать то, что я описал выше, потому что близость операций можно определить по дельте между ними
                        deltaBetweenInnerOps = Math.Round(deltaBetweenInnerOps); // На будущее - уберу дробную часть


                        var dateDeltaNoAbs = (mod.MinDatesData[0].MinValue - block.Lists[0].DateOfStart[0]);
                        var dateDelta = Math.Abs(dateDeltaNoAbs.TotalSeconds);
                        dateDelta = Math.Round(dateDelta);

                        var prostoi = dateDeltaNoAbs.TotalSeconds < 0 ? -dateDeltaNoAbs : TimeSpan.Zero;

                        return (block, prostoi: prostoi, crit1: dateDelta, crit2: deltaBetweenInnerOps, TotalCost: (float)(dateDelta + deltaBetweenInnerOps / 10000f));
                    })
                    .OrderBy(x => x.crit1)
                    .ThenBy(y => y.crit2)
                    .ToArray();

                    result = true;
                    var parrBlockVariant = variants.First();
                    float localResult0 = parrBlockVariant.TotalCost;

                    DateTime dateOfEnd;
                    /*
                     
                    var dateofend = basisLink.StartOfPlanningDate + ( ? (localResultData.TotalTimeNoOp + TimeSpan.FromSeconds()) : localResultData.TotalTime);
                     */

                    if (basisLink.SessionLink.SettingsController.CurrentSettings.RNewDeadlineCounter)
                    {
                        string logString = "";
                        dateOfEnd = basisLink.StartOfPlanningDate + TimeSpan.FromSeconds(parrBlockVariant.crit1 + Math.Max(
                            
                            parrBlockVariant.block.Lists.Max(list => basisLink.getOpsDurToEnd(list, 1.5f, ref logString)),
                            
                            basisLink.getOpsDurToEnd(mod, 1.5f, ref logString)));
                    }
                    else
                    {
                        dateOfEnd = basisLink.StartOfPlanningDate + TimeSpan.FromSeconds(parrBlockVariant.crit1 + Math.Max(parrBlockVariant.block.MaxTime(currentCenter).TotalSeconds, mod.AllTime[currentCenter].TotalSeconds));
                    }




                    var minDeadline = mod.StagesAsNormal.Min(x => x.OrderLink.Deadline);
                    var minDeadline2 = parrBlockVariant.block.Lists.Min(list => list.StagesAsNormal.Min(stage => stage.OrderLink.Deadline));
                    if (minDeadline2 < minDeadline) minDeadline = minDeadline2;

                    HashSet<OrderID> orders = mod.OrdersHere;//StagesAsNormal.Select(x => x.OrderLink.OrderID).ToHashSet();
                    bool IsFromSameOrder = parrBlockVariant.block.Lists.Any(list => list.StagesAsNormal.Any(stage => orders.Contains(stage.OrderLink.OrderID)));


                    int timeTransfer = 0;
                    PlanSimulatorMod lastModFromPartia = null;
                    if (mod.PrevMods.Count > 0)
                    {
                        lastModFromPartia = mod.PrevMods.MaxBy(mod => mod.DateOfEnd[0]);
                        Uco lastSubdivision = lastModFromPartia.BlockLink.GetCenterData(0).CenterModelDataLink.Subdivision;
                        Uco currentSubdivion = basisLink.SessionLink.MasterDataLinks.CentersMasterData.CentersData[currentCenter].Subdivision;
                        if (lastSubdivision != currentSubdivion)
                        {
                            timeTransfer = basisLink.SessionLink.MasterDataLinks.SubdivisionsTransfersMasterData.GetSubdivisionTransferTime(lastSubdivision, currentSubdivion);
                        }
                    }

                    // Теперь посчитаем переход в другой цех
                    // Из времени перехода можно отнять простой, но не переналадку, так как переналадка должна начаться в другом цехе.
                    timeTransfer -= (int)parrBlockVariant.prostoi.TotalSeconds;

                    if (basisLink.SessionLink.SettingsController.CurrentSettings.TransferToGamma)
                    {
                        dateOfEnd = dateOfEnd.Add(TimeSpan.FromSeconds(timeTransfer));
                        timeTransfer = 0;
                    }

                    var lastBlock = parrBlockVariant.block.PreviousBlockOnThisRC[0];

                    var lastCenterDate = lastBlock == null ? basisLink.StartOfPlanningDate : lastBlock.Lists[0].DateOfEnd[0];

                    // Из цеха так же надо вычесть разницу между lastCenterTime и lastMod, чтобы убрать лишний временной отрезок
                    var delta1 = lastModFromPartia == null ? TimeSpan.Zero : (lastCenterDate - lastModFromPartia.DateOfEnd[0]);
                    if (delta1.Ticks < 0) delta1 = TimeSpan.Zero;
                    timeTransfer -= (int)delta1.TotalSeconds;
                    if (timeTransfer < 0) timeTransfer = 0;

                    RTempResult localResult = new RTempResult(TimeSpan.Zero /*переналадки уже уплочены*/, dateOfEnd - basisLink.StartOfPlanningDate, TimeSpan.FromSeconds(parrBlockVariant.crit1), minDeadline - dateOfEnd/*parrBlockVariant.crit1*/, mod.WorstOrderPriority, IsFromSameOrder, mod.FirstOperJust.Level, localResult0, 0, mod.Count, timeTransfer/*, mod.BlockLink.PoolLink.RecommendedTara.TryGetValue(mod, out var recTara) && recTara == currentTara*/, true, parrBlockVariant.prostoi,
                        mod.BlockLink.NeedsToBeFirst);

                    lock (locker)
                    {
                        bool ThisWillBrokeMyResult = ForceThisTara && !searcherClass.ThisResultIsBad;
                        bool IneedItBcsItsGood = !ForceThisTara && searcherClass.ThisResultIsBad;

                        bool condition = (localResult.IsBetterThanOther(searcherClass.minResult, true || !basisLink.SessionLink.SettingsController.CurrentSettings.UseRStar)/*searcherClass.minResult > localResult*/ || IneedItBcsItsGood)
                           &&
                           !ThisWillBrokeMyResult
                           ;

                        if (basisLink.SessionLink.SettingsController.CurrentSettings.Demos_BigPlanningLog)
                        {
                            BigLogAdd(iterationIndex, parrBlockVariant.block, currentCenter, currentTara, localResult, searcherClass.minResult, true || !basisLink.SessionLink.SettingsController.CurrentSettings.UseRStar, searcherClass, condition, true);
                        }
                        if (condition)
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

        bool Step2_1_1_Check(int iterationIndex, PlanSimulatorMod mod, RSearcher searcherClass, TemporaryTaraSearchData data, Uco? NeedThisOneRC, List<Uco> allowedRC, PlanSimulatorEngineResult LastResult)
        {
            bool result = false;
            var possibleRCs = data.PossibleCentersAndTaras;
            for (int j = 0; j < possibleRCs.Length; j++)
            {
                var currentCenter = possibleRCs[j].RC;
                var currentTara = possibleRCs[j].Tara;

                Uco tara = Uco.EmptyObject;
                Uco rc = Uco.EmptyObject;
                double MaxNamot = 0;
                /*
                 
                    
                 */


                if (
                    mod.EasyLifeData == null ? AcceptableFilterAllVariants(mod, data, currentCenter, currentTara, out tara, out rc, out MaxNamot) :
                    (mod.EasyLifeData.All(easy => AcceptableFilterAllVariants(easy.Value.AsMod, data, currentCenter, currentTara, out tara, out rc, out MaxNamot)))
                    )
                //if (TaraSearchEngine.FilterAllVariants(mod, data, out var tara, out var rc, out var MaxNamot, out _, out _, new SumIsPossibleOptions(false)
                //{
                //    LogSomeErrors = false,
                //    PrevsInfo = PrevsInfoForR,
                //    RCMustBeThis = new HashSet<Uco>() { currentCenter },
                //    TaraMustBeThis = new HashSet<Uco>() { currentTara },
                //    ModToTaraInfoLink = basisLink.ModToTaraInfo
                //}))
                {
                    // Поскольку фильтрашка сама учитывает "NoCheckNamotka", и конечную тару, здесь проверять уже ничего не надо.

                    bool isBadAfterAll = CheckOneRCIsBad(allowedRC, NeedThisOneRC, currentCenter);
                    result |= ParallelCheck(iterationIndex, mod, searcherClass, rc, tara/*currentCenter, currentTara*/, MaxNamot, isBadAfterAll, LastResult);
                }
            }
            return result;
        }

        void Step2_1_2_Errored(int iterationIndex, PlanSimulatorMod mod, RSearcher searcherClass, PlanSimulatorEngineResult LastResult)
        {
            var possibleRCs = mod.PossibleRCsHere;
            //var subProcess = Parallel.For(0, possibleRCs.Count, j =>
            for (int j = 0; j < possibleRCs.Count; j++)
            {
                var currentCenter = possibleRCs[j];
                ParallelCheck(iterationIndex, mod, searcherClass, currentCenter, mod.FirstOperJust.CurrentTara, mod.FirstOperJust.CurrentTaraMaxCapacity, true, LastResult);
            }
            //);
            //while (!subProcess.IsCompleted) ;
        }

        bool CheckOneRCIsBad(List<Uco> allowedRC, Uco? NeedThisOneRC, Uco currentCenter)
        {
            return allowedRC != null && !allowedRC.Contains(currentCenter)
                || NeedThisOneRC.HasValue && NeedThisOneRC.Value != currentCenter; // Если какое-то из этих условий не выполняется, то это ПЛОХОЙ рц для рассмотрения. Достаточно проверить две вещи - либо я раньше назначал РЦ, и это не тот, что нужен, ЛИБО - нет в списке РЦ подходящего.
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

        void Step2_2_Linear(int iterationIndex, PlanSimulatorBlock[] PossibleOpersHere, RSearcher searcherClass)
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
                    if (Can1) anythingWas |= Step2_2_1_TaraAndRCsCheck(iterationIndex, mod, searcherClass, data, neededThisRC, allowedRC);

                    //var was = searcherClass.ChoosenCenter;
                    if (!Can1 || !anythingWas)
                    {
                        Step2_2_2_ForcedCheckNoTara(iterationIndex, mod, searcherClass, data);
                    }
                }
                );
                while (!process.IsCompleted) ;
            }
        }

        bool Step2_2_1_TaraAndRCsCheck(int iterationIndex, PlanSimulatorMod mod, RSearcher searcherClass, TemporaryTaraSearchData data, Uco? NeedThisOneRC, List<Uco> allowedRC)
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

                result |= TaraAndRCsSubchecker(iterationIndex, mod, searcherClass, data, currentCenter, currentTara, isBadAfterAll);
            }
            return result;
            //);
            //while (!subProcess.IsCompleted)
            //{
            //    ;
            //}
        }

        void Step2_2_2_ForcedCheckNoTara(int iterationIndex, PlanSimulatorMod mod, RSearcher searcherClass, TemporaryTaraSearchData data)
        {
            var possibleRCs = mod.PossibleRCsHere;
            //var subProcess = Parallel.For(0, possibleRCs.Count, j =>
            for (int j = 0; j < possibleRCs.Count; j++)
            {
                var currentCenter = possibleRCs[j];
                TaraAndRCsSubchecker(iterationIndex, mod, searcherClass, data, currentCenter, mod.FirstOperJust.CurrentTara /*Если вспомогательный или НеМенятьНамотку, то тара всё равно будет пустая*/, true);
            }
            //);
            //while (!subProcess.IsCompleted)
            //{
            //    ;
            //}
        }

        bool AcceptableFilterAllVariants(IAbleForTaraCalcInterface mod, TemporaryTaraSearchData data, Uco currentCenter, Uco currentTara, out Uco tara, out Uco rc, out double MaxNamot)
        {
            tara = Uco.EmptyObject;
            rc = Uco.EmptyObject;
            MaxNamot = 0;
            var b = TaraSearchEngine.FilterAllVariants(mod, data, out var res, new SumIsPossibleOptions(false)
            {
                LogSomeErrors = false,
                PrevsInfo = PrevsInfoForR,
                RCMustBeThis = new HashSet<Uco>() { currentCenter },
                TaraMustBeThis = new HashSet<Uco>() { currentTara },
                ModToTaraInfoLink = basisLink.ModToTaraInfo
            });
            if (b && res != null)
            {
                tara = res.TaraName;
                rc = res.FinalRC;
                MaxNamot = res.Max;
            }
            return b;
        }
        
        
        bool TaraAndRCsSubchecker(int iterationIndex, PlanSimulatorMod mod, RSearcher searcherClass, TemporaryTaraSearchData data, Uco currentCenter, Uco currentTara, bool ForceThisTara)
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
                if (ForceThisTara ||
                    (
                    
                    mod.EasyLifeData == null ? AcceptableFilterAllVariants(mod, data, currentCenter, currentTara, out tara, out rc, out MaxNamot) : 
                    (mod.EasyLifeData.All(easy => AcceptableFilterAllVariants(easy.Value.AsMod, data, currentCenter, currentTara, out tara, out rc, out MaxNamot)))
                    )
                    )
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
                    string logString = "";
                    var dateofend = basisLink.StartOfPlanningDate + (basisLink.SessionLink.SettingsController.CurrentSettings.RNewDeadlineCounter ? localResultData.TotalTimeNoOp + TimeSpan.FromSeconds(basisLink.getOpsDurToEnd(mod, 1.5f, ref logString)) : localResultData.TotalTime);

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
                        errors, mod.Count, localResultData.timeTransferLeft,
                        false, localResultData.prostoi,
                        mod.BlockLink.NeedsToBeFirst
                        //mod.BlockLink.PoolLink.RecommendedTara.TryGetValue(mod, out var recTara) && recTara == (ForceThisTara ? currentTara : tara)
                        );



                    bool ThisWillBrokeMyResult = ForceThisTara && !searcherClass.ThisResultIsBad;
                    bool IneedItBcsItsGood = !ForceThisTara && searcherClass.ThisResultIsBad;
                    lock (locker)
                    {
                        bool condition = (localResult.IsBetterThanOther(searcherClass.minResult, !basisLink.SessionLink.SettingsController.CurrentSettings.UseRStar)/*searcherClass.minResult > localResult*/ || IneedItBcsItsGood)
                            &&
                            !ThisWillBrokeMyResult;



                        if (basisLink.SessionLink.SettingsController.CurrentSettings.Demos_BigPlanningLog)
                        {
                            BigLogAdd(iterationIndex, mod.BlockLink, ForceThisTara ? currentCenter : rc, ForceThisTara ? currentTara : tara, localResult, searcherClass.minResult, !basisLink.SessionLink.SettingsController.CurrentSettings.UseRStar, searcherClass, condition, false);
                        }

                        if (condition)
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

        IEnumerable<PlanSimulatorBlock> GetOpersToCheckByLevel(int Level)
        {
            //другая попытка, хуже:
List<PlanSimulatorBlock> toCheck;
            if (FixedCompleted < basisLink.GLOBAL_FIXED_MIN || Level == -1) toCheck = OpersCopy;
            else
            {
                if (GroupedByLevel.TryGetValue(Level, out var list)) toCheck = list;
                else return Array.Empty<PlanSimulatorBlock>();
            }
            var PossibleOpersHere = toCheck.Where(x =>


                        //var PossibleOpersHere = OpersCopy.Where(x =>

                        //(FixedCompleted < basisLink.GLOBAL_FIXED_MIN ? true : Level == -1 || x.Lists[0].FirstOperJust.Level == Level) // Если я расставляю фиксированные, там насрать на уровень, там главное дата.

                        //&&

                        /*FixedCompleted < basisLink.GLOBAL_FIXED_MIN ||*/ x.Lists[0].PrevMods.All(mod => ExistingBlocks.Contains(mod.BlockLink)) // Проверяем на хронологию, если конечно это не фиксированные этапы.

            &&

            //!ExistingBlocks.Contains(x)

            //&&

            (FixedCompleted < basisLink.GLOBAL_FIXED_MIN ? x.Lists[0].IsFixed :

                VPZCompleted < basisLink.GLOBAL_VPZ_MAX ? x.Lists[0].IsFactOrVPZ
                : true
            )

            );
            return PossibleOpersHere;
        }

        bool Step2_Process()
        {
            Console.WriteLine("R: get max level...");
            int MaxLevel = OpersCopy.Max(x => x.Lists.Max(y => y.StagesAsNormal.Max(z => z.Level)));
            
            PlanSimulatorEngineResult LastResult = PlanSimulatorEngine.CalculatePlan(LinearPlan, 0, basisLink.StartOfPlanningDate, PlanSimulatorEngineResult.CreateFullInstance(basisLink.SessionLink, basisLink.StartOfPlanningDate));

            basisLink.UpdateBcsSuccess(LastResult, LinearPlan, 0);

            int CurrentLevel = 2; // Потому что мы первый уровень уже прошли, полагается.
            //CurrentLevel = 1; // Не пытайся поставить начало на 1, потому что почему-то это ухудшает план на 1-2 дня.


            int LevelCounter = 0;
            int MaxLevelCounter = OpersCopy.Count;

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

                var PossibleOpersHere = GetOpersToCheckByLevel(CurrentLevel).ToArray();

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

                    Step2_1_Parallel(iterationIndex, PossibleOpersHere, searcherClass, LastResult); // Попробуем параллель поискать

                    Step2_2_Linear(iterationIndex, PossibleOpersHere, searcherClass); // А теперь, если не было параллелизма, обычный варик

                    int nextCount = 0;
                    if (searcherClass.minResult.ErrorsCount > 0
                        ||
                        searcherClass.ThisResultIsBad
                        || (NeedsToBeFirstCounter > 0 && !searcherClass.ChoosenMod.BlockLink.NeedsToBeFirst))
                    {
                        var nextPossibleOpers = GetOpersToCheckByLevel(-1)
                            .Where(block => !PossibleOpersHere.Contains(block)) // Возьмём только те, что я не рассматривал
                            .ToArray();
                        nextCount = nextPossibleOpers.Length;
                        RSearcher searcherClass2 = new();

                        Step2_1_Parallel(iterationIndex, nextPossibleOpers, searcherClass2, LastResult); // Попробуем параллель поискать

                        Step2_2_Linear(iterationIndex, nextPossibleOpers, searcherClass2); // А теперь, если не было параллелизма, обычный варик

                        if (
                            !searcherClass2.ThisResultIsBad && searcherClass.ThisResultIsBad
                            || searcherClass.ThisResultIsBad == searcherClass2.ThisResultIsBad && searcherClass2.minResult.ErrorsCount < searcherClass.minResult.ErrorsCount
                            )
                        {
                            searcherClass = searcherClass2;
                        }
                    }

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

                    if (searcherClass.ThisResultIsBad)
                    {
                        Console.WriteLine($"badly set {searcherClass.ChoosenMod} {{{searcherClass.ChoosenMod.ModType}}} as {LinearPlan.Count} on RC {searcherClass.ChoosenCenter} (was {searcherClass.ChoosenMod.FirstOperJust.Center}), tara was {searcherClass.ChoosenMod.FirstOperJust.CurrentTara}, now {searcherClass.ChoosenTara}");
                    }


                    //if (searcherClass.ChoosenMod.StagesAsNormal.Any(op => op.GUID.ToString() == "18792b6b-89b6-11f0-be4a-3cecef1c38f6") /*&& LinearPlan.Count >= 100*/)
                    //{
                    //    Console.WriteLine($"{LinearPlan.Count}: {searcherClass.minResult.Rechanges} with price {searcherClass.minResult.TotalCost} on {searcherClass.ChoosenCenter.Label}");
                    //    ;
                    //}
                    //if (searcherClass.ChoosenMod.BlockLink.PoolLink.RecommendedTara.TryGetValue(searcherClass.ChoosenMod, out var recTara))
                    //{
                    //    if (searcherClass.ChoosenTara != recTara)
                    //        Console.WriteLine($"set {searcherClass.ChoosenMod}, recommended: {recTara}, but got {searcherClass.ChoosenTara}");
                    //}

                    //if (searcherClass.ChoosenMod.StagesAsNormal.Any(x => x.Index == 4178))
                    //{
                    //    ;
                    //}

                    if (basisLink.SessionLink.SettingsController.CurrentSettings.Demos_BigPlanningLog)
                    {

                        string line = $"{iterationIndex};Финал;{searcherClass.ChoosenMod?.FirstOperJust.GUID};{searcherClass.ChoosenMod?.FirstOperJust.Registrator};{searcherClass.ChoosenCenter};;{searcherClass.ChoosenTara};;{searcherClass.ChoosenMod?.FirstOperJust.CurrentModelling};{(searcherClass.ThisResultIsBad ? "ПЛОХ" : "НОРМ")};{PossibleOpersHere.Length};{nextCount}";
                        /*
                         string line = $"{ExistingBlocks.Count};Поиск;{block.Lists[0].FirstOperJust.GUID};{block.Lists[0].FirstOperJust.Registrator};{searcherClass.ChoosenMod?.FirstOperJust.GUID};{searcherClass.ChoosenMod?.FirstOperJust.Registrator};" +
                $"{center};{searcherClass.ChoosenCenter};" +
                $"{tara};{searcherClass.ChoosenTara};" +
                $"{localResult.ErrorsCount};{minResult.ErrorsCount};" +
                $"{localResult.TransferTime};{minResult.TransferTime};" +
                $"{noStars};" +
                $"{parallel};{searcherClass.WillBeParr};" +
                $"{localResult.DeltaFromStart};{minResult.DeltaFromStart};" +
                $"{localResult.TotalCost};{minResult.TotalCost};" +
                $"{localResult.Rechanges};{minResult.Rechanges};" +
                $"{localResult.DeltaFromDeadline};{minResult.DeltaFromDeadline};" +
                $"{localResult.OperCount};{minResult.OperCount};" +
                $"{localResult.IsFromSameOrder};{minResult.IsFromSameOrder};" +
                $"{localResult.Level};{minResult.Level};" +
                $"{(itsBetter ? "Да" : "Нет")}";
                         */
                        planning_logger.WriteLine(line);
                    }

                    if (searcherClass.WillBeParr)
                    {
                        AddBlockParallel(searcherClass.ChoosenMod.BlockLink, searcherClass.ChoosenCenter, searcherClass.ChoosenTara, searcherClass.MaxNamotka, searcherClass.parrBlock);
                    }
                    else
                    {
                        AddBlockToPlan(searcherClass.ChoosenMod.BlockLink, searcherClass.ChoosenCenter, searcherClass.ChoosenTara, searcherClass.MaxNamotka);
                    }

                    //bool ChronologyCheck = basisLink.CheckChronology(LinearPlan, out _, out _);
                    //if (!ChronologyCheck)
                    //{
                    //    ;
                    //}

                    if (searcherClass.ChoosenMod.NextMods.Count == 0 && OpersCopy.Count > 0)
                    {
                        MaxLevel = OpersCopy.Max(x => x.Lists.Max(y => y.StagesAsNormal.Max(z => z.Level)));
                    }

                    if (OpersCopy.Count == 0) Success = true;

                    ErroredIterationsCount = 0;

                    if (searcherClass.WillBeParr)
                    {
                        LastResult = PlanSimulatorEngine.CalculatePlan(LinearPlan, 0, basisLink.StartOfPlanningDate, PlanSimulatorEngineResult.CreateFullInstance(basisLink.SessionLink, basisLink.StartOfPlanningDate)); // Вызывем симулятор плана - придётся полный, без флагов и прочего.
                    }
                    else
                    {
                        LastResult = PlanSimulatorEngine.CalculatePlan(LinearPlan, 0, basisLink.StartOfPlanningDate, LastResult, StartFrom: LinearPlan.Count - 1); // Вызывем симулятор плана


                        //var reches = LastResult1.RechangesBeforeFirstStages[LinearPlan[^1].Lists[0].FirstOperJust.Index];
                        //var prostoi = LastResult1.ProstoiBeforeFirstStages.TryGetValue(LinearPlan[^1].Lists[0].FirstOperJust.Index, out var res) ? res : (ProstoiItself: new PlanSimulatorDatesData(DateTime.MinValue, -1, -1, -1), new CachedInterval(TimeSpan.Zero, Array.Empty<int>(), -1));


                        //LastResult = PlanSimulatorEngine.CalculatePlan(LinearPlan, 0, basisLink.StartOfPlanningDate, PlanSimulatorEngineResult.CreateFullInstance(basisLink.SessionLink, basisLink.StartOfPlanningDate)); // Вызывем симулятор плана - придётся полный, без флагов и прочего.
                        //var reches2 = LastResult.RechangesBeforeFirstStages[LinearPlan[^1].Lists[0].FirstOperJust.Index];
                        //var prostoi2 = LastResult.ProstoiBeforeFirstStages.TryGetValue(LinearPlan[^1].Lists[0].FirstOperJust.Index, out var res2) ? res2 : (ProstoiItself: new PlanSimulatorDatesData(DateTime.MinValue, -1, -1, -1), new CachedInterval(TimeSpan.Zero, Array.Empty<int>(), -1));

                        //if (prostoi.GetHashCode() == prostoi2.GetHashCode())
                        //{
                        //    ;
                        //}

                        //if (LastResult1.GlobalPlanEndDate != LastResult.GlobalPlanEndDate)
                        //{
                        //    ;
                        //}

                        ////if (prostoi.ProstoiItself.MinValue > DateTime.MinValue)
                        ////{
                        ////    ;
                        ////}
                        ////if (prostoi2.ProstoiItself.MinValue > DateTime.MinValue)
                        ////{
                        ////    ;
                        ////}

                        //if (prostoi.ProstoiItself.MinValue != prostoi2.ProstoiItself.MinValue)
                        //{
                        //    ;
                        //}

                        //var dur1 = reches.RechangeItself?.Duration ?? 0;
                        //var dur2 = reches2.RechangeItself?.Duration ?? 0;
                        //if (dur1 != dur2)
                        //{
                        //    ;
                        //}
                    }


                    basisLink.UpdateBcsSuccess(LastResult, LinearPlan, 0);
                    basisLink.UsedWorkflow.Generation++;

                    basisLink.AlgorithmCallback?.Invoke(basisLink.CurrentStage, LinearPlan.Count);
                }

                if (LinearPlan.Count % 500 == 0)
                {
                    Console.WriteLine($"R: {iterationIndex}");
                    basisLink.SessionLink.AssignedLogger.LogQueue.Enqueue($"step 1: {iterationIndex} - {LinearPlan.Count} / {OpersCopy.Count} done\r\n");
                    LastResult = PlanSimulatorEngine.CalculatePlan(LinearPlan, 0, basisLink.StartOfPlanningDate, PlanSimulatorEngineResult.CreateFullInstance(basisLink.SessionLink, basisLink.StartOfPlanningDate)); // Вызывем симулятор плана - НА ВСЯКИЙ СЛУЧАЙ.
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

        (TimeSpan Rechanges, TimeSpan prostoi, TimeSpan TotalTime, TimeSpan TotalTimeNoOp, int timeTransferLeft) HeuristicCheckBlock(PlanSimulatorMod mod, Uco choosenCenter, int poolIndex/*, List<PlanSimulatorBlock> list*/, DateTime startDate, out int errorsCount)
        {

            errorsCount = 0;

            if (mod.IsFixed && mod.FirstOperJust.StartTimeFact.HasValue) return (
                    TimeSpan.Zero,
                    TimeSpan.Zero,
                    mod.FirstOperJust.StartTimeFact.Value - basisLink.MinAcceptableDate,
                    mod.FirstOperJust.StartTimeFact.Value - basisLink.MinAcceptableDate,
                    0);//(float)(mod.FirstOperJust.StartTimeFact.Value - basisLink.MinAcceptableDate).TotalSeconds;

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

                Uco currentSubdivion = basisLink.SessionLink.MasterDataLinks.CentersMasterData.CentersData[choosenCenter].Subdivision;
            var minDateWithMod = mod.PrevMods.Select(prevMod =>
            {
                Uco lastSubdivision = prevMod.BlockLink.GetCenterData(0).CenterModelDataLink.Subdivision;
                return (Mod: prevMod, Time: prevMod.DateOfEnd[0].AddSeconds(basisLink.SessionLink.MasterDataLinks.SubdivisionsTransfersMasterData.GetSubdivisionTransferTime(lastSubdivision, currentSubdivion)));
            }
            );

            int timeTransfer = 0;
            DateTime minDate = basisLink.StartOfPlanningDate;
            if (mod.PrevMods.Count > 0)
            {
                var maxHere = minDateWithMod.MaxBy(x => x.Time);
                minDate = maxHere.Time;
                timeTransfer = (int)(maxHere.Time - maxHere.Mod.DateOfEnd[0]).TotalSeconds;
            }


            if (minDate == DateTime.MinValue) minDate = basisLink.StartOfPlanningDate; // Получаем минимальную дату

            var lastCenterDate = lastBlock == null ? basisLink.StartOfPlanningDate : lastBlock.Lists[0].DateOfEnd[0];

            TimeSpan prostoiDur = minDate - lastCenterDate;

            var rechangesResult = operationA == null ? null : basisLink.SessionLink.MasterDataLinks.RechangesMasterData.FindRechanges(new(lastBlock.GetCenterData(0).CenterDataLink, mod.MinDatesData[0], operationA.Stage, mod.FirstOperJust, choosenCenter));

            TimeSpan rechesDur = rechangesResult == null ? new TimeSpan() : TimeSpan.FromSeconds(rechangesResult.Duration);

            var NaladLengthsMasterData = basisLink.SessionLink.MasterDataLinks.NaladLengthsMasterData;

            bool asFirst = NaladLengthsMasterData.ItsFirstLaunch(rechangesResult, lastBlock?.Lists[0]);
            var OperB = mod.FirstOperJust;
            GottenNaladLengthStruct naladLengths = NaladLengthsMasterData.GetNaladLengthsForCurrentStageOnly(OperB, choosenCenter, asFirst);
            if (OperB.NeededNaladLength.HasValue)
            {
                var a = NaladLengthsMasterData.NaladLengthsConverted(naladLengths.NaladLengthHere, OperB.EdIzmClassificator, OperB.KgKm).Value;
                var b = OperB.NeededNaladLength.Value.Length.Value;
                if (a > b)
                {
                    errorsCount = 1000000;//(int)((a - b) * 1000f);
                }
            }

            //int timeTransfer = 0;
            //PlanSimulatorMod lastModFromPartia = null;
            //if (mod.PrevMods.Count > 0)
            //{
            //    timeTransfer = mod.PrevMods.MaxBy(prevMod =>
            //    {
            //        
            //        prevMod.DateOfEnd[0]
            //    });
            //    if (lastSubdivision != currentSubdivion)
            //    {
            //        timeTransfer = basisLink.SessionLink.MasterDataLinks.SubdivisionsTransfersMasterData.GetSubdivisionTransferTime(lastSubdivision, currentSubdivion);
            //    }
            //}

            // Чтобы сделать только переналадки, хватит вот этого критерия:
            //return (float)rechesDur.TotalSeconds;

            prostoiDur -= rechesDur;

            if (prostoiDur.Ticks < 0) prostoiDur = TimeSpan.Zero;

            // Теперь посчитаем переход в другой цех
            // Из времени перехода можно отнять простой, но не переналадку, так как переналадка должна начаться в другом цехе.
            //timeTransfer -= (int)prostoiDur.TotalSeconds;

            //// Из цеха так же надо вычесть разницу между lastCenterTime и lastMod, чтобы убрать лишний временной отрезок
            //var delta1 = lastModFromPartia == null ? TimeSpan.Zero : (lastCenterDate - lastModFromPartia.DateOfEnd[0]);
            //if (delta1.Ticks < 0) delta1 = TimeSpan.Zero;
            //timeTransfer -= (int)delta1.TotalSeconds;
            //if (timeTransfer < 0) timeTransfer = 0;

            //if (basisLink.SessionLink.SettingsController.CurrentSettings.TransferToGamma)
            //{
            //    prostoiDur = prostoiDur.Add(TimeSpan.FromSeconds(timeTransfer));
            //    timeTransfer = 0;
            //}

            var PplusR = prostoiDur + rechesDur;

            var time1 = lastCenterDate + PplusR;

            var lastDate = time1 + mod.AllTime[choosenCenter];
            //var res1 = (float)(time1 - minDate).TotalSeconds;

            //var res2 = (float)/*rechesDur.TotalSeconds;//*/(lastDate - basisLink.MinAcceptableDate).TotalSeconds;

            //(TimeSpan Rechanges, TimeSpan prostoi, TimeSpan TotalTime, TimeSpan TotalTimeNoOp, int timeTransferLeft)
            return (rechesDur, prostoiDur, lastDate - startDate, time1 - startDate, timeTransfer);

        }

    }
}

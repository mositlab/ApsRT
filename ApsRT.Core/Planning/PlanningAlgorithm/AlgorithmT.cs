using ApsRT.Core.PlanSimulator;
using ApsRT.Core.PlanSimulator.PlanSimulatorEngine;
using ApsRT.Core.Rechanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;

namespace ApsRT.Core.Planning.PlanningAlgorithm
{
    /// <summary>
    /// T - после R в алфавите.
    /// R->T = RT, "Aps RT".
    /// T = Time, Table.
    /// Т = ТИРАН.
    /// </summary>
    internal class AlgorithmT
    {
        public PlanningAlgorithm basisLink;

        public AlgorithmT(PlanningAlgorithm basisLink)
        {
            this.basisLink = basisLink;
        }

        class TCell
        {
            public float Cost;

            public TCell(float cost)
            {
                Cost = cost;
            }
        }

        List<PlanSimulatorBlock> OpersCopy;

        Dictionary<PlanSimulatorBlock,
            Dictionary<PlanSimulatorBlock, TCell>
            > TransitionsCost = new();

        Dictionary<int, int> StagesValues = new();


        public void Start()
        {
            var start = DateTime.Now;

            var INDEX = basisLink.UsedPool.BestPoolIndex;

            OpersCopy = basisLink.UsedPool.CopyBlocksDeep(basisLink, INDEX, true);

            Console.WriteLine("T, beg, " + OpersCopy.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Count());
            Console.WriteLine($"T: begin, blocks count = {OpersCopy.Count}");

            //.Where(x => x.Lists[0].FirstOperJust.Center.Label.Contains("EEL-40")).ToList();
            var poolCopy = new PlanSimulatorPool(basisLink.SessionLink, OpersCopy, basisLink.MinAcceptableDate, basisLink.UsedPool.PoolSize);
            //poolCopy.PrepareOnOneRCData();
            //poolCopy.PrepareAllRCsOnBlocks();

            PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngineResult LastResult = PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(OpersCopy, 0, basisLink.StartOfPlanningDate, PlanSimulatorEngineResult.CreateFullInstance(basisLink.SessionLink, basisLink.StartOfPlanningDate));

            TimeSpan worstPossiblePlanLength = LastResult.ProductionSeconds * (1 + basisLink.SessionLink.SettingsController.CurrentSettings.TMaxDaysPercent / 100.0);
            worstPossiblePlanLength = worstPossiblePlanLength.Add(TimeSpan.FromDays(basisLink.SessionLink.SettingsController.CurrentSettings.TMaxDays));


            var RCgroups = OpersCopy.GroupBy(block => block.GetCenterData(INDEX).Center);

            Dictionary<Uco, (TimeSpan Rechanges, TimeSpan OpersTime, double RottingTime)> RCData = new();

            foreach (var pair in RCgroups)
            {
                var opersSum = pair.Sum(block => block.GetCenterData(INDEX).CenterDataLink.OpersTime.TotalSeconds);
                var rechangesSum = pair.Sum(block => block.GetCenterData(INDEX).CenterDataLink.RechangesTime.TotalSeconds);
                var rotTime = pair.Sum(block => block.Lists.Sum(mod =>
                {
                    var delta = mod.RottingTimeInDays[INDEX];
                    if (delta < 0) delta = 0;
                    return delta;
                }));

                RCData.Add(pair.Key, (TimeSpan.FromSeconds(rechangesSum), TimeSpan.FromSeconds(opersSum), rotTime));
            }

            var worstRCByOpers = RCData.Max(x => x.Value.OpersTime);
            var worstRCsByOpers = RCData.OrderByDescending(x => x.Value.OpersTime).Select(x => x.Key);
            Console.WriteLine($"T: worst RC by opers:");
            foreach (var item in worstRCsByOpers)
            {
                Console.WriteLine($"{item}: {RCData[item].OpersTime} ({Math.Round(RCData[item].OpersTime / worstRCByOpers, 3)}, rechanges: {Math.Round(RCData[item].Rechanges / worstRCByOpers, 3)}, rotTime: {RCData[item].RottingTime})");
            }
            ;

            var onlyBig = worstRCsByOpers
                //.TakeWhile(item =>  || RCData[item].RottingTime > 0)
                .Where(item =>
                (RCData[item].Rechanges.Ticks > 0 && Math.Round(RCData[item].OpersTime / worstRCByOpers, 3) >= 0.4f)
                || RCData[item].RottingTime > 0);

            Console.WriteLine($"T: decided to check these RC:");
            foreach (var item in onlyBig)
            {
                Console.WriteLine($"{item}: {RCData[item].OpersTime} ({Math.Round(RCData[item].OpersTime / worstRCByOpers, 3)}, rechanges: {Math.Round(RCData[item].Rechanges / worstRCByOpers, 3)})");
            }

            ;

            if (onlyBig.Count() == 0) return;

            int noResultIters = 0;
            int forcedToReminimize = 0;

            while (true)
            {
                //Console.WriteLine($"T: blocks count = {OpersCopy.Count}");

                if (basisLink.WannaStop)
                {
                    break;
                }

                noResultIters++;

                var rechanges = LastResult.RechangesBeforeFirstStages
                    .Where(item =>
                    {
                        if (item.Value.RechangeItself == null || item.Value.RechangeItself.Duration == 0) return false;
                        var center = poolCopy.StagesLinks[item.Key].ModLink.BlockLink.GetCenterData(INDEX).Center;
                        return onlyBig.Contains(center);
                    })
                    .OrderByDescending(x =>
                    {
                        StagesValues.TryAdd(x.Key, 3);
                        return StagesValues[x.Key];
                    })
                    .ThenByDescending(x =>
                    {
                        var delta = poolCopy.StagesLinks[x.Key].ModLink.RottingTimeInDays[INDEX]; // По гниению давай
                        if (delta <= 0) delta = 0;
                        return delta;
                    })
                    .ThenByDescending(x =>
                    {
                        return x.Value.RechangeItself.Duration;
                    })
                    .ToArray();
                if (rechanges.Length == 0) break;

                var operIndex = rechanges[0].Key;

                var value = StagesValues[operIndex];

                //if (rechanges[0].Value.RechangeItself.Duration * value  == 0) break;

                var blockLink = poolCopy.StagesLinks[operIndex].ModLink.BlockLink;

                var blockIndex = blockLink.BlockIndex[INDEX];

                var centerID = blockLink.GetCenterData(INDEX).Center;

                PlanSimulatorPool.MoveCentersInAllBlocks(OpersCopy, INDEX, 1);
                PlanSimulatorPool.MoveBlockIndexesInAllBlocks(OpersCopy, INDEX, 1);
                PlanSimulatorPool.MovePassedInfoInAllBlocks(OpersCopy, INDEX, 1);

                //OpersCopy = PlanSimulatorPool.Refragmentate(OpersCopy, 0, 1);

                //var chron = basisLink.CheckChronology(OpersCopy, out _, out _);
                //if (!chron)
                //{
                //    ;
                //}

                var opertoIns = OpersCopy[blockIndex];
                OpersCopy.RemoveAt(blockIndex); // Удаляем её, намереваясь потом вставить

                (int MIN, int MAX, int Prev, int Next) = poolCopy.FindDiapazons(opertoIns, 1, centerID, blockIndex);

                //if (errs)
                //{
                //    ;
                //}

                // Итого
                //if (MAX <= MIN) // Если границы плохие, то возвращаем операцию
                //{
                //    OpersCopy.Insert(blockIndex, opertoIns);
                //    value -= 1; // Вариант нежизнеспособен.
                //}

                /*
                 A - блок, который стоял перед блоком B
                 B - блок, перед которым хочу поставить

                 C - пред. блок, который за мною стоял
                 D - БЛОК, КОТОРЫЙ ХОЧУ ПЕРЕСТАВИТЬ
                 E - след блок, за мною же

                Надо выполнить след. условие:
                (A->B)+(C->D)+(D->E) = prev
                (A->D)+(D->B)+(C->E) = now

                prev - now = delta

                now должен быть меньше prev; delta > 0 - является искомым условием.

                 */

                PlanSimulatorBlock D = opertoIns;
                PlanSimulatorBlock C = opertoIns.PreviousBlockOnThisRC[INDEX];
                PlanSimulatorBlock E = opertoIns.NextBlockOnThisRC[INDEX];

                var rmd = basisLink.SessionLink.MasterDataLinks.RechangesMasterData;

                var DID = D.Lists[0].FirstOperJust.Index;
                var CID = C == null ? -1 : C.Lists[0].FirstOperJust.Index;

                // c -> d
                int CtoD = C == null ? 0 : rmd.FindRechanges(new Rechanges.RechangesEngine.RechangesEngineData(D.GetCenterData(INDEX).CenterDataLink, D.Lists[0].MinDatesData[INDEX], C.Lists[0].FirstOperJust, D.Lists[0].FirstOperJust, D.GetCenterData(INDEX).Center)).Duration;


                var rech1 = LastResult.RechangesBeforeFirstStages[D.Lists[0].FirstOperJust.Index];
                ;

                // d -> e
                int DtoE = E == null ? 0 : rmd.FindRechanges(new Rechanges.RechangesEngine.RechangesEngineData(E.GetCenterData(INDEX).CenterDataLink, E.Lists[0].MinDatesData[INDEX], D.Lists[0].FirstOperJust, E.Lists[0].FirstOperJust, E.GetCenterData(INDEX).Center)).Duration;

                if (E != null)
                {
                    var rech2 = LastResult.RechangesBeforeFirstStages[E.Lists[0].FirstOperJust.Index];
                    ;
                }

                // c -> e
                int CtoE = E == null || C == null ? 0 : rmd.FindRechanges(new Rechanges.RechangesEngine.RechangesEngineData(E.GetCenterData(INDEX).CenterDataLink, E.Lists[0].MinDatesData[INDEX], C.Lists[0].FirstOperJust, E.Lists[0].FirstOperJust, E.GetCenterData(INDEX).Center)).Duration;

                var possibleBlocks = OpersCopy
                    .Where(block => block != opertoIns.NextBlockOnThisRC[INDEX]
                    && block.BlockIndex[INDEX] >= MIN
                    && block.BlockIndex[INDEX] < MAX
                    && block.GetCenterData(INDEX).Center == centerID);

                var possibleBlocksFinal = possibleBlocks.Select((block, e_index) =>
                    {
                        PlanSimulatorBlock B = block;
                        PlanSimulatorBlock A = block.PreviousBlockOnThisRC[INDEX];

                        // a -> d
                        var AtoDdata = A == null ? null : rmd.FindRechanges(new Rechanges.RechangesEngine.RechangesEngineData(D.GetCenterData(INDEX).CenterDataLink, D.Lists[0].MinDatesData[INDEX], A.Lists[0].FirstOperJust, D.Lists[0].FirstOperJust, D.GetCenterData(INDEX).Center));
                        int AtoD = A == null ? 0 : AtoDdata.Duration;

                        // a -> b
                        var AtoBdata = A == null ? null : rmd.FindRechanges(new Rechanges.RechangesEngine.RechangesEngineData(B.GetCenterData(INDEX).CenterDataLink, B.Lists[0].MinDatesData[INDEX], A.Lists[0].FirstOperJust, B.Lists[0].FirstOperJust, B.GetCenterData(INDEX).Center));
                        int AtoB = A == null ? 0 : AtoBdata.Duration;

                        // d -> b
                        var DtoBdata = rmd.FindRechanges(new Rechanges.RechangesEngine.RechangesEngineData(B.GetCenterData(INDEX).CenterDataLink, B.Lists[0].MinDatesData[INDEX], D.Lists[0].FirstOperJust, B.Lists[0].FirstOperJust, B.GetCenterData(INDEX).Center));
                        int DtoB = DtoBdata.Duration;

                        var AID = A == null ? -1 : A.Lists[0].FirstOperJust.Index;
                        var rech3 = LastResult.RechangesBeforeFirstStages[B.Lists[0].FirstOperJust.Index];
                        ;
                        /*
                         
                Надо выполнить след. условие:
                (A->B)+(C->D)+(D->E) = prev
                (A->D)+(D->B)+(C->E) = now

                prev - now = delta

                now должен быть меньше prev; delta > 0 - является искомым условием.
                         */
                        var prev = AtoB + CtoD + DtoE;
                        var now = AtoD + DtoB + CtoE;

                        var delta = prev - now;

                        var worstDelta = block.Lists.Max(x => x.RottingTimeInDays[INDEX]);
                        if (worstDelta <= 0) worstDelta = 0;

                        return (Block: block, HisValue: delta, RotDelta: worstDelta, AtoBdata: AtoBdata, DtoBdata: DtoBdata, AtoDdata: AtoDdata, A: A, B: B);
                    })
                    .OrderByDescending(x =>
                    {
                        // Давай сначала по гниению
                        return x.RotDelta == 0 ? -1 : x.Block.BlockIndex[INDEX]; // Сначала давай по гниению - чем левее, тем лучше
                    })
                    .ThenByDescending(x =>
                        {
                            TransitionsCost.TryAdd(D, new());
                            TransitionsCost[D].TryAdd(x.Block, new TCell(1));
                            var value = TransitionsCost[D][x.Block];
                            return x.HisValue * value.Cost;
                        })
                    .ToArray();

                if (possibleBlocksFinal.Length == 0)
                {
                    value -= 1;
                    OpersCopy.Insert(blockIndex, D); // В оригинале тоже вносим такие изменения
                }
                else
                {
                    var first = possibleBlocksFinal[0];
                    if (first.HisValue <= 0 && first.RotDelta == 0)
                    {
                        value -= 1;
                        OpersCopy.Insert(blockIndex, D); // В оригинале тоже вносим такие изменения
                    }
                    else
                    {
                        var pointToInsert = first.Block.BlockIndex[INDEX];
                        if (pointToInsert > blockIndex) pointToInsert--; // Так как я удалял кубик, то и индексация должна была сдвинуться

                        if (pointToInsert == blockIndex)
                        {
                            ;
                        }

                        var swallowCopy = PlanSimulatorPool.CopyBlocksSwallow(OpersCopy);

                        var neededPoint = swallowCopy.FindIndex(x => x == first.Block);

                        if (neededPoint != pointToInsert)
                        {
                            ;
                        }

                        var Awas = first.A;
                        var Bwas = first.B;

                        HashSet<PlanSimulatorBlock> movedElements1 = new();
                        int moves = 0;
                        if (pointToInsert < MIN)
                        {
                            moves= PlanSimulatorPool.MoveLeft(swallowCopy, D, blockIndex, pointToInsert, false, 1, Awas, Bwas, out movedElements1);
                        }
                        else
                        {
                            swallowCopy.Insert(pointToInsert, D);
                        }

                        var chron = basisLink.CheckChronology(swallowCopy, out _, out _);
                        if (!chron)
                        {
                            ;
                        }


                        var localResult = PlanSimulatorEngine.CalculatePlan(swallowCopy, 1, basisLink.StartOfPlanningDate, PlanSimulatorEngineResult.CreateFullInstance(basisLink.SessionLink, basisLink.StartOfPlanningDate));


                        var newCtoE = E == null ? (null, null, -1) : localResult.RechangesBeforeFirstStages[E.Lists[0].FirstOperJust.Index];
                        var newAtoD = localResult.RechangesBeforeFirstStages[D.Lists[0].FirstOperJust.Index];
                        var newDtoB = localResult.RechangesBeforeFirstStages[Bwas.Lists[0].FirstOperJust.Index];

                        int errorsRateHere = 0;

                        //Console.Write(moves + ": ");
                        //if (movedElements1.Contains(D)) Console.Write("D");
                        //if (Awas != null && movedElements1.Contains(Awas)) Console.Write("Awas");
                        //if (movedElements1.Contains(Bwas)) Console.Write("Bwas");
                        //if (movedElements1.Contains(E)) Console.Write("E");
                        //if (movedElements1.Contains(C)) Console.Write("C");

                        if (D.PreviousBlockOnThisRC[1] != Awas)
                        {
                            errorsRateHere += 1;
                        }

                        if (Bwas.PreviousBlockOnThisRC[0] != Awas)
                        {
                            errorsRateHere += 10;
                        }

                        if (Awas != null && Awas.NextBlockOnThisRC[0] != Bwas)
                        {
                            errorsRateHere += 100;
                        }

                        if (Awas != null && Awas.NextBlockOnThisRC[1] != D)
                        {
                            errorsRateHere += 1000;
                        }

                        if (D.NextBlockOnThisRC[1] != Bwas)
                        {
                            errorsRateHere += 10000;
                        }

                        if (D.NextBlockOnThisRC[0] != E)
                        {
                            errorsRateHere += 100000;
                        }

                        if (C.NextBlockOnThisRC[1] != E)
                        {
                            errorsRateHere += 1000000;
                        }

                        if (C.NextBlockOnThisRC[0] != D)
                        {
                            errorsRateHere += 10000000;
                        }

                        //Console.WriteLine($"{pointToInsert < MIN}: {errorsRateHere}");

                        var difference = localResult.RechangesBeforeFirstStages.Where(x =>
                        {
                            var was0 = LastResult.RechangesBeforeFirstStages[x.Key];
                            var was = was0.RechangeItself;
                            if (x.Value.LastOperID != was0.LastOperID) return true;
                            if (x.Value.Interval.Duration != was0.Interval.Duration) return true;
                            if (x.Value.RechangeItself == null && was != null) return true;
                            if (x.Value.RechangeItself != null && was == null) return true;
                            if (x.Value.RechangeItself == null && was == null) return false;
                            if (x.Value.RechangeItself.Duration != was.Duration) return true;
                            if (x.Value.RechangeItself.ProvokesNaladLengthLaunch != was.ProvokesNaladLengthLaunch) return true;
                            if (x.Value.RechangeItself.Result == null && was.Result != null) return true;
                            if (x.Value.RechangeItself.Result != null && was.Result == null) return true;
                            if (x.Value.RechangeItself.Result == null && was.Result == null) return false;
                            if (x.Value.RechangeItself.Result.Length != was.Result.Length) return true;
                            return false;
                        }
                            ).Select(x => (Now: x.Value, Was: LastResult.RechangesBeforeFirstStages[x.Key]));

                        var intervalsNow = difference.Sum(x => x.Now.Interval.Duration.TotalSeconds);
                        var intervalsWas = difference.Sum(x => x.Was.Interval.Duration.TotalSeconds);

                        var link = TransitionsCost[D][first.Block];
                        if (
                            (
                            (localResult.TaraCriticalUsage < LastResult.TaraCriticalUsage) // Сначала прочекаем гниль
                            || // Если не удалось, посмотрим хотя бы на переналадки
                            (localResult.TaraCriticalUsage == LastResult.TaraCriticalUsage && localResult.RechangesSecondsWithoutLunches < LastResult.RechangesSecondsWithoutLunches)
                            )
                            && localResult.ProductionSeconds <= worstPossiblePlanLength
                            && localResult.CatchedNaladLengthsErrorsCount <= LastResult.CatchedNaladLengthsErrorsCount)
                        {
                            ;
                            //LastResult = localResult;
                            OpersCopy = swallowCopy;
                            //PlanSimulatorPool.MoveBlockIndexesInAllBlocks(OpersCopy, 1, INDEX);
                            //OpersCopy.Insert(pointToInsert, D); // В оригинале тоже вносим такие изменения

                            LastResult = PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(OpersCopy, INDEX, basisLink.StartOfPlanningDate, PlanSimulatorEngineResult.CreateFullInstance(basisLink.SessionLink, basisLink.StartOfPlanningDate));

                            basisLink.UpdateBcsSuccess(localResult, OpersCopy, INDEX);

                            noResultIters = 0;
                            link.Cost += 0.1f;
                        }
                        else
                        {
                            ;
                            OpersCopy.Insert(blockIndex, D); // В оригинале тоже вносим такие изменения
                            link.Cost -= 0.1f;
                            value -= 1;
                        }
                    }
                }

                    ;

                //value -= 1;

                if (value < 0) value = -OpersCopy.Count;

                StagesValues[operIndex] = value;

                int min = 0;
                if (StagesValues.All(x => x.Value < 0))
                {
                    min = StagesValues.Min(x => x.Value);
                    forcedToReminimize++;
                }
                foreach (var x in StagesValues)
                {
                    if (x.Key == operIndex)
                    {
                        //StagesValues[x.Key]--;
                    }
                    else
                    {
                        if (x.Value < 0)
                        {
                            var nw = x.Value - min + 1;
                            if (nw > 3) nw = 3;
                            StagesValues[x.Key] = nw;

                        }
                    }
                }
                basisLink.UsedWorkflow.Generation++;

                basisLink.AlgorithmCallback?.Invoke(basisLink.CurrentStage, noResultIters);

                if (noResultIters >= 1000 || forcedToReminimize > 100)
                {
                    break;
                }
            }

            ;
        }
    }
}

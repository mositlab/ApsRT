
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using ApsRT.Core.Centers.MasterData;
using ApsRT.Core.Taras.TaraSearchEngine;
using Azure;
using ApsRT.Core.NaladLengths;
using System.Diagnostics.Metrics;
using ApsRT.Core.Stages;
using ApsRT.Core.Partias.PartiasClasses;
using ApsRT.Core.Taras.TaraSearchEngine;
using ZLinq;
using ApsRT.Core.PlanSimulator;
using ApsRT.Core.PlanSimulator.PlanSimulatorEngine;

namespace ApsRT.Core.Planning.PlanningAlgorithm
{
    public class AlgorithmG
    {
        public PlanningAlgorithm basisLink;

        public AlgorithmG(PlanningAlgorithm basisLink)
        {
            this.basisLink = basisLink;
        }

        public void Start(int parallelCount, int depth)
        {

            //var ops = OpersPool.CopyBlocks(basisLink.BestBlocksList, false);

            //var preparedData = new OpersPoolConstructor(ops, basisLink.MinAcceptableDate);

            var tasks = new Task[parallelCount];//16

            var workflow = basisLink.UsedWorkflow;

            workflow.RawData[0] = 0; // taraCheckRestricts
            workflow.RawData[1] = new ConcurrentDictionary<int, int>(); // NaladErrorsCounter
            workflow.RawData[2] = true; // Чекать ли тару и налад. длины в след. раз
            workflow.RawData[3] = new ConcurrentDictionary<Uco, int>(); // Это счётчик маршрутов РЦ

            //var taraCheckRestricts = This.FixTaraCheckBox.Checked;

            //var poolLoc = new OpersPool[parallelCount];
            var Randoms = new Random[parallelCount];
            for (var i = 0; i < parallelCount; i++)
            {
                //poolLoc[i] = new OpersPool(basisLink.SessionLink, ops, preparedData, basisLink.MinAcceptableDate);
                Randoms[i] = new Random();
            }

            var ALL_COUNT = basisLink.UsedPool.Blocks.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Count();
            Console.WriteLine("G, beg, " + ALL_COUNT);
            ALL_COUNT = basisLink.UsedPool.Blocks.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Distinct().Count();
            Console.WriteLine("G, beg, " + ALL_COUNT);

            // TODO: переделать на EventHandler и использовать InvokeAsync

            var max = depth * 32;

            //ConcurrentDictionary<int, int> NaladErrorsCounter = new();

            List<PlanSimulatorBlock>[] opersCopy = new List<PlanSimulatorBlock>[parallelCount];

            while (!basisLink.WannaStop)
            {
                workflow.FoundForDynamic = false;
                var count = depth;
                var va = workflow.Generation - basisLink.LastGen;

                if (va > max) // count
                {
                    for (var i = 0; i < parallelCount; i++)
                    {
                        Randoms[i] = new Random(); // Попробуем поменять рандом
                    }
                }


                try
                {
                    var resu = Parallel.For(0, parallelCount - 3, i => // -3 потому что последняя зарезервирована для ручного анализа
                    {
                        if (i == basisLink.UsedPool.BestPoolIndex) return;
                        var opersCopyL = PlanSimulatorPool.CopyBlocksSwallow(basisLink.UsedPool.Blocks);//.OrderBy(x => x.BlockIndex[index]).ToList();
                        PlanSimulatorPool.MoveCentersInAllBlocks(opersCopyL, basisLink.UsedPool.BestPoolIndex, i);
                        opersCopy[i] = opersCopyL;
                    });
                    while (!resu.IsCompleted)
                    {
                        ;
                    }
                    var bestIndex = basisLink.UsedPool.BestPoolIndex;

                    resu = Parallel.For(0, parallelCount - 3, (i, pls) =>
                    {
                        if (i == bestIndex) return;

                        G_Iteration(opersCopy[i], Randoms[i], workflow, parallelCount, depth, i);

                        //if (workflow.currentMode == 6) pls.Stop();
                    });
                    while (!resu.IsCompleted)
                    {
                        ;
                    }
                    //for (int K = 0; K < tasks.Length; K++)
                    //{
                    //    var n = K;
                    //tasks[n] = Task.Factory.StartNew(() => doit(n));
                    //    tasks[n] = Task.Run(() => doit(n));
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(delegate { doit(n, Lists[n]); }));
                    //}
                    //Task.WaitAll(tasks);
                }
                catch (AggregateException ex)
                {
                    Console.WriteLine($"Task has finished with exception {ex.InnerException.Message}");
                }
                // Если по итогу лучший результат сказал "тару можно не искать" - то опять сбрасываем в -10.
                int taraCounter = (int)workflow.RawData[0];
                if ((bool)workflow.RawData[2] == false) taraCounter = -50;
                taraCounter++; // И в любом случае даём +1. Если добирается до 0 - считать тару. Если до 10 - сброс в любом случае.
                if (taraCounter == 10) taraCounter = -50;
                workflow.RawData[0] = taraCounter;

                if (!workflow.NextIteration()) break;
            }
        }

        public void ChangeRC(PlanSimulatorBlock opertoIns, Random r, int poolIndex)
        {
            var CentersMasterData = basisLink.SessionLink.MasterDataLinks.CentersMasterData;

            var sdvig = new PlanSimulatorMod(opertoIns.Lists.AsValueEnumerable().SelectMany(x => x.StagesAsNormal).ToList(), false, PlanningModellingType.AfterPartias, -1, opertoIns.PoolLink.PoolSize);
            var prevs = sdvig.StagesAsNormal.AsValueEnumerable().SelectMany(x => x.Prevs).Distinct().Where(x => 
            !basisLink.SessionLink.MasterDataLinks.StagesMasterData.AllStages[x].CurrentTara.EmptyCheck())
                .ToDictionary(x => x, x => basisLink.SessionLink.MasterDataLinks.StagesMasterData.AllStages[x].CurrentTara);

            var nextRCs = TaraSearchEngine.FindNeededNextRCs(sdvig.StagesAsNormal, null);// sdvig.StagesAsNormal.AsValueEnumerable().SelectMany(x => x.Nexts).Distinct().Select(x => basisLink.SessionLink.MasterDataLinks.StagesMasterData.AllStages[x].Center).ToHashSet();

            var PossibleRCsHere = opertoIns.PossibleRCsHere;

            var options = new SumIsPossibleOptions(false)
            {
                LogSomeErrors = false,
                TaraMustBeThis = opertoIns.ExistingTaraHere.Select(x => x.Key).ToHashSet(),
                PrevsInfo = prevs,
                RCMustBeThis = PossibleRCsHere,
                NextRCsMustBeThese = nextRCs,
                SaveAvailableResults = true
            };
            if (
                TaraSearchEngine.SumIsPossible(sdvig, out _, out _, out _, out _, out _, out var searchData, options)
                && TaraSearchEngine.FindSomeCentersBecauseOfPrevs(sdvig, PossibleRCsHere.ToList(), prevs, options.TaraMustBeThis.ToList(), out _, out var allRCs, out _, false)
                )
            {
                var possible = searchData.PossibleCentersAndTaras.AsValueEnumerable().Where((x, index) => (searchData.AvailableResults == null || searchData.AvailableResults[index] >= 0)
                &&
                allRCs.Contains(x.RC)
                &&
                x.RC != opertoIns.GetCenterData(poolIndex).Center
                &&
                (!opertoIns.Lists[0].StagesAsNormal[0].IsVspom || CentersMasterData.CentersData[x.RC].AbleToVspom)
                ).ToArray();

                if (possible.Length > 0)
                {
                    //var finalRC = allRCs.OrderBy(x => (rcRoutesData.TryGetValue(rcRouteToWorkWith, out var lvl0) && lvl0.RCsData.TryGetValue(x, out var data) ? data : new TimeSpan())).First();//list[RC /*r.Next(0, possible.Count)*/]; // по итогу выбираем его
                    var final = possible[r.Next(0, possible.Length)];
                    opertoIns.SetCenter(final.RC, poolIndex);//ActualRC = final.RC;
                    opertoIns.PoolLink.UpdateOpsOnOneRC(opertoIns, poolIndex);
                    //for (int i = 0; i < opertoIns.Lists.Count; i++)
                    //{
                    //    for (int j = 0; j < opertoIns.Lists[i].StagesAsNormal.Count; j++)
                    //    {
                    //        opertoIns.Lists[i].StagesAsNormal[j].Center = final.RC;
                    //    }
                    //}
                    //setPos(N);
                    //ChangedRC = true;
                }
            }
        }


        public void G_Iteration(List<PlanSimulatorBlock> opersCopy, Random r, PlanningAlgorithmWorkflow workflow, int parallelCount, int depth, int poolIndex)
        {
            var CentersMasterData = basisLink.SessionLink.MasterDataLinks.CentersMasterData;

            var WillThis = -1;
            var onlyLeft = false;
            //int WannaSendHere = -1;

            var mode1 = workflow.currentMode == 1; // Заказы
            var mode2 = workflow.currentMode == 2; // 
            var mode3 = workflow.currentMode == 3;//LoadedData.Method == Save.CalcType.Prostoi || (or && currentMode == 3);
            var mode6 = workflow.currentMode == 6; // Распределение РЦ
            onlyLeft = mode3;

            int mode6Counter = 0;

            Dictionary<Uco, (DateTime WorstTime, Dictionary<Uco, TimeSpan> RCsData)> rcRoutesData = null;


            //WillThis = ChooseSomething(mode1, mode2, mode3, LastOrdersPointsList, LastRechangesPointsList, LastProstoiPointsList);
            bool isFirst = true;
            //bool TriedNaladLengthsFixer = false;
            //bool FixedNaladLengths = false;
            PlanSimulatorEngineResult prevResult = workflow.BestValue;
            for (var H = 0; H < depth; H++)
            {
                if (basisLink.WannaStop)
                {
                    break;
                }

                Uco rcRouteToWorkWith = new();
                Uco worstRC;
                ConcurrentDictionary<Uco, int> dic = (ConcurrentDictionary<Uco, int>)workflow.RawData[3];

                switch (workflow.currentMode)
                {
                    case 6: // Задача - поменять РЦ?
                        {
                            if (mode6Counter > 0 && rcRoutesData.Count > 0)
                            {
                                for (int vvv = 0; vvv < 1; vvv++)
                                {
                                    lock (basisLink.LockerObject)
                                    {
                                        if (dic.Count == 0) // Если словарь пуст,
                                        {
                                            foreach (var rcRoute in rcRoutesData)
                                            {
                                                dic.TryAdd(rcRoute.Key, 0);
                                            }
                                        }
                                        if (dic.All(x => x.Value < 0)) // Если они все внезапно стали все отрицательными
                                        {
                                            var neededOffset = dic.Max(x => x.Value);
                                            foreach (var rcRoute in rcRoutesData)
                                            {
                                                dic[rcRoute.Key] = 0;
                                            }
                                        }
                                        var rcRouteToWorkWith0 = dic.Where(x => /*x.Key.Label.Contains("9900")*/x.Value >= 0).OrderByDescending(x => /*rcRoutesData[x.Key].WorstTime*/CentersMasterData.RCRoutesToListDic.TryGetValue(x.Key, out var rcs) ? rcs.Count : 0);/*MaxBy(x => rcRoutesData[x.Key].WorstTime)*//*MinBy(x => x.Value*/
                                        if (rcRouteToWorkWith0.Count() > 0)
                                        {
                                            var rcRouteToWorkWith1 = rcRouteToWorkWith0.Take(Math.Min(4, rcRouteToWorkWith0.Count()));
                                            rcRouteToWorkWith = rcRouteToWorkWith1.ElementAt(r.Next(0, rcRouteToWorkWith1.Count())).Key;
                                            dic[rcRouteToWorkWith]++;
                                            worstRC = rcRoutesData[rcRouteToWorkWith].RCsData.MaxBy(x => x.Value).Key;
                                            if (dic[rcRouteToWorkWith] == parallelCount * 2) dic[rcRouteToWorkWith] = -100;


                                            var allBlocksHere = opersCopy.Select((x, index) => (x, index)).Where(y => y.x.GetCenterData(poolIndex).Center == worstRC/*Lists[0].FirstOper.RCRoute == rcRouteToWorkWith*/).Select(y => y.index).ToArray();

                                            var N = allBlocksHere[r.Next(0, allBlocksHere.Length)];//WillThis > -1 ? WillThis : r.Next(GLOBAL_FIXED_MIN, opersCopy.Blocks.Count); // Берём какую нибудь операцию, ОСНОВЫВАЯСЬ на нашем опыте.
                                            var opertoIns = opersCopy[N];

                                            ChangeRC(opertoIns, r, poolIndex);

                                        }
                                    }

                                    //var neededOtdTaras =;

                                    //var N2 = 0;//.Next(0, opertoIns.Count); - по итогу выбираем самую первую, 0. Потом поменяем ваще все. // N - это всё ещё старый номер
                                    //var list = opertoIns.Lists[0].PossibleRCsHere;
                                    mode6Counter = 1;
                                }
                            }
                            break;
                        }
                    default: // Режим классической G - рандомная перестановка
                        {
                            var N = WillThis > -1 ? WillThis : r.Next(basisLink.GLOBAL_FIXED_MIN, opersCopy.Count); // Берём какую нибудь операцию
                            if (WillThis != -2) // Если -2, то значит пред. итерация запретила что-то менять - нужно лишь просто пересчитать.
                            {
                                if (false/*LoadedData.MoveChilds*/)
                                    setPos(N);
                                else
                                {
                                    int NN = setPos(N);
                                    if (NN != -1)
                                    {
                                        WillThis = opersCopy[NN].Lists[0].FirstOperJust.Nexts.Count > 0 ? opersCopy[NN].Lists[0].FirstOperJust.Nexts[0] : -1;
                                        if (WillThis != -1) WillThis = opersCopy.FindIndex(x => x.Lists.Any(y => y.StagesAsNormal.Any(z => z.Index == WillThis)));
                                        if (WillThis != -1)
                                            H--;
                                    }
                                }
                            }
                            break;
                        }
                }

                int setPos(int N)
                {
                    var opertoIns = opersCopy[N];
                    WillThis = -1;
                    opersCopy.RemoveAt(N); // Удаляем её, намереваясь потом вставить

                    (int MIN, int MAX, int Prev, int Next) = opersCopy[0].PoolLink.FindDiapazons(opertoIns, poolIndex, opertoIns.GetCenterData(poolIndex).Center, N);

                    //if (errs)
                    //{
                    //    ;
                    //}

                    // Итого
                    if (MAX <= MIN) // Если границы плохие, то возвращаем операцию
                    {
                        opersCopy.Insert(N, opertoIns);
                        return -1; // Вариант нежизнеспособен.
                    }

                    //if (onlyLeft || workflow.currentMode == 4) MAX = N;

                    // TODO: Prev и Next нужно, чтобы имело смысл переставлять точнее? На самом деле не особо.
                    var RangNow = WillThis == -1 ?
                        //(WannaSendHere != -1 && WannaSendHere >= MIN && WannaSendHere <= MAX)
                        //? WannaSendHere :
                        r.Next(MIN, Math.Min(MAX + 1, opersCopy.Count)) : MIN;

                    opersCopy.Insert(RangNow, opertoIns);

                    if (!mode6 && r.NextDouble() > 0.75) ChangeRC(opertoIns, r, poolIndex);

                    return RangNow;
                }

                bool taraCheckRestricts = (int)workflow.RawData[0] >= 0;

                PlanSimulatorEngineResult res;

                //var dt1 = DateTime.Now;
                res = PlanSimulatorEngine.CalculatePlan(opersCopy, poolIndex, basisLink.MinAcceptableDate, new PlanSimulatorEngineResult(basisLink.SessionLink, basisLink.MinAcceptableDate)
                {
                    FirstStagesFullDurations = mode6 && mode6Counter == 0 ? new Dictionary<int, CachedInterval>() : null,
                    FirstStagesStartDate = mode6 && mode6Counter == 0 ? new Dictionary<int, DateTime>() : null,
                }, prevResult);

                //if (!TriedNaladLengthsFixer)
                //{
                //    FixedNaladLengths = FixSomeNaladLengths(opersCopy, res0);
                //    TriedNaladLengthsFixer = true;
                //    if (FixedNaladLengths)
                //    {
                //        prevResult = res0;
                //        H--;
                //        continue; // Провоцируем вернуться в начало цикла, чтоб изменить РЦ
                //    }
                //    else
                //    {
                //        res = res0;
                //    }
                //}
                //else
                //{
                //    res = res0;
                //}

                if (mode6 && mode6Counter == 0)
                {
                    var groups = opersCopy.SelectMany(block => block.Lists.Select(x => (block, x))).Where(
                        x => {

                            var rcRoute = x.x.FirstOperJust.RCRoute;
                            return !rcRoute.IsEmpty && CentersMasterData.RCRoutesToListDic.TryGetValue(rcRoute, out var list) && list.Count > 1;
                        }
                    ).GroupBy(pair => pair.x.FirstOperJust.RCRoute);//.Blocks.SelectMany()

                    Dictionary<Uco, (DateTime WorstTime, Dictionary<Uco, TimeSpan> RCsData)> final = groups.ToDictionary(x => x.Key, x =>
                    {

                        // (DateTime.MinValue WorstTime, new Dictionary<Uco, TimeSpan>() RCsData)
                        return (x.Max(mod => res.FirstStagesStartDate[mod.x.FirstOperJust.Index] + res.FirstStagesFullDurations[mod.x.FirstOperJust.Index].Duration), // макс время окончания берем

                        x.GroupBy(mod => mod.block.GetCenterData(poolIndex).Center) // сначала группируем по РЦ
                        .ToDictionary(mod => mod.Key, // РЦ - ключ
                        mod => TimeSpan.FromSeconds(mod.Sum(x => res.FirstStagesFullDurations[x.x.FirstOperJust.Index].Duration.TotalSeconds))) // сумма всех длительностей с учетом обедов - то, что нужно

                        );
                    });

                    rcRoutesData = final;
                    H--;
                    mode6Counter++;
                    continue; // Провоцируем вернуться в начало цикла, чтоб изменить РЦ
                }

                var t = res;

                var nonStrictB = false;
                var strictB = false;

                var OtherCritRestrictions = workflow.CheckRestrictions(t);

                ref PlanSimulatorEngineResult BestValue = ref workflow.BestValue;

                var R6 = !taraCheckRestricts || (t.TaraErrorsCount <= BestValue.TaraErrorsCount);

                if (mode1) // Уменьшить срывы
                {
                    nonStrictB = //t.TotalTimeSeconds <= origTime.TotalTimeSeconds &&
                                 //t.MostRCSeconds <= BestValue.MostRCSeconds
                                 //&&
                                 //t.RechangesSeconds <= BestValue.RechangesSeconds
                                 //&&
                     t.DaysRuinedValue <= BestValue.DaysRuinedValue;

                    strictB = nonStrictB
                     &&
                     t.DaysRuinedValue < BestValue.DaysRuinedValue;
                }
                if (mode2) // Лучшие переналадки
                {
                    nonStrictB = t.RechangesSeconds <= BestValue.RechangesSeconds; // Как и количество переналадок
                                                                                   //&&
                                                                                   //(!LoadedData.HalfHourAccepted
                                                                                   //^ // XOR
                                                                                   //    (t.MostRCSeconds <= BestValue.MostRCSeconds // Дедлайн загруженного РЦ лучше?
                                                                                   //    ||
                                                                                   //       (
                                                                                   //       Math.Abs((BestValue.MostRCSeconds - t.MostRCSeconds).TotalSeconds) < 1800
                                                                                   //       &&
                                                                                   //       t.RechangesSeconds <= BestValue.RechangesSeconds // Ну или хотя бы переналадок меньше?
                                                                                   //       )
                                                                                   //    )
                                                                                   //);

                    strictB = nonStrictB
                        &&
                     t.RechangesSeconds < BestValue.RechangesSeconds; // Как и количество переналадок
                     //&&
                     //(!LoadedData.HalfHourAccepted
                     //^ // XOR
                     //    (t.MostRCSeconds < BestValue.MostRCSeconds // Дедлайн загруженного РЦ лучше?
                     //    ||
                     //       (
                     //       Math.Abs((BestValue.MostRCSeconds - t.MostRCSeconds).TotalSeconds) < 1800
                     //       &&
                     //       t.RechangesSeconds < BestValue.RechangesSeconds // Ну или хотя бы переналадок меньше?
                     //       )
                     //    )
                     //);
                }
                if (mode3)
                {
                    nonStrictB = //t.TotalTimeSeconds <= origTime.TotalTimeSeconds &&
                                 //t.RuinedCount <= origTime.RuinedCount // Количество заказов должно быть меньше срываемого
                                 //&&
                                 //t.RechangesSeconds <= origTime.RechangesSeconds // Как и количество переналадок
                                 //&&
                     t.ProstoiSeconds <= BestValue.ProstoiSeconds;

                    strictB = nonStrictB
                        &&
                     t.ProstoiSeconds < BestValue.ProstoiSeconds;
                }
                if (workflow.currentMode == 5)
                {
                    nonStrictB = //t.TotalTimeSeconds <= origTime.TotalTimeSeconds &&
                                 //t.RuinedCount <= origTime.RuinedCount // Количество заказов должно быть меньше срываемого
                                 //&&
                                 //t.RechangesSeconds <= origTime.RechangesSeconds // Как и количество переналадок
                                 //&&
                     t.TaraRedutantUsage <= BestValue.TaraRedutantUsage;

                    strictB = nonStrictB
                        &&
                     t.TaraRedutantUsage < BestValue.TaraRedutantUsage;
                }
                if (workflow.currentMode == 4)
                {
                    nonStrictB = //t.TotalTimeSeconds <= origTime.TotalTimeSeconds
                                 //&&
                     t.ProductionSeconds <= BestValue.ProductionSeconds;

                    strictB = nonStrictB
                        &&
                     t.ProductionSeconds < BestValue.ProductionSeconds;
                }
                if (workflow.currentMode == 7)
                {
                    nonStrictB = //t.TotalTimeSeconds <= origTime.TotalTimeSeconds
                                 //&&
                     t.BadOffsets <= BestValue.BadOffsets;

                    strictB = nonStrictB
                        &&
                     t.BadOffsets < BestValue.BadOffsets;
                }

                if (mode6)
                {
                    nonStrictB = strictB = true;
                    if (mode6Counter == 1 && OtherCritRestrictions && rcRoutesData.Count > 0)
                        dic[rcRouteToWorkWith]--;
                }

                //if (IS_DEBUGGING)
                //{
                //    if (CheckChronology(opersCopy.Blocks, out _, out _))
                //    {
                //        ;
                //    }
                //}

                // Выбираем след. точку
                //WillThis = ChooseSomething(mode1, mode2, mode3, res.OrdersPointsList, res.RechangesPointsList, res.ProstoiPointsList);

                // Смотрим на тару, чтобы её исправить
                // Если мы переставляем рц (мод6), то ничего не чекаем, ибо есть опасность смены рц и обрушения на следующей итерации
                //if (!mode6 && taraCheckRestricts && (res.OverflowedTaraPointsList?.Count > 0 || res.ResultData.CatchedErrorsCount > 0))
                //{
                //    if (res.CatchedErrorsCount > 0)
                //    {
                //        var elements = res.CatchedErrors.Where(x => x.Value.Contains(CalculateItTempData.CatchedErrorType.TooMuchNaladLengths));
                //        if (elements.Any())
                //        {
                //            KeyValuePair<int, HashSet<CatchedErrorType>> some;
                //            ConcurrentDictionary<int, int> NaladErrorsCounter = (ConcurrentDictionary<int, int>)workflow.RawData[1];
                //            int count = -1;
                //            do
                //            {
                //                some = elements.ElementAt(r.Next(0, elements.Count()));
                //                //if (!opersCopy.Blocks[some.Value.Pos].Lists[0].FirstOper.OrigTara.IsEmpty)
                //                //{
                //                //    res.ResultData.CatchedErrors.Remove(some.Key);
                //                //    var t2 = res.ResultData;
                //                //    t2.CatchedErrorsCount--;
                //                //    res.ResultData = t2;
                //                //    continue;
                //                //}
                //                if (!NaladErrorsCounter.ContainsKey(some.Key)) NaladErrorsCounter.TryAdd(some.Key, 0);
                //                count = NaladErrorsCounter[some.Key];
                //                count++;
                //                if (count >= 10) count = -10;
                //                NaladErrorsCounter[some.Key] = count;
                //            }
                //            while (count < 0);
                //            WillThis = //some.Value.Pos;
                //            opersCopy.Blocks.FindIndex(x => x.Lists.Any(y => y.ModID == some.Key));
                //        }
                //        else
                //        {
                //            jstOverwflow();
                //        }
                //    }
                //    else
                //    {
                //        jstOverwflow();
                //    }

                //    void jstOverwflow()
                //    {
                //        var checkThis = r.Next(0, res.OverflowedTaraPointsList.Count);
                //        WillThis = res.OverflowedTaraPointsList.Count > 0 ? res.OverflowedTaraPointsList[checkThis] : -1;
                //    }

                //    //R6 = t.TaraErrorsCount < BestValue.TaraErrorsCount;
                //    onlyLeft = false;
                //    if (r.Next() > 0.25 && WillThis > -1)
                //    {
                //        var list = opersCopy.Blocks[WillThis].Lists[0];
                //        // В большинстве случаев операции предлагается менять тару.
                //        Operation op = list.FirstOper;
                //        float nalad = 0;
                //        if (list.ObshEtapData != null)
                //        {
                //            var firstHere = list.ObshEtapData.First();
                //            op = firstHere.Value.FirstOper; // ну возьмем допустим самую первую
                //        }
                //        var modHere = op.IsCustomMod ? op.CustomModelling : op.Modelling;
                //        if (modHere == Guid.Empty)
                //        {
                //            nalad = opersCopy.NaladLengthsData[op.ID].data.NaladLengthHere.Value + opersCopy.NaladLengthsData[op.ID].data.NaladLengthNexts.Value;
                //        }
                //        else
                //        {
                //            nalad = opersCopy.NaladLengthsDataAsMods[modHere].data.NaladLengthHere.Value + opersCopy.NaladLengthsDataAsMods[modHere].data.NaladLengthNexts.Value;
                //        }
                //        var operToChangeList = (list.ObshEtapData == null ? list.OpersList
                //            : (modHere == Guid.Empty ? (new List<Operation>() { op }) : (list.OpersList.Where(x => modHere == x.Modelling || modHere == x.CustomModelling)))).ToList();
                //        lock (locker)
                //        {
                //            var spo = new Merger.SumIsPossibleOptions(false)
                //            {
                //                NaladLength = nalad,
                //                PrevsInfo = operToChangeList[0].Prevs
                //                    .Select(x =>
                //                    {
                //                        var frst = opersCopy.Blocks.First(y => y.Lists.Any(l => l.OpersList.FindIndex(z => z.ID == x) > -1));
                //                        var lst = frst.Lists[0].OpersList[0];
                //                        return (x, lst.CurrentTara);
                //                    })
                //                    .ToDictionary(x => x.x, x => x.CurrentTara),
                //                    ExceptTheseTaras = /*res.ResultData.CatchedErrorsCount > 0 ? null : */new HashSet<Uco>()
                //                    {
                //                                operToChangeList[0].CurrentTara
                //                    },
                //                    RCMustBeThis = list.ObshEtapData != null ? opersCopy.Blocks[WillThis].ActualRC : null,
                //                    NextRCsMustBeThese = operToChangeList[0].Nexts.Count > 0 ? new HashSet<Uco>(
                //                        operToChangeList[0].Nexts
                //                        .Select(x =>
                //                        {
                //                            var block = opersCopy.Blocks.FirstOrDefault(y => y.Lists[0].OpersList.FindIndex(z => z.ID == x) > -1);
                //                            if (block != null) return (Uco?)(block.Lists[0].OpersList[0].Center);
                //                            return (Uco?)null;
                //                        }).Where(x => x.HasValue).Select(x => x.Value)) : null // Находим операцию
                //            };

                //            if (Merger.SumIsPossible(new PartiaSdvigSmall(operToChangeList),
                //            out var tara, out var rc, out var max,
                //            out _, out _,
                //            out _, spo))
                //            {
                //                if (isFirst)
                //                {
                //                    foreach (var op1 in operToChangeList)
                //                    {
                //                        op1.CurrentTara = tara;
                //                        op1.Center = rc;
                //                        op1.MaxNamotka = max;
                //                    }
                //                    opersCopy.Blocks[WillThis].ActualRC = rc; // Если тут одно моделирование, то рц все равно можно менять всем
                //                                                              //operToChangeList[0] = op;

                //                    WillThis = -2;
                //                    H--;
                //                    isFirst = false;
                //                    continue; // Мы же вынуждены поменять тару, поэтому смотрим, что получилось.
                //                }
                //            }
                //            else
                //            {
                //                if (res.CatchedErrorsCount > 0 && isFirst)
                //                {
                //                    H--;
                //                    isFirst = false;
                //                    continue; // Мы же вынуждены попробовать переставить это куда-то, чтобы изменить налад длину
                //                }
                //            }
                //        }
                //    }
                //}

                #region Финальное сравнение

                //if (LoadedData.LastHope > 0)
                //{
                //    var b3 = workflow.Generation - workflow.Best >= LoadedData.LastHope;
                //    nonStrictB |= b3;
                //    strictB |= b3;
                //}

                bool resToCheck;

                if (taraCheckRestricts)
                {
                    resToCheck =
                    (res.CatchedNaladLengthsErrorsCount < BestValue.CatchedNaladLengthsErrorsCount
                    ||
                    (res.CatchedNaladLengthsErrorsCount <= BestValue.CatchedNaladLengthsErrorsCount && nonStrictB && OtherCritRestrictions)
                    )
                    && R6; // в любом случае должно быть не больше выходов за пределы
                }
                else
                {
                    resToCheck = (nonStrictB && OtherCritRestrictions && res.CatchedNaladLengthsErrorsCount <= BestValue.CatchedNaladLengthsErrorsCount)/* || FixedNaladLengths*/;
                }

                if (resToCheck)
                {
                    lock (basisLink.LockerObject)
                    {
                        //if (!LoadedData.StrictCalcs)
                        //{
                        //    ops = opersCopy.Blocks;
                        //    workflow.BestValue = t;
                        //}
                        if (strictB)
                        {
                            //if (LoadedData.StrictCalcs)
                            //{
                                //ops = opersCopy.Blocks;
                                workflow.BestValue = t;
                            //}
                            basisLink.UpdateBcsSuccess(t, opersCopy, poolIndex);
                            //workflow.Best = workflow.Generation;
                            workflow.FoundForDynamic = true;
                        }
                        workflow.RawData[2] = true; // Мы в любом случае ставим ее равной true, чтобы счётчик шёл дальше
                        if (taraCheckRestricts && res.CatchedNaladLengthsErrorsCount == 0 && res.TaraErrorsCount == 0) // Но если выяснилось, что никаких ошибок нет, мы просто перестаём сразу же это делать
                            workflow.RawData[2] = false;
                    }
                    break;
                }

                #endregion
            }
        }

        bool ThereIsModInHere(PlanSimulatorMod y, Guid guid, out Stage firstEtap)
        {
            if (y.EasyLifeData != null)
            {
                if (y.EasyLifeData.TryGetValue(guid, out var data))
                {
                    firstEtap = data.FirstOper;
                    return true;
                }
                else
                {
                    firstEtap = null;
                    return false;
                }
            }
            else
            {
                if (y.FirstOperJust.IsCustomModelling ? (y.FirstOperJust.CustomModelling == guid) : (y.FirstOperJust.MesModelling.Code == guid))
                {
                    firstEtap = y.FirstOperJust;
                    return true;
                }
                else
                {
                    firstEtap = null;
                    return false;
                }
            }
        }

        public ConcurrentDictionary<Guid, ConcurrentDictionary<float, NaladLengthFixerClass>> IgnoredModellingsByFreeNaladLengths = new();

        //public bool FixSomeNaladLengths(OpersPool pool, PlanningCalculatorEngineResult resultToFill)
        //{
        //    if (resultToFill == null || resultToFill.CatchedNaladLengthsErrorsCount == 0) return false;

        //    bool ChangedSomething = false;

        //    return false;

        //    // Пройдемся по всем длинам, которые не влезли на тару
        //    foreach (var error in resultToFill.ModellingsWithBrokenFreeNaladLengths)
        //    {
        //        var guidToFind = error;
        //        var blockIndex = pool.Blocks.FindIndex(x => x.Lists.Any(y => ThereIsModInHere(y, guidToFind, out _))); // Найдем сначала номер блока где была ошибка
        //        var block = pool.Blocks[blockIndex];
        //        var modIndex = block.Lists.FindIndex(y => ThereIsModInHere(y, guidToFind, out _)); // Теперь найдём номер мода
        //        var mod = block.Lists[modIndex];

        //        // Получаем инфу по этапам - где лежит первый, в каком количестве, и является ли это просто модом
        //        (int FirstEtapIndex, int Count, bool ItsJustMod) = (mod.EasyLifeData != null && mod.EasyLifeData.TryGetValue(guidToFind, out var data)) ? (mod.OpersList.IndexOf(data.FirstOper), data.Count, false) : (0, mod.OpersList.Count, true);

        //        Stage op = mod.OpersList[FirstEtapIndex];

        //        // Если партия заблокирована, то мы ща будем просто смотреть на текущее количество мода, а не его сумму со следующим
        //        var PFPartiasData = basisLink.SessionLink.MasterDataLinks.OrdersMasterData.PFPartiasData;

        //        bool PartiaIsLocked = op.OrderLink != null
        //            && op.OrderLink.PartiaPF.HasValue
        //            && PFPartiasData.TryGetValue(op.OrderLink.PartiaPF.Value, out var partia)
        //            && partia.IsNotFree;

        //        var naladLengths = pool.NaladLengthsDataAsMods[guidToFind];
        //        var naladLengthCount = naladLengths.Data.NaladLengthHere.Value + (PartiaIsLocked ? 0 : naladLengths.Data.NaladLengthNexts.Value);
        //        naladLengthCount = MathF.Round(naladLengthCount, 3);

        //        // Создаём мод для поиска
        //        IAbleForTaraCalcInterface ops = ItsJustMod ? mod : new PartiaModelling(mod.OpersList.GetRange(FirstEtapIndex, Count).ToList(), op.CurrentTara, op.CurrentTaraMaxCapacity, op.EdIzm, op.Center, 0, null);

        //        var ALL = basisLink.SessionLink.MasterDataLinks.StagesMasterData.AllStages;

        //        var nextRCs = ops.AllOpers.SelectMany(x => x.Nexts).Select(x => ALL[x].Center).ToHashSet();// найти след рц
        //        var prevsInfo = ops.AllOpers.SelectMany(x => x.Prevs).Where(x => !ALL[x].IsVspom && !ALL[x].CurrentTara.IsEmpty).Distinct().ToDictionary(x => x, x => ALL[x].CurrentTara);
        //        if (TaraSearchEngine.SumIsPossible(ops, out var tara, out var rc, out var max, out var format, out _, out _, new SumIsPossibleOptions(false)
        //        {
        //            NextRCsMustBeThese = nextRCs,
        //            PrevsInfo = prevsInfo,// найти предыдущие этапы
        //            NaladLength = naladLengthCount,
        //            LogSomeErrors = false,
        //            ModToTaraInfoLink = basisLink.ModToTaraInfo
        //        }))
        //        {
        //            // Всё нашёл, задаем тару
        //            block.ActualRC = rc;
        //            basisLink.UpdateOpsOnOneRC(block, pool);
        //            for (int i = FirstEtapIndex; i < FirstEtapIndex + Count; i++)
        //            {
        //                mod.OpersList[i].Center = rc;
        //                mod.OpersList[i].CurrentTara = tara;
        //                mod.OpersList[i].CurrentTaraMaxCapacity = max;
        //            }
        //            ChangedSomething = true;
        //            resultToFill.ModellingsWithBrokenFreeNaladLengths.Remove(guidToFind);
        //        }
        //        else
        //        {
        //            // Нет? Попробуем найти здесь самую максимальную тару
        //            if (TaraSearchEngine.SumIsPossible(ops, out var taraName, out _, out var maxHere, out _, out _, out _, new SumIsPossibleOptions(false)
        //            {
        //                NextRCsMustBeThese = nextRCs,
        //                PrevsInfo = prevsInfo,
        //                SearchForMaxTara = true,
        //                LogSomeErrors = false,
        //                ModToTaraInfoLink = basisLink.ModToTaraInfo
        //            }) && maxHere > op.CurrentTaraMaxCapacity)
        //            {
        //                // Да, тара найдена
        //                var maxPossibleNaladLengths = maxHere - ops.Sum;
        //                IgnoredModellingsByFreeNaladLengths.TryAdd(guidToFind, new());
        //                IgnoredModellingsByFreeNaladLengths[guidToFind].TryAdd(naladLengthCount, new NaladLengthFixerClass() { ops = ops, type = 2, tara = taraName, max = maxHere });
        //            }
        //            else
        //            {
        //                // Нет? А если без ограничений? Это неверно, но хоть что-то понятнее станет
        //                if (TaraSearchEngine.SumIsPossible(ops, out var taraName2, out _, out var maxHere2, out _, out _, out _, new SumIsPossibleOptions(false)
        //                {
        //                    NaladLength = naladLengthCount,
        //                    LogSomeErrors = false,
        //                    ModToTaraInfoLink = basisLink.ModToTaraInfo
        //                }))
        //                {
        //                    IgnoredModellingsByFreeNaladLengths.TryAdd(guidToFind, new());
        //                    IgnoredModellingsByFreeNaladLengths[guidToFind].TryAdd(naladLengthCount, new NaladLengthFixerClass() { ops = ops, type = 0, tara = taraName2, max = maxHere2 });
        //                    ;
        //                }
        //                else
        //                {
        //                    IgnoredModellingsByFreeNaladLengths.TryAdd(guidToFind, new());
        //                    IgnoredModellingsByFreeNaladLengths[guidToFind].TryAdd(naladLengthCount, new NaladLengthFixerClass() { ops = ops, type = 1 });
        //                    ;
        //                }

        //            }


        //        }
        //    }

        //    return ChangedSomething;
        //}
    }
}

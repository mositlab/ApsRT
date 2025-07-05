using ApsRT.Core.NaladLengths;
using ApsRT.Core.Planning.ParallelPlanning;
using ApsRT.Core.Planning.PlanningClasses;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Rechanges.RechangesEngine;
using ApsRT.Core.Rechanges;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using ApsRT.Core.Planning.PlanningClasses.PoolClasses;
using ApsRT.Core.Centers.MasterData;
using System.Collections.Frozen;
using ApsRT.Core.Taras.TaraSearchEngine;

namespace ApsRT.Core.Planning.PlanningAlgorithm
{
    public class AlgorithmV
    {
        public PlanningAlgorithm basisLink;

        public AlgorithmV(PlanningAlgorithm basisLink)
        {
            this.basisLink = basisLink;
        }

        public void Start(PlanningAlgorithmWorkflow workflow)
        {

            double[] Ks = basisLink.SessionLink.SettingsController.CurrentSettings.VKoeffs;

            var NaladLengthsMasterData = basisLink.SessionLink.MasterDataLinks.NaladLengthsMasterData;
            var ParallelPlanningMasterData = basisLink.SessionLink.MasterDataLinks.ParallelPlanningMasterData;

            // Выбираем те операции, у которых нет предков. Ну или последнюю, которую можно юзать
            int[] possibleFirstIndexes;

            // Сет операций, которые ВПЗ, и которые надо выполнить ПОСЛЕ фиксированных, и ДО остальных
            //HashSet<int> VPZOpersOpenedList = basisLink.BestBlocksList.Where(x => x.Lists[0].OpersList[0].VPZ).Select(x => x.Lists[0].OpersList[0].ID).ToHashSet();

            // Не имеет значения, по какому индексу моделирования искать операцию. Этот массив нужен только для последующего поиска и вставки.
            if (basisLink.GLOBAL_FIXED_MIN == 0)
            {
                possibleFirstIndexes = basisLink.BestBlocksList.Where(x => x.Lists[0].OpersList.All(y => y.Prevs.Count == 0)).Select((x, index) => x.Lists[0].OpersList[0].Index).ToArray();
            }
            else
            {
                possibleFirstIndexes = new int[1] { basisLink.BestBlocksList.First(x => x.Lists.All(y => y.PrevMods.Count == 0)).Lists[0].OpersList[0].Index }; // Ну она же всё равно первая
            }

            var copy0 = OpersPool.CopyBlocks(basisLink.BestBlocksList, true);
            var preparedData = new OpersPoolConstructor(copy0, basisLink.MinAcceptableDate);
            var pool = new OpersPool(basisLink.SessionLink, copy0, preparedData, basisLink.MinAcceptableDate);
            var OpersCopy1 = pool.Blocks; // Копируем сие дело

            var centers_choosen = pool.CentersData; // Каждый раз копию делать не нужно, потому что считаются все итерации друг за другом


            Console.WriteLine("V, beg, " + OpersCopy1.SelectMany(x => x.Lists.SelectMany(y => y.OpersList.Select(z => z.Index))).Count());

            Console.WriteLine("V koeffs:");
            for (int i = 0; i < Ks.Length; i++)
            {
                Console.WriteLine($"[{i}]: {Ks[i]}");
            }

            var globalres = TimeSpan.MaxValue;
            for (var i = 0; i < (1) || globalres == TimeSpan.MaxValue; i++)
            {
                if (i == possibleFirstIndexes.Length)
                {
                    break;
                }

                if (basisLink.WannaStop)
                {
                    break;
                }

                //workflow.ProgressChanged(0);

                long TotalRechanges = 0; // Сумма вообще всех переналадок
                long TotalProstoi = 0; // Сумма вообще всех простоев

                // Нужно делать копию каждый раз
                //Dictionary<int, (int Row, Center Struct)> CentersDic = new(centers_choosen);

                // Это - проверка, оценка, стоит ли прям щас поставить какой-то обед или ремонт
                //var keys = centers_choosen.Keys;
                //foreach (var key in keys)
                //{
                //    var val = centers_choosen[el.Key];
                //    if (val.Struct.NextFixingIndex.Item2)
                //    {
                //        val.Struct.LastDate += (CentersFixingTime[el.Key][val.Struct.NextFixingIndex.Item1].When - MIN_DATE_TIME);
                //        val.Struct.NextFixingIndex.Item1++;
                //        val.Struct.NextFixingIndex.Item2 = false;
                //        if (val.Struct.NextFixingIndex.Item1 == CentersFixingTime[el.Key].Count) val.Struct.NextFixingIndex.Item1 = -1;
                //        centers_choosen[el.Key] = val;
                //    }
                //}

                // Создаём локальную копию
                var OpersCopy = OpersPool.CopyBlocks(OpersCopy1, true);

                // Так как у нас есть пул для работы, вычищаем из него всё
                pool.ClearTaraTimeLineDics();
                pool.NaladLengthsDataRecreate(preparedData);

                // Постепенно заполняем текущую 
                var ProstoiTimes = new Dictionary<int, DateTime>(basisLink.BestBlocksList.Count);
                var DoneModellings = new HashSet<int>();
                var OpersNow = new List<OpersBlock>(basisLink.BestBlocksList.Count);

                // Словарик для хранения выбранных тар для правильного алгоритма
                var PrevsInfoForV = new Dictionary<int, Uco>();

                var OneRCGroupsForV = basisLink.OnOneRCGroupData.ToDictionary(x => x.Key, x => (Uco?)null);

                // Начав, естественно, с самой первой.
                var fInd = OpersCopy.FindIndex(x => x.Lists[0].OpersList.Any(y => y.Index == possibleFirstIndexes[i]));
                var nalads = NaladLengthsMasterData.GetNaladLengthsForCurrentStageOnly(null, OpersCopy[fInd].Lists[0].FirstOper, OpersCopy[fInd].ActualRC, null);
                AddOperHere(fInd, AsParallel: false, 0, 0, nalads, centers_choosen, (OpersCopy[fInd].Lists[0].FirstOper.CurrentTara, OpersCopy[fInd].Lists[0].FirstOper.CurrentTaraMaxCapacity)/*new(Guid.Empty, "")*/, false, pool, OpersCopy, ProstoiTimes, DoneModellings, OpersNow, PrevsInfoForV);
                
                if (OpersCopy[fInd].Lists[0].FirstOper.OnOneRC && basisLink.ModIDToOnOneRCGroupID.TryGetValue(OpersCopy[fInd].Lists[0].ModID, out var groupID))
                {
                    OneRCGroupsForV.Add(groupID, OpersCopy[fInd].ActualRC);
                }

                var isbad = false;

                // Цикл проходится
                while (OpersCopy.Count > 0)
                {
                    if (basisLink.WannaStop)
                    {
                        isbad = true;
                        break;
                    }
                    var best = -1;
                    var bestV = double.MaxValue;
                    var bestErr = int.MaxValue;
                    var locker = new object();
                    long timeR = 0;
                    long timeP = 0;
                    GottenNaladLengthStruct bestNalads = new GottenNaladLengthStruct(new(0, new(Guid.Empty, DimensionUnit.m)), new(0, new(Guid.Empty, DimensionUnit.m)), new(0, new(Guid.Empty, DimensionUnit.m)), new(0, new(Guid.Empty, DimensionUnit.m)), false);
                    bool asParallel = false;
                    int bestParID = 0;
                    Uco bestC = new(Guid.Empty, "");
                    (Uco tara, double max) bestTara = (new(Guid.Empty, ""), 0);
                    bool bestTaraIsLast = false;

                    // Проходимся по всем операциям, что есть сейчас в списке
                    if (OpersNow.Count < basisLink.GLOBAL_FIXED_MIN || OpersCopy[0].Lists[0].FirstOper.PrevDateTime.HasValue)
                    {
                        // Сейчас у нас - фиксированные
                        checkThisOp(0);
                        //break;
                    }
                    else
                    {
                        if (OpersNow.Count < basisLink.GLOBAL_VPZ_MAX)
                        {
                            // Изучаем лишь те, что в опред. промежутке (ВПЗ)
                            var resu1 = Parallel.For(0, OpersCopy.Count/*Planning.GLOBAL_VPZ_MAX - OpersNow.Count - 1*/, n =>
                            {
                                if (OpersCopy[n].Lists.Any(x => x.OpersList.Any(y => y.VPZ || y.IsFact)))
                                    checkThisOp(n);
                            });
                            while (!resu1.IsCompleted)
                            {
                                ;
                            }
                        }
                        else
                        {
                            // Это обычные операции
                            var resu1 = Parallel.For(0, OpersCopy.Count, n =>
                            //for (int n = 0; n < OpersCopy.Count; n++)
                            {
                                checkThisOp(n);
                            }

                            );
                            while (!resu1.IsCompleted)
                            {
                                ;
                            }
                        }
                    }

                    bool checkThisOp(int j)
                    {
                        var bad = false;
                        // Сначала проверяем: можно ли эту операцию поставить?
                        if (OpersNow.Count >= basisLink.GLOBAL_FIXED_MIN) // но это при условии, что она уже не фиксированная
                        {
                            for (var jjjj = 0; jjjj < OpersCopy[j].Lists[0].OpersList.Count; jjjj++)
                            {
                                var indexes = OpersCopy[j].Lists[0].PrevMods;//.OpersList[jjjj].Prevs;
                                var allkids_found = indexes.All(x => DoneModellings.Contains(x));
                                if (indexes.Count > 0 && !allkids_found)
                                {
                                    //var ModsPrev = indexes.Select(x => OpersCopy.Find(y => y.Lists.Any(z => z.ModID == x)) ?? OpersNow.Find(y => y.Lists.Any(z => z.ModID == x))).Select(x => (x, "F:" + x.Lists.Any(y => y.OpersList.Any(z => z.IsFact)) + "N:" + x.Lists.Any(y => y.OpersList.Any(z => z.Fixed)) + "V:" + x.Lists.Any(y => y.OpersList.Any(z => z.VPZ)))).ToList();
                                    bad = true;
                                    break;
                                }
                            }
                        }

                        if (bad)
                        {
                            return false;
                        }

                        var list = OpersCopy[j].Lists[0].PossibleRCsHere;//.OpersList[0].PossibleCenters;
                        var alt = (list != null) && false;

                        //var taraList = Tara.GetOperTara(OpersCopy[j].Lists[0].FirstOper, false);


                        TemporaryTaraSearchData data = null;
                        bool Can1 = TaraSearchEngine.GetAllVariantsForThese(OpersCopy[j].Lists[0], out data, new SumIsPossibleOptions(false) { PrevsInfo = PrevsInfoForV, ModToTaraInfoLink = basisLink.ModToTaraInfo });

                        //bool Can = Merger.SumIsPossible(OpersCopy[j].Lists[0].OpersList,
                        //    out _, out _, out _, out _, out _, false, true,
                        //    null, // ПРЕД.
                        //    null,
                        //    null
                        //    );

                        for (var jj = 0; jj < (alt ? list.Count : 1); jj++)
                        {
                            var choosencent = alt ? list[jj] : OpersCopy[j].Lists[0].FirstOper.Center; // Выбираем мы значит РЦ
                            var lastBlock = centers_choosen[choosencent].LastBlock; // чекам посл блок

                            // ВНИМАНИЕ.
                            // lastBlock вообще-то нужно объявлять не тут - а ниже, где перечисляются РЦ.
                            // Но по каким-то причинам (пока не понял, каким), если перенести блок туда, альтернативные РЦ перестают помечаться.
                            // + план ищется хуже (результаты не такие красивые)
                            // Скорее всего, это надо будет пофиксить, но пока смотрится параллельный отжиг, алгоритм остаётся таким.


                            // А что если можно с параллелью?
                            // Он должен быть задан, и быть не заполнен до конца, чтобы в этом был какой-то смысл, + наш РЦ должен поддерживать параллелизм
                            //var myParId = OpersCopy[j].Lists[0].FirstOper.ParallelPlanningID;

                            bool canChangeTara = true; // Ну, изначально тару менять можно

                            if (!OpersCopy[j].Lists[0].FirstOper.EndTara.IsEmpty)
                            {
                                // Если задана конечная тара изначально, то менять тару ЗАПРЕЩАЕМ.
                                canChangeTara &= false;
                            }

                            bool b2 = false;

                            if (OpersCopy[j].Lists[0].FirstOper.ParallelPlanningIDs.Any(x => x.Value != -1) && OpersCopy[j].Lists[0].FirstOper.ParallelPlanningIDs.TryGetValue(choosencent, out var myParId)
                                && ParallelPlanningMasterData.ParIDParams.TryGetValue(myParId, out var paramsHere)) // Может быть параллельным? И есть подходящий рц?
                            {

                                //var paramsHere = ParallelPlanningHelper.ParIDParams[myParId];
                                canChangeTara &= !paramsHere.DoNotIncreaseTara; // тут же НЕЛЬЗЯ тару менять

                                if (lastBlock != null)
                                {
                                    bool MyFactVPZ = OpersCopy[j].Lists[0].OpersList.Any(x => x.IsFact || x.VPZ);
                                    DateTime? date1 = OpersCopy[j].Lists[0].FirstOper.StartTimeFact;
                                    bool MyFix = OpersCopy[j].Lists[0].OpersList.Any(x => x.Fixed);
                                    bool LastFactVPZ = lastBlock.Lists[0].OpersList.Any(x => x.IsFact || x.VPZ);
                                    DateTime? date2 = lastBlock.Lists[0].FirstOper.StartTimeFact;
                                    bool LastFix = lastBlock.Lists[0].OpersList.Any(x => x.Fixed);
                                    if (MyFactVPZ == LastFactVPZ && MyFix == LastFix && date1 == date2 &&

                                        lastBlock.FillKoeff > -1 && lastBlock.FillKoeff < 1 && lastBlock.ExistingTaraHere.Count > 0
                                        && lastBlock.Lists[0].FirstOper.ParallelPlanningIDs.Any(x => x.Value != -1) // Предыдущий тоже может быть паралелльным?
                                        && lastBlock.Lists[0].FirstOper.ParallelPlanningIDs.TryGetValue(choosencent, out var toChekID) && toChekID == myParId
                                        && lastBlock.ActualRC == choosencent
                                        )
                                    {
                                        var seconds1 = lastBlock.Lists.Min(x => x.AllTime[choosencent]).TotalSeconds; // получаем МИНИМАЛЬНУЮ длительность моделирования
                                        var secondsWant = OpersCopy[j].Lists[0].AllTime[choosencent].TotalSeconds; // получаем длительность блока, которую хотим вставить
                                        var delta = paramsHere.Delta; // обращаемся к максимально допустимой дельте
                                        if (Math.Abs(secondsWant - seconds1) <= delta)
                                        {
                                            b2 = CheckIt(true, canChangeTara);
                                        }
                                    }
                                }
                            }

                            // чекаем без параллели
                            if (!b2) CheckIt(false, canChangeTara);

                            bool CheckIt(bool Par, bool CanChangeTara)
                            {
                                bool b1 = false;
                                var AnyCan = false;
                                if (Can1/* && CanChangeTara*/)
                                {
                                    int groupID1 = -1;
                                    List<Uco> allowedRC = null;
                                    if (OpersCopy[j].Lists[0].FirstOper.OnOneRC
                                            &&
                                            basisLink.ModIDToOnOneRCGroupID.TryGetValue(OpersCopy[j].Lists[0].ModID, out groupID1)
                                            )
                                    {
                                        allowedRC = basisLink.OnOneRCGroupData[groupID1].PossibleRCs;
                                    }
                                    else
                                        groupID1 = -1;

                                    // Перебираем тары
                                    for (int h = 0; h < data.PossibleCentersAndTaras.Length; h++) // ПО ИДЕЕ этот список - идейный продолжатель списка "PossibleCenters", поэтому в V этот список не нужен.
                                    {
                                        var tara = data.PossibleCentersAndTaras[h];
                                        if (Par && centers_choosen[tara.RC].LastBlock == null) continue;
                                        if (!canChangeTara && tara.Tara != OpersCopy[j].Lists[0].FirstOper.EndTara) continue; // Если тару нельзя менять, то в случае несовпадения этот вариант исключается.

                                        //if (OpersCopy[j].Lists[0].FirstOper.RecommendedRC.HasValue && OpersCopy[j].Lists[0].FirstOper.RecommendedRC.Value != tara.RC) continue;//resF.res -= 1000000; // Если уже есть выбранный РЦ, то забиваем болт на все остальные
                                        if (allowedRC != null && !allowedRC.Contains(tara.RC)) continue; // Если в списке разрещенных РЦ нет этого, то дальше не идём

                                        if (groupID1 != -1 && OneRCGroupsForV[groupID1] != null && tara.RC != OneRCGroupsForV[groupID1]) continue; // Если я уже ставил РЦ в эту группу, то другие нельзя

                                        var Can2 = TaraSearchEngine.FilterAllVariants(OpersCopy[j].Lists[0], data, out _, out _, out var GOTmax, out _, out _,
                                            new SumIsPossibleOptions(false)
                                            {
                                                PrevsInfo = PrevsInfoForV,// ПРЕД.
                                                TaraMustBeThis = tara.Tara,// огр. по таре
                                                RCMustBeThis = tara.RC,// огр. по РЦ
                                                LogSomeErrors = false,
                                                ModToTaraInfoLink = basisLink.ModToTaraInfo
                                            });
                                        AnyCan |= Can2;
                                        if (Can2)
                                        {
                                            b1 |= localFChecking(lastBlock, Par, tara.RC, (tara.Tara, GOTmax)); // Фигачим с выбором тары
                                            //if (OpersCopy[j].Lists[0].FirstOper.RecommendedRC.HasValue && OpersCopy[j].Lists[0].FirstOper.RecommendedRC.Value == tara.RC) break; // Если уже есть выбранный РЦ, то забиваем болт на все остальные
                                        }
                                    }
                                }
                                if (!CanChangeTara || !Can1 || !AnyCan) // Если ну вот вообще ничего не может,
                                {
                                    //localFChecking(lastBlock, Par, choosencent, new()); // Выбора нет, делаем без тары 
                                    b1 |= localFChecking(lastBlock, Par, choosencent, (OpersCopy[j].Lists[0].FirstOper.CurrentTara, OpersCopy[j].Lists[0].FirstOper.CurrentTaraMaxCapacity)); // Выбора нет, делаем с чем есть
                                }
                                return b1;
                            }

                            break;
                        }



                        bool localFChecking(OpersBlock lastBlock, bool isPar, Uco choosencent, (Uco choosenTara, double CurrentTaraMaxCapacity) tara)
                        {
                            bool bestTaraIsLastLocal = false;
                            int parID = 0;
                            if (isPar)
                            {
                                if (tara.choosenTara.IsEmpty) // Собрался в параллель пустую тару ставить???
                                {
                                    return false; // Так нельзя!
                                }
                                parID = ParallelPlanningMasterData.GetParPlanID(choosencent, OpersCopy[j].Lists[0].FirstOper.OperationVid, OpersCopy[j].Lists[0].FirstOper);
                                var result = ParallelPlanningMasterData.GetMostFreeTara(
                                    choosencent,
                                    OpersCopy[j].Lists[0].FirstOper.OperationVid,
                                    parID,
                                    OpersCopy[j].Lists.Count, //TODO: у общих этапов их количество может быть больше
                                    lastBlock.ExistingTaraHere,
                                    tara.choosenTara,
                                    out var resultat
                                    );
                                if (result == -1)
                                {
                                    return false; // Так НЕЛЬЗЯ.
                                }
                                if (result == 0)
                                {
                                    // Если вернул 0, значит влазит всё, НО - под завязку, больше ничего не влезет, поэтому 
                                    bestTaraIsLastLocal = true;
                                }
                            }

                            var resF = F(pool, lastBlock, centers_choosen[choosencent].LastMod, OpersCopy[j].Lists[0], choosencent, tara.choosenTara, tara.CurrentTaraMaxCapacity, isPar,
                                out var gottenRech, out var gottenprost, out var gottenNalads, Ks, centers_choosen, TotalRechanges, TotalProstoi, ProstoiTimes);//, ref centA.Struct, centA.RechangeTime

                            if (OpersCopy[j].Lists[0].FirstOper.RecommendedRC.HasValue && OpersCopy[j].Lists[0].FirstOper.RecommendedRC.Value != choosencent) resF.res -= 1000000;

                            lock (locker)
                            {
                                if (resF.errorsRate <= bestErr && resF.res < bestV)
                                {
                                    bestV = resF.res;
                                    bestErr = resF.errorsRate;
                                    best = j;
                                    timeR = gottenRech;
                                    timeP = gottenprost;
                                    bestC = choosencent;
                                    asParallel = isPar;
                                    bestParID = parID;
                                    bestTara = tara;
                                    bestTaraIsLast = bestTaraIsLastLocal;
                                    bestNalads = gottenNalads;
                                    return true;
                                }
                            }
                            return false;
                        }

                        return true;
                    }

                    // Лучшую операцию, если она есть, пихаем
                    if (best == -1)
                    {
                        basisLink.CurrentStage = PlanningAlgorithmStage.FailedV;
                        basisLink.CurrentStageChanged?.Invoke(basisLink.CurrentStage, OpersCopy.Count);
                        isbad = true;
                        break;
                    }

                    // Выбранную операцию вставляем, добиваем доп. данные
                    OpersCopy[best].ActualRC = bestC;
                    TotalRechanges += timeR; // Добавляем общих переналадок
                    TotalProstoi += timeP; // Добавляем общих простоев

                    OneRCGroupsForV.Add(OpersCopy[best].Lists[0].ModID, OpersCopy[best].ActualRC);

                    if (OpersCopy[best].Lists[0].FirstOper.OnOneRC
                        && basisLink.ModIDToOnOneRCGroupID.TryGetValue(OpersCopy[best].Lists[0].ModID, out var groupID1))
                    {
                        OneRCGroupsForV.TryAdd(groupID1, OpersCopy[best].ActualRC);
                    }

                    AddOperHere(best, asParallel, timeP, timeR, bestNalads, centers_choosen, bestTara, bestTaraIsLast, pool, OpersCopy, ProstoiTimes, DoneModellings, OpersNow, PrevsInfoForV);

                    //workflow.ProgressChanged(OpersNow.Count); // Провоцируем перерисовку

                    basisLink.AlgorithmCallback?.Invoke(basisLink.CurrentStage, OpersNow.Count);

                }
                if (isbad)
                {
                    //This.OnCalcDebugger.Text += $"не дошёл, лишь {OpersCopy.Count}\r\n\r\n";
                    //i--;
                    //if (LoadedData.OnlyOneVRun)
                    //{
                        break;
                    //}

                    //continue; // Если там никакая операция не подошла, то он ничего не вычисляет, а просто переходит к следующему циклу
                }
                Console.WriteLine($"V, step {i}, " + OpersNow.SelectMany(x => x.Lists.SelectMany(y => y.OpersList.Select(z => z.Index))).Distinct().Count());
                var pool0 = new OpersPool(basisLink.SessionLink, OpersNow, preparedData, basisLink.MinAcceptableDate);
                var t = PlanningCalculatorEngine.PlanningCalculatorEngine.CalculatePlan(pool0, basisLink.MinAcceptableDate, null);
                //This.SomeResultsTextBox.Text += $"Дошёл, {TimeSpan.FromSeconds(t.RechangesSeconds)}\r\n";
                if (true/*globalres > t.RechangesSeconds*/) // тут не только переналадки вообще-то
                {
                    workflow.BestValue = t;
                    //workflow.LastRechangesForCombo = t.RechangesSeconds;
                    //workflow.LastProductionForCombo = t.GlobalPlanEndDate - basisLink.MinAcceptableDate;
                    //workflow.LastProstoiForCombo = t.ProstoiSeconds.TotalMinutes;
                    //workflow.LastDaysForCombo = t.DaysRuinedValue;
                    //workflow.LastBadOffsetsForCombo = t.BadOffsets;
                    basisLink.UpdateBcsSuccess(t, pool0.Blocks);
                    globalres = t.RechangesSeconds;
                }
                //else
                //{
                //    if (!IsSilent && !This.GraphsRBNever.Checked)
                //    {
                //        GraphsController.AddNewFailedPoint();
                //    }
                //}

                workflow.Generation++;
            }
            //MessageBox.Show(TimeSpan.FromSeconds(globalres).ToString());

        }

        /// <summary>
        /// Расчёт оптимальности плана при вставке моделирования на линию
        /// </summary>
        /// <param name="Index">Индекс моделирования</param>
        /// <param name="AsParallel">Рассматривать моделирование как параллельное</param>
        /// <param name="prostoiTime"></param>
        /// <param name="rechTime"></param>
        /// <param name="centers_choosen"></param>
        /// <param name="tara"></param>
        /// <param name="pool"></param>
        /// <param name="OpersCopy"></param>
        /// <param name="ProstoiTimes"></param>
        /// <param name="DoneModellings"></param>
        /// <param name="OpersNow"></param>
        public void AddOperHere(
            int Index,
            bool AsParallel,
            long prostoiTime,
            long rechTime,
            GottenNaladLengthStruct gottenNaladLength,
            FrozenDictionary<Uco, CenterInPlanData>? centers_choosen,
            (Uco tara, double max) tara,
            bool ThisTaraMustBeLast,
            OpersPool pool,
            List<OpersBlock>? OpersCopy,
            Dictionary<int, DateTime>? ProstoiTimes,
            HashSet<int>? DoneModellings,
            List<OpersBlock>? OpersNow,
            Dictionary<int, Uco> PrevsInfoForV)
        {
            var op00 = OpersCopy[Index]; // получаем блок
            OpersCopy.RemoveAt(Index); // удаляем его из мешка с блоками
            var cent2 = centers_choosen[op00.ActualRC]; // получаем инфу об рц


            var firstMod = op00.Lists[0];
            var first = firstMod.FirstOper;
            var lastOp = cent2.LastMod?.FirstOper;
            cent2.LastMod = firstMod; // в любом случае назначаем мод последний

            foreach (var op in firstMod.OpersList)
            {
                op.CurrentTara = tara.tara; // Задаём тару и тут тоже собственно.
                op.CurrentTaraMaxCapacity = tara.max; // Задаём тару и тут тоже собственно.
                op.Center = op00.ActualRC;
            }


            if (AsParallel)
            {
                // Сдвигаем показатели, чтобы потом было яснее
                if (firstMod.OpersList.Any(x => x.Fixed))
                {
                    basisLink.GLOBAL_FIXED_MIN--;
                    basisLink.GLOBAL_VPZ_MAX--;
                }
                else
                {
                    if (firstMod.OpersList.Any(x => x.IsFact || x.VPZ))
                    {
                        basisLink.GLOBAL_VPZ_MAX--;
                    }
                }

                var lastBlock = cent2.LastBlock;//OpersNow[^1];
                lastBlock.Lists.Add(firstMod); // Добавляем мод
                //lastBlock.FillKoeff += 0.25f; // TODO: другое значение вставить, мб передать в виде аргумента
                if (ThisTaraMustBeLast) lastBlock.FillKoeff = 1;
                if (!lastBlock.ExistingTaraHere.ContainsKey(tara.tara))
                {
                    lastBlock.ExistingTaraHere.Add(tara.tara, 0);
                }

                lastBlock.ExistingTaraHere[tara.tara]++;
            }
            else
            {
                OpersNow.Add(op00); // Добавляем последовательно
                cent2.ProstoiTime += TimeSpan.FromSeconds(prostoiTime);
                cent2.RechangesTime += TimeSpan.FromSeconds(rechTime);
                cent2.OpersTime += firstMod.AllTime[op00.ActualRC];

                cent2.LastBlock = op00; // назначаем блок
                cent2.LastStartDate = cent2.LastDate.AddSeconds(prostoiTime + rechTime); // Добавляем к дате начала простои и переналадки
                // TODO: тут же ещё фиксированные даты есть, их тоже добавить надо

                //cent2.LastStartDate = cent2.LastDate; // сосбственно меняем дату начала на последнюю дату загрузки
                //pool.AddInfoTaraTimeLine(firstMod, cent2.LastStartDate); // добавляем инфу про тары, юзая последнюю дату рц, ПОСЛЕ сдвига LastStartDate

                //cent2.LastDate = cent2.LastStartDate.AddSeconds(first.Time); // это - дата окончания блока, ПОСЛЕ сдвига LastStartDate

            }

            pool.AddInfoTaraTimeLine(firstMod, op00.ActualRC, cent2.LastStartDate, $"{first.GUID} с {op00.ActualRC.Label}"); // добавляем инфу про тары, юзая последнюю дату начала рц

            var last = cent2.LastStartDate.AddSeconds(first.GetDuration(op00.ActualRC)/*firstMod.AllTime[op00.ActualRC]*/); // это - дата окончания блока
            cent2.LastDate = !AsParallel || last > cent2.LastDate ? last : cent2.LastDate; // последняя дата начала становится максимальной из бывших там (либо если последовательно, либо если дата дольше последнего)

            var CentersMasterData = first.SessionLink.MasterDataLinks.CentersMasterData;
            var AdditionalConditionsMasterData = first.SessionLink.MasterDataLinks.AdditionalConditionsMasterData;
            var ALL = first.SessionLink.MasterDataLinks.StagesMasterData.AllStages;

            //var op = ooop.Oper;
            if (CentersMasterData.CentersData.TryGetValue(op00.ActualRC, out var clas) && clas.HasAdditionalActions)
            {
                if (lastOp != null) cent2.AdditinalConditions = AdditionalConditionsMasterData.UpdateAdditionalParamsOnCenter(new RechangesEngineData(cent2, new(DateTime.MinValue, -1, -1, -1), lastOp, first, op00.ActualRC));
                //MOSITLAB.AfterOpChangesEvent(op00.ActualRC, cent2, first); // Ну потому что нам нужны все эти вещи
            }

            DoneModellings.Add(firstMod.ModID);
            foreach (var ooop in firstMod.OpersList)
            {
                PrevsInfoForV.Add(ooop.Index, tara.tara);
                foreach (var next in ooop.Nexts)
                {
                    if (ProstoiTimes.ContainsKey(next))
                    {
                        if (cent2.LastDate > ProstoiTimes[next])
                        {
                            ProstoiTimes[next] = cent2.LastDate;
                        }
                    }
                    else
                    {
                        ProstoiTimes.Add(next, cent2.LastDate);
                    }
                }
            }

            // Добавляем наладочных длин
            naladLengthsSet(firstMod.OpersList[0].Index, gottenNaladLength.NaladLengthNexts, true, gottenNaladLength.NaladLengthFirst, gottenNaladLength.NaladLengthOther, gottenNaladLength.IsFirst);

            void naladLengthsSet(int etapID, NaladLengthValue sum, bool firstInvoke, NaladLengthValue firstLength, NaladLengthValue otherLength, bool firstLaunch)
            {
                var info = pool.NaladLengthsData[etapID];

                if (firstInvoke)
                {
                    info.data.NaladLengthFirst = firstLength;
                    info.data.NaladLengthOther = otherLength;
                    info.data.IsFirst = firstLaunch;
                }
                info.data.NaladLengthNexts += sum; // TODO - ПЕРЕПИЛИТЬ!!!

                //pool.NaladLengthsData[etapID] = info;
                foreach (var prev in ALL[etapID].Prevs)
                {
                    naladLengthsSet(prev, sum, false, new NaladLengthValue(0, new (Guid.Empty, DimensionUnit.m)), new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m)), false);
                }
            }

        }

        /// <summary>
        /// Сравнение двух моделирований и возврат коэффициента похожести
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="lastBlock"></param>
        /// <param name="a">Моделирование А</param>
        /// <param name="b">Моделирование Б</param>
        /// <param name="chosenRC"></param>
        /// <param name="tara"></param>
        /// <param name="asParallel">Рассматривать моделирования для параллельного выполнения</param>
        /// <param name="timeR"></param>
        /// <param name="timeP"></param>
        /// <param name="Ks">Коэффициенты V</param>
        /// <param name="centers_choosen"></param>
        /// <param name="TotalRechanges"></param>
        /// <param name="TotalProstoi"></param>
        /// <param name="ProstoiTimes"></param>
        /// <returns></returns>
        public (double res, int errorsRate) F(
            OpersPool pool,
            OpersBlock lastBlock,
            PlanningModelling a,
            PlanningModelling b,
            Uco chosenRC,
            Uco tara,
            double maxTaraNamot,
            bool asParallel,
            out long timeR,
            out long timeP,
            out GottenNaladLengthStruct naladsOut,
            double[] Ks,
            FrozenDictionary<Uco, CenterInPlanData>? centers_choosen,
            long TotalRechanges,
            long TotalProstoi,
            Dictionary<int, DateTime>? ProstoiTimes)
        {
            var masterDatas = b.FirstOper.SessionLink.MasterDataLinks;
            var NaladLengthsMasterData = masterDatas.NaladLengthsMasterData;
            var CentersMasterData = masterDatas.CentersMasterData;
            var RechangesMasterData = masterDatas.RechangesMasterData;
            var AdditionalConditionsMasterData = masterDatas.AdditionalConditionsMasterData;
            var MIN_DATE_TIME_AFTER_FIX = basisLink.MinAcceptableDate;
            var ALL = masterDatas.StagesMasterData.AllStages;
            var CentersFixingTime = masterDatas.WorkingIntervalsMasterData.CentersFixingTime;
            var CentersData = masterDatas.CentersMasterData.CentersData;
            var TaraDataSet = masterDatas.TarasMasterData;

            var addConds = CentersMasterData.CentersData.TryGetValue(chosenRC, out var dataHere) && dataHere.HasAdditionalActions;
            var center = centers_choosen[chosenRC].Copy();



            //var paramsCopy = center.HasToProcessAdditionalConditions ? new Dictionary<string, string>(center.Params) : null; // копируем
            var RechangeTimesOnThisCenter = center.RechangesTime; // копируем
            var ProstoiTimesOnThisCenter = center.ProstoiTime; // копируем
            var lastDate = asParallel ? center.LastStartDate : center.LastDate; // копируем. Если параллельно
            var nextFixIndex = (center.NextNonWorkingInterval, false); // копируем
            var totalRechangesCopy = TotalRechanges; // копируем
            var totalProstoiCopy = TotalProstoi; // копируем

            double final = 0;
            timeR = 0;
            timeP = 0;
            long timeRK = 0;

            int errorsRate = 0;

            var OperA = a?.OpersList[^1];// : B.OpersList[i - 1];
            var OperB = b.FirstOper;

            RechangesEngineResult reches = null;

            if (!asParallel)
            {
                if (OperA != null)
                {
                    reches = RechangesMasterData.FindRechanges(new(center, new(DateTime.MinValue, -1, -1, -1), OperA, OperB, chosenRC));
                    timeRK = reches.Duration;

                    timeR += timeRK;

                    final += doit(8) * Ks[0];
                    final += doit(0) * Ks[1];
                    final += doit(1) * Ks[2];
                    final += doit(2) * Ks[3];
                    final += doit(3) * Ks[4];
                    final += doit(4) * Ks[5];
                    final += doit(5) * Ks[6];
                    final += doit(6) * Ks[7];
                    final += doit(7) * Ks[8];

                    final += timeRK * Ks[9];
                }

                if (addConds)
                {

                    if (OperA != null) center.AdditinalConditions = AdditionalConditionsMasterData.UpdateAdditionalParamsOnCenter(new RechangesEngineData(center, new(DateTime.MinValue, -1, -1, -1), OperA, OperB, chosenRC));
                    //MOSITLAB.AfterOpChangesEvent(chosenRC, center, OperB);
                }

                final += (Ks[12] - timeRK) * Ks[10];
                final += totalRechangesCopy * Ks[11];
                final += RechangeTimesOnThisCenter.TotalSeconds * Ks[13];
            }

            // Чекаем наладочные длины здесь и у предков
            GottenNaladLengthStruct naladLengths = NaladLengthsMasterData.GetNaladLengthsForCurrentStageOnly(reches, OperB, chosenRC, a);
            naladsOut = naladLengths;

            // Если вышел за пределы допустимой наладочной длины на этом РЦ, то хреначим ошибку
            if (OperB.NeededNaladLength.HasValue && NaladLengthsMasterData.NaladLengthsConverted(naladLengths.NaladLengthNexts, OperB.EdIzmClassificator, OperB.KgKm).Value > OperB.NeededNaladLength.Value.Length.Value) // TODO: в результате пустой след. налад. длины V ослепла
            {
                errorsRate += 10000000;
            }

            TrynaladLengthsSet(b.FirstOper.Index, naladLengths.NaladLengthNexts, true, naladLengths.NaladLengthFirst, naladLengths.NaladLengthOther, naladLengths.IsFirst);

            void TrynaladLengthsSet(int etapID, NaladLengthValue sum, bool firstInvoke, NaladLengthValue firstLength, NaladLengthValue otherLength, bool firstLaunch)
            {
                var info = pool.NaladLengthsData[etapID];

                // Суммируем:
                if (NaladLengthsMasterData.NaladLengthsConverted(info.data.NaladLengthNexts // Сохраненное количество длины на этом моделировании
                    +
                    sum, b.FirstOper.EdIzmClassificator, b.FirstOper.KgKm).Value // Текущую, предлагаемую добавку к ним
                    +
                    b.Count // и статическое количество из меса
                    > maxTaraNamot) // сравниваем с макс намоткой на таре
                {
                    // Вылез за пределы намотки! Такое НЕДОПУСТИМО, хотя мб G это потом и исправит
                    errorsRate += 10000;
                }

                foreach (var prev in ALL[etapID].Prevs)
                {
                    TrynaladLengthsSet(prev, sum, false, new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m)), new NaladLengthValue(0, new(Guid.Empty, DimensionUnit.m)), false);
                }
            }

            //14 - простой сейчас
            var timePK = OperB.Fixed
                ? (long)(OperB.StartTimeFact.Value - lastDate).TotalSeconds
                : ProstoiTimes.TryGetValue(OperB.Index, out var dat)
                    ? (long)(dat - lastDate).TotalSeconds
                    : (long)( MIN_DATE_TIME_AFTER_FIX - lastDate).TotalSeconds;
            //if (timePK > 10000000)
            //{
            //    if (Settings.LoadedData.LogErrors)
            //    {
            //        ConcurrentLogger.AddToLogQueue.Enqueue(
            //            $"timePK: {timePK}\r\n" +
            //            $"fact: {OperB.StartTimeFact}\r\n" +
            //            $"dyn: {DYNAMIC_START_TIME}\r\n" +
            //            $"min: {MIN_DATE_TIME}\r\n");
            //    }
            //}
            timePK -= timeRK; // Отнимает переналадки
            if (timePK < 0)
            {
                timePK = 0;
            }

            long timePKSum = 0;
            while (timePK > 0)
            {
                if (nextFixIndex.Item1 > -1)
                {
                    var enddateP = lastDate.AddSeconds(timePK);
                    var enddateF = CentersFixingTime[chosenRC][nextFixIndex.Item1].When;
                    var span = enddateP - enddateF;
                    if (span.Ticks >= 0)
                    {
                        var b1 = span.TotalSeconds < 1;
                        var asint = (int)span.TotalSeconds;
                        AddProstoi(timePK - asint);
                        timePK = asint;
                        timePK -= (int)CentersFixingTime[chosenRC][nextFixIndex.Item1].Time.TotalSeconds;
                        AddFixingTime(null);
                        if (b1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        AddProstoi(timePK);
                        break;
                    }
                }
                else
                {
                    AddProstoi(timePK);
                    break;
                }
            }

            void AddFixingTime(TimeSpan? time)
            {
                lastDate = lastDate.Add(time ?? CentersFixingTime[chosenRC][nextFixIndex.Item1].Time);
                nextFixIndex.Item1++;
                nextFixIndex.Item2 = false;
                if (nextFixIndex.Item1 == CentersFixingTime[chosenRC].Count)
                {
                    nextFixIndex.Item1 = -1;
                }
            }

            void AddProstoi(long secs)
            {
                timePKSum += secs;

                AddTime(secs); // новая строчка
            }

            void AddTime(double secs)
            {
                lastDate = lastDate.AddSeconds(secs);
            }

            final += timePKSum * Ks[14];
            timeP += timePKSum;

            //15 - простоев всего
            final += totalProstoiCopy * Ks[15];

            //16 - простой на этом рц
            final += ProstoiTimesOnThisCenter.TotalSeconds * Ks[16];

            final += (OperB.Level) * Ks[17];

            final += (OperB.IsLast ? 1 : 0) * Ks[18];

            if (OperB.IsLast && !string.IsNullOrEmpty(OperB.OrderLink.OrderID.Version))
            {
                final += Ks[19] * (OperB.OrderLink.Deadline - lastDate).TotalDays;
            }

            RechangeTimesOnThisCenter += TimeSpan.FromSeconds(timeRK);
            totalRechangesCopy += timeRK;
            ProstoiTimesOnThisCenter += TimeSpan.FromSeconds(timePKSum);
            totalProstoiCopy += timePKSum;

            while (timeRK > 0)
            {
                if (nextFixIndex.Item1 > -1)
                {
                    var enddateP = lastDate.AddSeconds(timeRK);
                    var enddateF = CentersFixingTime[chosenRC][nextFixIndex.Item1].When;
                    var span = enddateP - enddateF;
                    if (span.Ticks >= 0)
                    {
                        var b1 = span.TotalSeconds < 1;
                        var asint = (int)span.TotalSeconds;
                        AddTime(timeRK - asint);
                        timeRK = asint;


                        AddFixingTime(null);
                        if (b1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        AddTime(timeRK);
                        break;
                    }
                }
                else
                {
                    AddTime(timeRK);
                    break;
                }
            }

            var time = OperB.GetDuration(chosenRC);// * (double)CentersData[chosenRC].TimeK;

            while (time > 0)
            {
                if (nextFixIndex.Item1 > -1)
                {
                    var enddateP = lastDate.AddSeconds(time);
                    var enddateF = CentersFixingTime[chosenRC][nextFixIndex.Item1].When;
                    var span = enddateP - enddateF;
                    if (span.Ticks >= 0)
                    {
                        var b1 = span.TotalSeconds < 1;
                        AddTime(time - span.TotalSeconds);
                        time = (int)span.TotalSeconds;
                        AddFixingTime(null);
                        if (b1)
                        {
                            break;
                        }
                    }
                    else
                    {
                        AddTime(time);
                        break;
                    }
                }
                else
                {
                    AddTime(time);
                    break;
                }
            }

            double doit(int cri)
            {
                var rees = new RechangesEngineResult(new Dictionary<Guid, Rechange>[RechangesMasterData.MaxPrevailingLevel], new(), 0, false);
                RechangesMasterData.GetRechangesListByCriteria(new RechangesEngineData(center, new(DateTime.MinValue, -1, -1, -1), OperA, OperB, chosenRC), cri, rees);
                rees.UpdateFields();
                return rees.Duration;
            }

            // Напоследок: сколько в теории может появиться простоев
            if (Ks.Length > 20)
            {
                if (Ks[20] != 0)
                {
                    foreach (var next in b.OpersList.SelectMany(x => x.Nexts).Distinct())
                    {
                        var lastD = centers_choosen[ALL[next].Center].LastDate;
                        var span = lastDate - lastD;
                        final += span.TotalSeconds * Ks[20];
                    }
                }
                //if (!asParallel)
                //var diff = (lastDate - center.LastDate).TotalMinutes; // - Было так
                var diff = (lastDate - basisLink.MinAcceptableDate).TotalMinutes;
                if (!asParallel)
                {
                    final += diff * Ks[21]; // "смещение вправо"
                }

                if (asParallel)
                {
                    final += diff * Ks[25]; // "смещение вправо"
                }

                if (Ks.Length > 26)
                {
                    if (asParallel)
                    {
                        final -= Ks[26];
                    }
                }

                if (!tara.IsEmpty && TaraDataSet.TarasPossibleCount.TryGetValue(tara, out var allCountTara))
                {
                    var taraCountHere = pool.GetTaraCountAt(tara, center.LastStartDate);
                    var taraK = (float)taraCountHere / allCountTara;

                    final += Ks[22] * taraK;
                    if (taraK > 1)
                    {
                        final += Ks[23];
                        errorsRate += (int)Ks[23]; // Добавляем штрафу
                    }
                }
                //if (asParallel) final += (lastDate - center.LastStartDate).TotalMinutes * Ks[21]; // "смещение вправо"

                var guid1 = OperA != null ? (OperA.CustomPartiaGuid ?? Guid.Empty/*(OperA.IsCustomMod ? OperA.CustomModelling : OperA.Modelling)*/) : Guid.Empty;
                var guid2 = OperB != null ? (OperB.CustomPartiaGuid ?? Guid.Empty/*(OperB.IsCustomMod ? OperB.CustomModelling : OperB.Modelling)*/) : Guid.Empty;
                if (guid1 != Guid.Empty && guid2 != Guid.Empty && guid1 == guid2)
                    final += Ks[24]; // "близость к одному гп, линейн"

                if (Ks.Length > 27) final += Ks[27] * center.TotalTime.TotalMinutes;

                if (Ks.Length > 28 && OperB.PrevDateTime.HasValue) final += Ks[28] * Math.Abs((lastDate - OperB.PrevDateTime.Value).TotalDays);
            }

            return (final, errorsRate);
        }

    }
}

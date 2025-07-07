using Azure;
using ApsRT.Core.Partias.PartiasAlgorithm;
using ApsRT.Core.Planning.PlanningCalculatorEngine;
using ApsRT.Core.Planning.PlanningClasses;
using ApsRT.Core.Planning.PlanningClasses.PoolClasses;
using ApsRT.Core.Session;
using ApsRT.Core.Stages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using System.Collections;
using ApsRT.Core.Taras.TaraSearchEngine;
using ApsRT.Core.Leftovers;

namespace ApsRT.Core.Planning.PlanningAlgorithm
{
    public class PlanningAlgorithm
    {


        public Session.Session SessionLink { get; }

        public PlanningAlgorithm(Session.Session session)
        {
            SessionLink = session;
            CurrentStage = PlanningAlgorithmStage.None;
        }

        public DateTime MinAcceptableDate { get; set; }// => FIXED_START_TIME.HasValue ? FIXED_START_TIME.Value : MIN_DATE_TIME_AFTER_FIX;
        public DateTime StartOfPlanningDate { get; set; }// => FIXED_START_TIME.HasValue ? FIXED_START_TIME.Value : MIN_DATE_TIME_AFTER_FIX;

        public List<OpersBlock> BestBlocksList { get; private set; }

        public PlanningCalculatorEngineResult OriginalValue { get; private set; }

        public PlanningAlgorithmWorkflow UsedWorkflow { get; private set; }

        public PlanningAlgorithmStage CurrentStage { get; internal set; }

        public Action<PlanningAlgorithmStage, int> CurrentStageChanged;

        public Action<PlanningAlgorithmStage, int> AlgorithmCallback;

        public Action<PlanningCalculatorEngineResult, OpersPool> OnFirstResultAcquried;

        public Action<PlanningCalculatorEngineResult, List<OpersBlock>> OnNewResult;

        public Task<bool> Process { get; private set; }

        public bool Start()
        {
            if (CurrentStage == PlanningAlgorithmStage.None || CurrentStage == PlanningAlgorithmStage.Prepared)
            {
                if (SessionLink.PartiasAlgorithmIsNotRunning)
                {
                    // Да, можно.
                    Process = Task.Run(() => AlgorithmProcess(16, 1));
                    return true;
                }
                else
                {
                    // Нельзя. Выведи что-нибудь
                    return false;
                }
            }
            else
            {
                // Нельзя. Выведи что-нибудь
                return false;
            }
        }

        public object LockerObject { get; set; } = new();

        public bool WannaStop { get; private set; } = false;

        public void Stop() => WannaStop = true;

        public int GLOBAL_FIXED_MIN = 0;
        public int GLOBAL_VPZ_MAX = 0;

        public bool FoundSmth = false, FoundGlobal = false;

        public bool AlgorithmProcess(int parallelCount, int depth)
        {
            try
            {
                if (CurrentStage != PlanningAlgorithmStage.Prepared)
                {
                    PrepareEverything();
                }

                (var result, var localCopy) = SessionLink.SessionOutputEngine.Output(ForcedToRecreate: false, true);
                OnFirstResultAcquried?.Invoke(result, localCopy);

                if (SessionLink.SessionOptions.Jsort)
                {
                    SessionLink.AssignedLogger.LogQueue.Enqueue($"[{DateTime.Now}]: jsort done.\r\n");
                    CurrentStage = PlanningAlgorithmStage.Complete;
                    CurrentStageChanged?.Invoke(CurrentStage, 0);
                    if (SessionLink.SessionOptions.IsSilent) SessionLink.EndSession(0);//Close();
                    return true;
                }


                var LoadedData = SessionLink.SettingsController.CurrentSettings;

                UsedWorkflow = new PlanningAlgorithmWorkflow(
                   SessionLink,
                   !LoadedData.UseCycles && !LoadedData.UseMinutes ? LoadedData.IterationsCount : 0,
                   LoadedData.UseCycles ? LoadedData.CyclesCount : 0,
                   LoadedData.UseMinutes ? LoadedData.MinutesCount : 0,
                   OriginalValue,
                   8
                   );

                CurrentStage = PlanningAlgorithmStage.RunningV;
                CurrentStageChanged?.Invoke(CurrentStage, BestBlocksList.Count);
                var V = new AlgorithmV(this);

                V.Start(UsedWorkflow);
                if (WannaStop)
                {
                    CurrentStage = PlanningAlgorithmStage.Failed;
                    CurrentStageChanged?.Invoke(CurrentStage, 0);
                    return false;
                }

                CurrentStage = PlanningAlgorithmStage.RunningG;
                CurrentStageChanged?.Invoke(CurrentStage, LoadedData.UseMinutes ? (int)(LoadedData.MinutesCount * 100) : (LoadedData.UseCycles ? LoadedData.CyclesCount : LoadedData.IterationsCount));
                var G = new AlgorithmG(this);
                G.Start(parallelCount, depth);

                if (WannaStop)
                {
                    CurrentStage = PlanningAlgorithmStage.Failed;
                    CurrentStageChanged?.Invoke(CurrentStage, 0);
                    return false;
                }

                CurrentStage = PlanningAlgorithmStage.Complete;
                CurrentStageChanged?.Invoke(CurrentStage, 1);
                return true;
            }
            catch (Exception ex)
            {
                CurrentStage = PlanningAlgorithmStage.Failed;
                CurrentStageChanged?.Invoke(CurrentStage, 0);
                return false;
            }
            
        }

        public bool PrepareEverything()
        {
            if (WannaStop)
            {
                WannaStop = false; // Сбрасываем, если кнопка была нажата ранее
            }


            MinAcceptableDate = DateTime.Now;
            StartOfPlanningDate = DateTime.Now;

            Console.WriteLine($"planning start: {MinAcceptableDate}");

            // Формируем список этапчиков
            List<Stage> stages = SessionLink.MasterDataLinks.StagesMasterData.AllStages.Select(x => x.Value).ToList();

            // Фильтр этапчиков
            stages = stages.Where(x => x != null && !x.ClosedByZadels).ToList();

            if (stages.Count == 0)
            {
                Console.WriteLine("no any opers, exit.");
                return false;
            }

            Console.WriteLine("convert");
            GLOBAL_FIXED_MIN = 0;
            GLOBAL_VPZ_MAX = 0;
            var mods = CreateModsInfo(stages);
            List<OpersBlock> ops = ConvertToModellings(stages, mods);
            Console.WriteLine($"blocks: {ops.Count}");
            Console.WriteLine("4.3 - order them");
            (GLOBAL_FIXED_MIN, GLOBAL_VPZ_MAX) = Order_Fixed_VPZ_Free(ref ops, true/*OpersAreNotHighlyStricted*/);

            Console.WriteLine(ops.SelectMany(x => x.Lists.SelectMany(y => y.OpersList.Select(z => z.Index))).Count());
            Console.WriteLine(ops.SelectMany(x => x.Lists.SelectMany(y => y.OpersList.Select(z => z.Index))).Distinct().Count());

            BestBlocksList = ops;

            Console.WriteLine("calc...");

            var pool = new OpersPool(SessionLink, ops, new OpersPoolConstructor(ops, MinAcceptableDate), MinAcceptableDate);

            SessionLink.MasterDataLinks.WorkingIntervalsMasterData.PrepareCentersForMinDataTime(pool.CentersData.Keys, MinAcceptableDate);

            PrepareOnOneRCData(pool);

            var result = PlanningCalculatorEngineResult.CreateFullInstance(SessionLink, MinAcceptableDate);
            var localCopy = new OpersPool(SessionLink, ops, new OpersPoolConstructor(ops, MinAcceptableDate), MinAcceptableDate);
            OriginalValue = PlanningCalculatorEngine.PlanningCalculatorEngine.CalculatePlan(localCopy, MinAcceptableDate, result);

            // Ну ещё и остатки приготавливаем, сначала очищаем, затем создаём
            SessionLink.MasterDataLinks.LeftoversMasterData.PrepareModsForLeftovers(ops);

            FoundSmth = false;
            FoundGlobal = false;

            Console.WriteLine("done.");
            CurrentStage = PlanningAlgorithmStage.Prepared;
            CurrentStageChanged?.Invoke(CurrentStage, 0);
            return true;
        }

        Dictionary<(PlanningModellingType, Guid), int> CreateModsInfo(List<Stage> opers)
        {
            Dictionary<(PlanningModellingType, Guid), int> localModellings = new();
            HashSet<int> allowed = new HashSet<int>(opers.Select(x => x.Index));

            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;

            var groupsEasyKey = allowed.GroupBy(x => ALL[x].EasyFlag.HasValue);

            foreach (var groupFlags in groupsEasyKey)
            {
                if (groupFlags.Key) // Это общий этап? (если нет - то гуид будет пустой)
                {
                    // Есть общий этап
                    var groups = groupFlags.GroupBy(x => ALL[x].EasyFlag.Value);
                    foreach (var group in groups)
                    {
                        localModellings.Add((ALL[group.First()].EasyFlagOriginal.HasValue ? PlanningModellingType.EasyFlagFromMES : PlanningModellingType.EasyFlagAfterPartias, group.Key), group.Count());
                    }
                }
                else
                {
                    // нет общего этапа. Вообще. Это просто моделирование

                    // Находим обычные моделирования
                    var groups = groupFlags.Where(x => !ALL[x].IsCustomModelling && ALL[x].MesModelling.Code != Guid.Empty).GroupBy(x => ALL[x].MesModelling.Code);
                    foreach (var group in groups)
                    {
                        localModellings.Add((PlanningModellingType.FromMES, group.Key), group.Count());
                    }

                    // Находим моделирования после партий
                    var groups2 = allowed.Where(x => ALL[x].IsCustomModelling).GroupBy(x => ALL[x].CustomModelling);
                    foreach (var group in groups2)
                    {
                        localModellings.Add((PlanningModellingType.AfterPartias, group.Key), group.Count());
                    }
                }
            }

            return localModellings;
        }

        List<OpersBlock> ConvertToModellings(List<Stage> opers, Dictionary<(PlanningModellingType, Guid), int> mods)//, List<int> FirstOnes)
        {
            //var str = string.Join(", ", opers.Select(x => x.ID));
            //Console.WriteLine(str);
            // Создаём объединения на основании всего этого
            var final = new List<OpersBlock>();
            var I = 0;
            var HasErrors = false;
            HashSet<int> ClosedIDs = new();

            var CentersData = SessionLink.MasterDataLinks.CentersMasterData.CentersData;

            //var index = opers.FindIndex(x => x.ID == 614);

            Dictionary<int, int> OperIdsToModsIds = new(opers.Count);
            int CurrentModId = 0;

            while (I < opers.Count)
            {
                if (ClosedIDs.Contains(opers[I].Index))
                {
                    I++;
                    continue;
                }

                if (mods == null)
                {
                    addOne(); // Добавляем по одной
                }
                else
                {
                    if (opers[I].EasyFlag.HasValue)
                    {
                        // Это общий этап
                        findWithThisMod(opers[I].EasyFlagOriginal.HasValue ? PlanningModellingType.EasyFlagFromMES : PlanningModellingType.EasyFlagAfterPartias, opers[I].EasyFlag.Value, x => x.EasyFlag == opers[I].EasyFlag.Value);
                    }
                    else
                    {
                        // Это не общий этап
                        //if (opers[I].Modelling == Guid.Empty)
                        //{
                        //    if (opers[I].IsCustomMod)
                        //    {
                        //        findWithThisMod(ModellingType.AfterPartii, opers[I].CustomModelling, x => x.IsCustomMod && x.CustomModelling == opers[I].CustomModelling);
                        //    }
                        //    else
                        //    {
                        //        addOne(); // добавляем одно
                        //    }
                        //}
                        //else
                        //{
                        //    findWithThisMod(ModellingType.FromMES, opers[I].Modelling, x => !x.IsCustomMod && x.Modelling == opers[I].Modelling);
                        //}


                        // Это кастомное моделирование?
                        if (opers[I].IsCustomModelling)
                        {
                            findWithThisMod(PlanningModellingType.AfterPartias, opers[I].CustomModelling, x => x.IsCustomModelling && x.CustomModelling == opers[I].CustomModelling);
                        }
                        else
                        {
                            if (opers[I].MesModelling.Code == Guid.Empty)
                            {
                                addOne(); // добавляем одно
                            }
                            else
                            {
                                findWithThisMod(PlanningModellingType.FromMES, opers[I].MesModelling.Code, x => !x.IsCustomModelling && x.MesModelling.Code == opers[I].MesModelling.Code);
                            }
                        }
                    }
                }

                void addOne()
                {
                    var arr = new OpersBlock(
                        new List<PlanningModelling>()
                        {
                                new PlanningModelling(new List<Stage>(1) { opers[I] }, CentersData[opers[I].Center].CanBeParallel, PlanningModellingType.Elementar, CurrentModId, null, null)
                        }, opers[I].Center, CentersData[opers[I].Center].CanBeParallel ? 0f : -1, true);
                    OperIdsToModsIds.Add(opers[I].Index, CurrentModId);
                    ClosedIDs.Add(opers[I].Index);
                    //if (opers[I].ID == 614)
                    //{
                    //    ;
                    //}
                    final.Add(arr);
                    CurrentModId++;
                }

                void findWithThisMod(PlanningModellingType type, Guid m, Predicate<Stage> matcher)
                {
                    var c = mods[(type, m)];
                    //var c2 = 0;
                    var arr = new List<Stage>();
                    for (var j = 0; j < c; j++)
                    {
                        var N = opers.FindIndex(I, x => !ClosedIDs.Contains(x.Index) && matcher.Invoke(x));//x.IsCustomMod && x.CustomModelling == m);
                        if (N == -1)
                        {
                            Console.WriteLine($"несоответствие моделирования! Моделирование {m}, операция {opers[I + j].Index} не нашла достаточно моделирования ({j}/{c}).");
                            HasErrors = true;
                            break;
                        }
                        arr.Add(opers[N]);
                        //if (opers[N].ID == 614)
                        //{
                        //    ;
                        //}
                        ClosedIDs.Add(opers[N].Index);
                        OperIdsToModsIds.Add(opers[N].Index, CurrentModId);
                    }

                    var block = new OpersBlock(
                        new List<PlanningModelling>()
                        {
                                new PlanningModelling(arr, CentersData[opers[I].Center].CanBeParallel, type, CurrentModId, null, null)
                        }, arr[0].Center, CentersData[opers[I].Center].CanBeParallel ? 0f : -1, true);

                    //block.FillKoeff = CentersDataSet.CentersData[opers[I].Center].CanBeParallel ? 0.25f : -1; // TODO

                    final.Add(block
                        );//По идее тут может быть false, если мы хотим прям заблокировать возможность разрывать
                          //I++;
                    CurrentModId++;
                }

            }

            for (int i = 0; i < final.Count; i++)
            {
                var mod = final[i].Lists[0];
                mod.PrevMods = mod.OpersList.SelectMany(x => x.Prevs).Where(x => OperIdsToModsIds.ContainsKey(x)).Select(x => OperIdsToModsIds[x]).Distinct().ToList(); // TODO: исправить эту залупу
                mod.NextMods = mod.OpersList.SelectMany(x => x.Nexts).Where(x => OperIdsToModsIds.ContainsKey(x)).Select(x => OperIdsToModsIds[x]).Distinct().ToList();
            }

            //if (HasErrors)
            //{
            //    Helper.AddToLogQueue.Enqueue($"[{DateTime.Now}]: mods errors. Stopping with -2.\r\n");
            //    Environment.Exit(-2);//Close();
            //}
            // Перетаскиваем зафиксированные операции, чтоб они слиплись


            return final;
        }

        (int Fixed, int VPZ) Order_Fixed_VPZ_Free(ref List<OpersBlock> list, bool SortThem)
        {
            //var str = string.Join(", ", list.SelectMany(y => y.Lists[0].Select(x => x.ID)));
            //Console.WriteLine("а теперь");
            //Console.WriteLine(str);

            //NeededMinTime = MIN_DATE_TIME;
            if (SortThem)
            {
                Sorter(ref list);
            }

            //var FIXED = list.Where(x => x.Lists[0].OpersList[0].Fixed).ToList();
            var FIXED = list.Where(x => x.Lists.Any(y => y.OpersList.Any(z => z.Fixed))).ToList();
            if (SortThem)
            {
                FIXED = FIXED.OrderBy(x => x.Lists[0].OpersList[0].StartTimeFact).ToList();
            }

            var VPZ = list.Where(x => !x.Lists[0].OpersList.Any(z => z.Fixed) && (x.Lists[0].OpersList.Any(z => z.VPZ) || x.Lists[0].OpersList.Any(z => z.IsFact))).ToList();
            if (SortThem) Sorter(ref VPZ);

            var FREE = list.Where(x => !x.Lists[0].OpersList.Any(z => z.Fixed) && !x.Lists[0].OpersList.Any(z => z.VPZ) && !x.Lists[0].OpersList.Any(z => z.IsFact)).ToList();
            if (SortThem) Sorter(ref FREE);


            list = new();
            list.AddRange(FIXED);//.Union(VPZ).Union(FREE).ToList();
            //if (FIXED?.Count > 0)
            //{
            //    var v1 = FIXED.Where(x => x.Lists?[0]?.OpersList[0].StartTimeFact.HasValue ?? false);
            //    if (v1.Count() > 0)
            //        NeededMinTime = v1.Min(x => x.Lists[0].OpersList[0].StartTimeFact.Value);
            //}
            list.AddRange(VPZ);//.Union(VPZ).Union(FREE).ToList();
            int vpz = list.Count;
            list.AddRange(FREE);//.Union(VPZ).Union(FREE).ToList();

            // Чекаем хронологию сразу
            HashSet<int> tried = new();
            while (!CheckChronology(list, out var operN, out var opToFix))
            {
                var block = list[opToFix];
                list.RemoveAt(opToFix);
                list.Insert(operN, block);
            }

            return (FIXED.Count, vpz);
        }

        void Sorter(ref List<OpersBlock> toSort)
        {
            toSort = toSort.OrderBy(x => x.Lists[0].OpersList[0].PrevDateTime.HasValue ? x.Lists[0].OpersList[0].PrevDateTime.Value : DateTime.MaxValue).ThenBy(x => x.Lists[0].OpersList[0].Level).ToList();
            return;
        }

        bool CheckChronology(List<OpersBlock> opers, out int operN, out int ToFix)
        {
            var good = true;
            var Already = new HashSet<int>();
            var dic = new Dictionary<int, int>();
            operN = 0;
            ToFix = 0;

            for (var i = 0; i < opers.Count; i++)
            {
                for (var o = 0; o < opers[i].Lists.Count; o++)
                {
                    for (var j = 0; j < opers[i].Lists[o].OpersList.Count; j++)
                    {
                        var id = opers[i].Lists[o].OpersList[j].Index;
                        for (var h = 0; h < opers[i].Lists[o].OpersList[j].Nexts.Count; h++)
                        {
                            var op = opers[i].Lists[o].OpersList[j];
                            var n = op.Nexts[h];
                            //var f = opers.FindIndex(x => x.Lists.Any(y => y.Any(z => z.ID == opers[i].Lists[o][j].Prevs[h])));//opers.FindIndex(ref opers, , 0, false);
                            if (Already.Contains(n) && dic[n] != i && !op.Fixed) // Уже есть ребенок???
                            {
                                good = false;
                                operN = i;
                                ToFix = dic[n];
                                break;
                            }
                        }
                        Already.Add(id);
                        dic.Add(id, i);
                        if (!good)
                        {
                            break;
                        }
                    }
                    if (!good)
                    {
                        break;
                    }
                }
                if (!good)
                {
                    break;
                }
            }
            return good;
        }

        public ConcurrentDictionary<int, TemporaryTaraSearchData> ModToTaraInfo = new();

        public Dictionary<int, int> ModIDToOnOneRCGroupID = new();
        public Dictionary<int, (List<int> ModIDs, List<Uco> PossibleRCs)> OnOneRCGroupData = new();
        public void PrepareOnOneRCData(OpersPool pool)
        {
            ModIDToOnOneRCGroupID.Clear();
            OnOneRCGroupData.Clear();

            var modsToGroup = pool.Blocks.SelectMany(x => x.Lists).Where(x => x.FirstOper.OnOneRC); // Выберем все нужные моды

            var groups = modsToGroup.GroupBy(mod => (mod.FirstOper.GetOrderOrPartiaCode(), mod.FirstOper.Level, mod.FirstOper.OperationVid, mod.FirstOper.Nomenklature));
            foreach (var group in groups) // каждая группа определена по номеру партии, уровню, виду операции, номенклатуры
            {
                var groupID = OnOneRCGroupData.Count;
                var modList = group.Select(x => x.ModID).ToList();

                List<Uco> rcs = null;
                foreach (var mod in group)
                {
                    if (rcs == null)
                        rcs = mod.PossibleRCsHere;
                    else
                        rcs = rcs.Intersect(mod.PossibleRCsHere).ToList();
                }


                OnOneRCGroupData.Add(groupID, (modList, rcs));
                foreach (var modID in modList)
                {
                    ModIDToOnOneRCGroupID.Add(modID, groupID); // Заполняем ссылки на группы
                }
            }
        }

        public void UpdateOpsOnOneRC(OpersBlock blockToStart, OpersPool pool)
        {
            var settedRC = blockToStart.ActualRC;
            foreach (var mod in blockToStart.Lists)
            {
                if (mod.FirstOper.OnOneRC
                    && ModIDToOnOneRCGroupID.TryGetValue(mod.ModID, out var dataID)
                    && OnOneRCGroupData.TryGetValue(dataID, out var data))
                {
                    foreach (var neededMod in data.ModIDs)
                    {
                        if (neededMod == mod.ModID) continue; // текущий мод не нужно искать и менять РЦ
                        var index = pool.Blocks.FindIndex(x => x.Lists.Any(y => y.ModID == neededMod));
                        if (index != -1) pool.Blocks[index].ActualRC = settedRC;
                    }
                }
            }
        }

        public int LastGen { get; private set; } = 0;
        public TimeSpan LastDate { get; private set; } = TimeSpan.MinValue;

        public void UpdateBcsSuccess(PlanningCalculatorEngineResult t, List<OpersBlock> ops)
        {
            //lock (LockerObject)
            //{
            //}
            BestBlocksList = ops;
            LastGen = UsedWorkflow.Generation;
            LastDate = DateTime.Now - StartOfPlanningDate;
            FoundSmth = true;
            FoundGlobal = true;
            OnNewResult?.Invoke(t, ops);
        }
    }
}

using Azure;
using ApsRT.Core.Partias.PartiasAlgorithm;
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
using ApsRT.Core.Rechanges.MasterData;
using System.Xml.Linq;
using ApsRT.Core.PlanSimulator;
using ApsRT.Core.PlanSimulator.PlanSimulatorEngine;

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

        public PlanSimulatorPool UsedPool { get; private set; }

        public PlanSimulatorEngineResult OriginalValue { get; private set; }

        public PlanningAlgorithmWorkflow UsedWorkflow { get; private set; }

        public PlanningAlgorithmStage CurrentStage { get; internal set; }

        public Action<PlanningAlgorithmStage, int> CurrentStageChanged;

        public Action<PlanningAlgorithmStage, int> AlgorithmCallback;

        public Action<PlanSimulatorEngineResult, List<PlanSimulatorBlock>, int> OnFirstResultAcquried;

        public Action<PlanSimulatorEngineResult> OnNewResult;

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

        public bool AlgorithmProcess(int poolSize, int depth)
        {
            try
            {
                if (CurrentStage != PlanningAlgorithmStage.Prepared)
                {
                    PrepareEverything();
                }

                (var result, var localCopy, var index) = SessionLink.SessionOutputEngine.Output(ForcedToRecreate: false, true);
                OnFirstResultAcquried?.Invoke(result, localCopy, index);

                if (SessionLink.SessionOptions.Jsort)
                {
                    SessionLink.AssignedLogger.LogQueue.Enqueue($"[{DateTime.Now}]: jsort done.\r\n");
                    CurrentStage = PlanningAlgorithmStage.Complete;
                    CurrentStageChanged?.Invoke(CurrentStage, 0);
                    if (SessionLink.SessionOptions.IsSilent) SessionLink.EndSession(0);//Close();
                    return true;
                }

                //PlanFixer(BestBlocksList);

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
                CurrentStageChanged?.Invoke(CurrentStage, UsedPool.Blocks.Count);
                var V = new AlgorithmV(this);
                SessionLink.AssignedLogger.LogQueue.Enqueue("planning step 1 started");
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
                SessionLink.AssignedLogger.LogQueue.Enqueue("planning step 2 started");
                G.Start(poolSize, depth);

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
            int PoolSize = SessionLink.SettingsController.CurrentSettings.PoolSize;
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
            List<PlanSimulatorBlock> ops = ConvertToModellings(stages, mods, PoolSize);
            Console.WriteLine($"blocks: {ops.Count}");
            Console.WriteLine("4.3 - order them");
            (GLOBAL_FIXED_MIN, GLOBAL_VPZ_MAX) = Order_Fixed_VPZ_Free(ref ops, true/*OpersAreNotHighlyStricted*/);

            Console.WriteLine(ops.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Count());
            Console.WriteLine(ops.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Distinct().Count());

            //BestBlocksList = ops;

            UsedPool = new PlanSimulatorPool(SessionLink, ops, MinAcceptableDate, PoolSize);

            Console.WriteLine("calc...");

            //var pool = new OpersPool(SessionLink, ops, new OpersPoolConstructor(ops, MinAcceptableDate), MinAcceptableDate);

            SessionLink.MasterDataLinks.WorkingIntervalsMasterData.PrepareCentersForMinDataTime(UsedPool.CentersData.Keys, MinAcceptableDate);

            PrepareOnOneRCData(UsedPool);

            var result = PlanSimulatorEngineResult.CreateFullInstance(SessionLink, MinAcceptableDate);
            //var localCopy = new OpersPool(SessionLink, ops, new OpersPoolConstructor(ops, MinAcceptableDate), MinAcceptableDate);
            var dt0 = DateTime.Now;
            OriginalValue = PlanSimulatorEngine.CalculatePlan(ops, 0, MinAcceptableDate, result);
            var dt1 = DateTime.Now;
            Console.WriteLine($"plan calc time: {dt1 - dt0}");

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

        List<PlanSimulatorBlock> ConvertToModellings(List<Stage> opers, Dictionary<(PlanningModellingType, Guid), int> mods, int PoolSize)//, List<int> FirstOnes)
        {
            //var str = string.Join(", ", opers.Select(x => x.ID));
            //Console.WriteLine(str);
            // Создаём объединения на основании всего этого
            var final = new List<PlanSimulatorBlock>();
            var I = 0;
            var HasErrors = false;
            HashSet<int> ClosedIDs = new();

            var CentersData = SessionLink.MasterDataLinks.CentersMasterData.CentersData;

            //var index = opers.FindIndex(x => x.ID == 614);

            //Dictionary<int, int> OperIdsToModsIds = new(opers.Count);
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
                    var arr = new PlanSimulatorBlock(
                        new List<PlanSimulatorMod>()
                        {
                                new PlanSimulatorMod(new List<Stage>(1) { opers[I] }, CentersData[opers[I].Center].CanBeParallel, PlanningModellingType.Elementar, CurrentModId)
                        }, opers[I].Center, PoolSize, CentersData[opers[I].Center].CanBeParallel ? 0f : -1, true);
                    //OperIdsToModsIds.Add(opers[I].Index, CurrentModId);
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
                        //OperIdsToModsIds.Add(opers[N].Index, CurrentModId);
                    }

                    var block = new PlanSimulatorBlock(
                        new List<PlanSimulatorMod>()
                        {
                                new PlanSimulatorMod(arr, CentersData[opers[I].Center].CanBeParallel, type, CurrentModId)
                        }, arr[0].Center, PoolSize, CentersData[opers[I].Center].CanBeParallel ? 0f : -1, true);

                    //block.FillKoeff = CentersDataSet.CentersData[opers[I].Center].CanBeParallel ? 0.25f : -1; // TODO

                    final.Add(block
                        );//По идее тут может быть false, если мы хотим прям заблокировать возможность разрывать
                          //I++;
                    CurrentModId++;
                }

            }

            //for (int i = 0; i < final.Count; i++)
            //{
            //    var mod = final[i].Lists[0];
            //    mod.PrevMods = mod.s.SelectMany(x => x.Prevs).Where(x => OperIdsToModsIds.ContainsKey(x)).Select(x => OperIdsToModsIds[x]).Distinct().ToList(); // TODO: исправить эту залупу
            //    mod.NextMods = mod.OpersList.SelectMany(x => x.Nexts).Where(x => OperIdsToModsIds.ContainsKey(x)).Select(x => OperIdsToModsIds[x]).Distinct().ToList();
            //}

            //if (HasErrors)
            //{
            //    Helper.AddToLogQueue.Enqueue($"[{DateTime.Now}]: mods errors. Stopping with -2.\r\n");
            //    Environment.Exit(-2);//Close();
            //}
            // Перетаскиваем зафиксированные операции, чтоб они слиплись


            return final;
        }

        (int Fixed, int VPZ) Order_Fixed_VPZ_Free(ref List<PlanSimulatorBlock> list, bool SortThem)
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
            var FIXED = list.Where(x => x.Lists.Any(y => y.StagesAsNormal.Any(z => z.Fixed))).ToList();
            if (SortThem)
            {
                FIXED = FIXED.OrderBy(x => x.Lists[0].FirstOperJust.StartTimeFact).ToList();
            }

            var VPZ = list.Where(x => !x.Lists[0].StagesAsNormal.Any(z => z.Fixed) && (x.Lists[0].StagesAsNormal.Any(z => z.VPZ) || x.Lists[0].StagesAsNormal.Any(z => z.IsFact))).ToList();
            if (SortThem) Sorter(ref VPZ);

            var FREE = list.Where(x => !x.Lists[0].StagesAsNormal.Any(z => z.Fixed) && !x.Lists[0].StagesAsNormal.Any(z => z.VPZ) && !x.Lists[0].StagesAsNormal.Any(z => z.IsFact)).ToList();
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
                //if (!tried.Add(opToFix)) break;
            }

            return (FIXED.Count, vpz);
        }

        void Sorter(ref List<PlanSimulatorBlock> toSort)
        {
            toSort = toSort.OrderBy(x => x.Lists[0].FirstOperJust.PrevDateTime.HasValue ? x.Lists[0].FirstOperJust.PrevDateTime.Value : DateTime.MaxValue).ThenBy(x => x.Lists[0].FirstOperJust.Level).ToList();
            return;
        }

        bool CheckChronology(List<PlanSimulatorBlock> opers, out int operN, out int ToFix)
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
                    for (var j = 0; j < opers[i].Lists[o].StagesAsNormal.Count; j++)
                    {
                        var id = opers[i].Lists[o].StagesAsNormal[j].Index;
                        for (var h = 0; h < opers[i].Lists[o].StagesAsNormal[j].Nexts.Count; h++)
                        {
                            var op = opers[i].Lists[o].StagesAsNormal[j];
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
        public Dictionary<int, (List<PlanSimulatorMod> ModIDs, List<Uco> PossibleRCs)> OnOneRCGroupData = new();
        public void PrepareOnOneRCData(PlanSimulatorPool pool)
        {
            ModIDToOnOneRCGroupID.Clear();
            OnOneRCGroupData.Clear();

            var modsToGroup = pool.Blocks.SelectMany(x => x.Lists).Where(x => x.FirstOperJust.OnOneRC); // Выберем все нужные моды

            var groups = modsToGroup.GroupBy(mod => (mod.FirstOperJust.GetOrderOrPartiaCode(), mod.FirstOperJust.Level, mod.FirstOperJust.OperationVid, mod.FirstOperJust.Nomenklature));
            foreach (var group in groups) // каждая группа определена по номеру партии, уровню, виду операции, номенклатуры
            {
                var groupID = OnOneRCGroupData.Count;
                var modList = group/*.Select(x => x.ModID)*/.ToList();

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
                    ModIDToOnOneRCGroupID.Add(modID.ModID, groupID); // Заполняем ссылки на группы
                }
            }
        }

        public void UpdateOpsOnOneRC(PlanSimulatorBlock blockToStart, int PoolIndex)
        {
            (var settedRC, _, _) = blockToStart.GetCenterData(PoolIndex);
            foreach (var mod in blockToStart.Lists)
            {
                if (mod.FirstOperJust.OnOneRC
                    && ModIDToOnOneRCGroupID.TryGetValue(mod.ModID, out var dataID)
                    && OnOneRCGroupData.TryGetValue(dataID, out var data))
                {
                    foreach (var neededMod in data.ModIDs)
                    {
                        if (neededMod.ModID == mod.ModID) continue; // текущий мод не нужно искать и менять РЦ
                        neededMod.BlockLink.SetCenter(settedRC, PoolIndex);
                        //var index = pool.Blocks.FindIndex(x => x.Lists.Any(y => y.ModID == neededMod));
                        //if (index != -1) pool.Blocks[index].ActualRC = settedRC;
                    }
                }
            }
        }

        public int LastGen { get; private set; } = 0;
        public TimeSpan LastDate { get; private set; } = TimeSpan.MinValue;

        public void UpdateBcsSuccess(PlanSimulatorEngineResult t, List<PlanSimulatorBlock> ops, int indexToWrite)
        {
            //lock (LockerObject)
            //{
            //}
            UsedPool.BestPoolIndex = indexToWrite;
            UsedPool.Blocks = ops;
            LastGen = UsedWorkflow.Generation;
            LastDate = DateTime.Now - StartOfPlanningDate;
            FoundSmth = true;
            FoundGlobal = true;
            OnNewResult?.Invoke(t);
        }

        /// <summary>
        /// Функция, которая пытается по-максимуму исправить план: свести к 0 ошибки длин, и к минимуму - критичные размотки тар и прочего.
        /// </summary>
        //public bool PlanFixer(List<PlanSimulatorBlock> blocks)
        //{
        //    if (blocks == null) return false;

        //    var dt0 = DateTime.Now;
        //    Console.WriteLine($"fixer start: {dt0}");

        //    var ops = OpersPool.CopyBlocks(blocks, false);

        //    var preparedData = new OpersPoolConstructor(ops, MinAcceptableDate);

        //    var pool = new OpersPool(SessionLink, ops, preparedData, MinAcceptableDate);

        //    PlanSimulatorEngineResult res = PlanSimulatorEngineResult.CreateFullInstance(SessionLink, MinAcceptableDate);

        //    res = PlanSimulatorEngine.CalculatePlan(pool, MinAcceptableDate, res);

        //    if (res.CatchedNaladLengthsErrorsCount > 0)
        //    {
        //        // Попробуем исправить наладочные длины.
        //        ;

        //        bool ThereIsModInHere(PlanSimulatorMod y, Guid guid) => (y.EasyLifeData != null && y.EasyLifeData.ContainsKey(guid)) || (y.FirstOperJust.IsCustomModelling ? (y.FirstOperJust.CustomModelling == guid) : (y.FirstOperJust.MesModelling.Code == guid));

        //        foreach (var error in res.ModellingsWithBrokenFixedNaladLengths)
        //        {
        //            var dt01 = DateTime.Now;
        //            var guidToFind = error;
        //            var blockIndex = pool.Blocks.FindIndex(x => x.Lists.Any(y => ThereIsModInHere(y, guidToFind))); // Найдем сначала номер блока где была ошибка
        //            var block = pool.Blocks[blockIndex];


        //            //b8a71b1e-9463-4088-bf21-93176ae78e64

        //            var modIndex = block.Lists.FindIndex(y => ThereIsModInHere(y, guidToFind)); // Теперь найдём номер мода
        //            var mod = block.Lists[modIndex];

        //            var opToCheck = mod.EasyLifeData != null ? mod.EasyLifeData[guidToFind].FirstOper : mod.FirstOper;

        //            if (!opToCheck.NeededNaladLength.HasValue)
        //            {
        //                continue;
        //            }

        //            var neededLength = opToCheck.NeededNaladLength.Value;

        //            var currentLengthWas = pool.NaladLengthsData[opToCheck.Index];

        //            var possibleBlocksGroups = pool.Blocks.Select((x, index) => (Block: x, index)).Where(y => mod.PossibleRCsHere.Any(z => z == y.Block.ActualRC)).GroupBy(x => x.Block.ActualRC);

        //            var indexToSet = -1;

        //            foreach (var possibleBlockGroup in possibleBlocksGroups)
        //            {
        //                if (indexToSet != -1) break;
        //                var elements = possibleBlockGroup.ToArray();
        //                var ChoosenCenter = possibleBlockGroup.Key;

        //                var parr = Parallel.For(0, elements.Length, (index, pls) =>
        //                {
        //                    if (indexToSet != -1) return;
        //                    var currentMod = elements[index].Block.Lists[0];
        //                    Stage operationB = currentMod.FirstOper;

        //                    var lastIndex = index - 1;
        //                    var lastMod = lastIndex == -1 ? null : elements[lastIndex].Block.Lists[0];
        //                    Stage operationA = lastMod?.FirstOper;

        //                    var centGlobalStruct = res.FirstStagesAdditionalInfos[operationB.Index];
        //                    var prostoiData = res.ProstoiBeforeFirstStages.TryGetValue(operationB.Index, out var pros) ? pros.ProstoiItself : new PlanSimulatorDatesData(res.FirstStagesStartDate[operationB.Index], 0, -1, -1);

        //                    var rechangesResult = operationA == null ? null : SessionLink.MasterDataLinks.RechangesMasterData.FindRechanges(new(centGlobalStruct, prostoiData, operationA, operationB, ChoosenCenter));

        //                    var naladLengthsData = SessionLink.MasterDataLinks.NaladLengthsMasterData.GetNaladLengthsForCurrentStageOnly(rechangesResult, operationB, ChoosenCenter, lastMod);

        //                    if (naladLengthsData.NaladLengthHere.Value <= neededLength.Length.Value)
        //                    {
        //                        if (indexToSet != -1) return;
        //                        ;
        //                        //var opsL = OpersPool.CopyBlocks(ops, false);
        //                        //var poolL = new OpersPool(SessionLink, opsL, preparedData, MinAcceptableDate);

        //                        //poolL.Blocks.Insert(elements[index].index, block);

        //                        //var blockIndexToRemove = poolL.Blocks.FindIndex(x => x.Lists.Any(y => ThereIsModInHere(y, guidToFind)));

        //                        //poolL.Blocks.RemoveAt(blockIndexToRemove); // Убираем

        //                        //var result = PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(poolL, MinAcceptableDate, null);

        //                        //if (result.ModellingsWithBrokenFixedNaladLengths.Except(res.ModellingsWithBrokenFixedNaladLengths).Count() == 0)
        //                        //{
        //                        //    indexToSet = elements[index].index;
        //                        //    pls.Stop();
        //                        //}
        //                    }

        //                });
        //                while (!parr.IsCompleted)
        //                {
        //                    if (indexToSet != -1) break;
        //                    ;
        //                }
        //            }

                    

        //            var dt02 = DateTime.Now;
        //            Console.WriteLine($"iter: {dt02 - dt01}, index {indexToSet}");
        //            ;
        //        }
        //    }

        //    var dt1 = DateTime.Now;
        //    Console.WriteLine($"fixer end: {dt1}, total {dt1 - dt0}");

        //    BestBlocksList = ops;

        //    return true;
        //}


        #region Наладочные длины

        #endregion

    }
}

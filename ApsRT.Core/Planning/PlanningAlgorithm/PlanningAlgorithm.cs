using ApsRT.Core.Centers.MasterData;
using ApsRT.Core.Leftovers;
using ApsRT.Core.Partias.PartiasAlgorithm;
using ApsRT.Core.PlanSimulator;
using ApsRT.Core.PlanSimulator.PlanSimulatorEngine;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Session;
using ApsRT.Core.Stages;
using ApsRT.Core.Taras.TaraSearchEngine;
using Azure;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ZLinq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;

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

        public PlanSimulatorPool UsedPool { get; /*private*/ set; }

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
                    Process = Task.Run(AlgorithmProcess);
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

        public bool AlgorithmProcess()
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
                SessionLink.AssignedLogger.LogQueue.Enqueue("planning step 1 started");

                if (SessionLink.SettingsController.CurrentSettings.UseR)
                {
                    var Step1 = new AlgorithmR(this);// new AlgorithmV(this);
                    Step1.Start(UsedWorkflow);
                }
                else
                {
                    var Step1 = new AlgorithmV(this);// new AlgorithmV(this);
                    Step1.Start(UsedWorkflow);
                }

                if (WannaStop)
                {
                    CurrentStage = PlanningAlgorithmStage.Failed;
                    CurrentStageChanged?.Invoke(CurrentStage, 0);
                    return false;
                }


                NaladLengthsFixer();
                if (WannaStop)
                {
                    CurrentStage = PlanningAlgorithmStage.Failed;
                    CurrentStageChanged?.Invoke(CurrentStage, 0);
                    return false;
                }


                CurrentStage = PlanningAlgorithmStage.RunningG;
                CurrentStageChanged?.Invoke(CurrentStage, LoadedData.UseMinutes ? (int)(LoadedData.MinutesCount * 100) : (LoadedData.UseCycles ? LoadedData.CyclesCount : LoadedData.IterationsCount));
                
                if (!SessionLink.SettingsController.CurrentSettings.UseR)
                {
                    var G = new AlgorithmG(this);
                    SessionLink.AssignedLogger.LogQueue.Enqueue("planning step 2 started");
                    G.Start(UsedPool.PoolSize, 1);
                }

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

            UsedPool.PrepareOnOneRCData();

            UsedPool.PrepareAllRCsOnBlocks();

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
                                new PlanSimulatorMod(new List<Stage>(1) { opers[I] }, CentersData[opers[I].Center].CanBeParallel, PlanningModellingType.Elementar, CurrentModId, PoolSize)
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
                                new PlanSimulatorMod(arr, CentersData[opers[I].Center].CanBeParallel, type, CurrentModId, PoolSize)
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
                FIXED = FIXED.OrderBy(x => x.Lists[0].FirstOperJust.Level).ThenBy(x => x.Lists[0].FirstOperJust.StartTimeFact).ToList();
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
            //HashSet<int> tried = new();
            while (!CheckChronology(list, out var operN, out var childToFix))
            {
                var block = list[childToFix];
                list.RemoveAt(childToFix);
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

        public bool CheckChronology(List<PlanSimulatorBlock> opers, out int operN, out int childToFix)
        {
            var good = true;
            var Already = new HashSet<int>();
            var BlockIndexes = new Dictionary<int, int>();
            operN = 0;
            childToFix = 0;

            for (var currentBlockIndex = 0; currentBlockIndex < opers.Count; currentBlockIndex++)
            {
                for (var o = 0; o < opers[currentBlockIndex].Lists.Count; o++)
                {
                    for (var j = 0; j < opers[currentBlockIndex].Lists[o].StagesAsNormal.Count; j++)
                    {
                        var currentID = opers[currentBlockIndex].Lists[o].StagesAsNormal[j].Index;

                        bool PrevsAreGood = true;
                        foreach (var prev in opers[currentBlockIndex].Lists[o].StagesAsNormal[j].Prevs)
                        {
                            if (!Already.Contains(prev))
                            {
                                PrevsAreGood = false;
                                break;
                            }
                        }


                        for (var h = 0; h < opers[currentBlockIndex].Lists[o].StagesAsNormal[j].Nexts.Count; h++)
                        {
                            var op = opers[currentBlockIndex].Lists[o].StagesAsNormal[j];
                            var childID = op.Nexts[h];
                            //var f = opers.FindIndex(x => x.Lists.Any(y => y.Any(z => z.ID == opers[i].Lists[o][j].Prevs[h])));//opers.FindIndex(ref opers, , 0, false);
                            if (Already.Contains(childID) && BlockIndexes[childID] != currentBlockIndex && !op.Fixed) // Уже есть ребенок???
                            {


                                good = false;
                                operN = currentBlockIndex;
                                childToFix = BlockIndexes[childID];
                                break;
                            }
                        }
                        Already.Add(currentID);
                        BlockIndexes.Add(currentID, currentBlockIndex);
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

        #region Наладочные длины
        /// <summary>
        /// Функция, которая пытается по-максимуму исправить план: свести к 0 ошибки длин, и к минимуму - критичные размотки тар и прочего.
        /// </summary>
        public HashSet<int> IgnoredGlobalFixedNaladLengthsErrors = null;
        object locker = new();

        bool NaladLengthsFixer()
        {
            if (UsedPool == null) return false;


            var dt0 = DateTime.Now;
            Console.WriteLine($"fixer start: {dt0}");

            bool IHaveFoundSomething;
            do
            {
                IHaveFoundSomething = false;




                HashSet<int> ignoredFixed = new();

                UsedPool.Blocks = UsedPool.Blocks.OrderBy(x => x.BlockIndex[UsedPool.BestPoolIndex]).ToList();
                PlanSimulatorEngineResult res;


                object[] lockers = new object[UsedPool.PoolSize];
                for (int i = 0; i < lockers.Length; i++) lockers[i] = new();

                do
                {
                    if (WannaStop) break;

                    res = PlanSimulatorEngineResult.CreateFullInstance(SessionLink, MinAcceptableDate);

                    var opersBlocksCopy = PlanSimulatorPool.CopyBlocksSwallow(UsedPool.Blocks);

                    res = PlanSimulatorEngine.CalculatePlan(opersBlocksCopy, UsedPool.BestPoolIndex, MinAcceptableDate, res);

                    int bestIndex = UsedPool.BestPoolIndex;

                    if (IgnoredGlobalFixedNaladLengthsErrors == null)
                    {
                        var dt001 = DateTime.Now;
                        IgnoredGlobalFixedNaladLengthsErrors = new();
                        foreach (var index in res.StagesWithBrokenFixedNaladLengths)
                        {
                            var list = UsedPool.StagesLinks[index].ModLink;
                            var options = new SumIsPossibleOptions(false)
                            {
                                LogSomeErrors = false,
                                TaraMustBeThis = list.BlockLink.ExistingTaraHere.Select(x => x.Key).ToHashSet(),
                                //PrevsInfo = prevs,
                                //RCMustBeThis = PossibleRCsHere,
                                //NextRCsMustBeThese = nextRCs,
                                //SaveAvailableResults = true,
                                NaladLength = list.NaladLengthData[bestIndex].Data.NaladLengthHere.Value, // Ну а какую ещё? Это не та по идее, она с текущего РЦ, и след длины могут отличаться, но других данных нет
                            };
                            if (!TaraSearchEngine.SumIsPossible(list, out _, out _, out _, out _, out _, out var searchData, options))
                            {
                                IgnoredGlobalFixedNaladLengthsErrors.Add(index);
                            }
                        }
                        var dt002 = DateTime.Now;
                        Console.WriteLine($"global ignored: {dt002 - dt001}, total: {IgnoredGlobalFixedNaladLengthsErrors.Count}");
                    }

                    res.StagesWithBrokenFixedNaladLengths = res.StagesWithBrokenFixedNaladLengths.Except(IgnoredGlobalFixedNaladLengthsErrors).Except(ignoredFixed).ToHashSet();

                    // Попробуем исправить наладочные длины.
                    if (res.CatchedNaladLengthsErrorsCount > 0)
                    {
                        ConcurrentQueue<int> AvailableIndexes = new ConcurrentQueue<int>();
                        for (int i = 0; i < UsedPool.PoolSize - 3; i++) // -1 потому что последний пул нужен для ручного анализа.
                        {
                            if (i == bestIndex) continue;
                            AvailableIndexes.Enqueue(i);
                        }

                        if (res.StagesWithBrokenFixedNaladLengths.Count > 0)
                        {
                            var error = res.StagesWithBrokenFixedNaladLengths.First(x => !ignoredFixed.Contains(x) && !IgnoredGlobalFixedNaladLengthsErrors.Contains(x));

                            var opToCheck = UsedPool.StagesLinks[error];

                            var neededLength = opToCheck.Stage.NeededNaladLength?.Length ?? null;

                            Func<float, bool> equality1 = (float fl) => { return neededLength.HasValue ? fl <= neededLength.Value.Value : true; };
                            IHaveFoundSomething |= fixerMethod(opToCheck, bestIndex, opersBlocksCopy, res, AvailableIndexes, lockers, ignoredFixed, equality1);
                        }

                    }
                }
                while (res.StagesWithBrokenFixedNaladLengths.Count > 0);





            }
            while (IHaveFoundSomething);



            var dt1 = DateTime.Now;
            Console.WriteLine($"fixer end: {dt1}, total {dt1 - dt0}");

            //    BestBlocksList = ops;

            //foreach (var item in ignoredFixed) IgnoredGlobalFixedNaladLengthsErrors.Add(item);

            return true;
        }

        bool fixerMethod(PlanSimulatorStage opToCheck, int bestIndex, List<PlanSimulatorBlock> opersBlocksCopy, PlanSimulatorEngineResult res, ConcurrentQueue<int> AvailableIndexes, object[] lockers, HashSet<int> ignoredFixed, Func<float, bool> equalityOne)
        {
            var CentersMasterData = SessionLink.MasterDataLinks.CentersMasterData;
            var dt01 = DateTime.Now;
            //
            int guidToFind = opToCheck.Stage.Index;
            var block = opToCheck.ModLink.BlockLink;

            //var opToCheck = mod.EasyLifeData != null ? mod.Stages[mod.EasyLifeData[guidToFind].FirstElementIndex] : mod.FirstOper;

            //if (!opToCheck.Stage.NeededNaladLength.HasValue)
            //{
            //    return;
            //}

            var indexToSet = -1;

            Uco bestRC = Uco.EmptyObject;


            var currentLengthWas = opToCheck.NaladLengthData[bestIndex];//pool.NaladLengthsData[opToCheck.Index];

            var PossibleRCsHere = block.PossibleRCsHere;

            var sdvig = new PlanSimulatorMod(block.Lists[0].StagesAsNormal, false, PlanningModellingType.AfterPartias, -1, block.PoolLink.PoolSize);
            var prevs = sdvig.StagesAsNormal.SelectMany(x => x.Prevs).Distinct().Where(x =>
            !SessionLink.MasterDataLinks.StagesMasterData.AllStages[x].CurrentTara.EmptyCheck())
                .ToDictionary(x => x, x => SessionLink.MasterDataLinks.StagesMasterData.AllStages[x].CurrentTara);

            var nextRCs = TaraSearchEngine.FindNeededNextRCs(sdvig.StagesAsNormal, null);// sdvig.StagesAsNormal.SelectMany(x => x.Nexts).Distinct().Select(x => SessionLink.MasterDataLinks.StagesMasterData.AllStages[x].Center).ToHashSet();

            var options = new SumIsPossibleOptions(false)
            {
                LogSomeErrors = false,
                TaraMustBeThis = block.ExistingTaraHere.Select(x => x.Key).ToHashSet(),
                PrevsInfo = prevs,
                RCMustBeThis = PossibleRCsHere,
                NextRCsMustBeThese = nextRCs,
                SaveAvailableResults = true,
                NaladLength = opToCheck.NaladLengthData[bestIndex].Data.NaladLengthHere.Value, // Ну а какую ещё? Это не та по идее, она с текущего РЦ, и след длины могут отличаться, но других данных нет
            };

            if (
                TaraSearchEngine.SumIsPossible(sdvig, out var possibleTara, out _, out _, out _, out _, out var searchData, options)
                && TaraSearchEngine.FindSomeCentersBecauseOfPrevs(sdvig, PossibleRCsHere.ToList(), prevs, options.TaraMustBeThis?.ToList(), out _, out var allRCs, out _, false)
                )
            {
                bool allowedToMoveOn = true;
                if (possibleTara != sdvig.FirstOperJust.CurrentTara)
                {
                    if (block.Lists.Count > 1)
                    {
                        indexToSet = -4; // Для таких случаев тару не меняю.
                        allowedToMoveOn = false;
                    }
                    else
                    {
                        foreach (var stage in sdvig.StagesAsNormal)
                        {
                            stage.CurrentTara = possibleTara;
                        }
                    }
                }

                if (allowedToMoveOn)
                {
                    var possible = searchData.PossibleCentersAndTaras.Where((x, index) => (searchData.AvailableResults == null || searchData.AvailableResults[index])
                        &&
                        allRCs.Contains(x.RC)
                        &&
                        (!block.Lists[0].StagesAsNormal[0].IsVspom || CentersMasterData.CentersData[x.RC].AbleToVspom)
                        ).Select(x => x.RC).Distinct().ToArray();

                    if (possible.Length > 0)
                    {
                        var parr0 = Parallel.For(0, possible.Length, (group, pls0) =>
                        {
                            if (indexToSet != -1)
                            {
                                pls0.Stop();
                                return;
                            }
                            var ChoosenCenter = possible[group];

                            (var MIN, var MAX, var Prev, var Next) = block.PoolLink.FindDiapazons(block, bestIndex, ChoosenCenter, block.BlockIndex[bestIndex]);

                            var elements = opersBlocksCopy.Where(x => x.GetCenterData(bestIndex).Center == ChoosenCenter && x.BlockIndex[bestIndex] >= MIN && x.BlockIndex[bestIndex] <= MAX).ToArray();

                            var parr = Parallel.For(0, elements.Length + 1, (index, pls) =>
                            {
                                if (indexToSet != -1) return;
                                var currentMod = block.Lists[0];//elements[index].Lists[0];
                                var operationB = currentMod.FirstOper;

                                var lastIndex = index - 1;
                                var lastMod = lastIndex == -1 ? null : elements[lastIndex].Lists[0];
                                var operationA = lastMod?.FirstOper;

                                var centGlobalStruct = res.FirstStagesAdditionalInfos[operationB.Stage.Index];
                                var prostoiData = res.ProstoiBeforeFirstStages.TryGetValue(operationB.Stage.Index, out var pros) ? pros.ProstoiItself : new PlanSimulatorDatesData(res.FirstStagesStartDate[operationB.Stage.Index], 0, -1, -1);

                                var rechangesResult = operationA == null ? null : SessionLink.MasterDataLinks.RechangesMasterData.FindRechanges(new(centGlobalStruct, prostoiData, operationA.Stage, operationB.Stage, ChoosenCenter));

                                bool isFirstRes = SessionLink.MasterDataLinks.NaladLengthsMasterData.ItsFirstLaunch(rechangesResult, lastMod);
                                var naladLengthsData = operationB.Stage.GetNaladFullLengthFromCache(ChoosenCenter, isFirstRes ? NaladLengths.RequiredNaladCondition.FirstLaunch : NaladLengths.RequiredNaladCondition.OtherLaunch);//SessionLink.MasterDataLinks.NaladLengthsMasterData.GetNaladLengthsForCurrentStageOnly(rechangesResult, operationB, ChoosenCenter, lastMod);

                                if (equalityOne(naladLengthsData.NaladLengthHere.Value))
                                {
                                    if (indexToSet != -1) return;

                                    indexToSet = -3;

                                    //indexToSet = elements[index].BlockIndex[bestIndex];
                                    //bestRC = ChoosenCenter;

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
                                        var blocksCopyLocal = PlanSimulatorPool.CopyBlocksSwallow(opersBlocksCopy);

                                        PlanSimulatorPool.MoveCentersInAllBlocks(blocksCopyLocal, bestIndex, IndexToWorkWith);

                                        var blockIndex = elements.Length == 0 ? MIN : (index < elements.Length) ? (elements[index].BlockIndex[bestIndex]) : (elements[index - 1].BlockIndex[bestIndex] + 1);

                                        var indexHere = blockIndex;

                                        var indexToRemove = block.BlockIndex[bestIndex];

                                        blocksCopyLocal.RemoveAt(indexToRemove);

                                        if (indexToRemove < indexHere) indexHere--;

                                        blocksCopyLocal.Insert(indexHere, block);

                                        block.SetCenter(ChoosenCenter, IndexToWorkWith);
                                        block.PoolLink.UpdateOpsOnOneRC(block, IndexToWorkWith);

                                        var result = PlanSimulatorEngineResult.CreateFullInstance(SessionLink, MinAcceptableDate);

                                        result = PlanSimulatorEngine.CalculatePlan(blocksCopyLocal, IndexToWorkWith, MinAcceptableDate, result);

                                        result.StagesWithBrokenFixedNaladLengths = result.StagesWithBrokenFixedNaladLengths.Except(IgnoredGlobalFixedNaladLengthsErrors).Except(ignoredFixed).ToHashSet();

                                        var intersection = result.StagesWithBrokenFixedNaladLengths.Except(res.StagesWithBrokenFixedNaladLengths);
                                        var iHaveIt = result.StagesWithBrokenFixedNaladLengths.Contains(guidToFind);
                                        if (intersection.Count() == 0 && !iHaveIt)
                                        {
                                            if (indexToSet == -1)
                                            {
                                                lock (locker)
                                                {
                                                    indexToSet = blockIndex;
                                                    bestRC = ChoosenCenter;
                                                    UsedPool.Blocks = blocksCopyLocal;
                                                    UsedPool.BestPoolIndex = IndexToWorkWith;
                                                    pls.Stop();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //var badBlocks = intersection.Select(x => UsedPool.StagesLinks[x]).Select(x => new WasNow(x, bestIndex, IndexToWorkWith)).ToArray();
                                            //var myNaladInfo = UsedPool.StagesLinks[guidToFind];
                                            //var howItWas = new WasNow(myNaladInfo, bestIndex, IndexToWorkWith);
                                            //;
                                        }

                                        AvailableIndexes.Enqueue(IndexToWorkWith);
                                    }

                                }


                            });
                            while (!parr.IsCompleted)
                            {
                                if (indexToSet != -1) break;
                                ;
                            }
                        });
                        //for (int group = 0; group < possible.Length; group++)
                        //{
                        //}
                        while (!parr0.IsCompleted)
                        {
                            if (indexToSet != -1) break;
                            ;
                        }
                    }
                    else
                    {
                        indexToSet = -2;
                    }
                }
            }
            else
            {
                indexToSet = -2;
            }



            if (indexToSet < 0)
                ignoredFixed.Add(guidToFind);

            var dt02 = DateTime.Now;

            Console.WriteLine($"iter: {dt02 - dt01}, index {indexToSet} and RC {bestRC}; [{(string.Join(",", opToCheck.Stage.Index))}]; total {res.StagesWithBrokenFixedNaladLengths.Count}, ignored {IgnoredGlobalFixedNaladLengthsErrors.Count} + {ignoredFixed.Count}, total {ignoredFixed.Count + IgnoredGlobalFixedNaladLengthsErrors.Count}");

            return indexToSet >= 0;
        }

        //class WasNow
        //{
        //    public Uco РЦБыл, РЦСтал;
        //    public PlanSimulatorNaladLengthData ДлиныБыли, ДлиныСтали;
        //    public int ИндексБыл, ИндексСтал;
        //    public PlanSimulatorBlock ПредБлокБыл, ПредБлокСтал;

        //    public WasNow(PlanSimulatorStage stage, int best, int now)
        //    {
        //        РЦБыл = stage.ModLink.BlockLink.GetCenterData(best).Center;
        //        РЦСтал = stage.ModLink.BlockLink.GetCenterData(now).Center;
        //        ДлиныБыли = stage.NaladLengthData[best];
        //        ДлиныСтали = stage.NaladLengthData[now];
        //        ИндексБыл = stage.ModLink.BlockLink.BlockIndex[best];
        //        ИндексСтал = stage.ModLink.BlockLink.BlockIndex[now];
        //        ПредБлокБыл = stage.ModLink.BlockLink.PreviousBlockOnThisRC[best];
        //        ПредБлокСтал = stage.ModLink.BlockLink.PreviousBlockOnThisRC[now];
        //    }
        //}



        #endregion

    }
}

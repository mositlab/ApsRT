using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ApsRT.Core.Partias.PartiasClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Azure;
using ApsRT.Core.DataErrors;
using ApsRT.Core.Session;
using ApsRT.Core.Orders;
using ApsRT.Core.Stages;
using UniversalPack.Common.Drawing;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Drawing;
using ApsRT.Core.Partias.TaraSearchEngine;
using UniversalPack;
using ApsRT.Core.Partias.Leftovers;
using System.Collections.Concurrent;
using ApsRT.Core.Partias.PartiasConnectorEngine;
using System.Diagnostics.Metrics;
using ApsRT.Core.Settings;
using UniversalPack.Common;
using ApsRT.Core.Planning.PlanningClasses;
using System.Data;
using UniversalPack.Extensions;
using ApsRT.Core.Planning.PlanningAlgorithm;
using ApsRT.Core.NaladLengths;

namespace ApsRT.Core.Partias.PartiasAlgorithm
{
    public class PartiasAlgorithm
    {

        public Session.Session SessionLink { get; }

        public PartiasAlgorithm(Session.Session session)
        {
            SessionLink = session;
            CurrentStage = PartiasAlgorithmStage.None;
        }

        public PartiasAlgorithmStage CurrentStage { get; private set; }

        public event Action<PartiasAlgorithmStage, int> CurrentStageChanged;

        public event Action<PartiasAlgorithmStage, int> AlgorithmCallback;

        public Task<bool> Process { get; private set; }

        public bool Start(bool WithLog)
        {
            if (CurrentStage == PartiasAlgorithmStage.None)
            {
                if (SessionLink.PlanningAlgorithmIsNotRunning)
                {
                    // Да, можно.
                    Process = Task.Run(() => AlgorithmProcess(WithLog));
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

        bool AlgorithmProcess(bool WithLog)
        {
            try
            {
                var dateOfStart = DateTime.Now;

                // 1. Готовим сессию
                CurrentStage = PartiasAlgorithmStage.PreparingSession;
                CurrentStageChanged?.Invoke(CurrentStage, 0);
                PrepareSession();

                // 2. Готовим партии (и 3 - заделы)
                CurrentStage = PartiasAlgorithmStage.PreparingPartias;
                CurrentStageChanged?.Invoke(CurrentStage, 0);
                PreparePartias(WithLog);

                // 3. ЗАДЕЛЫ
                CurrentStage = PartiasAlgorithmStage.PreparingZadels;
                CurrentStageChanged?.Invoke(CurrentStage, 0);
                SessionLink.MasterDataLinks.ZadelsMasterData.PreparePartiasForZadels(PartiiList);

                // 4. Подготавливаем партии к остаткам
                CurrentStage = PartiasAlgorithmStage.PreparingLeftovers;
                CurrentStageChanged?.Invoke(CurrentStage, 0);
                SessionLink.MasterDataLinks.LeftoversMasterData.PreparePartiasForLeftovers(PartiiList);
                //SessionLink.MasterDataLinks.LeftoversMasterData.OutputLeftovers(PartiiList); // И сразу же их и выводим

                // 5. Готовим таблицу дедлайнов
                CurrentStage = PartiasAlgorithmStage.FiilingDeadlineTables;
                CurrentStageChanged?.Invoke(CurrentStage, 0);
                PrepareDeadlinesTable();

                // 6. Готовим таблицу глубин
                CurrentStage = PartiasAlgorithmStage.FillingDeepTables;
                CurrentStageChanged?.Invoke(CurrentStage, (allGPToCheckNow.Length * allGPToCheckNow.Length - allGPToCheckNow.Length) / 2);
                PrepareDeepTable(WithLog);

                // 7. Непосредственно поиска
                CurrentStage = PartiasAlgorithmStage.Searching;
                int maxDeads = DEAD.Sum(x => x.Value.Count);
                CurrentStageChanged?.Invoke(CurrentStage, maxDeads);
                SearchProcess(WithLog, maxDeads);

                var fin = PartiiList.Where(x => x.OpersHere[0].Count > 0).ToArray();

                CurrentStage = PartiasAlgorithmStage.AfterSearchErrorsCheck;
                CurrentStageChanged?.Invoke(CurrentStage, fin.Length);
                AfterSearchErrorsCheck(fin);

                CurrentStage = PartiasAlgorithmStage.InsidePartiasOptimization;
                CurrentStageChanged?.Invoke(CurrentStage, fin.Length);
                InsidePartiasOptimization(fin, WithLog);

                CurrentStage = PartiasAlgorithmStage.FormingEasyFlags;
                CurrentStageChanged?.Invoke(CurrentStage, fin.Length);
                var easyFlags = CreateEasyFlags(fin);

                CurrentStage = PartiasAlgorithmStage.Output;
                CurrentStageChanged?.Invoke(CurrentStage, fin.Length);
                WriteToSession(fin, easyFlags);

                var dateOfEnd = DateTime.Now;

                Console.WriteLine($"partias algorithm time estimated: {dateOfEnd - dateOfStart}");

                // Вызов функции глобального вывода
                try
                {
                    SessionLink.SessionOutputEngine.Output(ForcedToRecreate: true);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Что-то не то с выводом данных после партий: {e.Message}");
                }

                if (fin.Length == 0 | (SessionLink.SettingsController.CurrentSettings.LogErrors && SessionLink.DataErrors.TryOutputDataErrors())) // если логи велись, и я что-то вывел...
                {
                    // То выходим с кодом 200. В партиях ошибки, и построить правильный план не выйдет.
                    CurrentStage = PartiasAlgorithmStage.Failed;
                    CurrentStageChanged?.Invoke(CurrentStage, 0);
                    if (SessionLink.SessionOptions.IsSilent)
                        SessionLink.EndSession(200); // TODO: а что если я только партии формирую то что тогда?
                    return true;
                }
                CurrentStage = PartiasAlgorithmStage.Complete;
                CurrentStageChanged?.Invoke(CurrentStage, 1);
                return true;
            }
            catch (Exception ex)
            {
                CurrentStage = PartiasAlgorithmStage.Failed;
                CurrentStageChanged?.Invoke(CurrentStage, 0);
                return false;
            }
        }

        void PrepareSession()
        {
            foreach (var op in SessionLink.MasterDataLinks.StagesMasterData.AllStages.Values)
            {
                op.IsCustomModelling = false;
                op.CustomModelling = Guid.Empty;
                op.Center = op.OriginalCenter;
                op.CurrentTara = op.EndTara;
                op.CurrentTaraMaxCapacity = 0;
                op.EasyFlag = op.EasyFlagOriginal;
                op.CustomPartiaGuid = null;
                op.StageMaterialsInfo.UsedLeftovers = op.StageMaterialsInfo.UsedMaterials.Select(x => (Leftovers.StageLeftover)null).ToList();

            }

            SessionLink.MasterDataLinks.StagesMasterData.ApsModellingsInfo.Clear();

            SessionLink.MasterDataLinks.ZadelsMasterData.ClearZadelsUsageInfo();
        }

        List<PartiaClass> PartiiList;

        Dictionary<OrderID, List<List<PartiaModelling>>> OriginalCopies;
        Dictionary<OrderID, PartiaClass> orderToPartia;

        public Dictionary<Guid, bool> PartiaHasErrors { get; private set; } = new(); // пока оставил словарь, вдруг что-то ещё понадобится

        public bool ThisOperHasPartiaErrors(Stage op)
        {
            if (op.CustomPartiaGuid.HasValue)
            {
                if (PartiaHasErrors.TryGetValue(op.CustomPartiaGuid.Value, out var err))
                {
                    return err;
                }
            }
            else
            {
                var Pf =op.OrderLink.PartiaPF;
                if (Pf.HasValue)
                {
                    if (PartiaHasErrors.TryGetValue(Pf.Value, out var err))
                    {
                        return err;
                    }
                }
            }
            return false;
        }

        public Dictionary<Guid, (List<List<PartiaModelling>> List, int GPsCount, float K)> SavedPartiasTrees = new();
        public Dictionary<Guid, HashSet<int>> SavedPartias { get; private set; } = new();

        SortedList<double, List<(PartiaClass from, PartiaClass to)>> DEAD = new();
        ConcurrentDictionary<PartiaClass, ConcurrentDictionary<PartiaClass, (int Total, float K, int Diff, int LongestModsMaxCount)>> DEEP = new(); // TODO: нужен оптимальный concurrencyLevel (поискать в IlSpy)

        void PreparePartias(bool WithLog)
        {
            PartiiList = new();

            // Составляем список всего и вся

            // 1.1 Создаём хранилище для копий - оно понадобится в следующем шаге. Это чтобы не вызывать постоянно GenerateTree.
            // Те партии, что есть тут, будут пытаться схлопнуться в дальнейшем.
            // Заблоченные партии сюда не попадают.
            var orders = SessionLink.MasterDataLinks.OrdersMasterData.AllOrders;
            int count = orders.Count;
            var PFPartiasData = SessionLink.MasterDataLinks.OrdersMasterData.PFPartiasData;
            OriginalCopies = new Dictionary<OrderID, List<List<PartiaModelling>>>(count);

            // А это = небольшой словарь, который хранит ссылки на полученные только что элементарные партии. Он нужен, чтобы сформировать на первом этапе таблицы глубины и дедлайнов, потом уже не понадобится.
            orderToPartia = new Dictionary<OrderID, PartiaClass>(count);

            // Сначала найдем заказы, которые полностью элементарны. Их можно объединять без проблем
            var elementarGPs = orders.Where(x => !x.Value.PartiaPF.HasValue);
            foreach (var order in elementarGPs)
            {
                // Каждый заказ это одна партия здесь
                CreatePartiasAndAddToList(new List<KeyValuePair<OrderID, OrderBaseModel>>() { order }, 0, false, Guid.NewGuid(), WithLog);
            }

            // 1.2 В зависимости от типа запуска АПС, манера может быть разной.
            if (SessionLink.SessionOptions.PartOrZadel)
            {
                // Ситуация схожая с классикой - находим те, что свободны от ограничений, и юзаем их.

                // Сначала найдем норм гпшки, которые свободны от всего. При этом партии будут иметь флаги, чтобы объединяться только между собой.
                var elementarGPsGood = orders.Where(x => x.Value.PartiaPF.HasValue && !PFPartiasData[x.Value.PartiaPF.Value].IsNotOnlyPartFree);

                // Группируем по партиям ПФ - в этом режиме их можно схлопывать только между собой.
                var goodGroups = elementarGPsGood.GroupBy(x => x.Value.PartiaPF);
                int PfFlag = 1;
                foreach (var oneGroup in goodGroups)
                {
                    // Одна группа. У нее есть гуид, по которому их сгруппировали
                    foreach (var orderInGroup in oneGroup)
                    {
                        CreatePartiasAndAddToList(new List<KeyValuePair<OrderID, OrderBaseModel>>() { orderInGroup }, PfFlag, false, oneGroup.Key.Value, WithLog); // Флаг: PfFlag (схлопывание между своими), НЕ ограничено
                    }
                    PfFlag++;
                }

                // Те, что оказались несвободны - записать в лог и исключить из формирования, при этом инфа о них появится в логах
                PfFlag = -1;
                var elementarGPsBadGroups = orders
                    .Where(x => x.Value.PartiaPF.HasValue && PFPartiasData[x.Value.PartiaPF.Value].IsNotOnlyPartFree)
                    .GroupBy(X => X.Value.PartiaPF.Value);
                foreach (var elementarGPsBad in elementarGPsBadGroups)
                {
                    var partiaPf = elementarGPsBad.Key;
                    foreach (var order in elementarGPsBad)
                    {
                        CreatePartiasAndAddToList(new List<KeyValuePair<OrderID, OrderBaseModel>>() { order }, PfFlag, true, partiaPf, WithLog); // Флаг: PfFlag (схлопывание невозможно), ограничено

                        var partiaClass = PFPartiasData[partiaPf];
                        SessionLink.AssignedLogger.PartiasLogQueue.Enqueue($"[{DateTime.Now}] ТОЛЬКО ПАРТИИ: Обнаружена партия {partiaPf}, Заказ: '{order.Key.Object}', ГП '{order.Key.Version}'," +
                            $" данные: VPZ {partiaClass.VPZ}, Fact {partiaClass.IsFact}, NoReplan {partiaClass.IsNotFree}, считается ошибочной.\r\n");
                        // TODO: гуид партии пусть остаётся
                    }

                    PfFlag--;
                }
            }
            else
            {
                // Это - работа по старинке. Все элементарные партии находим и соединять можем, а заблокированные не трогаем.

                // Сначала - получаем свободные ГПшки
                var elementarGPsWithPF = orders.Where(x => x.Value.PartiaPF.HasValue && !PFPartiasData[x.Value.PartiaPF.Value].IsNotFree);
                foreach (var element in elementarGPsWithPF)
                {
                    CreatePartiasAndAddToList(new List<KeyValuePair<OrderID, OrderBaseModel>>() { element }, 0, false, Guid.NewGuid(), WithLog); // Флаг: 0 (свободное схлопывание), НЕ ограничено, гуид рандомный назначит partiaOutput
                }

                // Теперь получаем те, что уже с партиями ПФ
                var allPFGroups = orders
                    .Where(x => x.Value.PartiaPF.HasValue && PFPartiasData[x.Value.PartiaPF.Value].IsNotFree)
                    .GroupBy(x => x.Value.PartiaPF.Value);
                int PfFlag = 1;
                foreach (var allPF in allPFGroups)
                {
                    //foreach (var concretePF in allPF)
                    //{
                    // Условие: эта партия ни с кем не схлопывается, и гуид берет из пф.
                    // В таком случае все заказы из этой группы превратятся в одну большую партию
                    CreatePartiasAndAddToList(allPF, PfFlag, true, allPF.Key, WithLog); // Гуид из партии ПФ, ОГРАНИЧЕН, да и флаг 1+
                    PfFlag++;
                    //}
                }
            }

        }

        public List<Stage> FindStagesToProcess(Guid? savedPartiaPF, OrderID? order)
        {
            IEnumerable<int> ids = null;
            var orders = SessionLink.MasterDataLinks.OrdersMasterData.AllOrders;
            if (savedPartiaPF.HasValue && SavedPartias.TryGetValue(savedPartiaPF.Value, out var hash))
            {
                // Ищем по айдишнику
                ids = hash;
            }
            else
            {
                if (order.HasValue)
                {
                    // Ищем тупо по ключу
                    ids = orders[order.Value].RelatedOpers;
                }
            }
            if (ids == null) return null;
            var ops1 = ids.Select(x => SessionLink.MasterDataLinks.StagesMasterData.AllStages[x]).ToList();
            return ops1;
        }

        void CreatePartiasAndAddToList(IEnumerable<KeyValuePair<OrderID, OrderBaseModel>> orders, int flag, bool restrictIt, Guid guidToSet, bool WithLog)
        {
            var ops = new List<Stage>();

            bool LogErrors = SessionLink.SettingsController.CurrentSettings.LogErrors;
            var dataErrors = SessionLink.DataErrors;

            foreach (var ord in orders)
            {
                var opsHere = FindStagesToProcess(null, ord.Key);
                if (opsHere != null && opsHere.Count > 0)
                {
                    if (LogErrors)
                    {
                        if (string.IsNullOrEmpty(ord.Key.Version))
                        {
                            dataErrors.AddNewPartiaError(opsHere[0].Index, NewDataErrorType.NoGP, new DataErrorStruct());
                            if (LogErrors)
                            {
                                PartiaHasErrors.TryAdd(guidToSet, true);
                                PartiaHasErrors[guidToSet] = true;
                            }
                        }
                    }
                    ops.AddRange(opsHere);
                }
            }

            if (ops.Count > 0)
            {
                if (ops.All(x => x.EndTara.IsEmpty))
                {
                    dataErrors.AddNewPartiaError(ops[0].Index, NewDataErrorType.NoOrigTara, new DataErrorStruct());
                    if (LogErrors)
                    {
                        PartiaHasErrors.TryAdd(guidToSet, true);
                        PartiaHasErrors[guidToSet] = true;
                    }
                }
                var tree = PartiasGeneratorEngine.PartiasGeneratorEngine.GenerateTree(ops, Colors.NewRandomColor(), WithLog, guidToSet);
                if (tree != null)
                {
                    var partiaL = new PartiaClass(tree, orders.Max(x => x.Value.Deadline), restrictIt, flag, guidToSet);
                    if (!restrictIt)
                    {
                        // Если партия не заблокированная, то добавляется в список копий, с которыми пойдёт дальше.
                        foreach (var ord in orders)
                        {
                            OriginalCopies.Add(ord.Key, tree);
                            orderToPartia.Add(ord.Key, partiaL);
                        }
                    }
                    PartiiList.Add(partiaL);
                }
            }
        }

        OrderID[] allGPToCheckNow;

        void PrepareDeadlinesTable()
        {
            allGPToCheckNow = OriginalCopies.Select(x => x.Key).ToArray(); // Выписываем те, что сейчас будем проверять

            var orders = SessionLink.MasterDataLinks.OrdersMasterData.AllOrders;

            for (var i = 0; i < allGPToCheckNow.Length; i++)
            {
                for (var j = 0; j < allGPToCheckNow.Length; j++)
                {
                    if (i <= j)
                    {
                        continue;
                    }

                    if (orderToPartia[allGPToCheckNow[i]].MergeFlag != orderToPartia[allGPToCheckNow[j]].MergeFlag) continue;

                    if (orderToPartia[allGPToCheckNow[i]].IsRestricted || orderToPartia[allGPToCheckNow[j]].IsRestricted) continue;

                    var days = orders[allGPToCheckNow[i]].Deadline - orders[allGPToCheckNow[j]].Deadline;
                    var daysd = Math.Abs(days.TotalDays);

                    if (!DEAD.ContainsKey(daysd))
                    {
                        DEAD.Add(daysd, new());
                    }

                    DEAD[daysd].Add((orderToPartia[allGPToCheckNow[i]], orderToPartia[allGPToCheckNow[j]]));

                }
            }
        }

        void PrepareDeepTable(bool WithLog)
        {
            var orders = SessionLink.MasterDataLinks.OrdersMasterData.AllOrders;
            int N = 0;
            var res0 = Parallel.For(0, allGPToCheckNow.Length, i =>
            {
                var res = Parallel.For(0, allGPToCheckNow.Length, j =>
                {
                    if (
                    //    allElemenetal.Length == 1 || // новое условие, нужно его проверить, если что - закомментить
                    //allElemenetal.Length > 1 &&
                    i <= j)
                    {
                        return;
                    }

                    if (orderToPartia[allGPToCheckNow[i]].MergeFlag != orderToPartia[allGPToCheckNow[j]].MergeFlag) return;

                    if (orderToPartia[allGPToCheckNow[i]].IsRestricted || orderToPartia[allGPToCheckNow[j]].IsRestricted) return;

                    var prt1 = orderToPartia[allGPToCheckNow[i]];
                    var prt2 = orderToPartia[allGPToCheckNow[j]];

                    var partiaClass1 = new PartiaClass(PartiasGeneratorEngine.PartiasGeneratorEngine.DeepCopy(OriginalCopies[allGPToCheckNow[i]]), orders[allGPToCheckNow[i]].Deadline, /*new List<int>() { 0 }, new List<string>() { allElemenetal[i].ToString() },*/ prt1.IsRestricted, prt1.MergeFlag, prt1.MyGuid);
                    var partiaClass2 = new PartiaClass(PartiasGeneratorEngine.PartiasGeneratorEngine.DeepCopy(OriginalCopies[allGPToCheckNow[j]]), orders[allGPToCheckNow[j]].Deadline, /*new List<int>() { 1 }, new List<string>() { allElemenetal[j].ToString() },*/ prt2.IsRestricted, prt2.MergeFlag, prt2.MyGuid);


                    var res = PartiasConnectorEngine.PartiasConnectorEngine.Connect(partiaClass1, partiaClass2, WithLog, false);

                    if (!DEEP.ContainsKey(prt1))
                    {
                        DEEP.TryAdd(prt1, new());
                    }

                    DEEP[prt1].TryAdd(prt2, (res.Total, res.FilledK, res.Diff, res.LongestModsMaxCount));// res.K - был недавно, и работало

                    N++;
                    AlgorithmCallback?.Invoke(CurrentStage, N);
                }
                );
                while (!res.IsCompleted)
                {
                    ;
                }
            }
    );
            while (!res0.IsCompleted)
            {
                ;
            }
        }

        void SearchProcess(bool WithLog, int maxdeads)
        {
            var diapazon = SessionLink.SettingsController.CurrentSettings.MaxDaysBetweenPartias;
            var logQueue = SessionLink.AssignedLogger.PartiasLogQueue;
            object localLocker = new();
            var count = 0;
            while (true)
            {
                if (SessionLink.SessionOptions.IsSilent)
                {
                    logQueue.Enqueue($"Partia: {count}\r\n");
                }

                //SortedList<int, SortedList<float, List<(PartiaClass from, PartiaClass to)>>> localDEEP = new();
                // кол во корзин, кол во гп, затем кол во барабанов
                SortedList<int, SortedList<int, SortedList<int, List<(PartiaClass from, PartiaClass to)>>>> localDEEP = new();

                void addDeepLocal(double valday)
                {
                    foreach (var deadL in DEAD[valday])
                    {
                        var val = DEEP.ContainsKey(deadL.from) ? DEEP[deadL.from][deadL.to] : DEEP[deadL.to][deadL.from];
                        //Хотел: 1 - колво корзин, 2 - разница в гп, 3 - колво барабанов
                        var effiencyHere = val.Total;
                        var gpCount = Math.Abs(deadL.from.GPsCountNow - deadL.to.GPsCountNow);//val.K;
                        var korzMax = val.LongestModsMaxCount;//deadL.from.LongestModsMaxCount + deadL.to.LongestModsMaxCount;//Math.Max(deadL.from.LongestModsMaxCount, deadL.to.LongestModsMaxCount);//val.K;
                        if (val.Total > 0)
                        {
                            if (!localDEEP.ContainsKey(korzMax))
                            {
                                localDEEP.Add(korzMax, new());
                            }
                            if (!localDEEP[korzMax].ContainsKey(gpCount))
                            {
                                localDEEP[korzMax].Add(gpCount, new());
                            }
                            if (!localDEEP[korzMax][gpCount].ContainsKey(effiencyHere))
                            {
                                localDEEP[korzMax][gpCount].Add(effiencyHere, new());
                            }

                            localDEEP[korzMax][gpCount][effiencyHere].Add(deadL);
                        }
                    }
                }

                // 1. Сначала найдем те, что равны 0
                if (DEAD.ContainsKey(0) && DEAD[0].Count > 0)
                {
                    addDeepLocal(0);
                }

                if (localDEEP.Count == 0)
                {
                    // 2. Потом все остальные
                    foreach (var val in DEAD.Keys)
                    {
                        if (val > diapazon)
                        {
                            break;
                        }

                        if (DEAD[val].Count > 0)
                        {
                            addDeepLocal(val);
                        }
                    }
                }

                // 3. Если не нашли, то выходим
                if (localDEEP.Count == 0)
                {
                    break;
                }

                var partia1 = localDEEP.First().Value.Last().Value.Last().Value[0].from;
                var partia2 = localDEEP.First().Value.Last().Value.Last().Value[0].to;

                // 4. Нам приходится делать копию: на случай, если схлопнется не полностью, или вовсе испортится (такое бывает)
                var partia1C = new PartiaClass(PartiasGeneratorEngine.PartiasGeneratorEngine.DeepCopy(partia1.OpersHere), partia1.PossibleDeadline, /*new(partia1.ModelsHere), new(partia1.OrdersHere), */partia1.IsRestricted, partia1.MergeFlag, partia1.MyGuid, partia1.GPsCountNow/*, partia1.LongestModsMaxCount*/);
                var partia2C = new PartiaClass(PartiasGeneratorEngine.PartiasGeneratorEngine.DeepCopy(partia2.OpersHere), partia2.PossibleDeadline, /*new(partia2.ModelsHere), new(partia2.OrdersHere), */partia2.IsRestricted, partia2.MergeFlag, partia2.MyGuid, partia2.GPsCountNow/*, partia2.LongestModsMaxCount*/);
                var final = PartiasConnectorEngine.PartiasConnectorEngine.Connect(partia1C, partia2C, WithLog, false);
                var finalDead = partia1C.PossibleDeadline;
                if (final.Total > 0) // И в случае успеха, мы ссылку на partia1 юзаем и копируем туда полученные данные
                {
                    partia1.GetDataFromOtherPartia(partia1C);
                    partia2.GetDataFromOtherPartia(partia2C);
                }

                // 5.1 нужно проверить значения final

                // 5.2 - Успешно схлопнулся, значит нужно удалить только вторую партию, а первую пересчитать.
                // По факту, из-за пересчёта проще удалить обе партии и затем пересчитать всё заново.

                // 5.3 - неуспешно, значит просто эти две элементарные партии удаляем (ну раз они не смогли схлопнуться даже с самой близкой)

                // Поэтому - первым делом удаляем из обоих списков упоминания об этих партиях

                // Чистим список
                var keysHere = DEAD.Keys.ToArray(); // 30-31 мая тут замечены вылеты. На всякий случай сделал так.
                var foreach0 = Parallel.ForEach(keysHere, x =>
                {
                    //lock (locker)
                    DEAD[x] = DEAD[x].Where(y => y.from != partia1 && y.from != partia2 && y.to != partia1 && y.to != partia2).ToList();
                });
                while (!foreach0.IsCompleted)
                {
                    ;
                    //Thread.Sleep(100);
                }

                // Чистим словари
                var foreach1 = Parallel.ForEach(DEEP, x =>
                {
                    if (x.Value.ContainsKey(partia1))
                    {
                        x.Value.Remove(partia1, out _);
                    }

                    if (x.Value.ContainsKey(partia2))
                    {
                        x.Value.Remove(partia2, out _);
                    }
                });
                while (!foreach1.IsCompleted)
                {
                    ;
                }

                if (DEEP.ContainsKey(partia1))
                {
                    DEEP.Remove(partia1, out _);
                }

                if (DEEP.ContainsKey(partia2))
                {
                    DEEP.Remove(partia2, out _);
                }

                // 5.5 - Теперь проверка и добавление новых ссылок
                if (final.Total > 0)
                {
                    //var orders = partia1.OpersHere
                    //    .SelectMany(y => y.SelectMany(z => z.Opers.Select(w => w.oper.OrderN)))
                    //    .Union(partia2.OpersHere
                    //        .SelectMany(y => y.SelectMany(z => z.Opers.Select(w => w.oper.OrderN)))
                    //        )
                    //    .Distinct().ToList();

                    //foreach (var ord in orders)
                    //{
                    //    Economy[ord] = Math.Max(Economy[ord], final.Total);
                    //}

                    //openedPartii.Add(partia1);
                    var thisCopy = PartiasGeneratorEngine.PartiasGeneratorEngine.DeepCopy(partia1.OpersHere);

                    var remains = PartiiList.Where(x => x.OpersHere[0].Count > 0).ToList();
                    //var remains = remains0.Distinct().ToList(); // плюс ещё partia1, которая сейчас объединена

                    if (remains.Count == 0)
                    {
                        break;
                    }

                    var resp0 = Parallel.ForEach(remains, remain =>
                    {
                        if (remain == partia1)
                        {
                            return;
                        }

                        var partia = new PartiaClass(PartiasGeneratorEngine.PartiasGeneratorEngine.DeepCopy(thisCopy), finalDead, /*new List<int>() { 0 }, new(partia1.OrdersHere),*/ partia1.IsRestricted, partia1.MergeFlag, partia1.MyGuid, partia1.GPsCountNow/*, partia1.LongestModsMaxCount*/); // копия
                        var partiaLocal = new PartiaClass(PartiasGeneratorEngine.PartiasGeneratorEngine.DeepCopy(remain.OpersHere), remain.PossibleDeadline, /*new List<int>() { 1 }, new(remain.OrdersHere), */remain.IsRestricted, remain.MergeFlag, remain.MyGuid, remain.GPsCountNow/*, remain.LongestModsMaxCount*/);

                        //var perc = FoundSecondsAndWin(
                        //    new PartiaClass(DeepCopy(thisCopy), finalDead, new List<int>() { 0 }),
                        //    new PartiaClass(DeepCopy(remain.OpersHere), remain.PossibleDeadline, new List<int>() { 1 })
                        //    , withUI, CheckChildren);

                        var dead = Math.Abs((partia.PossibleDeadline - partiaLocal.PossibleDeadline).TotalDays);

                        var mergedRes = PartiasConnectorEngine.PartiasConnectorEngine.Connect(partia, partiaLocal, WithLog/*, CheckChildren*/, false);

                        var key1 = (remain, partia1);

                        lock (localLocker)
                        {
                            if (!DEAD.ContainsKey(dead))
                            {
                                DEAD.Add(dead, new());
                            }

                            DEAD[dead].Add(key1);
                        }

                        if (!DEEP.ContainsKey(remain))
                        {
                            DEEP.TryAdd(remain, new());
                        }

                        DEEP[remain].TryAdd(partia1, (mergedRes.Total, mergedRes.FilledK, mergedRes.Diff, mergedRes.LongestModsMaxCount));
                    });
                    while (!resp0.IsCompleted)
                    {
                        ;
                    }
                    //foreach (var remain in remains)
                    //{

                    //}
                }

                var Count = DEAD.Sum(x => x.Value.Count);
                logQueue.Enqueue($"{Count} шт...\r\n");

                AlgorithmCallback?.Invoke(CurrentStage, maxdeads - Count);
                count++;
            }

        }

        /// <summary>
        /// Функция, проверяющая партии на ошибки до того, как они друг с другом соединятся. Так будет правильнее
        /// </summary>
        void AfterSearchErrorsCheck(PartiaClass[] partias)
        {
            var PFPartiasData = SessionLink.MasterDataLinks.OrdersMasterData.PFPartiasData;
            var dataErrors = SessionLink.DataErrors;

            HashSet<Guid> passedGuids = new();
            Dictionary<Guid, PartiaClass> lastPartiesWithThisGuid = new();

            Dictionary<Guid, int> firstOperInPartia = new(); // А этот словарь нужен, чтобы хранить первую операцию из партии, потому что это просит Алена.

            int i = 0;
            foreach (var res in partias)
            {
                var chosenPartiaGuid = res.MyGuid/* ?? Guid.NewGuid()*/;
                if (!PartiaHasErrors.ContainsKey(chosenPartiaGuid)) PartiaHasErrors.Add(chosenPartiaGuid, false);
                bool ThisPartiaIsRestricted = res.IsRestricted;

                // Партия элементарная, и это ПЛОХО, если она не единственная в группе.
                //bool ThisPartiaIsBadlyElementar = res.IsElementar && (!res.MyGuid.HasValue || (PFPartiasData.TryGetValue(res.MyGuid.Value, out var classData) && classData.RelatedGPs.Count > 1));

                // Сначала смотрим, не было ли её уже в списке чекнутых партий. Если была - значит она не смогла объединиться с другими, причины нужно искать в словаре Part_WhyNotMerged
                bool ThisPartiaAlreadyWAS = passedGuids.Contains(chosenPartiaGuid);

                // Теперь уже добавляем
                passedGuids.Add(chosenPartiaGuid);

                // Находим последнюю партию, если она была, и записываем новый результат
                PartiaClass lastPartia = null;
                if (ThisPartiaAlreadyWAS)
                {
                    lastPartia = lastPartiesWithThisGuid[chosenPartiaGuid];
                }
                lastPartiesWithThisGuid[chosenPartiaGuid] = res;


                if (!firstOperInPartia.ContainsKey(chosenPartiaGuid)) firstOperInPartia.Add(chosenPartiaGuid, res.OpersHere[0][0].Opers[0].Index);

                
                if (SessionLink.SettingsController.CurrentSettings.LogErrors)
                {
                    var list = res.OpersHere.SelectMany(x => x.SelectMany(y => y.Opers)).ToList();

                    if (SessionLink.SessionOptions.PartOrZadel) // Дополнительно чекаем ещё и это
                    {
                        // Если партия заблокирована, то выводим признаки, если есть
                        if (ThisPartiaIsRestricted)
                        {
                            var firstVPZ = list.FindIndex(x => x.VPZ);
                            if (firstVPZ != -1) dataErrors.AddNewPartiaError(list[firstVPZ].Index, NewDataErrorType.Restricted_VPZ, new DataErrorStruct());
                            var firstFact = list.FindIndex(x => x.IsFact);
                            if (firstFact != -1) dataErrors.AddNewPartiaError(list[firstFact].Index, NewDataErrorType.Restricted_Fact, new DataErrorStruct());
                            var firstFixed = list.FindIndex(x => x.Fixed);
                            if (firstFixed != -1) dataErrors.AddNewPartiaError(list[firstFixed].Index, NewDataErrorType.Restricted_NoReplan, new DataErrorStruct());

                            //AddPartiaError(res.OpersHere[0][0].Opers[0].ID, PartiaErrorLogType.PartiaIsRestricted);
                        }

                        //if (ThisPartiaIsBadlyElementar)
                        //{
                        //    AddPartiaError(res.OpersHere[0][0].Opers[0].Index, PartiaErrorLogType.PartiaIsStillElementar);
                        //}

                        if (ThisPartiaAlreadyWAS)
                        {
                            // Выбираем первые уровни у обоих партий.
                            var list1 = res.OpersHere.Find(x => x.Any(y => !y.Opers[0].IsVspom && y.ZadelCode == 0)) ?? res.OpersHere[0];
                            var list2 = lastPartia.OpersHere.Find(x => x.Any(y => !y.Opers[0].IsVspom && y.ZadelCode == 0)) ?? lastPartia.OpersHere[0];
                            List<PartiaModelling> ThisList, OtherList;

                            // Текущий список должен быть меньше, чем список предыдущей партии
                            if (list1.Count > list2.Count)
                            {
                                ThisList = list2;
                                OtherList = list1;
                            }
                            else
                            {
                                ThisList = list1;
                                OtherList = list2;
                            }

                            //var opsOther = OtherList.SelectMany(sdvig => sdvig.Opers.Select(x => (op: x, zadel: sdvig.ZadelCode))).ToArray();
                            bool hasAlreadyError = false;
                            foreach (PartiaModelling s in ThisList) // Для каждого из моделирований на уровне 1
                            {
                                if (s.Opers[0].IsVspom) continue; // Вспомогательные не смотрим

                                bool HasSets = true;
                                HashSet<string> data = new();

                                var oper = s.Opers[0];
                                var ord = oper.OrderLink.OrderID;

                                bool contains = false;

                                foreach (PartiaModelling s2 in OtherList)
                                {
                                    if (s2.Opers[0].IsVspom) continue;
                                    if (s2.ZadelCode != s.ZadelCode) continue; // Не совпадают заделы? тоже не смотрим

                                    var ress = SessionLink.MasterDataLinks.MergeConditionsMasterData.StagesEqualityCheck(oper, s2.Opers[0], out _, out var reason, out var sets);

                                    if (!ress)
                                    {
                                        data.Add(reason);
                                        HasSets &= sets;
                                    }
                                    else
                                    {
                                        // Попробуем их совместить
                                        var l = s.Opers.Union(s2.Opers).ToList();

                                        if (TaraSearchEngine.TaraSearchEngine.SumIsPossible(new PartiaModellingSmall(l), out _, out _, out _, out _, out _, out var searchData, new SumIsPossibleOptions(false) { LogSomeErrors = false }))
                                        {
                                            // Можно? ну тогда я не знаю...
                                            var dem = searchData.dataErrorMore;
                                            dem.PartiaGuid = chosenPartiaGuid;
                                            dataErrors.AddNewPartiaError(-1, NewDataErrorType.WasNotAbleToMergeDontKnow, dem);
                                        }
                                        else
                                        {
                                            // Нельзя. Записываем
                                            var dem = searchData.dataErrorMore;
                                            dem.PartiaGuid = chosenPartiaGuid;
                                            dem.customEtapsGuids = l.Select(x => x.GUID).ToList();
                                            if (l[0].RCRoute.IsEmpty)
                                            {
                                                dem.customRCroute = null;
                                                dem.customRC = l[0].Center;
                                            }
                                            else
                                            {
                                                dem.customRCroute = l[0].RCRoute;
                                                dem.customRC = null;
                                            }
                                            dem.Count = searchData.sum;
                                            dem.customVid = l[0].OperationVid;
                                            dem.customNomen = l[0].Nomenklature;
                                            dem.customProd = l[0].Production;
                                            dataErrors.AddNewPartiaError(-1, NewDataErrorType.WasNotAbleToMergeMax, dem);
                                        }
                                    }

                                    contains |= ress;
                                }

                                if (!contains)
                                {
                                    if (!hasAlreadyError)
                                    {
                                        var dem = new DataErrorStruct();

                                        dem.PartiaSettings = string.Join(", ", data);
                                        dem.PartiaGuid = chosenPartiaGuid;
                                        dem.customVid = oper.OperationVid;
                                        dem.customNomen = oper.Nomenklature;
                                        dem.customProd = oper.Production;
                                        dataErrors.AddNewPartiaError(-1, HasSets ? NewDataErrorType.WasNotAbleToMerge : NewDataErrorType.WasNotAbleToMergeNoSettings, dem);
                                        hasAlreadyError = true;
                                    }
                                }
                            }

                            //AddPartiaError(ALL[firstOperInPartia[chosenPartiaGuid]].ID, PartiaErrorLogType.PartiaWasThere);
                        }

                        if (/*ThisPartiaIsBadlyElementar || */ThisPartiaIsRestricted || ThisPartiaAlreadyWAS)
                        {
                            PartiaHasErrors[chosenPartiaGuid] = true; // это для CalculateIt важно
                        }
                    }
                }

                i++;
                AlgorithmCallback?.Invoke(CurrentStage, i);
            }
        }

        void InsidePartiasOptimization(PartiaClass[] fin, bool WithLog)
        {
            var COUNTER = 0;
            var resu = Parallel.ForEach(fin, el =>
            {
                var r = PartiasConnectorEngine.PartiasConnectorEngine.Connect(el, el, WithLog, true);
                if (SessionLink.SettingsController.SpecialSettings.OptimizeSubdivisionsInPartias) el.SubdivisiosOptimizer(r);
                Interlocked.Increment(ref COUNTER);
                AlgorithmCallback?.Invoke(CurrentStage, COUNTER);
            });
            while (!resu.IsCompleted)
            {
                ;
            }
        }

        Dictionary<Guid, Dictionary<int, Guid>> CreateEasyFlags(PartiaClass[] partias)
        {
            Dictionary<Guid, Dictionary<int, Guid>> result = new();
            // Флаг легкой жизни
            int stage = 0;
            foreach (var res in partias)
            {
                Dictionary<int, Guid> IDtoGroup = null;
                Dictionary<int, PartiaModelling> IDtoSdvig = res.OpersHere.SelectMany(x => x.SelectMany(y => y.Opers.Select(z => (z.Index, y)))).ToDictionary(x => x.Index, x => x.y);

                var opsForEasyLife = res.OpersHere.SelectMany(x => x).ToList();//res.OpersHere.SelectMany(x => x.SelectMany(y => y.Opers.Select(z => z.ID))).ToList();
                IDtoGroup = result.TryGetValue(res.MyGuid, out var dic) ? dic : new Dictionary<int, Guid>(opsForEasyLife.Count);

                result.TryAdd(res.MyGuid, IDtoGroup);

                while (opsForEasyLife.Count > 0)
                {
                    var sdvigF = opsForEasyLife[0];
                    var first = sdvigF.Opers[0];
                    List<PartiaModelling> IdsHere = new()
                {
                    opsForEasyLife[0]
                };
                    opsForEasyLife.RemoveAt(0);
                    var sps1 = sdvigF.Opers
                        .SelectMany(x => x.Prevs) // Берём ВСЕХ предков у моделирования
                        .Distinct() // убираем дубликаты
                        .Select(x => IDtoSdvig.TryGetValue(x, out var l) ? l : null) // превращаем в сдвиги
                        .ToList();
                    var nxt = sdvigF.Opers
                        .SelectMany(x => x.Nexts)
                        .Distinct()
                        .Select(x => IDtoSdvig.TryGetValue(x, out var l) ? l : null)
                        .ToList();
                    //var nxt = string.Join(",", first.Nexts.OrderBy(x => x));
                    for (var i = 0; i < opsForEasyLife.Count; i++)
                    {
                        var idhere = opsForEasyLife[i];
                        var idhere1 = idhere.Opers[0];

                        //ALL[idhere].Prevs.Select(x => IDtoSdvig.TryGetValue(x, out var l) ? l : null).ToList();
                        if (SessionLink.MasterDataLinks.MergeConditionsMasterData.StagesEqualityCheck(first, idhere1, out _, out _, out _, true) && first.Level == idhere1.Level) // общ этапам нужно обязательно чекать альт рц, поэтому true
                        {
                            if (first.IsFictive && first.NoChangeTara && idhere1.NoChangeTara)
                            {
                                // Если они фиктивные, и у них есть признак "не менять тару", то вместо классической проверки,
                                // достаточно проверить, чтоб гуиды общих этапов выше совпадали (и вообще были).
                                var sps0 = sdvigF.Opers
                                .SelectMany(x => x.Prevs); // Берём ВСЕХ предков у этого первого моделирования

                                var sps2 = idhere.Opers
                                    .SelectMany(x => x.Prevs) // Берём ВСЕХ предков у этого моделирования
                                    .Union(sps0) // СМЕШИВАЕМ с первым моделированием
                                    .Select(x => IDtoGroup.TryGetValue(x, out var l) ? l : (Guid?)null) // превращаем в номера общих этапов
                                    .Distinct().ToArray();
                                if (sps2.Length == 1 && sps2[0] != null) // Должен быть один одинаковый обший этап, и при этом не пустой
                                {
                                    IdsHere.Add(opsForEasyLife[i]);
                                    opsForEasyLife.RemoveAt(i);
                                    i--;
                                }
                            }
                            else
                            {
                                // Рабоаем по старинке, сравниваем все условия.
                                var nxt2 = idhere.Opers
                                .SelectMany(x => x.Nexts)
                                .Distinct()
                                .Select(x => IDtoSdvig.TryGetValue(x, out var l) ? l : null)
                                .ToList();
                                //ALL[idhere].Nexts.Select(x => IDtoSdvig.TryGetValue(x, out var l) ? l : null).ToList();
                                if ((nxt.All(x => nxt2.Contains(x))
                                    &&
                                    nxt2.All(x => nxt.Contains(x)))
                                    || // Второй вариант - чекаем, чтоб они были схожи технически, у них были одинаковые РЦ (понятно с предыдущей проверки выше) + не менять тару у ребенка
                                    (nxt2.All(x => x.Opers[0].NoChangeTara))
                                    )
                                {
                                    var sps2 = idhere.Opers
                                    .SelectMany(x => x.Prevs) // Берём ВСЕХ предков у моделирования
                                    .Distinct() // убираем дубликаты
                                    .Select(x => IDtoSdvig.TryGetValue(x, out var l) ? l : null) // превращаем в сдвиги
                                    .ToList();
                                    //var inter = sps1.Intersect(sps2).Count();
                                    if (sps1.All(x => sps2.Contains(x))
                                        &&
                                        sps2.All(x => sps1.Contains(x))
                                        //first.Prevs.All(x => ALL[opsForEasyLife[i]].Prevs.Contains(x))
                                        //&&
                                        //ALL[opsForEasyLife[i]].Prevs.All(x => first.Prevs.Contains(x))
                                        )
                                    {
                                        IdsHere.Add(opsForEasyLife[i]);
                                        opsForEasyLife.RemoveAt(i);
                                        i--;
                                    }
                                }
                            }
                        }
                    }
                    if (IdsHere.Count > 1/* && IdsHere.Select(x => IDtoSdvig.TryGetValue(x, out var grp) ? grp : null).Distinct().Count() > 1*/)
                    {
                        var guid = Guid.NewGuid();
                        foreach (var ids in IdsHere)
                        {
                            foreach (var id in ids.Opers) IDtoGroup.Add(id.Index, guid);
                        }
                    }
                }

                stage++;
                AlgorithmCallback?.Invoke(CurrentStage, stage);
            }
            return result;
        }

        void WriteToSession(PartiaClass[] partias, Dictionary<Guid, Dictionary<int, Guid>> EasyFlagsInfo)
        {
            int stage = 0;
            foreach (var res in partias)
            {
                bool ThisPartiaIsRestricted = res.IsRestricted;
                bool CreateCustomMods = !ThisPartiaIsRestricted; // Кастомные моделирования можно создавать, только если партия была не зафиксированная. И это относится к part И partplan.

                var chosenPartiaGuid = res.MyGuid/* ?? Guid.NewGuid()*/;

                var IDtoGroup = EasyFlagsInfo == null ? null : (EasyFlagsInfo.TryGetValue(chosenPartiaGuid, out var l) ? l : null);

                SavedPartias.TryAdd(chosenPartiaGuid, new());

                SavedPartiasTrees.TryAdd(chosenPartiaGuid, (res.OpersHere, res.GPsCountNow, res.LastK));

                foreach (var lvl1 in res.OpersHere)
                {
                    foreach (var lvl2 in lvl1)
                    {
                        var modGuid = CreateCustomMods ? Guid.NewGuid() : lvl2.Opers[0].MesModelling.Code; // если ко мне пришла заблоченная партия, значит надо брать моделирования из меса. Для этого достаточно взять первую попавшуюся операцию из этого моделирования

                        if (CreateCustomMods)
                        {
                            SessionLink.MasterDataLinks.StagesMasterData.ApsModellingsInfo.Add(modGuid, (Colors.NewRandomColor(), new()));
                        }

                        for (var i = 0; i < lvl2.Opers.Count; i++)
                        {
                            var op1 = lvl2.Opers[i];

                            op1.Center = lvl2.RC;
                            op1.CurrentTara = lvl2.Tara;
                            op1.CurrentTaraMaxCapacity = lvl2.Max;

                            if (CreateCustomMods)
                            {
                                op1.IsCustomModelling = true; // ну если это без кастомных модов, то будет false
                                op1.CustomModelling = modGuid;
                                op1.EasyFlag = IDtoGroup != null ? (IDtoGroup.TryGetValue(op1.Index, out var guid) ? guid : null) : null;
                                //op1.EasyFlagIsFromMes = false;
                                op1.CustomPartiaGuid = chosenPartiaGuid;
                            }

                            // Записываем инфу об ошибках тары, если они есть
                            // отсутствие тары (только если операция не вспомогательная). Решено Аленой, 30.05.
                            if (SessionLink.SettingsController.CurrentSettings.LogErrors && lvl2.ZadelCode == 0 && !lvl2.Opers[0].IsVspom && lvl2.Tara.IsEmpty && !lvl2.Opers[0].NoCheckNamotka)
                            {
                                PartiaHasErrors[chosenPartiaGuid] = true;
                            }

                            SavedPartias[chosenPartiaGuid].Add(op1.Index);

                            if (CreateCustomMods)
                            {
                                SessionLink.MasterDataLinks.StagesMasterData.ApsModellingsInfo[modGuid].RelatedOpers.Add(op1.Index);
                            }
                        }
                    }
                }

                stage++;
                AlgorithmCallback?.Invoke(CurrentStage, stage);
            }
        }
    }
}

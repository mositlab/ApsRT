using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;
using System.Reflection.PortableExecutable;
using System.Collections.Frozen;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using LeftoverUco = (UniversalPack.Common.CodedObject<System.Guid, string> Nomenklature, UniversalPack.Common.CodedObject<System.Guid, string> Characteristics);
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ApsRT.Core.Orders;
using ApsRT.Core.Partias.PartiasClasses;
using ApsRT.Core.Planning.PlanningClasses;
using static System.Net.Mime.MediaTypeNames;
using ApsRT.Core.Leftovers;
using ApsRT.Core.Taras.TaraSearchEngine;
using ApsRT.Core.Partias.PartiasConnectorEngine;

namespace ApsRT.Core.Leftovers
{
    public class LeftoversMasterData
    {
        public Session.Session SessionLink { get; }

        public LeftoversMasterData(Session.Session session)
        {
            SessionLink = session;
        }

        /// <summary>
        /// Текущее количество остатков (номенклатура + характеристика)
        /// </summary>
        public Dictionary<LeftoverUco, Dictionary<Uco, double>> CurrentLeftoversCount { get; private set; } = new();

        public void ClearLeftoversCount()
        {
            CurrentLeftoversCount.Clear();
        }

        public void AddLeftoverCountData(LeftoverCountDataLoaderModel model)
        {
            LeftoverUco key = (model.Nomenklature, model.Characteristics);
            CurrentLeftoversCount.TryAdd(key, new());
            CurrentLeftoversCount[key].TryAdd(model.Subdivision, model.CountNow);
        }

        public Dictionary<LeftoverUco, List<(DateTime When, Uco Subdivision, double Count)>> LeftoversArrivalInfo { get; private set; } = new();

        public void ClearLeftoversArrivalData()
        {
            LeftoversArrivalInfo.Clear();
        }

        public void AddLeftoverArrivalData(LeftoverArrivalDataLoaderModel model)
        {
            LeftoverUco key = (model.Nomenklature, model.Characteristics);
            LeftoversArrivalInfo.TryAdd(key, new());
            LeftoversArrivalInfo[key].Add((model.DateArrival, model.Subdivision, model.CountArrival));
        }


        internal Dictionary<(OrderID Order, Uco Nomenklature, Uco Specification),
            Dictionary<Guid, LeftoversReplacementClass>
            > LeftoversGroupsInfo { get; private set; } = new();


        internal void AddLeftoversGroupsInfo(LeftoverGroupDataLoaderModel model)
        {
            (OrderID Order, Uco Nomenklature, Uco Specification) key = (model.Order, model.Nomenklature, model.Specification);
            LeftoversGroupsInfo.TryAdd(key, new());
            LeftoversGroupsInfo[key].TryAdd(model.MaterialsGroup, new());
            LeftoversGroupsInfo[key][model.MaterialsGroup].List.Add(new LeftoverReplacementInfo(model));
        }

        void FillPossibleAnalogueGroupsForMod(Dictionary<IAbleForTaraCalcInterface, Dictionary<Guid, LeftoversReplacementClass>> opsPossibleLeftoversForOne, IAbleForTaraCalcInterface mod)
        {
            if (!opsPossibleLeftoversForOne.ContainsKey(mod)) // Посмотрим, находил ли я для этой операции данные
            {
                Dictionary<Guid, LeftoversReplacementClass> dic = null;// GetReplacementInfo(op, x => true/*x.Value.Count == 1*/); // Найдём все группы возможной замены для этого этапчика, даже не взирая на то, что они могут быть групповыми

                foreach (var opMod in mod.AllOpers)
                {
                    var result = GetReplacementInfo(opMod, x => true/*x.Value.Count == 1*/);
                    dic = dic == null ? result : dic.IntersectBy(result.Select(x => x.Value.Hash), keySelector => keySelector.Value.Hash).ToDictionary();
                }

                opsPossibleLeftoversForOne.Add(mod, dic);
            }
        }


        Dictionary<LeftoverUco, double> FindAnaloguesAll(Dictionary<IAbleForTaraCalcInterface, Dictionary<Guid, LeftoversReplacementClass>> opsPossibleLeftoversForOne, IAbleForTaraCalcInterface mod, (LeftoverUco MatNomen, double MatWeight) item)
        {
            FillPossibleAnalogueGroupsForMod(opsPossibleLeftoversForOne, mod);

            // А это вообще обычная группа, которая работает, начиная с аналогов
            var possibleAnalogues = opsPossibleLeftoversForOne[mod]
                .SelectMany(x => x.Value.List) // У нас появятся вообще все группы
                .Where(x => x.Original == item.MatNomen) // Сначала выберем только те, у которых та же номенклатура, что и у предмета текущего

                .Select(analogue =>
                {
                    // Получаем инфу об аналоге
                    return (Key: analogue.Analogue, Value: item.MatWeight / analogue.OriginalCount * analogue.AnalogueCount); // Получаем пару: ключом служит мат-аналог, а значением - подсчитанное количество, посчитанное формулой
                })
                .Where(x => x.Key != item.MatNomen) // Уберем те, что равны моей номенклатуре, они нафиг не нужны
                .GroupBy(x => x.Key) // Теперь сгруппируем по этим самым остаткам-аналогам
                .ToDictionary(x => x.Key, x => x.Max(y => y.Value)); // Поздравляю, у нас появился словарь где есть инфа, какие остатки можно юзать, и какой вес у них будет из-за этого. Вес при этом берется максимальный

            return possibleAnalogues;
        }

        internal Dictionary<Guid, LeftoversReplacementClass> GetReplacementInfo(Stages.Stage op, Func<KeyValuePair<Guid, LeftoversReplacementClass>, bool> comparer)
        {
            var dicAll = new List<KeyValuePair<Guid, LeftoversReplacementClass>>();

            // Найдем группу заменяемости по полным данным

            // ООО
            var key1 = (op.OrderLink.OrderID, op.Nomenklature, op.Specification);
            if (LeftoversGroupsInfo.TryGetValue(key1, out var dic1))
                dicAll.AddRange(dic1.Where(comparer));

            // ООХ
            var key8 = (op.OrderLink.OrderID, op.Nomenklature, Uco.EmptyObject);
            if (LeftoversGroupsInfo.TryGetValue(key8, out var dic8))
                dicAll.AddRange(dic8.Where(comparer));

            // ОХО
            var key7 = (op.OrderLink.OrderID, Uco.EmptyObject, op.Specification);
            if (LeftoversGroupsInfo.TryGetValue(key7, out var dic7))
                dicAll.AddRange(dic7.Where(comparer));

            // ОХХ
            var key6 = (op.OrderLink.OrderID, Uco.EmptyObject, Uco.EmptyObject);
            if (LeftoversGroupsInfo.TryGetValue(key6, out var dic6))
                dicAll.AddRange(dic6.Where(comparer));

            // ХОО
            var key2 = (OrderID.EmptyObject, op.Nomenklature, op.Specification);
            if (LeftoversGroupsInfo.TryGetValue(key2, out var dic2))
                dicAll.AddRange(dic2.Where(comparer));

            // ХОХ
            var key4 = (OrderID.EmptyObject, op.Nomenklature, Uco.EmptyObject);
            if (LeftoversGroupsInfo.TryGetValue(key4, out var dic4))
                dicAll.AddRange(dic4.Where(comparer));

            // ХХО
            var key3 = (OrderID.EmptyObject, Uco.EmptyObject, op.Specification);
            if (LeftoversGroupsInfo.TryGetValue(key3, out var dic3))
                dicAll.AddRange(dic3.Where(comparer));

            // ХХХ
            var key5 = (OrderID.EmptyObject, Uco.EmptyObject, Uco.EmptyObject);
            if (LeftoversGroupsInfo.TryGetValue(key5, out var dic5))
                dicAll.AddRange(dic5.Where(comparer));

            return dicAll.ToDictionary(X => X.Key, x => x.Value);
        }


        //StreamWriter sw = new StreamWriter("Остатки.txt", false);
        //var matsToOutput = allItems.Select(x => (x.MatNomen, Material: x.stageLink.StageMaterialsInfo.UsedMaterials[x.MatIndex])).DistinctBy(x => x.MatNomen).ToList();
        //sw.WriteLine("Номенклатура_GUID" +
        //    "\tНоменклатура" +
        //    "\tКоличество" +
        //    "\tХарактеристика_GUID" +
        //    "\tХарактеристика" +
        //    "\tПодразделение_GUID" +
        //    "\tПодразделение" +
        //    "\tДатаПоступления" +
        //    "\tКоличествоПоступления");
        //foreach (var matEl in matsToOutput )
        //{
        //    var mat = matEl.MatNomen;
        //    var matClass = matEl.Material;
        //    sw.WriteLine($"{mat.Code}\t{mat.Label}\t{300}" +
        //        $"\t{matClass.Characteristics.Code}\t{matClass.Characteristics.Label}" +
        //        $"\t{Guid.Empty}\t" +
        //        $"\t");
        //}
        //sw.Close();


        public double GetMatSumFromMod(IAbleForTaraCalcInterface mod, LeftoverUco mat)
        {
            return mod.AllOpers.Sum(x =>
            {
                var index = x.StageMaterialsInfo.UsedMaterials.FindIndex(y => y.AsLeftoverData() == mat);
                if (index < 0) return 0;
                return x.StageMaterialsInfo.UsedMaterials[index].Count;
            });
        }

        public void PrepareModsForLeftovers(List<OpersBlock> ops)
        {
            
            PrepareModsForLeftovers(ops.SelectMany(block => block.Lists.SelectMany(list =>
            {
                if (list.ObshEtapData == null) return new List<IAbleForTaraCalcInterface>() { list as IAbleForTaraCalcInterface };
                return list.ObshEtapData.Select(x => x.Value.AsMod);
                //List<IAbleForTaraCalcInterface> result = new();
                //foreach (var modGuid in list.ObshEtapData)
                //{
                //    var range = new Range(modGuid.Value.FirstElementIndex, modGuid.Value.FirstElementIndex + modGuid.Value.Count);
                //    result.Add( new Partias.PartiasClasses.PartiaModellingSmall(list.OpersList.Take(range).ToList()));
                //}
                //return result;
            }
                /*
                 list => list.ObshEtapData != null ? ((list.ObshEtapData.Select(easy => new Partias.PartiasClasses.PartiaModellingSmall(list.OpersList.Take(new Range()))) : list) as IAbleForTaraCalcInterface)
                 */

                )
            ).ToList());
        }


        public void PrepareModsForLeftovers(List<IAbleForTaraCalcInterface> modsToArrange)
        {
            if (CurrentLeftoversCount == null || CurrentLeftoversCount.Count == 0) return;

            // Сначала всё очищаем
            foreach (var mod in modsToArrange)
            {
                var op = mod.FirstOperLink;
                op.StageMaterialsInfo.UsedLeftovers = op.StageMaterialsInfo.UsedMaterials.Select(x => (StageLeftover)null).ToList();
            }

            // Создадим решатель
            CpModel model = new CpModel();

            //var SessionLink = partias[0].OpersHere[0][0].Opers[0].SessionLink;

            // TO DO: А что делать с моделированиями из МЕС??? там же 2+ этапчика
            var allItems =

                modsToArrange

                //partias.SelectMany(x => PartiasConnectorEngine.SeeSdvigsAsList(x.OpersHere)) // Возьмём этапы со всех партий


                .Where(sdvig => sdvig.FirstOperLink.StageMaterialsInfo != null)
                .SelectMany(sdvig =>

                sdvig.FirstOperLink.StageMaterialsInfo.UsedMaterials.Select(
                    (mat, index) =>

                    (stageLink: sdvig.FirstOperLink, modLink: sdvig, MatData: mat.AsLeftoverData(), MatIndex: index, MatWeight: (int)(GetMatSumFromMod(sdvig, mat.AsLeftoverData()) * 1000.0))
                )
            )
                .Where(
                item =>
                //CurrentLeftoversCount.ContainsKey(item.MatNomen)
                //&&
                item.stageLink.StageMaterialsInfo.UsedMaterials[item.MatIndex].IsBuying) // Второе ограничение.
                .Select((x, index) => (elementIndex: index, x.stageLink, x.modLink, x.MatData, x.MatIndex, x.MatWeight))

                //.Take(5000)

                .ToList();

            Dictionary<IAbleForTaraCalcInterface, Dictionary<Guid, LeftoversReplacementClass>> opsPossibleLeftoversForOne = new Dictionary<IAbleForTaraCalcInterface, Dictionary<Guid, LeftoversReplacementClass>>();

            List<Dictionary<LeftoverUco, double>> AllPossibleAnaloguesForMats = new();
            for (int i = 0; i < allItems.Count; i++)
            {
                var item = allItems[i];
                var mod = item.modLink;

                var possibleAnalogues = FindAnaloguesAll(opsPossibleLeftoversForOne, mod, (item.MatData, item.MatWeight));

                AllPossibleAnaloguesForMats.Add(possibleAnalogues); // Добавляю информацию об этом в копию
            }

            bool anyItemHas(LeftoverUco what)
            {
                for (int i = 0; i < allItems.Count; i++)
                {
                    if (allItems[i].MatData == what) return true;
                    var dic = AllPossibleAnaloguesForMats[i];
                    if (dic.ContainsKey(what)) return true;
                }
                return false;
            }

            // Получаем собственно информацию
            var allBins = CurrentLeftoversCount.SelectMany(x =>

            x.Value.Select(sub =>
            (MatData: x.Key, Subdivision: sub.Key, BinCapacity: (int)(sub.Value * 1000.0))

            )

            .Where(x => anyItemHas(x.MatData)) // Небольшое ограничение, чтоб лишних не хватал

            ).ToList();


            //(MatData: x.Key, Subdivision: 0, BinCapacity: (int)(x.Value * 1000.0)));.ToList();

            // Сначала просто создаём переменную
            ILiteral[,] xNew = new ILiteral[allItems.Count, allBins.Count];
            for (int i = 0; i < allItems.Count; i++)
            {
                for (int b = 0; b < allBins.Count; b++)
                {
                    xNew[i, b] = model.NewBoolVar($"ПРЕДМЕТ: {allItems[i].MatData} в корзину {allBins[b].MatData}");
                }
            }

            // Создаём таблицу для данных
            int[,] IBtable = new int[allItems.Count, allBins.Count];
            // Заполняем ее данными по весу
            for (int i = 0; i < allItems.Count; i++)
            {
                for (int b = 0; b < allBins.Count; b++)
                {
                    if (allBins[b].MatData == allItems[i].MatData)
                    {
                        IBtable[i, b] = allItems[i].MatWeight;
                    }
                    else
                    {
                        if (AllPossibleAnaloguesForMats[i].TryGetValue(allBins[b].MatData, out var analogueW)) // НО! Если у мата есть аналог, как раз такой остаток, то поместить МОЖНО!
                        {
                            // Причем не с абы каким весом, а с новым
                            IBtable[i, b] = (int)analogueW;
                        }
                    }
                }
            }

            // Ограничение: каждый предмет можно только в одну корзину, и при том не в каждую
            for (int i = 0; i < allItems.Count; i++)
            {
                List<ILiteral> literals = new List<ILiteral>();
                List<ILiteral> literalsRestricted = new List<ILiteral>();
                for (int b = 0; b < allBins.Count; b++)
                {
                    if (IBtable[i, b] == 0)
                    {
                        // Это значит, что ни аналога, ни обычного совпадения по номенклатуре не было (иначе бы заполнилась), поместить нельзя
                        literalsRestricted.Add(xNew[i, b]);
                    }
                    else
                    {
                        // Поместить можно.
                        literals.Add(xNew[i, b]);
                    }
                }
                model.Add(Google.OrTools.Sat.LinearExpr.Sum(literalsRestricted) == 0); // В сумме запрещенных литератов быть не должно! Должен быть 0!
                model.AddAtMostOne(literals); // А разрешенных - лишь 1!
            }

            // Групповые ограничения: если вес аналога есть, это не значит, что он самостоятельный. Требуется указать ограничения по группам
            var groupedByOps = allItems.GroupBy(x => x.stageLink); // Сгруппирем по этапам.
            foreach (var group in groupedByOps)
            {
                var lists = opsPossibleLeftoversForOne[group.First().modLink]; // Получим все списки остатков для этого этапа, чтобы задать инфу этапам
                foreach (var list in lists)
                {
                    if (list.Value.List.Count < 2) continue; // Если это одиночная группа, то пофиг на неё.

                    // Сначала нам нужно найти внутри этапчика ВСЕ материалы из этой группы, чтобы замена тут была возможна.
                    int[] indexes = list.Value.List.Select(listGroupInfo => // Переменная инфы о заменяемости

                    // Мы ищем в группе group первый элемент, чья номенклатура равна "оригиналу" из groupInfo
                    group.FirstOrDefault(item => item.MatData == listGroupInfo.Original, // Сравниваем предмет
                    (-1, null, null, new (), -1, -1) // А это заглушка Default.
                    )
                    .elementIndex // По итогу возвращаем elementIndex. Если элемент не найден, он будет равен -1.

                    ).ToArray();

                    if (indexes.Any(x => x == -1))
                    {
                        DenyThisGroup(); // вызов запрещалки
                        continue; // Если хоть один равен -1, значит эта группа не подходит - она будет не полностью заменяться.
                    }

                    // Отлично. Есть на что заменять.
                    // Теперь нужно и оригинальным материалам, и остаткам сказать, что они либо все равны true в решении, либо false.

                    // Сначала оригиналы. Найдём предмет, а также рюкзак, в котором он в таком случае сидит.
                    // 18.06.2025 - с другой стороны!!! Если на входе у материала инфа об аналоге, но не сама номенклатура материала, то и смысла в этом нет - MatNomen не будет найден ни в одной из корзин. Но если они решат это задействовать, то этот код спасёт.
                    List<ILiteral>[] items = indexes.Select(
                        elementIndex => // Это сам индекс собственно
                        allBins.Select((bin, binIndex) => (xNew[elementIndex, binIndex], bin)) // Выбираем ща все элементы, связанные с этой корзиной + саму корзину
                        .Where(x => x.bin.MatData == allItems[elementIndex].MatData) // Фильтруем: оставляем только те предметы, которые влезут в ту корзину, чья номенлкатура равна номенклатуре предмета (оригиналу в данном случае)
                        .Select(x => x.Item1) // Берём переменную.
                        .ToList()
                        )
                        .ToArray();

                    // Теперь то же самое, только с альтернативами

                    // В самом конце - добавляем ограничения

                    // Теперь альтернативы. Найдём предмет, а также рюкзак, в котором он в таком случае сидит.
                    List<ILiteral>[] items2 = indexes.Select(
                        (elementIndex, arrayIndex) => // Это сам индекс собственно + индекс массива
                        allBins.Select((bin, binIndex) => (xNew[elementIndex, binIndex], bin)) // Выбираем ща все элементы, связанные с этой корзиной + саму корзину
                        .Where(x => x.bin.MatData == list.Value.List[arrayIndex].Analogue) // Фильтруем: оставляем только те предметы, которые влезут в ту корзину, чья номенлкатура равна номенклатуре аналога
                        .Select(x => x.Item1) // Берём переменную.
                        .ToList()
                        )
                        .ToArray();

                    AddConstraint(items);
                    AddConstraint(items2);

                    void DenyThisGroup()
                    {
                        for (int i = 0; i < indexes.Length; i++)
                        {
                            var index = indexes[i];
                            if (index == -1) continue;

                            // Для всех индексов, которые есть, надо запретить вставать на аналоги
                            List<ILiteral> deniedItems = allBins.Select((bin, binIndex) => (xNew[index, binIndex], bin)) // Выбираем ща все элементы, связанные с этой корзиной + саму корзину
                            .Where(x => x.bin.MatData == list.Value.List[i].Analogue) // Фильтруем: оставляем только те предметы, которые влезут в ту корзину, чья номенлкатура равна номенклатуре аналога
                            .Select(x => x.Item1) // Берём переменную.
                            .ToList();

                            model.Add(Google.OrTools.Sat.LinearExpr.Sum(deniedItems) == 0); // Они не должны встать ни на одну корзину этого аналога!!!
                        }
                    }

                    void AddConstraint(List<ILiteral>[] items)
                    {
                        // В каждом случае они должны хоть в одной корзине да сидеть
                        Google.OrTools.Sat.LinearExpr[] expressions = items.Select(litsList => Google.OrTools.Sat.LinearExpr.Sum(litsList)).ToArray();

                        var table = model.AddAllowedAssignments(expressions);
                        table.AddTuple(expressions.Select(x => 0).ToArray()); // Они должны бытб либо ВСЕ равны 0... (ни один из предметов не состоит в этом рюкзаке)
                        table.AddTuple(expressions.Select(x => 1).ToArray()); // Либо ВСЕ равны 1 - т.е. каждый из элементов попал в нужный рюкзак
                    }

                }
                // BinsMatNomensIndexes
            }

            // Больше чем вместимость рюкзака - нельзя вместить
            for (int b = 0; b < allBins.Count; b++)
            {
                List<ILiteral> items = new List<ILiteral>();
                List<int> Weights = new();
                for (int i = 0; i < allItems.Count; i++)
                {
                    if (IBtable[i, b] != 0)
                    {
                        items.Add(xNew[i, b]);
                        Weights.Add(IBtable[i, b]);
                    }
                }
                if (items.Count > 0)
                {
                    model.Add(Google.OrTools.Sat.LinearExpr.WeightedSum(items, Weights) <= allBins[b].BinCapacity);
                }
            }

            // На основании таблицы задаём удельную ценность
            LinearExprBuilder obj = Google.OrTools.Sat.LinearExpr.NewBuilder();
            for (int i = 0; i < allItems.Count; i++)
            {
                for (int b = 0; b < allBins.Count; b++)
                {
                    obj.AddTerm(xNew[i, b], IBtable[i, b] / (allItems[i].stageLink.Subdivision != allBins[b].Subdivision ? 2 : 1)); // Если не то подразделение, то этот остаток не настолько хорош, как другие.
                }
            }
            model.Maximize(obj);

            // Решалка
            var dt0 = DateTime.Now;
            CpSolver solverNew = new CpSolver();
            CpSolverStatus status = CpSolverStatus.Unknown;
            CustomCpSolverSolutionCallback cb = new CustomCpSolverSolutionCallback(50);
            solverNew.StringParameters = "max_time_in_seconds:20.0";// enumerate_all_solutions:true; - было в документации, но и без нее всё работает
            status = solverNew.Solve(model, cb);
            var dt1 = DateTime.Now;
            var delta = dt1 - dt0;
            Console.WriteLine($"total time wasted on cpSolver is {delta}");
            SessionLink.AssignedLogger.PartiasLogQueue.Enqueue($"total time wasted on solver is {delta}");
            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                for (int i = 0; i < allItems.Count; i++)
                {
                    for (int b = 0; b < allBins.Count; b++)
                    {
                        if (solverNew.BooleanValue(xNew[i, b]))
                        {
                            var list = allItems[i].stageLink.StageMaterialsInfo.UsedLeftovers;
                            // Назначим ту номенклатуру, что у рюкзака - это может быть как оригинал, так и аналог.
                            list[allItems[i].MatIndex] = new StageLeftover(allBins[b].MatData, IBtable[i, b] / 1000.0);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"cpSolver ended with {status}");
                SessionLink.AssignedLogger.LogQueue.Enqueue($"solver for leftovers ended with {status}");
            }
        }

        public List<LeftoverLackData> PlanAndFixMaterialsInfo(OpersPool pool, IDictionary<int, DateTime> firstStageStartData)
        {
            List<LeftoverLackData> result = new();

            if (CurrentLeftoversCount == null || CurrentLeftoversCount.Count == 0) return result;

            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;

            var points = pool.Blocks.SelectMany(block =>
                block.Lists.Select(list => (Date: firstStageStartData[list.FirstOper.Index], Object: (object)list)
                    //list => list.FirstOper.StageMaterialsInfo == null ? Array.Empty<(int, DateTime, Uco, double)>() : list.FirstOper.StageMaterialsInfo.UsedMaterials.Select(x => (list.FirstOper.Index, When: firstStageStartData[list.FirstOper.Index], x.Nomenklature, list.Count))
                )
                );

            var arrivalInfo = LeftoversArrivalInfo.SelectMany(pair => pair.Value.Select(x => (Date: x.When, Object: (object)(Material: pair.Key, Subdivision: x.Subdivision, Count: x.Count))));

            var pointsToAnalyze = points
                .Union(arrivalInfo)
                .OrderBy(point => point.Date).ToList();

            var howMuchAtStart = CurrentLeftoversCount.ToDictionary(x => x.Key, x => x.Value.ToDictionary()); // Создаем независимую копию

            Dictionary<IAbleForTaraCalcInterface, Dictionary<Guid, LeftoversReplacementClass>> opsPossibleLeftoversForOne = new Dictionary<IAbleForTaraCalcInterface, Dictionary<Guid, LeftoversReplacementClass>>();

            for (int currentPoint = 0; currentPoint < pointsToAnalyze.Count; currentPoint++)
            {
                var point = pointsToAnalyze[currentPoint];
                if (point.Object is PlanningModelling planMod)
                {
                    var startDate = firstStageStartData[planMod.FirstOper.Index];

                    List<IAbleForTaraCalcInterface> allMods = planMod.ObshEtapData == null ? new() { new PartiaModellingSmall(planMod.OpersList) } : planMod.ObshEtapData.Select(x => x.Value.AsMod).ToList();

                    foreach (var mod in allMods)
                    {
                        var oper = mod.FirstOperLink;//OpersList[0];
                        var matsClass = oper.StageMaterialsInfo;
                        var matsList = matsClass.UsedMaterials;

                        var originalCount = matsList.Select(X => X.IsBuying ? GetMatSumFromMod(mod, X.AsLeftoverData()) : 0.0).ToList();//new List<double>();
                        

                        //var matsList = matsClass.UsedMaterials.Select(x =>
                        //{
                        //    var nomen = x.AsLeftoverData();
                        //    return (Data: nomen, IsBying:  Count: GetMatSumFromMod(mod, nomen));
                        //    }
                        //).ToList();
                        var leftoversList = matsClass.UsedLeftovers;
                        if (matsList == null) continue;

                        bool hasAnyErrorsHere = false;

                        for (int i = 0; i < matsList.Count; i++)
                        {
                            if (!matsList[i].IsBuying)
                            {
                                //originalCount.Add(0); // Для правильной индексации добавляем пустоту
                                continue;
                            }

                            if (leftoversList[i] == null && !howMuchAtStart.ContainsKey(matsList[i].AsLeftoverData()))
                            {
                                hasAnyErrorsHere = true;
                                break; // Ошибки тут очевидно есть, и надо назначить этому материалу аналог. Выходим отсюда и идём в заменялку.
                            }
                            if (howMuchAtStart.TryGetValue(leftoversList[i]?.LeftoverData ?? matsList[i].AsLeftoverData(), out var existingCounts) && existingCounts.MaxBy(x => x.Value).Value < (leftoversList[i]?.CalculatedCount ?? originalCount[i]))
                            {
                                hasAnyErrorsHere = true;
                                break;
                            }
                        }

                        if (!hasAnyErrorsHere)
                        {
                            // Вычитаем всю информацию
                            for (int i = 0; i < matsList.Count; i++)
                            {
                                if (!matsList[i].IsBuying) continue;
                                var key = leftoversList[i]?.LeftoverData ?? matsList[i].AsLeftoverData();
                                var valueToMinus = leftoversList[i]?.CalculatedCount ?? originalCount[i];
                                if (howMuchAtStart.TryGetValue(key, out var neededCount))
                                {
                                    var elements = neededCount.Where(x => x.Value >= valueToMinus).OrderByDescending(x => x.Key == oper.Subdivision ? 1 : 0).ThenByDescending(x => x.Value);
                                    if (elements.Count() == 0)
                                    {
                                        ;
                                    }
                                    var pair = elements.First();

                                    //var lack = new LeftoverLackData(
                                    //    key,
                                    //    oper.GetPartiaCode() ?? Guid.Empty,
                                    //    valueToMinus,
                                    //    howMuchAtStart[key][pair.Key],
                                    //    pair.Key,
                                    //    point.Date,
                                    //    null,
                                    //    null,
                                    //    oper.GUID,
                                    //    oper.IsCustomModelling ? oper.CustomModelling : oper.MesModelling.Code,
                                    //    oper.GetEasyFlagToOutput()
                                    //    );

                                    //result.Add(lack);


                                    howMuchAtStart[key][pair.Key] -= valueToMinus;
                                }
                            }
                            continue; // Если тут никаких ошибок нет, то забиваем болт и идём дальше
                        }

                        // Сначала вообще все возможные замены представим себе

                        FillPossibleAnalogueGroupsForMod(opsPossibleLeftoversForOne, mod);
                        //if (!opsPossibleLeftoversForOne.ContainsKey(oper)) // Посмотрим, находил ли я для этой операции данные
                        //{
                        //    Dictionary<Guid, List<LeftoverReplacementInfo>> dic = GetReplacementInfo(oper, x => true/*x.Value.Count == 1*/); // Найдём все группы возможной замены для этого этапчика, даже не взирая на то, что они могут быть групповыми
                        //    opsPossibleLeftoversForOne.Add(oper, dic);
                        //}

                        var mats = matsList.Select((x, index) => (ElementIndex: index, Material: x)).Where(x => x.Material.IsBuying).ToList();

                        // Получаем список всех замен, и отсортируем ещё и по общей сумме оставшегося количества на складе
                        var dictionariesReplacement = opsPossibleLeftoversForOne[mod].OrderByDescending(x => x.Value.List.Count).ThenByDescending(x => x.Value.List.Sum(y => howMuchAtStart[y.Analogue].Sum(x => x.Value)));

                        foreach (var someGroup in dictionariesReplacement)
                        {
                            var listHere = someGroup.Value;
                            var resultedIndexes = listHere.List.Select(x => mats.FindIndex(mat => mat.Material.AsLeftoverData() == x.Original)).ToArray(); // Найдём сначала оригиналы.

                            // ElementIndex внутри mats = ссылка на индексы в основном массиве для простого поиска после фильтрации по IsBuying.
                            // resultedIndexes = индексы внутри mats после того как была найдена номенклатура
                            // resultedIndexes[i] - проход по всем найденным индексам
                            // mats[resultedIndexes[i]] - обращение к элементу в отфильтрованном списке
                            // mats[resultedIndexes[i]].ElementIndex - обращение к глобальному элементу

                            if (resultedIndexes.Any(x => x == -1)) continue; // Это значит, группа не подходит.

                            var analogues = listHere.List.Select((x, index) => (AnalogueToSet: x.Analogue, CountToSet: originalCount[mats[resultedIndexes[index]].ElementIndex] / x.OriginalCount * x.AnalogueCount)).ToArray(); // Создаём аналоги для каждого мата и вычисляем новое количество. В качестве аргумента originalCount[mats[resultedIndexes[index]].ElementIndex] - это количество уже моделирования, а не исходного этапчика. Пригодится для объединенных длин.

                            bool bad = false;
                            for (int i = 0; i < analogues.Length; i++)
                            {
                                if (!howMuchAtStart.TryGetValue(analogues[i].AnalogueToSet, out var howLeft))
                                {
                                    bad = true;
                                    break;
                                }

                                var globalIndex = mats[resultedIndexes[i]].ElementIndex;

                                if (howLeft.Any(x => x.Value >= analogues[i].CountToSet) || leftoversList[globalIndex] == null && !howMuchAtStart.ContainsKey(matsList[globalIndex].AsLeftoverData())) // Назначеннный остаток пуст + оригинальный мат не может быть выбран, так как не является остатком
                                {
                                    // По идее это норм, можно идти дальше...
                                }
                                else
                                {
                                    bad = true;
                                    break;
                                }
                            }

                            if (bad) continue; // Тоже не подходит!

                            // Задаём уже наконец-то данные
                            for (int i = 0; i < analogues.Length; i++)
                            {
                                var globalIndex = mats[resultedIndexes[i]].ElementIndex;
                                leftoversList[globalIndex] = new StageLeftover(analogues[i].AnalogueToSet, analogues[i].CountToSet);
                            }
                            resultedIndexes = resultedIndexes.OrderByDescending(x => x).ToArray();
                            foreach (var index in resultedIndexes) mats.RemoveAt(index); // Отсортировав по убыванию, можно безопасно удалить элементы
                        }

                        for (int i = 0; i < matsList.Count; i++)
                        {
                            if (!matsList[i].IsBuying) continue;
                            (LeftoverUco Material, double Count) whatWillDecrease;
                            if (matsClass.UsedLeftovers[i] == null)
                                whatWillDecrease = (matsList[i].AsLeftoverData(), originalCount[i]);
                            else
                                whatWillDecrease = (leftoversList[i].LeftoverData, leftoversList[i].CalculatedCount);

                            if (howMuchAtStart.TryGetValue(whatWillDecrease.Material, out var howMuchNow))
                            {

                                var pair = howMuchNow/*.Where(x => x.Value >= valueToMinus)*/.OrderByDescending(x => x.Key == oper.Subdivision ? 1 : 0).ThenByDescending(x => x.Value).First();
                                var howMuchNowCount = pair.Value;
                                howMuchNowCount -= whatWillDecrease.Count;
                                if (howMuchNowCount < 0)
                                {
                                    var n = pointsToAnalyze.FindIndex(currentPoint + 1, x =>
                                    {
                                        if (x.Object is PlanningModelling) return false;
                                        (LeftoverUco Material, Uco Subdivision, double Count) info = ((LeftoverUco Material, Uco Subdivision, double Count))x.Object;
                                        return whatWillDecrease.Material == info.Material; // Нам нужен приход того же материала
                                    }); // найдем следующий приход
                                    DateTime? nextPrihod = n == -1 ? null : pointsToAnalyze[n].Date;
                                    //Uco? nextPrihodSub = n == -1 ? null : (((LeftoverUco Material, Uco Subdivision, double Count))pointsToAnalyze[n].Object).Subdivision;

                                    var lack = new LeftoverLackData(
                                        whatWillDecrease.Material,
                                        oper.GetPartiaCode() ?? Guid.Empty,
                                        whatWillDecrease.Count,
                                        howMuchNowCount + whatWillDecrease.Count,
                                        //pair.Key,
                                        point.Date,
                                        nextPrihod ?? DateTime.MinValue,
                                        //nextPrihodSub,
                                        oper.GUID,
                                        oper.IsCustomModelling ? oper.CustomModelling : oper.MesModelling.Code,
                                        oper.GetEasyFlagToOutput()
                                        );

                                    result.Add(lack);

                                }
                                else
                                {

                                    //var lack = new LeftoverLackData(
                                    //    whatWillDecrease.Material,
                                    //    oper.GetPartiaCode() ?? Guid.Empty,
                                    //    whatWillDecrease.Count,
                                    //    howMuchNowCount + whatWillDecrease.Count,
                                    //    pair.Key,
                                    //    point.Date,
                                    //    null,
                                    //    null,
                                    //    oper.GUID,
                                    //    oper.IsCustomModelling ? oper.CustomModelling : oper.MesModelling.Code,
                                    //    oper.GetEasyFlagToOutput()
                                    //    );

                                    //result.Add(lack);

                                    howMuchAtStart[whatWillDecrease.Material][pair.Key] = howMuchNowCount; // Подсчитанное количество вбиваем, если оно все ещё больше 0.
                                }

                            }


                        }
                    }
                }
                else
                {
                    // Это просто приход
                    (LeftoverUco Material, Uco Subdivision, double Count) info = ((LeftoverUco Material, Uco Subdivision, double Count))point.Object;
                    howMuchAtStart[info.Material].TryAdd(info.Subdivision, 0);
                    howMuchAtStart[info.Material][info.Subdivision] += info.Count;
                }
            }


            return result;
        }

    }
}

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
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Rodana.Core.Orders;
using Rodana.Core.Partias.PartiasClasses;
using Rodana.Core.Planning.PlanningClasses;

namespace Rodana.Core.Partias.Leftovers
{
    public class LeftoversMasterData
    {
        public Session.Session SessionLink { get; }

        public LeftoversMasterData(Session.Session session)
        {
            SessionLink = session;
        }

        Dictionary<Uco, List<LeftoverBaseModel>> LeftoversRawData = new();
        internal void AddLeftover(LeftoverBaseModel model)
        {
            LeftoversRawData.TryAdd(model.Nomenklature, new());
            LeftoversRawData[model.Nomenklature].Add(model);
        }

        internal FrozenDictionary<Uco, double> CurrentLeftoversCount { get; private set; }
        internal FrozenDictionary<Uco, List<(DateTime When, double Count)>> LeftoversArrivalInfo { get; private set; }

        internal void PostProcess()
        {
            CurrentLeftoversCount = LeftoversRawData.ToFrozenDictionary(x => x.Key, x => x.Value.Max(y => y.CountNow));
            LeftoversArrivalInfo = LeftoversRawData.ToFrozenDictionary(x => x.Key,
                x => x.Value
                .Where(left => left.DateArrival.HasValue && left.DateArrival.Value != default(DateTime)) // Берём только те, у которых определена дата
                .Select(left => (When: left.DateArrival.Value, Count: left.CountArrival)) // Превращаем в кортеж
                .ToList());
        }

        internal void ClearLeftoversInfo()
        {
            LeftoversRawData.Clear();
            CurrentLeftoversCount = null;
            LeftoversArrivalInfo = null;
        }

        internal Dictionary<(OrderID Order, Uco Nomenklature, Uco Specification),
            Dictionary<Guid, List<LeftoverReplacementInfo>>
            > LeftoversGroupsInfo { get; private set; } = new();

        internal void AddLeftoversGroupsInfo(LeftoverGroupDataLoaderModel model)
        {
            (OrderID Order, Uco Nomenklature, Uco Specification) key = (model.Order, model.Nomenklature, model.Specification);
            LeftoversGroupsInfo.TryAdd(key, new());
            LeftoversGroupsInfo[key].TryAdd(model.MaterialsGroup, new());
            LeftoversGroupsInfo[key][model.MaterialsGroup].Add(new LeftoverReplacementInfo(model));
        }


        internal Dictionary<Guid, List<LeftoverReplacementInfo>> GetReplacementInfo(Stages.Stage op, Func<KeyValuePair<Guid, List<LeftoverReplacementInfo>>, bool> comparer)
        {
            var dicAll = new List<KeyValuePair<Guid, List<LeftoverReplacementInfo>>>();

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

        internal void PreparePartiasForLeftovers(List<PartiaClass> partias)
        {
            if (CurrentLeftoversCount == null || CurrentLeftoversCount.Count == 0) return;

            // Создадим решатель
            CpModel model = new CpModel();

            var SessionLink = partias[0].OpersHere[0][0].Opers[0].SessionLink;

            // TO DO: А что делать с моделированиями из МЕС??? там же 2+ этапчика
            var allItems = partias.SelectMany(x => PartiasConnectorEngine.PartiasConnectorEngine.SeeSdvigsAsList(x.OpersHere)) // Возьмём этапы со всех партий
                .SelectMany(sdvig =>

                sdvig.Opers[0].StageMaterialsInfo.UsedMaterials.Select(
                    (mat, index) =>
                    
                    (stageLink: sdvig.Opers[0], MatNomen: mat.Nomenklature, MatIndex: index, MatWeight: (int)(mat.Count * 1000.0))
                )
            )
                .Where(
                item =>
                //CurrentLeftoversCount.ContainsKey(item.MatNomen)
                //&&
                item.stageLink.StageMaterialsInfo.UsedMaterials[item.MatIndex].IsBuying) // Второе ограничение.
                .Select((x, index) => (elementIndex: index, stageLink: x.stageLink, MatNomen: x.MatNomen, MatIndex: x.MatIndex, MatWeight: x.MatWeight))
                .ToList();

            Dictionary<Stages.Stage, Dictionary<Guid, List<LeftoverReplacementInfo>>> opsPossibleLeftoversForOne = new Dictionary<Stages.Stage, Dictionary<Guid, List<LeftoverReplacementInfo>>>();

            List<Dictionary<Uco, double>> AllPossibleAnaloguesForMats = new();
            for (int i = 0; i < allItems.Count; i++)
            {
                var item = allItems[i];
                var op = item.stageLink;
                if (!opsPossibleLeftoversForOne.ContainsKey(op)) // Посмотрим, находил ли я для этой операции данные
                {
                    Dictionary<Guid, List<LeftoverReplacementInfo>> dic = GetReplacementInfo(op, x => true/*x.Value.Count == 1*/); // Найдём все группы возможной замены для этого этапчика, даже не взирая на то, что они могут быть групповыми
                    opsPossibleLeftoversForOne.Add(op, dic);
                }

                var possibleAnaloguesButInversed = opsPossibleLeftoversForOne[op]
                    .SelectMany(x => x.Value) // У нас появятся вообще все группы
                    .Where(x => x.Analogue == item.MatNomen) // Сначала выберем только те, у которых та же номенклатура, что и у предмета текущего
                    .GroupBy(x => x.Original) // Группируем по оригиналу - надо выудить из группы информацию
                    .SelectMany(group =>
                    {
                        var data = group.First(x => x.Analogue == item.MatNomen); // найдем первую ячейку, где аналогом выступает нужный нам материал, чтоб взять у него инфу по 
                        var koeff = 1.0 / data.AnalogueCount * data.OriginalCount; // для удобства написано так. Коэффициент будет юзаться так, чтобы мы количество аналога превратили в количество оригинала, и вот от оригинала уже будем плясать.

                        return group.Select(gr => (Key: gr.Analogue, Value: koeff * item.MatWeight / gr.OriginalCount * gr.AnalogueCount));
                    });

                // А это вообще обычная группа, которая работает, начиная с аналогов
                var possibleAnalogues = opsPossibleLeftoversForOne[op]
                    .SelectMany(x => x.Value) // У нас появятся вообще все группы
                    .Where(x => x.Original == item.MatNomen) // Сначала выберем только те, у которых та же номенклатура, что и у предмета текущего

                    .Select(analogue =>
                {
                    // Получаем инфу об аналоге
                    return (Key: analogue.Analogue, Value: item.MatWeight / analogue.OriginalCount * analogue.AnalogueCount); // Получаем пару: ключом служит мат-аналог, а значением - подсчитанное количество, посчитанное формулой
                })
                    .Union(possibleAnaloguesButInversed)
                    .Where(x => x.Key != item.MatNomen) // Уберем те, что равны моей номенклатуре, они нафиг не нужны
                    .GroupBy(x => x.Key) // Теперь сгруппируем по этим самым остаткам-аналогам
                    .ToDictionary(x => x.Key, x => x.Max(y => y.Value)); // Поздравляю, у нас появился словарь где есть инфа, какие остатки можно юзать, и какой вес у них будет из-за этого. Вес при этом берется максимальный

                AllPossibleAnaloguesForMats.Add(possibleAnalogues); // Добавляю информацию об этом в копию
            }

            var allBins = CurrentLeftoversCount.Select(x => (MatNomen: x.Key, BinCapacity: (int)(x.Value * 1000.0))).ToList();

            // Сначала просто создаём переменную
            ILiteral[,] xNew = new ILiteral[allItems.Count, allBins.Count];
            for (int i = 0; i < allItems.Count; i++)
            {
                for (int b = 0; b < allBins.Count; b++)
                {
                    xNew[i, b] = model.NewBoolVar($"ПРЕДМЕТ: {allItems[i].MatNomen} в корзину {allBins[b].MatNomen}");
                }
            }

            // Создаём таблицу для данных
            int[,] IBtable = new int[allItems.Count, allBins.Count];
            // Заполняем ее данными по весу
            for (int i = 0; i < allItems.Count; i++)
            {
                for (int b = 0; b < allBins.Count; b++)
                {
                    if (allBins[b].MatNomen == allItems[i].MatNomen)
                    {
                        IBtable[i, b] = allItems[i].MatWeight;
                    }
                    else
                    {
                        if (AllPossibleAnaloguesForMats[i].TryGetValue(allBins[b].MatNomen, out var analogueW)) // НО! Если у мата есть аналог, как раз такой остаток, то поместить МОЖНО!
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
                var lists = opsPossibleLeftoversForOne[group.Key]; // Получим все списки остатков для этого этапа, чтобы задать инфу этапам
                foreach (var list in lists)
                {
                    if (list.Value.Count < 2) continue; // Если это одиночная группа, то пофиг на неё.

                    // Сначала нам нужно найти внутри этапчика ВСЕ материалы из этой группы, чтобы замена тут была возможна.
                    int[] indexes = list.Value.Select(listGroupInfo => // Переменная инфы о заменяемости

                    // Мы ищем в группе group первый элемент, чья номенклатура равна "оригиналу" из groupInfo
                    group.FirstOrDefault(item => item.MatNomen == listGroupInfo.Original, // Сравниваем предмет
                    (-1, null, new (), -1, -1) // А это заглушка Default.
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
                        .Where(x => x.bin.MatNomen == allItems[elementIndex].MatNomen) // Фильтруем: оставляем только те предметы, которые влезут в ту корзину, чья номенлкатура равна номенклатуре предмета (оригиналу в данном случае)
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
                        .Where(x => x.bin.MatNomen == list.Value[arrayIndex].Analogue) // Фильтруем: оставляем только те предметы, которые влезут в ту корзину, чья номенлкатура равна номенклатуре аналога
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
                            .Where(x => x.bin.MatNomen == list.Value[i].Analogue) // Фильтруем: оставляем только те предметы, которые влезут в ту корзину, чья номенлкатура равна номенклатуре аналога
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
                model.Add(Google.OrTools.Sat.LinearExpr.WeightedSum(items, Weights) <= allBins[b].BinCapacity);
            }

            // На основании таблицы задаём удельную ценность
            LinearExprBuilder obj = Google.OrTools.Sat.LinearExpr.NewBuilder();
            for (int i = 0; i < allItems.Count; i++)
            {
                for (int b = 0; b < allBins.Count; b++)
                {
                    obj.AddTerm(xNew[i, b], IBtable[i, b]);
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
            SessionLink.AssignedLogger.PartiasLogQueue.Enqueue($"total time wasted on cpSolver is {delta}");
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
                            list[allItems[i].MatIndex] = new StageLeftover(allBins[b].MatNomen, IBtable[i, b] / 1000.0);

                            //if (allBins[b].MatNomen == allItems[i].MatNomen)
                            //{
                            //    list[allItems[i].MatIndex] = new StageLeftover(allItems[i].MatNomen, IBtable[i, b] / 1000.0);
                            //}
                            //else
                            //{
                            //    // Тут другой материал
                            //}
                        }
                    }
                }
            }

        }

        public List<LeftoverLackData> PlanAndFixMaterialsInfo(OpersPool pool, IDictionary<int, DateTime> firstStageStartData)
        {
            List<LeftoverLackData> result = new();

            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;

            var points = pool.Blocks.SelectMany(block =>
                block.Lists.SelectMany(
                    list => list.FirstOper.StageMaterialsInfo == null ? Array.Empty<(int, DateTime, Uco, double)>() : list.FirstOper.StageMaterialsInfo.UsedMaterials.Select(x => (list.FirstOper.Index, When: firstStageStartData[list.FirstOper.Index], x.Nomenklature, list.Count))
                )
                );

            var groups = points.GroupBy(x => x.Nomenklature);


            foreach (var group in groups)
            {
                var material = group.Key;

                if (CurrentLeftoversCount.TryGetValue(material, out var howMuchAtStart))
                {
                    if (LeftoversArrivalInfo.TryGetValue(material, out var arrivalList))
                    {
                        // -1 означает приход
                        var sortedPoints = group.Union(arrivalList.Select(x => (ID: -1, When: x.When, material: material, Count: x.Count))).OrderBy(x => x.When).ToList();

                        for (int i = 0; i < sortedPoints.Count; i++)
                        {
                            var point = sortedPoints[i];

                            if (point.Item1 == -1) // это приход?
                            {
                                howMuchAtStart += point.Count;
                            }
                            else
                            {
                                howMuchAtStart -= point.Count;
                                if (howMuchAtStart < 0)
                                {
                                    /*
                                     •	N – Уникальный идентификатор записи о дефиците. Будет полезно при записи в базы данных SQL.
                        •	Номенклатура – GUID и наименование недостающего материала.
                        •	ИдентификаторПартииПФ – GUID партии ПФ.
                        •	КоличествоПотребность - Сколько требуется на момент времени (количество этапа, общего, объединенного, или исходного, с учётом наладочных длин).
                        •	КоличествоДоступно - Сколько есть в наличии на данный момент свободного остатка.
                        •	ДатаПотребности - Когда требуется данный материал (дата начала выполнения этапа), это дата дефицита.
                        •	ДатаПоступления - Если есть заказанный материал — когда он прибудет, для сравнения. Допускает пустое значение – поступлений может вообще не быть.
                        •	GUID – исходного этапа, который спровоцировал дефицит.
                        •	GUIDОбъединения, если оно есть.
                        •	GUIDОбщегоЭтапа, если оно есть.

                                     */

                                    var n = sortedPoints.FindIndex(i + 1, x => x.Item1 == -1); // найдем следующий приход
                                    DateTime? nextPrihod = n == -1 ? null : sortedPoints[n].When;

                                    var op = ALL[point.Item1];

                                    var lack = new LeftoverLackData(material, op.GetPartiaCode() ?? Guid.Empty, point.Count, howMuchAtStart + point.Count, point.When, nextPrihod, op.GUID, op.IsCustomModelling ? op.CustomModelling : op.MesModelling.Code, op.GetEasyFlagToOutput());

                                }
                            }
                        }

                    }
                }
            }

            return result;
        }

    }
}

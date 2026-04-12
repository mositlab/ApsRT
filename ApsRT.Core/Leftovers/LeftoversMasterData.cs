using ApsRT.Core.Leftovers;
using ApsRT.Core.Orders;
using ApsRT.Core.Partias.PartiasClasses;
using ApsRT.Core.Partias.PartiasConnectorEngine;
using ApsRT.Core.PlanSimulator;
using ApsRT.Core.Taras.TaraSearchEngine;
using Google.OrTools.LinearSolver;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using UniversalPack.Common;
using UniversalPack.Extensions;
using static System.Net.Mime.MediaTypeNames;
using LeftoverUco = (UniversalPack.Common.CodedObject<System.Guid, string> Nomenklature, UniversalPack.Common.CodedObject<System.Guid, string> Characteristics, UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string> Seria);
using MaterialUco = (UniversalPack.Common.CodedObject<System.Guid, string> Nomenklature, UniversalPack.Common.CodedObject<System.Guid, string> Characteristics);
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;

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
        public Dictionary<LeftoverUco, Dictionary<Uco, (double Count, UcoDU DimensionUnit)>> CurrentLeftoversCount { get; private set; } = new();

        public Dictionary<Guid, double> LinearLeftoversCount { get; private set; } = new();

        public void ClearLeftoversCount()
        {
            CurrentLeftoversCount.Clear();
            LinearLeftoversCount.Clear();
        }

        public void AddLeftoverCountData(LeftoverCountDataLoaderModel model)
        {
            LeftoverUco key = (model.Nomenklature, model.Characteristics, model.Seria);
            CurrentLeftoversCount.TryAdd(key, new());
            CurrentLeftoversCount[key].TryAdd(model.Subdivision, (model.CountNow, model.EdIzm));
            LinearLeftoversCount.TryAdd(model.Seria.Object.Code, model.CountNow);
        }

        public Dictionary<LeftoverUco, List<(DateTime When, Uco Subdivision, double Count, UcoDU EdIzm)>> LeftoversArrivalInfo { get; private set; } = new();

        //Dictionary<int, int>
        public Dictionary<Guid, double> UsedLeftoversFromBeggining = new();
        public Dictionary<Guid, List<(int Stage, int Place)>> UsedLeftoversFromBegginingData = new();

        public void ClearLeftoversUsageInfo()
        {
            UsedLeftoversFromBeggining.Clear();
            UsedLeftoversFromBegginingData.Clear();
        }

        public void ClearLeftoversArrivalData()
        {
            LeftoversArrivalInfo.Clear();
        }

        public void AddLeftoverArrivalData(LeftoverArrivalDataLoaderModel model)
        {
            LeftoverUco key = (model.Nomenklature, model.Characteristics, CodedObjectVersioned<Guid, string, string>.EmptyObject);
            LeftoversArrivalInfo.TryAdd(key, new());
            LeftoversArrivalInfo[key].Add((model.DateArrival, model.Subdivision, model.CountArrival, model.EdIzm));
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


        Dictionary<MaterialUco, double> FindAnaloguesAll(Dictionary<IAbleForTaraCalcInterface, Dictionary<Guid, LeftoversReplacementClass>> opsPossibleLeftoversForOne, IAbleForTaraCalcInterface mod, (MaterialUco MatNomen, double MatWeight) item)
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


        public double GetMatSumFromMod(IAbleForTaraCalcInterface mod, MaterialUco mat)
        {
            return mod.AllOpers.Sum(stage =>
            {
                var index = stage.StageMaterialsInfo.UsedMaterials.FindIndex(y => y.AsMaterialUcoData() == mat);
                if (index < 0) return 0;
                return stage.StageMaterialsInfo.UsedMaterials[index].Count;
            });
        }

        public void PrepareModsForLeftovers(List<PlanSimulatorBlock> ops, int PoolIndex)
        {
            
            PrepareModsForLeftovers(ops.SelectMany(block => block.Lists.SelectMany(list =>
            {
                if (list.EasyLifeData == null) return new List<(IAbleForTaraCalcInterface, PlanSimulatorNaladLengthData)>() { (list as IAbleForTaraCalcInterface, list.NaladLengthData[PoolIndex]) };
                return list.EasyLifeData.Select(x => (x.Value.AsMod, x.Value.NaladLengthData[PoolIndex]));
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

        public void PrepareModsForLeftovers(List<(IAbleForTaraCalcInterface mod, PlanSimulatorNaladLengthData naladLengthData)> modsToArrange)
        {
            if (CurrentLeftoversCount == null || CurrentLeftoversCount.Count == 0) return;

            // Сначала всё очищаем
            foreach (var mod in modsToArrange)
            {
                var op = mod.mod.FirstOperLink;
                op.StageMaterialsInfo.UsedLeftovers = op.StageMaterialsInfo.UsedMaterials.Select(x => (StageLeftover)null).ToList();
            }

            // Создадим решатель
            CpModel model = new CpModel();

            //var SessionLink = partias[0].OpersHere[0][0].Opers[0].SessionLink;

            // TO DO: А что делать с моделированиями из МЕС??? там же 2+ этапчика
            var allItems =

                modsToArrange

                //partias.SelectMany(x => PartiasConnectorEngine.SeeSdvigsAsList(x.OpersHere)) // Возьмём этапы со всех партий


                .Where(sdvig => sdvig.mod.FirstOperLink.StageMaterialsInfo != null)
                .SelectMany(sdvig =>

                sdvig.mod.FirstOperLink.StageMaterialsInfo.UsedMaterials.Select(
                    (mat, index) =>
                    {
                        //var naladLengthData = planMod.EasyLifeData == null ? planMod.NaladLengthData[PoolIndex] : planMod.EasyLifeData[oper.EasyFlag.Value].NaladLengthData[PoolIndex];
                        var count = (GetMatSumFromMod(sdvig.mod, mat.AsMaterialUcoData()) + (sdvig.naladLengthData?.Data.NaladLengthHere.Value ?? 0));
                        return (stageLink: sdvig.mod.FirstOperLink, modLink: sdvig, MatData: mat.AsMaterialUcoData(), MatIndex: index, MatWeight: (long)(count * 1000.0), MatWeightOrig: count);
                    }
                )
            )
                .Where(
                item =>
                true
                //&&
                //item.stageLink.StageMaterialsInfo.UsedMaterials[item.MatIndex].IsBuying
                ) // Второе ограничение.
                .Select((x, index) => (itsStage: false, elementIndex: index, x.stageLink, x.modLink, x.MatData, x.MatIndex, x.MatWeight, x.MatWeightOrig))

                //.Take(5000)

                .ToList();

            // Добавим самих ЭТАПЧИКОВ - это буквально а-ля заделы, потому что в остатках может сидеть и то, и то
            var stage1 = modsToArrange.SelectMany(mod =>
            {
                var placesCount = mod.mod.FirstOperLink.PlacesCount; // Количество мест
                return Enumerable.Range(0, placesCount).Select(place => (mod, place));
            }).ToArray();
            var stage2 = stage1.Select((x, index) =>
            (
            itsStage: true,
            elementIndex: allItems.Count + index,
            stageLink: x.mod.mod.FirstOperLink,
            modLink: x.mod,
            new MaterialUco(x.mod.mod.FirstOperLink.Nomenklature, x.mod.mod.FirstOperLink.Characteristics),
            MatIndex: x.place,
            MatWeight: (long)(x.mod.mod.Sum * 1000),
            MatWeightOrig: x.mod.mod.Sum
            ));
            allItems.AddRange(stage2);

            Dictionary<IAbleForTaraCalcInterface, Dictionary<Guid, LeftoversReplacementClass>> opsPossibleLeftoversForOne = new Dictionary<IAbleForTaraCalcInterface, Dictionary<Guid, LeftoversReplacementClass>>();

            List<Dictionary<MaterialUco, double>> AllPossibleAnaloguesForMats = new();
            for (int i = 0; i < allItems.Count; i++)
            {
                var item = allItems[i];
                var mod = item.modLink;

                var possibleAnalogues = FindAnaloguesAll(opsPossibleLeftoversForOne, mod.mod, (item.MatData, item.MatWeight));

                AllPossibleAnaloguesForMats.Add(possibleAnalogues); // Добавляю информацию об этом в копию
            }

            bool anyItemHas(LeftoverUco whatAsLeftover)
            {
                var what = (whatAsLeftover.Nomenklature, whatAsLeftover.Characteristics);
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
            (MatData: x.Key, Subdivision: sub.Key, BinCapacity: (long)(sub.Value.Count * 1000.0), BinEdIzm: sub.Value.DimensionUnit, UniqueGUID: x.Key.Seria.Object.Code)

            )

            .Where(x => anyItemHas(x.MatData)) // Небольшое ограничение, чтоб лишних не хватал

            ).ToList();

            var OpersToSdvigs = allItems.Where(x => x.itsStage && x.MatIndex == 0).Select(x => new KeyValuePair<int, PartiaModelling>(x.stageLink.Index, x.modLink.mod as PartiaModelling)).ToDictionary();

    //(MatData: x.Key, Subdivision: 0, BinCapacity: (int)(x.Value * 1000.0)));.ToList();
    List <(int i, int b, long w, double orig)> weights = new();
            // Заполняем ее данными по весу
            for (int i = 0; i < allItems.Count; i++)
            {
                bool canBeInAnyBin = false;
                for (int b = 0; b < allBins.Count; b++)
                {
                    var binMatData = (allBins[b].MatData.Nomenklature, allBins[b].MatData.Characteristics);

                    if (binMatData == allItems[i].MatData)
                    {
                        //IBtable[i, b] = allItems[i].MatWeight;
                        weights.Add((i, b, allItems[i].MatWeight, allItems[i].MatWeightOrig));
                        canBeInAnyBin = true;
                    }
                    else
                    {
                        if (allItems[i].itsStage) continue; // Если это этап на закрытие, то ни о каком аналоге речи не идет

                        if (AllPossibleAnaloguesForMats[i].TryGetValue(binMatData, out var analogueW)) // НО! Если у мата есть аналог, как раз такой остаток, то поместить МОЖНО!
                        {
                            // Причем не с абы каким весом, а с новым
                            //IBtable[i, b] = (long)analogueW;
                            weights.Add((i, b, (long)analogueW, allItems[i].MatWeightOrig));
                            canBeInAnyBin = true;
                        }
                    }
                }

                if (!canBeInAnyBin)
                {
                    allItems.RemoveAt(i);
                    i--;
                }
            }

            // Обновляем индексы на всякий случай
            for (int i = 0; i < allItems.Count; i++)
            {
                var el = allItems[i];
                el.elementIndex = i;
                allItems[i] = el;
            }

            // Создаём таблицу для данных
            (long Count, double Orig)[,] IBtable = new (long Count, double Orig)[allItems.Count, allBins.Count];
            foreach (var el in weights) IBtable[el.i, el.b] = (el.w, el.orig);

            // И ещё бонусная таблица
            long[,] BONUS_Table = new long[allItems.Count, allBins.Count];
            // Сначала просто создаём переменную
            ILiteral[,] xNew = new ILiteral[allItems.Count, allBins.Count];
            for (int i = 0; i < allItems.Count; i++)
            {
                for (int b = 0; b < allBins.Count; b++)
                {
                    xNew[i, b] = IBtable[i, b].Count == 0 ? null : model.NewBoolVar($"ПРЕДМЕТ: {allItems[i].MatData} в корзину {allBins[b].MatData}");
                }
            }

            // Тут лежит инфа - заюзан ли где то предмет или нет
            BoolVar[] itemChosen = new BoolVar[allItems.Count];

            for (int i = 0; i < allItems.Count; i++)
            {
                itemChosen[i] = model.NewBoolVar($"chosen_{i}");

                // chosen[i] = 1, если предмет лежит ХОТЯ БЫ в одном рюкзаке
                model.AddMaxEquality(itemChosen[i], Enumerable.Range(0, allBins.Count).Select(b => xNew[i, b]).Where(x => x != null).Select(x => (Google.OrTools.Sat.LinearExpr)x));
            }

            // А тут лежит инфа, залочен рюкзак или нет
            //BoolVar[] binLocked = new BoolVar[allBins.Count];
            //for (int b = 0; b < allBins.Count; b++)
            //    binLocked[b] = model.NewBoolVar($"binLocked_{b}");

            // Ограничение: каждый предмет можно только в одну корзину, и при том не в каждую
            for (int i = 0; i < allItems.Count; i++)
            {
                List<ILiteral> literals = new List<ILiteral>();
                List<ILiteral> literalsRestricted = new List<ILiteral>();
                for (int b = 0; b < allBins.Count; b++)
                {
                    if (IBtable[i, b].Count == 0)
                    {
                        // Это значит, что ни аналога, ни обычного совпадения по номенклатуре не было (иначе бы заполнилась), поместить нельзя
                        if (xNew[i, b] != null) literalsRestricted.Add(xNew[i, b]);
                    }
                    else
                    {
                        // Поместить можно.
                        if (xNew[i, b] != null) literals.Add(xNew[i, b]);


                        if (allItems[i].itsStage)
                        {
                            // Во первых, все этапы которые те же что и этот, но являются материалами, закроются тут же
                            List<(ILiteral lit, int i, int b)> allOtherRestrictedMatsFromThisStage = allItems.Where(x => x.stageLink == allItems[i].stageLink && !x.itsStage)
                                .SelectMany(x => Enumerable.Range(0, allBins.Count).Select(b => (xNew[x.elementIndex, b], x.elementIndex, b))).ToList();

                            model.Add(Google.OrTools.Sat.LinearExpr.Sum(allOtherRestrictedMatsFromThisStage.GroupBy(x => x.i).Select(x => itemChosen[x.Key])) == 0).OnlyEnforceIf(xNew[i, b]);

                            // Во вторых, все его предки, неважно, материалы это или нет, тоже будут в сумме должны быть 0. Они не пойдут ни в одну из корзин, и никуда не встанут.
                            List<int> allPrevs = allItems[i].modLink.mod.AllOpers.SelectMany(x => x.Prevs).Distinct().ToList();
                            Queue<int> toCheck = new Queue<int>(allPrevs);
                            while (toCheck.Count > 0)
                            {
                                var el = toCheck.Dequeue();
                                foreach (var prev in SessionLink.MasterDataLinks.StagesMasterData.AllStages[el].Prevs)
                                {
                                    allPrevs.Add(prev);
                                    /*if (!AllStages[prev].Fixed)*/
                                    toCheck.Enqueue(prev);
                                }
                            }
                            HashSet<int> asHashSetPrevs = allPrevs.ToHashSet();
                            List<(ILiteral lit, int i, int b)> allPreviousOps = allItems.Where(x => asHashSetPrevs.Contains(x.stageLink.Index))


                                .SelectMany(x => Enumerable.Range(0, allBins.Count).Select(b => (xNew[x.elementIndex, b], x.elementIndex, b))).ToList();


                            model.Add(Google.OrTools.Sat.LinearExpr.Sum(allPreviousOps.GroupBy(x => x.i).Select(x => itemChosen[x.Key]))/*.Select(x => x.lit))*/ == 0).OnlyEnforceIf(xNew[i, b]);

                            // И последнее - больше никакой из элементов не может встать в эту корзину - она теперь задел, и этап ее всю забрал себе
                            //List<(ILiteral lit, int i, int b)> allOtherElements = Enumerable.Range(0, allItems.Count).Where(x => x != i).Select(j => (xNew[j, b], j, b)).ToList();
                            //foreach (var el in allOtherElements)
                            //    if (el.lit != null) model.AddImplication(xNew[i, b], el.lit.Not());
                            

                            var finalList = allOtherRestrictedMatsFromThisStage.Union(allPreviousOps)./*Union(allOtherElements).*/Distinct().ToArray();
                            long bonus = 0;
                            foreach (var el in finalList)
                            {
                                bonus += IBtable[el.i, el.b].Count;
                            }
                            bonus += 1000 * allItems[i].stageLink.Level;
                            BONUS_Table[i, b] = bonus;
                        }

                    }
                }
                if (literalsRestricted.Count > 0) model.Add(Google.OrTools.Sat.LinearExpr.Sum(literalsRestricted) == 0); // В сумме запрещенных литератов быть не должно! Должен быть 0!
                if (literals.Count > 0) model.AddAtMostOne(literals); // А разрешенных - лишь 1!


                //model.Add().
            }

            // Групповые ограничения: если вес аналога есть, это не значит, что он самостоятельный. Требуется указать ограничения по группам
            var groupedByOps = allItems.GroupBy(x => (x.stageLink, x.itsStage)); // Сгруппирем по этапам.
            foreach (var group in groupedByOps)
            {
                if (group.Key.itsStage) continue; // Целые этапы мы В ПРИНЦИПЕ не рассматриваем
                var lists = opsPossibleLeftoversForOne[group.First().modLink.mod]; // Получим все списки остатков для этого этапа, чтобы задать инфу этапам
                foreach (var list in lists)
                {
                    if (list.Value.List.Count < 2) continue; // Если это одиночная группа, то пофиг на неё.

                    // Сначала нам нужно найти внутри этапчика ВСЕ материалы из этой группы, чтобы замена тут была возможна.
                    int[] indexes = list.Value.List.Select(listGroupInfo => // Переменная инфы о заменяемости

                    // Мы ищем в группе group первый элемент, чья номенклатура равна "оригиналу" из groupInfo
                    group.FirstOrDefault(item => item.MatData == listGroupInfo.Original, // Сравниваем предмет
                    (false /*это ж материал, потому фолс*/, -1, null, (null, null), new (), -1, -1, -1) // А это заглушка Default.
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
                        .Where(x =>
                        {
                            var binMatData = (x.bin.MatData.Nomenklature, x.bin.MatData.Characteristics);
                            return binMatData == allItems[elementIndex].MatData;
                            
                            }) // Фильтруем: оставляем только те предметы, которые влезут в ту корзину, чья номенлкатура равна номенклатуре предмета (оригиналу в данном случае)
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
                        .Where(x =>
                        {
                            var binMatData = (x.bin.MatData.Nomenklature, x.bin.MatData.Characteristics);
                            return binMatData == list.Value.List[arrayIndex].Analogue;
                            }) // Фильтруем: оставляем только те предметы, которые влезут в ту корзину, чья номенлкатура равна номенклатуре аналога
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
                            .Where(x =>
                            {
                                var binMatData = (x.bin.MatData.Nomenklature, x.bin.MatData.Characteristics);
                                return binMatData == list.Value.List[i].Analogue;
                                }) // Фильтруем: оставляем только те предметы, которые влезут в ту корзину, чья номенлкатура равна номенклатуре аналога
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
                List<long> Weights = new();
                for (int i = 0; i < allItems.Count; i++)
                {
                    if (IBtable[i, b].Count != 0)
                    {
                        items.Add(xNew[i, b]);
                        Weights.Add(IBtable[i, b].Count);
                    }
                }
                if (items.Count > 0)
                {
                    model.Add(Google.OrTools.Sat.LinearExpr.WeightedSum(items, Weights) <= allBins[b].BinCapacity);
                }
            }

            // Добавляем информацию об использовании рюкзака, Если хоть один предмет в неё вошёл.
            BoolVar[] binUsed = new BoolVar[allBins.Count];
            for (int b = 0; b < allBins.Count; b++)
            {
                binUsed[b] = model.NewBoolVar($"binUsed_{b}");
                for (int i = 0; i < allItems.Count; i++)
                {
                    if (xNew[i, b] != null) model.AddImplication(xNew[i, b], binUsed[b]);
                }
            }

            // Считаем общий вес (точнее СТОИМОСТЬ) всем назначенным остаткам
            LinearExprBuilder totalWeightExpr = Google.OrTools.Sat.LinearExpr.NewBuilder();
            for (int b = 0; b < allBins.Count; b++)
            {
                for (int i = 0; i < allItems.Count; i++)
                {
                    if (xNew[i, b] != null) totalWeightExpr.AddTerm(xNew[i, b], IBtable[i, b].Count + BONUS_Table[i, b]);
                }
            }


            // Создаём решалку
            var dt0 = DateTime.Now;
            CpSolverStatus status = CpSolverStatus.Unknown;
            CpSolver solverNew = new CpSolver();
            solverNew.StringParameters =
                // enumerate_all_solutions:true; - было в документации, но и без нее всё работает
                "max_time_in_seconds:30.0 " +
                "num_search_workers:8 " +
                "cp_model_probing_level:2 " +
                "cp_model_presolve:true";

            // Шаг 1. Макс веса
            model.Maximize(totalWeightExpr);
            CustomCpSolverSolutionCallback cb = new CustomCpSolverSolutionCallback(50000);
            status = solverNew.Solve(model, cb);

            var dt1 = DateTime.Now;
            var delta = dt1 - dt0;
            Console.WriteLine($"1/2 time wasted on cpSolver is {delta}, status: {status}");
            SessionLink.AssignedLogger.PartiasLogQueue.Enqueue($"1/2 time wasted on solver is {delta}, status: {status}");
            SetResults(status);

            long maxWeightFound = (long)solverNew.ObjectiveValue;
            // ограничение: что меньше этого не надо
            model.Add(totalWeightExpr >= maxWeightFound);

            // Старая цель: просто максимизировать количество задействованных материалов в КГ:
            //LinearExprBuilder obj = Google.OrTools.Sat.LinearExpr.NewBuilder();
            //for (int i = 0; i < allItems.Count; i++)
            //{
            //    for (int b = 0; b < allBins.Count; b++)
            //    {
            //        var subdivisionsDecreaser = allItems[i].stageLink.Subdivision != allBins[b].Subdivision ? 2 : 1;

            //        // На основании таблицы задаём удельную ценность
            //        obj.AddTerm(xNew[i, b], IBtable[i, b] / subdivisionsDecreaser); // Если не то подразделение, то этот остаток не настолько хорош, как другие.
            //    }
            //}
            //model.Maximize(obj);


            var maxCap = allBins.Max(x => x.BinCapacity);
            //var minCap = allBins.Min(x => x.BinCapacity);

            // Заполненность рюкзаков
            LinearExprBuilder fillBonus = Google.OrTools.Sat.LinearExpr.NewBuilder();
            for (int b = 0; b < allBins.Count; b++)
            {
                LinearExprBuilder binWeight = Google.OrTools.Sat.LinearExpr.NewBuilder();
                for (int i = 0; i < allItems.Count; i++)
                {
                    if (xNew[i, b] != null) binWeight.AddTerm(xNew[i, b], IBtable[i, b].Count * 1000 * maxCap / allBins[b].BinCapacity);
                }

                fillBonus.Add(binWeight);
            }
            model.Maximize(fillBonus);

            // Решалка - второй уровень
            cb = new CustomCpSolverSolutionCallback(50000);
            var status2 = solverNew.Solve(model, cb);

            dt1 = DateTime.Now;
            delta = dt1 - dt0;
            Console.WriteLine($"total time wasted on cpSolver is {delta}, status: {status2}");
            SessionLink.AssignedLogger.PartiasLogQueue.Enqueue($"total time wasted on solver is {delta}, status: {status2}");
            SetResults(status2);

            void SetResults(CpSolverStatus statusHere)
            {
                if (statusHere == CpSolverStatus.Optimal || statusHere == CpSolverStatus.Feasible)
                {
                    var mods = allItems.GroupBy(x => x.modLink.mod as PartiaModelling);

                    foreach (var mod in mods)
                    {
                        var el = mod.Key;
                        el.ZadelCode = 0;
                        el.ZadelsGUIDs = null;

                        SessionLink.MasterDataLinks.ZadelsMasterData.EraseZadelsTree(el, 0, OpersToSdvigs, null);
                    }

                    for (int i = 0; i < allItems.Count; i++)
                    {
                        if (!allItems[i].itsStage) allItems[i].stageLink.StageMaterialsInfo.UsedLeftovers[allItems[i].MatIndex] = null; // очистим все на всякий случай
                            for (int b = 0; b < allBins.Count; b++)
                            {
                                var lit = xNew[i, b];
                                if (lit != null && solverNew.BooleanValue(lit))
                                {
                                    if (allItems[i].itsStage)
                                    {
                                    // Это ЗАДЕЛ для ВСЕГО ЭТАПА.
                                    var mod = allItems[i].modLink.mod as PartiaModelling;
                                        int code = SessionLink.MasterDataLinks.ZadelsMasterData.SetZadelToSdvig(mod, allBins[b].UniqueGUID, Uco.EmptyObject, IBtable[i, b].Count / 1000.0/*LinearLeftoversCount[allBins[b].UniqueGUID]*/);

                                    SessionLink.MasterDataLinks.ZadelsMasterData.EraseZadelsTree(mod, code, OpersToSdvigs, null);

                                    Console.WriteLine($"zadel set to stage {allItems[i].modLink.mod.FirstOperLink.ID} (место {allItems[i].MatIndex}) = {allBins[b].UniqueGUID}");
                                    }
                                    else
                                    {
                                        // Это остаток для материала.
                                        var list = allItems[i].stageLink.StageMaterialsInfo.UsedLeftovers;
                                        // Назначим ту номенклатуру, что у рюкзака - это может быть как оригинал, так и аналог.
                                        list[allItems[i].MatIndex] = new StageLeftover(allBins[b].MatData, IBtable[i, b].Count / 1000.0, IBtable[i, b].Orig, allBins[b].BinEdIzm);
                                    }
                                }
                            }
                    }
                }
            }

        }

        public List<LeftoverLackData> PlanAndFixMaterialsInfo(List<PlanSimulatorBlock> Blocks, IDictionary<int, DateTime> firstStageStartData, int PoolIndex)
        {
            List<LeftoverLackData> result = new();

            if (CurrentLeftoversCount == null || CurrentLeftoversCount.Count == 0) return result;

            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;

            var points = Blocks.SelectMany(block =>
                block.Lists.Select(list => (Date: firstStageStartData[list.FirstOperJust.Index], Object: (object)list)
                    //list => list.FirstOper.StageMaterialsInfo == null ? Array.Empty<(int, DateTime, Uco, double)>() : list.FirstOper.StageMaterialsInfo.UsedMaterials.Select(x => (list.FirstOper.Index, When: firstStageStartData[list.FirstOper.Index], x.Nomenklature, list.Count))
                )
                );

            var arrivalInfo = LeftoversArrivalInfo.SelectMany(pair => pair.Value.Select(x => (Date: x.When, Object: (object)(Material: pair.Key, Subdivision: x.Subdivision, Count: x.Count, EdIzm: x.EdIzm))));

            var pointsToAnalyze = points
                .Union(arrivalInfo)
                .OrderBy(point => point.Date).ToList();



            var howMuchAtStart = CurrentLeftoversCount.ToDictionary(x => x.Key, x =>
            {
                return x.Value.ToDictionary(y => y.Key, y => (Count: y.Value.Count - (UsedLeftoversFromBeggining.TryGetValue(x.Key.Seria.Object.Code, out var z) ? z : 0),
                y.Value.DimensionUnit));
            }); // Создаем независимую копию
            /*
             
            Dictionary<MaterialUco, Dictionary<CodedObjectVersioned<Guid, string, string>, Dictionary<Uco, double>>>
                howMuchAtStart = new();// = CurrentLeftoversCount.ToDictionary(x => x.Key, x => x.Value.ToDictionary()); // Создаем независимую копию

            foreach (var leftover in CurrentLeftoversCount)
            {
                var key1 = (leftover.Key.Nomenklature, leftover.Key.Characteristics);
                howMuchAtStart.TryAdd(key1, new());
                howMuchAtStart[key1].TryAdd(leftover.Key.Seria, leftover.Value.ToDictionary());
                howMuchAtStart[key1].TryAdd(CodedObjectVersioned<Guid, string, string>.EmptyObject, leftover.Value.ToDictionary());
            }
             */

            Dictionary<IAbleForTaraCalcInterface, Dictionary<Guid, LeftoversReplacementClass>> opsPossibleLeftoversForOne = new Dictionary<IAbleForTaraCalcInterface, Dictionary<Guid, LeftoversReplacementClass>>();

            for (int currentPoint = 0; currentPoint < pointsToAnalyze.Count; currentPoint++)
            {
                var point = pointsToAnalyze[currentPoint];
                if (point.Object is PlanSimulatorMod planMod)
                {
                    var startDate = firstStageStartData[planMod.FirstOperJust.Index];

                    List<IAbleForTaraCalcInterface> allMods = planMod.EasyLifeData == null ? new() { new PartiaModellingSmall(planMod.StagesAsNormal) } : planMod.EasyLifeData.Select(x => x.Value.AsMod).ToList();

                    var center = planMod.BlockLink.GetCenterData(PoolIndex).Center;

                    foreach (var mod in allMods)
                    {
                        var oper = mod.FirstOperLink;//OpersList[0];
                        var matsClass = oper.StageMaterialsInfo;
                        var matsList = matsClass.UsedMaterials;


                        var naladLengthData = planMod.EasyLifeData == null ? planMod.NaladLengthData[PoolIndex] : planMod.EasyLifeData[oper.CurrentModelling].NaladLengthData[PoolIndex];

                        var originalCount = matsList.Select(X => X.IsBuying ? GetMatSumFromMod(mod, X.AsMaterialUcoData()) : 0.0).ToList();//new List<double>();
                        

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

                            var origMatToFindHere = matsList[i].AsLeftoverUcoData();

                            // Добавляем ПУСТОТУ - новое обновление
                            var nothing = !howMuchAtStart.Any(x => x.Key.Nomenklature == origMatToFindHere.Nomenklature && x.Key.Characteristics == origMatToFindHere.Characteristics);
                            if (nothing)
                            {
                                howMuchAtStart.TryAdd(origMatToFindHere, new());
                            }

                            if (leftoversList[i] == null && nothing) // Нет ни одного похожего?? (раньше тут просто было
                            {
                                hasAnyErrorsHere = true;
                                break; // Ошибки тут очевидно есть, и надо назначить этому материалу аналог. Выходим отсюда и идём в заменялку.
                            }
                            if (howMuchAtStart.TryGetValue(leftoversList[i]?.LeftoverData ?? origMatToFindHere, out var existingCounts)
                                && (existingCounts.Count == 0 || existingCounts.MaxBy(x => x.Value).Value.Count < (leftoversList[i]?.CalculatedCount ?? originalCount[i]) + naladLengthData.Data.NaladLengthHere.Value))
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
                                var key = leftoversList[i]?.LeftoverData ?? matsList[i].AsLeftoverUcoData();
                                var valueToMinus = (leftoversList[i]?.CalculatedCount ?? originalCount[i]) + naladLengthData.Data.NaladLengthHere.Value;
                                if (howMuchAtStart.TryGetValue(key, out var neededCount))
                                {
                                    var elements = neededCount.Where(x => x.Value.Count >= valueToMinus).OrderByDescending(x => x.Key == oper.Subdivision ? 1 : 0).ThenByDescending(x => x.Value);
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


                                    var count = howMuchAtStart[key][pair.Key];
                                    count.Count -= valueToMinus;
                                    howMuchAtStart[key][pair.Key] = count;
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
                        var dictionariesReplacement = opsPossibleLeftoversForOne[mod].OrderByDescending(x => x.Value.List.Count).ThenByDescending(x => x.Value.List.Sum(y => howMuchAtStart.Sum(x =>
                        x.Key.Nomenklature == y.Analogue.Nomenklature
                        &&
                        x.Key.Characteristics == y.Analogue.Characteristics
                        ?
                        x.Value.Sum(y => y.Value.Count)
                        : 0)));
                        // Раньше там было просто howMuchAtStart[y.Analogue].Sum(x => x.Value), но из-за штрихкода теперь придётся суммировать ещё и по всем штрихкодам


                        foreach (var someGroup in dictionariesReplacement)
                        {
                            var listHere = someGroup.Value;
                            var resultedIndexes = listHere.List.Select(x => mats.FindIndex(mat => mat.Material.AsMaterialUcoData() == x.Original)).ToArray(); // Найдём сначала оригиналы.

                            // ElementIndex внутри mats = ссылка на индексы в основном массиве для простого поиска после фильтрации по IsBuying.
                            // resultedIndexes = индексы внутри mats после того как была найдена номенклатура
                            // resultedIndexes[i] - проход по всем найденным индексам
                            // mats[resultedIndexes[i]] - обращение к элементу в отфильтрованном списке
                            // mats[resultedIndexes[i]].ElementIndex - обращение к глобальному элементу

                            if (resultedIndexes.Any(x => x == -1)) continue; // Это значит, группа не подходит.

                            var analoguesWithoutSeries = listHere.List.Select(
                                (x, index) =>
                                {
                                    var count = (originalCount[mats[resultedIndexes[index]].ElementIndex] + naladLengthData.Data.NaladLengthHere.Value);
                                    return (AnalogueToSet: x.Analogue,
                            CountToSet: count / x.OriginalCount * x.AnalogueCount,
                            OriginalCount: count);
                                }
                            
                            ).ToArray(); // Создаём аналоги для каждого мата и вычисляем новое количество. В качестве аргумента originalCount[mats[resultedIndexes[index]].ElementIndex] - это количество уже моделирования, а не исходного этапчика. Пригодится для объединенных длин.

                            (LeftoverUco AnalogueToSet, double CountToSet, double OriginalCount, UcoDU EdIzm)[] analogues = analoguesWithoutSeries.Select(x =>
                            {
                                //var key1 =
                                return ((x.AnalogueToSet.Nomenklature, x.AnalogueToSet.Characteristics, CodedObjectVersioned<Guid, string, string>.EmptyObject), x.CountToSet, x.OriginalCount, UcoDU.EmptyObject);
                            }).ToArray();

                            bool bad = false;
                            for (int i = 0; i < analogues.Length; i++)
                            {
                                var filteredDictinary = howMuchAtStart.Where(x => x.Key.Nomenklature == analogues[i].AnalogueToSet.Nomenklature
                                && x.Key.Characteristics == analogues[i].AnalogueToSet.Characteristics).ToDictionary();

                                if (filteredDictinary.Count == 0)
                                {
                                    bad = true;
                                    break; // Тут break, потому что как минимум на один из материалов во всей группе нет аналога, значит выходим
                                }

                                //if (!howMuchAtStart.TryGetValue(analogues[i].AnalogueToSet, out var howLeft))
                                //{
                                //    bad = true;
                                //    break;
                                //}

                                var globalIndex = mats[resultedIndexes[i]].ElementIndex;

                                var matToSearchHere = matsList[globalIndex].AsMaterialUcoData();

                                bad = true; // Поставим пока bad
                                foreach (var filteredResult in filteredDictinary)
                                {
                                    var seria = filteredResult.Key.Seria;
                                    var howLeft = filteredResult.Value;

                                    if (howLeft.Any(x => x.Value.Count >= analogues[i].CountToSet) 
                                        || leftoversList[globalIndex] == null &&
                                        !howMuchAtStart.Any(much => much.Key.Nomenklature == matToSearchHere.Nomenklature && much.Key.Characteristics == matToSearchHere.Characteristics)) // Назначеннный остаток пуст + оригинальный мат не может быть выбран, так как не является остатком (причем из-за штрихкода приходится искать повсюду, а не только через ContainsKey)
                                    {
                                        // По идее это норм, можно идти дальше...
                                        analogues[i].AnalogueToSet.Seria = seria; // Задаём остатку серию
                                        analogues[i].EdIzm = howLeft.First().Value.DimensionUnit;
                                        bad = false;
                                        break;
                                    }
                                }
                                if (bad) break;

                            }

                            if (bad) continue; // Тоже не подходит!

                            // Задаём уже наконец-то данные
                            for (int i = 0; i < analogues.Length; i++)
                            {
                                var globalIndex = mats[resultedIndexes[i]].ElementIndex;
                                leftoversList[globalIndex] = new StageLeftover(analogues[i].AnalogueToSet, analogues[i].CountToSet, analogues[i].OriginalCount, analogues[i].EdIzm);
                            }
                            resultedIndexes = resultedIndexes.OrderByDescending(x => x).ToArray();
                            foreach (var index in resultedIndexes) mats.RemoveAt(index); // Отсортировав по убыванию, можно безопасно удалить элементы
                        }

                        for (int i = 0; i < matsList.Count; i++)
                        {
                            if (!matsList[i].IsBuying) continue;
                            (LeftoverUco Material, double Count, double OrigCount) whatWillDecrease;
                            if (matsClass.UsedLeftovers[i] == null)
                            {
                                var val = originalCount[i] + naladLengthData.Data.NaladLengthHere.Value;
                                whatWillDecrease = (matsList[i].AsLeftoverUcoData(), val, val); // тут налад длину вставляем потому что в оригинал каунт ее нет
                            }
                            else
                                whatWillDecrease = (leftoversList[i].LeftoverData, leftoversList[i].CalculatedCount, leftoversList[i].OriginalCount);

                            var filteredHowMuchAtStart = howMuchAtStart.Where(x => x.Key.Nomenklature == whatWillDecrease.Material.Nomenklature && x.Key.Characteristics == whatWillDecrease.Material.Characteristics && (matsClass.UsedLeftovers[i] == null ? true : (x.Key.Seria == whatWillDecrease.Material.Seria))).OrderByDescending(x => x.Value.Count);

                            if (filteredHowMuchAtStart.Count() > 0/*howMuchAtStart.TryGetValue(whatWillDecrease.Material, out var howMuchNow)*/)
                            {
                                var howMuchNow = filteredHowMuchAtStart.First().Value;

                                if (howMuchNow.Count == 0)
                                {
                                    ;

                                    DateTime? nextPrihod = GetPrihod(pointsToAnalyze, currentPoint, whatWillDecrease.Material);

                                    var lack = new LeftoverLackData(
                                        whatWillDecrease.Material,
                                        matsList[i].AsMaterialUcoData(),
                                        oper.GetPartiaCode() ?? Guid.Empty,
                                        whatWillDecrease.OrigCount,
                                        whatWillDecrease.Count,
                                        null,
                                        UcoDU.EmptyObject,
                                        //pair.Key,
                                        point.Date,
                                        nextPrihod ?? DateTime.MinValue,
                                        //nextPrihodSub,
                                        oper.ID,
                                        oper.CurrentModelling,
                                        oper.GetEasyFlagToOutput(),
                                        center,
                                        true
                                        );

                                    result.Add(lack);
                                }
                                else
                                {

                                    var pair = howMuchNow/*.Where(x => x.Value >= valueToMinus)*/.OrderByDescending(x => x.Key == oper.Subdivision ? 1 : 0).ThenByDescending(x => x.Value).First();
                                    var howMuchNowCount = pair.Value.Count;
                                    howMuchNowCount -= whatWillDecrease.Count;
                                    if (howMuchNowCount < 0)
                                    {
                                        DateTime? nextPrihod = GetPrihod(pointsToAnalyze, currentPoint, whatWillDecrease.Material);

                                        var lack = new LeftoverLackData(
                                            whatWillDecrease.Material,
                                            matsList[i].AsMaterialUcoData(),
                                            oper.GetPartiaCode() ?? Guid.Empty,
                                            whatWillDecrease.OrigCount,
                                            whatWillDecrease.Count,
                                            howMuchNowCount + whatWillDecrease.Count,
                                            pair.Value.DimensionUnit,
                                            //pair.Key,
                                            point.Date,
                                            nextPrihod ?? DateTime.MinValue,
                                            //nextPrihodSub,
                                            oper.ID,
                                            oper.CurrentModelling,
                                            oper.GetEasyFlagToOutput(),
                                            center,
                                            true
                                            );

                                        result.Add(lack);

                                    }
                                    else
                                    {

                                        var lack = new LeftoverLackData(
                                            whatWillDecrease.Material,
                                            matsList[i].AsMaterialUcoData(),
                                            oper.GetPartiaCode() ?? Guid.Empty,
                                            whatWillDecrease.OrigCount,
                                            whatWillDecrease.Count,
                                            howMuchNowCount + whatWillDecrease.Count,
                                            pair.Value.DimensionUnit,
                                            //pair.Key,
                                            point.Date,
                                            null,
                                            //null,
                                            oper.ID,
                                            oper.CurrentModelling,
                                            oper.GetEasyFlagToOutput(),
                                            center,
                                            false
                                            );

                                        result.Add(lack);

                                        //howMuchAtStart.TryAdd(whatWillDecrease.Material, new());
                                        var keyHere = filteredHowMuchAtStart.First().Key;
                                        howMuchAtStart[keyHere][pair.Key] = (howMuchNowCount, pair.Value.DimensionUnit); // Подсчитанное количество вбиваем, если оно все ещё больше 0.
                                    }
                                }


                            }


                        }
                    }
                }
                else
                {
                    // Это просто приход
                    (LeftoverUco Material, Uco Subdivision, double Count, UcoDU EdIzm) info = ((LeftoverUco Material, Uco Subdivision, double Count, UcoDU EdIzm))point.Object;
                    howMuchAtStart.TryAdd(info.Material, new());
                    howMuchAtStart[info.Material].TryAdd(info.Subdivision, (0, info.EdIzm));
                    var count = howMuchAtStart[info.Material][info.Subdivision];
                    count.Count += info.Count;
                    howMuchAtStart[info.Material][info.Subdivision] = count;
                }
            }


            return result;
        }

        DateTime? GetPrihod(List<(DateTime Date, object Object)> pointsToAnalyze, int currentPoint, LeftoverUco Material)
        {
            var n = pointsToAnalyze.FindIndex(currentPoint + 1, x =>
            {
                if (x.Object is PlanSimulatorMod) return false;
                (LeftoverUco Material, Uco Subdivision, double Count, UcoDU edIzm) info = ((LeftoverUco Material, Uco Subdivision, double Count, UcoDU edIzm))x.Object;
                // УЧТИ, У ПРИХОДЯЩИХ МАТЕРИАЛОВ СЕРИЯ И ШТРИХКОД ВСЕГДА ПУСТЫЕ.
                // НЕ СМОТРИМ НА НИХ, ПОТОМУ ЧТО МОЖЕТ БЫТЬ ТАКОЕ, ЧТО НЕ ХВАТАЕТ ОСТАТКА ВЫШЕ, А У НЕГО СЕРИЯ+ШК ЗАДАНЫ.
                // А НАМ НУЖНО ПРОСТО УЗНАТЬ, КОГДА ПРИЕДЕТ МАТЕРИАЛ С ТАКОЙ ЖЕ НОМЕНКЛ+ХАРКОЙ.
                return Material.Nomenklature == info.Material.Nomenklature
                && Material.Characteristics == info.Material.Characteristics; // Нам нужен приход того же материала
            }); // найдем следующий приход
            DateTime? nextPrihod = n == -1 ? null : pointsToAnalyze[n].Date;
            //Uco? nextPrihodSub = n == -1 ? null : (((LeftoverUco Material, Uco Subdivision, double Count))pointsToAnalyze[n].Object).Subdivision;

            return nextPrihod;
        }

    }
}

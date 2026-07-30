using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Microsoft.Extensions.Options;
using ApsRT.Core.Partias.PartiasClasses;
using ApsRT.Core.Partias.PartiasGeneratorEngine;
using UniversalPack.Extensions;
using ApsRT.Core.Taras.TaraSearchEngine;
using System.Collections.Concurrent;
using ApsRT.Core.Stages;

namespace ApsRT.Core.Partias.Zadels
{

    public class ZadelsMasterData
    {

        public Session.Session SessionLink { get; }

        public ZadelsMasterData(Session.Session session)
        {
            SessionLink = session;
        }

        internal Dictionary<Guid, // гуид партии пф
            Dictionary<OrderID, // ГПшка
                Dictionary<int, // конкретный УРОВЕНЬ
                    Dictionary<Uco, // номенклатура
                        Dictionary<Uco, // Хар-ка
                            Dictionary<int, // номер повива (0 = -1, т.е. любой)
                                            //Dictionary<Guid, // гуид этапа (тут всегда пустой гуид один, если НЕ конкретный этап
                                Dictionary<Guid, ConcreteZadel> // список конкретных заделов, которые тут перечислены
                                >
                            >
                        //>
                        >
                    >
                >
            > ZadelsData = new();

        internal Dictionary<Guid, ConcreteZadel> LinearZadelsDictionary = new();

        internal void AddZadelData(ZadelDataLoaderModel model)
        {

            // 1. Сначала гуид партии. Если он не задан, значит это глюк таблицы - пустая строка.
            if (model.PartiaPF.HasValue)
            {
                var partiaPF = model.PartiaPF.Value;
                if (!ZadelsData.ContainsKey(partiaPF))
                {
                    ZadelsData.Add(partiaPF, new());
                }

                // 2. Превращаем в ГП
                var finalEtap = model.StageGuid;
                var OperationsDataSet = SessionLink.MasterDataLinks.StagesMasterData;
                var id = OperationsDataSet.TryGetStageIndexByGuid(finalEtap.Object.Code, finalEtap.Version);
                Stage? op = null;
                if (true/*id != -1 && */)
                {
                    OperationsDataSet.AllStages.TryGetValue(id, out op);
                    var order = op?.OrderLink.OrderID ?? OrderID.EmptyObject;
                    if (!ZadelsData[partiaPF].ContainsKey(order))
                    {
                        ZadelsData[partiaPF].Add(order, new());
                    }

                    // 3. Уровень
                    var level = model.Level ?? 0;
                    if (!ZadelsData[partiaPF][order].ContainsKey(level))
                    {
                        ZadelsData[partiaPF][order].Add(level, new());
                    }

                    // 4. Номенклатура
                    var nomen = model.Nomenklature;
                    if (!ZadelsData[partiaPF][order][level].ContainsKey(nomen))
                    {
                        ZadelsData[partiaPF][order][level].Add(nomen, new());
                    }

                    // 4.1 Характеристика
                    var chars = model.Characteristics;
                    if (!ZadelsData[partiaPF][order][level][nomen].ContainsKey(chars))
                    {
                        ZadelsData[partiaPF][order][level][nomen].Add(chars, new());
                    }

                    // 5. Номер повива
                    int poviv = model.Poviv ?? 0;
                    if (!ZadelsData[partiaPF][order][level][nomen][chars].ContainsKey(poviv))
                    {
                        ZadelsData[partiaPF][order][level][nomen][chars].Add(poviv, new());
                    }

                    // 6. Последнее - сам задел
                    var zadelGuid = model.ZadelGuid ?? (Guid?)(Guid.NewGuid());

                    // 6.1 Штрихкод ещё
                    var seria = model.SeriaCode;

                    if (zadelGuid.HasValue && zadelGuid.Value != Guid.Empty)
                    {
                        if (!LinearZadelsDictionary.ContainsKey(zadelGuid.Value))
                        {
                            var tara = model.Tara;
                            var justTara = model.JustTara;
                            var newZadel = new ConcreteZadel(tara, justTara, level, poviv, nomen, chars, finalEtap, partiaPF, seria);
                            newZadel.ZadelStatus = ZadelStatus.CannotBeFound;
                            LinearZadelsDictionary.Add(zadelGuid.Value, newZadel);
                        }

                        var zadel = LinearZadelsDictionary[zadelGuid.Value]; // берем ссылку на задел и добавляем инфу про ГП в него
                        //zadel.RelatedGPs.Add(order);

                        //if (!USE_CONCRETE_ETAPS_GUIDS) finalEtap = Guid.Empty;

                        if (!zadel.WhereCanUsed.ContainsKey(finalEtap)) // Если такого задела нет, то мы его создаём
                        {
                            zadel.WhereCanUsed.Add(finalEtap, model.Count);
                        }
                        zadel.Count = Math.Max(zadel.Count, model.Count); // Обновляем количество.

                        if (!ZadelsData[partiaPF][order][level][nomen][chars][poviv]/*[finalEtap]*/.ContainsKey(zadelGuid.Value)) // Если такого задела нет, то мы его создаём
                        {
                            ZadelsData[partiaPF][order][level][nomen][chars][poviv]/*[finalEtap]*/.Add(zadelGuid.Value, zadel);
                        }

                    }
                }
            }
        }

        internal void ClearAll()
        {
            ZadelsData.Clear();
            LinearZadelsDictionary.Clear();
        }
    
        public void PreparePartiasForLockedZadels(List<PartiaClass> partias)
        {
            //bool freeZadels = !SessionLink.SessionOptions.LaunchZadel;
            foreach (var partia in partias)
            {
                var partia1 = partia.OpersHere;
                Dictionary<int, PartiaModelling> OpersToSdvigs = PartiasConnectorEngine.PartiasConnectorEngine.SeeSdvigsAsDic(partia1);
                Dictionary<int, (Uco Tara, double namot)> OriginallyTaraWasThis = OpersToSdvigs.ToDictionary(x => x.Key, x => (x.Value.Tara, x.Value.Max));


                // Нужно нафиг эту систему переделать, чтоб потом удобнее искать было
                for (int i = 0; i < partia1.Count; i++) // Проходимся по всем уровням
                {
                    for (int j = 0; j < partia1[i].Count; j++)
                    {
                        var opHere = partia1[i][j].Opers[0]; // потому что они тут всё равно всегда одни.

                        var finalZadelsHere = GetPossibleZadels(false, partia.MyGuid, opHere);

                        if (finalZadelsHere != null && finalZadelsHere.Count > 0)
                        {
                            // теперь тут надо найти ЛИБО гуид этого этапа, либо гуид нулевой
                            //var guidToFind = Zadels.USE_CONCRETE_ETAPS_GUIDS ? opHere.GUID.Value : Guid.Empty;

                            // Нашли? значит переходим к заделам
                            foreach (var zadelV in finalZadelsHere)
                            {
                                var zadel = zadelV.Value;

                                if (zadel.ZadelStatus == ZadelStatus.SussesfullyUsed) continue; // Игнорим, он же использован уже.
                                // Чекаем, сколько у нас тут осталось
                                double countToSet = 0;
                                if (zadel.WhereCanUsed.TryGetValue(opHere.ID, out countToSet)
                                    || zadel.WhereCanUsed.TryGetValue(new UniversalPack.Common.CodedObjectVersioned<Guid, string, int>(opHere.ID.Object, 0), out countToSet))//) // сюда можно сунуть толерантность ещё. 18.06: внезапно выяснилось, что есть право выдать задел, который совершенно не подходит по количеству - и мол может встать, под ответственность юзера.
                                                  // 19.06: если задела вообще не осталось, то операции назначить его не получится.
                                {
                                    if (partia1[i][j].ZadelsGUIDs != null && partia1[i][j].ZadelsGUIDs.Count == opHere.PlacesCount)
                                    {
                                        // Он уже сюда не влезет. Другие заделы использовали
                                        //if (freeZadels && zadel.ZadelStatus == ZadelStatus.CannotBeFound)
                                        zadel.ZadelStatus = ZadelStatus.AnotherUsed;
                                    }
                                    else
                                    {
                                        // Место ещё есть. Суем
                                        //var finalCount = Math.Max(0, opHere.Count);

                                        zadel.WhereIsUsed.Add((opHere.Index, countToSet)); // добавляем моделирования (теперь уже - ссылка на первую операцию, все равно они там объединяются в одно моделирование) и количество

                                        //zadel.CalcCount -= finalCount; // отнимает из общего количества

                                        zadel.ZadelStatus = ZadelStatus.SussesfullyUsed;

                                        var guid = zadelV.Key;

                                        int code = SetZadelToSdvig(partia1[i][j], guid, zadel.UsedTara, countToSet);

                                        //var code = partia1[i][j].ZadelCode;

                                        //var codeToAdd = guid.GetHashCode();

                                        //if (code == 0)
                                        //    code = codeToAdd;
                                        //else
                                        //    code = HashCode.Combine(code, codeToAdd);

                                        //partia1[i][j].ZadelsGUIDs.Add(guid);
                                        //partia1[i][j].ZadelCode = code;

                                        //partia1[i][j].Tara = zadel.UsedTara;
                                        //partia1[i][j].Max = countToSet;

                                        EraseZadelsTree(partia1[i][j], code);
                                    }
                                }
                                else
                                {
                                    if (zadel.WhereIsUsed.Count == 0) // Она нигде ещё не юзалась?
                                    {
                                        zadel.ZadelStatus = ZadelStatus.NotEnoughCount;
                                    }
                                }
                            }
                        }

                    }
                }

                // У всех предыдущих стираем всё
                void EraseZadelsTree(PartiaModelling sdvig, int zadelCode)
                {
                    foreach (var prev in sdvig.Opers.SelectMany(x => x.Prevs)) // смотрим предков
                    {
                        if (OpersToSdvigs.TryGetValue(prev, out var sdvigP))
                        {
                            sdvigP.ZadelCode = zadelCode;
                            sdvigP.Tara = OriginallyTaraWasThis[prev].Tara; // Возвращаем тару на месту, если там была перезапись
                            sdvigP.Max = OriginallyTaraWasThis[prev].namot; // Возвращаем тару на месту, если там была перезапись
                            if (sdvigP.ZadelsGUIDs != null)
                            {
                                foreach (var zadelGuid in sdvigP.ZadelsGUIDs)
                                {
                                    //Zadels.LinearZadelsDictionary[zadelGuid].CalcCount = Zadels.LinearZadelsDictionary[zadelGuid].Count; // Очищаем инфу в заделах
                                    LinearZadelsDictionary[zadelGuid].WhereIsUsed.Clear(); // всё стираем
                                    LinearZadelsDictionary[zadelGuid].ZadelStatus = ZadelStatus.HigherZadelUsed;
                                }
                                sdvigP.ZadelsGUIDs.Clear();
                            }
                            EraseZadelsTree(sdvigP, zadelCode);
                        }
                    }
                }
            }
        }


        public void PreparePartiasForFreeZadels(List<PartiaClass> partias, ConcurrentDictionary<PartiaClass, ConcurrentDictionary<PartiaClass, (int Total, float K, int Diff, int LongestModsMaxCount, int LastLevel)>> DEEP)
        {
            var orderedZadels = LinearZadelsDictionary.OrderBy(x => x.Value.Level).ToDictionary();

            Dictionary<Guid, Dictionary<int, PartiaModelling>> OpersToSdvigsGlobal = new();
            Dictionary<Guid, Dictionary<int, (Uco Tara, double namot)>> OriginallyTaraWasThisGlobal = new();

            foreach (var zadel in orderedZadels)
            {
                partias = partias.OrderBy(x => DEEP.TryGetValue(x, out var res) ? (res.Where(y => y.Value.LastLevel > zadel.Value.Level).Count()) : 0).ToList(); // Подсчитаем, сколько раз она теоретически сойдется с кем-нибудь на этом уровне
                foreach (var partia in partias)
                {
                    var partia1 = partia.OpersHere;
                    Dictionary<int, PartiaModelling> OpersToSdvigs;
                    if (!OpersToSdvigsGlobal.TryGetValue(partia.MyGuid, out OpersToSdvigs))
                    {
                        var res = PartiasConnectorEngine.PartiasConnectorEngine.SeeSdvigsAsDic(partia1);
                        OpersToSdvigsGlobal.Add(partia.MyGuid, res);
                        OpersToSdvigs = OpersToSdvigsGlobal[partia.MyGuid];
                    }

                    Dictionary<int, (Uco Tara, double namot)> OriginallyTaraWasThis;
                    if (!OriginallyTaraWasThisGlobal.TryGetValue(partia.MyGuid, out OriginallyTaraWasThis))
                    {
                        var res = OpersToSdvigs.ToDictionary(x => x.Key, x => (x.Value.Tara, x.Value.Max));
                        OriginallyTaraWasThisGlobal.Add(partia.MyGuid, res);
                        OriginallyTaraWasThis = OriginallyTaraWasThisGlobal[partia.MyGuid];
                    }

                    for (int i = 0; i < partia1.Count; i++) // Проходимся по всем уровням
                    {
                        for (int j = 0; j < partia1[i].Count; j++)
                        {

                            if (partia1[i][j].ZadelsGUIDs.Count > 0 && LinearZadelsDictionary[partia1[i][j].ZadelsGUIDs[0]].UsedTara != zadel.Value.UsedTara)
                            {
                                // Этот задел нельзя тут поставить, потому что тут другая тара.
                                continue;
                            }

                            var opHere = partia1[i][j].Opers[0]; // потому что они тут всё равно всегда одни.

                            if ((partia1[i][j] as IAbleForTaraCalcInterface).Sum <= zadel.Value.Count) // Задел может сюда влезть по количеству?
                            {
                                var finalZadelsHere = GetPossibleZadels(true, partia.MyGuid, opHere); // Найдем все доступные заделы для данного этапчика, но на этот раз они могут быть уже свободными
                                if (finalZadelsHere != null && finalZadelsHere.ContainsKey(zadel.Key)) // Текущий рассматриваемый задел может сюда встать?
                                {
                                    if (partia1[i][j].ZadelsGUIDs.Count == opHere.PlacesCount)
                                    {
                                        // Он сюда уже не влезет. Можно будет че нить вывести если надо
                                    }
                                    else
                                    {
                                        var guid = zadel.Key;

                                        int code = SetZadelToSdvig(partia1[i][j], guid, zadel.Value.UsedTara, zadel.Value.Count);

                                        EraseZadelsTree(partia1[i][j], code, OpersToSdvigs, OriginallyTaraWasThis);
                                    }
                                }
                            }

                        }
                    }

                }
            }
        }


        // У всех предыдущих стираем всё
        public void EraseZadelsTree(PartiaModelling sdvig, int zadelCode, Dictionary<int, PartiaModelling> OpersToSdvigs, Dictionary<int, (Uco Tara, double namot)> OriginallyTaraWasThis)
        {
            foreach (var prev in sdvig.Opers.SelectMany(x => x.Prevs)) // смотрим предков
            {
                if (OpersToSdvigs.TryGetValue(prev, out var sdvigP))
                {
                    sdvigP.ZadelCode = zadelCode;
                    if (OriginallyTaraWasThis != null)
                    {
                        sdvigP.Tara = OriginallyTaraWasThis[prev].Tara; // Возвращаем тару на месту, если там была перезапись
                        sdvigP.Max = OriginallyTaraWasThis[prev].namot; // Возвращаем тару на месту, если там была перезапись
                    }
                    sdvigP.ZadelsGUIDs = new();
                    EraseZadelsTree(sdvigP, zadelCode, OpersToSdvigs, OriginallyTaraWasThis);
                }
            }
        }

        public int SetZadelToSdvig(PartiaModelling partia1, Guid guid, Uco tara, double max)
        {
            var code = partia1.ZadelCode;


            var codeToAdd = guid.GetHashCode();

            if (code == 0)
                code = codeToAdd;
            else
                code = HashCode.Combine(code, codeToAdd);
            partia1.ZadelsGUIDs ??= new();
            partia1.ZadelsGUIDs.Add(guid);
            partia1.ZadelCode = code;

            partia1.Tara = tara;
            partia1.Max = partia1.Max == 0 ? max : Math.Min(max, partia1.Max);

            return code;
        }

        public void UpdateFreeZadelsStatuses(List<PartiaClass> partiasToCheck)
        {
            foreach (var partia in partiasToCheck)
            {
                var OpersToSdvigs = partia.SeeSdvigsAsDic();
                var ModsWithUsedZadels = OpersToSdvigs.Where(y => y.Value.ZadelsGUIDs != null && y.Value.ZadelsGUIDs.Count > 0).Select(x => x.Value).Distinct().ToList();

                foreach (var mod in ModsWithUsedZadels)
                {
                    for (int i = 0; i < mod.ZadelsGUIDs.Count; i++)
                    {
                        var zadel = mod.ZadelsGUIDs[i];
                        if (LinearZadelsDictionary.TryGetValue(zadel, out var zadelClass))
                        {
                            zadelClass.ZadelStatus = ZadelStatus.SussesfullyUsed;
                            foreach (var oper in mod.Opers)
                            {
                                //oper.ZadelsInherited = zadelsCount;
                                zadelClass.WhereIsUsed.Add((oper.Index, oper.Count/*(mod as IAbleForTaraCalcInterface).Sum*/));
                            }
                        }
                        else
                        {
                            // Значит это остатки из таблицы остатков
                            var dic = SessionLink.MasterDataLinks.LeftoversMasterData.UsedLeftoversFromBeggining;
                            dic.TryAdd(zadel, new());
                            //var index = mod.Opers[0].Index;
                            dic[zadel] += mod.Opers.Sum(x => x.Count);
                            //dic[zadel].TryAdd(index, 0);
                            //dic[zadel][index]++;

                            var dic2 = SessionLink.MasterDataLinks.LeftoversMasterData.UsedLeftoversFromBegginingData;
                            dic2.TryAdd(zadel, new());
                            foreach (var op in mod.Opers)
                            {
                                var val = (op.Index, i);
                                //if (dic2[zadel].Contains(val))
                                //{
                                //    ;
                                //}
                                dic2[zadel].Add(val);
                                //Console.WriteLine($"{mod};{partia};{zadel};{op.Index};{i}");
                            }
                        }
                    }

                    // Записываем инфу о закрытых заделах
                    WriteZadelsTree(mod, mod.ZadelsGUIDs.Count);
                }

                // У всех предыдущих стираем всё
                void WriteZadelsTree(PartiaModelling sdvig, int zadelsCount)
                {
                    foreach (var oper in sdvig.Opers)
                    {
                        oper.ZadelsInherited = zadelsCount;
                    }
                    var prevs = sdvig.Opers.SelectMany(x => x.Prevs).Distinct();
                    foreach (var prev in prevs) // смотрим предков
                    {
                        if (OpersToSdvigs.TryGetValue(prev, out var sdvigP))
                        {
                            WriteZadelsTree(sdvigP, zadelsCount);
                        }
                    }
                }
            }
        }

        public Dictionary<Guid, ConcreteZadel> GetPossibleZadels(bool freeZadels, Guid partiaPFguid, Stages.Stage stage)
        {
            // Сначала поищем инфу о партиях.

            Dictionary<OrderID, // ГПшка
                    Dictionary<int, // конкретный УРОВЕНЬ
                        Dictionary<Uco, // номенклатура
                            Dictionary<Uco, // хар ка
                                Dictionary<int, // номер повива (0 = -1, т.е. любой)
                                            //Dictionary<Guid, // гуид этапа (тут всегда пустой гуид один, если НЕ конкретный этап
                                    Dictionary<Guid, ConcreteZadel> // список конкретных заделов, которые тут перечислены
                                    >
                                >
                            //>
                            >
                        >
                    > zadelsGPsHere = new();
            if (ZadelsData.TryGetValue(partiaPFguid, out var res1)) zadelsGPsHere = zadelsGPsHere.Union(res1).ToDictionary();
            if (freeZadels && ZadelsData.TryGetValue(Guid.Empty, out var res2)) zadelsGPsHere = zadelsGPsHere.Union(res2).ToDictionary();


            if (zadelsGPsHere.Count > 0)
            {
                Dictionary<int, // конкретный УРОВЕНЬ
                    Dictionary<Uco, // номенклатура
                        Dictionary<Uco, // хар ка
                                Dictionary<int, // номер повива (0 = -1, т.е. любой)
                                                //Dictionary<Guid, // гуид этапа (тут всегда пустой гуид один, если НЕ конкретный этап
                                    Dictionary<Guid, ConcreteZadel> // список конкретных заделов, которые тут перечислены
                                    >
                                >
                        //>
                        >
                    > zadelsLevelsHere = new();

                if (zadelsGPsHere.TryGetValue(stage.OrderLink.OrderID, out var res11)) zadelsLevelsHere = zadelsLevelsHere.Union(res11).ToDictionary();
                if (/*freeZadels && */zadelsGPsHere.TryGetValue(OrderID.EmptyObject, out var res22)) zadelsLevelsHere = zadelsLevelsHere.Union(res22).ToDictionary();

                if (zadelsLevelsHere.Count > 0)
                {
                    var levelHere = stage.Level;//partia1[i][0].Opers[0].Level;

                    Dictionary<Uco, // номенклатура
                    Dictionary<Uco, // хар ка
                                Dictionary<int, // номер повива (0 = -1, т.е. любой)
                                                //Dictionary<Guid, // гуид этапа (тут всегда пустой гуид один, если НЕ конкретный этап
                                    Dictionary<Guid, ConcreteZadel> // список конкретных заделов, которые тут перечислены
                                    >
                                >
                    //>
                    > zadelsNomensHere = new();

                    if (zadelsLevelsHere.TryGetValue(levelHere, out var res111)) zadelsNomensHere = zadelsNomensHere.Union(res111).ToDictionary();
                    if (freeZadels && zadelsLevelsHere.TryGetValue(0, out var res222)) zadelsNomensHere = zadelsNomensHere.Union(res222).ToDictionary();

                    if (zadelsNomensHere.Count > 0)
                    {
                        Dictionary<Uco, // хар ка
                                    Dictionary<int, // номер повива (0 = -1, т.е. любой)
                                                    //Dictionary<Guid, // гуид этапа (тут всегда пустой гуид один, если НЕ конкретный этап
                                        Dictionary<Guid, ConcreteZadel> // список конкретных заделов, которые тут перечислены
                                        >
                        //>
                        > zadelsCharsHere = new();

                        if (zadelsNomensHere.TryGetValue(stage.Nomenklature, out var res111111)) zadelsCharsHere = zadelsCharsHere.Union(res111111).ToDictionary();
                        if (freeZadels && zadelsNomensHere.TryGetValue(Uco.EmptyObject, out var res222222)) zadelsCharsHere = zadelsCharsHere.Union(res222222).ToDictionary();

                        if (zadelsCharsHere.Count > 0)
                        {
                            //var opHere = partia1[i][j].Opers[0]; // потому что они тут всё равно всегда одни.

                            Dictionary<int, // номер повива (0 = -1, т.е. любой)
                                            //Dictionary<Guid, // гуид этапа (тут всегда пустой гуид один, если НЕ конкретный этап
                            Dictionary<Guid, ConcreteZadel> // список конкретных заделов, которые тут перечислены
                            > zadelsPovivsHere = new();

                            if (zadelsCharsHere.TryGetValue(stage.Characteristics, out var res1111)) zadelsPovivsHere = zadelsPovivsHere.Union(res1111).ToDictionary();
                            if (freeZadels && zadelsCharsHere.TryGetValue(Uco.EmptyObject, out var res2222)) zadelsPovivsHere = zadelsPovivsHere.Union(res2222).ToDictionary();

                            if (zadelsPovivsHere.Count > 0)
                            {
                                Dictionary<Guid, ConcreteZadel> finalZadelsHere = new();

                                if (zadelsPovivsHere.TryGetValue(stage.Poviv, out var res11111)) finalZadelsHere = finalZadelsHere.Union(res11111).ToDictionary();
                                if (freeZadels && zadelsPovivsHere.TryGetValue(0, out var res22222)) finalZadelsHere = finalZadelsHere.Union(res22222).ToDictionary();

                                // Номенклатуру нашли, теперь ищем повивы, и если находим - находим наконец-то заделы.

                                return finalZadelsHere.Where(x => x.Value.WhereCanUsed.ContainsKey(stage.ID)
                                || x.Value.WhereCanUsed.ContainsKey(new UniversalPack.Common.CodedObjectVersioned<Guid, string, int>(stage.ID.Object, 0)) // Поищу по 0 версии
                                || (freeZadels && x.Value.WhereCanUsed.ContainsKey(UniversalPack.Common.CodedObjectVersioned<Guid, string, int>.EmptyObject))).ToDictionary();
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void ClearZadelsUsageInfo()
        {
            foreach (var zadel in LinearZadelsDictionary)
            {
                //zadel.Value.CalcCount = zadel.Value.Count;
                zadel.Value.WhereIsUsed.Clear();
                zadel.Value.ZadelStatus = Zadels.ZadelStatus.CannotBeFound;
            }
        }

    }
}

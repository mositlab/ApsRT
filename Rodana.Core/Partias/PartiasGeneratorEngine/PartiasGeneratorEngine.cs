using Azure;
using Rodana.Core.Partias.PartiasClasses;
using Rodana.Core.Partias.TaraSearchEngine;
using Rodana.Core.Stages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Rodana.Core.Partias.Zadels;

namespace Rodana.Core.Partias.PartiasGeneratorEngine
{
    public static class PartiasGeneratorEngine
    {

        public static List<List<PartiaModelling>> DeepCopy(List<List<PartiaModelling>> original)
        {
            var copied = new List<List<PartiaModelling>>();
            foreach (var list0 in original)
            {
                var listhere = new List<PartiaModelling>();
                foreach (var list1 in list0)
                {
                    var listhere2 = new PartiaModelling(new List<Stages.Stage>(), list1.Tara, list1.Max, list1.Format, list1.RC, list1.ZadelCode,
                        list1.ZadelsGUIDs != null ? new(list1.ZadelsGUIDs) : null);
                    //listhere2.RCIsChecked = list1.RCIsChecked;
                    foreach (var list2 in list1.Opers)
                    {
                        listhere2.Opers.Add(list2);
                    }
                    listhere.Add(listhere2);
                }
                copied.Add(listhere);
            }
            return copied;
        }

        static IEnumerable<PartiaModelling> selectSdvigs(IGrouping<Guid, Stage> group)
        {
            if (group.Key == Guid.Empty)
            {
                // Это не моделирование, а элементарные операции
                foreach (var op in group)
                {
                    yield return new PartiaModelling(new List<Stage>() { op }, Uco.EmptyObject, 0, DimensionUnit.Unknown, Uco.EmptyObject, 0, null);
                }

            }
            else
            {
                // Это моделирование. Создаём из него список
                yield return //checkThisList(group.ToList());
                    new PartiaModelling(group.ToList(), Uco.EmptyObject, 0, DimensionUnit.Unknown, Uco.EmptyObject, 0, null);
            }
        }
        public static List<List<PartiaModelling>> GenerateTree(List<Stage> ops, Color col, bool WithLog, Guid? partiaPFguid)
        {
            if (ops == null || ops.Count == 0)
            {
                return null;
            }

            var partia1 = new List<List<PartiaModelling>>();

            var engineData = new PartiasGeneratorEngineData();
            engineData.Session = ops[0].SessionLink;
            engineData.Stages = ops;
            var ALL = engineData.Session.MasterDataLinks.StagesMasterData.AllStages;

            var DoneIDS = engineData.DoneIDS;
            //var OriginallyTaraWasThis = engineData.OriginallyTaraWasThis;
            var RestrictionsVia74110 = engineData.RestrictionsVia74110;
            //var OpersToSdvigs = engineData.OpersToSdvigs;
            //var prevs = engineData.prevs;
            engineData.Color = col;


            var Level = 1;
            var max = ops.Max(x => x.Level);


            while (Level <= max)
            {
                var CurrentLevel1 = ops
                    .Where(x => x.Level == Level)
                    .GroupBy(x => x.MesModelling.Code) // достаточно лишь того, который пришёл из меса, поэтому так

                    .SelectMany(group => selectSdvigs(group)).ToList();

                if (CurrentLevel1.Count > 0)
                {
                    partia1.Add(CurrentLevel1);

                    foreach (var op in CurrentLevel1)
                    {
                        DoneIDS.Add(op.Opers[0].Index);
                        //if (NeededIDsForLast.Contains(op.Opers[0].oper.ID) || op.Opers[0].oper.IsLast)
                        //{
                        //    foreach (var opN in op.Opers[0].oper.Nexts)
                        //        NeededIDsForLast.Add(opN);
                        //}
                    }
                }
                Level++;
            }

            // Проверочки 74-110
            var dicSdvigs = PartiasConnectorEngine.PartiasConnectorEngine.SeeSdvigsAsDic(partia1);
            var sdvigs = partia1.SelectMany(x => x).ToList();
            foreach (var sdvig in sdvigs)
            {
                var prevsIds = sdvig.Opers.SelectMany(x => x.Prevs).Distinct().Select(x => ALL[x]).ToList();

                for (int i = 0; i < prevsIds.Count; i++)
                {
                    if (prevsIds[i].NoChangeTara)
                    {
                        prevsIds.AddRange(prevsIds[i].Prevs.Select(x => ALL[x]));
                        prevsIds.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (prevsIds[i].IsVspom/* || finalOps[i].UsesVspomTara || IDtoSdvig[finalOps[i].ID].ZadelCode != 0*/)
                    {
                        prevsIds.RemoveAt(i);
                        i--;
                    }
                }
                var prevsSdvigs = prevsIds.Select(x => dicSdvigs[x.Index]).ToList();
                if (prevsSdvigs.Count > 1)
                {
                    // Больше 1. Изучаем все РЦ, куда можно поставить.
                    var possibleRCs = sdvig.Opers[0].PossibleCenters ?? new() { sdvig.Opers[0].OriginalCenter };
                    foreach (var rc in possibleRCs) // пройдемся по каждому РЦ и найдём вариант, при котором все prevsSdvigs смогут найти себе пару.
                    {
                        List<(bool Can, Uco Tara, PartiaModelling sdvig)> analysis = prevsSdvigs.Select(sdvigToCheck =>
                        {
                            bool can = TaraSearchEngine.TaraSearchEngine.SumIsPossible(sdvigToCheck, out var tara, out _, out _, out _, out _, out _, new SumIsPossibleOptions(false) { LogSomeErrors = false, NextRCsExcludeNoChangeTara = true, NextRCsMustBeThese = new() { rc } });
                            return (can, tara, sdvigToCheck);
                        }).ToList();

                        if (analysis.All(x => x.Can))
                        {
                            foreach (var an in analysis) RestrictionsVia74110.TryAdd(an.sdvig, an.Tara);
                            break; // Ну и уходим отсюда тут же
                        }
                    }
                }
            }


            // Постобработка
            for (int i = 0; i < partia1.Count; i++)
            {
                for (int j = 0; j < partia1[i].Count; j++)
                {
                    partia1[i][j] = checkThisList(partia1[i][j], engineData, WithLog);
                }
            }

            // Устроим себе создание ЗАДЕЛОВ
            ZadelsCreation(partia1, engineData, partiaPFguid);


            return partia1;
        }


        static PartiaModelling checkThisList(PartiaModelling sdvigWas, PartiasGeneratorEngineData engineData, bool WithLog)
        {
            var DoneIDS = engineData.DoneIDS;
            var OriginallyTaraWasThis = engineData.OriginallyTaraWasThis;
            var RestrictionsVia74110 = engineData.RestrictionsVia74110;
            var OpersToSdvigs = engineData.OpersToSdvigs;
            var prevs = engineData.prevs;
            var ALL = engineData.Session.MasterDataLinks.StagesMasterData.AllStages;
            var col = engineData.Color;

            var opers = sdvigWas.Opers;
            // TODO: если уж так нужен флаг будет, то добавить его можно будет где-то здесь.
            foreach (var op in opers) op.Color = col;

            var x = opers[0];


            // Это вспомогательная? В таком случае на тару пока забиваем
            if (x.IsVspom)
            {
                //var sdvg = ;
                //sdvg.RCIsChecked = true; // У вспомогательных операций нефиг РЦ проверять. Всё равно ничего полезного не найдем. Поэтому ставим первый попавшийся и забываем.
                // Так как очень часто создаются глубокие копии, то здесь назначать фиксацию РЦ бессмысленно, и было перетащено в CONNECT.
                var tarao = Uco.EmptyObject;
                //OriginallyTaraWasThis.Add(x.ID, (tarao, 0));


                var sdvig = new PartiaModelling(opers, tarao, 0.0, DimensionUnit.Unknown, x.PossibleCenters?[0] ?? x.OriginalCenter, 0,
                    new());
                addInfoLocal(opers, tarao, 0, sdvig, false, engineData);
                //OpersToSdvigs.Add(x.ID, sdvig);
                return sdvig;
            }

            // Основная функция - поиск тары

            Uco? trr = RestrictionsVia74110 == null ? null : (RestrictionsVia74110.TryGetValue(sdvigWas, out var rtara) ? rtara : null);
            if (TaraSearchEngine.TaraSearchEngine.SumIsPossible(new PartiaModellingSmall(opers), out var tara, out var rc, out var max, out var form, out _, out _, new SumIsPossibleOptions(WithLog) { PrevsInfo = prevs, TaraMustBeThis = trr, LogSomeErrors = false }))
            {
                // Нашлась, отработала хорошо.
                // Пометки, что этот РЦ проверен, пока не ставится. В случае ненахода норм РЦ или перераспределений итд, потом в партиях с помощью "метода поиска проверенного РЦ" он в случае чего заменится.
                //prevs.Add(x.ID, tara);
                //OriginallyTaraWasThis.Add(x.ID, (tara, max));
                var sdvig = new PartiaModelling(opers, tara, max, form, rc, 0,
                    new());
                if (trr.HasValue) sdvig.RCIsChecked = true; // блокируем возможность что-то поставить
                addInfoLocal(opers, tara, max, sdvig, true, engineData);
                //OpersToSdvigs.Add(x.ID, sdvig);
                return sdvig;
            }
            else
            {
                if (TaraSearchEngine.TaraSearchEngine.SumIsPossible(new PartiaModellingSmall(opers), out tara, out rc, out max, out form, out _, out _, new SumIsPossibleOptions(WithLog) { PrevsInfo = prevs }))
                {
                    // Нашлась, отработала хорошо.
                    // Пометки, что этот РЦ проверен, пока не ставится. В случае ненахода норм РЦ или перераспределений итд, потом в партиях с помощью "метода поиска проверенного РЦ" он в случае чего заменится.
                    //prevs.Add(x.ID, tara);
                    //OriginallyTaraWasThis.Add(x.ID, (tara, max));
                    var sdvig = new PartiaModelling(opers, tara, max, form, rc, 0,
                        new());
                    addInfoLocal(opers, tara, max, sdvig, true, engineData);
                    //OpersToSdvigs.Add(x.ID, sdvig);
                    return sdvig;
                }
            }

            // Последняя попытка - если стоит "не менять тару", то берём у предыдущего этапа тару
            if (x.NoChangeTara)
            {
                var firstGoodPrevIndex = x.Prevs.Count == 0 ? -1 : x.Prevs.FindIndex(x => ALL.ContainsKey(x) && !ALL[x].IsVspom);
                if (firstGoodPrevIndex > -1)
                {
                    var prevIndex = x.Prevs[firstGoodPrevIndex];
                    if (prevs.TryGetValue(prevIndex, out var taraHere))
                    {
                        //prevs.Add(x.ID, taraHere); // Новая строчка. Из-за нее следует пройтись по всему алгоритму.
                        //OriginallyTaraWasThis.Add(x.ID, (taraHere, 0));
                        var sdvig = new PartiaModelling(opers, taraHere, 0, x.EdIzm, x.PossibleCenters?[0] ?? x.OriginalCenter, 0,
                            new());
                        addInfoLocal(opers, taraHere, 0, sdvig, true, engineData);
                        //OpersToSdvigs.Add(x.ID, sdvig);
                        return sdvig;
                    }
                }
            }

            // Если ничего не нашёл, то делаем пустоту
            var tarae = Uco.EmptyObject;
            //OriginallyTaraWasThis.Add(x.ID, (tarae, 0));
            var sdvig2 = new PartiaModelling(opers, tarae, 0.0, DimensionUnit.Unknown, x.PossibleCenters?[0] ?? x.OriginalCenter, 0,
                new());
            addInfoLocal(opers, tarae, 0, sdvig2, false, engineData);
            //OpersToSdvigs.Add(x.ID, sdvig2);
            return sdvig2;
        }

        static void addInfoLocal(List<Stage> opers, Uco tara, double max, PartiaModelling sdvig, bool toPrevs, PartiasGeneratorEngineData engineData)
        {
            var OriginallyTaraWasThis = engineData.OriginallyTaraWasThis;
            var OpersToSdvigs = engineData.OpersToSdvigs;
            var prevs = engineData.prevs;
            foreach (var x in opers)
            {
                OriginallyTaraWasThis.Add(x.Index, (tara, max));
                OpersToSdvigs.Add(x.Index, sdvig);
                if (toPrevs) prevs.Add(x.Index, tara);
            }
        }

        static void ZadelsCreation(List<List<PartiaModelling>> partia1, PartiasGeneratorEngineData engineData, Guid? partiaPFguid)
        {
            var Zadels = engineData.Session.MasterDataLinks.ZadelsMasterData;
            if (engineData.Session.SessionOptions.LaunchZadel && partiaPFguid.HasValue && Zadels.ZadelsData.TryGetValue(partiaPFguid.Value, out var zadelsGPsHere))
            {
                if (zadelsGPsHere.TryGetValue(engineData.Stages[0].OrderLink.OrderID, out var zadelsLevelsHere))
                {
                    // Нужно нафиг эту систему переделать, чтоб потом удобнее искать было
                    for (int i = 0; i < partia1.Count; i++) // Проходимся по всем уровням
                    {
                        var levelHere = partia1[i][0].Opers[0].Level;
                        if (zadelsLevelsHere.TryGetValue(levelHere, out var zadelsNomensHere))
                        {
                            for (int j = 0; j < partia1[i].Count; j++)
                            {
                                var opHere = partia1[i][j].Opers[0]; // потому что они тут всё равно всегда одни.
                                if (zadelsNomensHere.TryGetValue(opHere.Nomenklature, out var zadelsPovivsHere))
                                {
                                    // Номенклатуру нашли, теперь ищем повивы, и если находим - находим наконец-то заделы.
                                    if (zadelsPovivsHere.TryGetValue(opHere.Poviv, out var finalZadelsHere))
                                    {
                                        // теперь тут надо найти ЛИБО гуид этого этапа, либо гуид нулевой
                                        //var guidToFind = Zadels.USE_CONCRETE_ETAPS_GUIDS ? opHere.GUID.Value : Guid.Empty;

                                        // Нашли? значит переходим к заделам
                                        foreach (var zadelV in finalZadelsHere)
                                        {
                                            var zadel = zadelV.Value;
                                            // Чекаем, сколько у нас тут осталось
                                            if (zadel.WhereCanUsed.TryGetValue(opHere.GUID, out var countToSet))//zadel.CalcCount >= opHere.Count) // сюда можно сунуть толерантность ещё. 18.06: внезапно выяснилось, что есть право выдать задел, который совершенно не подходит по количеству - и мол может встать, под ответственность юзера.
                                                                                                                      // 19.06: если задела вообще не осталось, то операции назначить его не получится.
                                            {
                                                if (partia1[i][j].ZadelsGUIDs.Count == opHere.PlacesCount)
                                                {
                                                    // Он уже сюда не влезет. Другие заделы использовали
                                                    zadel.ZadelStatus = ZadelStatus.AnotherUsed;
                                                }
                                                else
                                                {
                                                    // Место ещё есть. Суем
                                                    //var finalCount = Math.Max(0, opHere.Count);

                                                    zadel.WhereIsUsed.Add((opHere.Index, countToSet)); // добавляем моделирования (теперь уже - ссылка на первую операцию, все равно они там объединяются в одно моделирование) и количество

                                                    //zadel.CalcCount -= finalCount; // отнимает из общего количества

                                                    zadel.ZadelStatus = ZadelStatus.SussesfullyUsed;

                                                    var code = partia1[i][j].ZadelCode;

                                                    var guid = zadelV.Key;

                                                    var codeToAdd = guid.GetHashCode();

                                                    if (code == 0)
                                                        code = codeToAdd;
                                                    else
                                                        code = HashCode.Combine(code, codeToAdd);

                                                    partia1[i][j].ZadelsGUIDs.Add(guid);
                                                    partia1[i][j].ZadelCode = code;

                                                    partia1[i][j].Tara = zadel.UsedTara;
                                                    partia1[i][j].Max = countToSet;

                                                    EraseZadelsTree(partia1[i][j], code, engineData);
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
                        }
                    }
                }
            }
        }


        // У всех предыдущих стираем всё
        static void EraseZadelsTree(PartiaModelling sdvig, int zadelCode, PartiasGeneratorEngineData engineData)
        {
            var OriginallyTaraWasThis = engineData.OriginallyTaraWasThis;
            var OpersToSdvigs = engineData.OpersToSdvigs;
            //var prevs = engineData.prevs;
            var Zadels = engineData.Session.MasterDataLinks.ZadelsMasterData;
            foreach (var prev in sdvig.Opers[0].Prevs) // смотрим предков
            {
                if (OpersToSdvigs.TryGetValue(prev, out var sdvigP))
                {
                    sdvigP.ZadelCode = zadelCode;
                    sdvigP.Tara = OriginallyTaraWasThis[prev].Tara; // Возвращаем тару на месту, если там была перезапись
                    sdvigP.Max = OriginallyTaraWasThis[prev].namot; // Возвращаем тару на месту, если там была перезапись
                    foreach (var zadelGuid in sdvigP.ZadelsGUIDs)
                    {
                        //Zadels.LinearZadelsDictionary[zadelGuid].CalcCount = Zadels.LinearZadelsDictionary[zadelGuid].Count; // Очищаем инфу в заделах
                        Zadels.LinearZadelsDictionary[zadelGuid].WhereIsUsed.Clear(); // всё стираем
                        Zadels.LinearZadelsDictionary[zadelGuid].ZadelStatus = ZadelStatus.HigherZadelUsed;
                    }
                    sdvigP.ZadelsGUIDs.Clear();
                    EraseZadelsTree(sdvigP, zadelCode, engineData);
                }
            }
        }
    }
}

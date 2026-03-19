using ApsRT.Core.Partias.PartiasClasses;
using ApsRT.Core.Partias.PartiasConnectorEngine;
using ApsRT.Core.Partias.Zadels;
using ApsRT.Core.Stages;
using ApsRT.Core.Taras.TaraSearchEngine;
using Azure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using static System.Runtime.InteropServices.JavaScript.JSType;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Partias.PartiasGeneratorEngine
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
                    var listhere2 = new PartiaModelling(new List<Stages.Stage>(), list1.Tara, list1.Max, list1.Format, list1.RC, list1.PlacesCountMultiplierToRewrite, list1.ZadelCode,
                        list1.ZadelsGUIDs != null ? new(list1.ZadelsGUIDs) : null);
                    listhere2.RCIsChecked = false;// list1.RCIsChecked;
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
                    yield return new PartiaModelling(new List<Stage>() { op }, Uco.EmptyObject, 0, DimensionUnit.Unknown, Uco.EmptyObject, null, 0, null);
                }

            }
            else
            {
                // Это моделирование. Создаём из него список
                yield return //checkThisList(group.ToList());
                    new PartiaModelling(group.ToList(), Uco.EmptyObject, 0, DimensionUnit.Unknown, Uco.EmptyObject, null, 0, null);
            }
        }

        public static void PrepareNaladLengths(PartiaClass partia, Dictionary<int, NaladLengths.GottenNaladLengthStruct> NaladLengthsData)
        {
            if (NaladLengthsData == null || partia == null) return;
            var dic = partia.SeeSdvigsAsDic();

            var session = dic.First().Value.Opers[0].SessionLink;

            var ALL = session.MasterDataLinks.StagesMasterData.AllStages;

            var firsts = dic.Where(x => ALL[x.Key].Nexts.Count == 0);

            foreach (var index in firsts)
            {
                RecursiveSet(index.Key, 1);
            }

            void RecursiveSet(int operIndex, double prevKoeff)
            {
                NaladLengthsData.TryAdd(operIndex, new NaladLengths.GottenNaladLengthStruct());
                // всё, что дальше, сделано по приказу алены и марины, 23.07.24.
                var stageNaladInfo = NaladLengthsData[operIndex];
                //ref var stageData = ref stageNaladInfo.data;
                var thatEtap = ALL[operIndex];
                //stageData.prevKoeff = prevKoeff;

                // след. налад. длина
                double nextNalad = (prevKoeff * thatEtap.Count) - thatEtap.Count; // "план. потребность + след. длина"
                stageNaladInfo.NaladLengthNexts.Value = (float)nextNalad; // уже в нужном нам измерении (было назначено в функции выше)

                stageNaladInfo.NaladLengthHere = session.MasterDataLinks.NaladLengthsMasterData.GetMinimumNaladLengthFromMasterData(thatEtap);

                stageNaladInfo.prevKoeff = (float)prevKoeff;

                NaladLengthsData[operIndex] = stageNaladInfo;

                var koeff = (nextNalad + thatEtap.Count + stageNaladInfo.NaladLengthHere.Value) / thatEtap.Count; // к длине даём текущую длину, и делим на плановую потребность, получая новый коээфициент

                foreach (var prev in thatEtap.Prevs)
                {
                    RecursiveSet(prev, (float)koeff);
                }
            }
        }

        public static List<List<PartiaModelling>> GenerateTree(PartiasGeneratorEngineData engineData, Color col, bool WithLog, Guid? partiaPFguid)
        {
            var ops = engineData.Stages;

            if (ops == null || ops.Count == 0)
            {
                return null;
            }

            var partia1 = new List<List<PartiaModelling>>();

            //var engineData = new PartiasGeneratorEngineData();
            //engineData.Session = ops[0].SessionLink;
            //engineData.Stages = ops;

            var DoneIDS = engineData.DoneIDS;
            //var OriginallyTaraWasThis = engineData.OriginallyTaraWasThis;
            var RestrictionsVia74110 = engineData.RestrictionsVia74110;
            //var OpersToSdvigs = engineData.OpersToSdvigs;
            //var prevs = engineData.prevs;
            engineData.Color = col;


            var Level = 0; // Раньше стояло 1, из-за волокна теперь 0
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
            Checks74_110(partia1, RestrictionsVia74110, true);

            // Постобработка


            var finalResult = new PartiasConnectorEngineResult(0, 0, 0, int.MaxValue, 0, 0, 0);
            PartiasConnectorEngine.PartiasConnectorEngine.PrepareResultForOneRCData(finalResult, PartiasConnectorEngine.PartiasConnectorEngine.SeeSdvigsAsList(partia1).Select(x => (IAbleForTaraCalcInterface)x).ToList(), new());

            for (int i = 0; i < partia1.Count; i++)
            {
                for (int j = 0; j < partia1[i].Count; j++)
                {
                    partia1[i][j] = checkThisList(partia1[i][j], engineData, WithLog, finalResult);
                }
            }



            return partia1;
        }

        public static void Checks74_110(List<List<PartiaModelling>> partia1, Dictionary<int, Uco> RestrictionsVia74110, bool searchForMax)
        {
            var ALL = partia1[0][0].Opers[0].SessionLink.MasterDataLinks.StagesMasterData.AllStages;
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
                var filteredSdvigs = prevsSdvigs.DistinctBy(x => x.Opers[0].OperationVid);
                if (filteredSdvigs.Count() > 1)
                {
                    // Больше 1. Изучаем все РЦ, куда можно поставить.
                    var possibleRCs = sdvig.Opers[0].PossibleCenters ?? new() { sdvig.Opers[0].OriginalCenter };
                    foreach (var rc in possibleRCs) // пройдемся по каждому РЦ и найдём вариант, при котором все prevsSdvigs смогут найти себе пару.
                    {
                        List<(bool Can, Uco Tara, PartiaModelling sdvig)> analysis = prevsSdvigs.Select(sdvigToCheck =>
                        {
                            bool can = Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(sdvigToCheck, out var res, out _, new SumIsPossibleOptions(false) { LogSomeErrors = false, NextRCsExcludeNoChangeTara = true, SearchForMaxTara = searchForMax, NextRCsMustBeThese = new() { rc } });
                            return (can, res.TaraName, sdvigToCheck);
                        }).ToList();

                        if (analysis.All(x => x.Can))
                        {
                            foreach (var an in analysis)
                            {
                                if (!an.sdvig.Opers[0].NoChangeTara && !an.sdvig.Opers[0].NoCheckNamotka)
                                    foreach (var op in an.sdvig.Opers)
                                        RestrictionsVia74110.TryAdd(op.Index, an.Tara);
                            }
                            break; // Ну и уходим отсюда тут же
                        }
                    }
                }
            }
        }


        public static PartiaModelling checkThisList(PartiaModelling sdvigWas, PartiasGeneratorEngineData engineData, bool WithLog, PartiasConnectorEngineResult finalResult)
        {
            var DoneIDS = engineData.DoneIDS;
            //var OriginallyTaraWasThis = engineData.OriginallyTaraWasThis;
            var RestrictionsVia74110 = engineData.RestrictionsVia74110;
            //var OpersToSdvigs = engineData.OpersToSdvigs;
            var prevs = engineData.prevs;
            var ALL = engineData.Session.MasterDataLinks.StagesMasterData.AllStages;
            var col = engineData.Color;

            var opers = sdvigWas.Opers;
            // TODO: если уж так нужен флаг будет, то добавить его можно будет где-то здесь.
            foreach (var op in opers) op.Color = col;

            if (sdvigWas.ZadelCode != 0) return sdvigWas;

            var x = opers[0];

            Uco? subdiv = engineData.BestSubdivision;

            // Это вспомогательная? В таком случае на тару пока забиваем
            if (x.IsVspom)
            {
                //var sdvg = ;
                //sdvg.RCIsChecked = true; // У вспомогательных операций нефиг РЦ проверять. Всё равно ничего полезного не найдем. Поэтому ставим первый попавшийся и забываем.
                // Так как очень часто создаются глубокие копии, то здесь назначать фиксацию РЦ бессмысленно, и было перетащено в CONNECT.
                var tarao = Uco.EmptyObject;
                //OriginallyTaraWasThis.Add(x.ID, (tarao, 0));


                var sdvig = new PartiaModelling(opers, tarao, 0.0, DimensionUnit.Unknown, x.PossibleCenters?[0] ?? x.OriginalCenter, sdvigWas.PlacesCountMultiplierToRewrite, sdvigWas.ZadelCode, sdvigWas.ZadelsGUIDs);
                addInfoLocal(opers, tarao, 0, sdvig, false, engineData);
                //OpersToSdvigs.Add(x.ID, sdvig);
                return sdvig;
            }

            // Основная функция - поиск тары

            Uco? trr = RestrictionsVia74110 == null ? null : (RestrictionsVia74110.TryGetValue(sdvigWas.Opers[0].Index, out var rtara) ? rtara : null);


            // + ограничения на одном рц запомним
            //if (opers.Any(x => x.GUID.ToString() == "a860f322-6702-11f0-be4a-3cecef1c38f6"))
            //{
            //    ;
            //}

            var listOnOneRc1 = new PartiaModellingSmall(opers);

            var resultOneRC = PartiasConnectorEngine.PartiasConnectorEngine.GetCenterFromOneRCGroup(listOnOneRc1, finalResult, out Uco? NeededRC, out var group1);


            if (Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(listOnOneRc1, out var RES, out _, new SumIsPossibleOptions(WithLog) { PrevsInfo = prevs, TaraMustBeThis = trr.HasValue ? new(1) { trr.Value } : null, LogSomeErrors = false,
                RCMustBeThis = NeededRC.HasValue ? new(1) { NeededRC.Value } : null,
                RecommendedSubdivision = subdiv
            }))
            {
                // Нашлась, отработала хорошо.
                // Пометки, что этот РЦ проверен, пока не ставится. В случае ненахода норм РЦ или перераспределений итд, потом в партиях с помощью "метода поиска проверенного РЦ" он в случае чего заменится.
                //prevs.Add(x.ID, tara);
                //OriginallyTaraWasThis.Add(x.ID, (tara, max));
                var sdvig = new PartiaModelling(opers, RES.TaraName, RES.Max, RES.used_format, RES.FinalRC, RES.CurrentPlacesCountK, sdvigWas.ZadelCode, sdvigWas.ZadelsGUIDs);
                if (trr.HasValue) sdvig.RCIsChecked = true; // блокируем возможность что-то поставить
                addInfoLocal(opers, RES.TaraName, RES.Max, sdvig, true, engineData);
                //OpersToSdvigs.Add(x.ID, sdvig);
                return sdvig;
            }
            else
            {
                if (Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(listOnOneRc1, out var RES2, out _, new SumIsPossibleOptions(WithLog) { PrevsInfo = prevs,
                    RCMustBeThis = NeededRC.HasValue ? new(1) { NeededRC.Value } : null,
                    RecommendedSubdivision = subdiv
                }))
                {
                    // Нашлась, отработала хорошо.
                    // Пометки, что этот РЦ проверен, пока не ставится. В случае ненахода норм РЦ или перераспределений итд, потом в партиях с помощью "метода поиска проверенного РЦ" он в случае чего заменится.
                    //prevs.Add(x.ID, tara);
                    //OriginallyTaraWasThis.Add(x.ID, (tara, max));
                    var sdvig = new PartiaModelling(opers, RES2.TaraName, RES2.Max, RES2.used_format, RES2.FinalRC, RES2.CurrentPlacesCountK, sdvigWas.ZadelCode, sdvigWas.ZadelsGUIDs);
                    addInfoLocal(opers, RES2.TaraName, RES2.Max, sdvig, true, engineData);
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
                        var sdvig = new PartiaModelling(opers, taraHere, 0, x.EdIzm, x.PossibleCenters?[0] ?? x.OriginalCenter, sdvigWas.PlacesCountMultiplierToRewrite, sdvigWas.ZadelCode, sdvigWas.ZadelsGUIDs);
                        addInfoLocal(opers, taraHere, 0, sdvig, true, engineData);
                        //OpersToSdvigs.Add(x.ID, sdvig);
                        return sdvig;
                    }
                }
            }

            // Если ничего не нашёл, то делаем пустоту
            var tarae = Uco.EmptyObject;
            //OriginallyTaraWasThis.Add(x.ID, (tarae, 0));
            var sdvig2 = new PartiaModelling(opers, tarae, 0.0, DimensionUnit.Unknown, x.PossibleCenters?[0] ?? x.OriginalCenter, sdvigWas.PlacesCountMultiplierToRewrite, sdvigWas.ZadelCode, sdvigWas.ZadelsGUIDs);
            addInfoLocal(opers, tarae, 0, sdvig2, false, engineData);
            //OpersToSdvigs.Add(x.ID, sdvig2);
            return sdvig2;
        }

        static void addInfoLocal(List<Stage> opers, Uco tara, double max, PartiaModelling sdvig, bool toPrevs, PartiasGeneratorEngineData engineData)
        {
            //var OriginallyTaraWasThis = engineData.OriginallyTaraWasThis;
            //var OpersToSdvigs = engineData.OpersToSdvigs;
            var prevs = engineData.prevs;
            foreach (var x in opers)
            {
                //OriginallyTaraWasThis.Add(x.Index, (tara, max));
                //OpersToSdvigs.Add(x.Index, sdvig);
                if (toPrevs) prevs.Add(x.Index, tara);
            }
        }

    }
}

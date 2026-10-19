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
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using StageGUID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, int>;
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
                    .GroupBy(x => engineData.Session.SessionOptions.LaunchZadel ? Guid.Empty : x.MesModelling.Code) // достаточно лишь того, который пришёл из меса, поэтому так

                    .SelectMany(group => selectSdvigs(group))
                    
                    .OrderBy(x => x.Opers[0].StageVersion)

                    .ToList();

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

            // Запоминаем РЦшки
            RememberRCs(partia1, engineData);

            // Проверочки 74-110
            Checks74_110(partia1, RestrictionsVia74110, engineData.PossibleRCsGlobal, true);

            // Постобработка


            var finalResult = new PartiasConnectorEngineResult(0, 0, 0, int.MaxValue, 0, 0, 0);
            PartiasConnectorEngine.PartiasConnectorEngine.PrepareResultForOneRCData(finalResult, PartiasConnectorEngine.PartiasConnectorEngine.SeeSdvigsAsList(partia1).Select(x => (IAbleForTaraCalcInterface)x).ToList(), new(), false);


            CheckListWithDeepChecks(partia1, engineData, WithLog, finalResult);


            return partia1;
        }

        public class DeepCheckData
        {
            public bool CanSearchTara;
            public HashSet<Uco> ExceptThisTara;
            public int TryCount = 0;

            public DeepCheckData()
            {
                CanSearchTara = true;
                ExceptThisTara = new();
            }
        }

        public static void RememberRCs(List<List<PartiaModelling>> partia1, PartiasGeneratorEngineData engineData)
        {
            var possibleRCs = engineData.PossibleRCsGlobal;
            var errored = engineData.EmptyErroredTarasInfo;

            // Сначала найдём все доступные РЦ для каждого этапа
            for (int i = 0; i < partia1.Count; i++)
            {
                for (int j = 0; j < partia1[i].Count; j++)
                {
                    var sdvig = partia1[i][j];



                    var RCs = sdvig.Opers[0].PossibleCenters ?? new() { sdvig.Opers[0].OriginalCenter };
                    //Uco? trr = RestrictionsVia74110 == null ? null : (RestrictionsVia74110.TryGetValue(sdvig.Opers[0].Index, out var rtara) ? rtara : null);
                    var result = RCs.Where(rc =>
                    {
                        return TaraSearchEngine.SumIsPossible(sdvig, out _, out _, new SumIsPossibleOptions(false)
                        {
                            LogSomeErrors = false,
                            RCMustBeThis = new HashSet<Uco>() { rc },
                            EmptyErroredTarasInfo = errored,
                            //TaraMustBeThis = trr.HasValue ? new(1) { trr.Value } : null,
                        });
                    });
                    foreach (var stage in sdvig.Opers) possibleRCs[stage.Index] = result.ToHashSet();
                }
            }
        }

        /// <summary>
        /// 05.08: метод, который учитывает ошибки вроде 16 бмк барабана. Если на уровне 1 поставлен он, на 3-м уровне, где корзина, вылазит ошибка. Этот большой метод чекает тары и выбирает ту, которая позволяет собрать цепочку.
        /// </summary>
        public static void CheckListWithDeepChecks(List<List<PartiaModelling>> partia1, PartiasGeneratorEngineData engineData, bool WithLog, PartiasConnectorEngineResult finalResult)
        {
            var errored = engineData.EmptyErroredTarasInfo;
            var RestrictionsVia74110 = engineData.RestrictionsVia74110;

            var ExceptTheseTarasGlobal = engineData.ExceptTheseTarasGlobal;

            var dicSdvigs = PartiasConnectorEngine.PartiasConnectorEngine.SeeSdvigsAsDic(partia1);

            Dictionary<int, DeepCheckData> dic1 = dicSdvigs.ToDictionary(x => x.Key, x => new DeepCheckData()
            {
                CanSearchTara = true,
                ExceptThisTara = ExceptTheseTarasGlobal != null && ExceptTheseTarasGlobal.TryGetValue(x.Key, out var except) ? except : new()
            });

            bool anyError = false;

            do
            {
                anyError = false;
                for (int i = 0; i < partia1.Count; i++)
                {
                    for (int j = 0; j < partia1[i].Count; j++)
                    {
                        var result = checkThisList(partia1[i][j], engineData, WithLog, finalResult, dic1, dicSdvigs, engineData.PossibleRCsGlobal);

                        partia1[i][j] = result.sdvig;

                        continue;


                        if (result.success)
                        {
                            partia1[i][j] = result.sdvig;
                        }
                        else
                        {
                            dic1.TryAdd(result.firstPrevIndex, new());
                            if (result.last /*|| dic1[result.firstPrevIndex].TryCount > 100*/)
                            {
                                // Больше НЕЛЬЗЯ менять тару для него
                                dic1[result.firstPrevIndex].CanSearchTara = false;
                                partia1[i][j] = result.sdvig;
                            }
                            else
                            {
                                if (engineData.prevs.TryGetValue(result.firstPrevIndex, out var tara))
                                {
                                    dic1[result.firstPrevIndex].ExceptThisTara.Add(tara);
                                    dic1[result.firstPrevIndex].TryCount++;
                                }
                                else
                                {
                                    dic1[result.firstPrevIndex].CanSearchTara = false;
                                    partia1[i][j] = result.sdvig;
                                }
                                anyError = true;
                                break;
                            }
                        }
                    }
                    if (anyError) break;
                }
            }
            while (anyError);

            // Сохраняем данные глобально
            foreach (var dicVal in dic1)
            {
                ExceptTheseTarasGlobal.TryAdd(dicVal.Key, new());
                ExceptTheseTarasGlobal[dicVal.Key] = dicVal.Value.ExceptThisTara;
            }
        }

        public static IEnumerable<T> IntersectMany<T>(this IEnumerable<IEnumerable<T>> source)
        {
            var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                return Enumerable.Empty<T>();

            // Начинаем с первой коллекции
            var hashSet = new HashSet<T>(enumerator.Current);

            // Пересекаем с каждой последующей
            while (enumerator.MoveNext())
            {
                hashSet.IntersectWith(enumerator.Current);
            }

            return hashSet;
        }

        public static void Checks74_110(List<List<PartiaModelling>> partia1, Dictionary<int, Uco> RestrictionsVia74110, Dictionary<int, HashSet<Uco>> PossibleRCs, /*PartiaConnectorEngineData engineData, */bool searchForMax)
        {
            var ALL = partia1[0][0].Opers[0].SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            var dicSdvigs = PartiasConnectorEngine.PartiasConnectorEngine.SeeSdvigsAsDic(partia1);
            var sdvigs = partia1.SelectMany(x => x).ToList();
            foreach (var sdvig in sdvigs)
            {
                if (sdvig.Opers[0].Index == 13915)
                {
                    ;
                }

                var prevsIds = sdvig.Opers.SelectMany(x => x.Prevs).Distinct().Select(x => ALL[x]).ToList();

                for (int i = 0; i < prevsIds.Count; i++)
                {
                    if (prevsIds[i].IsVspom || prevsIds[i].NoCheckNamotka/* || finalOps[i].UsesVspomTara || IDtoSdvig[finalOps[i].ID].ZadelCode != 0*/)
                    {
                        prevsIds.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (prevsIds[i].NoChangeTara)
                    {
                        prevsIds.AddRange(prevsIds[i].Prevs.Select(x => ALL[x]));
                        prevsIds.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                var prevsSdvigs = prevsIds.Select(x => dicSdvigs[x.Index]).ToList();
                var filteredSdvigs = prevsSdvigs.DistinctBy(x => x.Opers[0].OperationVid);
                if (filteredSdvigs.Count() > 1)
                {
                    /*
                     
                PrevsInfo = prevs,
                TaraMustBeThis = trr.HasValue ? new(1) { trr.Value } : null,
                LogSomeErrors = false,
                RCMustBeThis = NeededRC.HasValue ? new(1) { NeededRC.Value } : null,
                RecommendedSubdivision = subdiv,
                ExceptTheseTaras = deepCheckData.TryGetValue(listOnOneRc1.Opers[0].Index, out var exceptTheseTaras) ? exceptTheseTaras.ExceptThisTara : null,
                NextRCsMustBeThese = ,
                     */

                    var possibleRCs = IntersectMany(filteredSdvigs.Select(x => TaraSearchEngine.FindNeededNextRCsInDic(x.Opers, PossibleRCs)));
                        //PossibleRCs.TryGetValue(sdvig.Opers[0].Index, out var rcs) ? rcs.ToList() : (sdvig.Opers[0].PossibleCenters ?? new() { sdvig.Opers[0].OriginalCenter });

                    // Больше 1. Изучаем все РЦ, куда можно поставить.
                    foreach (var rc in possibleRCs) // пройдемся по каждому РЦ и найдём вариант, при котором все prevsSdvigs смогут найти себе пару.
                    {
                        List<(bool Can, Uco Tara, PartiaModelling sdvig)> analysis = prevsSdvigs.Select(sdvigToCheck =>
                        {
                            bool can = Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(sdvigToCheck, out var res, out _, new SumIsPossibleOptions(false)
                            {
                                //EmptyErroredTarasInfo = engineData. errored,
                                LogSomeErrors = false,
                                //NextRCsExcludeNoChangeTara = true,
                                SearchForMaxTara = searchForMax,
                                NextRCsMustBeThese = new() { rc }//TaraSearchEngine.FindNeededNextRCsInDic(sdvig.Opers, PossibleRCs, rc)
                            });
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

        public struct DeepCheckResult
        {
            public PartiaModelling sdvig;
            public bool success;
            public int firstPrevIndex;
            public bool last;

            public DeepCheckResult(PartiaModelling sdvig, bool success, int firstPrevIndex, bool last)
            {
                this.sdvig = sdvig;
                this.success = success;
                this.firstPrevIndex = firstPrevIndex;
                this.last = last;
            }
        }

        public static DeepCheckResult checkThisList(PartiaModelling sdvigWas, PartiasGeneratorEngineData engineData, bool WithLog, PartiasConnectorEngineResult finalResult,
            Dictionary<int, DeepCheckData> deepCheckData,
            Dictionary<int, PartiaModelling> dicSdvigs, Dictionary<int, HashSet<Uco>> possibleRCs)
        {
            var DoneIDS = engineData.DoneIDS;
            //var OriginallyTaraWasThis = engineData.OriginallyTaraWasThis;
            var RestrictionsVia74110 = engineData.RestrictionsVia74110;
            //var OpersToSdvigs = engineData.OpersToSdvigs;
            var prevs = engineData.prevs;
            var errored = engineData.EmptyErroredTarasInfo;
            var ALL = engineData.Session.MasterDataLinks.StagesMasterData.AllStages;
            var col = engineData.Color;


            if (sdvigWas.Opers[0].Index == 13913)
            {
                ;
            }

            var opers = sdvigWas.Opers;
            // TODO: если уж так нужен флаг будет, то добавить его можно будет где-то здесь.
            foreach (var op in opers) op.Color = col;

            if (sdvigWas.ZadelCode != 0) return new DeepCheckResult(sdvigWas, true, -1, false);

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
                addInfoLocal(opers, tarao, 0, sdvig, true, true, engineData);
                //OpersToSdvigs.Add(x.ID, sdvig);
                return new DeepCheckResult(sdvig, true, -1, false);
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

            if (Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(listOnOneRc1, out var RES, out _, new SumIsPossibleOptions(WithLog)
            {
                PrevsInfo = prevs,
                EmptyErroredTarasInfo = errored,
                TaraMustBeThis = trr.HasValue ? new(1) { trr.Value } : null,
                LogSomeErrors = false,
                RCMustBeThis = NeededRC.HasValue ? new(1) { NeededRC.Value } : null,
                RecommendedSubdivision = subdiv,
                //ExceptTheseTaras = deepCheckData.TryGetValue(listOnOneRc1.Opers[0].Index, out var exceptTheseTaras) ? exceptTheseTaras.ExceptThisTara : null,
                NextRCsMustBeThese = TaraSearchEngine.FindNeededNextRCsInDic(listOnOneRc1.Opers, possibleRCs),
            }))
            {
                // Нашлась, отработала хорошо.
                // Пометки, что этот РЦ проверен, пока не ставится. В случае ненахода норм РЦ или перераспределений итд, потом в партиях с помощью "метода поиска проверенного РЦ" он в случае чего заменится.
                //prevs.Add(x.ID, tara);
                //OriginallyTaraWasThis.Add(x.ID, (tara, max));
                var sdvig = new PartiaModelling(opers, RES.TaraName, RES.Max, RES.used_format, RES.FinalRC, RES.CurrentPlacesCountK, sdvigWas.ZadelCode, sdvigWas.ZadelsGUIDs);
                if (trr.HasValue) sdvig.RCIsChecked = true; // блокируем возможность что-то поставить
                addInfoLocal(opers, RES.TaraName, RES.Max, sdvig, true, true, engineData);
                //OpersToSdvigs.Add(x.ID, sdvig);
                return new DeepCheckResult(sdvig, true, -1, false);
            }
            else
            {
                if (Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(listOnOneRc1, out var tempRes, out _, new SumIsPossibleOptions(WithLog)
                {
                    //PrevsInfo = prevs, // - Убираем вот эту важную часть, чтобы убедиться, что это исключительно из-за предков, и надо искать другую тару выше
                    //NextRCsMustBeThese = TaraSearchEngine.FindNeededNextRCsInDic(listOnOneRc1.Opers, possibleRCs),
                    EmptyErroredTarasInfo = errored,
                    TaraMustBeThis = trr.HasValue ? new(1) { trr.Value } : null,
                    LogSomeErrors = false, // Ошибки пока всё ещё выключены
                    RCMustBeThis = NeededRC.HasValue ? new(1) { NeededRC.Value } : null,
                    RecommendedSubdivision = subdiv,
                    //ExceptTheseTaras = deepCheckData.TryGetValue(listOnOneRc1.Opers[0].Index, out var exceptTheseTaras2) ? exceptTheseTaras2.ExceptThisTara : null
                }))
                {
                    // Получаем всех предков
                    var possiblePrevsToCheck = listOnOneRc1.Opers.SelectMany(x => x.Prevs).Select(x => dicSdvigs[x]).Distinct();//.ToList();

                    // Теперь смотрим, каких из них можно проверить
                    possiblePrevsToCheck = possiblePrevsToCheck.Where(prev => deepCheckData.TryGetValue(prev.Opers[0].Index, out var data) && data.CanSearchTara && data.TryCount < 100);

                    if (possiblePrevsToCheck.Count() == 0)
                    {
                        // Если таких нету, то мне приходится просто вывести все ошибки с помощью SumIsPossible, и тупо пойти дальше.
                        Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(listOnOneRc1, out _, out _, new SumIsPossibleOptions(WithLog)
                        {
                            PrevsInfo = prevs,
                            EmptyErroredTarasInfo = errored,
                            TaraMustBeThis = trr.HasValue ? new(1) { trr.Value } : null,
                            //LogSomeErrors = false, // Включаем ошибки.
                            RCMustBeThis = NeededRC.HasValue ? new(1) { NeededRC.Value } : null,
                            RecommendedSubdivision = subdiv,
                            //ExceptTheseTaras = deepCheckData.TryGetValue(listOnOneRc1.Opers[0].Index, out var exceptTheseTaras3) ? exceptTheseTaras3.ExceptThisTara : null,
                            NextRCsMustBeThese = TaraSearchEngine.FindNeededNextRCsInDic(listOnOneRc1.Opers, possibleRCs)
                        });

                        var sdvig = new PartiaModelling(opers, Uco.EmptyObject, 0.0, DimensionUnit.Unknown, x.PossibleCenters?[0] ?? x.OriginalCenter, sdvigWas.PlacesCountMultiplierToRewrite, sdvigWas.ZadelCode, sdvigWas.ZadelsGUIDs);
                        addInfoLocal(opers, Uco.EmptyObject, 0, sdvig, true, true, engineData);
                        return new DeepCheckResult(sdvig, false, -1, true);
                    }
                    else
                    {
                        var sdvig = new PartiaModelling(opers, Uco.EmptyObject, 0.0, DimensionUnit.Unknown, x.PossibleCenters?[0] ?? x.OriginalCenter, sdvigWas.PlacesCountMultiplierToRewrite, sdvigWas.ZadelCode, sdvigWas.ZadelsGUIDs);
                        addInfoLocal(opers, Uco.EmptyObject, 0, sdvig, true, true, engineData);
                        return new DeepCheckResult(sdvig, false, possiblePrevsToCheck.First().Opers[0].Index, tempRes.ExceptTheseTarasNonErrorsCount == 0);
                    }
                }
                else
                {
                    // Если таких нету, то мне приходится просто вывести все ошибки с помощью SumIsPossible, и тупо пойти дальше.
                    Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(listOnOneRc1, out _, out _, new SumIsPossibleOptions(WithLog)
                    {
                        PrevsInfo = prevs,
                        EmptyErroredTarasInfo = errored,
                        TaraMustBeThis = trr.HasValue ? new(1) { trr.Value } : null,
                        //LogSomeErrors = false, // Включаем ошибки.
                        RCMustBeThis = NeededRC.HasValue ? new(1) { NeededRC.Value } : null,
                        RecommendedSubdivision = subdiv,
                        //ExceptTheseTaras = deepCheckData.TryGetValue(listOnOneRc1.Opers[0].Index, out var exceptTheseTaras3) ? exceptTheseTaras3.ExceptThisTara : null
                    });

                    //var sdvig = new PartiaModelling(opers, Uco.EmptyObject, 0.0, DimensionUnit.Unknown, x.PossibleCenters?[0] ?? x.OriginalCenter, sdvigWas.PlacesCountMultiplierToRewrite, sdvigWas.ZadelCode, sdvigWas.ZadelsGUIDs);
                    //addInfoLocal(opers, Uco.EmptyObject, 0, sdvig, true, true, engineData);
                    //return new DeepCheckResult(sdvig, false, -1, true);
                }
                //if (Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(listOnOneRc1, out var RES2, out _, new SumIsPossibleOptions(WithLog)
                //{
                //    PrevsInfo = prevs,
                //    EmptyErroredTarasInfo = errored,
                //    RCMustBeThis = NeededRC.HasValue ? new(1) { NeededRC.Value } : null,
                //    RecommendedSubdivision = subdiv
                //}))
                //{
                //    // Нашлась, отработала хорошо.
                //    // Пометки, что этот РЦ проверен, пока не ставится. В случае ненахода норм РЦ или перераспределений итд, потом в партиях с помощью "метода поиска проверенного РЦ" он в случае чего заменится.
                //    //prevs.Add(x.ID, tara);
                //    //OriginallyTaraWasThis.Add(x.ID, (tara, max));
                //    var sdvig = new PartiaModelling(opers, RES2.TaraName, RES2.Max, RES2.used_format, RES2.FinalRC, RES2.CurrentPlacesCountK, sdvigWas.ZadelCode, sdvigWas.ZadelsGUIDs);
                //    addInfoLocal(opers, RES2.TaraName, RES2.Max, sdvig, true, true, engineData);
                //    //OpersToSdvigs.Add(x.ID, sdvig);
                //    return sdvig;
                //}
                //else
                //{
                //    ;
                //}
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
                        addInfoLocal(opers, taraHere, 0, sdvig, true, true, engineData);
                        //OpersToSdvigs.Add(x.ID, sdvig);
                        return new DeepCheckResult(sdvig, true, -1, false);
                    }
                }
            }

            // Если ничего не нашёл, то делаем пустоту
            var tarae = Uco.EmptyObject;
            //OriginallyTaraWasThis.Add(x.ID, (tarae, 0));
            var sdvig2 = new PartiaModelling(opers, tarae, 0.0, DimensionUnit.Unknown, x.PossibleCenters?[0] ?? x.OriginalCenter, sdvigWas.PlacesCountMultiplierToRewrite, sdvigWas.ZadelCode, sdvigWas.ZadelsGUIDs);
            addInfoLocal(opers, tarae, 0, sdvig2, true, false, engineData);
            //OpersToSdvigs.Add(x.ID, sdvig2);
            return new DeepCheckResult(sdvig2, true, -1, false);
        }

        static void addInfoLocal(List<Stage> opers, Uco tara, double max, PartiaModelling sdvig, bool toPrevs, bool goodTara, PartiasGeneratorEngineData engineData)
        {
            //var OriginallyTaraWasThis = engineData.OriginallyTaraWasThis;
            //var OpersToSdvigs = engineData.OpersToSdvigs;
            var prevs = engineData.prevs;
            var errored = engineData.EmptyErroredTarasInfo;
            foreach (var x in opers)
            {
                //OriginallyTaraWasThis.Add(x.Index, (tara, max));
                //OpersToSdvigs.Add(x.Index, sdvig);
                if (toPrevs)
                {
                    if (goodTara)
                    {
                        if (!tara.IsEmpty) prevs[x.Index] = tara;
                    }
                    else
                    {
                        errored.Add(x.Index);
                    }
                }
            }
        }

    }
}

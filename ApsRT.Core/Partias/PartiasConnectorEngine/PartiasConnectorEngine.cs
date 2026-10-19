using ApsRT.Core.Partias.PartiasClasses;
using ApsRT.Core.Taras.TaraSearchEngine;
using Azure;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.OrTools.Algorithms;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using UniversalPack.Extensions;
using ZLinq;
using static ApsRT.Core.Partias.PartiasGeneratorEngine.PartiasGeneratorEngine;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Partias.PartiasConnectorEngine
{
    public class PartiasConnectorEngine
    {

        public static Dictionary<int, PartiaModelling> SeeSdvigsAsDic(List<List<PartiaModelling>> opers)
        {
            var list = opers.AsValueEnumerable().SelectMany((x, index1) =>
            {
                return x.SelectMany((y, index2) =>
                {
                    return y.Opers.Select(z => (z.Index, y, x, index2, index1));
                });
            });
            return list.ToDictionary(x => x.Index, x => x.y);
        }

        public static List<PartiaModelling> SeeSdvigsAsList(List<List<PartiaModelling>> opers)
        {
            var list = opers.AsValueEnumerable().SelectMany(x => x).ToList();
            return list;
        }


        public static void ClearLongestOpers(List<Stages.Stage> finalOps, Dictionary<int, PartiaModelling> IDtoSdvig)
        {
            // Очищаем от лишнего
            for (int i = 0; i < finalOps.Count; i++)
            {
                if (finalOps[i].NoChangeTara)
                {
                    finalOps.AddRange(finalOps[i].Prevs.Select(x => finalOps[i].SessionLink.MasterDataLinks.StagesMasterData.AllStages[x]));
                    finalOps.RemoveAt(i);
                    i--;
                    continue;
                }
                if (finalOps[i].IsVspom || finalOps[i].UsesVspomTara || IDtoSdvig[finalOps[i].Index].ZadelCode != 0)
                {
                    finalOps.RemoveAt(i);
                    i--;
                }
            }
        }


        public static void updateDataBcsSuccess(PartiaModelling here1, Uco gotTara, Uco gotRc, double gotMax, DimensionUnit use_format, int? placesCount, PartiasConnectorEngineResult resultToFill)
        {
            here1.Tara = gotTara;
            here1.RC = gotRc;
            here1.RCIsChecked = true;
            here1.Max = gotMax;
            here1.Format = use_format;
            here1.PlacesCountMultiplierToRewrite = placesCount.HasValue && placesCount.Value > 1 ? placesCount.Value : null;
            SaveTaraInfo(here1, gotTara, resultToFill.IDToTara);

            //if (gotTara.IsEmpty) return; // Дальше не идём, чтобы не засорять словарь мешающей инфой.
            //foreach (var Index in here1.Opers)
            //{
            //    var id2 = Index.Index;

            //    if (resultToFill.IDToTara.ContainsKey(id2))
            //    {
            //        resultToFill.IDToTara[id2] = gotTara;
            //    }
            //    else
            //    {
            //        resultToFill.IDToTara.Add(id2, gotTara);
            //    }
            //}
        }

        static void SaveTaraInfo(PartiaModelling here1, Uco gotTara, Dictionary<int, Uco> IDToTara)
        {
            if (gotTara.IsEmpty) return; // Дальше не идём, чтобы не засорять словарь мешающей инфой.
                                         //if (here1.Opers[0].ID == 665)
                                         //{
                                         //    ;
                                         //}
            if (here1 == null)
            {
                Console.WriteLine("here1 = null");
                return;
            }
            if (here1.Opers == null)
            {
                Console.WriteLine("here1.Opers = null");
                return;
            }
            try
            {
                foreach (var id in here1.Opers)
                {
                    var id2 = id.Index;

                    if (IDToTara.ContainsKey(id2))
                    {
                        IDToTara[id2] = gotTara;
                    }
                    else
                    {
                        IDToTara.Add(id2, gotTara);
                    }
                }
            }
            catch
            {
                try
                {
                    Console.WriteLine("ERROR: " + here1.Opers[0].OrderLink.OrderID);
                }
                catch
                {

                }
            }
        }


        static void MoveThem(List<PartiaModelling> here1, List<PartiaModelling> here2)
        {
            for (var j = 0; j < here2.Count; j++)
            {
                if (here2[j].Opers.Count > 0 && !here1.Contains(here2[j]))
                {
                    var kortez = here2[j];
                    here1.Add(kortez);
                    here2.RemoveAt(j);
                    j--;
                }
            }
        }

        static bool AbleToMergeLevel(List<PartiaModelling> here1, List<PartiaModelling> here2, int CurrLevel, bool isFirstLevel, PartiaConnectorEngineData data, PartiasConnectorEngineResult resultToFill)
        {
            (PartiaClass firstPartia, PartiaClass secondPartia, bool withLog, bool withHerself) = data;
            if (withHerself)
            {
                // Сама с собой
                for (var i = 0; i < here1.Count; i++)
                {
                    var res = 0;
                    do
                    {
                        if (here1.Count == 0 || i >= here1.Count)
                        {
                            break;
                        }

                        SaveTaraInfo(here1[i], here1[i].Tara, resultToFill.IDToTara);
                        res = AbleToMerge(here1[i], here2, CurrLevel, false, data, resultToFill); // false - внутри себя никаких проверок внутри себя уже делать НЕ НАДО, поэтому имитирую, что это НЕ первый уровень
                        for (var j = 0; j < here2.Count; j++)
                        {
                            if (here2[j].Opers.Count == 0)
                            {
                                here2.RemoveAt(j);
                                if (j < i)
                                    //if (i > 0)
                                    i--;
                                //i = 0;
                                j--;
                            }
                        }
                    }
                    while (res > 0);

                }
                //smthWasGood = true;
                return true;//Jcounter > 0 && (CurrLevel > 0 || must == 0);
            }
            else
            {
                var Vcounter = 0;
                var Jcounter = 0;
                int must = Math.Min(here1.Where(x => !x.Opers[0].IsVspom
                && x.ZadelCode == 0 // это позволяет проигнорить часть первых уровней
                ).Count(), here2.Where(x => !x.Opers[0].IsVspom
                && x.ZadelCode == 0
                ).Count()); // TODO: 29.05 - решено пока игнорить вспомогательные. В будущем будет поправлено.
                for (var i = 0; i < here1.Count; i++)
                {
                    var h = here1[i];
                    SaveTaraInfo(h, h.Tara, resultToFill.IDToTara);
                    var res = AbleToMerge(h, here2, CurrLevel, isFirstLevel, data, resultToFill);
                    //Debugger.NextDebugTime("connect " + CurrLevel);
                    /*
                    0 ничего
                    1 вспом
                    2 норм
                     */
                    if (res > 0)
                    {
                        //if (CurrLevel == 0)
                        //    must--;

                        //else
                        //    smthWasGood = true;
                        if (res == 1)
                        {
                            Vcounter++;
                        }
                        else
                        {
                            Jcounter++; // TODO: а что если НЕ ВСЕ ВСПОМОГАТЕЛЬНЫЕ объединились?
                        }
                    }
                    here1[i] = h;

                    for (var j = 0; j < here2.Count; j++)
                    {
                        if (here2[j].Opers.Count == 0)
                        {
                            here2.RemoveAt(j);
                            //if (i > 0)
                            //    i--;
                            //i = 0;
                            j--;
                        }
                    }
                }
                //if (CurrLevel == 0 && must == 0) smthWasGood = true;
                return
                    (Jcounter > 0 || must == 0)
                    &&
                    (CurrLevel > 0 || Jcounter >= must/*must <= 0*/); // must - может быть меньше 0, потому что мы считаем в том числе все объединившиеся
            }
        }

        public static bool GetCenterFromOneRCGroup(
            PartiaModellingSmall here1, // один сдвиг
            PartiasConnectorEngineResult resultToFill, out Uco? NeededRC, out int group1)
        {
            

            NeededRC = null;
            group1 = -1;
            //return true;

            group1 = ModOnOneRC(here1, out var oneRcIndex1) /*here1.Opers[0].OnOneRC*/ && resultToFill.ForOneRCGroups.TryGetValue(here1.Opers[oneRcIndex1].Index, out var group11) ? group11 : -1;

            if (group1 != -1 && resultToFill.ForOneRCGroupAlreadyUsed.TryGetValue(group1, out var myRC))
            {
                // РЦ Найден.
                NeededRC = myRC;
            }
            else
            {
                // Установки ещё не было. Смотрим полностью
                var res = ModOnOneRC(here1, out oneRcIndex1);
                var list1 = res /*here1.Opers[0].OnOneRC*/ ? (group1 != -1 && resultToFill.PossibleCentersForOneRCGroups.TryGetValue(group1, out var list1F) ? list1F : new()) : (here1.Opers[0].PossibleCenters ?? new List<Uco>() { here1.Opers[0].OriginalCenter });

                if (res)
                {
                    if (list1.Count == 0)
                    {
                        ;
                    }
                    else
                        NeededRC = list1[0];
                }
            }

            return true;
        }

        public static bool ModOnOneRC(IAbleForTaraCalcInterface mod, out int index)
        {
            for (int i = 0; i < mod.AllOpers.Count; i++)
            {
                if (mod.AllOpers[i].OnOneRC)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        static bool GetCenterFromOneRCGroup(
            PartiaModelling here1, // один сдвиг
            PartiaModelling here2, // все остальные тут
            PartiasConnectorEngineResult resultToFill, out Uco? NeededRC, out int group1, out int group2)
        {
            NeededRC = null;

            group1 = -1;
            group2 = -2;
            //return true;

            group1 = ModOnOneRC(here1, out var oneRcIndex1)/*here1.Opers[0].OnOneRC*/ && resultToFill.ForOneRCGroups.TryGetValue(here1.Opers[oneRcIndex1].Index, out var group11) ? group11 : -1;
            group2 = ModOnOneRC(here2, out var oneRcIndex2)/*here2.Opers[0].OnOneRC*/ && resultToFill.ForOneRCGroups.TryGetValue(here2.Opers[oneRcIndex2].Index, out var group22) ? group22 : -1;

            if (group1 != -1 && resultToFill.ForOneRCGroupAlreadyUsed.TryGetValue(group1, out var myRC))
            {
                // РЦ Найден.
                NeededRC = myRC;
                if (group2 != -1 && resultToFill.ForOneRCGroupAlreadyUsed.TryGetValue(group2, out myRC))
                {
                    if (NeededRC != myRC)
                    {
                        // ОШИБКА !!! Притом невозможная...
                        return false;
                    }
                }
            }
            else
            {
                // Установки ещё не было. Смотрим полностью
                var res1 = ModOnOneRC(here1, out oneRcIndex1);
                var list1 = res1/*here1.Opers[0].OnOneRC*/ ? (group1 != -1 && resultToFill.PossibleCentersForOneRCGroups.TryGetValue(group1, out var list1F) ? list1F : new()) : (here1.Opers[0].PossibleCenters ?? new List<Uco>() { here1.Opers[0].OriginalCenter });

                var res2 = ModOnOneRC(here2, out oneRcIndex2);
                var list2 = res2/* here2.Opers[0].OnOneRC*/ ? (group2 != -1 && resultToFill.PossibleCentersForOneRCGroups.TryGetValue(group2, out var list2F) ? list2F : new()) : (here2.Opers[0].PossibleCenters ?? new List<Uco>() { here2.Opers[0].OriginalCenter });

                if (res1)
                {
                    list1 = list1.Intersect(list2).ToList();
                    if (list1.Count == 0)
                    {
                        // ОШИБКА !!!
                        return false;
                    }
                    else
                    {
                        NeededRC = list1[0];
                    }
                }
                else
                {
                    if (res2)
                    {
                        list2 = list2.Intersect(list1).ToList();
                        if (list2.Count == 0)
                        {
                            // ОШИБКА !!!
                            return false;
                        }
                        else
                        {
                            NeededRC = list2[0];
                        }
                    }
                }
            }

            return true;
        }

        static byte AbleToMerge(PartiaModelling here1, // один сдвиг
            List<PartiaModelling> here2, // все остальные тут
            int currentLevel, bool isFirstLevel, PartiaConnectorEngineData data, PartiasConnectorEngineResult resultToFill)
        {
            (PartiaClass firstPartia, PartiaClass secondPartia, bool withLog, bool withHerself) = data;
            var SessionLink = here1.Opers[0].SessionLink;
            var options = SessionLink.SessionOptions;
            var stagesMasterData = SessionLink.MasterDataLinks.StagesMasterData;
            var ALL = stagesMasterData.AllStages;
            var LogErrorsBySettings = SessionLink.SettingsController.CurrentSettings.LogErrors;
            var Zadels = SessionLink.MasterDataLinks.ZadelsMasterData;

            //x.Prevs.Count > 0 && (!IDmergedPrevs.TryGetValue(x.Index, out var ct) || x.PrevsNormCount > ct)
            if (here1.Opers.Any(x =>
            ((resultToFill.IDmergedPrevs.TryGetValue(x.Index, out var ct) && x.PrevsNormCount > ct)) // Эта штука проверяет, все ли предки схлопнуты. Если не везде и не все, то это моделирование непригодно
                                                                                     //|| // Ещё одно условие
                                                                                     //(withHerself && x.Nexts.Count > 0 && x.Nexts.Any(y => ALL[y].Prevs.Count > 1)) // Если у нее есть дети, и у ребенка предков больше, чем 1, значит это нечто типа скрутки, и на этой стадии нельзя схлопываться.
                )
                )
            {
                return 0; // Если не у всех схлопнуты предки
            }

            var isVspom = here1.Opers[0].IsVspom;

            //var ThatWasVspom = false;
            //var enough = false;
            //int counter = 0;

            // Функция
            bool check(PartiaModelling x,
                int id1,
                HashSet<int> nexts2
                )
            {
                // Если хотя бы первая равна, то подразумевается, что они там все норм
                if (SessionLink.MasterDataLinks.MergeConditionsMasterData.StagesEqualityCheck(here1.Opers[0], x.Opers[0], out _, out var reason, out var hasSets))
                {

                    // В случае, если это было ручное формирование, то мы можем запомнить, что он справился
                    //if (LogErrorsBySettings && options.OnlyPart && isFirstLevel)
                    //{
                    //    AddNewMergedPossibility(here1.Opers[0].Index, x.Opers[0].Index);
                    //}

                    // все дети должны быть на месте
                    if (x.Opers.All(y => y.PrevsNormCount == 0 || (resultToFill.IDmergedPrevs.TryGetValue(y.Index, out var ct) && y.PrevsNormCount <= ct)))
                    {
                        // Проверяем, чтобы это не была она сама
                        if (here1 != x)
                        {
                            // Если я могу менять места (у обоих этот признак на этот момент гарантированно одинаковый),
                            // то надо, чтобы было не больше 2 этапов,
                            // и чтоб сами этапы ещё ни в чем таком не участвовали (коэффициент = 1 или не задан).
                            if (here1.Opers[0].CanChangePlacesCount) 
                            {
                                //if (here1.Opers.Count + x.Opers.Count > 2) return false;
                                //if (here1.PlacesCountMultiplierToRewrite.HasValue && here1.PlacesCountMultiplierToRewrite.Value > 1) return false;
                                //if (x.PlacesCountMultiplierToRewrite.HasValue && x.PlacesCountMultiplierToRewrite.Value > 1) return false;
                            }

                            if (withHerself)
                            {
                                // он не должен быть закрытым уже + последователи должны быть разными (изначально делалось, чтобы они, предшествуя скрутке, не полезли друг на друга, при этом с разными скрутками можно)
                                var nexts1 = x.Opers.SelectMany(x => x.Nexts).ToHashSet();
                                return !nexts1.Any() || !nexts2.Any() || nexts1.All(x => !nexts2.Contains(x));
                                var id2 = x.Opers[0].Nexts.Count > 0 ? x.Opers[0].Nexts[0] : -1;
                                return //!closedSdvigs.Contains(x)
                                       //&&
                                   (id1 != id2 || id1 == -1 || id2 == -1);
                            }
                            return true;
                        }
                        return false;
                        //return here1 != x && (!withHerself || !closedSdvigs.Contains(x));
                    }
                    else
                    {
                        if (withLog)
                        {
                            SessionLink.AssignedLogger.PartiasLogQueue.Enqueue($"Уровень: {currentLevel + 1} - у операций {(string.Join(", ", x.Opers.Select(x => x.Index)))} не схлопнуты предки.\r\n");
                        }

                        return false;
                    }

                }
                else
                {
                    if (withLog)
                    {
                        SessionLink.AssignedLogger.PartiasLogQueue.Enqueue($"Уровень: {currentLevel + 1} - Нельзя схлопнуть операции: {here1.Opers[0]}, {x.Opers[0]} ({reason})\r\n");
                    }

                    // В случае, если это было ручное формирование, то мы можем запомнить, почему он не справился
                    //if (LogErrorsBySettings && options.OnlyPart && isFirstLevel)
                    //{
                    //    AddNewUnmergedReason(here1.Opers[0].Index, x.Opers[0].Index, hasSets, reason);
                    //}

                    return false;
                }
            }

            SortedList<int, List<int>> possibleISList = new(here2.Count);
            var id1 = here1.Opers[0].Nexts.Count > 0 ? here1.Opers[0].Nexts[0] : -1;
            var ids1 = here1.Opers.SelectMany(x => x.Nexts).ToHashSet();//here1.Opers[0].Nexts;//.Count > 0 ? here1.Opers[0].Nexts[0] : -1;
            for (var i = 0; i < here2.Count; i++)
            {
                if (here2[i].ZadelCode == here1.ZadelCode // гуиды заделов совпадают? (если без заделов то они все всегда нул)
                    && here2[i].Opers[0].IsVspom == here1.Opers[0].IsVspom // Вспомогательные только друг с другом
                    && here2[i].Opers[0].UsesVspomTara == here1.Opers[0].UsesVspomTara // Вспомогательные тары только друг с другом
                    && check(here2[i], id1, ids1))
                {
                    int liv = stagesMasterData.LevenshteinDistance(here1.Opers[0].Index, here2[i].Opers[0].Index, here1.Opers[0].Index, here2[i].Opers[0].Index, withLog);
                    if (!possibleISList.ContainsKey(liv))
                    {
                        possibleISList.Add(liv, new(here2.Count));
                    }

                    possibleISList[liv].Add(i);
                }
            }

            //var possibleIS = here2.Select((x, index) => (x, index))
            //    .Where(x => x.x.Opers[0].IsVspom == here1.Opers[0].IsVspom && check(x.x)) // Почему-то тупит, пришлось в отдельную функцию вытащить
            //    .OrderBy(x =>
            //    // С помощью расстояния Левенштейна находим подходящее
            //    Merger.LevenshteinDistance(here1.Opers[0].Index, x.x.Opers[0].Index, here1.Opers[0].Index, x.x.Opers[0].Index)
            //    ).Select(x => x.index);
            foreach (var possibleIS in possibleISList)
            {
                foreach (var i in possibleIS.Value)
                //foreach (var i in possibleIS)
                {

                    var grp1 = here1.Opers.Select(x => resultToFill.IDmergedGroups.TryGetValue(x.Index, out var Index) ? Index : -1).Where(x => x != -1).ToHashSet();
                    var grp2 = here2[i].Opers.Select(x => resultToFill.IDmergedGroups.TryGetValue(x.Index, out var Index) ? Index : -1).Where(x => x != -1).ToHashSet();

                    if (
                        (here1.Opers.Any(x => x.Prevs.Count > 0) && here2[i].Opers.Any(x => x.Prevs.Count > 0) && grp1.Count == 0 && grp2.Count == 0) ||

                        (grp1.Count == 0 ^ grp2.Count == 0) || // Раньше было вот так: (grp1.Count == 0 ^ grp2.Count == 0) ||
                        (
                        //grp1.Any()
                        //&&
                        //grp2.Any()
                        //&&
                        grp1.Except(grp2).Count() > 0
                        ||
                        grp2.Except(grp1).Count() > 0
                        )
                        ||
                        (here1.Opers[0].NoChangeTara && (grp1.Count == 0 ^ grp2.Count == 0))
                        //IDmergedGroups.TryGetValue(here1.Opers[0].Index, out var gr1)
                        //&&
                        //gr1 != -1
                        //&&
                        //IDmergedGroups.TryGetValue(here2[i].Opers[0].Index, out var gr2)
                        //&&
                        //gr2 != -1
                        //&&
                        ////gr1.All(x => !gr2.Contains(x))
                        //gr1 != gr2
                        )
                    {
                        if (withLog)
                        {
                            SessionLink.AssignedLogger.PartiasLogQueue.Enqueue($"Уровень: {currentLevel + 1} - Нельзя схлопнуть операции: {here1.Opers[0]}, {here2[i].Opers[0]} (схлоп. группы разные)\r\n");//{gr1} и {gr2}
                        }

                        continue;
                    }
                    SaveTaraInfo(here2[i], here2[i].Tara, resultToFill.IDToTara); // Сохраняем инфу по таре

                    var here01 = new PartiaModellingSmall(new List<Stages.Stage>(here1.Opers), here1.ZadelCode, here1.ZadelsGUIDs); // копируем слегка
                    here01.Opers.AddRange(here2[i].Opers);
                    var sum = false;
                    Uco gotTara = new(Guid.Empty, "");
                    Uco gotRc = here1.Opers[0].PossibleCenters?[0] ?? here1.Opers[0].OriginalCenter;//new(Guid.Empty, "");
                    var gotMax = here1.Max;
                    int? gotPlacesMult = null;
                    var use_format = DimensionUnit.Unknown;

                    bool isZadel = here1.ZadelCode != 0;

                    //bool isZadel2 = here2[i].ZadelCode != 0;

                    // Если это задел, то юзается тара из первого же задела тут.
                    // TODO: а что если у каждого задела свои тары?
                    if (isZadel)
                    {
                        gotMax = here01.Opers.Sum(x => x.Count);
                        if (here1.ZadelsGUIDs != null && here1.ZadelsGUIDs.Count > 0)
                        {
                            gotTara = Zadels.LinearZadelsDictionary.TryGetValue(here1.ZadelsGUIDs[0], out var zet) ? zet.UsedTara : Uco.EmptyObject;
                        }
                    }

                    //if (isZadel2)
                    //{
                    //    ;
                    //}

                    // Условие "на одном РЦ" - поиск нужного РЦ или хотя бы ограничение

                    if (!GetCenterFromOneRCGroup(here1, here2[i], resultToFill, out Uco? NeededRC, out var group1, out var group2)) continue;

                    if (!isVspom && !isZadel)
                    {
                        float naladLength = 0;

                        if (data.NaladLengthsData != null)
                        {
                            var currentF = here01.Opers.Max(x => data.NaladLengthsData.TryGetValue(x.Index, out var l1) ? l1.NaladLengthHere.Value : 0);
                            var sumMod = here01.Opers.Sum(x => x.Count);
                            var nextF = (here01.Opers.Min(x => data.NaladLengthsData.TryGetValue(x.Index, out var l1) ? l1.prevKoeff : 1) * sumMod) - sumMod;
                            //var nextF = here01.Opers.Sum(x => data.NaladLengthsData.TryGetValue(x.Index, out var l1) ? l1.NaladLengthNexts.Value : new NaladLengths.GottenNaladLengthStruct().NaladLengthNexts.Value);
                            //var op1 = data.NaladLengthsData.TryGetValue(here1.Opers[0].Index, out var l1) ? l1 : new NaladLengths.GottenNaladLengthStruct();
                            //var op2 = data.NaladLengthsData.TryGetValue(here2[i].Opers[0].Index, out var l2) ? l2 : new NaladLengths.GottenNaladLengthStruct();
                            //var current1 = op1.NaladLengthHere.Value;
                            //var current2 = op2.NaladLengthHere.Value;
                            //var currentF = Math.Max(current1, current2);
                            //var next1 = op1.NaladLengthNexts.Value;
                            //var next2 = op2.NaladLengthNexts.Value;
                            //var nextF = next1 + next2;
                            naladLength = currentF + (float)nextF;
                        }
                        TaraSearchEngineResult got = null;
                        sum = Taras.TaraSearchEngine.TaraSearchEngine.GetAllVariantsForThese(here01, out var searchData, new SumIsPossibleOptions(withLog)
                        {
                            PrevsInfo = resultToFill.IDToTara,
                            EmptyErroredTarasInfo = resultToFill.EmptyErroredTarasInfo,
                            LogSomeErrors = false,
                            RCMustBeThis = NeededRC.HasValue ? new(1) { NeededRC.Value } : null,
                            NextRCsMustBeThese = TaraSearchEngine.FindNeededNextRCsInDic(here01.Opers, data.PossibleRCsGlobal)
                        })
                            &&
                            Taras.TaraSearchEngine.TaraSearchEngine.FilterAllVariants(here01, searchData, out got, new SumIsPossibleOptions(withLog)
                            {
                                PrevsInfo = resultToFill.IDToTara,
                                EmptyErroredTarasInfo = resultToFill.EmptyErroredTarasInfo,

                                TaraMustBeThis =
                                here1.ZadelsGUIDs != null && here1.ZadelsGUIDs.Count > 0 && Zadels.LinearZadelsDictionary.TryGetValue(here1.ZadelsGUIDs[0], out var zet) ?
                                new(1) { zet.UsedTara }
                                :
                                null,

                                RecommendedTara = data.RestrictionsVia74110 == null ? null : (data.RestrictionsVia74110.TryGetValue(here1.Opers[0].Index, out var tara1) ? tara1 : (data.RestrictionsVia74110.TryGetValue(here2[i].Opers[0].Index, out var tara2) ? tara2 : null)),

                                LogSomeErrors = false,

                                RecommendedSubdivision = resultToFill.RecommendedSubdivision,

                                RCMustBeThis = NeededRC.HasValue ? new(1) { NeededRC.Value } : null,

                                NaladLength = naladLength,

                                ExceptTheseTaras = data.ExceptTheseTarasGlobal == null ? null : data.ExceptTheseTarasGlobal.TryGetValue(here1.Opers[0].Index, out var exceptTheseTaras) ? exceptTheseTaras : null
                            });
                        if (sum && got != null)
                        {
                            gotTara = got.TaraName;
                            gotMax = got.Max;
                            gotRc = got.FinalRC;
                            use_format = got.used_format;
                            gotPlacesMult = got.CurrentPlacesCountK;
                        }
                        //sum = Merger.SumIsPossible(here01, out gotTara, out gotRc, out gotMax, out use_format, out _, withUI, CheckChildren, IDToTara);
                    }

                    if (sum || isVspom || isZadel)
                    {
                        //if (here1.Opers[0].Time == longestHere && here2[i].Opers[0].Time == longestWhat) badOffset = 0;
                        if (!isVspom && !isZadel) resultToFill.mergedNorm++; // на вспомогательные и заделы ща насрать. Потому что они - не показатель.
                        else resultToFill.mergedVspom++;
                        //ThatWasVspom = isVspom;
                        //if (!isZadel) enough = true; // решено не считать задельные МНОЮ, 21.06. Надо у макарова уточнить


                        // Запоминаем условие для одного РЦ
                        if (here1.Opers[0].OnOneRC) resultToFill.ForOneRCGroupAlreadyUsed.TryAdd(group1, gotRc);
                        if (here2[i].Opers[0].OnOneRC) resultToFill.ForOneRCGroupAlreadyUsed.TryAdd(group2, gotRc);

                        here1.Opers.AddRange(here2[i].Opers);
                        here2[i].Opers.Clear();

                        //if (here1.ZadelCode == 0 && here2[i].ZadelCode != 0)
                        //{
                        //    ;
                        //    here1.ZadelCode = here2[i].ZadelCode;
                        //    here1.ZadelsGUIDs = here2[i].ZadelsGUIDs;
                        //    here1.Max = here2[i].Max;
                        //}

                        if (sum || isZadel)
                        {
                            updateDataBcsSuccess(here1, gotTara, gotRc, gotMax, use_format, gotPlacesMult, resultToFill);
                        }

                        // Следующий код срабатывает, даже если мы ща объединили вспомогательные. Пока что вспомогательные описаны слабо.
                        // авотхуй. 10.06: убрал, и стало схлопываться лучше.
                        //if (sum || isZadel)
                        //{
                        var r = Random.Shared.Next();
                        foreach (var Index in here1.Opers)
                        {
                            foreach (var id2 in ALL[Index.Index].Nexts)
                            {
                                if (sum || isZadel)
                                {
                                    if (resultToFill.IDmergedPrevs.ContainsKey(id2))
                                    {
                                        resultToFill.IDmergedPrevs[id2]++;
                                    }
                                    else
                                    {
                                        resultToFill.IDmergedPrevs.Add(id2, 1);
                                    }
                                }

                                if (!resultToFill.IDmergedGroups.ContainsKey(id2))
                                {
                                    resultToFill.IDmergedGroups.Add(id2, r);
                                }
                                else
                                {
                                    if (withHerself)
                                    {
                                        //;
                                        resultToFill.IDmergedGroups[id2] = r;
                                    }
                                }
                                //SessionLink.AssignedLogger.PartiasLogQueue.Enqueue($"op {id2} now has {resultToFill.IDmergedGroups[id2]}");
                                //Console.WriteLine($"op {id2} now has {resultToFill.IDmergedGroups[id2]}");

                            }
                        }
                        //}

                        return (byte)(isZadel ? 0 : (isVspom ? 1 : 2));//(byte)(enough ? (ThatWasVspom ? 1 : 2) : 0);
                                                                       //break;
                    }
                    else
                    {
                        //foreach (var op in here1.Opers)
                        //{
                        //    resultToFill.EmptyErroredTarasInfo.Add(op.Index);
                        //}
                    //    if (here1.Opers[0].Time == longestHere && here2[i].Opers[0].Time == longestWhat)
                    //    {
                    //        // А ведь это были те, что надо запихать. Поэтому говорим, что это плохо!
                    //        badOffset--;
                    //    }
                    }
                }
            }
            return 0;//(byte)(enough ? (ThatWasVspom ? 1 : 2) : 0);
        }

        static float FindFilledK(PartiaConnectorEngineData data,
            (List<Stages.Stage> Modelling, int LevelIndex) wannaMergeOps1,
            (List<Stages.Stage> Modelling, int LevelIndex) wannaMergeOps2, int maxArgument, PartiasConnectorEngineResult resultToFill)
        {
            if (wannaMergeOps1.Modelling != null)
            {
                var sessionLink = wannaMergeOps1.Modelling[0].SessionLink;
                var setting = sessionLink.SettingsController.SpecialSettings.UsedPartiaMergingRestriction;
                switch (setting)
                {
                    case Settings.SpecialSettings.PartiaMergingRestrictionType.Level2_Multiplies2:
                        {
                            return FindFilledKWithFunc(data, wannaMergeOps1, wannaMergeOps2, x => x % 2 == 0, resultToFill);
                        }
                    default:
                        {
                            return FindFilledKWithFunc(data, wannaMergeOps1, wannaMergeOps2, x => x <= maxArgument, resultToFill);
                        }
                }
            }
            else
            {
                return resultToFill.mergedNorm;
            }
        }


        static void FillDictionary(List<List<PartiaModelling>> OpersHere, Dictionary<(Uco Nomen, Uco Char, Uco Vid), (int Max, int Mul, int Cnt, float KZT)> calculator)
        {
            for (int i = 0; i < OpersHere.Count; i++)
            {
                for (int j = 0; j < OpersHere[i].Count; j++)
                {
                    var mod = OpersHere[i][j];

                    if (mod.ZadelCode != 0) continue; // То, что закрыто заделами - забиваем болт

                    var key = (mod.Opers[0].Nomenklature, mod.Opers[0].Characteristics, mod.Opers[0].OperationVid);
                    calculator.TryAdd(key, (0, 0, 0, 0));
                    var data = calculator[key];

                    int multHere = 0, maxHere = 0;

                    for (int o = 0; o < mod.Opers.Count; o++)
                    {
                        var op = mod.Opers[o];
                        var mult = op.MultiplicityInPartiasConstraint;
                        var max = op.MaxCountInPartiasConstraint;
                        if (multHere == 0 || multHere > mult) multHere = mult;
                        if (maxHere == 0 || maxHere > max) maxHere = max;
                    }

                    if (data.Max == 0 || data.Max > maxHere) data.Max = maxHere;
                    if (data.Mul == 0 || data.Mul > multHere) data.Mul = multHere;
                    data.Cnt++;

                    if (maxHere > 0 || multHere > 0) data.KZT += (float)(mod.Max == 0 ? 1 : (((IAbleForTaraCalcInterface)mod).Sum / mod.Max));

                    calculator[key] = data;
                }
            }
        }

        static float CalculateFilledK(bool withHerself, List<List<PartiaModelling>> OpersHere1, List<List<PartiaModelling>> OpersHere2, PartiasConnectorEngineResult resultToFill)
        {
            var SessionLink = OpersHere1[0][0].Opers[0].SessionLink;
            var options = SessionLink.SessionOptions;

            var needToCheckKZT = !withHerself && resultToFill.mergedNorm > 0;

            if (!needToCheckKZT) return resultToFill.mergedNorm; // Ну а что ещё возвращать?

            needToCheckKZT = (options.LaunchPlan || (!options.IsSilent && !options.PartOrZadel));

            Dictionary<(Uco Nomen, Uco Char, Uco Vid), (int Max, int Mul, int Cnt, float KZT)> calculator = new();
            FillDictionary(OpersHere1, calculator);
            FillDictionary(OpersHere2, calculator);
            float currentKZT = 1;
            foreach (var el in calculator)
            {
                if (needToCheckKZT && el.Value.Mul > 0 && el.Value.Cnt >= el.Value.Mul && el.Value.Cnt % el.Value.Mul > 0)
                {
                    // По идее, тут ещё можно доп всего накидать
                    if (SessionLink.SettingsController.CurrentSettings.Demos_EnableNonMultPartiasMerge)
                    {
                        return 0.001f;
                    }
                    else
                    {
                        resultToFill.mergedNorm = 0;
                        return 0;
                    }
                }

                if (needToCheckKZT && el.Value.Max > 0 && el.Value.Cnt > el.Value.Max)
                {
                    // По идее, тут ещё можно доп всего накидать

                    //return 0.001f;

                    resultToFill.mergedNorm = 0;
                    return 0;

                }

                if ((el.Value.Max > 0 || el.Value.Mul > 0) && currentKZT > el.Value.KZT) currentKZT = el.Value.KZT;

            }

            return currentKZT;
        }

        static float FindFilledKWithFunc(PartiaConnectorEngineData data, (List<Stages.Stage> Modelling, int LevelIndex) wannaMergeOps1, (List<Stages.Stage> Modelling, int LevelIndex) wannaMergeOps2, Func<int, bool> UsedRestriction, PartiasConnectorEngineResult resultToFill)
        {
            (PartiaClass firstPartia, PartiaClass secondPartia, bool withLog, bool withHerself) = data;
            var SessionLink = firstPartia.OpersHere[0][0].Opers[0].SessionLink;
            var options = SessionLink.SessionOptions;
            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            float filledK = 0;
            bool needToCheckKZT = (options.LaunchPlan || (!options.IsSilent && !options.PartOrZadel));
            if (wannaMergeOps1.Modelling != null && wannaMergeOps2.Modelling != null)
            {
                if (!withHerself)
                {
                    if (resultToFill.mergedNorm > 0)
                    {
                        //HashSet<Stages.Stage> finalOps = new();

                        Dictionary<int, PartiaModelling> IDtoSdvig = firstPartia.SeeSdvigsAsDic().Union(secondPartia.SeeSdvigsAsDic()).ToDictionary(x => x.Key, x => x.Value);//OpersHere.SelectMany(x => x.SelectMany(y => y.Opers.Select(z => (z.Index, y)))).ToDictionary(x => x.Index, x => x.y);
                        var fop1 = wannaMergeOps1.Modelling[0];
                        wannaMergeOps1.Modelling = firstPartia.OpersHere[wannaMergeOps1.LevelIndex].SelectMany(x => x.Opers)
                            .Where(x => x.Nomenklature == fop1.Nomenklature && x.OperationVid == fop1.OperationVid && x.Poviv == fop1.Poviv)
                            .ToList();
                        var fop2 = wannaMergeOps2.Modelling[0];
                        wannaMergeOps2.Modelling = secondPartia.OpersHere[wannaMergeOps2.LevelIndex].SelectMany(x => x.Opers)
                            .Where(x => x.Nomenklature == fop2.Nomenklature && x.OperationVid == fop2.OperationVid && x.Poviv == fop2.Poviv)
                            .ToList();

                        List<Stages.Stage> finalOps = wannaMergeOps1.Modelling.Union(wannaMergeOps2.Modelling).ToList();

                        List<Stages.Stage> finalOps2 = new(finalOps);

                        // Очищаем от лишнего
                        PartiasConnectorEngine.ClearLongestOpers(finalOps, IDtoSdvig);

                        var groups = finalOps.Union(finalOps2).GroupBy(x => (x.Poviv, x.Nomenklature, x.OperationVid)).OrderBy(x => x.First().NoChangeTara);

                        foreach (var group in groups)
                        {
                            var sdvigs = group.Select(x => IDtoSdvig[x.Index]).Distinct().ToArray();

                            bool PrevsGood = true;

                            if (sdvigs.All(x => x.Opers[0].NoChangeTara) 
                                && sdvigs.SelectMany(x => x.Opers.SelectMany(y => y.Prevs))
                                .Select(y => ALL[y])
                                .Where(op => !op.IsVspom && IDtoSdvig[op.Index].ZadelCode == 0)
                                .Select(op => IDtoSdvig[op.Index])
                                .Distinct()
                                .Count() < sdvigs.Count()) PrevsGood = false;

                            var cnt = sdvigs.Count();

                            if (resultToFill.LongestModsMaxCount == int.MaxValue || resultToFill.LongestModsMaxCount < cnt) resultToFill.LongestModsMaxCount = cnt;

                            if (UsedRestriction(cnt)/*cnt <= MaxPossibleTheirCount*/ && PrevsGood)
                            {
                                if (sdvigs.All(x => x.Opers[0].NoChangeTara)) continue; // Если у него признак не менять тару, то дальше считать коэффициент бессмысленно
                                var newK = sdvigs.Sum(x =>
                                {
                                    var sdvig = (IAbleForTaraCalcInterface)x;
                                    return sdvig.Sum / x.Max;
                                });
                                newK /= group.Count();
                                if (newK > filledK) filledK = (float)newK;
                            }
                            else
                            {
                                if (PrevsGood && sdvigs.All(x =>
                                x.Opers.All(x => (x.PrevsNormCount == 1 && x.Level >= resultToFill.LastLevel && !x.NoChangeTara)
                                ||
                                (/*x.NoChangeTara &&*/ x.Prevs.All(x => ALL[x].IsVspom || IDtoSdvig[x].ZadelCode != 0 || ALL[x].PrevsNormCount == 1))
                                ))
                                    ) continue;

                                if (needToCheckKZT)
                                {
                                    // Первый варик
                                    //filledK = 0.001f;

                                    // Текущий варик для ЗМК
                                    filledK = 0;
                                    resultToFill.mergedNorm = 0;

                                    // Текущий варик для СКО
                                    //filledK = 0.001f;
                                }

                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                filledK = resultToFill.mergedNorm;
            }
            return filledK;
        }

        static void FinalConnectorChecks(PartiaConnectorEngineData data, PartiasConnectorEngineResult resultToFill)
        {
            (PartiaClass firstPartia, PartiaClass secondPartia, bool withLog, bool withHerself) = data;
            var SessionLink = firstPartia.OpersHere[0][0].Opers[0].SessionLink;

            //resultToFill.ForOneRCGroupAlreadyUsed = new(); // Всё обнуляем - А ВОТ НЕ СТОИТ СКОРЕЕ ВСЕГО.

            if (resultToFill.mergedNorm > 0 || withHerself)
            {
                //if (withHerself) добавлен чтобы эти ошибки появлялись только ближе к концу, чтоб в процессе расчёта не появлялись ошибки
                for (var CurrentLevel = 0; CurrentLevel < firstPartia.OpersHere.Count; CurrentLevel++)
                {
                    for (var currentSdvig = 0; currentSdvig < firstPartia.OpersHere[CurrentLevel].Count; currentSdvig++)
                    {
                        var sdvig = firstPartia.OpersHere[CurrentLevel][currentSdvig];
                        if (!sdvig.RCIsChecked && !sdvig.Opers[0].IsVspom && sdvig.ZadelCode == 0) // Этот РЦ НЕ проверен, и тут нет никаких заделов
                        {

                            var fop = sdvig.Opers[0];

                            if (!GetCenterFromOneRCGroup(sdvig, resultToFill, out Uco? NeededRC, out var group1)) continue;

                            //sdvig.RCIsChecked = true;
                            if (true/*fop.EndTara.IsEmpty*/)
                            {
                                // Ищем по старинке

                                if (Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(sdvig, out var res, out _, new SumIsPossibleOptions(withLog)
                                {
                                    LogSomeErrors = withHerself,
                                    PrevsInfo = resultToFill.IDToTara,
                                    EmptyErroredTarasInfo = resultToFill.EmptyErroredTarasInfo,
                                    RecommendedTara = data.RestrictionsVia74110 == null ? null : (data.RestrictionsVia74110.TryGetValue(sdvig.Opers[0].Index, out var tara1) ? tara1 : null),
                                    RCMustBeThis = NeededRC.HasValue ? new HashSet<Uco>(1) { NeededRC.Value } : null,// (data.PossibleRCsGlobal == null ? null : (data.PossibleRCsGlobal.TryGetValue(sdvig.Opers[0].Index, out var rcs) ? rcs : null)),
                                    //ExceptTheseTaras = data.ExceptTheseTarasGlobal == null ? null : data.ExceptTheseTarasGlobal.TryGetValue(sdvig.Opers[0].Index, out var exceptTheseTaras) ? exceptTheseTaras : null,
                                    NextRCsMustBeThese = TaraSearchEngine.FindNeededNextRCsInDic(sdvig.Opers, data.PossibleRCsGlobal)
                                })) // ошибки - только если она сама с собой, на последнем этапе
                                {
                                    updateDataBcsSuccess(sdvig, res.TaraName, res.FinalRC, res.Max, res.used_format, res.CurrentPlacesCountK, resultToFill);
                                    //sdvig.RC = RC;
                                    // Запоминаем условие для одного РЦ
                                    if (sdvig.Opers[0].OnOneRC) resultToFill.ForOneRCGroupAlreadyUsed.TryAdd(group1, res.FinalRC);
                                }
                                else
                                {
                                    // не нашёл нифига. Записываем в лог при необходимости.
                                    if (withHerself)
                                    {
                                        if (SessionLink.SettingsController.CurrentSettings.LogErrors)
                                        {
                                            var prevs = sdvig.Opers.SelectMany(x => x.Prevs).Distinct(); // находим предков
                                            var possibleTara = prevs
                                                .Where(x => resultToFill.IDToTara.ContainsKey(x)) // выбираем тех, про кого есть инфа в словаре о предках
                                                .Select(x => resultToFill.IDToTara[x]) // выбираем тару из них
                                                .Distinct();
                                            var possibleCenters = fop.PossibleCenters ?? new List<Uco>() { fop.OriginalCenter }; // Формируем список рц, взяв его из первой попавшейся операции в этом сдвиге / моделировании

                                            //AddPartiaError(fop.Index, PartiaErrorLogType.NotFoundRCAfterCheck, (new Uco(Guid.Empty, ""), possibleCenters, possibleTara.ToList()));
                                        }
                                    }
                                    else
                                    {
                                        if (!SessionLink.SettingsController.CurrentSettings.Demos_IgnoreAnyTaraErrorsInPartias)
                                            resultToFill.mergedNorm = 0;
                                    }
                                }
                            }
                            else
                            {
                                //Поищем РЦ, у которого среди отдающих тар только те, что были у предков
                                if (Taras.TaraSearchEngine.TaraSearchEngine.FindSomeCentersBecauseOfPrevs(sdvig, null, resultToFill.IDToTara, new() { fop.EndTara }, out var RC, out var allRCs, out var allOtdTaras, true))
                                {
                                    sdvig.RC = RC;
                                    sdvig.RCIsChecked = true;
                                }
                                else
                                {
                                    if (withHerself)
                                    {
                                        //AddPartiaError(fop.Index, PartiaErrorLogType.NotFoundRCAfterCheck, (fop.OrigTara, allRCs, allOtdTaras));
                                    }
                                    else
                                        resultToFill.mergedNorm = 0;
                                }
                                //AddNewPartiaError(fop.Index, PartiaErrorLogType.No_RC_Tara_Intersect, new PartiaErrorData("блабла", allRCs, null, allOtdTaras, null, null));
                            }


                        }
                    }
                }
            }
        }

        public static void PrepareResultForOneRCData(PartiasConnectorEngineResult finalResult, List<IAbleForTaraCalcInterface> listOnOneRc1, List<IAbleForTaraCalcInterface> listOnOneRc2, bool searchForMax)
        {
            //return;


            var SessionLink = listOnOneRc1[0].AllOpers[0].SessionLink;
            var CentersData = SessionLink.MasterDataLinks.CentersMasterData.CentersData;

            finalResult.ForOneRCGroups = new();
            finalResult.PossibleCentersForOneRCGroups = new();
            finalResult.ForOneRCGroupAlreadyUsed = new();
            var groupToCheck1 = listOnOneRc1.Where(x => x.AllOpers[0].OnOneRC).GroupBy(mod => (mod.AllOpers[0].OperationVid, mod.AllOpers[0].Nomenklature));
            var groupToCheck2 = listOnOneRc2.Where(x => x.AllOpers[0].OnOneRC).GroupBy(mod => (mod.AllOpers[0].OperationVid, mod.AllOpers[0].Nomenklature));

            Dictionary<(Uco vid, Uco nomen), List<IAbleForTaraCalcInterface>> groupsToCheck = new();
            foreach (var group in groupToCheck1)
            {
                groupsToCheck.TryAdd(group.Key, new());
                groupsToCheck[group.Key].AddRange(group);
            }
            foreach (var group in groupToCheck2)
            {
                groupsToCheck.TryAdd(group.Key, new());
                groupsToCheck[group.Key].AddRange(group);
            }

            foreach (var group in groupsToCheck)
            {
                var groupN = finalResult.ForOneRCGroups.Count; // Получаем номер для этой группы
                foreach (var mod in group.Value)
                {
                    finalResult.ForOneRCGroups.Add(mod.AllOpers[0].Index, groupN); // Заполняем данными
                }

                List<Uco> allowedRCs = group.Value.First().AllOpers[0].PossibleCenters ?? new List<Uco>() { group.Value.First().AllOpers[0].OriginalCenter };
                allowedRCs = allowedRCs.Select(rc =>
                {
                    var res0 = group.Value.Select(mod =>
                    {
                        var possible = Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(mod, out var res, out _, new SumIsPossibleOptions(false)
                        {
                            LogSomeErrors = false, // ошибки не фиксируем
                            RCMustBeThis = new(1) { rc }, // РЦ должен быть именно такой
                            SearchForMaxTara = searchForMax
                        });
                        return (possible, res);
                    });

                    bool allCan = res0.All(x => x.possible);
                    double max = 0;
                    if (allCan) max = res0.Max(x => x.res.Max);

                    return (rc, allCan, max);
                })
                    .Where(rc => rc.allCan)
                    .OrderByDescending(rc => rc.max)
                    .ThenByDescending(rc => finalResult.RecommendedSubdivision.HasValue && CentersData[rc.rc].Subdivision == finalResult.RecommendedSubdivision.Value ? 1 : 0) // Сортируем по цеху, если он вообще задан
                    .Select(rc => rc.rc)
                    .ToList();

                finalResult.PossibleCentersForOneRCGroups.Add(groupN, allowedRCs); // Сохраняем информацию
            }
        }


        static int PreZadelsConnection(PartiaClass partia1, PartiaClass partia2, bool withHerself/*пока низачем не нужен, но мб потом пригодится*/, HashSet<int> restrictedCodes)
        {
            var dic1 = SeeSdvigsAsDic(partia1.OpersHere);

            var myPartiaSdvigs = dic1.AsValueEnumerable().Where(x => x.Value.ZadelsGUIDs != null && x.Value.ZadelsGUIDs.Count > 0 && (restrictedCodes == null || !restrictedCodes.Contains(x.Value.ZadelCode))); // Получаем все сдвиги с заделами

            if (myPartiaSdvigs.Count() == 0) return 0;

            var allOtherSdvigs = SeeSdvigsAsDic(partia2.OpersHere);

            int maxLevel = 0;

            foreach (var sdvigWithZadel in myPartiaSdvigs)
            {
                var mod = sdvigWithZadel.Value;
                var firstOp = mod.Opers[0];
                var possibleOtherPartiaSdvigs = allOtherSdvigs.AsValueEnumerable().Where(x =>
                {
                    var othermod = x.Value;
                    var firstOpOther = othermod.Opers[0];

                    return othermod.ZadelCode == 0 // Нужно искать те, которым вообще ничего не назначено
                    && firstOpOther.SessionLink.MasterDataLinks.MergeConditionsMasterData.StagesEqualityCheck(firstOp, firstOpOther, out _, out _, out _) // Равны по настройкам
                    && firstOpOther.Level == firstOp.Level // Одинаковый уровень
                    && firstOpOther.IsVspom == firstOp.IsVspom // Вспомогательные только друг с другом
                    && firstOpOther.UsesVspomTara == firstOp.UsesVspomTara; // Вспомогательные тары только друг с другом
                }
                );
                if (possibleOtherPartiaSdvigs.Count() > 0)
                {
                    maxLevel = firstOp.Level;

                    continue;

                    // Получаем список УЖЕ зарезервированных модов, которые в моей партии при объединении схлопнутся.
                    var alreadyReserved = dic1.AsValueEnumerable().Where(x => x.Value.Opers[0].Level == firstOp.Level && x.Value.ZadelCode == mod.ZadelCode).Select(x => x.Value as IAbleForTaraCalcInterface).ToHashSet();

                    // Получаем количество задела, и вычитаем из него то количество, которое я уже зарезервировал за другими этапчиками.
                    var allCount = mod.ZadelsGUIDs.AsValueEnumerable().Min(x => firstOp.SessionLink.MasterDataLinks.ZadelsMasterData.LinearZadelsDictionary.TryGetValue(x, out var zet) ? zet.Count : firstOp.SessionLink.MasterDataLinks.LeftoversMasterData.LinearLeftoversCount[x]); // Возьмем минимальный из заделов здесь
                    var reservedCount = alreadyReserved.Count > 0 ? alreadyReserved.Sum(x => x.Sum) : 0;
                    var availableCount = allCount - reservedCount;

                    if (availableCount > 0)
                    {
                        // проверки на сумму, чтобы задел не был больше положенного. Решение задачи о рюкзаке
                        long[] values = possibleOtherPartiaSdvigs.Select(x => (long)((x.Value as IAbleForTaraCalcInterface).Sum * 1000.0)).ToArray();
                        long[,] weights = new long[1, values.Length];
                        for (int i = 0; i < values.Length; i++) weights[0, i] = values[i];

                        KnapsackSolver solver = new KnapsackSolver(KnapsackSolver.SolverType.KNAPSACK_DYNAMIC_PROGRAMMING_SOLVER, $"KnapsackZadelsFor: {mod.ZadelCode}");
                        solver.Init(values, weights, new long[1] { (long)(availableCount * 1000.0) });
                        solver.set_time_limit(1.0);
                        long result = solver.Solve();
                        if (result > 0)
                        {
                            var packedItems = possibleOtherPartiaSdvigs.Where((x, index) => solver.BestSolutionContains(index)).Select(x => x.Value).ToArray();
                            for (int i = 0; i < packedItems.Length; i++)
                            {
                                SetZadelCodes(packedItems[i], mod.ZadelCode, mod.ZadelsGUIDs, mod.Max); // Назначаем
                            }

                        }
                    }

                    void SetZadelCodes(PartiaModelling mod, int zadelCode, List<Guid> zadelsGUIDs, double max)
                    {
                        // TO DO: А что если тут какой-то другой задел оказался???
                        mod.ZadelCode = zadelCode;
                        mod.ZadelsGUIDs = zadelsGUIDs;
                        //mod.Max = max;
                        var prevMods = mod.Opers.AsValueEnumerable().SelectMany(x => x.Prevs).Select(x => allOtherSdvigs[x]).Distinct();
                        foreach (var prev in prevMods) SetZadelCodes(prev, zadelCode, new() /*потому что уровням выше нужно пустоту задать*/, max);
                    }
                }
            }

            return maxLevel;
        }

        public static Uco GetBestSubdivisionHere(List<PartiaModelling> listOnOne)
        {
            var SessionLink = listOnOne[0].Opers[0].SessionLink;
            var CentersData = SessionLink.MasterDataLinks.CentersMasterData.CentersData;

            var bestSubdivision = listOnOne.SelectMany(x => x.Opers[0].PossibleCenters ?? new List<Uco>() { x.Opers[0].OriginalCenter }) // Сначала найдём ВСЕ центры здесь, у обоих партий
                .Where(x => !x.IsEmpty)
                    .GroupBy(x => CentersData[x].Subdivision) // группируем по цеху
                    .MaxBy(x => x.Count()) // найдем максимальный
                    .Key; // берем ключ, потому что это цех
            return bestSubdivision;
        }

        public static PartiasConnectorEngineResult Connect(PartiaConnectorEngineData data)
        {
            (PartiaClass firstPartia, PartiaClass secondPartia, bool withLog, bool withHerself) = data;
            if (firstPartia.IsRestricted || secondPartia.IsRestricted || firstPartia.IsExcepted || secondPartia.IsExcepted || firstPartia.MergeFlag != secondPartia.MergeFlag) // они ограниченные, либо флаг не тот?
            {
                return new PartiasConnectorEngineResult(0, 0, 0, int.MaxValue, 0, 0, 0);
            }

            var CurrentCount = firstPartia.OpersHere.Sum(x => x.Count) + secondPartia.OpersHere.Sum(x => x.Count);

            var SessionLink = firstPartia.OpersHere[0][0].Opers[0].SessionLink;
            var CentersData = SessionLink.MasterDataLinks.CentersMasterData.CentersData;

            int maxLevelZadels = 0;

            if (!SessionLink.SessionOptions.LaunchZadel)
            {
                int l1 = PreZadelsConnection(firstPartia, secondPartia, withHerself, secondPartia.SeeSdvigsAsDic().Where(x => x.Value.ZadelCode > 0).Select(x => x.Value.ZadelCode).Distinct().ToHashSet());
                if (l1 > maxLevelZadels) maxLevelZadels = l1;
                int l2 = PreZadelsConnection(secondPartia, firstPartia, withHerself, firstPartia.SeeSdvigsAsDic().Where(x => x.Value.ZadelCode > 0).Select(x => x.Value.ZadelCode).Distinct().ToHashSet()); // Друг для друга создаём инфу о заделах
                if (l2 > maxLevelZadels) maxLevelZadels = l2;
            }

            // Сначала находим время
            (List<Stages.Stage> Modelling, int LevelIndex) wannaMergeOps1 = (null, 0);
            (List<Stages.Stage> Modelling, int LevelIndex) wannaMergeOps2 = (null, 0);

            if (SessionLink.SettingsController.CurrentSettings.Demos_UseOldPartiasRestrictions)
            {
                wannaMergeOps1 = firstPartia.GetLongestOpersForPostCheck();
                wannaMergeOps2 = secondPartia.GetLongestOpersForPostCheck();


                if (firstPartia.MaxThereCountForLongestOpers == -1 && wannaMergeOps1.Modelling != null)
                {
                    var fop1 = wannaMergeOps1.Modelling[0];
                    firstPartia.MaxThereCountForLongestOpers = firstPartia.OpersHere[wannaMergeOps1.LevelIndex].SelectMany(x => x.Opers)
                        .Where(x => x.Nomenklature == fop1.Nomenklature && x.OperationVid == fop1.OperationVid && x.Poviv == fop1.Poviv)
                        .Count();
                }
                if (secondPartia.MaxThereCountForLongestOpers == -1 && wannaMergeOps2.Modelling != null)
                {
                    var fop2 = wannaMergeOps2.Modelling[0];
                    secondPartia.MaxThereCountForLongestOpers = secondPartia.OpersHere[wannaMergeOps2.LevelIndex].SelectMany(x => x.Opers)
                        .Where(x => x.Nomenklature == fop2.Nomenklature && x.OperationVid == fop2.OperationVid && x.Poviv == fop2.Poviv)
                        .Count();
                }
            }

            var finalResult = new PartiasConnectorEngineResult(0, 0, 0, int.MaxValue, CurrentCount * 2, CurrentCount * 2, CurrentCount * 2);

            // Найдём самый часто встречающийся ЦЕХ.
            // + ограничения на одном рц запомним
            var listOnOneRc1 = SeeSdvigsAsList(firstPartia.OpersHere);
            List<PartiaModelling> listOnOneRc2 = data.withHerself ? new() : SeeSdvigsAsList(secondPartia.OpersHere);


            if (SessionLink.SettingsController.SpecialSettings.CalculateBestSubdivisionInPartias)
            {
                //var bestSubdivision = .SelectMany(x => x.Opers[0].PossibleCenters ?? new List<Uco>() { x.Opers[0].OriginalCenter }) // Сначала найдём ВСЕ центры здесь, у обоих партий
                //    .GroupBy(x => CentersData[x].Subdivision) // группируем по цеху
                //    .MaxBy(x => x.Count()) // найдем максимальный
                //    .Key; // берем ключ, потому что это цех
                finalResult.RecommendedSubdivision = GetBestSubdivisionHere(listOnOneRc1.Union(listOnOneRc2).ToList());
            }

            PrepareResultForOneRCData(finalResult, listOnOneRc1.Select(x => (IAbleForTaraCalcInterface)x).ToList(), listOnOneRc2.Select(x => (IAbleForTaraCalcInterface)x).ToList(), true);

            //));

            // Объединяем по всем уровням
            var isMerging = true;

            bool firstLevelBad = false;

            for (int CurrentLevel = 0; CurrentLevel < Math.Max(secondPartia.OpersHere.Count, firstPartia.OpersHere.Count); CurrentLevel++) // сначала проходимся по стадиям
            {
                if (isMerging)
                {
                    if (firstPartia.OpersHere.Count - 1 < CurrentLevel || secondPartia.OpersHere.Count - 1 < CurrentLevel || !AbleToMergeLevel(firstPartia.OpersHere[CurrentLevel], secondPartia.OpersHere[CurrentLevel], CurrentLevel, CurrentLevel == 0, data, finalResult))
                    {
                        if (CurrentLevel == 0) firstLevelBad = true;//merged = 0; // Если я даже на первом уровне не смог объединить что-то, то считаю, что вообще ничего не смог
                        if (CurrentLevel < firstPartia.OpersHere.Count && maxLevelZadels <= firstPartia.OpersHere[CurrentLevel][0].Opers[0].Level) isMerging = false;
                        //if (CurrentLevel == 0) break; // если даже на первом уровне не смог, то вообще ничего не позволяем ему соединить, а сразу обрываем цикл
                    }
                    else
                        finalResult.LastLevel = firstPartia.OpersHere[CurrentLevel][0].Opers[0].Level;
                }
                if (finalResult.mergedVspom > 0/*smthWasGood*/ || finalResult.mergedNorm > 0 || isMerging)
                {
                    if (firstPartia.OpersHere.Count - 1 < CurrentLevel)
                    {
                        firstPartia.OpersHere.Add(new());
                    }

                    if (secondPartia.OpersHere.Count - 1 >= CurrentLevel) //OpersHere.Add(new ());
                    {
                        MoveThem(firstPartia.OpersHere[CurrentLevel], secondPartia.OpersHere[CurrentLevel]);
                    }
                }
            }

            if (firstLevelBad)
                finalResult.mergedNorm = 0;

            // Теперь - группируем, чтобы для каждого повива/задела было лишь по одному запуску
            //if (What.OpersHere[0].Count > 0) merged = 0; // очевидно что он не схлопнулся до конца, и это плохо
            //float K = 0;

            float filledK;

            if (SessionLink.SettingsController.CurrentSettings.Demos_UseOldPartiasRestrictions)
            {
                filledK = FindFilledK(data, wannaMergeOps1, wannaMergeOps2, Math.Max(Math.Max(firstPartia.MaxThereCountForLongestOpers, secondPartia.MaxThereCountForLongestOpers), 2), finalResult);
            }
            else
            {
                filledK = CalculateFilledK(withHerself, firstPartia.OpersHere, secondPartia.OpersHere, finalResult);
            }

            // После всего: проходим по всем уровням, по всем РЦ, и смотрим - кому РЦ не задался, тому пытаемся задать с учётом отдающей тары, чтоб он в выходной таблице норм был.
            FinalConnectorChecks(data, finalResult);

            if (finalResult.mergedNorm > 0 && secondPartia.PossibleDeadline > firstPartia.PossibleDeadline)
            {
                firstPartia.PossibleDeadline = secondPartia.PossibleDeadline;
            }

            var FinalCount = firstPartia.OpersHere.Sum(x => x.Count) + secondPartia.OpersHere.Sum(x => x.Count);

            if (finalResult.mergedNorm > 0 && !withHerself) firstPartia.IsElementar = false; // Теперь она уже НЕ элементарная

            if (finalResult.mergedNorm == 0) filledK = 0;

            firstPartia.LastK = filledK;

            if (finalResult.mergedNorm > 0 && !withHerself) firstPartia.GPsCountNow += secondPartia.GPsCountNow;

            finalResult.FilledK = filledK;
            finalResult.Diff = CurrentCount - FinalCount;
            finalResult.Total = finalResult.mergedNorm;

            return finalResult;
        }

    }
}

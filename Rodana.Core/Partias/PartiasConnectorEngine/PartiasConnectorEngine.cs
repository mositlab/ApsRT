using Azure;
using Microsoft.VisualBasic;
using Rodana.Core.Partias.PartiasClasses;
using Rodana.Core.Partias.TaraSearchEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.PartiasConnectorEngine
{
    public class PartiasConnectorEngine
    {

        public static Dictionary<int, PartiaModelling> SeeSdvigsAsDic(List<List<PartiaModelling>> opers)
        {
            var list = opers.SelectMany(x => x.SelectMany(y => y.Opers.Select(z => (z.Index, y))));
            return list.ToDictionary(x => x.Index, x => x.y);
        }

        public static List<PartiaModelling> SeeSdvigsAsList(List<List<PartiaModelling>> opers)
        {
            var list = opers.SelectMany(x => x).ToList();
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


        public static void updateDataBcsSuccess(PartiaModelling here1, Uco gotTara, Uco gotRc, double gotMax, DimensionUnit use_format, PartiasConnectorEngineResult resultToFill)
        {
            here1.Tara = gotTara;
            here1.RC = gotRc;
            here1.RCIsChecked = true;
            here1.Max = gotMax;
            here1.Format = use_format;
            if (gotTara.IsEmpty) return; // Дальше не идём, чтобы не засорять словарь мешающей инфой.
            foreach (var Index in here1.Opers)
            {
                var id2 = Index.Index;

                if (resultToFill.IDToTara.ContainsKey(id2))
                {
                    resultToFill.IDToTara[id2] = gotTara;
                }
                else
                {
                    resultToFill.IDToTara.Add(id2, gotTara);
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

        static bool AbleToMergeLevel(List<PartiaModelling> here1, List<PartiaModelling> here2, int CurrLevel, bool isFirstLevel, bool withHerself, bool withLog, PartiasConnectorEngineResult resultToFill)
        {
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

                        res = AbleToMerge(here1[i], here2, CurrLevel, false, withHerself, withLog, resultToFill); // false - внутри себя никаких проверок внутри себя уже делать НЕ НАДО, поэтому имитирую, что это НЕ первый уровень
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
                    var res = AbleToMerge(h, here2, CurrLevel, isFirstLevel, withHerself, withLog, resultToFill);
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


        static byte AbleToMerge(PartiaModelling here1, // один сдвиг
            List<PartiaModelling> here2, // все остальные тут
            int currentLevel, bool isFirstLevel, bool withHerself, bool withLog, PartiasConnectorEngineResult resultToFill)
        {
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
                        (grp1.Count == 0 ^ grp2.Count == 0) ||
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

                    var here01 = new PartiaModellingSmall(new List<Stages.Stage>(here1.Opers), here1.ZadelCode, here1.ZadelsGUIDs); // копируем слегка
                    here01.Opers.AddRange(here2[i].Opers);
                    var sum = false;
                    Uco gotTara = new(Guid.Empty, "");
                    Uco gotRc = here1.Opers[0].PossibleCenters?[0] ?? here1.Opers[0].OriginalCenter;//new(Guid.Empty, "");
                    var gotMax = here1.Max;
                    var use_format = DimensionUnit.Unknown;

                    bool isZadel = here1.ZadelCode != 0;

                    // Если это задел, то юзается тара из первого же задела тут.
                    // TODO: а что если у каждого задела свои тары?
                    if (isZadel)
                    {
                        gotMax = here01.Opers.Sum(x => x.Count);
                        if (here1.ZadelsGUIDs.Count > 0)
                        {
                            gotTara = Zadels.LinearZadelsDictionary[here1.ZadelsGUIDs[0]].UsedTara;
                        }
                    }

                    #region Условие "на одном РЦ" - поиск нужного РЦ или хотя бы ограничение

                    Uco? NeededRC = null;

                    var group1 = here1.Opers[0].OnOneRC && resultToFill.ForOneRCGroups.TryGetValue(here1.Opers[0].Index, out var group11) ? group11 : -1;
                    var group2 = here2[i].Opers[0].OnOneRC && resultToFill.ForOneRCGroups.TryGetValue(here2[i].Opers[0].Index, out var group22) ? group22 : -1;

                    if (group1 != -1 && resultToFill.ForOneRCGroupAlreadyUsed.TryGetValue(group1, out var myRC))
                    {
                        // РЦ Найден.
                        NeededRC = myRC;
                        if (group2 != -1 && resultToFill.ForOneRCGroupAlreadyUsed.TryGetValue(group2, out myRC))
                        {
                            if (NeededRC != myRC)
                            {
                                // ОШИБКА !!! Притом невозможная...
                                continue;
                            }
                        }
                    }
                    else
                    {
                        // Установки ещё не было. Смотрим полностью
                        var list1 = here1.Opers[0].OnOneRC ? (group1 != -1 && resultToFill.PossibleCentersForOneRCGroups.TryGetValue(group1, out var list1F) ? list1F : new()) : (here1.Opers[0].PossibleCenters ?? new List<Uco>() { here1.Opers[0].OriginalCenter });

                        var list2 = here2[i].Opers[0].OnOneRC ? (group2 != -1 && resultToFill.PossibleCentersForOneRCGroups.TryGetValue(group2, out var list2F) ? list2F : new()) : (here2[i].Opers[0].PossibleCenters ?? new List<Uco>() { here2[i].Opers[0].OriginalCenter });

                        if (here1.Opers[0].OnOneRC)
                        {
                            list1 = list1.Intersect(list2).ToList();
                            if (list1.Count == 0)
                            {
                                // ОШИБКА !!!
                                continue;
                            }
                            else
                            {
                                NeededRC = list1[0];
                            }
                        }
                        else
                        {
                            if (here2[i].Opers[0].OnOneRC)
                            {
                                list2 = list2.Intersect(list1).ToList();
                                if (list2.Count == 0)
                                {
                                    // ОШИБКА !!!
                                    continue;
                                }
                                else
                                {
                                    NeededRC = list2[0];
                                }
                            }
                        }
                    }
                    #endregion

                    if (!isVspom && !isZadel)
                    {
                        sum = TaraSearchEngine.TaraSearchEngine.GetAllVariantsForThese(here01, out var searchData, new SumIsPossibleOptions(withLog) { PrevsInfo = resultToFill.IDToTara })
                            &&
                            TaraSearchEngine.TaraSearchEngine.FilterAllVariants(here01, searchData, out gotTara, out gotRc, out gotMax, out use_format, out _, new SumIsPossibleOptions(withLog)
                            {
                                PrevsInfo = resultToFill.IDToTara,

                                TaraMustBeThis =
                                here1.ZadelsGUIDs.Count > 0 ?
                                (Zadels.LinearZadelsDictionary[here1.ZadelsGUIDs[0]].UsedTara)
                                :
                                null,

                                LogSomeErrors = false,

                                RecommendedSubdivision = resultToFill.RecommendedSubdivision,

                                RCMustBeThis = NeededRC
                            });
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
                        if (here1.Opers[0].OnOneRC) resultToFill.ForOneRCGroupAlreadyUsed.Add(group1, gotRc);
                        if (here2[i].Opers[0].OnOneRC) resultToFill.ForOneRCGroupAlreadyUsed.Add(group2, gotRc);

                        here1.Opers.AddRange(here2[i].Opers);
                        here2[i].Opers.Clear();

                        if (sum || isZadel)
                        {
                            updateDataBcsSuccess(here1, gotTara, gotRc, gotMax, use_format, resultToFill);
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

                            }
                        }
                        //}

                        return (byte)(isZadel ? 0 : (isVspom ? 1 : 2));//(byte)(enough ? (ThatWasVspom ? 1 : 2) : 0);
                                                                       //break;
                    }
                    //else
                    //{
                    //    if (here1.Opers[0].Time == longestHere && here2[i].Opers[0].Time == longestWhat)
                    //    {
                    //        // А ведь это были те, что надо запихать. Поэтому говорим, что это плохо!
                    //        badOffset--;
                    //    }
                    //}
                }
            }
            return 0;//(byte)(enough ? (ThatWasVspom ? 1 : 2) : 0);
        }


        static float FindFilledK(PartiaClass firstPartia, PartiaClass secondPartia, bool withHerself, (List<Stages.Stage> Modelling, int LevelIndex) wannaMergeOps1, (List<Stages.Stage> Modelling, int LevelIndex) wannaMergeOps2, PartiasConnectorEngineResult resultToFill)
        {
            var SessionLink = firstPartia.OpersHere[0][0].Opers[0].SessionLink;
            var options = SessionLink.SessionOptions;
            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            float filledK = 0;
            if (wannaMergeOps1.Modelling != null && wannaMergeOps2.Modelling != null && (options.LaunchPlan || (!options.IsSilent && !options.PartOrZadel)))
            {
                if (!withHerself)
                {
                    if (resultToFill.mergedNorm > 0)
                    {
                        //HashSet<Stages.Stage> finalOps = new();

                        Dictionary<int, PartiaModelling> IDtoSdvig = firstPartia.SeeSdvigsAsDic().Union(secondPartia.SeeSdvigsAsDic()).ToDictionary(x => x.Key, x => x.Value);//OpersHere.SelectMany(x => x.SelectMany(y => y.Opers.Select(z => (z.Index, y)))).ToDictionary(x => x.Index, x => x.y);
                        wannaMergeOps1.Modelling = firstPartia.OpersHere[wannaMergeOps1.LevelIndex].SelectMany(x => x.Opers).ToList();
                        wannaMergeOps2.Modelling = secondPartia.OpersHere[wannaMergeOps2.LevelIndex].SelectMany(x => x.Opers).ToList();

                        List<Stages.Stage> finalOps = wannaMergeOps1.Modelling.Union(wannaMergeOps2.Modelling).ToList();

                        List<Stages.Stage> finalOps2 = new(finalOps);

                        // Очищаем от лишнего
                        PartiasConnectorEngine.ClearLongestOpers(finalOps, IDtoSdvig);

                        var groups = finalOps.Union(finalOps2).GroupBy(x => (x.Poviv, x.Nomenklature, x.OperationVid)).OrderBy(x => x.First().NoChangeTara);

                        foreach (var group in groups)
                        {
                            var sdvigs = group.Select(x => IDtoSdvig[x.Index]).Distinct().ToArray();

                            bool PrevsGood = true;

                            if (sdvigs.All(x => x.Opers[0].NoChangeTara) && sdvigs.SelectMany(x => x.Opers.SelectMany(y => y.Prevs)).Select(y => ALL[y]).Where(op => !op.IsVspom && IDtoSdvig[op.Index].ZadelCode == 0).Select(op => IDtoSdvig[op.Index]).Distinct().Count() < sdvigs.Count()) PrevsGood = false;

                            var cnt = sdvigs.Count();

                            if (resultToFill.LongestModsMaxCount == int.MaxValue || resultToFill.LongestModsMaxCount < cnt) resultToFill.LongestModsMaxCount = cnt;

                            if (cnt < 3 && PrevsGood)
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
                                //K = 0.001f;
                                filledK = 0;
                                resultToFill.mergedNorm = 0;
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

        static void FinalConnectorChecks(PartiaClass firstPartia, PartiaClass secondPartia, bool withHerself, bool withLog, PartiasConnectorEngineResult resultToFill)
        {
            var SessionLink = firstPartia.OpersHere[0][0].Opers[0].SessionLink;
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
                            //sdvig.RCIsChecked = true;
                            if (fop.EndTara.IsEmpty)
                            {
                                // Ищем по старинке

                                if (TaraSearchEngine.TaraSearchEngine.SumIsPossible(sdvig, out var newTara, out var newRC, out var newMax, out var newUnit, out _, out _, new SumIsPossibleOptions(withLog) { LogSomeErrors = withHerself, PrevsInfo = resultToFill.IDToTara })) // ошибки - только если она сама с собой, на последнем этапе
                                {
                                    updateDataBcsSuccess(sdvig, newTara, newRC, newMax, newUnit, resultToFill);
                                    //sdvig.RC = RC;
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
                                        resultToFill.mergedNorm = 0;
                                }
                            }
                            else
                            {
                                //Поищем РЦ, у которого среди отдающих тар только те, что были у предков
                                if (TaraSearchEngine.TaraSearchEngine.FindSomeCentersBecauseOfPrevs(sdvig, null, resultToFill.IDToTara, new() { fop.EndTara }, out var RC, out var allRCs, out var allOtdTaras, true))
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


        static void PreZadelsConnection(PartiaClass partia1, PartiaClass partia2, bool withHerself)
        {
            var myPartiaSdvigs = SeeSdvigsAsDic(partia1.OpersHere).Where(x => x.Value.ZadelsGUIDs.Count > 0); // Получаем все сдвиги с заделами

            var allOtherSdvigs = SeeSdvigsAsDic(partia2.OpersHere);

            foreach (var sdvigWithZadel in myPartiaSdvigs)
            {
                var mod = sdvigWithZadel.Value;
                var firstOp = mod.Opers[0];
                var possibleOtherPartiaSdvigs = allOtherSdvigs.Where(x =>
                {
                    var othermod = x.Value;
                    var firstOpOther = othermod.Opers[0];

                    return firstOpOther.SessionLink.MasterDataLinks.MergeConditionsMasterData.StagesEqualityCheck(firstOp, firstOpOther, out _, out _, out _)
                    && firstOpOther.IsVspom == firstOp.IsVspom // Вспомогательные только друг с другом
                    && firstOpOther.UsesVspomTara == firstOp.UsesVspomTara; // Вспомогательные тары только друг с другом
                }
                );
                if (possibleOtherPartiaSdvigs.Count() > 0)
                {
                    // Где-то тут нужны проверки на сумму, чтобы задел не был больше положенного
                    // в whereIsUsed по идее тоже надо добавить
                    SetZadelCodes(possibleOtherPartiaSdvigs.First().Value, mod.ZadelCode, mod.ZadelsGUIDs); // Назначаем пока только самому первому


                    void SetZadelCodes(PartiaModelling mod, int zadelCode, List<Guid> zadelsGUIDs)
                    {
                        mod.ZadelCode = zadelCode;
                        mod.ZadelsGUIDs = zadelsGUIDs;
                        var prevMods = mod.Opers.SelectMany(x => x.Prevs).Select(x => allOtherSdvigs[x]).Distinct();
                        foreach (var prev in prevMods) SetZadelCodes(prev, zadelCode, zadelsGUIDs);
                    }
                }
            }

            
        }

        public static PartiasConnectorEngineResult Connect(PartiaClass firstPartia, PartiaClass secondPartia, bool withLog, bool withHerself)
        {
            if (firstPartia.IsRestricted || secondPartia.IsRestricted || firstPartia.MergeFlag != secondPartia.MergeFlag) // они ограниченные, либо флаг не тот?
            {
                return new PartiasConnectorEngineResult(0, 0, 0, int.MaxValue);
            }

            var CurrentCount = firstPartia.OpersHere.Sum(x => x.Count) + secondPartia.OpersHere.Sum(x => x.Count);

            var SessionLink = firstPartia.OpersHere[0][0].Opers[0].SessionLink;
            var CentersData = SessionLink.MasterDataLinks.CentersMasterData.CentersData;

            if (!SessionLink.SessionOptions.LaunchZadel)
            {
                PreZadelsConnection(firstPartia, secondPartia, withHerself);
                PreZadelsConnection(secondPartia, firstPartia, withHerself); // Друг для друга создаём инфу о заделах
            }

            // Сначала находим время
            var wannaMergeOps1 = firstPartia.GetLongestOpers();
            var wannaMergeOps2 = secondPartia.GetLongestOpers();

            var finalResult = new PartiasConnectorEngineResult(0, 0, 0, int.MaxValue, CurrentCount * 2, CurrentCount * 2, CurrentCount * 2);

            // Найдём самый часто встречающийся ЦЕХ.
            // + ограничения на одном рц запомним
            var listOnOneRc1 = SeeSdvigsAsList(firstPartia.OpersHere);
            var listOnOneRc2 = SeeSdvigsAsList(secondPartia.OpersHere);


            if (SessionLink.SettingsController.SpecialSettings.CalculateBestSubdivisionInPartias)
            {
                var bestSubdivision = listOnOneRc1.Union(listOnOneRc2).SelectMany(x => x.Opers[0].PossibleCenters ?? new List<Uco>() { x.Opers[0].OriginalCenter }) // Сначала найдём ВСЕ центры здесь, у обоих партий
                    .GroupBy(x => CentersData[x].Subdivision) // группируем по цеху
                    .MaxBy(x => x.Count()) // найдем максимальный
                    .Key; // берем ключ, потому что это цех
                finalResult.RecommendedSubdivision = bestSubdivision;
            }

            finalResult.ForOneRCGroups = new();
            finalResult.PossibleCentersForOneRCGroups = new();
            finalResult.ForOneRCGroupAlreadyUsed = new();
            var groupToCheck1 = listOnOneRc1.Where(x => x.Opers[0].OnOneRC).GroupBy(mod => (mod.Opers[0].Level, mod.Opers[0].OperationVid, mod.Opers[0].Nomenklature));
            var groupToCheck2 = listOnOneRc2.Where(x => x.Opers[0].OnOneRC).GroupBy(mod => (mod.Opers[0].Level, mod.Opers[0].OperationVid, mod.Opers[0].Nomenklature));
            var groupsToCheck = groupToCheck1.Union(groupToCheck2);
            foreach (var group in groupsToCheck)
            {
                var groupN = finalResult.ForOneRCGroups.Count; // Получаем номер для этой группы
                foreach (var mod in group)
                {
                    finalResult.ForOneRCGroups.Add(mod.Opers[0].Index, groupN); // Заполняем данными
                }

                List<Uco> allowedRCs = group.First().Opers[0].PossibleCenters ?? new List<Uco>() { group.First().Opers[0].OriginalCenter };
                allowedRCs = allowedRCs.Where(rc =>
                    group.All(
                        mod => TaraSearchEngine.TaraSearchEngine.SumIsPossible(mod, out _, out _, out _, out _, out _, out _, new SumIsPossibleOptions(false)
                        {
                            LogSomeErrors = false, // ошибки не фиксируем
                            RCMustBeThis = rc // РЦ должен быть именно такой
                        })
                    )
                    )
                    .OrderByDescending(rc => finalResult.RecommendedSubdivision.HasValue && CentersData[rc].Subdivision == finalResult.RecommendedSubdivision.Value ? 1 : 0) // Сортируем по цеху, если он вообще задан
                    .ToList();

                finalResult.PossibleCentersForOneRCGroups.Add(groupN, allowedRCs); // Сохраняем информацию
            }

            //));

            // Объединяем по всем уровням
            var isMerging = true;

            bool firstLevelBad = false;

            for (int CurrentLevel = 0; CurrentLevel < Math.Max(secondPartia.OpersHere.Count, firstPartia.OpersHere.Count); CurrentLevel++) // сначала проходимся по стадиям
            {
                if (isMerging)
                {
                    if (firstPartia.OpersHere.Count - 1 < CurrentLevel || secondPartia.OpersHere.Count - 1 < CurrentLevel || !AbleToMergeLevel(firstPartia.OpersHere[CurrentLevel], secondPartia.OpersHere[CurrentLevel], CurrentLevel, CurrentLevel == 0, withHerself, withLog, finalResult))
                    {
                        if (CurrentLevel == 0) firstLevelBad = true;//merged = 0; // Если я даже на первом уровне не смог объединить что-то, то считаю, что вообще ничего не смог
                        isMerging = false;
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

            if (firstLevelBad) finalResult.mergedNorm = 0;

            // Теперь - группируем, чтобы для каждого повива/задела было лишь по одному запуску
            //if (What.OpersHere[0].Count > 0) merged = 0; // очевидно что он не схлопнулся до конца, и это плохо
            //float K = 0;
            float filledK = FindFilledK(firstPartia, secondPartia, withHerself, wannaMergeOps1, wannaMergeOps2, finalResult);

            // После всего: проходим по всем уровням, по всем РЦ, и смотрим - кому РЦ не задался, тому пытаемся задать с учётом отдающей тары, чтоб он в выходной таблице норм был.
            FinalConnectorChecks(firstPartia, secondPartia, withHerself, withLog, finalResult);

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

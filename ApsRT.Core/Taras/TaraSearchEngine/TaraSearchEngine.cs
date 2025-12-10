using ApsRT.Core.DataErrors;
using ApsRT.Core.Partias.MergeConditions;
using ApsRT.Core.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using UniversalPack.Extensions;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using ZLinq;

namespace ApsRT.Core.Taras.TaraSearchEngine
{
    public class TaraSearchEngine
    {

        public static HashSet<Uco> FindNeededNextRCs(IEnumerable<Stages.Stage> stages, Func<int, Uco?>? customSelector)
        {
            HashSet<Uco> result = new();

            foreach (Stages.Stage stage in stages)
            {
                checkOp(stage, true);
            }

            void checkOp(Stages.Stage stage, bool itsFirst)
            {
                if (!itsFirst)
                {
                    result.Add(customSelector?.Invoke(stage.Index) ?? stage.Center);
                }
                if (stage.NoChangeTara || itsFirst)
                {
                    foreach (var next in stage.Nexts)
                    {
                        checkOp(stage.SessionLink.MasterDataLinks.StagesMasterData.AllStages[next], false);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// "Метод поиска проверенного РЦ". Эта функция нужна в паре моментов, чтобы найти какой-нибудь РЦ на случай, если известна тара у предков (с 05.06 - известна приемная тара), а запустить полный алгоритм поиска по каким-то причинам затратно. Эта функция позволит найти какой-нибудь РЦ, среди отдающих тар которого есть все, что были назначены у предков + имеется нужная тара в списке приемной (если не пуста). На данный момент используется после объединения (когда для вывода в Result.csv приходилось устанавливать РЦ, даже если не нашлась тара) - чтобы цепочка приёмно-отдающей тары не нарушалась (например, hsr 630 поставил вместо 560-й, хотя у него нет 630-й в отдающей, а тару вообще не нашёл). Введён после замечания Алёны, 30.05. Дополнен 05.06, теперь позволяет отфильтровать РЦ ещё и по приемной таре, которая у него должна быть. Это пригодится для конечных тар - они назначены извне, а РЦ нужно подобрать самим. А ещё юзается в G, когда нужно получить список РЦ, на который влезет вся
        /// </summary>
        /// <param name="CurrentOpers">Текущий сдвиг / моделирование для анализа</param>
        /// <param name="IDToTara">словарь с тарами предков</param>
        /// <param name="thisRC">РЦ, который заполняется, как выходной параметр. Этот рц содержит в списке отдающей тары все нужные тары, и может подойти нам.</param>
        /// <param name="TaraNeedsToBeHere">Тары, которая должны у него быть в списке приёмной.</param>
        /// <returns>Ответ: нашёл рц или нет?</returns>
        public static bool FindSomeCentersBecauseOfPrevs(IAbleForTaraCalcInterface CurrentOpers, List<Uco> possibleCenters, Dictionary<int, Uco> IDToTara, List<Uco> TaraNeedsToBeHere, out Uco thisRC, out List<Uco> rcs, out List<Uco> otdTara, bool returnFirst)
        {
            thisRC = Uco.EmptyObject;

            var sessionLink = CurrentOpers.FirstOperLink.SessionLink;
            var ALL = sessionLink.MasterDataLinks.StagesMasterData.AllStages;
            var TaraDataSet = sessionLink.MasterDataLinks.TarasMasterData;

            var prevs = CurrentOpers.AllOpers.AsValueEnumerable().SelectMany(x => x.Prevs).Distinct(); // находим предков
            var possibleTara = prevs
                .Where(x => IDToTara.TryGetValue(x, out var tar) && !tar.IsEmpty) // выбираем тех, про кого есть инфа в словаре о предках
                .Select(x => (isVspom: ALL[x].UsesVspomTara, tara: IDToTara[x])) // выбираем тару из них
                .Distinct();
            possibleCenters ??= CurrentOpers.FirstOperLink.PossibleCenters ?? new List<Uco>() { CurrentOpers.FirstOperLink.OriginalCenter }; // Если этого списка не пришло, Формируем список рц, взяв его из первой попавшейся операции в этом сдвиге / моделировании

            // РЦ должен содержать ВСЕ тары в отдающих.

            //bool VspomTara = CurrentOpers.FirstOperLink.Prevs.Count > 0 && ALL[CurrentOpers.FirstOperLink.Prevs[0]].UsesVspomTara;

            if (returnFirst)
                rcs = possibleCenters;
            else
                rcs = new List<Uco>();
            otdTara = possibleTara.Select(x => x.tara).ToList();
            foreach (var center in possibleCenters.AsValueEnumerable().Where(x => TaraDataSet.FromAndToTara.ContainsKey(x)))
            {
                if (
                    (!possibleTara.Any() || possibleTara.All(tara => TaraDataSet.GetNeededTaras(center, CurrentOpers.FirstOperLink.OperationVid, tara.isVspom).otd.TryGetValue(tara.isVspom, out var has) && has.Contains(tara.tara))) // первое условие: все тары в списке предков есть (если эти предки вообще существуют, и есть тара для этого)
                    &&
                    (
                        TaraNeedsToBeHere == null || TaraNeedsToBeHere.Count == 0
                        ||
                        TaraDataSet.GetNeededTaras(center, CurrentOpers.FirstOperLink.OperationVid, null).priem.TryGetValue(false, out var has) && TaraNeedsToBeHere.All(x => x.IsEmpty || has.Contains(x))
                    ) // второе условие: тара, которую надо чекнуть, не пустая, и есть в списке приемных

                    )
                {
                    thisRC = center;
                    if (returnFirst)
                        return true; // Збс, нашёл, уходим
                    else
                        rcs.Add(center); // Если нужны все, то продолжаем запись
                }
            }
            return returnFirst ? false : rcs.Count > 0;
        }

        /// <summary>
        /// Получаем список РЦ и тары, которая может встать для них
        /// </summary>
        public static bool GetAllVariantsForThese(IAbleForTaraCalcInterface allOpers, out TemporaryTaraSearchData searchData, SumIsPossibleOptions options)
        {
            var SessionLink = allOpers.FirstOperLink.SessionLink;
            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            var TaraDataSet = SessionLink.MasterDataLinks.TarasMasterData;
            var dataErrors = SessionLink.DataErrors;
            var CentersData = SessionLink.MasterDataLinks.CentersMasterData.CentersData;
            bool LogErrorsBySettings = SessionLink.SettingsController.CurrentSettings.LogErrors;
            var ModToTaraInfoLink = options.ModToTaraInfoLink;


            //if (allOpers.ModId != -1 && (ModToTaraInfoLink?.TryGetValue(allOpers.ModId, out var resu) ?? false))
            //{
            //    searchData = resu;
            //    return resu != null;
            //}

            bool willVspom = allOpers.FirstOperLink.UsesVspomTara || allOpers.AllOpers.AsValueEnumerable().Any(x => x.Nexts.AsValueEnumerable().Any(y => ALL[y].UsesVspomTara && ALL[y].NoChangeTara)); // Признак "вспом. тару" должен быть у текущего, либо у ребенка (и у ребенка должен быть признак не менять тару)

            searchData = new()
            {
                PossibleCentersAndTaras = null,
                BARABANAS = null,
                errorMessage = options.WithLog ? new("\r\n") : null,
                sum = allOpers.Sum,
                opersFormat = allOpers.FirstOperLink.EdIzm,
                OpersCount = allOpers.AllOpers.Count,
                dataErrorMore = new(),
                WillVspom = willVspom
                //NotTaraCount = 0,
                //notChangeTara = false
            };

            searchData.dataErrorMore.RCsOnPrevEtap = allOpers.AllOpers.AsValueEnumerable().SelectMany(x => x.Prevs).SelectMany(Index =>
            {
                var op = ALL[Index];
                return op.PossibleCenters ?? new List<Uco> { op.OriginalCenter };
            }).Distinct().ToList();

            searchData.dataErrorMore.Count = searchData.sum;
            searchData.dataErrorMore.NaladLength = options.NaladLength;

            searchData.errorMessage?.Append($"Поиск тар и рц для: {string.Join("; ", allOpers.AllOpers)}\r\n");

            if (allOpers.FirstOperLink.NoCheckNamotka || allOpers.FirstOperLink.IsVspom)
            {
                searchData.errorMessage?.Append($"{allOpers.FirstOperLink}: не проверяет намотку - NoCheckNamotka={allOpers.FirstOperLink.NoCheckNamotka} | IsVspom={allOpers.FirstOperLink.IsVspom}. Сразу ухожу.\r\n");

                List<Uco> rcs = null;

                foreach (var element in allOpers.AllOpers)
                {
                    var res = element.PossibleCenters ?? new List<Uco>() { element.OriginalCenter };
                    rcs = (rcs == null) ? res : rcs.AsValueEnumerable().Intersect(res).ToList();
                }
                searchData.PossibleCentersAndTaras = rcs.AsValueEnumerable().Select(x => (RC: x, Tara: Uco.EmptyObject)).ToArray();
                return searchData.PossibleCentersAndTaras.Length > 0;
            }

            var doNotCheckWindings = allOpers.FirstOperLink.DoNotCheckWindings;// SessionLink.MasterDataLinks.MergeConditionsMasterData.DoNotCheckWindingsSafe(allOpers.FirstOperLink.OperationVid.Code, allOpers.FirstOperLink.Subdivision.Code);//allOpers.Select(e => e.oper.OperationVid.Code).First());
            var checkBarabanasCount = !doNotCheckWindings && !allOpers.FirstOperLink.NoChangeTara && allOpers.FirstOperLink.EndTara.IsEmpty;
            // Ищем тары для них всех

            var WantThisOrigTara = allOpers.FirstOperLink.EndTara;

            var ii = 0;
            foreach (var first in allOpers.AllOpers)
            {
                searchData.errorMessage?.Append($"Рассматриваемый этап {first} | [Продукция {first.Production}] [Номенклатура {first.Nomenklature}] [ТО {first.OperationVid}] [{first.OrderLink?.OrderID}]\r\n");

                (Uco RC, Uco Tara)[] res;

                List<Uco> rcs = new();

                // Это список тары, которая была у предков, а значит должна быть в
                // списке отдающей тары на том оборудовании, среди которого мы сейчас ищем результат
                HashSet<(Uco Tara, bool WasVspom)> TaraHereMustBeThis = null;
                if (options.PrevsInfo != null && first.Prevs.Count > 0)
                {
                    TaraHereMustBeThis = first.Prevs.AsValueEnumerable().Select(x => options.PrevsInfo.TryGetValue(x, out var taraMax) ? (taraMax, first.NoChangeTara ? false : ALL[x].UsesVspomTara) : (new(Guid.Empty, ""), false)).Distinct().Where(x => !x.Item1.IsEmpty).ToHashSet();
                    searchData.errorMessage?.Append($"Отд. тары у {first.Index}: {string.Join(", ", TaraHereMustBeThis)}\r\n");
                }


                if (TaraHereMustBeThis != null) searchData.dataErrorMore.NeededOtdTarasOnThisEtap = 
                    searchData.dataErrorMore.NeededOtdTarasOnThisEtap == null ? TaraHereMustBeThis/*.Select(x => x.Tara)*/.Distinct().ToList() :
                    searchData.dataErrorMore.NeededOtdTarasOnThisEtap.Union(TaraHereMustBeThis/*.Select(x => x.Tara)*/).ToList()
                    ;

                //bool VspomTara = first.Prevs.Count > 0 && ALL[first.Prevs[0]].UsesVspomTara;
                if (first.PossibleCenters == null)
                {
                    rcs.Add(first.OriginalCenter);
                    res = Array.Empty<(Uco RC, Uco Tara)>();
                    var res00 = TaraDataSet.FromAndToTara.ContainsKey(first.OriginalCenter)
                        && (!first.IsVspom || CentersData[first.OriginalCenter].AbleToVspom);
                    if (res00)
                    {
                        if (TaraHereMustBeThis == null || TaraHereMustBeThis.Count == 0 || TaraHereMustBeThis.All(x => TaraDataSet.GetNeededTaras(first.OriginalCenter, first.OperationVid, x.WasVspom)/*.FromAndToTara[first.Center]*/.otd.TryGetValue(x.WasVspom, out var has) && has.Contains(x.Tara)))
                        {
                            res = (TaraDataSet.GetNeededTaras(first.OriginalCenter, first.OperationVid, null).priem.TryGetValue(false, out var has) ? has.AsValueEnumerable().Select(x => (RC: first.OriginalCenter, Tara: x)).ToArray() : new (Uco, Uco)[] { });
                        }
                        else
                        {
                            if (LogErrorsBySettings && options.LogSomeErrors && (allOpers.AllOpers.Count == 1 || allOpers.FirstOperLink.MesModelling.Code != Guid.Empty))
                                dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoAllNeededOtdTaras, searchData.dataErrorMore);
                            return false;
                        }
                    }
                }
                else
                {
                    rcs.AddRange(first.PossibleCenters);
                    res = Array.Empty<(Uco RC, Uco Tara)>();
                    var res00 = first.PossibleCenters.AsValueEnumerable().Where(x => TaraDataSet.FromAndToTara.ContainsKey(x)
                    && (!first.IsVspom || CentersData[x].AbleToVspom));
                    if (res00.Count() > 0)
                    {
                        var res01 = res00.Where(x => TaraHereMustBeThis == null || TaraHereMustBeThis.Count == 0 || TaraHereMustBeThis.All(z => TaraDataSet.GetNeededTaras(x, first.OperationVid, z.WasVspom).otd.TryGetValue(z.WasVspom, out var has) && has.Contains(z.Tara)));
                        if (res01.Count() == 0)
                        {
                            if (LogErrorsBySettings && options.LogSomeErrors && (allOpers.AllOpers.Count == 1 || allOpers.FirstOperLink.MesModelling.Code != Guid.Empty))
                                dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoAllNeededOtdTaras, searchData.dataErrorMore);
                            return false;
                        }
                        else
                        {
                            res = res01.SelectMany(y => TaraDataSet.GetNeededTaras(y, first.OperationVid, null)/*.FromAndToTara[y]*/.priem.TryGetValue(false, out var has) ? has.Select(x => (RC: y, Tara: x)) : new (Uco, Uco)[] { }).ToArray();
                        }
                    }


                    //res = first.PossibleCenters.Where(x => TaraDataSet.FromAndToTara.ContainsKey(x)
                    //&& (!first.IsVspom || CentersData[x].AbleToVspom)
                    //    && (TaraHereMustBeThis == null || TaraHereMustBeThis.Count == 0 || TaraHereMustBeThis.All(z => TaraDataSet.GetNeededTaras(x, first.OperationVid, z.WasVspom).otd.TryGetValue(z.WasVspom, out var has) && has.Contains(z.Tara)))
                    //).SelectMany(y => TaraDataSet.GetNeededTaras(y, first.OperationVid, null)/*.FromAndToTara[y]*/.priem.TryGetValue(false, out var has) ? has.Select(x => (RC: y, Tara: x)) : new (Uco, Uco)[] { }).ToArray();
                }


                searchData.dataErrorMore.RCsOnThisEtap = 
                    searchData.dataErrorMore.RCsOnThisEtap == null ? rcs.ToList() :
                    searchData.dataErrorMore.RCsOnThisEtap.Union(rcs).ToList()
                    ;

                searchData.dataErrorMore.TarasOnThisEtap = 
                    searchData.dataErrorMore.TarasOnThisEtap == null ? res.Select(x => x.Tara).ToList() :
                    searchData.dataErrorMore.TarasOnThisEtap.Union(res.Select(x => x.Tara)).ToList()
                    ;

                // Проверим конечную тару
                if (!WantThisOrigTara.IsEmpty && !res.AsValueEnumerable().Any(X => X.Tara == WantThisOrigTara))
                {
                    // Тут нет ни единой тары, которая подойдёт как конечная
                    if (LogErrorsBySettings)
                    {
                        var dem = searchData.dataErrorMore;
                        dem.DataToInsert = WantThisOrigTara.Label;
                        dataErrors.AddNewPartiaError(allOpers.FirstOperLink.Index, NewDataErrorType.NoPriemOrigTara, dem); // не нашёл нужную ПРИЕМНУЮ конечную тару
                    }
                    return false;
                }

                if (res.Length == 0) // В результате пересечения нет ни одного РЦ
                {
                    //if (withUI) This.MergeTextBox.Text += $"Нет пересечения РЦ на этапе {ii}.\r\n";
                    searchData.errorMessage?.Insert(0, $"ОШИБКА! У этапа {first} нет ни одного РЦ, у которого есть тара.");
                    searchData.errorMessage?.Append($"\r\n");
                    if (options.WithLog)
                    {
                        SessionLink.AssignedLogger.PartiasLogQueue.Enqueue(searchData.errorMessage.ToString());
                    }

                    if (allOpers.ModId != -1)
                    {
                        ModToTaraInfoLink?.TryAdd(allOpers.ModId, null);
                    }

                    //if (first.PossibleCenters == null)
                    //{
                    //    if (!TaraDataSet.FromAndToTara.ContainsKey(first.Center))
                    //        dataErrors.AddPartiaError(allOpers.FirstOperLink.Index, PartiaErrorLogType.NoRCTara, TaraHereMustBeThis);
                    //}
                    //else
                    //{
                    //    if (!first.PossibleCenters.Where(x => TaraDataSet.FromAndToTara.ContainsKey(x)).Any())
                    //        dataErrors.AddPartiaError(allOpers.FirstOperLink.Index, PartiaErrorLogType.NoRCTara, TaraHereMustBeThis);
                    //}


                    return false;
                }

                if (searchData.errorMessage != null)
                {
                    searchData.errorMessage.Append("Доступные РЦ для этапа:\r\n");
                    foreach (var rc in rcs)
                    {
                        searchData.errorMessage.Append(rc + "\r\n");
                    }
                }

                // Набираем все возможные намотки для этой операции, если это нужно
                var res0 = checkBarabanasCount ? TaraDataSet.GetOperTara(first, options.WithLog) : null;

                searchData.errorMessage?.Append("Тары для такой продукции+номенклатуры+тех.операции:\r\n");
                // Пересекаем с теми, что уже есть
                if (res0?.Count > 0)
                {
                    if (searchData.BARABANAS == null)
                    {
                        searchData.BARABANAS = res0.ToArray();
                    }
                    else
                    {
                        var set1 =/* new HashSet<Uco>(*/res0.AsValueEnumerable().Select(x => x.Tara).ToHashSet();
                        searchData.BARABANAS = searchData.BARABANAS.AsValueEnumerable().Where(x => set1.Contains(x.Tara)).ToArray();//.Add(poss2);
                    }

                    searchData.dataErrorMore.MaxNamotka = searchData.BARABANAS.AsValueEnumerable().Select(x => (x.Tara, x.Namot)).ToList();

                    //if (withUI) This.MergeTextBox.Text += $"Намотки для {first.oper}: {string.Join(", ", poss2.Select(x => x.Tara))}\r\n";
                    searchData.errorMessage?.Append(string.Join("\r\n", res0.Select(x => $"{x.Tara} ({x.Namot} {x.Format.DimensionUnitToString()})")) + "\r\n");

                    if (/*!first.IsVspom && */searchData.BARABANAS.Length == 0 && checkBarabanasCount)
                    {
                        //if (withUI) This.MergeTextBox.Text += $"Нет пересечения на этапе {ii}.\r\n";
                        searchData.errorMessage?.Insert(0, $"ОШИБКА! Нет пересечения намоток из-за операции {first}.");
                        searchData.errorMessage?.Append($"\r\n");
                        if (options.WithLog)
                        {
                            SessionLink.AssignedLogger.PartiasLogQueue.Enqueue(searchData.errorMessage.ToString());
                        }

                        if (allOpers.ModId != -1)
                        {
                            ModToTaraInfoLink?.TryAdd(allOpers.ModId, null);
                        }

                        return false;
                    }
                }
                else
                {
                    if (checkBarabanasCount)
                    {
                        searchData.errorMessage?.Insert(0, $"ОШИБКА! У операции {first} нет намоток вообще.");
                        searchData.errorMessage?.Append($"\r\n");
                        if (options.WithLog)
                        {
                            SessionLink.AssignedLogger.PartiasLogQueue.Enqueue(searchData.errorMessage.ToString());
                        }

                        if (allOpers.ModId != -1)
                        {
                            ModToTaraInfoLink?.TryAdd(allOpers.ModId, null);
                        }


                        //if (!first.IsVspom && LogErrorsBySettings)
                        //{
                        //    dataErrors.AddPartiaError(first.Index, PartiaErrorLogType.NoMaxNamotka, (first.Production, first.Nomenklature, first.OperationVid));
                        //}

                        return false;
                    }
                }

                // Смотрим на РЦ
                if (searchData.errorMessage != null)
                {
                    var rcsG = res.GroupBy(x => x.RC);
                    searchData.errorMessage.Append("РЦ этапа и их возможные тары:\r\n");
                    foreach (var grp in rcsG)
                    {
                        searchData.errorMessage.Append(grp.Key + " (" + string.Join(", ", grp.Select(x => x.Tara)) + ")\r\n");
                    }
                }

                if (searchData.PossibleCentersAndTaras == null)
                {
                    searchData.PossibleCentersAndTaras = res;
                }
                else
                {
                    searchData.PossibleCentersAndTaras = searchData.PossibleCentersAndTaras.AsValueEnumerable().Intersect(res/*, new CodedObjectTaraEqualityComparer()*/).ToArray();
                }

                if (searchData.PossibleCentersAndTaras.Length == 0)
                {
                    //if (withUI) This.MergeTextBox.Text += $"Нет пересечения РЦ на этапе {ii}.\r\n";
                    searchData.errorMessage?.Insert(0, $"ОШИБКА! Нет пересечения РЦ из-за операции {first}.");
                    searchData.errorMessage?.Append($"\r\n");
                    if (options.WithLog)
                    {
                        SessionLink.AssignedLogger.PartiasLogQueue.Enqueue(searchData.errorMessage.ToString());
                    }

                    if (allOpers.ModId != -1)
                    {
                        ModToTaraInfoLink?.TryAdd(allOpers.ModId, null);
                    }

                    //AddPartiaError(allOpers.FirstOperLink.Index, PartiaErrorLogType.No_RC_Intersections);

                    return false;
                }

                ii++;
            }


            // Теперь нужно ещё и пересечение с детьми, если конечно надо
            if (true/*CheckChildren*/)
            {
                // У самых первых детей есть булевый признак, который врубает проверку на РЦ. Если там дальше будет "не менять тару", этот признак позволяет отключить лишние проверки.
                var childs = allOpers.AllOpers.AsValueEnumerable().SelectMany(x => x.Nexts).Distinct().Select(y => (op: ALL[y], isFirst: true))
                    //.Where(x => !x.op.IsLast)
                    .ToList();


                for (var i = 0; i < childs.Count; i++)
                {
                    var first = childs[i];


                    List<(Uco RC, Uco Tara)> foundOtdTara;

                    searchData.errorMessage?.Append($"Доступные РЦ следующего этапа ({first}):\r\n");
                    if (first.op.PossibleCenters == null)
                    {
                        searchData.errorMessage?.Append(first.op.OriginalCenter + "\r\n");
                        // Сначала проверим момент "НеМенятьТару"
                        if (first.op.NoChangeTara)
                        {

                            // Проверим, чтоб только те тары, что есть у детей с признаком "не менять тару"
                            Uco? alreadyWasThisTara = null;

                            // Во-первых, добавляем ещё детей
                            childs.AddRange(first.op.Nexts.AsValueEnumerable().Where(x => ALL.ContainsKey(x)).Select(x => (op: ALL[x], isFirst: false)).ToArray());

                            // Во вторых, если есть конечная тара, то прочекать, чтоб была в списке приемных

                            if (!childs[i].op.EndTara.EmptyCheck())
                            {
                                var thisTaraNeededToBe = childs[i].op.EndTara;
                                if (alreadyWasThisTara.HasValue && alreadyWasThisTara.Value != thisTaraNeededToBe && options.LogSomeErrors)
                                {
                                    // Ошибка
                                    var dem = searchData.dataErrorMore;
                                    dem.DataToInsert = childs[i].op.EndTara.Label;
                                    dataErrors.AddNewPartiaError(childs[i].op.Index, NewDataErrorType.NoNeededNoChangeFinalTaraOnChildMoreThanOne, dem);
                                }
                                else
                                {
                                    alreadyWasThisTara = thisTaraNeededToBe;
                                }
                                searchData.PossibleCentersAndTaras = searchData.PossibleCentersAndTaras.AsValueEnumerable().Where(x => x.Tara == thisTaraNeededToBe).ToArray();
                                if (searchData.PossibleCentersAndTaras.Length == 0)
                                {
                                    // Ошибка
                                    if (options.LogSomeErrors)
                                    {
                                        var dem = searchData.dataErrorMore;
                                        dem.DataToInsert = childs[i].op.EndTara.Label;
                                        dataErrors.AddNewPartiaError(childs[i].op.Index, NewDataErrorType.NoNeededNoChangeFinalTaraOnChild, dem);
                                    }
                                    return false;
                                }

                            }


                            if (!first.isFirst) // если это первый раз, то не проходим мимо, а отдающую тару чекаем
                            {
                                continue;
                            }
                        }

                        // А вот теперь все остальное
                        if (TaraDataSet.FromAndToTara.ContainsKey(first.op.OriginalCenter) //&& (!first.IsVspom || CentersData[first.Center].NotChangeTara)
                            && (options.NextRCsMustBeThese == null || /*!first.isFirst ||*/ first.op.NoChangeTara && options.NextRCsExcludeNoChangeTara || options.NextRCsMustBeThese.Contains(first.op.OriginalCenter)) // Если есть ограничение по РЦ детей, то нужно чекнуть, входит ли он сюда (это для G используется)
                                                                                                                                                                                                                   //&& first.op.Prevs.All(x => !PrevsInfo.TryGetValue(x, out var prevTara) || )
                            )
                        {

                            // чекнем конечную тару
                            var hasFinalTara = first.op.EndTara.IsEmpty || TaraDataSet.GetNeededTaras(first.op.OriginalCenter, first.op.OperationVid, null)/*.FromAndToTara[x]*/.priem.TryGetValue(false, out var taras) && taras.Contains(first.op.EndTara); // у него должна спец приемная тара в случае чего

                            if (hasFinalTara)
                            {
                                bool FirVsp = first.isFirst && first.op.NoChangeTara;

                                HashSet<(Uco Tara, bool WasVspom)> localTaraHereMustBeThis = options.PrevsInfo == null ? null : first.op.Prevs.AsValueEnumerable().Where(x => !ALL[x].IsVspom && !ALL[x].NoCheckNamotka && options.PrevsInfo.ContainsKey(x) && allOpers.AllOpers.All(op => op.Index != x)).Select(x => (options.PrevsInfo[x], ALL[x].UsesVspomTara)).ToHashSet();

                                var res01 = localTaraHereMustBeThis == null || localTaraHereMustBeThis.Count == 0 || localTaraHereMustBeThis.AsValueEnumerable().All(z => TaraDataSet.GetNeededTaras(first.op.OriginalCenter, first.op.OperationVid, FirVsp ? null : z.WasVspom).otd.TryGetValue(FirVsp ? false : z.WasVspom, out var has) && has.Contains(z.Tara));

                                if (res01)
                                {
                                    foundOtdTara = TaraDataSet.GetNeededTaras(first.op.OriginalCenter, first.op.OperationVid, FirVsp ? null : willVspom)/*.FromAndToTara[first.op.Center]*/.otd
                                        .TryGetValue(FirVsp ? false : willVspom, out var has) // вот как у этапа выше вспом. тара назначена - вот здесь она себя и проявляет
                                                                                              //&&
                                                                                              //first.op.Prevs.All(x => PrevsInfo == null || !PrevsInfo.TryGetValue(x, out var t) || has.Contains(t)) // Нужно ещё чекнуть, чтоб все тары у предыдущих этапов были тут (полная проверка)
                                        ? has.AsValueEnumerable().Select(x => (RC: first.op.OriginalCenter, Tara: x)).ToList()
                                        : new (Uco, Uco)[] { }.ToList();
                                }
                                else
                                {
                                    if (options.LogSomeErrors)
                                    {
                                        var dem = searchData.dataErrorMore;
                                        dem.DataToInsert = first.op.EndTara.Label;
                                        dataErrors.AddNewPartiaError(first.op.Index, NewDataErrorType.NoAllNeededOtdTarasWithCheck, dem);
                                    }
                                    foundOtdTara = new();
                                }
                            }
                            else
                            {
                                var dem = searchData.dataErrorMore;
                                dem.DataToInsert = first.op.EndTara.Label;

                                var rcsToIns = first.op.PossibleCenters ?? new() { first.op.OriginalCenter };
                                dem.RCsOnNextEtap = 
                                    dem.RCsOnNextEtap == null ? rcsToIns.ToList() :
                                    dem.RCsOnNextEtap.Union(rcsToIns).Distinct().ToList()
                                    ;

                                dataErrors.AddNewPartiaError(first.op.Index, NewDataErrorType.NoPriemOrigTaraOnNextEtap, dem); // не нашёл нужную ПРИЕМНУЮ конечную тару
                                foundOtdTara = new();
                            }
                        }
                        else
                        {
                            foundOtdTara = new();
                        }
                    }
                    else
                    {
                        if (searchData.errorMessage != null)
                        {
                            foreach (var rc in first.op.PossibleCenters)
                            {
                                searchData.errorMessage.Append(rc + "\r\n");
                            }
                        }
                        // Сначала проверим момент "НеМенятьТару" - если она есть,
                        if (first.op.NoChangeTara/*PossibleCenters.All(x => CentersData[x].NotChangeTara)*/)
                        {
                            childs.AddRange(first.op.Nexts.AsValueEnumerable().Where(x => ALL.ContainsKey(x)).Select(x => (op: ALL[x], isFirst: false)).ToArray());
                            if (!first.isFirst) // если это первый раз, то не проходим мимо, а отдающую тару чекаем
                            {
                                continue;
                            }
                        }

                        // А вот теперь все остальное
                        bool FirVsp = first.isFirst && first.op.NoChangeTara;

                        var rcs1 = first.op.PossibleCenters.AsValueEnumerable().Where(x => TaraDataSet.FromAndToTara.ContainsKey(x)
                        && (first.op.EndTara.IsEmpty || TaraDataSet.GetNeededTaras(x, first.op.OperationVid, null)/*.FromAndToTara[x]*/.priem.TryGetValue(false, out var taras) && taras.Contains(first.op.EndTara))); // у него должна спец приемная тара в случае чего
                        //);



                        if (rcs1.Count() > 0)
                        {

                            HashSet<(Uco Tara, bool WasVspom)> localTaraHereMustBeThis = options.PrevsInfo == null ? null : first.op.Prevs.AsValueEnumerable().Where(x => !ALL[x].IsVspom && !ALL[x].NoCheckNamotka && options.PrevsInfo.ContainsKey(x) && allOpers.AllOpers.All(op => op.Index != x)).Select(x => (options.PrevsInfo[x], ALL[x].UsesVspomTara)).ToHashSet();

                            var res01 = rcs1.Where(x => localTaraHereMustBeThis == null || localTaraHereMustBeThis.Count == 0 || localTaraHereMustBeThis.AsValueEnumerable().All(z => TaraDataSet.GetNeededTaras(x, first.op.OperationVid, FirVsp ? null : z.WasVspom).otd.TryGetValue(FirVsp ? false : z.WasVspom, out var has) && has.Contains(z.Tara)));

                            if (res01.Count() > 0)
                            {
                                foundOtdTara = res01.Where(x =>
                                 //&& (!first.IsVspom || CentersData[x].AbleToVspom)
                                 options.NextRCsMustBeThese == null || /*!first.isFirst ||*/ first.op.NoChangeTara && options.NextRCsExcludeNoChangeTara || options.NextRCsMustBeThese.Contains(x) // Если есть ограничение по РЦ детей, то нужно чекнуть, входит ли он сюда
                                ).SelectMany(y => TaraDataSet.GetNeededTaras(y, first.op.OperationVid, FirVsp ? null : willVspom)/*.FromAndToTara[y]*/.otd.TryGetValue(FirVsp ? false : willVspom, out var has)
                                    //&&
                                    //        first.op.Prevs.All(x => PrevsInfo == null || !PrevsInfo.TryGetValue(x, out var t) || has.Contains(t)) // Нужно ещё чекнуть, чтоб все тары у предыдущих этапов были тут (полная проверка)
                                    ? has.Select(x => (RC: y, Tara: x)) : new (Uco, Uco)[] { }).ToList();
                            }
                            else
                            {
                                if (options.LogSomeErrors)
                                {
                                    var dem = searchData.dataErrorMore;
                                    dem.DataToInsert = first.op.EndTara.Label;
                                    dataErrors.AddNewPartiaError(first.op.Index, NewDataErrorType.NoAllNeededOtdTarasWithCheck, dem);
                                }
                                foundOtdTara = new();
                            }

                        }
                        else
                        {
                            var dem = searchData.dataErrorMore;
                            dem.DataToInsert = first.op.EndTara.Label;

                            var rcsToIns = first.op.PossibleCenters ?? new() { first.op.OriginalCenter };
                            dem.RCsOnNextEtap = 
                                dem.RCsOnNextEtap == null ? rcsToIns.ToList() :
                                dem.RCsOnNextEtap.Union(rcsToIns).Distinct().ToList()
                                ;

                            dataErrors.AddNewPartiaError(first.op.Index, NewDataErrorType.NoPriemOrigTaraOnNextEtap, dem); // не нашёл нужную ПРИЕМНУЮ конечную тару
                            foundOtdTara = new();
                        }
                    }

                    if (searchData.errorMessage != null)
                    {
                        var rcsG = foundOtdTara.GroupBy(x => x.RC);
                        searchData.errorMessage.Append($"РЦ следующего этапа ({first}) и его доступные тары:\r\n");
                        foreach (var grp in rcsG)
                        {
                            searchData.errorMessage.Append(grp.Key + " (" + string.Join(", ", grp.Select(x => x.Tara)) + ")\r\n");
                        }
                    }

                    //if (withUI) This.MergeTextBox.Text += $"РЦ для ребенка - {first}: {string.Join(", ", res.Select(x => $"{Centers.CentersIntToStr[x.RC]} ({x.Tara})"))}\r\n";

                    //if (allOpers.FirstOperLink.Index == 5671)
                    //{
                    //    ;
                    //}

                    searchData.dataErrorMore.TarasOnNextEtap = 
                        searchData.dataErrorMore.TarasOnNextEtap == null ? foundOtdTara.Select(x => x.Tara).Distinct().ToList() :
                        searchData.dataErrorMore.TarasOnNextEtap.Union(foundOtdTara.Select(x => x.Tara)).Distinct().ToList()
                        ;

                    var rcs = first.op.PossibleCenters ?? new() { first.op.OriginalCenter };
                    searchData.dataErrorMore.RCsOnNextEtap = 
                        searchData.dataErrorMore.RCsOnNextEtap == null ? rcs.ToList() :
                        searchData.dataErrorMore.RCsOnNextEtap.Union(rcs).Distinct().ToList()
                        ;

                    // Если есть отдающая тара, проверим её
                    if (!WantThisOrigTara.IsEmpty && !foundOtdTara.Any(X => X.Tara == WantThisOrigTara))
                    {
                        // Тут нет ни единой тары, которая подойдёт как конечная
                        if (LogErrorsBySettings)
                        {
                            var dem = searchData.dataErrorMore;
                            dem.DataToInsert = WantThisOrigTara.Label;
                            dataErrors.AddNewPartiaError(
                                //first.op.Index
                                allOpers.FirstOperLink.Index
                                , NewDataErrorType.NoOtdOrigTara, dem); // не нашёл нужную ОТДАЮЩУЮ конечную тару
                        }
                        return false;
                    }

                    var set2 = foundOtdTara.AsValueEnumerable().Select(x => x.Tara).ToHashSet();
                    var setRC = searchData.PossibleCentersAndTaras.AsValueEnumerable().Select(x => x.RC).ToHashSet();
                    searchData.PossibleCentersAndTaras = searchData.PossibleCentersAndTaras.AsValueEnumerable().Where(x => set2.Contains(x.Tara)).ToArray();//.Add(poss2);

                    //PossibleCentersAndTaras = PossibleCentersAndTaras.IntersectBy(res.Select(x => x.Tara), x => x.Tara).ToList();
                    if (searchData.PossibleCentersAndTaras.Length == 0)
                    {
                        if (LogErrorsBySettings && (allOpers.AllOpers.Count == 1 || allOpers.FirstOperLink.MesModelling.Code != Guid.Empty) && options.NextRCsMustBeThese == null) // это пусть сработает только для начала
                        {
                            if (allOpers.FirstOperLink.NoChangeTara)
                            {
                                dataErrors.AddNewPartiaError(allOpers.FirstOperLink.Index, NewDataErrorType.NoPriemNOtdIntersectionNoChange, searchData.dataErrorMore);
                            }
                            else
                            {
                                if (willVspom)
                                    dataErrors.AddNewPartiaError(allOpers.FirstOperLink.Index, NewDataErrorType.NoPriemNOtdIntersectionVspom, searchData.dataErrorMore);
                                else
                                    dataErrors.AddNewPartiaError(allOpers.FirstOperLink.Index, NewDataErrorType.NoPriemNOtdIntersection, searchData.dataErrorMore);
                            }

                        }

                        //if (withUI) This.MergeTextBox.Text += $"Нет пересечения РЦ у детей на этапе {i}.\r\n";
                        searchData.errorMessage?.Insert(0, $"ОШИБКА! Нет пересечения РЦ из-за следующего этапа ({first}).");
                        searchData.errorMessage?.Append($"\r\n");
                        if (options.WithLog)
                        {
                            SessionLink.AssignedLogger.PartiasLogQueue.Enqueue(searchData.errorMessage.ToString());
                        }

                        if (allOpers.ModId != -1)
                        {
                            ModToTaraInfoLink?.TryAdd(allOpers.ModId, null);
                        }

                        return false;
                    }
                }
            }


            var set = searchData.PossibleCentersAndTaras.AsValueEnumerable().Select(x => x.Tara).ToHashSet();
            if (searchData.BARABANAS != null) searchData.BARABANAS = searchData.BARABANAS.AsValueEnumerable().Where(x => set.Contains(x.Tara)).ToArray();//.Add(poss2);

            if (searchData.BARABANAS != null) searchData.dataErrorMore.MaxPlusUsedTaras = searchData.BARABANAS.AsValueEnumerable().Select(x => x.Tara).Distinct().ToList();
            //BARABANAS = BARABANAS.IntersectBy(PossibleCentersAndTaras.Select(x => x.Tara), x => x.Tara).ToList();// в конце концов пересекаем.


            if (checkBarabanasCount && (searchData.BARABANAS == null || searchData.BARABANAS.Length == 0))
            {
                if (LogErrorsBySettings && allOpers.AllOpers.Count == 1 /*|| allOpers.FirstOperLink.Modelling != Guid.Empty*/ && options.NextRCsMustBeThese == null)
                {
                    //AddPartiaError(allOpers.FirstOperLink.Index, PartiaErrorLogType.No_RC_Tara_Intersect);
                    dataErrors.AddNewPartiaError(allOpers.FirstOperLink.Index, willVspom ? NewDataErrorType.NoTaraNMaxIntersectionVspom : NewDataErrorType.NoTaraNMaxIntersection, searchData.dataErrorMore);
                }

                //if (withUI) This.MergeTextBox.Text += $"Намоток 0 по итогу\r\n";
                searchData.errorMessage?.Insert(0, $"ОШИБКА! Нет пересечения списка тар максимальных намоток и списка применяемых тар на РЦ.");
                searchData.errorMessage?.Append($"\r\n");
                if (options.WithLog)
                {
                    SessionLink.AssignedLogger.PartiasLogQueue.Enqueue(searchData.errorMessage.ToString());
                }
                if (allOpers.ModId != -1)
                {
                    ModToTaraInfoLink?.TryAdd(allOpers.ModId, null);
                }

                return false;
            }

            if (allOpers.ModId != -1)
            {
                // 26.06: эта штука убрана, чтобы V могла работать с учётом выбранных рц и тар (так как их набор меняется в V постоянно из-за выбора тары там).
                // Если раскомментировать эту строчку, результат может не учесть хронологию/цепочку тар.
                //ModToTaraInfoLink?.TryAdd(allOpers.ModId, searchData);
            }

            return true;
        }
        
        public static bool FilterAllVariants(IAbleForTaraCalcInterface someOpers, TemporaryTaraSearchData searchData,
            out TaraSearchEngineResult result,
            SumIsPossibleOptions options)
        {

            var SessionLink = someOpers.FirstOperLink.SessionLink;
            var CentersDataSet = SessionLink.MasterDataLinks.CentersMasterData;
            var dataErrors = SessionLink.DataErrors;
            bool LogErrorsBySettings = SessionLink.SettingsController.CurrentSettings.LogErrors;

            result = new TaraSearchEngineResult();
            result.TaraName = Uco.EmptyObject;
            result.FinalRC = Uco.EmptyObject;
            result.used_format = DimensionUnit.Unknown;
            result.Max = options.SearchForMaxTara ? -1 : double.MaxValue;
            result.notChangeTara = false;
            var errorMessage = searchData.errorMessage;
            var opersFormat = searchData.opersFormat;

            if (options.SaveAvailableResults) searchData.AvailableResults = new double[searchData.PossibleCentersAndTaras.Length]; // Пока тут всё 0

            // Если - исключения из номенклатуры
            if (someOpers.FirstOperLink.NoCheckNamotka || someOpers.FirstOperLink.IsVspom)
            {
                searchData.errorMessage?.Append($"{someOpers.FirstOperLink}: не проверяет намотку. Сразу проверяю.\r\n");
                result.Max = 0;
                result.used_format = DimensionUnit.Unknown;
                if (options.RCMustBeThis == null || options.RCMustBeThis.Count == 0)
                {
                    var center = searchData.PossibleCentersAndTaras[0];
                    result.TaraName = center.Tara;
                    result.FinalRC = center.RC;
                }
                else
                {
                    var center = searchData.PossibleCentersAndTaras.Where(pair => options.RCMustBeThis.Contains(pair.RC)).FirstOrDefault();
                    if (center.RC.EmptyCheck()) return false;
                    result.TaraName = center.Tara;
                    result.FinalRC = center.RC;
                }
                return true;
            }

            // Посмотрим на "менять тару"

            Uco NotChangeTaraMustBeThis = Uco.EmptyObject;
            if (options.PrevsInfo != null)
            {
                foreach (var op in someOpers.AllOpers)
                {
                    if (!op.NoChangeTara)
                    {
                        continue; // Если "НеМенятьТару" нету, то пропускаем
                    }

                    result.notChangeTara = true;

                    if (op.Prevs.Count > 0)
                    {
                        foreach (var prev in op.Prevs)
                        {
                            if (options.PrevsInfo.TryGetValue(prev, out var tara)) // Берём тару у предка, если есть
                            {
                                if (NotChangeTaraMustBeThis.IsEmpty)
                                {
                                    // Всё ок, назначаем тару
                                    NotChangeTaraMustBeThis = tara;
                                }
                                else
                                {
                                    if (NotChangeTaraMustBeThis != tara)
                                    {
                                        // Не ок. Если тара уже назначена другой операцией, и они отличаются, значит ограничение нарушено:
                                        // две операции, обе требуют "не менять тару", но их предки имеют другую тару. Выходим отсюда.
                                        errorMessage?.Append($"НеМенятьТару: предлагается {tara} для {op}, но уже ранее другой ограниченный этап наложил ограничение в виде {NotChangeTaraMustBeThis}.\r\n\r\n");
                                        if (options.WithLog)
                                        {
                                            SessionLink.AssignedLogger.PartiasLogQueue.Enqueue(errorMessage.ToString());
                                        }
                                        if (LogErrorsBySettings && options.LogSomeErrors) dataErrors.AddNewPartiaError(op.Index, NewDataErrorType.NoChangeTaraMoreThanOne, searchData.dataErrorMore);
                                        return false;
                                    }
                                }
                            }
                        }
                    }

                }
            }

            var doNotCheckWindings = someOpers.FirstOperLink.DoNotCheckWindings;// SessionLink.MasterDataLinks.MergeConditionsMasterData.DoNotCheckWindingsSafe(someOpers.FirstOperLink.OperationVid.Code, someOpers.FirstOperLink.Subdivision.Code);// || TaraDataSet.NotCheckNamotka(someOpers.FirstOperLink);

            //bool recommendedRCSet = false;

            var origTara = someOpers.FirstOperLink.EndTara;

            if (options.RecommendedSubdivision.HasValue) // Если определено подразделение, то можно сначала отсортировать по нему.
            {
                searchData.PossibleCentersAndTaras = searchData.PossibleCentersAndTaras.OrderByDescending(x => CentersDataSet.CentersData[x.RC].Subdivision == options.RecommendedSubdivision.Value ? 1 : 0).ToArray();
            }

            int currentPlacesCountK = 1;
            var sum = (someOpers.Sum / currentPlacesCountK) + options.NaladLength;
            int NoChangeTaraNonErrorsCount = 0;
            int OrigTaraNonErrorsCount = 0;
            while (currentPlacesCountK < 10) // Попытаемся изменить количество мест, и уменьшить количество
            {
                sum = (someOpers.Sum / currentPlacesCountK) + options.NaladLength;
                NoChangeTaraNonErrorsCount = 0;
                OrigTaraNonErrorsCount = 0;
                for (int h = 0; h < searchData.PossibleCentersAndTaras.Length; h++)
                {
                    if (options.SaveAvailableResults) searchData.AvailableResults[h] = -1; // Пока недоступно.

                    var center = searchData.PossibleCentersAndTaras[h];

                    if (!NotChangeTaraMustBeThis.IsEmpty && center.Tara != NotChangeTaraMustBeThis)
                    {
                        continue; // на случай "не менять тару"
                    }
                    NoChangeTaraNonErrorsCount++; // Если алгоритм хоть раз зашёл сюда, значит дело не в том, что нет ни единого барабана нормального

                    if (!origTara.IsEmpty && center.Tara != origTara)
                    {
                        continue; // две проверки конечной тары
                    }
                    OrigTaraNonErrorsCount++;


                    if (options.RCMustBeThis != null && !options.RCMustBeThis.Contains(center.RC)/*  != options.RCMustBeThis.Value*/)
                    {
                        continue; // две проверки, если надо
                    }

                    if (options.TaraMustBeThis != null && !options.TaraMustBeThis.Contains(center.Tara))
                    {
                        continue;
                    }


                    // Тара в списке тех, кого выбирать нельзя?
                    if (options.ExceptTheseTaras != null && options.ExceptTheseTaras.Contains(center.Tara))
                    {
                        continue;
                    }

                    if (options.RecommendedSubdivision.HasValue // Есть ограничение?
                        && !result.FinalRC.IsEmpty // Я уже назначил РЦ?
                        && CentersDataSet.CentersData[result.FinalRC].Subdivision == options.RecommendedSubdivision.Value // Этот рц из нужного мне подразделения?
                        && CentersDataSet.CentersData[center.RC].Subdivision != options.RecommendedSubdivision.Value) // Текущий РЦ уже из другого подразделения?
                    {
                        break; // Значит искать больше смысла нет, я нашел что-то из рекомендуемого цеха.
                    }

                    if (result.notChangeTara || doNotCheckWindings || searchData.BARABANAS == null || !origTara.IsEmpty) // Не менять тару? Не проверять макс намотки?
                    {
                        if (!result.FinalRC.IsEmpty
                            && someOpers.FirstOperLink.RecommendedRC.HasValue
                            && someOpers.FirstOperLink.RecommendedRC.Value == result.FinalRC) continue;
                        result.TaraName = center.Tara;
                        result.Max = 0;
                        result.FinalRC = center.RC;
                        result.used_format = DimensionUnit.Unknown;
                        if (options.SaveAvailableResults) searchData.AvailableResults[h] = 0;
                    }
                    else
                    {
                        for (int hh = 0; hh < searchData.BARABANAS.Length; hh++)
                        {
                            var namot = searchData.BARABANAS[hh];
                            if (center.Tara != namot.Tara) // Тара не совпадает тарой на центре?
                            {
                                continue; // Отбраковываем
                            }

                            bool technicallyPossible = (namot.Namot >= sum
                                || someOpers.ZadelCode != 0); // Не меньше суммы должна быть намотка, ну либо это что с ЗАДЕЛАМИ, поэтому тут проверять на макс намотку не следует (Алёна, 17.06);

                            if (options.SaveAvailableResults && technicallyPossible) searchData.AvailableResults[h] = namot.Namot;

                            if (technicallyPossible

                                &&
                                (
                                    (!options.SearchForMaxTara && namot.Namot < result.Max) // Должен быть меньше уже предложенного
                                    || (options.SearchForMaxTara && namot.Namot > result.Max)
                                    || (options.RecommendedTara.HasValue && /*!TaraName.IsEmpty &&*/ options.RecommendedTara.Value == namot.Tara)
                                )
                                )
                            {
                                if (!result.TaraName.IsEmpty
                                    && options.RecommendedTara.HasValue
                                    && options.RecommendedTara.Value == result.TaraName)
                                {
                                    continue;
                                }

                                if (!result.FinalRC.IsEmpty
                                    && someOpers.FirstOperLink.RecommendedRC.HasValue
                                    && someOpers.FirstOperLink.RecommendedRC.Value == result.FinalRC)
                                {
                                    continue;
                                }

                                result.TaraName = center.Tara;
                                result.Max = namot.Namot;
                                result.FinalRC = center.RC;
                                result.used_format = namot.Format;
                            }
                        }
                    }
                }

                if (result.FinalRC.IsEmpty)
                {
                    // не найден, потому что ни одна не подошла из-за "не менять тару"
                    if (NoChangeTaraNonErrorsCount == 0)
                        break;

                    // не найден РЦ с подходящей приемной тарой
                    if (OrigTaraNonErrorsCount == 0)
                        break;

                    if (someOpers.FirstOperLink.MesModelling.Code != Guid.Empty) // Заблоченные моды я тоже не имею права ставить
                        break;

                    if (someOpers.AllOpers.Count > 2) // Больше двух тоже нельзя, иначе я на бесконечное количество итераций зависну
                        break;

                    if (!someOpers.FirstOperLink.CanChangePlacesCount) // Я не могу менять кол-во мест? - выходим
                        break;

                    currentPlacesCountK++;
                }
                else
                {
                    // Нашёл, выходим
                    break;
                }
            }

            if (currentPlacesCountK > 1) result.CurrentPlacesCountK = currentPlacesCountK;

            if (!result.FinalRC.IsEmpty)
            {
                errorMessage?.Append($"Сумма: {sum} {currentPlacesCountK} {opersFormat.DimensionUnitToString()}, выбрал {result.TaraName} ({result.Max} {result.used_format}), {result.FinalRC}.\r\n\r\n");
                if (options.WithLog)
                {
                    SessionLink.AssignedLogger.PartiasLogQueue.Enqueue(errorMessage.ToString());
                }

                return true;
            }
            errorMessage?.Append($"Сумма: {sum} {currentPlacesCountK} {opersFormat.DimensionUnitToString()}, подходящая тара не найдена.\r\n\r\n");
            if (options.WithLog)
            {
                SessionLink.AssignedLogger.PartiasLogQueue.Enqueue(errorMessage.ToString());
            }

            if (LogErrorsBySettings && options.LogSomeErrors)
            {
                if (NoChangeTaraNonErrorsCount == 0)
                {
                    // не найден, потому что ни одна не подошла из-за "не менять тару"
                    if (searchData.WillVspom)
                        dataErrors.AddNewPartiaError(someOpers.FirstOperLink.Index, NewDataErrorType.NoChangeTaraNoPriemVspom, searchData.dataErrorMore);
                    else
                        dataErrors.AddNewPartiaError(someOpers.FirstOperLink.Index, NewDataErrorType.NoChangeTaraNoPriem, searchData.dataErrorMore);
                }
                else
                {
                    if (OrigTaraNonErrorsCount == 0)
                    {
                        // не найден РЦ с подходящей приемной тарой
                        var dem = searchData.dataErrorMore;
                        dem.DataToInsert = origTara.Label;
                        dataErrors.AddNewPartiaError(someOpers.FirstOperLink.Index, NewDataErrorType.NoPriemOrigTara, dem);
                    }
                    else
                    {
                        // Здесь - банальная ошибка на случай, если нужно чекнуть "вылажу ли я за максимальные намотки"
                        if (someOpers.AllOpers.Count == 1 || someOpers.FirstOperLink.MesModelling.Code != Guid.Empty)
                        {
                            //AddPartiaError(someOpers.FirstOperLink.Index, PartiaErrorLogType.NotEnoughMaxOnTaras);

                            if (searchData.WillVspom)
                                dataErrors.AddNewPartiaError(someOpers.FirstOperLink.Index, NewDataErrorType.NotEnoughMaxOnTarasVspom, searchData.dataErrorMore);
                            else
                                dataErrors.AddNewPartiaError(someOpers.FirstOperLink.Index, NewDataErrorType.NotEnoughMaxOnTaras, searchData.dataErrorMore);
                        }
                    }
                }
            }


            return false;
        }

        public static bool SumIsPossible(IAbleForTaraCalcInterface allOpers,//, float addNaladLength,// object allOpersObject,
            //out Uco TaraName, out Uco FinalRC, out double Max, out DimensionUnit used_format, out bool notChangeTara, //bool withUI/*, bool CheckChildren*/,
            out TaraSearchEngineResult result,
            out TemporaryTaraSearchData searchData,
            SumIsPossibleOptions options
            )
        {
            result = new TaraSearchEngineResult();
            return
                GetAllVariantsForThese(allOpers, out searchData, options)
            &&
                FilterAllVariants(allOpers, searchData, out result, options);

            //return
            //    Merger.GetAllVariantsForThese(allOpers, withUI/*, CheckChildren*/, out searchData, PrevsInfo, NextRCsMustBeThese)
            //    &&
            //    Merger.FilterAllVariants(allOpers, searchData, addNaladLength, out TaraName, out FinalRC, out Max, out used_format, out notChangeTara,
            //    withUI, PrevsInfo, RCMustBeThis, TaraMustBeThis, ExceptTheseTaras, logSomeErros);
        }
    }
}

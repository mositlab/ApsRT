using Rodana.Core.DataErrors;
using Rodana.Core.Partias.MergeConditions;
using Rodana.Core.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using UniversalPack.Extensions;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.TaraSearchEngine
{
    internal class TaraSearchEngine
    {

        /// <summary>
        /// Получаем список РЦ и тары, которая может встать для них
        /// </summary>
        internal static bool GetAllVariantsForThese(IAbleForTaraCalcInterface allOpers, out TemporaryTaraSearchData searchData, SumIsPossibleOptions options)
        {
            var SessionLink = allOpers.FirstOperLink.SessionLink;
            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            var TaraDataSet = SessionLink.MasterDataLinks.TarasMasterData;
            var dataErrors = SessionLink.DataErrors;
            var CentersData = SessionLink.MasterDataLinks.CentersMasterData.CentersData;
            bool LogErrorsBySettings = SessionLink.SettingsController.CurrentSettings.LogErrors;
            var ModToTaraInfoLink = options.ModToTaraInfoLink;


            if (allOpers.ModId != -1 && (ModToTaraInfoLink?.TryGetValue(allOpers.ModId, out var resu) ?? false))
            {
                searchData = resu;
                return resu != null;
            }

            bool willVspom = allOpers.FirstOperLink.UsesVspomTara || allOpers.AllOpers.Any(x => x.Nexts.Any(y => ALL[y].UsesVspomTara && ALL[y].NoChangeTara)); // Признак "вспом. тару" должен быть у текущего, либо у ребенка (и у ребенка должен быть признак не менять тару)

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

            searchData.dataErrorMore.RCsOnPrevEtap = allOpers.AllOpers.SelectMany(x => x.Prevs).SelectMany(Index =>
            {
                var op = ALL[Index];
                return op.PossibleCenters ?? new List<Uco> { op.Center };
            }).Distinct().ToList();

            searchData.dataErrorMore.Count = searchData.sum;

            searchData.errorMessage?.Append($"Поиск тар и рц для: {string.Join("; ", allOpers.AllOpers)}\r\n");

            if (allOpers.FirstOperLink.NoCheckNamotka)
            {
                searchData.errorMessage?.Append($"{allOpers.FirstOperLink}: не проверяет намотку. Сразу ухожу.\r\n");
                var rcs = allOpers.FirstOperLink.PossibleCenters ?? new List<Uco>() { allOpers.FirstOperLink.Center };
                searchData.PossibleCentersAndTaras = rcs.Select(x => (RC: x, Tara: new Uco(Guid.Empty, ""))).ToArray();
                return true;
            }

            var doNotCheckWindings = SessionLink.MasterDataLinks.MergeConditionsMasterData.DoNotCheckWindingsSafe(allOpers.FirstOperLink.OperationVid.Code, allOpers.FirstOperLink.Subdivision.Code);//allOpers.Select(e => e.oper.OperationVid.Code).First());
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
                    TaraHereMustBeThis = new(first.Prevs.Select(x => options.PrevsInfo.TryGetValue(x, out var taraMax) ? (taraMax, first.NoChangeTara ? false : ALL[x].UsesVspomTara) : (new(Guid.Empty, ""), false)).Distinct().Where(x => !x.Item1.IsEmpty));
                    searchData.errorMessage?.Append($"Отд. тары у {first.Index}: {string.Join(", ", TaraHereMustBeThis)}\r\n");
                }


                if (TaraHereMustBeThis != null) searchData.dataErrorMore.NeededOtdTarasOnThisEtap = (
                    searchData.dataErrorMore.NeededOtdTarasOnThisEtap == null ? TaraHereMustBeThis/*.Select(x => x.Tara)*/.Distinct().ToList() :
                    searchData.dataErrorMore.NeededOtdTarasOnThisEtap.Union(TaraHereMustBeThis/*.Select(x => x.Tara)*/).ToList()
                    );

                //bool VspomTara = first.Prevs.Count > 0 && ALL[first.Prevs[0]].UsesVspomTara;
                if (first.PossibleCenters == null)
                {
                    rcs.Add(first.Center);
                    res = Array.Empty<(Uco RC, Uco Tara)>();
                    var res00 = TaraDataSet.FromAndToTara.ContainsKey(first.Center)
                        && (!first.IsVspom || CentersData[first.Center].AbleToVspom);
                    if (res00)
                    {
                        if (TaraHereMustBeThis == null || TaraHereMustBeThis.Count == 0 || TaraHereMustBeThis.All(x => TaraDataSet.GetNeededTaras(first.Center, first.OperationVid, x.WasVspom)/*.FromAndToTara[first.Center]*/.otd.TryGetValue(x.WasVspom, out var has) && has.Contains(x.Tara)))
                        {
                            res = (TaraDataSet.GetNeededTaras(first.Center, first.OperationVid, null).priem.TryGetValue(false, out var has) ? has.Select(x => (RC: first.Center, Tara: x)) : new (Uco, Uco)[] { }).ToArray();
                        }
                        else
                        {
                            if (LogErrorsBySettings && (allOpers.AllOpers.Count == 1 || allOpers.FirstOperLink.MesModelling.Code != Guid.Empty))
                                dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoAllNeededOtdTaras, searchData.dataErrorMore);
                            return false;
                        }
                    }
                }
                else
                {
                    rcs.AddRange(first.PossibleCenters);
                    res = Array.Empty<(Uco RC, Uco Tara)>();
                    var res00 = first.PossibleCenters.Where(x => TaraDataSet.FromAndToTara.ContainsKey(x)
                    && (!first.IsVspom || CentersData[x].AbleToVspom));
                    if (res00.Count() > 0)
                    {
                        var res01 = res00.Where(x => TaraHereMustBeThis == null || TaraHereMustBeThis.Count == 0 || TaraHereMustBeThis.All(z => TaraDataSet.GetNeededTaras(x, first.OperationVid, z.WasVspom).otd.TryGetValue(z.WasVspom, out var has) && has.Contains(z.Tara)));
                        if (res01.Count() == 0)
                        {
                            if (LogErrorsBySettings && (allOpers.AllOpers.Count == 1 || allOpers.FirstOperLink.MesModelling.Code != Guid.Empty))
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


                searchData.dataErrorMore.RCsOnThisEtap = (
                    searchData.dataErrorMore.RCsOnThisEtap == null ? rcs.ToList() :
                    searchData.dataErrorMore.RCsOnThisEtap.Union(rcs).ToList()
                    );

                searchData.dataErrorMore.TarasOnThisEtap = (
                    searchData.dataErrorMore.TarasOnThisEtap == null ? res.Select(x => x.Tara).ToList() :
                    searchData.dataErrorMore.TarasOnThisEtap.Union(res.Select(x => x.Tara)).ToList()
                    );

                // Проверим конечную тару
                if (!WantThisOrigTara.IsEmpty && !res.Any(X => X.Tara == WantThisOrigTara))
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
                        var set1 = new HashSet<Uco>(res0.Select(x => x.Tara));
                        searchData.BARABANAS = searchData.BARABANAS.Where(x => set1.Contains(x.Tara)).ToArray();//.Add(poss2);
                    }

                    searchData.dataErrorMore.MaxNamotka = searchData.BARABANAS.Select(x => (x.Tara, x.Namot)).ToList();

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
                    searchData.PossibleCentersAndTaras = searchData.PossibleCentersAndTaras.Intersect(res/*, new CodedObjectTaraEqualityComparer()*/).ToArray();
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
                var childs = allOpers.AllOpers.SelectMany(x => x.Nexts).Distinct().Select(y => (op: ALL[y], isFirst: true))
                    //.Where(x => !x.op.IsLast)
                    .ToList();


                for (var i = 0; i < childs.Count; i++)
                {
                    var first = childs[i];


                    List<(Uco RC, Uco Tara)> foundOtdTara;

                    searchData.errorMessage?.Append($"Доступные РЦ следующего этапа ({first}):\r\n");
                    if (first.op.PossibleCenters == null)
                    {
                        searchData.errorMessage?.Append(first.op.Center + "\r\n");
                        // Сначала проверим момент "НеМенятьТару"
                        if (first.op.NoChangeTara)
                        {
                            childs.AddRange(first.op.Nexts.Where(x => ALL.ContainsKey(x)).Select(x => (op: ALL[x], isFirst: false)));
                            if (!first.isFirst) // если это первый раз, то не проходим мимо, а отдающую тару чекаем
                            {
                                continue;
                            }
                        }

                        // А вот теперь все остальное
                        if (TaraDataSet.FromAndToTara.ContainsKey(first.op.Center) //&& (!first.IsVspom || CentersData[first.Center].NotChangeTara)
                            && (options.NextRCsMustBeThese == null || /*!first.isFirst ||*/ (first.op.NoChangeTara && options.NextRCsExcludeNoChangeTara) || options.NextRCsMustBeThese.Contains(first.op.Center)) // Если есть ограничение по РЦ детей, то нужно чекнуть, входит ли он сюда (это для G используется)
                                                                                                                                                                                                                   //&& first.op.Prevs.All(x => !PrevsInfo.TryGetValue(x, out var prevTara) || )
                            )
                        {

                            // чекнем конечную тару
                            var hasFinalTara = first.op.EndTara.IsEmpty || TaraDataSet.GetNeededTaras(first.op.Center, first.op.OperationVid, null)/*.FromAndToTara[x]*/.priem.TryGetValue(false, out var taras) && taras.Contains(first.op.EndTara); // у него должна спец приемная тара в случае чего

                            if (hasFinalTara)
                            {
                                bool FirVsp = first.isFirst && first.op.NoChangeTara;

                                HashSet<(Uco Tara, bool WasVspom)> localTaraHereMustBeThis = options.PrevsInfo == null ? null : first.op.Prevs.Where(x => !ALL[x].IsVspom && options.PrevsInfo.ContainsKey(x)).Select(x => (options.PrevsInfo[x], ALL[x].UsesVspomTara)).ToHashSet();

                                var res01 = localTaraHereMustBeThis == null || localTaraHereMustBeThis.Count == 0 || localTaraHereMustBeThis.All(z => TaraDataSet.GetNeededTaras(first.op.Center, first.op.OperationVid, FirVsp ? null : z.WasVspom).otd.TryGetValue(FirVsp ? false : z.WasVspom, out var has) && has.Contains(z.Tara));

                                if (res01)
                                {
                                    foundOtdTara = TaraDataSet.GetNeededTaras(first.op.Center, first.op.OperationVid, FirVsp ? null : willVspom)/*.FromAndToTara[first.op.Center]*/.otd
                                        .TryGetValue(FirVsp ? false : willVspom, out var has) // вот как у этапа выше вспом. тара назначена - вот здесь она себя и проявляет
                                                                                              //&&
                                                                                              //first.op.Prevs.All(x => PrevsInfo == null || !PrevsInfo.TryGetValue(x, out var t) || has.Contains(t)) // Нужно ещё чекнуть, чтоб все тары у предыдущих этапов были тут (полная проверка)
                                        ? has.Select(x => (RC: first.op.Center, Tara: x)).ToList()
                                        : new (Uco, Uco)[] { }.ToList();
                                }
                                else
                                {
                                    var dem = searchData.dataErrorMore;
                                    dem.DataToInsert = first.op.EndTara.Label;
                                    dataErrors.AddNewPartiaError(first.op.Index, NewDataErrorType.NoAllNeededOtdTarasWithCheck, dem);
                                    foundOtdTara = new();
                                }
                            }
                            else
                            {
                                var dem = searchData.dataErrorMore;
                                dem.DataToInsert = first.op.EndTara.Label;

                                var rcsToIns = first.op.PossibleCenters ?? new() { first.op.Center };
                                dem.RCsOnNextEtap = (
                                    dem.RCsOnNextEtap == null ? rcsToIns.ToList() :
                                    dem.RCsOnNextEtap.Union(rcsToIns).Distinct().ToList()
                                    );

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
                            childs.AddRange(first.op.Nexts.Where(x => ALL.ContainsKey(x)).Select(x => (op: ALL[x], isFirst: false)));
                            if (!first.isFirst) // если это первый раз, то не проходим мимо, а отдающую тару чекаем
                            {
                                continue;
                            }
                        }

                        // А вот теперь все остальное
                        bool FirVsp = first.isFirst && first.op.NoChangeTara;

                        var rcs1 = first.op.PossibleCenters.Where(x => TaraDataSet.FromAndToTara.ContainsKey(x)
                        && (first.op.EndTara.IsEmpty || TaraDataSet.GetNeededTaras(x, first.op.OperationVid, null)/*.FromAndToTara[x]*/.priem.TryGetValue(false, out var taras) && taras.Contains(first.op.EndTara))); // у него должна спец приемная тара в случае чего
                        //);



                        if (rcs1.Count() > 0)
                        {

                            HashSet<(Uco Tara, bool WasVspom)> localTaraHereMustBeThis = options.PrevsInfo == null ? null : first.op.Prevs.Where(x => !ALL[x].IsVspom && options.PrevsInfo.ContainsKey(x)).Select(x => (options.PrevsInfo[x], ALL[x].UsesVspomTara)).ToHashSet();

                            var res01 = rcs1.Where(x => (localTaraHereMustBeThis == null || localTaraHereMustBeThis.Count == 0 || localTaraHereMustBeThis.All(z => TaraDataSet.GetNeededTaras(x, first.op.OperationVid, FirVsp ? null : z.WasVspom).otd.TryGetValue(FirVsp ? false : z.WasVspom, out var has) && has.Contains(z.Tara))));

                            if (res01.Count() > 0)
                            {
                                foundOtdTara = res01.Where(x =>
                                 //&& (!first.IsVspom || CentersData[x].AbleToVspom)
                                 (options.NextRCsMustBeThese == null || /*!first.isFirst ||*/ (first.op.NoChangeTara && options.NextRCsExcludeNoChangeTara) || options.NextRCsMustBeThese.Contains(x)) // Если есть ограничение по РЦ детей, то нужно чекнуть, входит ли он сюда
                                ).SelectMany(y => TaraDataSet.GetNeededTaras(y, first.op.OperationVid, FirVsp ? null : willVspom)/*.FromAndToTara[y]*/.otd.TryGetValue(FirVsp ? false : willVspom, out var has)
                                    //&&
                                    //        first.op.Prevs.All(x => PrevsInfo == null || !PrevsInfo.TryGetValue(x, out var t) || has.Contains(t)) // Нужно ещё чекнуть, чтоб все тары у предыдущих этапов были тут (полная проверка)
                                    ? has.Select(x => (RC: y, Tara: x)) : new (Uco, Uco)[] { }).ToList();
                            }
                            else
                            {
                                var dem = searchData.dataErrorMore;
                                dem.DataToInsert = first.op.EndTara.Label;
                                dataErrors.AddNewPartiaError(first.op.Index, NewDataErrorType.NoAllNeededOtdTarasWithCheck, dem);
                                foundOtdTara = new();
                            }

                        }
                        else
                        {
                            var dem = searchData.dataErrorMore;
                            dem.DataToInsert = first.op.EndTara.Label;

                            var rcsToIns = first.op.PossibleCenters ?? new() { first.op.Center };
                            dem.RCsOnNextEtap = (
                                dem.RCsOnNextEtap == null ? rcsToIns.ToList() :
                                dem.RCsOnNextEtap.Union(rcsToIns).Distinct().ToList()
                                );

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

                    searchData.dataErrorMore.TarasOnNextEtap = (
                        searchData.dataErrorMore.TarasOnNextEtap == null ? foundOtdTara.Select(x => x.Tara).Distinct().ToList() :
                        searchData.dataErrorMore.TarasOnNextEtap.Union(foundOtdTara.Select(x => x.Tara)).Distinct().ToList()
                        );

                    var rcs = first.op.PossibleCenters ?? new() { first.op.Center };
                    searchData.dataErrorMore.RCsOnNextEtap = (
                        searchData.dataErrorMore.RCsOnNextEtap == null ? rcs.ToList() :
                        searchData.dataErrorMore.RCsOnNextEtap.Union(rcs).Distinct().ToList()
                        );

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

                    var set2 = new HashSet<Uco>(foundOtdTara.Select(x => x.Tara));
                    var setRC = new HashSet<Uco>(searchData.PossibleCentersAndTaras.Select(x => x.RC));
                    searchData.PossibleCentersAndTaras = searchData.PossibleCentersAndTaras.Where(x => set2.Contains(x.Tara)).ToArray();//.Add(poss2);

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


            var set = new HashSet<Uco>(searchData.PossibleCentersAndTaras.Select(x => x.Tara));
            if (searchData.BARABANAS != null) searchData.BARABANAS = searchData.BARABANAS.Where(x => set.Contains(x.Tara)).ToArray();//.Add(poss2);

            if (searchData.BARABANAS != null) searchData.dataErrorMore.MaxPlusUsedTaras = searchData.BARABANAS.Select(x => x.Tara).Distinct().ToList();
            //BARABANAS = BARABANAS.IntersectBy(PossibleCentersAndTaras.Select(x => x.Tara), x => x.Tara).ToList();// в конце концов пересекаем.


            if (checkBarabanasCount && (searchData.BARABANAS == null || searchData.BARABANAS.Length == 0))
            {
                if (LogErrorsBySettings && (allOpers.AllOpers.Count == 1 /*|| allOpers.FirstOperLink.Modelling != Guid.Empty*/) && options.NextRCsMustBeThese == null)
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

        internal static bool FilterAllVariants(IAbleForTaraCalcInterface someOpers, TemporaryTaraSearchData searchData,
            out Uco TaraName, out Uco FinalRC, out double Max, out DimensionUnit used_format, out bool notChangeTara,
            SumIsPossibleOptions options)
        {

            var SessionLink = someOpers.FirstOperLink.SessionLink;
            var CentersDataSet = SessionLink.MasterDataLinks.CentersMasterData;
            var dataErrors = SessionLink.DataErrors;
            bool LogErrorsBySettings = SessionLink.SettingsController.CurrentSettings.LogErrors;

            TaraName = new(Guid.Empty, "");
            FinalRC = new(Guid.Empty, "");
            used_format = DimensionUnit.Unknown;
            Max = options.SearchForMaxTara ? -1 : double.MaxValue;
            notChangeTara = false;
            var sum = searchData.sum + options.NaladLength;
            var errorMessage = searchData.errorMessage;
            var opersFormat = searchData.opersFormat;

            // Если - исключения из номенклатуры
            if (someOpers.FirstOperLink.NoCheckNamotka)
            {
                searchData.errorMessage?.Append($"{someOpers.FirstOperLink}: не проверяет намотку. Сразу проверяю.\r\n");
                var center = searchData.PossibleCentersAndTaras[0];
                TaraName = center.Tara;
                Max = 0;
                FinalRC = center.RC;
                used_format = DimensionUnit.Unknown;
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

                    notChangeTara = true;

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

            var doNotCheckWindings = SessionLink.MasterDataLinks.MergeConditionsMasterData.DoNotCheckWindingsSafe(someOpers.FirstOperLink.OperationVid.Code, someOpers.FirstOperLink.Subdivision.Code);// || TaraDataSet.NotCheckNamotka(someOpers.FirstOperLink);

            //bool recommendedRCSet = false;
            int NoChangeTaraNonErrorsCount = 0;
            int OrigTaraNonErrorsCount = 0;

            var origTara = someOpers.FirstOperLink.EndTara;

            if (options.RecommendedSubdivision.HasValue) // Если определено подразделение, то можно сначала отсортировать по нему.
            {
                searchData.PossibleCentersAndTaras = searchData.PossibleCentersAndTaras.OrderByDescending(x => CentersDataSet.CentersData[x.RC].Subdivision == options.RecommendedSubdivision.Value ? 1 : 0).ToArray();
            }

            for (int h = 0; h < searchData.PossibleCentersAndTaras.Length; h++)
            {
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


                if (options.RCMustBeThis.HasValue && center.RC != options.RCMustBeThis.Value)
                {
                    continue; // две проверки, если надо
                }

                if (options.TaraMustBeThis.HasValue && center.Tara != options.TaraMustBeThis.Value)
                {
                    continue;
                }


                // Тара в списке тех, кого выбирать нельзя?
                if (options.ExceptTheseTaras != null && options.ExceptTheseTaras.Contains(center.Tara))
                {
                    continue;
                }

                if (options.RecommendedSubdivision.HasValue // Есть ограничение?
                    && !FinalRC.IsEmpty // Я уже назначил РЦ?
                    && CentersDataSet.CentersData[FinalRC].Subdivision == options.RecommendedSubdivision.Value // Этот рц из нужного мне подразделения?
                    && CentersDataSet.CentersData[center.RC].Subdivision != options.RecommendedSubdivision.Value) // Текущий РЦ уже из другого подразделения?
                {
                    break; // Значит искать больше смысла нет, я нашел что-то из рекомендуемого цеха.
                }

                if (notChangeTara || doNotCheckWindings || searchData.BARABANAS == null || !origTara.IsEmpty) // Не менять тару? Не проверять макс намотки?
                {
                    if (!FinalRC.IsEmpty
                        && someOpers.FirstOperLink.RecommendedRC.HasValue
                        && someOpers.FirstOperLink.RecommendedRC.Value == FinalRC) continue;
                    TaraName = center.Tara;
                    Max = 0;
                    FinalRC = center.RC;
                    used_format = DimensionUnit.Unknown;
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

                        if (
                            (namot.Namot >= sum
                            || someOpers.ZadelCode != 0) // Не меньше суммы должна быть намотка, ну либо это что с ЗАДЕЛАМИ, поэтому тут проверять на макс намотку не следует (Алёна, 17.06)
                            && ((!options.SearchForMaxTara && namot.Namot < Max) || (options.SearchForMaxTara && namot.Namot > Max))) // Должен быть меньше уже предложенного
                        {
                            if (!FinalRC.IsEmpty
                                && someOpers.FirstOperLink.RecommendedRC.HasValue
                                && someOpers.FirstOperLink.RecommendedRC.Value == FinalRC) continue;

                            TaraName = center.Tara;
                            Max = namot.Namot;
                            FinalRC = center.RC;
                            used_format = namot.Format;
                        }
                    }
                }
            }

            if (!FinalRC.IsEmpty)
            {
                errorMessage?.Append($"Сумма: {sum} {opersFormat.DimensionUnitToString()}, выбрал {TaraName} ({Max} {used_format}), {FinalRC}.\r\n\r\n");
                if (options.WithLog)
                {
                    SessionLink.AssignedLogger.PartiasLogQueue.Enqueue(errorMessage.ToString());
                }

                return true;
            }
            errorMessage?.Append($"Сумма: {sum} {opersFormat.DimensionUnitToString()}, подходящая тара не найдена.\r\n\r\n");
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

        internal static bool SumIsPossible(IAbleForTaraCalcInterface allOpers,//, float addNaladLength,// object allOpersObject,
            out Uco TaraName, out Uco FinalRC, out double Max, out DimensionUnit used_format, out bool notChangeTara, //bool withUI/*, bool CheckChildren*/,
            out TemporaryTaraSearchData searchData,
            SumIsPossibleOptions options
            //Dictionary<int, CodedObject<string>> PrevsInfo = null, // в такой структуре он нуждается только из-за "НеМенятьТару" - напрашивается рефакторинг.
            //CodedObject<string>? RCMustBeThis = null, // ограничение, что РЦ должен быть именно этот
            //CodedObject<string>? TaraMustBeThis = null, // ограничение, что тара должна быть именно эта
            //HashSet<CodedObject<string>> ExceptTheseTaras = null, // ограничение, что вот эти тары юзать нельзя
            //HashSet<CodedObject<string>> NextRCsMustBeThese = null,// ограничение, что у детей должны быть только эти РЦ
            //bool logSomeErros = true
            )
        {
            TaraName = new(Guid.Empty, "");
            FinalRC = new(Guid.Empty, "");
            used_format = DimensionUnit.Unknown;
            Max = 0;
            notChangeTara = false;
            return
                GetAllVariantsForThese(allOpers, out searchData, options)
            &&
                FilterAllVariants(allOpers, searchData, out TaraName, out FinalRC, out Max, out used_format, out notChangeTara, options);

            //return
            //    Merger.GetAllVariantsForThese(allOpers, withUI/*, CheckChildren*/, out searchData, PrevsInfo, NextRCsMustBeThese)
            //    &&
            //    Merger.FilterAllVariants(allOpers, searchData, addNaladLength, out TaraName, out FinalRC, out Max, out used_format, out notChangeTara,
            //    withUI, PrevsInfo, RCMustBeThis, TaraMustBeThis, ExceptTheseTaras, logSomeErros);
        }
    }
}

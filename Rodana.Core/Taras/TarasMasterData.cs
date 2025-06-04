using Azure;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Rodana.Core.DataErrors;
using Rodana.Core.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using static Azure.Core.HttpHeader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Taras
{

    /// <summary>
    /// НСИ "Применяемая тара"
    /// </summary>
    public class TarasMasterData
    {
        public Session.Session SessionLink { get; }

        public TarasMasterData(Session.Session session)
        {
            SessionLink = session;
        }

        internal static Uco Pseudotara1 = new Uco(Guid.NewGuid(), "Псевдотара 1");
        internal static Uco Pseudotara2 = new Uco(Guid.NewGuid(), "Псевдотара 2");
        internal static Uco Pseudotara3 = new Uco(Guid.NewGuid(), "Псевдотара 3");

        // А это список тары и ее количества на заводе
        internal Dictionary<Uco, int> TarasPossibleCount = new();

        internal Dictionary<Uco /*Номенклатура*/,
            Dictionary<Uco, // ВидТехнологическойОперации
            (bool NotCheckPrice, bool NotCheckMax)>> IsklNomen
            = new(); // Исключения номенклатуры

        internal void AddIsklNomenData(IsklNomenDataLoaderModel model)
        {
            var nomen = model.Nomenklature;
            var vid = model.OperationVid;
            IsklNomen.TryAdd(nomen, new());
            IsklNomen[nomen].TryAdd(vid, new());
            IsklNomen[nomen][vid] = (model.NotCheckPrice, model.NotCheckMax);
        }

        internal bool NotCheckNamotka(Stages.Stage op)
        {
            if (IsklNomen.TryGetValue(op.Nomenklature, out var lvl1))
            {
                if (lvl1.TryGetValue(op.OperationVid, out var res)) return res.NotCheckMax;
                if (lvl1.TryGetValue(Uco.EmptyObject, out var res2)) return res2.NotCheckMax;
            }
            if (IsklNomen.TryGetValue(Uco.EmptyObject, out var lvl11))
            {
                if (lvl11.TryGetValue(op.OperationVid, out var res)) return res.NotCheckMax;
            }
            return false;
        }

        internal Dictionary<Uco /*РЦ*/,
            Dictionary<Uco, // вид операции
            (Dictionary<bool, List<Uco>> Otd, Dictionary<bool, List<Uco>> Priem)>> FromAndToTara
            = new(/*new UcoEqualityComparer()*/); // Для каждого РЦ - списки принимающей и отдающей

        internal void UsedTaraPostProcess(bool AddPseudoTara)
        {
            var rcs = FromAndToTara.Keys.ToArray();
            foreach (var rc in rcs)
            {
                var vids = FromAndToTara[rc].Keys.ToArray();
                foreach (var vid in vids)
                {
                    AddIt(rc, vid);
                }
                AddIt(rc, Uco.EmptyObject);


                void AddIt(Uco rc, Uco vid)
                {
                    if (!FromAndToTara.ContainsKey(rc) || !FromAndToTara[rc].ContainsKey(vid)) return;
                    if (FromAndToTara[rc][vid].Otd != null)
                    {
                        var dicOtd = FromAndToTara[rc][vid].Otd;// = (FromAndToTara[key].otd.Distinct().ToList(), FromAndToTara[key].priem.Distinct().ToList());

                        if (AddPseudoTara)
                        {
                            dicOtd.TryAdd(false, new());
                            dicOtd.TryAdd(true, new());
                        }

                        if (dicOtd.ContainsKey(false))
                        {
                            if (AddPseudoTara)
                            {
                                dicOtd[false].Add(Pseudotara1);
                                dicOtd[false].Add(Pseudotara2);
                                dicOtd[false].Add(Pseudotara3);
                            }
                            dicOtd[false] = dicOtd[false].Distinct().ToList();
                        }

                        if (dicOtd.ContainsKey(true))
                        {
                            if (AddPseudoTara)
                            {
                                dicOtd[true].Add(Pseudotara1);
                                dicOtd[true].Add(Pseudotara2);
                                dicOtd[true].Add(Pseudotara3);
                            }
                            dicOtd[true] = dicOtd[true].Distinct().ToList();
                        }

                        var dicPriem = FromAndToTara[rc][vid].Priem;// = (FromAndToTara[key].otd.Distinct().ToList(), FromAndToTara[key].priem.Distinct().ToList());

                        if (AddPseudoTara)
                        {
                            dicPriem.TryAdd(false, new());
                            dicPriem.TryAdd(true, new());
                        }

                        if (dicPriem.ContainsKey(false))
                        {
                            if (AddPseudoTara)
                            {
                                dicPriem[false].Add(Pseudotara1);
                                dicPriem[false].Add(Pseudotara2);
                                dicPriem[false].Add(Pseudotara3);
                            }
                            dicPriem[false] = dicPriem[false].Distinct().ToList();
                        }

                        if (dicPriem.ContainsKey(true))
                        {
                            if (AddPseudoTara)
                            {
                                dicPriem[true].Add(Pseudotara1);
                                dicPriem[true].Add(Pseudotara2);
                                dicPriem[true].Add(Pseudotara3);
                            }
                            dicPriem[true] = dicPriem[true].Distinct().ToList();
                        }

                        FromAndToTara[rc][vid] = (dicOtd, dicPriem);
                    }
                }
            }
        }

        internal (Dictionary<bool, List<Uco>> otd, Dictionary<bool, List<Uco>> priem) GetNeededTaras(Uco RC, Uco VidOper, bool? vspomOtd)
        {
            var lvl0 = FromAndToTara[RC];
            if (lvl0.TryGetValue(VidOper, out var lvl1) && (!vspomOtd.HasValue || lvl1.Otd.ContainsKey(vspomOtd.Value))) return lvl1;
            else
            {
                return lvl0[Uco.EmptyObject];
            }
        }

        internal void AddUsedTaraData(UsedTaraDataLoaderModel model, bool addPseudoTara)
        {
            var center_int = model.CenterName;
            var vid = model.TechOperation;
            bool isVspom = model.IsVspom;
            bool otd = model.IsOutput;
            bool priem = model.IsInput;

            FromAndToTara.TryAdd(center_int, new());

            if (!FromAndToTara[center_int].ContainsKey(vid))
            {
                FromAndToTara[center_int].Add(vid, (new(), new()));
            }

            if (!addPseudoTara) // чтобы не мешались обычные тары, тут стоит эта проверка
            {
                if (otd)
                {
                    if (!FromAndToTara[center_int][vid].Otd.ContainsKey(isVspom))
                    {
                        FromAndToTara[center_int][vid].Otd.Add(isVspom, new());
                    }

                    FromAndToTara[center_int][vid].Otd[isVspom].Add(model.TaraType);
                }

                if (priem)
                {
                    if (!FromAndToTara[center_int][vid].Priem.ContainsKey(isVspom))
                    {
                        FromAndToTara[center_int][vid].Priem.Add(isVspom, new());
                    }

                    FromAndToTara[center_int][vid].Priem[isVspom].Add(model.TaraType);
                }
            }
        }

        // макс намотки
        Dictionary<Uco, // Продукция
            Dictionary<Uco, // Номенклатура
                Dictionary<Uco, // ОПЕР
                    Dictionary<Uco, // ТипТары
                    List<(DimensionUnit Format, double Value)> // Намотка (формат и значения)
                    >
                    >
                >
            > TarasMaxCapacities = new(/*new CodedObjectEqualityComparer()*/);

        public void AddMaxCapacityData(MaxCapacityDataLoaderModel model)
        {
            var production = model.Production;
            var Nomen = model.Nomenklature;
            var oper_int = model.OperationVid;
            if (!TarasMaxCapacities.ContainsKey(production))
            {
                TarasMaxCapacities.Add(production, new(/*new CodedObjectEqualityComparer()*/));
            }

            if (!TarasMaxCapacities[production].ContainsKey(Nomen))
            {
                TarasMaxCapacities[production].Add(Nomen, new(/*new CodedObjectEqualityComparer()*/));
            }

            if (!TarasMaxCapacities[production][Nomen].ContainsKey(oper_int))
            {
                TarasMaxCapacities[production][Nomen].Add(oper_int, new());
            }

            var what = (model.DimensionUnit, model.Amount);

            Add2(model.TaraType, what);

            if (SessionLink.SettingsController.CurrentSettings.UsePseudoTara)
            {
                Add2(Pseudotara1, (what.DimensionUnit, 100000));
                Add2(Pseudotara2, (what.DimensionUnit, 100000));
                Add2(Pseudotara3, (what.DimensionUnit, 100000));
            }

            void Add2(Uco whichTara, (DimensionUnit, double) val)
            {
                if (!TarasMaxCapacities[production][Nomen][oper_int].ContainsKey(whichTara))
                {
                    TarasMaxCapacities[production][Nomen][oper_int].Add(whichTara, new());
                }

                if (!TarasMaxCapacities[production][Nomen][oper_int][whichTara].Contains(val))
                {
                    TarasMaxCapacities[production][Nomen][oper_int][whichTara].Add(val);
                }
            }
        }

        public void ClearMaxCapacities()
        {
            TarasMaxCapacities.Clear();
            IDAlreadyHasNamots.Clear();
            SavedTara.Clear();
        }

        ConcurrentBag<int> IDAlreadyHasNamots = new();
        ConcurrentDictionary<Stages.Stage, List<(Uco Tara, DimensionUnit Format, double Namot)>> SavedTara = new();
        internal List<(Uco Tara, DimensionUnit Format, double Namot)> GetOperTara(Stages.Stage first, bool withUI)
        {
            var SessionLink = first.SessionLink;

            bool usePseudoTara = SessionLink.SettingsController.CurrentSettings.UsePseudoTara;
            bool logErrors = SessionLink.SettingsController.CurrentSettings.LogErrors;

            var dataErrors = SessionLink.DataErrors;

            if (SavedTara.TryGetValue(first, out var resu))
            {
                return resu;
            }

            Dictionary<Uco, // Номенклатура
                Dictionary<Uco, // ОПЕР
                    Dictionary<Uco, // ТипТары
                    List<(DimensionUnit Format, double Value)> // Намотка (формат и значения)
                    >
                    >
                > lvl0;

            List<(Uco Tara, DimensionUnit Format, double Namot)> prodError()
            {
                if (usePseudoTara)
                {
                    return new()
                    {
                        (Pseudotara1, first.EdIzm, 100000),
                        (Pseudotara2, first.EdIzm, 100000),
                        (Pseudotara3, first.EdIzm, 100000)
                    };
                }

                if (withUI && logErrors && !IDAlreadyHasNamots.Contains(first.Index))
                {
                    SessionLink.AssignedLogger.PartiasLogQueue.Enqueue($"Операция {first}: не найдена намотка из-за продукции {first.Production}, ТО {first.OperationVid}, продукция {first.Nomenklature}, метод расчёта: {first.UsedCalcMethod}.\r\n\r\n");
                    IDAlreadyHasNamots.Add(first.Index);
                }
                SavedTara.TryAdd(first, null);
                return null;
            }

            // Через пару месяцев от этой хуйни откажемся.
            if (first.UsedCalcMethod == DimensionUnit.m)
            {
                if (TarasMaxCapacities.TryGetValue(first.Production, out lvl0))
                {
                    if (lvl0.TryGetValue(first.Nomenklature, out var lvl1))
                    {
                        if (lvl1.TryGetValue(first.OperationVid, out var possible))
                        {
                            var finalList = returnFunc(possible);
                            SavedTara.TryAdd(first, finalList);
                            return finalList;
                        }
                        else
                        {
                            // Сразу ошибка
                            dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoVidOper, new DataErrorStruct());
                            return prodError();
                        }
                    }
                    else
                    {
                        // Сразу ошибка
                        dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoNomenklature, new DataErrorStruct());
                        return prodError();
                    }
                }
                else
                {
                    // Сразу ошибка
                    dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoProduction, new DataErrorStruct());
                    return prodError();
                }
                //if (TaraDataSet.TarasMaxCapacities.TryGetValue(first.Production, out lvl0)
                //&&
                //lvl0.TryGetValue(first.Nomenklatura, out var lvl1)
                //&&
                //lvl1.TryGetValue(first.OperVid, out var possible)
                //)
                //{
                //    var finalList = returnFunc(possible);
                //    SavedTara.TryAdd(first, finalList);
                //    return finalList;
                //}
                //else
                //{
                //    // Сразу ошибка
                //    SavedTara.TryAdd(first, null);
                //    return prodError();
                //}
            }
            else
            {
                // Иначе - по весу. Сначала ищем по продукции
                List<(Uco Tara, DimensionUnit Format, double Namot)> finalList = new();

                bool b1 = TarasMaxCapacities.TryGetValue(first.Production, out lvl0);
                bool b2 = false;
                Dictionary<Uco, // ОПЕР
                    Dictionary<Uco, // ТипТары
                    List<(DimensionUnit Format, double Value)> // Намотка (формат и значения)
                    >
                    > lvl1 = null;
                Dictionary<Uco, // ТипТары
                    List<(DimensionUnit Format, double Value)> // Намотка (формат и значения)
                    > possible = null;
                if (b1) b2 = lvl0.TryGetValue(first.Nomenklature, out lvl1);
                bool b3 = false;
                if (b2) b3 = lvl1.TryGetValue(first.OperationVid, out possible);

                if (b1 && b2 && b3)
                {
                    finalList.AddRange(returnFunc(possible));
                }

                bool b4 = TarasMaxCapacities.TryGetValue(new(Guid.Empty, ""), out lvl0);
                bool b5 = false;
                if (b4) b5 = lvl0.TryGetValue(first.Nomenklature, out lvl1);
                bool b6 = false;
                if (b5) b6 = lvl1.TryGetValue(first.OperationVid, out possible);
                if (b4 && b5 && b6)
                {
                    finalList.AddRange(returnFunc(possible));
                }
                finalList = finalList.Distinct().ToList();
                if (finalList.Count == 0)
                {


                    // Выводим ошибки
                    if (!b1 && !b4)
                    {
                        dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoProduction, new DataErrorStruct());
                    }

                    if (!b2) dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoNomenklature, new DataErrorStruct());
                    if (!b5) dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoNomenklatureEmptyProd, new DataErrorStruct());

                    if (!b3) dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoVidOper, new DataErrorStruct());
                    if (!b6) dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoVidOperEmptyProd, new DataErrorStruct());

                    return prodError();
                }
                SavedTara.TryAdd(first, finalList);
                return finalList;
            }

            // Это превращалка в таблицу - на случай, если у одной тары есть несколько значений макс количества.
            List<(Uco, DimensionUnit, double)> returnFunc(Dictionary<Uco, List<(DimensionUnit Format, double Value)>> possibl)
            {
                var res = possibl.SelectMany(x => x.Value.Select(y => (x.Key, y.Format, y.Value))).ToList();//.Where(x => x.Value >= first.Count)
                return res;
            }
        }
    }
}

using ApsRT.Core.DataErrors;
using ApsRT.Core.Session;
using Azure;
using Google.Protobuf.Compiler;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using UniversalPack.Extensions;
using static Azure.Core.HttpHeader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Taras
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
                Dictionary<Uco, /*Хар-ка*/
            Dictionary<Uco, // ВидТехнологическойОперации
            (bool NotCheckPrice, bool NotCheckMax)>>
                    > IsklNomen
            = new(); // Исключения номенклатуры

        internal void AddIsklNomenData(IsklNomenDataLoaderModel model)
        {
            var nomen = model.Nomenklature;
            var chars = model.Characteristics;
            var vid = model.OperationVid;
            IsklNomen.TryAdd(nomen, new());
            IsklNomen[nomen].TryAdd(chars, new());
            IsklNomen[nomen][chars].TryAdd(vid, new());
            IsklNomen[nomen][chars][vid] = (model.NotCheckPrice, model.NotCheckMax);
        }

        internal bool NotCheckNamotka(Stages.Stage op)
        {
            if (IsklNomen.TryGetValue(op.Nomenklature, out var lvl1))
            {
                if (lvl1.TryGetValue(op.Characteristics, out var lvl12))
                {
                    if (lvl12.TryGetValue(op.OperationVid, out var res)) return res.NotCheckMax;
                    if (lvl12.TryGetValue(Uco.EmptyObject, out var res2)) return res2.NotCheckMax;
                }

                if (lvl1.TryGetValue(Uco.EmptyObject, out var lvl13))
                {
                    if (lvl13.TryGetValue(op.OperationVid, out var res)) return res.NotCheckMax;
                    if (lvl13.TryGetValue(Uco.EmptyObject, out var res2)) return res2.NotCheckMax;
                }
            }
            if (IsklNomen.TryGetValue(Uco.EmptyObject, out var lvl11))
            {

                if (lvl11.TryGetValue(op.Characteristics, out var lvl14))
                {
                    if (lvl14.TryGetValue(op.OperationVid, out var res)) return res.NotCheckMax;
                    if (lvl14.TryGetValue(Uco.EmptyObject, out var res2)) return res2.NotCheckMax;
                }

                if (lvl11.TryGetValue(Uco.EmptyObject, out var lvl15))
                {
                    if (lvl15.TryGetValue(op.OperationVid, out var res)) return res.NotCheckMax;
                    if (lvl15.TryGetValue(Uco.EmptyObject, out var res2)) return res2.NotCheckMax;
                }

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
                    FromAndToTara[center_int][vid].Priem.TryAdd(isVspom, new());
                    FromAndToTara[center_int][vid].Priem[isVspom].Add(model.TaraType);

                    // Заказчику насрать, есть ли признак "вспомогательная" у приемной тары при ее выборк.
                    if (isVspom)
                    {
                        // Скопируем в основную
                        FromAndToTara[center_int][vid].Priem.TryAdd(false, new());
                        FromAndToTara[center_int][vid].Priem[false].Add(model.TaraType);
                    }
                }
            }
        }


        // макс намотки
        public Dictionary<Uco, // Продукция
            Dictionary<Uco, // Номенклатура
                Dictionary<Uco, // Хар-ка
                    Dictionary<Uco, // ОПЕР
                        Dictionary<int, // ВЕРСИЯ ЭТАПА
                            Dictionary<Uco, // ТипТары
                                List<(DimensionUnit Format, double Value)>
                            >
                        >
                    >
                >
            >
        > TarasMaxCapacities
        { get; private set; } = new();

        public void AddMaxCapacityData(MaxCapacityDataLoaderModel model)
        {
            var production = model.Production;
            var Nomen = model.Nomenklature;
            var Chars = model.Characteristics;
            var oper_int = model.OperationVid;
            var version = model.StageVersion;
            if (version <= 0) version = 1;

            if (!TarasMaxCapacities.ContainsKey(production))
            {
                TarasMaxCapacities.Add(production, new(/*new CodedObjectEqualityComparer()*/));
            }

            if (!TarasMaxCapacities[production].ContainsKey(Nomen))
            {
                TarasMaxCapacities[production].Add(Nomen, new(/*new CodedObjectEqualityComparer()*/));
            }

            if (!TarasMaxCapacities[production][Nomen].ContainsKey(Chars))
            {
                TarasMaxCapacities[production][Nomen].Add(Chars, new());
            }

            if (!TarasMaxCapacities[production][Nomen][Chars].ContainsKey(oper_int))
            {
                TarasMaxCapacities[production][Nomen][Chars].Add(oper_int, new());
            }

            if (!TarasMaxCapacities[production][Nomen][Chars][oper_int].ContainsKey(version))
            {
                TarasMaxCapacities[production][Nomen][Chars][oper_int].Add(version, new());
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
                var lvl = TarasMaxCapacities[production][Nomen][Chars][oper_int][version];

                if (!lvl.ContainsKey(whichTara))
                {
                    lvl.Add(whichTara, new());
                }

                if (!lvl[whichTara].Contains(val))
                {
                    lvl[whichTara].Add(val);
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
            int version = first.StageVersion;

            var SessionLink = first.SessionLink;

            bool usePseudoTara = SessionLink.SettingsController.CurrentSettings.UsePseudoTara;
            bool logErrors = SessionLink.SettingsController.CurrentSettings.LogErrors;

            var dataErrors = SessionLink.DataErrors;

            if (SavedTara.TryGetValue(first, out var resu))
            {
                return resu;
            }

            Dictionary<Uco,
                Dictionary<Uco,
                    Dictionary<Uco,
                        Dictionary<int,
                            Dictionary<Uco,
                                List<(DimensionUnit Format, double Value)>
                            >
                        >
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
            if (first.UsedCalcMethod == DimensionUnit.m && !SessionLink.SettingsController.SpecialSettings.UseOnlyWeightMethodWhenSearchWindings)
            {
                if (TarasMaxCapacities.TryGetValue(first.Production, out lvl0))
                {
                    if (lvl0.TryGetValue(first.Nomenklature, out var lvl1))
                    {
                        if (lvl1.TryGetValue(first.Characteristics, out var lvl2))
                        {
                            if (lvl2.TryGetValue(first.OperationVid, out var byVersion))
                            {
                                if (byVersion.TryGetValue(version, out var possible))
                                {
                                    var finalList = returnFunc(possible);
                                    SavedTara.TryAdd(first, finalList);
                                    return finalList;
                                }
                                else
                                {
                                    // На случай если функционала ВООБЩЕ НЕТ
                                    if (false/*byVersion.TryGetValue(0, out var possible2)*/)
                                    {
                                        //var finalList = returnFunc(possible2);
                                        //SavedTara.TryAdd(first, finalList);
                                        //return finalList;
                                    }
                                    else
                                    {
                                        // Сразу ошибка
                                        dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoVersion, new DataErrorStruct());
                                        return prodError();
                                    }
                                }
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
                            if (lvl1.TryGetValue(Uco.EmptyObject, out var lvl22))
                            {
                                if (lvl22.TryGetValue(first.OperationVid, out var byVersion))
                                {
                                    if (byVersion.TryGetValue(version, out var possible))
                                    {
                                        var finalList = returnFunc(possible);
                                        SavedTara.TryAdd(first, finalList);
                                        return finalList;
                                    }
                                    else
                                    {
                                        // На случай если функционала ВООБЩЕ НЕТ
                                        if (false/*byVersion.TryGetValue(0, out var possible2)*/)
                                        {
                                            //var finalList = returnFunc(possible2);
                                            //SavedTara.TryAdd(first, finalList);
                                            //return finalList;
                                        }
                                        else
                                        {
                                            // Сразу ошибка
                                            dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoVersion, new DataErrorStruct());
                                            return prodError();
                                        }
                                    }
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
                                dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoCharacteristics, new DataErrorStruct());
                                return prodError();
                            }
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
                //bool b1_2 = TarasMaxCapacities.TryGetValue(Uco.EmptyObject, out var lvl0_1);
                //bool b1 = b1_1 || b1_2;
                //lvl0 = b1_1 && b1_2 ? lvl0_0.Union(lvl0_1).ToDictionary(x => x.Key, x => x.Value) : (b1_1 ? lvl0_0 : (b1_2 ? lvl0_1 : null));
                bool b2 = false;

                Dictionary<Uco,
                    Dictionary<Uco,
                        Dictionary<int,
                            Dictionary<Uco,
                                List<(DimensionUnit Format, double Value)>
                            >
                        >
                    >
                > lvl1 = null;


                Dictionary<Uco,
                    Dictionary<int,
                        Dictionary<Uco,
                            List<(DimensionUnit Format, double Value)>
                        >
                    >
                > lvl11 = null;

                Dictionary<int,
                    Dictionary<Uco,
                        List<(DimensionUnit Format, double Value)>
                    >
                > byVersion = null;

                Dictionary<Uco, // ТипТары
                    List<(DimensionUnit Format, double Value)> // Намотка (формат и значения)
                    > possible = null;

                if (b1) b2 = lvl0.TryGetValue(first.Nomenklature, out lvl1);

                bool b22 = false;
                if (b2)
                {
                    b22 = lvl1.TryGetValue(first.Characteristics, out lvl11);
                    if (!b22) b22 = lvl1.TryGetValue(Uco.EmptyObject, out lvl11);
                }

                bool b3 = false;
                if (b22)
                {
                    b3 = lvl11.TryGetValue(first.OperationVid, out byVersion);
                    if (!b3) b3 = lvl11.TryGetValue(Uco.EmptyObject, out byVersion);
                }

                bool b33 = false;
                if (b3)
                {
                    b33 = byVersion.TryGetValue(version, out possible);
                    //if (!b33) b33 = byVersion.TryGetValue(0, out possible);
                }

                if (b1 && b2 && b22 && b3 && b33)
                {
                    finalList.AddRange(returnFunc(possible));
                }

                bool b4 = TarasMaxCapacities.TryGetValue(Uco.EmptyObject, out lvl0);
                bool b5 = false;
                if (b4) b5 = lvl0.TryGetValue(first.Nomenklature, out lvl1);

                bool b55 = false;
                if (b5)
                {
                    b55 = lvl1.TryGetValue(first.Characteristics, out lvl11);
                    if (!b55) b55 = lvl1.TryGetValue(Uco.EmptyObject, out lvl11);
                }


                bool b6 = false;
                if (b55)
                {
                    b6 = lvl11.TryGetValue(first.OperationVid, out byVersion);
                    if (!b6) b6 = lvl11.TryGetValue(Uco.EmptyObject, out byVersion);
                }


                bool b66 = false;
                if (b6)
                {
                    b66 = byVersion.TryGetValue(version, out possible);
                    //if (!b66) b66 = byVersion.TryGetValue(0, out possible);
                }

                if (b4 && b5 && b55 && b6 && b66)
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

                    if (!b22) dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoCharacteristics, new DataErrorStruct());
                    if (!b55) dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoCharacteristicsEmptyProd, new DataErrorStruct());

                    if (!b33) dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoVersion, new DataErrorStruct());
                    if (!b66) dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoVersionEmptyProd, new DataErrorStruct());

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

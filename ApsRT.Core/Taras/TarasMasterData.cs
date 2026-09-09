using ApsRT.Core.DataErrors;
using ApsRT.Core.Session;
using Azure;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.Protobuf.Compiler;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using UniversalPack.Extensions;
using static Azure.Core.HttpHeader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;

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

            TaraStrToGuid.TryAdd(model.TaraType.Label, model.TaraType.Code);

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
                            Dictionary<OrderID, // ЗАКАЗ
                                Dictionary<Uco, // ТипТары
                                    List<(DimensionUnit Format, double Value)>
                                >
                            >
                        >
                    >
                >
            >
        > TarasMaxCapacities
        { get; private set; } = new();

        Dictionary<string, Guid> ProdStrToGuid = new();
        Dictionary<string, Guid> NomenStrToGuid = new();
        Dictionary<string, Guid> CharStrToGuid = new();
        Dictionary<string, Guid> VidStrToGuid = new();
        Dictionary<string, Guid> OrderStrToGuid = new();
        Dictionary<string, Guid> TaraStrToGuid = new();

        public void FillCustomDataFromOper(Stages.Stage op)
        {
            ProdStrToGuid.TryAdd(op.Production.Label, op.Production.Code);
            NomenStrToGuid.TryAdd(op.Nomenklature.Label, op.Nomenklature.Code);
            if (!string.IsNullOrEmpty(op.Characteristics.Label)) CharStrToGuid.TryAdd(op.Characteristics.Label, op.Characteristics.Code);
            VidStrToGuid.TryAdd(op.OperationVid.Label, op.OperationVid.Code);
            OrderStrToGuid.TryAdd(op.OrderLink.OrderID.Object.Label, op.OrderLink.OrderID.Object.Code);
        }

        public string CustomMaxCapacitiesFile = "customMax.xlsx";

        public void CustomMaxCapacityPostProcess()
        {
            ProdStrToGuid.TryAdd("", Guid.Empty);
            NomenStrToGuid.TryAdd("", Guid.Empty);
            CharStrToGuid.TryAdd("", Guid.Empty);
            VidStrToGuid.TryAdd("", Guid.Empty);
            OrderStrToGuid.TryAdd("", Guid.Empty);
            TaraStrToGuid.TryAdd("", Guid.Empty);

            // Сначала сохраняем файл
            if (!File.Exists(CustomMaxCapacitiesFile))
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Намотки");

                    worksheet.Row(1).Cell(1).Value = "Продукция";
                    //worksheet.Row(1).Cell(2).Value = "Продукция_GUID";
                    worksheet.Row(1).Cell(2).Value = "Номенклатура";
                    //worksheet.Row(1).Cell(4).Value = "Номенклатура_GUID";
                    worksheet.Row(1).Cell(3).Value = "ХарактеристикаНоменклатуры";
                    //worksheet.Row(1).Cell(6).Value = "ХарактеристикаНоменклатуры_GUID";

                    int i = 4;
                    if (SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput)
                    {
                        worksheet.Row(1).Cell(i).Value = "НомерОперации";
                        i++;
                    }

                    worksheet.Row(1).Cell(i).Value = "ВидОперации";
                    //worksheet.Row(1).Cell(i + 1).Value = "ВидОперации_GUID";
                    worksheet.Row(1).Cell(i + 1).Value = "Заказ";
                    //worksheet.Row(1).Cell(i + 3).Value = "Заказ_GUID";
                    worksheet.Row(1).Cell(i + 2).Value = "ПартияГП";
                    worksheet.Row(1).Cell(i + 3).Value = "ТипТары";
                    //worksheet.Row(1).Cell(i + 6).Value = "ТипТары_GUID";
                    worksheet.Row(1).Cell(i + 4).Value = "ВеличинаНамотки";

                    workbook.SaveAs(CustomMaxCapacitiesFile);
                }
            }

            // Теперь читаем
            try
            {
                using (var fileStream = new FileStream(CustomMaxCapacitiesFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (XLWorkbook xLWorkbook = new XLWorkbook(fileStream))
                    {
                        var firstSheet = xLWorkbook.Worksheets.First();
                        int populatedRowsCount = firstSheet.RowsUsed().Count();

                        if (populatedRowsCount > 1)
                        {
                            // Считываем всё из файла

                            var columnLetters = firstSheet.ColumnsUsed().Select((col, i) => (name: col.FirstCell().Value.ToString(), index: i)).ToDictionary(x => x.name, x => x.index + 1);

                            int stageVerIndex = columnLetters.TryGetValue("НомерОперации", out var i0) ? i0 : -1;

                            int operVIndex = columnLetters.TryGetValue("ВидОперации", out var i1) ? i1 : -1;
                            //int operVGIndex = columnLetters.TryGetValue("ВидОперации_GUID", out var i2) ? i2 : -1;

                            int CharIndex = columnLetters.TryGetValue("ХарактеристикаНоменклатуры", out var i3) ? i3 : -1;
                            //int CharGIndex = columnLetters.TryGetValue("ХарактеристикаНоменклатуры_GUID", out var i4) ? i4 : -1;

                            int NIndex = columnLetters.TryGetValue("Номенклатура", out var i5) ? i5 : -1;
                            //int NGIndex = columnLetters.TryGetValue("Номенклатура_GUID", out var i6) ? i6 : -1;

                            int PIndex = columnLetters.TryGetValue("Продукция", out var i7) ? i7 : -1;
                            //int PGIndex = columnLetters.TryGetValue("Продукция_GUID", out var i8) ? i8 : -1;

                            int TIndex = columnLetters.TryGetValue("ТипТары", out var i9) ? i9 : -1;
                            //int TGIndex = columnLetters.TryGetValue("ТипТары_GUID", out var i10) ? i10 : -1;

                            int aIndex = columnLetters.TryGetValue("ВеличинаНамотки", out var i11) ? i11 : -1;

                            int ZIndex = columnLetters.TryGetValue("Заказ", out var i12) ? i12 : -1;
                            //int ZGIndex = columnLetters.TryGetValue("Заказ_GUID", out var i13) ? i13 : -1;

                            int ZGPIndex = columnLetters.TryGetValue("ПартияГП", out var i14) ? i14 : -1;

                            List<(Uco pr, Uco nom, Uco cha, Uco vid, int ver, OrderID ord, Uco tar, double val)> RawList = new();

                            for (int i = 2; i <= populatedRowsCount; i++)
                            {
                                try
                                {
                                    string prod = firstSheet.Row(i).Cell(PIndex).GetValue<string>();
                                    Uco production = new Uco(ProdStrToGuid[prod], prod);

                                    string nomen = firstSheet.Row(i).Cell(NIndex).GetValue<string>();
                                    Uco Nomen = new Uco(NomenStrToGuid[nomen], nomen);

                                    string chars = CharIndex == -1 ? "" : firstSheet.Row(i).Cell(CharIndex).GetValue<string>();
                                    Uco Chars = CharIndex == -1 ? Uco.EmptyObject : new Uco(CharStrToGuid[chars], chars);

                                    string vid = firstSheet.Row(i).Cell(operVIndex).GetValue<string>();
                                    Uco oper_int = new Uco(VidStrToGuid[vid], vid);

                                    var version = stageVerIndex == -1 ? 1 : firstSheet.Row(i).Cell(stageVerIndex).GetValue<int>();

                                    string ord = firstSheet.Row(i).Cell(ZIndex).GetValue<string>();
                                    OrderID order = new OrderID(new Uco(OrderStrToGuid[ord], ord), firstSheet.Row(i).Cell(ZGPIndex).GetValue<string>());

                                    string tar = firstSheet.Row(i).Cell(TIndex).GetValue<string>();
                                    Uco tara = new Uco(TaraStrToGuid[tar], tar);

                                    double amount = firstSheet.Row(i).Cell(aIndex).GetValue<double>();

                                    RawList.Add((production, Nomen, Chars, oper_int, version, order, tara, amount));
                                }
                                catch (Exception ex)
                                {
                                    string error = $"Не удалось прочитать строку {i} кастомных макс намоток: " + ex.ToString();
                                    Console.WriteLine(error);
                                    SessionLink.AssignedLogger.LogQueue.Enqueue(error + "\r\n");
                                }
                            }

                            var groups = RawList.GroupBy(x => (x.pr, x.nom, x.cha, x.vid, x.ver, x.ord));

                            foreach (var group in groups)
                            {
                                var production = group.Key.pr;
                                var Nomen = group.Key.nom;
                                var Chars = group.Key.cha;
                                var oper_int = group.Key.vid;
                                var version = group.Key.ver;
                                var order = group.Key.ord;
                                if (version <= 0) version = 1;

                                // Убеждаемся, что есть словарь, и ОЧИЩАЕМ его сразу же
                                EnsureMaxCapacityDics(production, Nomen, Chars, oper_int, version, order, true);

                                foreach (var value in group)
                                {
                                    var amount = value.val;
                                    var tara = value.tar;

                                    // Добавляем ТАРУ
                                    AddMaxCapacityData(production, Nomen, Chars, oper_int, version, order, DimensionUnit.Unknown, amount, tara);
                                }

                            }

                        }

                    }
                }
                
            }
            catch (Exception ex)
            {
                string error = "Не удалось прочитать файл кастомных макс намоток: " + ex.ToString();
                Console.WriteLine(error);
                SessionLink.AssignedLogger.LogQueue.Enqueue(error + "\r\n");
            }
        }

        void EnsureMaxCapacityDics(Uco production, Uco Nomen, Uco Chars, Uco oper_int, int version, OrderID order, bool clear)
        {
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

            if (!TarasMaxCapacities[production][Nomen][Chars][oper_int].ContainsKey(version))
            {
                TarasMaxCapacities[production][Nomen][Chars][oper_int].Add(version, new());
            }

            if (!TarasMaxCapacities[production][Nomen][Chars][oper_int][version].ContainsKey(order))
            {
                TarasMaxCapacities[production][Nomen][Chars][oper_int][version].Add(order, new());
            }

            if (clear) TarasMaxCapacities[production][Nomen][Chars][oper_int][version][order].Clear();

            ProdStrToGuid.TryAdd(production.Label, production.Code);
            NomenStrToGuid.TryAdd(Nomen.Label, Nomen.Code);
            if (!string.IsNullOrEmpty(Chars.Label)) CharStrToGuid.TryAdd(Chars.Label, Chars.Code);
            VidStrToGuid.TryAdd(oper_int.Label, oper_int.Code);
        }

        public void AddMaxCapacityData(Uco production, Uco Nomen, Uco Chars, Uco oper_int, int version, OrderID order, DimensionUnit unit, double amount, Uco tara)
        {
            var what = (unit, amount);

            Add2(tara, what);

            if (SessionLink.SettingsController.CurrentSettings.UsePseudoTara)
            {
                Add2(Pseudotara1, (unit, 100000));
                Add2(Pseudotara2, (unit, 100000));
                Add2(Pseudotara3, (unit, 100000));
            }

            void Add2(Uco whichTara, (DimensionUnit, double) val)
            {
                var lvl = TarasMaxCapacities[production][Nomen][Chars][oper_int][version][order];

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

        public void AddMaxCapacityData(MaxCapacityDataLoaderModel model)
        {
            var production = model.Production;
            var Nomen = model.Nomenklature;
            var Chars = model.Characteristics;
            var oper_int = model.OperationVid;
            var version = model.StageVersion;
            var order = OrderID.EmptyObject;
            if (version <= 0) version = 1;

            EnsureMaxCapacityDics(production, Nomen, Chars, oper_int, version, order, false);

            AddMaxCapacityData(production, Nomen, Chars, oper_int, version, order, model.DimensionUnit, model.Amount, model.TaraType);

            TaraStrToGuid.TryAdd(model.TaraType.Label, model.TaraType.Code);
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
                            Dictionary<OrderID,
                                Dictionary<Uco,
                                    List<(DimensionUnit Format, double Value)>
                                >
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
                                if (byVersion.TryGetValue(version, out var byOrder))
                                {
                                    if (byOrder.TryGetValue(first.OrderLink.OrderID, out var possible))
                                    {
                                        var finalList = returnFunc(possible);
                                        SavedTara.TryAdd(first, finalList);
                                        return finalList;
                                    }
                                    else
                                    {
                                        if (byOrder.TryGetValue(OrderID.EmptyObject, out var possible2))
                                        {
                                            var finalList = returnFunc(possible2);
                                            SavedTara.TryAdd(first, finalList);
                                            return finalList;
                                        }
                                        else
                                        {
                                            // Сразу ошибка - если пустого заказа нету, это значит, что вся эта цепочка не вела никуда, а значит, всё началось с продукции.
                                            dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoProduction, new DataErrorStruct());
                                            return prodError();
                                        }
                                    }

                                }
                                else
                                {
                                    // Сразу ошибка
                                    dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoVersion, new DataErrorStruct());
                                    return prodError();
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
                                    if (byVersion.TryGetValue(version, out var byOrder))
                                    {
                                        if (byOrder.TryGetValue(first.OrderLink.OrderID, out var possible))
                                        {
                                            var finalList = returnFunc(possible);
                                            SavedTara.TryAdd(first, finalList);
                                            return finalList;
                                        }
                                        else
                                        {
                                            if (byOrder.TryGetValue(OrderID.EmptyObject, out var possible2))
                                            {
                                                var finalList = returnFunc(possible2);
                                                SavedTara.TryAdd(first, finalList);
                                                return finalList;
                                            }
                                            else
                                            {
                                                // Сразу ошибка - если пустого заказа нету, это значит, что вся эта цепочка не вела никуда, а значит, всё началось с продукции.
                                                dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoProduction, new DataErrorStruct());
                                                return prodError();
                                            }
                                        }

                                    }
                                    else
                                    {
                                        // Сразу ошибка
                                        dataErrors.AddNewPartiaError(first.Index, NewDataErrorType.NoVersion, new DataErrorStruct());
                                        return prodError();
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
                            Dictionary<OrderID,
                                Dictionary<Uco,
                                    List<(DimensionUnit Format, double Value)>
                                >
                            >
                        >
                    >
                > lvl1 = null;


                Dictionary<Uco,
                    Dictionary<int,
                        Dictionary<OrderID,
                            Dictionary<Uco,
                                List<(DimensionUnit Format, double Value)>
                            >
                        >
                    >
                > lvl11 = null;

                Dictionary<int,
                    Dictionary<OrderID,
                            Dictionary<Uco,
                                List<(DimensionUnit Format, double Value)>
                            >
                    >
                > byVersion = null;

                Dictionary<OrderID, 
                            Dictionary<Uco, // ТипТары
                                List<(DimensionUnit Format, double Value)> // Намотка (формат и значения)
                            >
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
                    if (possible.TryGetValue(first.OrderLink.OrderID, out var withOrder))
                    {
                        finalList.AddRange(returnFunc(withOrder));
                    }
                    else
                    {
                        if (possible.TryGetValue(OrderID.EmptyObject, out var nOrder))
                        {
                            finalList.AddRange(returnFunc(nOrder));
                        }
                        else
                        {
                            // Обнуляем вообще всё
                            b1 = false;
                            b2 = false;
                            b22 = false;
                            b3 = false;
                            b33 = false;
                        }
                    }

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
                    if (possible.TryGetValue(first.OrderLink.OrderID, out var withOrder))
                    {
                        finalList.AddRange(returnFunc(withOrder));
                    }
                    else
                    {
                        if (possible.TryGetValue(OrderID.EmptyObject, out var nOrder))
                        {
                            finalList.AddRange(returnFunc(nOrder));
                        }
                        else
                        {
                            // Обнуляем вообще всё
                            b4 = false;
                            b5 = false;
                            b55 = false;
                            b6 = false;
                            b66 = false;
                        }
                    }
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
                var res = possibl.SelectMany(x => x.Value.Select(y => (x.Key, y.Format, y.Value))).ToList();
                return res;
            }
        }
    }
}

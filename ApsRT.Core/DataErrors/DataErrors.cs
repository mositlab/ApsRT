using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApsRT.Core.Session;
using ApsRT.Core.Settings;
using UniversalPack;

namespace ApsRT.Core.DataErrors
{
    public partial class DataErrors
    {

        public DataErrors(Session.Session session)
        {
            this.session = session;
        }

        Session.Session session { get; init; }


        //public string PartiasErrorsFile = "partiasErrors.csv";
        //public string PartiasErrorsBigFile = "partiasErrorsBig.csv";
        public string DatasErrorsFile = "dataErrors.csv";
        public string DatasErrorsBigFile = "dataErrorsBig.csv";

        public ConcurrentDictionary<int, Dictionary<NewDataErrorType, DataErrorStruct>> NewDataErrors = new();

        object lockerHere = new();
        public void AddNewPartiaError(int ID, NewDataErrorType error, DataErrorStruct errorData)
        {
            if (!NewDataErrors.ContainsKey(ID))
            {
                NewDataErrors.TryAdd(ID, new());
            }
            lock (lockerHere) // Проседающая залупень, нужно как-то переписать. Нужен АНДРЕЙ
            {
                if (!NewDataErrors[ID].ContainsKey(error)) NewDataErrors[ID].Add(error, errorData);
            }
        }

        public ConcurrentBag<(NewDataErrorType type, DataErrorStruct data)> NewDataErrorsNoStages = new();

        public void AddNewPartiaError(NewDataErrorType error, string errorData)
        {
            NewDataErrorsNoStages.Add((error, new DataErrorStruct(errorData)));
            if (!NewDataErrors.ContainsKey(-1))
            {
                NewDataErrors.TryAdd(-1, new());
            }
        }

        internal bool TryOutputDataErrors()
        {
            bool HasErrors = false;
            //var sw = new StreamWriter(PartiasErrorsFile, false, Encoding.UTF8);
            //var swBig = new StreamWriter(PartiasErrorsBigFile, false, Encoding.UTF8);

            session.SessionOptions.GetPreparedOutputPath(out var outputPath);

            var swNew = new StreamWriter(outputPath + DatasErrorsFile, false, Encoding.UTF8);
            StreamWriter swNewBig = null;
            try
            {
                swNewBig = new StreamWriter(outputPath + DatasErrorsBigFile, false, Encoding.UTF8);
            }
            catch
            {
            }
            //sw.WriteLine("Заказ;ГП;ИдентификаторПартииПФ;Этап;Причина");
            //swBig.WriteLine("Заказ;ГП;ИдентификаторПартииПФ;Этап;Номенклатура;Вид;Продукция;Причина");

            string BtoS(bool b) => b ? "ДА" : "НЕТ";

            bool yes1 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.RCsOnThisEtap != null && x.RCsOnThisEtap.Count > 0);
            bool yes2 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.TarasOnThisEtap != null && x.TarasOnThisEtap.Count > 0);
            bool yes3 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.NeededOtdTarasOnThisEtap != null && x.NeededOtdTarasOnThisEtap.Count > 0);
            bool yes4 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.MaxNamotka != null && x.MaxNamotka.Count > 0);
            bool yes5 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.MaxPlusUsedTaras != null && x.MaxPlusUsedTaras.Count > 0);
            bool yes51 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.RCsOnNextEtap != null && x.RCsOnNextEtap.Count > 0);
            bool yes52 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.TarasOnNextEtap != null && x.TarasOnNextEtap.Count > 0);
            bool yes53 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.PartiaSettings != null);
            bool yes54 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.PartiaGuid != null);
            bool yesCOL = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.customEtapsGuids != null && x.customEtapsGuids.Count > 0);
            bool yesCRC = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.customRC != null);
            bool yesCRCR = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.customRCroute != null);
            bool yesdOP = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.TechData != null);
            bool yes6 = NewDataErrors.Any(x => x.Key >= 0);


            bool yesCharNom = NewDataErrors.Any(x => x.Key >= 0 /*&& allStages[x.Key]*/);
            bool yesCharProd = NewDataErrors.Any(x => x.Key >= 0 /*&& allStages[x.Key]*/);

            var allStages = session.MasterDataLinks.StagesMasterData.AllStages;

            bool yes61 = NewDataErrors.Any(x => x.Key >= 0 && allStages[x.Key].RCRoute.IsEmpty);
            bool yes62 = NewDataErrors.Any(x => x.Key >= 0 && !allStages[x.Key].RCRoute.IsEmpty);

            bool yes55 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.RCsOnPrevEtap != null && x.RCsOnPrevEtap.Count > 0);
            bool yes56 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.TarasOnPrevEtap != null && x.TarasOnPrevEtap.Count > 0);

            bool yesPrevs = NewDataErrors.Any(x => x.Key >= 0 && allStages[x.Key].PrevsNormCount > 0);
            bool yesNexts = NewDataErrors.Any(x => x.Key >= 0 && allStages[x.Key].Nexts.Count > 0);

            bool yesCount = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.Count != 0);
            bool yesNalad = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.NaladLength != 0);

            bool yesMod = NewDataErrors.Any(x => x.Key >= 0 && x.Value.Any(y => y.Value.Count != 0) && allStages[x.Key].MesModelling.Code != Guid.Empty);

            bool yesCProd = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.customProd != null);
            bool yesCNom = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.customNomen != null);
            bool yesCVid = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.customVid != null);

            var str1 = $"{BtoS(yes1)};{BtoS(yes2)};{BtoS(yes3)};{BtoS(yes4)};{BtoS(yes5)};{BtoS(yes51)};{BtoS(yes52)};" +
                $"{BtoS(yes55)};{BtoS(yes56)};" +
                $"{BtoS(yes6 || yesCOL)};" +
                $"{(session.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{BtoS(yes6 || yesCOL)};" : "")}" +
                $"{BtoS(yesPrevs)};" +
                $"{(session.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{BtoS(yesPrevs)};" : "")}" +
                $"{BtoS(yesNexts)};" +
                $"{(session.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{BtoS(yesNexts)};" : "")}" +
                $"{BtoS(yes6)};{BtoS(yes6 || yesCProd)};{BtoS(yes6 || yesCNom)};{(session.SettingsController.CurrentSettings.Demos_CharacteristicsOnOutput ? $"{BtoS(yes6 && yesCharNom)};{BtoS(yes6 && yesCharProd)};" : "")}{BtoS(yes6 || yesCVid)};" +
                $"{BtoS(yes61 || yesCRC)};{BtoS(yes62 || yesCRCR)};" +
                $"{BtoS(yes6)};{BtoS(yes6)};{BtoS(yes6 || yes54 || true)};" +
                $"{BtoS(yes53)};" +
                $"{BtoS(yesCount)};{BtoS(yesNalad)};{BtoS(yesMod)};" +// $"{BtoS(yes6 || yesCount)};{BtoS(yesMod)};" +
                $"ДА;{BtoS(yesdOP)}";
            var str2 = "ДоступныеРЦТекущегоЭтапа;ПриемнаяТараТекущегоЭтапа;ТребуемаяОтдающаяТара;МаксНамоткиТекущегоЭтапа;ПересечениеМаксНамотокПриемнойТары;РЦСледующегоЭтапа;ТараСледующегоЭтапа;" +
                "РЦПредыдущегоЭтапа;ТараПредыдущегоЭтапа;" +
                $"Этап;{(session.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсияЭтапа;" : "")}ПредыдущиеЭтапы;{(session.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсииПредыдущихЭтапов;" : "")}СледующиеЭтапы;{(session.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсииСледующихЭтапов;" : "")}" +
                $"МетодРасчёта;Продукция;Номенклатура;{(session.SettingsController.CurrentSettings.Demos_CharacteristicsOnOutput ? "ХарактеристикаНоменклатуры;ХарактеристикаПродукции;" : "")}ВидОперации;" +
                "РЦ;ГруппаЗаменяемостиРабочихЦентров;" +
                "Заказ;ПартияГП;ИдентификаторПартииПФ;" +
                "НастройкиФормированияПартий;" +
                "Количество;НаладочнаяДлина;Моделирование;" +
                "Причина;ДополнительнаяИнформация";

            swNew.WriteLine(str1);
            swNew.WriteLine(str2);
            swNewBig?.WriteLine(str1);
            swNewBig?.WriteLine(str2);

            if (NewDataErrors.Any())
            {
                HasErrors = true;
                foreach (var element in NewDataErrors)
                {
                    var errorsFromNoStages = NewDataErrorsNoStages.Select(x => new KeyValuePair<NewDataErrorType, DataErrorStruct>(x.type, x.data));
                    var list = element.Value.ToList();
                    if (element.Key == -1) list.AddRange(errorsFromNoStages);

                    //var list = element.Key == -1 ? .AddRange().ToArray() : element.Value.ToArray();

                    foreach (var error in list)
                    {
                        var message = error.Key switch
                        {
            
                            NewDataErrorType.NoProduction => (allStages[element.Key].UsedCalcMethod == DimensionUnit.m ? "По данной продукции не найдена максимальная намотка с методом расчета по метражу." : "По данной номенклатуре и тех. операции не найдена максимальная намотка с методом расчёта по весу."),
                            NewDataErrorType.NoNomenklature => "По данной продукции не найдена максимальная намотка по указанной номенклатуре.",
                            NewDataErrorType.NoCharacteristics => "По данной продукции и номенклатуре не найдена максимальная намотка по указанной характеристике.",
                            NewDataErrorType.NoVidOper => "По данной продукции, номенклатуре и характеристике не найдена максимальная намотка по виду операции.",
                            NewDataErrorType.NoVersion => "По данной продукции, номенклатуре, характеристике и виду операции не найдена максимальная намотка по номеру операции.",

                            NewDataErrorType.NoNomenklatureEmptyProd => "По пустой продукции не найдена максимальная намотка по указанной номенклатуре с методом расчёта по весу.",
                            NewDataErrorType.NoVidOperEmptyProd => "По номенклатуре, характеристике и пустой продукции не найдена максимальная намотка по виду операции с методом расчёта по весу.",
                            NewDataErrorType.NoCharacteristicsEmptyProd => "По номенклатуре и пустой продукции не найдена максимальная намотка по характеристике с методом расчёта по весу.",
                            NewDataErrorType.NoVersionEmptyProd => "По номенклатуре, характеристике, виду операции и пустой продукции не найдена максимальная намотка по номеру операции с методом расчёта по весу.",

                            NewDataErrorType.NoGP => "У партии не задан номер ГП.",

                            NewDataErrorType.Restricted_VPZ => "По партии выдано ПЗ, партию нельзя пересобрать. Обратитесь к ПДО.",
                            NewDataErrorType.Restricted_Fact => "По партии есть отражение факта, партию нельзя пересобрать. Обратитесь к ПДО.",
                            NewDataErrorType.Restricted_NoReplan => "По партии есть \"Не перепланировать\", партию нельзя пересобрать. Обратитесь к ПДО.",

                            NewDataErrorType.NoOrigTara => "Для ГП не задана тара готовой продукции в заказе на производство. Обратитесь к ПДО.",

                            NewDataErrorType.NoChangeTaraNoPriem => "Не найдена приемная тара для РЦ с признаком \"Не менять тару\". Требуется добавить 'требуемую отдающую' тару в качестве приёмной в НСИ.",
                            NewDataErrorType.NoChangeTaraNoPriemVspom => "Не найдена приемная вспомогательная тара для РЦ с признаком \"Не менять тару\". Требуется добавить 'требуемую отдающую' тару в качестве приёмной в НСИ.",
                            NewDataErrorType.NoChangeTaraMoreThanOne => "АПС затребовал 2 или больше тары с признаком \"Не менять тару\". Обратитесь в ОАУП.",

                            NewDataErrorType.NonPositiveLength => "Этап имеет отрицательную длительность: '{0}'.",
                            //NewDataErrorType.EmptyOpersTable => "Таблица операций пуста.",
                            NewDataErrorType.BadStartTimeFact => "Этап имеет ВремяНачалаФакт ранее 2000-го года: '{0}'.",

                            NewDataErrorType.SomeFileIsOpened => "Некоторые файлы открыты другим процессом/пользователем и недоступны для записи.",//: {0}.",

                            NewDataErrorType.CorruptedDataFile => "Ошибка при загрузке модуля данных '{0}'.",

                            NewDataErrorType.EmptyDataTable => "Пустая таблица модуля данных '{0}'.",

                            NewDataErrorType.EmptyPropType => "Неизвестный тип реквизита для '{0}'.",
                            NewDataErrorType.UnknownPropType => "Неизвестный тип реквизита для '{0}'.",

                            NewDataErrorType.CantLinkProp => "Не удаётся создать ссылку на реквизит для '{0}'.",

                            NewDataErrorType.UnknownPartiaParam => "Неизвестная настройка '{0}'.",

                            NewDataErrorType.NotEnoughMaxOnTaras => "Не найдена основная тара для рассчитанного количества с учетом выбранной тары по макс намотке и приемной таре по цепочке по таре (количество этапа слишком велико).",
                            NewDataErrorType.NotEnoughMaxOnTarasVspom => "Не найдена вспомогательная тара для рассчитанного количества с учетом выбранной тары по макс намотке и приемной таре по цепочке по таре (количество этапа слишком велико).",

                            NewDataErrorType.NoTaraNMaxIntersection => "Не найдена основная тара для рассчитанного количества среди максимальной намотки и применяемой приемной тары (нет пересечения).",

                            NewDataErrorType.NoTaraNMaxIntersectionVspom => "Не найдена вспомогательная тара для рассчитанного количества среди максимальной намотки и применяемой приемной тары (нет пересечения).",

                            NewDataErrorType.NoPriemNOtdIntersection => "Нарушение цепочки 'приемная-отдающая тара' по основной таре.",
                            NewDataErrorType.NoPriemNOtdIntersectionVspom => "Нарушение цепочки 'приемная-отдающая тара' по вспомогательной таре.",
                            NewDataErrorType.NoPriemNOtdIntersectionNoChange => "Нарущение цепочки 'приемная-отдающая' тара по основной таре с учетом РЦ  с признаком \"Не менять тару\".",

                            NewDataErrorType.NoPriemOrigTara => "Для барабана ГП '{0}' не найден РЦ с данной применяемой приёмной тарой.",
                            NewDataErrorType.NoOtdOrigTara => "На следующей стадии для барабана ГП '{0}' не найден РЦ с данной применяемой отдающей тарой.",
                            NewDataErrorType.NoPriemOrigTaraOnNextEtap => "На следующей стадии для барабана ГП '{0}' не найден РЦ с данной применяемой приёмной тарой.",
                            //NewDataErrorType.NoOtdOrigTara => "Для барабана ГП '{0}' не найден РЦ с данной применяемой отдающей тарой.",

                            NewDataErrorType.NoAnyRCWithTara => "У этапа нет ни одного РЦ с заданной приёмной тарой.",

                            NewDataErrorType.WasNotAbleToMerge => "На 1м уровне не выполняются настройки формирования партий.",

                            NewDataErrorType.WasNotAbleToMergeNoSettings => "Для данного вида операции отсутствуют настройки формирования партий.",

                            NewDataErrorType.WasNotAbleToMergeMax => "На 1м уровне не удалось объединить этапы по максимальной намотке. Обратитесь к ПДО.",

                            NewDataErrorType.WasNotAbleToMergeDontKnow => "Не удалось объединить партии по неизвестной причине. Обратитесь в ОАУП.",

                            NewDataErrorType.NoAllNeededOtdTaras => "У этапа нет ни одного РЦ, у которого есть все перечисленные требуемые отдающие тары.",

                            NewDataErrorType.NoAllNeededOtdTarasWithCheck => "У этапа нет ни одного РЦ, у которого есть все перечисленные требуемые отдающие тары (deep-check).",

                            NewDataErrorType.LivenshteinBadOper => "Обнаружено объединение, не связанное с объединенными этапами.",

                            NewDataErrorType.NoNeededNoChangeFinalTaraOnChildMoreThanOne => "На следующих этапах, имеющих признак 'не менять тару', заданы различные конечные тары.",
                            NewDataErrorType.NoNeededNoChangeFinalTaraOnChild => "На следующем этапе, имеющем признак 'не менять тару', задана конечная тара, которой нет в списке приёмных тар на текущем уровне.",

                            _ => "Необработанная ошибка. Обратитесь в ОАУП."

                        };

                        message = string.Format(message, error.Value.DataToInsert ?? "");
                        string list1 = ""; if (error.Value.RCsOnThisEtap != null) list1 = string.Join(", ", error.Value.RCsOnThisEtap.Select(X => X.Label));
                        string list2 = ""; if (error.Value.TarasOnThisEtap != null) list2 = string.Join(", ", error.Value.TarasOnThisEtap.Select(X => X.Label));


                        string list3 = "";
                        if (error.Value.NeededOtdTarasOnThisEtap != null && error.Value.NeededOtdTarasOnThisEtap.Count > 0) list3 = string.Join(", ", error.Value.NeededOtdTarasOnThisEtap.Select(X => X.Tara.Label + (X.WasVspom ? " (вспом.)" : "")));
                        else
                        {
                            if (error.Value.NeededOtdTarasOnThisEtapAnyBad) list3 = "Требуемые отдающие тары не определены из-за ошибок на предыдущих этапах.";
                        }

                        if (string.IsNullOrEmpty(list3) && element.Key >= 0 && allStages[element.Key].OrderLink.OrderID.Object.Label.Contains("338"))
                        {
                            ;
                        }

                            string list4 = ""; if (error.Value.MaxNamotka != null)
                            list4 = string.Join(", ", error.Value.MaxNamotka.Select(X => $"{X.Tara.Label} ({X.Max})"));
                        string list5 = ""; if (error.Value.MaxPlusUsedTaras != null) list5 = string.Join(", ", error.Value.MaxPlusUsedTaras.Select(X => X.Label));
                        string list51 = ""; if (error.Value.RCsOnNextEtap != null) list51 = string.Join(", ", error.Value.RCsOnNextEtap.Select(X => X.Label));
                        string list52 = ""; if (error.Value.TarasOnNextEtap != null) list52 = string.Join(", ", error.Value.TarasOnNextEtap.Select(X => X.Label));

                        string list53 = ""; if (error.Value.RCsOnPrevEtap != null) list53 = string.Join(", ", error.Value.RCsOnPrevEtap.Select(X => X.Label));
                        string list54 = ""; if (error.Value.TarasOnPrevEtap != null) list54 = string.Join(", ", error.Value.TarasOnPrevEtap.Select(X => X.Label));

                        string listCOLGuids = ""; if (error.Value.customEtapsGuids != null) listCOLGuids = string.Join(", ", error.Value.customEtapsGuids.Select(x => x.Code));
                        string listCOLGuidsVersions = ""; if (error.Value.customEtapsGuids != null) listCOLGuidsVersions = string.Join(", ", error.Value.customEtapsGuids.Select(x => x.Version));


                        var op = element.Key < 0 ? null : allStages[element.Key];

                        Guid? partiaGuid = op == null ? error.Value.PartiaGuid : (op.CustomPartiaGuid ?? (op.OrderLink.OrderID.Object.Label.Length > 0 ? (op.OrderLink.PartiaPF ?? Guid.Empty) : Guid.Empty));

                        var b1 = $"{(string.IsNullOrEmpty(listCOLGuidsVersions) ? op?.StageVersion : listCOLGuidsVersions)};";
                        var b2 = $"{(op == null ? "" : string.Join(", ", op?.Prevs.Select(x => allStages[x].StageVersion)))};";
                        var b3 = $"{(op == null ? "" : string.Join(", ", op?.Nexts.Select(x => allStages[x].StageVersion)))};";

                        swNew.WriteLine(
                            $"{list1};{list2};{list3};{list4};{list5};{list51};{list52};" +
                            $"{list53};{list54};" +
                            $"{(string.IsNullOrEmpty(listCOLGuids) ? op?.GUID : listCOLGuids)};" +
                        $"{(session.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? b1 : "")}" +
                            $"{(op == null ? "" : string.Join(", ", op?.Prevs.Select(x => allStages[x].GUID)))};" +
                        $"{(session.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? b2 : "")}" +
                            $"{(op == null ? "" : string.Join(", ", op?.Nexts.Select(x => allStages[x].GUID)))};" +
                        $"{(session.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? b3 : "")}" +
                            $"{(op == null ? "" : (op.UsedCalcMethod == DimensionUnit.m ? "По метражу" : "По весу"))};" +
                            $"{(error.Value.customProd.HasValue ? error.Value.customProd.Value.Code : op?.Production.Code)};{(error.Value.customNomen.HasValue ? error.Value.customNomen.Value.Code : op?.Nomenklature.Code)};" +
                            $"{(session.SettingsController.CurrentSettings.Demos_CharacteristicsOnOutput ? $"{op?.Characteristics.Code};{op?.ProductionCharacteristics.Code};" : "")}" +
                            $"{(error.Value.customVid.HasValue ? error.Value.customVid.Value.Code : op?.OperationVid.Code)};" +
                            $"{(error.Value.customRC.HasValue ? error.Value.customRC.Value.Code : (op == null ? "" : (op.RCRoute.IsEmpty ? op.Center.Code : "")))};" +
                            $"{(error.Value.customRCroute.HasValue ? error.Value.customRCroute.Value.Code : (op == null ? "" : (op.RCRoute.IsEmpty ? "" : op.RCRoute.Code)))};" +
                            $"{op?.OrderLink.OrderID.Object.Code};{op?.OrderLink.OrderID.Version};" +
                            $"{partiaGuid};{error.Value.PartiaSettings};" +
                            $"{(error.Value.Count == 0 ? "" : error.Value.Count)};{(error.Value.NaladLength == 0 ? "" : error.Value.NaladLength)};" +
                            $"{(error.Value.Count == 0 || op == null ? "" : (op.MesModelling.Code == Guid.Empty ? "" : op.MesModelling.Code))};" +
                            $"{message};{error.Value.TechData}"
                            );

                        swNewBig?.WriteLine(
                            $"{list1};{list2};{list3};{list4};{list5};{list51};{list52};" +
                            $"{list53};{list54};" +
                            $"{(string.IsNullOrEmpty(listCOLGuids) ? (op?.Registrator + " " + op?.GUID) : listCOLGuids)};" +
                        $"{(session.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? b1 : "")}" +
                            $"{(op == null ? "" : string.Join(", ", op?.Prevs.Select(x => allStages[x].GUID)))};" +
                        $"{(session.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? b2 : "")}" +
                            $"{(op == null ? "" : string.Join(", ", op?.Nexts.Select(x => allStages[x].GUID)))};" +
                        $"{(session.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? b3 : "")}" +
                            $"{(op == null ? "" : (op.UsedCalcMethod == DimensionUnit.m ? "По метражу" : "По весу"))};" +
                            $"{(error.Value.customProd.HasValue ? error.Value.customProd.Value.Label : op?.Production.Label)};{(error.Value.customNomen.HasValue ? error.Value.customNomen.Value.Label : op?.Nomenklature.Label)};" +
                            $"{(session.SettingsController.CurrentSettings.Demos_CharacteristicsOnOutput ? $"{op?.Characteristics.Label};{op?.ProductionCharacteristics.Label};" : "")}" +
                            $"{(error.Value.customVid.HasValue ? error.Value.customVid.Value.Label : op?.OperationVid.Label)};" +
                            //$"{op?.RCRoute.Label};" +
                            $"{(error.Value.customRC.HasValue ? error.Value.customRC.Value.Label : (op == null ? "" : (op.RCRoute.IsEmpty ? op.Center.Label : "")))};" +
                            $"{(error.Value.customRCroute.HasValue ? error.Value.customRCroute.Value.Label : (op == null ? "" : (op.RCRoute.IsEmpty ? "" : op.RCRoute.Label)))};" +
                            $"{op?.OrderLink.OrderID.Object.Label};{op?.OrderLink.OrderID.Version};" +
                            $"{partiaGuid};{error.Value.PartiaSettings};" +
                            $"{(error.Value.Count == 0 ? "" : error.Value.Count)};{(error.Value.NaladLength == 0 ? "" : error.Value.NaladLength)};" +
                            $"{(error.Value.Count == 0 || op == null ? "" : (op.MesModelling.Code == Guid.Empty ? "" : op.MesModelling))};" +
                            $"{message};{error.Value.TechData}"
                            );
                    }
                }
            }

            //sw.Close();
            //swBig.Close();

            swNew.Close();
            swNewBig?.Close();

            return HasErrors;
        }
    }
}

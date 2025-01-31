using Rodana.Core.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core
{
    public class DataErrors<RCCode, RCLabel, OrderCode, OrderLabel, OrderSubType, CommonCode, CommonLabel>
    {

        public Session<RCCode, RCLabel, OrderCode, OrderLabel, OrderSubType, CommonCode, CommonLabel> session { get; init; }


        public string PartiasErrorsFile = "partiasErrors.csv";
        public string PartiasErrorsBigFile = "partiasErrorsBig.csv";
        public string DatasErrorsFile = "dataErrors.csv";
        public string DatasErrorsBigFile = "dataErrorsBig.csv";

        public enum NewDataErrorType : byte
        {
            NoProduction,
            NoNomenklature,
            NoVidOper,

            NoNomenklatureEmptyProd,
            NoVidOperEmptyProd,

            NoGP, // гп не задана

            Restricted_VPZ,
            Restricted_Fact,
            Restricted_NoReplan,

            NoOrigTara, // не задана конечная тара

            NoChangeTaraNoPriem, // нет приемной тары при "не менять тару"
            NoChangeTaraNoPriemVspom, // То же, но с вспомогательной
            NoChangeTaraMoreThanOne, // апс откопал 2+ "не менять тару

            NonPositiveLength, // отрицательная длительность
            BadStartTimeFact, // дата начала факт

            SomeFileIsOpened, // открыты файлы
            CorruptedDataFile, // повреждены файлы с данными

            NotEnoughMaxOnTaras, // тара найдена, но не хватает макс намотки
            NotEnoughMaxOnTarasVspom, // то же, но вспом
            NoTaraNMaxIntersection, // Не найдена основная тара для рассчитанного количества с учетом выбранной тары по макс намотке и приемной таре по цепочке по таре.
            NoTaraNMaxIntersectionVspom, // то же, но вспом
            NoPriemNOtdIntersection, // Нарушение цепочки приемная-отдающая тара по основной таре
            NoPriemNOtdIntersectionVspom, // то же, но со вспомогательной
            NoPriemNOtdIntersectionNoChange, // то же, но с признаком не менять тару

            NoPriemOrigTara, // тут не найдена приемная отдающая тара
            NoOtdOrigTara, // нет РЦ с нужной отдающей тарой

            WasNotAbleToMerge, // не смог соединить на 1 уровне
            WasNotAbleToMergeNoSettings, // то же, но без настроек
            WasNotAbleToMergeMax, // не влезло на одну
            WasNotAbleToMergeDontKnow, // хз почему

            NoAllNeededOtdTaras, // нет ни одного рц у которого есть все нужные отдающие тары
            NoAllNeededOtdTarasWithCheck, // то же, с доп проверкой
        }

        public struct DataErrorMore
        {
            /*
             * Набор РЦ на текущей операции ☑
             * Подходящая тара с максимальной намотки ☑
             * Отдающая тара на текущей операции ☑
             * Тип тары приемная на текущей операции; ☑
             * максимальная намотка ☑
             * Пересечение макс намотки и применяемой тары
             * Набор РЦ на предыдущей операции
             * Тип  отдающей тары на предыдущей операции,вспомогательная истина/ложь;максимальная намотка
             * Следующий этап
             * Набор РЦ на следующей операции
             * Тип отдающей тары на следующей операции;максимальная намотка
             * Количество на уровне в партии
            */
            internal string DataToInsert; // строка, которую можно вставить в основное сообщение
            internal string TechData; // строка, которую можно вставить в основное сообщение
            internal double Count;
            internal string PartiaSettings; // список настроек партий
            internal Guid? PartiaGuid;
            internal List<Guid> customEtapsGuids;
            internal CodedObject<RCCode, RCLabel>? customRC, customRCroute;
            internal CodedObject<CommonCode, CommonLabel>? customNomen, customProd, customVid;
            internal List<CodedObject<RCCode, RCLabel>> RCsOnThisEtap; // Набор РЦ на текущей операции
            internal List<CodedObject<CommonCode, CommonLabel>> TarasOnThisEtap; // Тары на текущем этапе - Тип тары приемная на текущей операции
            internal List<(CodedObject<CommonCode, CommonLabel> Tara, bool WasVspom)> NeededOtdTarasOnThisEtap; // Требуемые отдающие тары здесь - Отдающая тара на текущей операции
            internal List<(CodedObject<CommonCode, CommonLabel> Tara, double Max)> MaxNamotka; // максимальная намотка
            internal List<CodedObject<CommonCode, CommonLabel>> MaxPlusUsedTaras; // Пересечение максимальных намоток и применяемых тар - Подходящая тара с максимальной намотки
            internal List<CodedObject<RCCode, RCLabel>> RCsOnNextEtap; // РЦ на след уровне
            internal List<CodedObject<CommonCode, CommonLabel>> TarasOnNextEtap; // тары на след. уровне

            internal List<CodedObject<RCCode, RCLabel>> RCsOnPrevEtap; // Набор РЦ на текущей операции
            internal List<CodedObject<CommonCode, CommonLabel>> TarasOnPrevEtap; // Тары на текущем этапе - Тип тары приемная на текущей операции

            public DataErrorMore(string dataToInsert) : this(dataToInsert, null, null, null, null, null, null, null, null) { }
            public DataErrorMore(string dataToInsert, string moreData) : this(dataToInsert, null, null, null, null, null, null, null, moreData) { }

            public DataErrorMore(
                string dataToInsert,
                List<CodedObject<RCCode, RCLabel>> rCsOnThisEtap,
                List<CodedObject<CommonCode, CommonLabel>> tarasOnThisEtap,
                List<(CodedObject<CommonCode, CommonLabel> Tara, bool WasVspom)> neededOtdTarasOnThisEtap,
                List<(CodedObject<CommonCode, CommonLabel> Tara, double Max)> maxNamotka,
                List<CodedObject<CommonCode, CommonLabel>> maxPlusUsedTaras,
                List<CodedObject<RCCode, RCLabel>> rCsOnNextEtap,
                List<CodedObject<CommonCode, CommonLabel>> tarasOnNextEtap,
                string moreData = null
                )
            {
                DataToInsert = dataToInsert;
                RCsOnThisEtap = rCsOnThisEtap;
                TarasOnThisEtap = tarasOnThisEtap;
                NeededOtdTarasOnThisEtap = neededOtdTarasOnThisEtap;
                MaxNamotka = maxNamotka;
                MaxPlusUsedTaras = maxPlusUsedTaras;
                TechData = moreData;
                RCsOnNextEtap = rCsOnNextEtap;
                TarasOnNextEtap = tarasOnNextEtap;
                Count = 0;
                PartiaSettings = null;
                PartiaGuid = null;
                customEtapsGuids = null;
                customRC = null;
                customRCroute = null;
                RCsOnPrevEtap = null;
                TarasOnPrevEtap = null;
                customNomen = customProd = customVid = null;
            }
        }

        public ConcurrentDictionary<int, Dictionary<NewDataErrorType, DataErrorMore>> NewDataErrors = new();

        object lockerHere = new();
        public void AddNewPartiaError(int ID, NewDataErrorType error, DataErrorMore errorData)
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

        internal bool TryOutputDataErrors()
        {
            bool HasErrors = false;
            /*
            var sw = new StreamWriter(PartiasErrorsFile, false, Encoding.UTF8);
            var swBig = new StreamWriter(PartiasErrorsBigFile, false, Encoding.UTF8);
            var swNew = new StreamWriter(DatasErrorsFile, false, Encoding.UTF8);
            StreamWriter swNewBig = null;
            try
            {
                swNewBig = new StreamWriter(DatasErrorsBigFile, false, Encoding.UTF8);
            }
            catch
            {
            }
            sw.WriteLine("Заказ;ГП;ИдентификаторПартииПФ;Этап;Причина");
            swBig.WriteLine("Заказ;ГП;ИдентификаторПартииПФ;Этап;Номенклатура;Вид;Продукция;Причина");

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

            bool yes61 = NewDataErrors.Any(x => x.Key >= 0 && OperationsDataSet.ALL[x.Key].RCRoute.IsEmpty);
            bool yes62 = NewDataErrors.Any(x => x.Key >= 0 && !OperationsDataSet.ALL[x.Key].RCRoute.IsEmpty);

            bool yes55 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.RCsOnPrevEtap != null && x.RCsOnPrevEtap.Count > 0);
            bool yes56 = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.TarasOnPrevEtap != null && x.TarasOnPrevEtap.Count > 0);

            bool yesPrevs = NewDataErrors.Any(x => x.Key >= 0 && OperationsDataSet.ALL[x.Key].PrevsNormCount > 0);
            bool yesNexts = NewDataErrors.Any(x => x.Key >= 0 && OperationsDataSet.ALL[x.Key].Nexts.Count > 0);

            bool yesCount = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.Count != 0);

            bool yesMod = NewDataErrors.Any(x => x.Key >= 0 && x.Value.Any(y => y.Value.Count != 0) && OperationsDataSet.ALL[x.Key].Modelling != Guid.Empty);

            bool yesCProd = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.customProd != null);
            bool yesCNom = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.customNomen != null);
            bool yesCVid = NewDataErrors.SelectMany(x => x.Value.Select(y => y.Value)).Any(x => x.customVid != null);

            var str1 = $"{BtoS(yes1)};{BtoS(yes2)};{BtoS(yes3)};{BtoS(yes4)};{BtoS(yes5)};{BtoS(yes51)};{BtoS(yes52)};" +
                $"{BtoS(yes55)};{BtoS(yes56)};" +
                $"{BtoS(yes6 || yesCOL)};{BtoS(yesPrevs)};{BtoS(yesNexts)};" +
                $"{BtoS(yes6)};{BtoS(yes6 || yesCProd)};{BtoS(yes6 || yesCNom)};{BtoS(yes6 || yesCVid)};" +
                $"{BtoS(yes61 || yesCRC)};{BtoS(yes62 || yesCRCR)};" +
                $"{BtoS(yes6)};{BtoS(yes6)};{BtoS(yes6 || yes54)};" +
                $"{BtoS(yes53)};" +
                $"{BtoS(yesCount)};{BtoS(yesMod)};" +// $"{BtoS(yes6 || yesCount)};{BtoS(yesMod)};" +
                $"ДА;{BtoS(yesdOP)}";
            var str2 = "ДоступныеРЦТекущегоЭтапа;ПриемнаяТараТекущегоЭтапа;ТребуемаяОтдающаяТара;МаксНамоткиТекущегоЭтапа;ПересечениеМаксНамотокПриемнойТары;РЦСледующегоЭтапа;ТараСледующегоЭтапа;" +
                "РЦПредыдущегоЭтапа;ТараПредыдущегоЭтапа;" +
                "Этап;ПредыдущиеЭтапы;СледующиеЭтапы;" +
                "МетодРасчёта;Продукция;Номенклатура;ВидОперации;" +
                "РЦ;ГруппаЗаменяемостиРабочихЦентров;" +
                "Заказ;ПартияГП;ИдентификаторПартииПФ;" +
                "НастройкиФормированияПартий;" +
                "Количество;Моделирование;" +
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
                    foreach (var error in element.Value)
                    {
                        var message = error.Key switch
                        {
                            NewDataErrorType.NoProduction => (OperationsDataSet.ALL[element.Key].UsedCalcMethod == DimensionUnit.m ? "По данной продукции не найдена максимальная намотка с методом расчета по метражу." : "По данной номенклатуре и тех. операции не найдена максимальная намотка с методом расчёта по весу."),
                            NewDataErrorType.NoNomenklature => "По данной продукции не найдена максимальная намотка по указанной номенклатуре.",
                            NewDataErrorType.NoVidOper => "По данной продукции и номенклатуре не найдена максимальная намотка по виду операции.",

                            NewDataErrorType.NoNomenklatureEmptyProd => "По пустой продукции не найдена максимальная намотка по указанной номенклатуре с методом расчёта по весу.",
                            NewDataErrorType.NoVidOperEmptyProd => "По номенклатуре и пустой продукции не найдена максимальная намотка по виду операции с методом расчёта по весу.",

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

                            NewDataErrorType.CorruptedDataFile => "Ошибка при загрузке модуля данных '{0}'. Обратитесь к администратору.",

                            NewDataErrorType.NotEnoughMaxOnTaras => "Не найдена основная тара для рассчитанного количества с учетом выбранной тары по макс намотке и приемной таре по цепочке по таре (количество этапа слишком велико).",
                            NewDataErrorType.NotEnoughMaxOnTarasVspom => "Не найдена вспомогательная тара для рассчитанного количества с учетом выбранной тары по макс намотке и приемной таре по цепочке по таре (количество этапа слишком велико).",

                            NewDataErrorType.NoTaraNMaxIntersection => "Не найдена основная тара для рассчитанного количества среди максимальной намотки и применяемой приемной тары (нет пересечения).",

                            NewDataErrorType.NoTaraNMaxIntersectionVspom => "Не найдена вспомогательная тара для рассчитанного количества среди максимальной намотки и применяемой приемной тары (нет пересечения).",

                            NewDataErrorType.NoPriemNOtdIntersection => "Нарушение цепочки 'приемная-отдающая тара' по основной таре.",
                            NewDataErrorType.NoPriemNOtdIntersectionVspom => "Нарушение цепочки 'приемная-отдающая тара' по вспомогательной таре.",
                            NewDataErrorType.NoPriemNOtdIntersectionNoChange => "Нарущение цепочки 'приемная-отдающая' тара по основной таре с учетом РЦ  с признаком \"Не менять тару\".",

                            NewDataErrorType.NoPriemOrigTara => "Для барабана ГП '{0}' не найден РЦ с данной применяемой приёмной тарой.",
                            NewDataErrorType.NoOtdOrigTara => "Для барабана ГП '{0}' не найден РЦ с данной применяемой отдающей тарой.",

                            NewDataErrorType.WasNotAbleToMerge => "На 1м уровне не выполняются настройки формирования партий.",

                            NewDataErrorType.WasNotAbleToMergeNoSettings => "На 1м уровне отсутсвуют настройки формирования партий, внут. проверки: кол-во мест, повивы, подразд., номенкл., операции (НЕ виды), напряжения, материалы, пересечение по РЦ, проверка на ТПЖ.",

                            NewDataErrorType.WasNotAbleToMergeMax => "На 1м уровне не удалось объединить этапы по максимальной намотке. Обратитесь к ПДО.",

                            NewDataErrorType.WasNotAbleToMergeDontKnow => "Не удалось объединить партии по неизвестной причине. Обратитесь в ОАУП.",

                            NewDataErrorType.NoAllNeededOtdTaras => "У этапа нет ни одного РЦ, у которого есть все перечисленные требуемые отдающие тары.",

                            NewDataErrorType.NoAllNeededOtdTarasWithCheck => "У этапа нет ни одного РЦ, у которого есть все перечисленные требуемые отдающие тары (deep-check).",

                            _ => "Необработанная ошибка. Обратитесь в ОАУП."
                        };

                        message = string.Format(message, error.Value.DataToInsert ?? "");
                        string list1 = ""; if (error.Value.RCsOnThisEtap != null) list1 = string.Join(", ", error.Value.RCsOnThisEtap.Select(X => X.Label));
                        string list2 = ""; if (error.Value.TarasOnThisEtap != null) list2 = string.Join(", ", error.Value.TarasOnThisEtap.Select(X => X.Label));
                        string list3 = ""; if (error.Value.NeededOtdTarasOnThisEtap != null) list3 = string.Join(", ", error.Value.NeededOtdTarasOnThisEtap.Select(X => X.Tara.Label + (X.WasVspom ? " (вспом.)" : "")));
                        string list4 = ""; if (error.Value.MaxNamotka != null)
                            list4 = string.Join(", ", error.Value.MaxNamotka.Select(X => $"{X.Tara.Label} ({X.Max})"));
                        string list5 = ""; if (error.Value.MaxPlusUsedTaras != null) list5 = string.Join(", ", error.Value.MaxPlusUsedTaras.Select(X => X.Label));
                        string list51 = ""; if (error.Value.RCsOnNextEtap != null) list51 = string.Join(", ", error.Value.RCsOnNextEtap.Select(X => X.Label));
                        string list52 = ""; if (error.Value.TarasOnNextEtap != null) list52 = string.Join(", ", error.Value.TarasOnNextEtap.Select(X => X.Label));

                        string list53 = ""; if (error.Value.RCsOnPrevEtap != null) list53 = string.Join(", ", error.Value.RCsOnPrevEtap.Select(X => X.Label));
                        string list54 = ""; if (error.Value.TarasOnPrevEtap != null) list54 = string.Join(", ", error.Value.TarasOnPrevEtap.Select(X => X.Label));

                        string listCOL = ""; if (error.Value.customEtapsGuids != null) listCOL = string.Join(", ", error.Value.customEtapsGuids);

                        var op = element.Key < 0 ? null : OperationsDataSet.ALL[element.Key];

                        Guid? partiaGuid = op == null ? error.Value.PartiaGuid : (op.CustomPartiaGuid ?? (op.OrderN.OrderName.Label.Length > 0 ? (Orders.GlobalOrders[op.OrderN].PartiaPF ?? Guid.Empty) : Guid.Empty));

                        swNew.WriteLine(
                            $"{list1};{list2};{list3};{list4};{list5};{list51};{list52};" +
                            $"{list53};{list54};" +
                            $"{(string.IsNullOrEmpty(listCOL) ? op?.GUID : listCOL)};" +
                            $"{(op == null ? "" : string.Join(", ", op?.Prevs.Select(x => OperationsDataSet.ALL[x].GUID)))};" +
                            $"{(op == null ? "" : string.Join(", ", op?.Nexts.Select(x => OperationsDataSet.ALL[x].GUID)))};" +
                            $"{(op == null ? "" : (op.UsedCalcMethod == DimensionUnit.m ? "По метражу" : "По весу"))};" +
                            $"{(error.Value.customProd.HasValue ? error.Value.customProd.Value.Code : op?.Production.Code)};{(error.Value.customNomen.HasValue ? error.Value.customNomen.Value.Code : op?.Nomenklatura.Code)};{(error.Value.customVid.HasValue ? error.Value.customVid.Value.Code : op?.OperVid.Code)};" +
                            $"{(error.Value.customRC.HasValue ? error.Value.customRC.Value.Code : (op == null ? "" : (op.RCRoute.IsEmpty ? op.Center.Code : "")))};" +
                            $"{(error.Value.customRCroute.HasValue ? error.Value.customRCroute.Value.Code : (op == null ? "" : (op.RCRoute.IsEmpty ? "" : op.RCRoute.Code)))};" +
                            $"{op?.OrderN.OrderName.Code};{op?.OrderN.NumberGP};" +
                            $"{partiaGuid};{error.Value.PartiaSettings};" +
                            $"{(error.Value.Count == 0 ? "" : error.Value.Count)};" +
                            $"{(error.Value.Count == 0 || op == null ? "" : (op.Modelling == Guid.Empty ? "" : op.Modelling))};" +
                            $"{message};{error.Value.TechData}"
                            );

                        swNewBig?.WriteLine(
                            $"{list1};{list2};{list3};{list4};{list5};{list51};{list52};" +
                            $"{list53};{list54};" +
                            $"{(string.IsNullOrEmpty(listCOL) ? (op?.Registrator + " " + op?.GUID) : listCOL)};" +
                            $"{(op == null ? "" : string.Join(", ", op?.Prevs.Select(x => OperationsDataSet.ALL[x].GUID)))};" +
                            $"{(op == null ? "" : string.Join(", ", op?.Nexts.Select(x => OperationsDataSet.ALL[x].GUID)))};" +
                            $"{(op == null ? "" : (op.UsedCalcMethod == DimensionUnit.m ? "По метражу" : "По весу"))};" +
                            $"{(error.Value.customProd.HasValue ? error.Value.customProd.Value.Label : op?.Production.Label)};{(error.Value.customNomen.HasValue ? error.Value.customNomen.Value.Label : op?.Nomenklatura.Label)};{(error.Value.customVid.HasValue ? error.Value.customVid.Value.Label : op?.OperVid.Label)};" +
                            //$"{op?.RCRoute.Label};" +
                            $"{(error.Value.customRC.HasValue ? error.Value.customRC.Value.Label : (op == null ? "" : (op.RCRoute.IsEmpty ? op.Center.Label : "")))};" +
                            $"{(error.Value.customRCroute.HasValue ? error.Value.customRCroute.Value.Label : (op == null ? "" : (op.RCRoute.IsEmpty ? "" : op.RCRoute.Label)))};" +
                            $"{op?.OrderN.OrderName.Label};{op?.OrderN.NumberGP};" +
                            $"{partiaGuid};{error.Value.PartiaSettings};" +
                            $"{(error.Value.Count == 0 ? "" : error.Value.Count)};" +
                            $"{(error.Value.Count == 0 || op == null ? "" : (op.Modelling == Guid.Empty ? "" : op.Modelling))};" +
                            $"{message};{error.Value.TechData}"
                            );
                    }
                }
            }

            sw.Close();
            swBig.Close();

            swNew.Close();
            swNewBig?.Close();

            */
            return HasErrors;
        }
    }
}

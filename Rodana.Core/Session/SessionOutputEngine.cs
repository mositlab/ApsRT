using Rodana.Core.DataErrors;
using Rodana.Core.NaladLengths;
using Rodana.Core.Partias.Zadels;
using Rodana.Core.Planning.PlanningCalculatorEngine;
using Rodana.Core.Planning.PlanningClasses;
using Rodana.Core.Planning.PlanningClasses.PoolClasses;
using Rodana.Core.Rechanges.RechangesEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Extensions;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Rodana.Core.Partias.PartiasClasses;
using Rodana.Core.Partias.PartiasConnectorEngine;
using System.Drawing;

namespace Rodana.Core.Session
{
    /// <summary>
    /// Движок вывода всяких данных в файлы. В отличие от других движков, которые статичны и содержат огромные методы, здесь скорее не движок, а просто совокупность методов для выводов в файлы и скуль-базы, к тому же ещё и привязанная к сессии.
    /// </summary>
    public class SessionOutputEngine
    {
        public string LogFile { get; set; } = "log.txt";
        public string PartiiLogFile { get; set; } = "partii_log.txt";
        public string ExitLogFile { get; set; } = "exit_log.txt";

        public Session SessionLink { get; }

        public SessionOutputEngine(Session session)
        {
            SessionLink = session;
        }

        public void ClassicSettingsByOptions(SessionOptions options)
        {
            LogFile = "log " + DateTime.Now.ToString("dd.MM.yyyy H-mm-ss") + ".txt";
            PartiiLogFile = "partii_log " + DateTime.Now.ToString("dd.MM.yyyy H-mm-ss") + ".txt";
            ExitLogFile = "exit_log " + DateTime.Now.ToString("dd.MM.yyyy H-mm-ss") + ".txt";
            OutputFileName = $"Result_{(options.IsSilent ? "" : "see")}{(options.LaunchZadel ? "part_zadel" : "")}{(options.LaunchPartia ? "part" : "")}{(options.LaunchPlan ? "plan" : "")}" +
    //$"{(string.IsNullOrEmpty(key) ? "" : "_" + key)}" +
    $".csv";
            OutputFileNameOld = OutputFileName.Substring(0, OutputFileName.Length - 4) + "_old.csv";
        }

        public void CheckFiles()
        {
            if (!SessionLink.SessionOptions.GetPreparedLaunchPath(out var sessionPath))
            {
                if (SessionLink.SessionOptions.IsSilent)
                    SessionLink.EndSession(602);
            }

            List<string> list = new List<string>();

            // Чекаем файлики, можно ли их открыть потом
            void CheckFile(string filename, int code, bool willClose = false)
            {
                if (File.Exists(filename))
                {
                    try
                    {
                        var f = File.OpenRead(filename);
                        f.Close();
                    }
                    catch
                    {
                        list.Add(filename);
                        if (willClose) // если это dataErrors, то выходим
                            SessionLink.EndSession(code);
                    }
                }
            }

            CheckFile(sessionPath + RangesManualFileName, 500);
            CheckFile(sessionPath + RangesManualFileNameOld, 501);
            CheckFile(sessionPath + ModsFileName, 502);
            CheckFile(sessionPath + ModsFileNameOld, 503);
            CheckFile(sessionPath + NaladLengthsFileName, 504);
            CheckFile(sessionPath + OrdersFileName, 505);
            CheckFile(sessionPath + OrdersFileNameOld, 506);
            CheckFile(sessionPath + SessionLink.DataErrors.DatasErrorsFile, 507, true);
            //CheckFile("dataErrorsBig.csv", 508);
            CheckFile(sessionPath + RechangesFileName, 509);
            CheckFile(sessionPath + OutputFileName, 510);
            CheckFile(sessionPath + OutputFileNameOld, 511);
            CheckFile(sessionPath + ZadelsFileName, 512);
            CheckFile(sessionPath + RechangesFileNameOld, 513);
            CheckFile(sessionPath + MaterialsDeficitsFileName, 514);
            //CheckFile(SessionLink.DataErrors.PartiasErrorsFile, 514, true);
            //CheckFile(SessionLink.DataErrors.PartiasErrorsBigFile, 515, true);

            if (list.Count > 0) SessionLink.DataErrors.AddNewPartiaError(-1, NewDataErrorType.SomeFileIsOpened, new DataErrorStruct("", string.Join(", ", list)));
            if (SessionLink.DataErrors.TryOutputDataErrors())
            {
                if (SessionLink.SessionOptions.IsSilent)
                    SessionLink.EndSession(601);
            }
        }

        public (PlanningCalculatorEngineResult result, OpersPool copy) Output(bool ForcedToRecreate, bool asOld = false)
        {
            // Вызов вывода заделов
            if (SessionLink.SessionOptions.LaunchZadel)
            {
                try
                {
                    OutputZadelsToFile(ZadelsFileName);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Что-то не то с выводом заделов: {e.Message}");
                }
            }

            // Вывод всего
            var algorithm = SessionLink.AssignedPlanningAlgorithm;
            if (algorithm == null || ForcedToRecreate) SessionLink.TryCreateNewPlanningAlgorithmInstance(out algorithm); // Сначала просто получим текущий алгоритм планирования, потому что именно там хранится всё, что нам надо для вывода
            if (algorithm.BestBlocksList == null)
            {
                // Значит, алгоритм не работает. Подготовить его
                algorithm.PrepareEverything();
            }

            List<OpersBlock> blocksToCopy = algorithm.BestBlocksList;

            if (blocksToCopy == null)
            {
                return (null, null);
            }

            lock (algorithm.LockerObject)
            {

                var localCopy = new OpersPool(SessionLink, blocksToCopy, new OpersPoolConstructor(blocksToCopy, algorithm.MinAcceptableDate), algorithm.MinAcceptableDate);

                var result = PlanningCalculatorEngineResult.CreateFullInstance(SessionLink, algorithm.MinAcceptableDate);

                PlanningCalculatorEngine.CalculatePlan(localCopy, algorithm.MinAcceptableDate, result);

                SessionLink.SessionOptions.GetPreparedLaunchPath(out var sessionPath);

                OutputMaterialsDeficitsToFile(sessionPath + MaterialsDeficitsFileName, localCopy, result.FirstStagesStartDate);

                OutputRechangesToFile(sessionPath + (asOld ? RechangesFileNameOld : RechangesFileName), result.RechangesBeforeFirstStages, result.FirstStagesStartDate);

                OutputRanksManualToFile(sessionPath + (asOld ? RangesManualFileNameOld : RangesManualFileName), localCopy, result, algorithm.MinAcceptableDate);

                OutputResultToFile(sessionPath + (asOld ? OutputFileNameOld : OutputFileName), localCopy, result);

                OutputNaladLengths(sessionPath + NaladLengthsFileName, blocksToCopy, localCopy.NaladLengthsData, localCopy.NaladLengthsDataAsMods, null);

                OutputOrdersToFile(sessionPath + (asOld ? OrdersFileNameOld : OrdersFileNameOld), localCopy.OrdersData);

                OutputMods(sessionPath + (asOld ? ModsFileNameOld : ModsFileName), blocksToCopy);

                OutputLeftovers(sessionPath + UsedLeftoversFileName, string.IsNullOrEmpty(UsedLeftoversFileNameBig) ? null : (sessionPath + UsedLeftoversFileNameBig));

                return (result, localCopy);
            }
        }

        public string ToCsvString(Uco element)
        {
            return $"{element.Label.ToString().Replace(';', '_')} [{element.Code}]";
        }

        public string UsedLeftoversFileName { get; set; } = "leftoversUsed.csv";
        public string UsedLeftoversFileNameBig { get; set; } = "leftoversUsedBig.csv";
        public void OutputLeftovers(string filename, string filenameBig)
        {
            var sw = new StreamWriter(filename, false, Encoding.UTF8);
            StreamWriter swBig = string.IsNullOrEmpty(filenameBig) ? null : new StreamWriter(filenameBig, false, Encoding.UTF8);

            sw.WriteLine($"GUID;Материал_GUID;Количество;ОригМатериал_GUID;ОригКоличество;ДатаПланирования");
            swBig?.WriteLine($"GUID;Материал_GUID;Материал;Количество;ОригМатериал_GUID;ОригМатериал;ОригКоличество;ДатаПланирования");


            foreach (var operPair in SessionLink.MasterDataLinks.StagesMasterData.AllStages)
            {
                var oper = operPair.Value;
                if (oper.StageMaterialsInfo.UsedMaterials == null) continue;
                for (int i = 0; i < oper.StageMaterialsInfo.UsedLeftovers.Count; i++)
                {
                    var element = oper.StageMaterialsInfo.UsedLeftovers[i];
                    var origMat = oper.StageMaterialsInfo.UsedMaterials[i];
                    if (element == null) continue;

                    sw.WriteLine($"" +
                        $"{oper.GUID};" +

                        $"{element.Nomenklature.Code};" +
                        $"{element.CalculatedCount};" +

                        $"{origMat.Nomenklature.Code};" +
                        $"{origMat.Count};" +

                        $"{DateTime.Now}");

                    swBig?.WriteLine($"" +
                        $"{oper.GUID};" +

                        $"{element.Nomenklature.Code};" +
                        $"{element.Nomenklature.Label};" +
                        $"{element.CalculatedCount};" +

                        $"{origMat.Nomenklature.Code};" +
                        $"{origMat.Nomenklature.Label};" +
                        $"{origMat.Count};" +

                        $"{DateTime.Now}");
                }
            }


            sw.Close();
            swBig?.Close();
        }

        public string ModsFileName { get; set; } = "mods.csv";
        public string ModsFileNameOld { get; set; } = "mods_old.csv";

        public void OutputMods(string filename, List<OpersBlock> blocks)
        {
            // Ранги моделирований для алены и алексея
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            var sw = new StreamWriter(filename, false, Encoding.UTF8);
            sw.WriteLine("N;Моделирование;" +
                //"ID операций;" +
                //"GUID операций;" +
                "Следующее моделирование");
            var D = 0;

            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            //int D2 = 0;
            foreach (var r in blocks)
            {
                foreach (var r2 in r.Lists)
                {
                    var first = r2.FirstOper;
                    var allNexts = r2.OpersList.SelectMany(x => x.Nexts).Where(x => ALL.ContainsKey(x)).Select(next1 =>
                    {
                        var next = ALL[next1];
                        return next.IsCustomModelling ? next.CustomModelling : (next.MesModelling.Code == Guid.Empty ? next.GUID : next.MesModelling.Code);
                    }).Distinct().ToArray();
                    foreach (var nexti in allNexts)
                    {
                        sw.WriteLine($"{D};{(first.IsCustomModelling ? first.CustomModelling : (first.MesModelling.Code == Guid.Empty ? first.GUID : first.MesModelling.Code))};" +
                            //$"{(string.Join(",", r2.Select(x => x.ID)))};" +
                            //$"{(string.Join(",", r2.Select(x => x.GUID)))};" +
                            $"{nexti}");
                        D++;
                    }
                    //Operation? next = first.Nexts.Count > 0 && ALL.TryGetValue(first.Nexts[0], out var op) ? op : null;
                    //r2[0].CustomModelling
                    //foreach (var r3 in r2)
                    //{
                    //    sw.WriteLine($"{D};{r3.ID};{r3.GUID};[{r3.Modelling}];{D2};{CentersIntToStr[r3.Center]};{CentersIntToStr[r.ActualRC]}");
                    //    D++;
                    //}
                }
                //D2++;
            }
            sw.Close();
        }


        public string OrdersFileName { get; set; } = "orders.csv";
        public string OrdersFileNameOld { get; set; } = "orders_old.csv";
        
        public void OutputOrdersToFile(string filename, FrozenDictionary<OrderID, OrdersPoolClass> orders)
        {
            //"orders_old.csv"
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                var sw = new StreamWriter(filename, false, Encoding.UTF8);
                sw.WriteLine("N;ID заказа;Метка;Сорван?;Количество дней;Значение");
                var D = 1;
                foreach (var ord in orders)
                {
                    if (ord.Key.Object.Label.Length == 0)
                    {
                        continue;
                    }

                    var vv = ord.Value.Time;
                    var v = vv > 0 ? "Да" : "Нет";
                    sw.WriteLine($"{D};{ord.Key};{ord.Key.Object.Label};{v};{vv};{ord.Value.TimeWithK}");
                    D++;
                }
                sw.Close();
            }
            catch
            {

            }
        }

        public string NaladLengthsFileName { get; set; } = "naladLengths.csv";

        public void OutputNaladLengths(string filename, List<OpersBlock> blocks/*Dictionary<int, ModellingOper> mods*/, FrozenDictionary<int, GottenNaladLengthPoolClass> nalaLengths, FrozenDictionary<Guid, GottenNaladLengthPoolClass> nalaLengthsMods/*, Dictionary<int, HashSet<CatchedErrorType>> CatchedErrors*/, ConcurrentDictionary<Guid, ConcurrentDictionary<float, NaladLengthFixerClass>> naladLengthsErrors)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            var sw = new StreamWriter(filename, false, Encoding.UTF8);

            SessionLink.SessionOptions.GetPreparedLaunchPath(out var sessionPath);

            StreamWriter fullOutput = null;
            if (!SessionLink.SessionOptions.IsSilent)
            {
                try
                {
                    var tine = DateTime.Now;
                    fullOutput = new StreamWriter($"{sessionPath}Отчет - наладочные длины.csv"/* ({tine.Year}-{tine.Month}-{tine.Day} {tine.Hour}.{tine.Minute}.{tine.Second})*/, false, Encoding.UTF8);
                }
                catch
                {
                    Console.WriteLine("Не могу подробный отчет выдать, ну и хер с ним.");
                }
            }


            sw.WriteLine("N;GUID;Место;Длина;СледующаяДлина;ПерваяВЗапуске");
            fullOutput?.WriteLine("N;Регистратор;GUID;Заказ;ГП;Партия;Уровень;Номенклатура;Вид;Продукция;РЦ;Место;Длина;СледующаяДлина;ПерваяВЗапуске;Вспом;Коэф;Колво;ЕдИзм;номер мода;Кол-воМода;НаладДлинаМода;СледНаладДлинаМода;КоэффМода;Тара этапа;Макс намотка;Влазит мод;Фикс?;Жалобы?");

            var D = 1;
            foreach (var block in blocks)
            {
                for (int i = 0; i < block.Lists.Count; i++)
                {
                    var list = block.Lists[i];
                    bool firstMod = true;
                    bool wasLastFlag = false;
                    Guid? lastEasyFlag = null;
                    for (int j = 0; j < list.OpersList.Count; j++)
                    {
                        var op = list.OpersList[j];
                        var mod = op.IsCustomModelling ? op.CustomModelling : op.MesModelling.Code;
                        var modNalads = mod == Guid.Empty ? nalaLengths[op.Index] : nalaLengthsMods[mod];
                        if (wasLastFlag && mod != lastEasyFlag)
                        {
                            firstMod = false; // это уже другой мод
                            lastEasyFlag = mod;
                        }
                        if (!wasLastFlag)
                        {
                            lastEasyFlag = mod;
                            wasLastFlag = true;
                        }
                        var lengths = nalaLengths[op.Index];
                        for (int place = 1; place <= op.PlacesCount; place++)
                        {

                            if (!op.NeededNaladLength.HasValue)
                                sw.WriteLine($"{D};{op.GUID};{place};{(place == 1 && firstMod ? lengths.data.NaladLengthFirst.Value : lengths.data.NaladLengthOther.Value)};{lengths.data.NaladLengthNexts.Value};{(place == 1 && firstMod && lengths.data.IsFirst ? "Да" : "Нет")}");


                            fullOutput?.WriteLine($"{D};\"{op.Registrator}\";{op.GUID};\"{op.OrderLink.OrderID.Object}\";{op.OrderLink.OrderID.Version};{op.CustomPartiaGuid ?? op.OrderLink.PartiaPF ?? Guid.Empty};" +
                                $"{op.Level};{ToCsvString(op.Nomenklature)};{ToCsvString(op.OperationVid)};{ToCsvString(op.Production)};{op.Center};" +
                                $"{place};{(place == 1 && firstMod ? lengths.data.NaladLengthFirst.Value : lengths.data.NaladLengthOther.Value)};{lengths.data.NaladLengthNexts.Value};{(place == 1 && firstMod && lengths.data.IsFirst ? "Да" : "Нет")}" +
                                $";{op.IsVspom};{lengths.data.prevKoeff};{op.Count};{list.FirstOper.EdIzmClassificator};" +
                                $"{list.ModID};" +
                                $"{list.Count};" +
                                $"{modNalads.data.NaladLengthHere};" +
                                $"{modNalads.data.NaladLengthNexts};" +
                                $"{modNalads.data.prevKoeff};" +
                                $"{op.CurrentTara};" +
                                $"{op.CurrentTaraMaxCapacity};" +
                                $"{(op.IsVspom || op.NoChangeTara || !op.EndTara.IsEmpty ? "" : (op.CurrentTaraMaxCapacity >= modNalads.data.NaladLengthHere.Value + list.Count))};" +
                                $"{op.NeededNaladLength.HasValue}"/*;{CatchedErrors != null && CatchedErrors.ContainsKey(list.ModID)}*/);


                        }
                    }
                    //firstMod = false;
                    D++;
                }
            }

            if (naladLengthsErrors != null)
            {
                StreamWriter errorsOutput = new StreamWriter($"naladLengthsErrors.csv", false, Encoding.UTF8);
                errorsOutput.WriteLine($"GUID мода;Этапы;Количество;Наладочная длина;Тара;Макс;Найденная тара;Найденный макс;Причина");
                foreach (var element in naladLengthsErrors)
                {
                    foreach (var length in element.Value)
                    {
                        var message = length.Value.type switch
                        {
                            0 => "Есть возможность поставить тару без учёта требуемых отдающих тар и следующих РЦ",
                            1 => "Невозможно найти никакой тары для текущего моделирования",
                            2 => "Есть более вместительная тара, но даже она не вмещает",
                            _ => "Неизвестная ошибка"
                        };
                        var ops = length.Value.ops;
                        errorsOutput.WriteLine($"{element.Key};{(string.Join(",", ops.AllOpers.Select(x => x.Registrator)))};{ops.Sum};{length.Key};{ops.FirstOperLink.CurrentTara.Label};{ops.FirstOperLink.CurrentTaraMaxCapacity};{length.Value.tara};{length.Value.max};{message}");
                    }
                }
                errorsOutput.Close();
            }

            sw.Close();
            fullOutput?.Close();
        }

        public string OutputFileName { get; set; } = "Result.csv";
        public string OutputFileNameOld { get; set; } = "Result_old.csv";

        public void OutputResultToFile(string filename, OpersPool pool, PlanningCalculatorEngineResult dataForHelp)
        {
            var outputFile = new StreamWriter(filename, false, Encoding.UTF8);

            outputFile.WriteLine($"GUID;Номенклатура;ЗП;Партия ГП;Спецификация;" +
                    $"Продукция;Количество;НомерПартии;ДебагНомерпартии;НомерМоделирования;" +
                    $"ДебагНомерМода;Подразделение;Тип тары;Максимальная Намотка;РЦ;Раб центр маршрута;" +
                    $"Время Начала Объединения;Время Окончания Объединения;ДлительностьЧ;ЕдиницаИзмерения;ИдентификаторОбщегоЭтапа;ЕстьОшибки");

            bool IsSilent = SessionLink.SessionOptions.IsSilent;
            bool LaunchPlan = SessionLink.SessionOptions.LaunchPlan;
            var partiasAlgorithm = SessionLink.AssignedPartiasAlgorithm;

            for (int i = 0; i < pool.Blocks.Count; i++)
            {
                var block = pool.Blocks[i];
                var blockRC = block.ActualRC;

                // Группируем по дате начала (у параллелок она одинаковая), а затем внутри них ещё и по
                var sortedGroups = block.Lists.GroupBy(x => dataForHelp.FirstStagesStartDate[x.FirstOper.Index]);

                foreach (var group in sortedGroups) // Пробежимся по каждой группе
                {
                    var start = group.Key;
                    var maxTimeWithFixes = group.Max(x => dataForHelp.FirstStagesFullDurations[x.FirstOper.Index].Duration);
                    foreach (var mod in group)
                    {

                        for (int op = 0; op < mod.OpersList.Count; op++)
                        {
                            var operLoc = mod.OpersList[op];

                            var guidToCheck = operLoc.CustomPartiaGuid ?? (operLoc.OrderLink.PartiaPF ?? Guid.Empty);

                            if (true/*partiasAlgorithm == null || !partiasAlgorithm.PartiaHasErrors.TryGetValue(guidToCheck, out var errors) || !errors*/)
                            {
                                Guid ord = operLoc.OrderLink.OrderID.Object.Label.Length > 0 ? operLoc.OrderLink.OrderID.Object.Code : Guid.Empty;
                                var gp = operLoc.OrderLink.OrderID.Object.Label.Length > 0 ? operLoc.OrderLink.OrderID.Version : "";
                                outputFile.WriteLine($"{operLoc.GUID};{operLoc.Nomenklature.Code};{ord};{gp};{operLoc.Specification.Code};" +
                                    $"{operLoc.Production.Code};{operLoc.CountMES};" +
                                    $"{guidToCheck}" +
                                    $";;{(operLoc.IsCustomModelling ? operLoc.CustomModelling : operLoc.MesModelling.Code)};" +
                                    $";{operLoc.Subdivision.Code};{operLoc.CurrentTara.Code};{operLoc.CurrentTaraMaxCapacity};{blockRC.Code};" +//{operFirst.CurrentTara.Code};{operFirst.MaxNamotka};{operFirst.Center.Code};" +
                                    $"{(operLoc.RCRoute.Code/*operLoc.RCRoute.IsEmpty ? "" : operLoc.RCRoute.Code*/)};" +
                                    (!IsSilent || LaunchPlan ? $"{start:G}" : "") + // 04.06.2024: требование одну дату для всех исходных этапчиков выводить, которые объеиднились в один мод или общий этап.
                                    $";" +
                                    (!IsSilent || LaunchPlan ? $"{start + maxTimeWithFixes:G}" : "") +//$"{(centStruct.LastDate + offset + secs:G)}" +
                                    $";{maxTimeWithFixes.TotalHours}" +
                                    $";{operLoc.EdIzm.DimensionUnitToString()};{operLoc.GetEasyFlagToOutput()}" +
                                    $";{(partiasAlgorithm != null && partiasAlgorithm.ThisOperHasPartiaErrors(operLoc) ? "Да" : "Нет")}");
                            }
                        }
                    }
                }
            }


            outputFile.Close();

        }

        public string RechangesFileName { get; set; } = "rechanges.csv";
        public string RechangesFileNameOld { get; set; } = "rechanges_old.csv";

        public void OutputRanksManualToFile(string filename, OpersPool pool, PlanningCalculatorEngineResult dataForHelp, DateTime minAcceptableDate)
        {
            var ExistingCriterias = SessionLink.MasterDataLinks.RechangesMasterData.ExistingCriterias;
            var NonWorkingIntervalsMasterData = SessionLink.MasterDataLinks.WorkingIntervalsMasterData;
            var StagesMasterData = SessionLink.MasterDataLinks.StagesMasterData;

            var rangesManual = new StreamWriter(filename, false, Encoding.UTF8);

            var finalstr = "N;" +
                "Тип;" +
                "Дата начала;" +
                "Дата конца;" +
                "Длительность;" +
                "ID;" +
                "GUID;" +
                "Регистратор;" +
                "Ранг;" +
                "Моделирование;" +
                "Кастомное моделирование;" +
                "Количество;" +
                "Тара;" +
                "Стандартный РЦ;" +
                "Выбранный РЦ;" +
                "Заказ;" +
                "Партия ГП;" +
                "Номенклатура;" +
                "Продукция;" +
                "Вид Операции;" +

                 string.Join(";", ExistingCriterias.Select((x, index) => $"{x.Name}")) +

                ";Было;Дельта";

            for (var i = 1; i < 8; i++)
            {
                finalstr += $";Переналадка{i};Время_переналадки{i}";
            }

            rangesManual.WriteLine(finalstr);

            int fileN = 0;
            for (int i = 0; i < pool.Blocks.Count; i++)
            {
                var block = pool.Blocks[i];
                var blockRC = block.ActualRC;
                var sortedMods = block.Lists.OrderBy(x => dataForHelp.FirstStagesRanks[x.FirstOper.Index]).ToArray(); // Получив их ранги, мы можем точно понять, какое моделирование было первым, и так далее. Перед каждым моделированием, мы можем посмотреть, были ли перед ним простои и переналадки, взяв у его первой операции айдишник, а ещё дату начала.
                for (int mod = 0; mod < sortedMods.Length; mod++)
                {
                    var currMod = sortedMods[mod];
                    var nextOper = currMod.FirstOper;
                    var firstID = nextOper.Index;
                    (DatesPoolClass data, CachedInterval interval) prostoi;
                    (RechangesEngineResult data, CachedInterval interval, int last) rechanges;
                    if (!dataForHelp.ProstoiBeforeFirstStages.TryGetValue(firstID, out prostoi)) prostoi = (null, CachedInterval.Empty);
                    if (!dataForHelp.RechangesBeforeFirstStages.TryGetValue(firstID, out rechanges)) rechanges = (null, CachedInterval.Empty, -1);

                    var dateTime = dataForHelp.FirstStagesStartDate[firstID];

                    if (prostoi.interval.Duration.Ticks > 0)
                    {
                        var start = dateTime - rechanges.interval.Duration - prostoi.interval.Duration;
                        var finastr = $"{fileN};" +
                            $"Простой;" +
                            $"{start:G};" +
                            $"{(start + prostoi.interval.Duration):G};" +
                            $"{prostoi.interval.Duration:G}" +
                            $";;;;;;;;;;" +
                            $"{blockRC}";
                        rangesManual.WriteLine(finastr);
                        fileN++;
                    }

                    if (rechanges.data != null)
                    {
                        var start = dateTime - rechanges.interval.Duration;
                        var finastr = $"{fileN};Переналадки;" +
                            $"{start:G};" +
                            $"{(start.Add(rechanges.interval.Duration)):G};" +
                            $"{rechanges.interval.Duration:G}" +
                            $";{nextOper.Index};{nextOper.GUID};{nextOper.Registrator}" +
                            $";;;;;;;{blockRC};;;;;;" +

                            string.Join(";", ExistingCriterias.Select((x, index) => $"")) +

                            $";;";

                        var result = rechanges.data.ToLinearDictionary();
                        if (result != null)
                            foreach (var hash in result)
                            {
                                //foreach (var hash in level.Value)
                                finastr += $";{hash.Value.Name};{hash.Value.Duration}";
                            }
                        rangesManual.WriteLine(finastr);
                        fileN++;
                    }

                    TimeSpan offset = new TimeSpan();
                    int currentRank = dataForHelp.FirstStagesRanks[firstID];
                    var intervalForOp = dataForHelp.FirstStagesFullDurations[firstID];
                    for (int op = 0; op < currMod.OpersList.Count; op++)
                    {
                        var operLoc = currMod.OpersList[op];
                        var duration = TimeSpan.FromSeconds(operLoc.GetDuration(blockRC)) * currMod.TimeK[blockRC];
                        var start = dateTime + offset;

                        //if (intervalForOp.CatchedIntervals != null)
                        //foreach (var lunch in intervalForOp.CatchedIntervals)
                        //{
                        //    var interval = NonWorkingIntervalsMasterData.CentersFixingTime[blockRC][lunch];

                        //    if (start.Add(duration) >= (interval.When + interval.Time) // Конец операции дальше конца этого интервала?
                        //        && start <= interval.When // И при этом начало этой операции должно стоять ДО него
                        //        ) duration += interval.Time;
                        //}

                        if (intervalForOp.CatchedIntervals != null && intervalForOp.CatchedIntervals.Length > 0)
                            duration = NonWorkingIntervalsMasterData.GetDurationWithNonWorkingIntervals(blockRC, start, (int)duration.TotalSeconds, intervalForOp.CatchedIntervals.First()).Duration;

                        var ord = operLoc.OrderLink.OrderID.Object.Label.Length > 0 ? operLoc.OrderLink.OrderID.Object.ToString() : "нет";
                        var gp = operLoc.OrderLink.OrderID.Object.Label.Length > 0 ? operLoc.OrderLink.OrderID.Version.ToString() : "---";
                        var finastr = $"{fileN};" +
                            $"Операция;" +
                            $"{start:G};" +
                            $"{start + duration:G};" +
                            $"{duration:G};" +
                            $"{operLoc.Index};" +
                            $"{operLoc.GUID};" +
                            $"{operLoc.Registrator};" +
                            $"{currentRank};" +
                            $"[{operLoc.MesModelling}];" +
                            $"[{operLoc.CustomModelling}];" +
                            $"{operLoc.Count};" +
                            $"{operLoc.CurrentTara};" +
                            $"{operLoc.OriginalCenter};" +
                            $"{blockRC};" +
                            $"{ord};" +
                            $"{gp};" +
                            $"{operLoc.Nomenklature};" +
                            $"{operLoc.Production};" +
                            $"{operLoc.OperationVid};" +
                            $"{string.Join(";", ExistingCriterias.Select((x, index) => operLoc.RechangeParameters[index].ToString()))};" +
                            $"{operLoc.PrevDateTime};{(operLoc.PrevDateTime.HasValue ? (dateTime - operLoc.PrevDateTime.Value - (minAcceptableDate - StagesMasterData.PrevTimeDelta.Value)) : "")}";

                        rangesManual.WriteLine(finastr);
                        fileN++;
                        currentRank++;
                        offset = offset.Add(duration);
                    }

                }
            }

            rangesManual.Close();
        }


        public string RangesManualFileName { get; set; } = "ranges manual.csv";
        public string RangesManualFileNameOld { get; set; } = "ranges manual old.csv";

        public void OutputRechangesToFile(string filename, IDictionary<int, (RechangesEngineResult RechangeItself, CachedInterval Interval, int last)> RechangesBeforeFirstStages, IDictionary<int, DateTime> FirstStagesStartDate)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            var NonWorkingIntervalsMasterData = SessionLink.MasterDataLinks.WorkingIntervalsMasterData;
            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;

            var sw = new StreamWriter(filename, false, Encoding.UTF8);
            sw.WriteLine("N;ID;GUID;Time;ИдентификаторПереналадки;Start;End");
            var D = 0;
            foreach (var r in RechangesBeforeFirstStages)
            {
                var start = FirstStagesStartDate[r.Key] - r.Value.Interval.Duration;// r.StartDate;
                //var rechs = string.Join(", ", r.Item3.Select(x => RechangesIntToStr[x]));
                var normList = r.Value.RechangeItself?.ToLinearDictionary();//.SelectMany(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                if (normList != null)
                    foreach (var rech in normList)
                    {
                        int secondsNow = rech.Value.Duration;


                        if (r.Value.Interval.CatchedIntervals != null && r.Value.Interval.CatchedIntervals.Length > 0)
                            secondsNow = (int)NonWorkingIntervalsMasterData.GetDurationWithNonWorkingIntervals(ALL[r.Key].Center, start, secondsNow, r.Value.Interval.CatchedIntervals.First()).Duration.TotalSeconds;

                        //if (r.Value.Interval.CatchedIntervals != null)
                        //    foreach (var lunch in r.Value.Interval.CatchedIntervals)
                        //{
                        //    var interval = NonWorkingIntervalsMasterData.CentersFixingTime[ALL[r.Key].Center][lunch];

                        //    if (start.AddSeconds(secondsNow) >= (interval.When + interval.Time) // Конец операции дальше конца этого интервала?
                        //        && start <= interval.When // И при этом начало этой операции должно стоять ДО него
                        //        ) secondsNow += (int)interval.Time.TotalSeconds;
                        //}

                        sw.WriteLine($"{D};{ALL[r.Key].Index};{ALL[r.Key].GUID};{secondsNow};{rech.Value.Name.Code};{start:G};{start.AddSeconds(secondsNow):G}");// {RECHANGES_TIMES[ALL[r.Item1].Center][rech.]}//r.Item2};{rechs}");
                        start = start.AddSeconds(secondsNow);
                    }
                //sw.WriteLine($"{D};{ALL[r.Item1].ID};{r.Item2};{rechs}");
                D++;
            }
            sw.Close();
        }


        public string MaterialsDeficitsFileName { get; set; } = "matsErrors.csv";

        public void OutputMaterialsDeficitsToFile(string filename, OpersPool pool, IDictionary<int, DateTime> firstStageStartData)
        {
            var MaterialsMasterData = SessionLink.MasterDataLinks.LeftoversMasterData;

            if (MaterialsMasterData.CurrentLeftoversCount == null) return;

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            var sw = new StreamWriter(filename, false, Encoding.UTF8);
            sw.WriteLine("N;Номенклатура;ИдентификаторПартииПФ;КоличествоПотребность;КоличествоДоступно;ДатаПотребности;ДатаПоступления;GUID;GUIDОбъединения;GUIDОбщегоЭтапа");
            var D = 0;

            var data = MaterialsMasterData.PlanAndFixMaterialsInfo(pool, firstStageStartData);

            foreach (var element in data)
            {
                sw.WriteLine(
                    $"{D};" +
                    $"{element.Nomenklature};" +
                    $"{element.PartiaPF};" +
                    $"{element.CountNeeded};" +
                    $"{element.CountExists};" + // вернем на место
                    $"{element.DateWhenNeeded};" +
                    $"{(element.NextArrivalDate?.ToString() ?? "")};" +
                    $"{element.StageGuid};" +
                    $"{element.ModellingGuid};" +
                    $"{element.EasyLifeGuid}"
                    );
                D++;
            }

            sw.Close();
        }


        public string ZadelsFileName { get; set; } = "used_zadels.csv";

        string ZadelToString(ZadelStatus x) => x switch
        {
            ZadelStatus.CannotBeFound => "Не найдены подходящие этапы для вставки.",
            ZadelStatus.SussesfullyUsed => "Успешно использовано.",
            ZadelStatus.NotEnoughCount => "Недостаточно длины.",
            ZadelStatus.AnotherUsed => "Потребность закрыта другим заделом.",
            ZadelStatus.HigherZadelUsed => "Использован вышестоящий задел.",
        };

        public void OutputZadelsToFile(string Filename)
        {
            var sw = new StreamWriter(Filename, false, Encoding.UTF8);
            //GUID (исходного этапа имеется в виду) и ИдентификаторОбщегоЭтапа
            sw.WriteLine("ИдентификаторЗадела;GUID;НомерМоделирования;ИдентификаторОбщегоЭтапа;КоличествоИспользовано;Причина");
            foreach (var zadel in SessionLink.MasterDataLinks.ZadelsMasterData.LinearZadelsDictionary)
            {
                if (zadel.Value.WhereIsUsed.Count > 0)
                {
                    foreach (var etap in zadel.Value.WhereIsUsed)
                    {
                        var opLink = SessionLink.MasterDataLinks.StagesMasterData.AllStages[etap.operID];
                        var modGuid = opLink.IsCustomModelling ? opLink.CustomModelling : opLink.MesModelling.Code;
                        sw.WriteLine($"{zadel.Key};{opLink.GUID};{modGuid};{(opLink.GetEasyFlagToOutput())};{etap.count};"); // последняя скобка это пустота
                    }
                }
                else
                {
                    foreach (var etap in zadel.Value.WhereCanUsed)
                    {
                        sw.WriteLine($"{zadel.Key};{etap.Key};{Guid.Empty};{Guid.Empty};0;{ZadelToString(zadel.Value.ZadelStatus)}");
                    }
                }
            }
            sw.Close();
        }

    }
}

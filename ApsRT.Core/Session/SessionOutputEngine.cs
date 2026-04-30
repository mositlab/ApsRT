using ApsRT.Core.DataErrors;
using ApsRT.Core.NaladLengths;
using ApsRT.Core.Partias.PartiasClasses;
using ApsRT.Core.Partias.PartiasConnectorEngine;
using ApsRT.Core.Partias.Zadels;
using ApsRT.Core.Planning;
using ApsRT.Core.PlanSimulator;
using ApsRT.Core.PlanSimulator.PlanSimulatorEngine;
using ApsRT.Core.Rechanges.RechangesEngine;
using Azure;
using ClosedXML.Excel;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Extensions;
using MaterialUco = (UniversalPack.Common.CodedObject<System.Guid, string> Nomenklature, UniversalPack.Common.CodedObject<System.Guid, string> Characteristics);
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Session
{
    /// <summary>
    /// Движок вывода всяких данных в файлы. В отличие от других движков, которые статичны и содержат огромные методы, здесь скорее не движок, а просто совокупность методов для выводов в файлы и скуль-базы, к тому же ещё и привязанная к сессии.
    /// </summary>
    public class SessionOutputEngine
    {
        public string LogFile { get; set; } = "log.log";
        public string PartiiLogFile { get; set; } = "partii_log.log";
        public string ExitLogFile { get; set; } = "exit_log.log";

        public Session SessionLink { get; }

        public SessionOutputEngine(Session session)
        {
            SessionLink = session;
        }

        public void ClassicSettingsByOptions(SessionOptions options)
        {
            LogFile = "log " + DateTime.Now.ToString("dd.MM.yyyy H-mm-ss") + ".log";
            PartiiLogFile = "partii_log " + DateTime.Now.ToString("dd.MM.yyyy H-mm-ss") + ".log";
            ExitLogFile = "exit_log " + DateTime.Now.ToString("dd.MM.yyyy H-mm-ss") + ".log";
            OutputFileName = $"Result_{(options.IsSilent ? "" : "see")}{(options.LaunchZadel ? "part_zadel" : "")}{(options.LaunchPartia ? "part" : "")}{(options.LaunchPlan ? "plan" : "")}" +
    //$"{(string.IsNullOrEmpty(key) ? "" : "_" + key)}" +
    $".csv";
            OutputFileNameOld = OutputFileName.Substring(0, OutputFileName.Length - 4) + "_old.csv";
        }

        public void CheckFiles()
        {

            SessionLink.SessionOptions.GetPreparedOutputPath(out var outputPath);

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

            CheckFile(outputPath + RangesManualFileName, 500);
            CheckFile(outputPath + RangesManualFileNameOld, 501);
            CheckFile(outputPath + ModsFileName, 502);
            CheckFile(outputPath + ModsFileNameOld, 503);
            CheckFile(outputPath + NaladLengthsFileName, 504);
            CheckFile(outputPath + OrdersFileName, 505);
            CheckFile(outputPath + OrdersFileNameOld, 506);
            CheckFile(outputPath + SessionLink.DataErrors.DatasErrorsFile, 507, true);
            //CheckFile("dataErrorsBig.csv", 508);
            CheckFile(outputPath + RechangesFileName, 509);
            CheckFile(outputPath + OutputFileName, 510);
            CheckFile(outputPath + OutputFileNameOld, 511);
            CheckFile(outputPath + ZadelsFileName, 512);
            CheckFile(outputPath + RechangesFileNameOld, 513);
            CheckFile(outputPath + MaterialsDeficitsFileName, 514);
            CheckFile(outputPath + StatsFileName, 515);
            CheckFile(outputPath + StatsFileNameOld, 516);
            //CheckFile(SessionLink.DataErrors.PartiasErrorsFile, 514, true);
            //CheckFile(SessionLink.DataErrors.PartiasErrorsBigFile, 515, true);

            if (list.Count > 0) SessionLink.DataErrors.AddNewPartiaError(-1, NewDataErrorType.SomeFileIsOpened, new DataErrorStruct("", string.Join(", ", list)));
            if (SessionLink.DataErrors.TryOutputDataErrors())
            {
                if (SessionLink.SessionOptions.IsSilent)
                    SessionLink.EndSession(601);
            }
        }

        public (PlanSimulatorEngineResult result, List<PlanSimulatorBlock> copy, int PoolIndex) Output(bool ForcedToRecreate, bool asOld = false)
        {
            SessionLink.SessionOptions.GetPreparedOutputPath(out var outputPath);

            // Вызов вывода заделов
            try
            {
                OutputZadelsToFile(outputPath + ZadelsFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Что-то не то с выводом заделов: {e.Message}");
            }

            // Вывод всего
            var algorithm = SessionLink.AssignedPlanningAlgorithm;
            if (algorithm == null || ForcedToRecreate) SessionLink.TryCreateNewPlanningAlgorithmInstance(out algorithm, ForcedToRecreate); // Сначала просто получим текущий алгоритм планирования, потому что именно там хранится всё, что нам надо для вывода
            if (algorithm.UsedPool == null)
            {
                // Значит, алгоритм не работает. Подготовить его
                algorithm.PrepareEverything();
            }

            List<PlanSimulatorBlock> blocksToCopy = algorithm.UsedPool.Blocks.OrderBy(x => x.BlockIndex[algorithm.UsedPool.BestPoolIndex]).ToList();

            if (blocksToCopy == null)
            {
                return (null, null, -1);
            }

            lock (algorithm.LockerObject)
            {


                //var localCopy = new PlanSimulatorPool(SessionLink, blocksToCopy, new OpersPoolConstructor(blocksToCopy, algorithm.MinAcceptableDate), algorithm.MinAcceptableDate);

                var result = PlanSimulatorEngineResult.CreateFullInstance(SessionLink, algorithm.MinAcceptableDate);

                PlanSimulatorEngine.CalculatePlan(blocksToCopy, algorithm.UsedPool.BestPoolIndex, algorithm.MinAcceptableDate, result);


                OutputMaterialsDeficitsToFile(outputPath + MaterialsDeficitsFileName, blocksToCopy, result.FirstStagesStartDate, algorithm.UsedPool.BestPoolIndex);

                OutputRechangesToFile(outputPath + (asOld ? RechangesFileNameOld : RechangesFileName), result.RechangesBeforeFirstStages, result.FirstStagesStartDate);

                OutputRanksManualToFile(outputPath + (asOld ? RangesManualFileNameOld : RangesManualFileName), blocksToCopy, algorithm.UsedPool.BestPoolIndex, result, algorithm.MinAcceptableDate);

                OutputResultToFile(outputPath + (asOld ? OutputFileNameOld : OutputFileName), blocksToCopy, algorithm.UsedPool.BestPoolIndex, result);

                OutputNaladLengths(outputPath + NaladLengthsFileName, blocksToCopy, algorithm.UsedPool.BestPoolIndex, null);

                OutputOrdersToFile(outputPath + (asOld ? OrdersFileNameOld : OrdersFileName), algorithm.UsedPool.OrdersData, algorithm.UsedPool.BestPoolIndex);

                OutputMods(outputPath + (asOld ? ModsFileNameOld : ModsFileName), blocksToCopy);

                OutputPlacesCount(outputPath + (asOld ? PlacesCountFileNameOld : PlacesCountFileName), blocksToCopy);

                OutputPlacesCountAll(outputPath + (asOld ? PlacesCountAllFileNameOld : PlacesCountAllFileName), blocksToCopy);

                // Используемые остатки ПОСЛЕ фикса материалов нужно вывести
                OutputLeftovers(outputPath + UsedLeftoversFileName, string.IsNullOrEmpty(UsedLeftoversFileNameBig) ? null : (outputPath + UsedLeftoversFileNameBig), result.FirstStagesStartDate);

                OutputStats(outputPath + (asOld ? StatsFileNameOld : StatsFileName), result, algorithm.OriginalValue);

                return (result, blocksToCopy, algorithm.UsedPool.BestPoolIndex);
            }
        }

        public string ToCsvString(Uco element)
        {
            return $"{element.Label.ToString().Replace(';', '_')} [{element.Code}]";
        }

        public string StatsFileName { get; set; } = "stats.csv";
        public string StatsFileNameOld { get; set; } = "stats_old.csv";

        public void OutputStats(string filename, PlanSimulatorEngineResult result, PlanSimulatorEngineResult originalResult)
        {
            var sw = new StreamWriter(filename, false, Encoding.UTF8);

            sw.WriteLine($"Criteria;Value;ValueB;OldValue;OldValueB;DesiredValue");

            sw.WriteLine($"OrdersDeadlines;{result.DaysRuinedValue};{result.DaysRuinedValue};{originalResult.DaysRuinedValue};{originalResult.DaysRuinedValue};{0}");

            sw.WriteLine($"RechangesSeconds;{result.RechangesSeconds.TotalSeconds};{result.RechangesSeconds};{originalResult.RechangesSeconds.TotalSeconds};{originalResult.RechangesSeconds};{3600}");

            sw.WriteLine($"ProstoiSeconds;{result.ProstoiSeconds.TotalSeconds};{result.ProstoiSeconds};{originalResult.ProstoiSeconds.TotalSeconds};{originalResult.ProstoiSeconds};{0}");

            sw.WriteLine($"GlobalPlanDurationSeconds;{result.ProductionSeconds.TotalSeconds};{result.ProductionSeconds};{originalResult.ProductionSeconds.TotalSeconds};{originalResult.ProductionSeconds};{72000}");

            sw.WriteLine($"TaraRedutantUsage;{result.TaraRedutantUsage};{TimeSpan.FromSeconds(result.TaraRedutantUsage)};{originalResult.TaraRedutantUsage};{TimeSpan.FromSeconds(originalResult.TaraRedutantUsage)};{0}");

            sw.WriteLine($"NaladLengthsTotalCount;{result.NaladLengthsTotal};{result.NaladLengthsTotal};{originalResult.NaladLengthsTotal};{originalResult.NaladLengthsTotal};{0}");

            var i = result.GetCatchedNaladLengthsErrorsCount(result.Session.AssignedPlanningAlgorithm.IgnoredGlobalFixedNaladLengthsErrors);
            var i2 = originalResult.GetCatchedNaladLengthsErrorsCount(result.Session.AssignedPlanningAlgorithm.IgnoredGlobalFixedNaladLengthsErrors);
            sw.WriteLine($"NaladLengthsCatchedErrors;{i};{i};{i2};{i2};{0}");

            sw.WriteLine();

            sw.WriteLine("CriteriasV;Value");
            var Ks = SessionLink.SettingsController.CurrentSettings.VKoeffs;
            for (int k = 0; k < Ks.Length; k++)
            {
                sw.WriteLine($"K{k};{Ks[k]}");
            }

            sw.Close();
        }

        public string UsedLeftoversFileName { get; set; } = "leftoversUsed.csv";
        public string UsedLeftoversFileNameBig { get; set; } = "leftoversUsedBig.csv";
        public void OutputLeftovers(string filename, string filenameBig, IDictionary<int, DateTime> startDates)
        {
            var sw = new StreamWriter(filename, false, Encoding.UTF8);
            StreamWriter swBig = string.IsNullOrEmpty(filenameBig) ? null : new StreamWriter(filenameBig, false, Encoding.UTF8);

            sw.WriteLine($"GUID;{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсияЭтапа;" : "")}" +
                $"МатериалНоменклатура_GUID;МатериалХарактеристика_GUID;Серия_GUID;ШтрихкодСерии;" +
                $"Количество;ЕдиницаИзмерения;" +
                $"ОригМатериалНоменклатура_GUID;ОригМатериалХарактеристика_GUID;" +
                $"ОригКоличество;ДатаПланирования;РЦ;ПодразделениеРЦ;GUIDОбъединения;GUIDОбщегоЭтапа");
            swBig?.WriteLine($"Регистратор;GUID;{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсияЭтапа;" : "")}" +
                $"МатериалНоменклатура_GUID;МатериалНоменклатура;" +
                $"МатериалХарактеристика_GUID;МатериалХарактеристика;" +
                $"Серия;Серия_GUID;ШтрихкодСерии;" +
                $"Количество;ЕдиницаИзмерения;" +
                $"ОригМатериалНоменклатура_GUID;ОригМатериалНоменклатура;" +
                $"ОригМатериалХарактеристика_GUID;ОригМатериалХарактеристика;" +
                $"ОригКоличество;ДатаПланирования;РЦ;ПодразделениеРЦ;GUIDОбъединения;GUIDОбщегоЭтапа");


            foreach (var operPair in SessionLink.MasterDataLinks.StagesMasterData.AllStages)
            {
                var oper = operPair.Value;
                if (oper.StageMaterialsInfo.UsedMaterials == null) continue;
                if (!startDates.TryGetValue(oper.Index, out var date)) continue;


                var center = oper.SessionLink.AssignedPlanningAlgorithm.UsedPool.StagesLinks[oper.Index].ModLink.BlockLink.GetCenterData(oper.SessionLink.AssignedPlanningAlgorithm.UsedPool.BestPoolIndex).Center;

                    IEnumerable <Stages.Stage> mod = oper.IsCustomModelling ? SessionLink.MasterDataLinks.StagesMasterData.ApsModellingsInfo[oper.CustomModelling].RelatedOpers.Select(x => SessionLink.MasterDataLinks.StagesMasterData.AllStages[x]) : (oper.MesModelling.IsEmpty ? (new Stages.Stage[] {oper }) : SessionLink.MasterDataLinks.StagesMasterData.MesModellingsInfo[oper.MesModelling.Code].RelatedOpers.Select(x => SessionLink.MasterDataLinks.StagesMasterData.AllStages[x]));

                var smallMod = /*new PartiaModellingSmall(new List<Stages.Stage>() { oper });*/new PartiaModellingSmall(mod.ToList());

                for (int i = 0; i < oper.StageMaterialsInfo.UsedLeftovers.Count; i++)
                {
                    var element = oper.StageMaterialsInfo.UsedLeftovers[i];
                    var origMat = oper.StageMaterialsInfo.UsedMaterials[i];
                    if (element == null) continue;

                    var modCount = SessionLink.MasterDataLinks.LeftoversMasterData.GetMatSumFromMod(smallMod, origMat.AsMaterialUcoData());

                    var usedRcSubdivision = oper.SessionLink.MasterDataLinks.CentersMasterData.CentersData[center].Subdivision;

                    sw.WriteLine(
                        $"{oper.GUID};" +
                        $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{oper.StageVersion};" : "")}" +

                        $"{element.LeftoverData.Nomenklature.Code};" +
                        $"{element.LeftoverData.Characteristics.Code};" +
                        $"{element.LeftoverData.Seria.Object.Code};" +
                        $"{element.LeftoverData.Seria.Version};" +
                        $"{element.CalculatedCount};" +
                        $"{element.EdIzm.Code};" +

                        $"{origMat.Nomenklature.Code};" +
                        $"{origMat.Characteristics.Code};" +
                        $"{modCount};" +

                        $"{date};" +

                        $"{center.Code};{usedRcSubdivision.Code};" +

                        $"{oper.CurrentModelling};{oper.EasyFlag ?? Guid.Empty}"

                        );

                    swBig?.WriteLine(
                        $"{oper.Registrator};" +
                        $"{oper.GUID};" +
                        $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{oper.StageVersion};" : "")}" +

                        $"{element.LeftoverData.Nomenklature.Code};" +
                        $"{element.LeftoverData.Nomenklature.Label};" +
                        $"{element.LeftoverData.Characteristics.Code};" +
                        $"{element.LeftoverData.Characteristics.Label};" +
                        $"{element.LeftoverData.Seria.Object.Code};" +
                        $"{element.LeftoverData.Seria.Object.Label};" +
                        $"{element.LeftoverData.Seria.Version};" +
                        $"{element.CalculatedCount};" +
                        $"{element.EdIzm};" +

                        $"{origMat.Nomenklature.Code};" +
                        $"{origMat.Nomenklature.Label};" +
                        $"{origMat.Characteristics.Code};" +
                        $"{origMat.Characteristics.Label};" +
                        $"{modCount};" +

                        $"{date};" +

                        $"{center};{usedRcSubdivision};" +

                        $"{oper.CurrentModelling};{oper.EasyFlag ?? Guid.Empty}"

                        );
                }
            }


            sw.Close();
            swBig?.Close();
        }

        public string ModsFileName { get; set; } = "mods.csv";
        public string ModsFileNameOld { get; set; } = "mods_old.csv";

        public void OutputMods(string filename, List<PlanSimulatorBlock> blocks)
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
                    var first = r2.FirstOperJust;
                    var allNexts = r2.StagesAsNormal.SelectMany(x => x.Nexts).Where(x => ALL.ContainsKey(x)).Select(next1 =>
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


        public string PlacesCountFileName { get; set; } = "placesCount.csv";
        public string PlacesCountFileNameOld { get; set; } = "placesCount_old.csv";

        public void OutputPlacesCount(string filename, List<PlanSimulatorBlock> blocks)
        {
            // Ранги моделирований для алены и алексея
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            var sw = new StreamWriter(filename, false, Encoding.UTF8);
            sw.WriteLine($"N;GUID;{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсияЭтапа;" : "")}КоличествоТары;КоличествоНаТаре");
            var D = 0;

            //var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            //int D2 = 0;
            foreach (var r in blocks)
            {
                foreach (var mod in r.Lists)
                {
                    foreach (var oper in mod.StagesAsNormal)
                    {
                        if (oper.PlacesCount != oper.OriginalPlacesCount)
                        {
                            sw.WriteLine($"{D};{oper.GUID};" +
                                $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{oper.StageVersion};" : "")}" +
                                $"{oper.PlacesCount};{oper.Count}");
                            D++;
                        }
                    }
                }
            }
            sw.Close();
        }

        public string PlacesCountAllFileName { get; set; } = "placesCountAll.csv";
        public string PlacesCountAllFileNameOld { get; set; } = "placesCountAll_old.csv";

        public void OutputPlacesCountAll(string filename, List<PlanSimulatorBlock> blocks)
        {
            // Ранги моделирований для алены и алексея
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            var sw = new StreamWriter(filename, false, Encoding.UTF8);
            sw.WriteLine($"N;Объединение;КоличествоТары;КоличествоНаТаре");
            var D = 0;

            //foreach (var r in blocks)
            //{
            //    foreach (var mod in r.Lists)
            //    {
            //        foreach (var oper in mod.StagesAsNormal)
            //        {
            //            if (oper.PlacesCount != oper.OriginalPlacesCount)
            //            {
            //                sw.WriteLine($"{D};{oper.GUID};" +
            //                    $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{oper.StageVersion};" : "")}" +
            //                    $"{oper.PlacesCount};{oper.Count}");
            //                D++;
            //            }
            //        }
            //    }
            //}

            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;

            var mods = ALL.GroupBy(x => x.Value.CurrentModelling);

            foreach (var mod in mods)
            {
                if (mod.Key == Guid.Empty) continue;

                var maxPlaces = mod.Max(x => x.Value.PlacesCount);

                //if (maxPlaces == 1) continue;

                double[] count = new double[maxPlaces];

                for (int i = 0; i < maxPlaces; i++)
                {
                    count[i] = mod.Sum(x => i < x.Value.PlacesCount ? x.Value.Count : 0);
                }

                var groups = count.GroupBy(x => x);

                foreach (var group in groups)
                {
                    var placesHere = group.Count();
                    var placeCount = group.Key;
                    sw.WriteLine($"{D};{mod.Key};{placesHere};{placeCount}");
                    D++;
                }

            }

            sw.Close();
        }
        public string OrdersFileName { get; set; } = "orders.csv";
        public string OrdersFileNameOld { get; set; } = "orders_old.csv";
        
        public void OutputOrdersToFile(string filename, FrozenDictionary<OrderID, PlanSimulatorOrderData[]> orders, int PoolIndex)
        {
            //"orders_old.csv"
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }

                var sw = new StreamWriter(filename, false, Encoding.UTF8);
                sw.WriteLine("N;ID заказа;Метка;Сорван?;Количество дней;Значение;Дедлайн");
                var D = 1;
                foreach (var ord in orders)
                {
                    if (ord.Key.Object.Label.Length == 0)
                    {
                        continue;
                    }

                    var vv = ord.Value[PoolIndex].Time;
                    var v = vv > 0 ? "Да" : "Нет";
                    sw.WriteLine($"{D};{ord.Key};{ord.Key.Object.Label};{v};{vv};{ord.Value[PoolIndex].TimeWithK};{SessionLink.MasterDataLinks.OrdersMasterData.AllOrders[ord.Key].Deadline}");
                    D++;
                }
                sw.Close();
            }
            catch
            {

            }
        }

        public string NaladLengthsFileName { get; set; } = "naladLengths.csv";

        public void OutputNaladLengths(string filename, List<PlanSimulatorBlock> blocks, int PoolIndex, ConcurrentDictionary<Guid, ConcurrentDictionary<float, NaladLengthFixerClass>> naladLengthsErrors)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            var sw = new StreamWriter(filename, false, Encoding.UTF8);

            SessionLink.SessionOptions.GetPreparedOutputPath(out var outputPath);

            StreamWriter fullOutput = null;
            if (!SessionLink.SessionOptions.IsSilent && SessionLink.SettingsController.CurrentSettings.Demos_BigNaladLengthsFile)
            {
                try
                {
                    var tine = DateTime.Now;
                    fullOutput = new StreamWriter($"{outputPath}Отчет - наладочные длины.csv"/* ({tine.Year}-{tine.Month}-{tine.Day} {tine.Hour}.{tine.Minute}.{tine.Second})*/, false, Encoding.UTF8);
                }
                catch
                {
                    Console.WriteLine("Не могу подробный отчет выдать, ну и хер с ним.");
                }
            }


            sw.WriteLine($"N;GUID;{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсияЭтапа;" : "")}Место;Длина;СледующаяДлина;ПерваяВЗапуске");
            fullOutput?.WriteLine($"N;Регистратор;GUID;{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсияЭтапа;" : "")}Заказ;ГП;Партия;Уровень;Номенклатура;Вид;Продукция;РЦ;Место;Длина;СледующаяДлина;ПерваяВЗапуске;Вспом;Коэф;Колво;ЕдИзм;номер мода;Кол-воМода;НаладДлинаМода;СледНаладДлинаМода;КоэффМода;Тара этапа;Макс намотка;Влазит мод;Фикс?;Жалобы?");

            var DLitt = 1;
            var Dbig = 1;
            foreach (var block in blocks)
            {
                for (int i = 0; i < block.Lists.Count; i++)
                {
                    var list = block.Lists[i];
                    bool firstMod = true;
                    bool wasLastFlag = false;
                    Guid? lastEasyFlag = null;
                    (var center, _, _) = block.GetCenterData(PoolIndex);
                    for (int j = 0; j < list.StagesAsNormal.Count; j++)
                    {
                        var op = list.Stages[j];
                        var mod = op.Stage.CurrentModelling;//IsCustomModelling ? op.Stage.CustomModelling : op.Stage.MesModelling.Code;
                        var modNalads = mod == Guid.Empty ? op.NaladLengthData[PoolIndex] : op.ModLink.NaladLengthData[PoolIndex];
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
                        var lengths = op.NaladLengthData[PoolIndex];
                        var OP1 = op.Stage;
                        for (int place = 1; place <= OP1.PlacesCount; place++)
                        {

                            if (!OP1.NeededNaladLength.HasValue)
                            {
                                sw.WriteLine($"{DLitt};{OP1.GUID};" +
                        //$"{OP1.StageVersion};" +

                        $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{OP1.StageVersion};" : "")}" +
                                    $"{place};{(place == 1 && firstMod ? lengths.Data.NaladLengthFirst.Value : lengths.Data.NaladLengthOther.Value)};{lengths.Data.NaladLengthNexts.Value};{(place == 1 && firstMod && lengths.Data.IsFirst ? "Да" : "Нет")}");
                                DLitt++;

                            }

                            if (fullOutput != null)
                            {
                                fullOutput?.WriteLine($"{Dbig};\"{OP1.Registrator}\";{OP1.GUID};" +
                            // $"{OP1.StageVersion};" +
                            $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{OP1.StageVersion};" : "")}" +
                                    $"\"{OP1.OrderLink.OrderID.Object}\";{OP1.OrderLink.OrderID.Version};{OP1.CustomPartiaGuid ?? OP1.OrderLink.PartiaPF ?? Guid.Empty};" +
                                    $"{OP1.Level};{ToCsvString(OP1.Nomenklature)};{ToCsvString(OP1.OperationVid)};{ToCsvString(OP1.Production)};{center};" +
                                    $"{place};{(place == 1 && firstMod ? lengths.Data.NaladLengthFirst.Value : lengths.Data.NaladLengthOther.Value)};{lengths.Data.NaladLengthNexts.Value};{(place == 1 && firstMod && lengths.Data.IsFirst ? "Да" : "Нет")}" +
                                    $";{OP1.IsVspom};{lengths.Data.prevKoeff};{OP1.Count};{list.FirstOperJust.EdIzmClassificator};" +
                                    $"{list.ModID};" +
                                    $"{list.Count};" +
                                    $"{modNalads.Data.NaladLengthHere};" +
                                    $"{modNalads.Data.NaladLengthNexts};" +
                                    $"{modNalads.Data.prevKoeff};" +
                                    $"{OP1.CurrentTara};" +
                                    $"{OP1.CurrentTaraMaxCapacity};" +
                                    $"{(OP1.IsVspom || OP1.NoChangeTara || !OP1.EndTara.IsEmpty ? "" : (OP1.CurrentTaraMaxCapacity >= modNalads.Data.NaladLengthHere.Value + list.Count))};" +
                                    $"{(OP1.NeededNaladLength.HasValue ? OP1.NeededNaladLength.Value.Length.Value.ToString() : "Нет")}"/*;{CatchedErrors != null && CatchedErrors.ContainsKey(list.ModID)}*/);
                                Dbig++;
                            }

                        }
                    }
                    //firstMod = false;
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

        public void OutputResultToFile(string filename, List<PlanSimulatorBlock> Blocks, int PoolIndex, PlanSimulatorEngineResult dataForHelp)
        {
            var outputFile = new StreamWriter(filename, false, Encoding.UTF8);

            if (SessionLink.SettingsController.CurrentSettings.Demos_NO_WHITESPACES_OnOutput)
            {
                outputFile.WriteLine($"GUID;{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсияЭтапа;" : "")}Номенклатура;{(SessionLink.SettingsController.CurrentSettings.Demos_CharacteristicsOnOutput ? "ХарактеристикаНоменклатуры;ХарактеристикаПродукции;" : "")}ЗП;ПартияГП;Спецификация;" +
                        $"Продукция;Количество;НомерПартии;ДебагНомерпартии;НомерМоделирования;" +
                        $"ДебагНомерМода;Подразделение;КоличествоТары;ТипТары;МаксимальнаяНамотка;РЦ;РабЦентрМаршрута;" +
                        $"ВремяНачалаОбъединения;ВремяОкончанияОбъединения;ДлительностьЧ;ЕдиницаИзмерения;" +
                        $"ИдентификаторОбщегоЭтапа;ЕстьОшибки;БылПропущен");
            }
            else
            {
                outputFile.WriteLine($"GUID;{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсияЭтапа;" : "")}Номенклатура;{(SessionLink.SettingsController.CurrentSettings.Demos_CharacteristicsOnOutput ? "ХарактеристикаНоменклатуры;ХарактеристикаПродукции;" : "")}ЗП;Партия ГП;Спецификация;" +
                        $"Продукция;Количество;НомерПартии;ДебагНомерпартии;НомерМоделирования;" +
                        $"ДебагНомерМода;Подразделение;КоличествоТары;Тип тары;Максимальная Намотка;РЦ;Раб центр маршрута;" +
                        $"Время Начала Объединения;Время Окончания Объединения;ДлительностьЧ;ЕдиницаИзмерения;ИдентификаторОбщегоЭтапа;ЕстьОшибки;БылПропущен");
            }


            bool IsSilent = SessionLink.SessionOptions.IsSilent;
            bool LaunchPlan = SessionLink.SessionOptions.LaunchPlan;
            var partiasAlgorithm = SessionLink.AssignedPartiasAlgorithm;

            for (int i = 0; i < Blocks.Count; i++)
            {
                var block = Blocks[i];
                (var blockRC, _, _) = block.GetCenterData(PoolIndex);

                // Группируем по дате начала (у параллелок она одинаковая), а затем внутри них ещё и по
                var sortedGroups = block.Lists.GroupBy(x => dataForHelp.FirstStagesStartDate[x.FirstOperJust.Index]);

                foreach (var group in sortedGroups) // Пробежимся по каждой группе
                {
                    var start = group.Key;
                    var maxTimeWithFixes = group.Max(x => /*x.WasPassed[PoolIndex] ? TimeSpan.Zero :*/ dataForHelp.FirstStagesFullDurations[x.FirstOperJust.Index].Duration);
                    foreach (var mod in group)
                    {

                        for (int op = 0; op < mod.StagesAsNormal.Count; op++)
                        {
                            var operLoc = mod.StagesAsNormal[op];

                            var guidToCheck = operLoc.CustomPartiaGuid ?? (operLoc.OrderLink.PartiaPF ?? Guid.Empty);

                            if (partiasAlgorithm == null || !partiasAlgorithm.PartiaHasErrors.TryGetValue(guidToCheck, out var errors) || !errors)
                            {
                                Guid ord = operLoc.OrderLink.OrderID.Object.Label.Length > 0 ? operLoc.OrderLink.OrderID.Object.Code : Guid.Empty;
                                var gp = operLoc.OrderLink.OrderID.Object.Label.Length > 0 ? operLoc.OrderLink.OrderID.Version : "";
                                outputFile.WriteLine($"{operLoc.GUID};" +
                                    $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{operLoc.StageVersion};" : "")}" +
                                    $"{operLoc.Nomenklature.Code};{(SessionLink.SettingsController.CurrentSettings.Demos_CharacteristicsOnOutput ? $"{operLoc.Characteristics.Code};{operLoc.ProductionCharacteristics.Code};" : "")}{ord};{gp};{operLoc.Specification.Code};" +
                                    $"{operLoc.Production.Code};{operLoc.CountMES};" +
                                    $"{guidToCheck}" +
                                    $";;{(operLoc.CurrentModelling/*.IsCustomModelling ? operLoc.CustomModelling : operLoc.MesModelling.Code*/)};" +
                                    $";{operLoc.SessionLink.MasterDataLinks.CentersMasterData.CentersData[blockRC].Subdivision.Code};" +
                                    $"{operLoc.PlacesCount};" +
                                    $"{operLoc.CurrentTara.Code};{operLoc.CurrentTaraMaxCapacity};{blockRC.Code};" +//{operFirst.CurrentTara.Code};{operFirst.MaxNamotka};{operFirst.Center.Code};" +
                                    $"{(operLoc.RCRoute.Code/*operLoc.RCRoute.IsEmpty ? "" : operLoc.RCRoute.Code*/)};" +
                                    (!IsSilent || LaunchPlan ? $"{start:G}" : "") + // 04.06.2024: требование одну дату для всех исходных этапчиков выводить, которые объеиднились в один мод или общий этап.
                                    $";" +
                                    (!IsSilent || LaunchPlan ? $"{start + maxTimeWithFixes:G}" : "") +//$"{(centStruct.LastDate + offset + secs:G)}" +
                                    $";{maxTimeWithFixes.TotalHours}" +
                                    $";{operLoc.EdIzmClassificator.Code};{operLoc.GetEasyFlagToOutput()}" +
                                    $";{(partiasAlgorithm != null && partiasAlgorithm.ThisOperHasPartiaErrors(operLoc) ? "Да" : "Нет")}" +
                                    $";{(mod.WasPassed[PoolIndex] ? "Да" : "Нет")}");
                            }
                        }
                    }
                }
            }


            outputFile.Close();

        }

        public string RangesManualFileName { get; set; } = "ranges manual.csv";
        public string RangesManualFileNameOld { get; set; } = "ranges manual old.csv";

        public void OutputRanksManualToFile(string filename, List<PlanSimulatorBlock> Blocks, int PoolIndex, PlanSimulatorEngineResult dataForHelp, DateTime minAcceptableDate)
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
                "ДлительностьСек;" +
                "ID;" +
                "GUID;" +
                $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсияЭтапа;" : "")}" +
                "Регистратор;" +
                "Ранг;" +
                "Моделирование;" +
                "Кастомное моделирование;" +
                "Общий этап;" +
                "Количество;" +
                "Тара;" +
                "Стандартный РЦ;" +
                "Выбранный РЦ;" +
                "ГруппаНаОдномРЦ;" +
                "ПодразделениеРЦ;" +
                "Заказ;" +
                "Партия ГП;" +
                "ПартияПФ;" +
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
            for (int i = 0; i < Blocks.Count; i++)
            {
                var block = Blocks[i];
                (var blockRC, _, _) = block.GetCenterData(PoolIndex);
                var sortedMods = block.Lists.OrderBy(x => x.Ranks[PoolIndex]).ToArray(); // Получив их ранги, мы можем точно понять, какое моделирование было первым, и так далее. Перед каждым моделированием, мы можем посмотреть, были ли перед ним простои и переналадки, взяв у его первой операции айдишник, а ещё дату начала.
                for (int mod = 0; mod < sortedMods.Length; mod++)
                {
                    var currMod = sortedMods[mod];
                    var nextOper = currMod.FirstOperJust;
                    var firstID = nextOper.Index;
                    (PlanSimulatorDatesData data, CachedInterval interval) prostoi;
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
                            $"{prostoi.interval.Duration:G};" +
                            $"{(int)prostoi.interval.Duration.TotalSeconds}" +
                            $";;;;;;;;;;;" +

                        $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $";" : "")}" +
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
                            $"{rechanges.interval.Duration:G};" +
                            $"{(int)rechanges.interval.Duration.TotalSeconds}" +
                            $";{nextOper.Index};{nextOper.GUID};" +
                            //$"{nextOper.StageVersion};" +

                        $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{nextOper.StageVersion};" : "")}" +
                            $"{nextOper.Registrator}" +
                            $";;;;;;;;{blockRC};;;;;;;;;" +

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
                    int currentRank = currMod.Ranks[PoolIndex];//dataForHelp.FirstStagesRanks[firstID];
                    var intervalForOp = dataForHelp.FirstStagesFullDurations[firstID];
                    for (int op = 0; op < currMod.StagesAsNormal.Count; op++)
                    {
                        var operLoc = currMod.StagesAsNormal[op];
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
                            $"{(int)duration.TotalSeconds};" +
                            $"{operLoc.Index};" +
                            $"{operLoc.GUID};" +
                           // $"{operLoc.StageVersion};" +

                        $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{operLoc.StageVersion};" : "")}" +
                            $"{operLoc.Registrator};" +
                            $"{currentRank};" +
                            $"[{operLoc.MesModelling}];" +
                            $"[{operLoc.CustomModelling}];" +
                            $"{operLoc.EasyFlag};" +
                            $"{operLoc.Count};" +
                            $"{operLoc.CurrentTara};" +
                            $"{operLoc.OriginalCenter};" +
                            $"{blockRC};" +
                            $"{(block.PoolLink.ModIDToOnOneRCGroupID.TryGetValue(currMod.ModID, out var group) ? group.ToString() : "-1")};" +
                            $"{SessionLink.MasterDataLinks.CentersMasterData.CentersData[blockRC].Subdivision};" +
                            $"{ord};" +
                            $"{gp};" +
                            $"{operLoc.GetOrderOrPartiaCode()};" +
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


        public string RechangesFileName { get; set; } = "rechanges.csv";
        public string RechangesFileNameOld { get; set; } = "rechanges_old.csv";

        public void OutputRechangesToFile(string filename, IDictionary<int, (RechangesEngineResult RechangeItself, CachedInterval Interval, int last)> RechangesBeforeFirstStages, IDictionary<int, DateTime> FirstStagesStartDate)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            var NonWorkingIntervalsMasterData = SessionLink.MasterDataLinks.WorkingIntervalsMasterData;
            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;

            var sw = new StreamWriter(filename, false, Encoding.UTF8);
            sw.WriteLine($"N;ID;GUID;{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсияЭтапа;" : "")}Time;ИдентификаторПереналадки;Start;End");
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

                        sw.WriteLine($"{D};{ALL[r.Key].Index};{ALL[r.Key].GUID};" +
                            $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{ALL[r.Key].StageVersion};" : "")}" +
                            $"{secondsNow};{rech.Value.Name.Code};{start:G};{start.AddSeconds(secondsNow):G}");// {RECHANGES_TIMES[ALL[r.Item1].Center][rech.]}//r.Item2};{rechs}");
                        start = start.AddSeconds(secondsNow);
                    }
                //sw.WriteLine($"{D};{ALL[r.Item1].ID};{r.Item2};{rechs}");
                D++;
            }
            sw.Close();
        }


        public string MaterialsDeficitsFileName { get; set; } = "matsErrors.csv";

        public void OutputMaterialsDeficitsToFile(string filename, List<PlanSimulatorBlock> Blocks, IDictionary<int, DateTime> firstStageStartData, int PoolIndex)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Расход остатков");
                worksheet.Row(1).Cell(1).Value = "ОригиналМатериалНоменклатура_GUID";
                worksheet.Row(1).Cell(2).Value = "ОригиналМатериалНоменклатура";
                worksheet.Row(1).Cell(3).Value = "ОригиналМатериалХарактеристика_GUID";
                worksheet.Row(1).Cell(4).Value = "ОригиналМатериалХарактеристика";
                worksheet.Row(1).Cell(5).Value = "МатериалНоменклатура_GUID";
                worksheet.Row(1).Cell(6).Value = "МатериалНоменклатура";
                worksheet.Row(1).Cell(7).Value = "МатериалХарактеристика_GUID";
                worksheet.Row(1).Cell(8).Value = "МатериалХарактеристика";
                worksheet.Row(1).Cell(9).Value = "МатериалСерия_GUID";
                worksheet.Row(1).Cell(10).Value = "МатериалСерия";
                worksheet.Row(1).Cell(11).Value = "ШтрихкодСерии";
                worksheet.Row(1).Cell(12).Value = "ИдентификаторПартииПФ";
                worksheet.Row(1).Cell(13).Value = "КоличествоПотребность";
                worksheet.Row(1).Cell(14).Value = "КоличествоПотребностьАналог";
                worksheet.Row(1).Cell(15).Value = "КоличествоПервогоЭтапа";
                worksheet.Row(1).Cell(16).Value = "КоличествоДоступно";
                worksheet.Row(1).Cell(17).Value = "ЕдиницаИзмерения";
                worksheet.Row(1).Cell(18).Value = "ДатаПотребности";
                worksheet.Row(1).Cell(19).Value = "ДатаПоступления";
                worksheet.Row(1).Cell(20).Value = "GUID";
                int i = 21;
                if (SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput)
                {
                    worksheet.Row(1).Cell(i).Value = "ВерсияЭтапа";
                    i++;
                }
                worksheet.Row(1).Cell(i).Value = "GUIDОбъединения";
                worksheet.Row(1).Cell(i + 1).Value = "КоличествоЭтапов";
                worksheet.Row(1).Cell(i + 2).Value = "GUIDОбщегоЭтапа";
                worksheet.Row(1).Cell(i + 3).Value = "РЦ_GUID";
                worksheet.Row(1).Cell(i + 4).Value = "РЦ";
                worksheet.Row(1).Cell(i + 5).Value = "ПодразделениеРЦ_GUID";
                worksheet.Row(1).Cell(i + 6).Value = "ПодразделениеРЦ";
                worksheet.Row(1).Cell(i + 7).Value = "ЭтоДефицит";


                var sw = new StreamWriter(filename, false, Encoding.UTF8);
                sw.WriteLine(
                    $"ОригиналМатериалНоменклатура_GUID;ОригиналМатериалНоменклатура;" +
                    $"ОригиналМатериалХарактеристика_GUID;ОригиналМатериалХарактеристика;" +
                    $"МатериалНоменклатура_GUID;МатериалНоменклатура;" +
                    $"МатериалХарактеристика_GUID;МатериалХарактеристика;" +
                    $"МатериалСерия_GUID;МатериалСерия;ШтрихкодСерии;" +
                    //$"ПодразделениеМатериала;" +
                    $"ИдентификаторПартииПФ;" +
                    $"КоличествоПотребность;" +
                    $"КоличествоПотребностьАналог;" +
                    $"КоличествоПервогоЭтапа;" +
                    $"КоличествоДоступно;" +
                    $"ЕдиницаИзмерения;" +
                    $"ДатаПотребности;ДатаПоступления;" +
                    $"GUID;{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсияЭтапа;" : "")}GUIDОбъединения;КоличествоЭтапов;GUIDОбщегоЭтапа;" +
                    $"РЦ_GUID;РЦ;ПодразделениеРЦ_GUID;ПодразделениеРЦ;" +
                    $"ЭтоДефицит");

                var data = SessionLink.MasterDataLinks.LeftoversMasterData.PlanAndFixMaterialsInfo(Blocks, firstStageStartData, PoolIndex);

                var stagesMasterData = SessionLink.MasterDataLinks.StagesMasterData;

                int rowIndex = 2;
                foreach (var element in data)
                {
                    var opIndex = stagesMasterData.TryGetStageIndexByGuid(element.StageGuid.Object.Code, element.StageGuid.Version);

                    var opLink = stagesMasterData.AllStages[opIndex];

                    var opsCount = opLink.IsCustomModelling ? stagesMasterData.ApsModellingsInfo[opLink.CustomModelling].RelatedOpers.Count : (opLink.MesModelling.IsEmpty ? 1 : stagesMasterData.MesModellingsInfo[opLink.MesModelling.Code].RelatedOpers.Count);

                    var subdiv = SessionLink.MasterDataLinks.CentersMasterData.CentersData[element.Center].Subdivision;

                    sw.WriteLine(
                        //$"{D};" +

                        $"{element.OrigMaterialData.Nomenklature.Code};" +
                        $"{element.OrigMaterialData.Nomenklature.Label};" +
                        $"{element.OrigMaterialData.Characteristics.Code};" +
                        $"{element.OrigMaterialData.Characteristics.Label};" +

                        $"{element.LeftoverData.Nomenklature.Code};" +
                        $"{element.LeftoverData.Nomenklature.Label};" +
                        $"{element.LeftoverData.Characteristics.Code};" +
                        $"{element.LeftoverData.Characteristics.Label};" +

                        $"{element.LeftoverData.Seria.Object.Code};" +
                        $"{element.LeftoverData.Seria.Object.Label};" +
                        $"{element.LeftoverData.Seria.Version};" +

                        //$"{element.MaterialSubdivision};" +

                        $"{element.PartiaPF};" +
                        $"{element.ORIGCountNeeded};" +
                        $"{element.CountNeeded};" +
                        $"{opLink.Count};" +
                        $"{element.CountExists};" + // вернем на место
                        $"{element.EdIzm.Code};" +
                        $"{element.DateWhenNeeded};" +
                        $"{(element.NextArrivalDate?.ToString() ?? "")};" +
                        $"{element.StageGuid.Object.Code};" +
                        $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{element.StageGuid.Version};" : "")}" +
                        $"{element.ModellingGuid};" +
                        $"{opsCount};" +
                        $"{element.EasyLifeGuid};" +
                        $"{element.Center.Code};{element.Center.Label};{subdiv.Code};{subdiv.Label};" +
                        $"{(element.ItsError ? "Да" : "Нет")}"
                        );

                    worksheet.Row(rowIndex).Cell(1).Value = $"{element.OrigMaterialData.Nomenklature.Code}";
                    worksheet.Row(rowIndex).Cell(2).Value = $"{element.OrigMaterialData.Nomenklature.Label}";
                    worksheet.Row(rowIndex).Cell(3).Value = $"{element.OrigMaterialData.Characteristics.Code}";
                    worksheet.Row(rowIndex).Cell(4).Value = $"{element.OrigMaterialData.Characteristics.Label}";
                    worksheet.Row(rowIndex).Cell(5).Value = $"{element.LeftoverData.Nomenklature.Code}";
                    worksheet.Row(rowIndex).Cell(6).Value = $"{element.LeftoverData.Nomenklature.Label}";
                    worksheet.Row(rowIndex).Cell(7).Value = $"{element.LeftoverData.Characteristics.Code}";
                    worksheet.Row(rowIndex).Cell(8).Value = $"{element.LeftoverData.Characteristics.Label}";
                    worksheet.Row(rowIndex).Cell(9).Value = $"{element.LeftoverData.Seria.Object.Code}";
                    worksheet.Row(rowIndex).Cell(10).Value = $"{element.LeftoverData.Seria.Object.Label}";
                    worksheet.Row(rowIndex).Cell(11).Value = $"{element.LeftoverData.Seria.Version}";
                    worksheet.Row(rowIndex).Cell(12).Value = $"{element.PartiaPF}";
                    worksheet.Row(rowIndex).Cell(13).Value = $"{element.ORIGCountNeeded}";
                    worksheet.Row(rowIndex).Cell(14).Value = $"{element.CountNeeded}";
                    worksheet.Row(rowIndex).Cell(15).Value = $"{opLink.Count}";
                    worksheet.Row(rowIndex).Cell(16).Value = $"{element.CountExists}"; // вернем на место
                    worksheet.Row(rowIndex).Cell(17).Value = $"{element.EdIzm.Code}";
                    worksheet.Row(rowIndex).Cell(18).Value = $"{element.DateWhenNeeded}";
                    worksheet.Row(rowIndex).Cell(19).Value = $"{(element.NextArrivalDate?.ToString() ?? "")}";
                    worksheet.Row(rowIndex).Cell(20).Value = $"{element.StageGuid.Object.Code}";
                    i = 21;
                    if (SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput)
                    {
                        worksheet.Row(rowIndex).Cell(i).Value = $"{element.StageGuid.Version}";
                        i++;
                    }
                    worksheet.Row(rowIndex).Cell(i).Value = $"{element.ModellingGuid}";
                    worksheet.Row(rowIndex).Cell(i + 1).Value = $"{opsCount}";
                    worksheet.Row(rowIndex).Cell(i + 2).Value = $"{element.EasyLifeGuid}";
                    worksheet.Row(rowIndex).Cell(i + 3).Value = $"{element.Center.Code}";
                    worksheet.Row(rowIndex).Cell(i + 4).Value = $"{element.Center.Label}";
                    worksheet.Row(rowIndex).Cell(i + 5).Value = $"{subdiv.Code}";
                    worksheet.Row(rowIndex).Cell(i + 6).Value = $"{subdiv.Label}";
                    worksheet.Row(rowIndex).Cell(i + 7).Value = $"{(element.ItsError ? "Да" : "Нет")}";


                    rowIndex++;
                }

                sw.Close();
                workbook.SaveAs("matsErrors.xlsx");
            }

            //var D = 0;

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
            sw.WriteLine($"ИдентификаторЗадела;Тара;Тара_GUID;Штрихкод;ИдентификаторЭтапа;GUID;{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? "ВерсияЭтапа;" : "")}НомерМоделирования;ИдентификаторОбщегоЭтапа;НомерМеста;КоличествоИспользовано;Причина");
             Dictionary<int, int> stagesPlacesCounter = new ();
            foreach (var zadel in SessionLink.MasterDataLinks.ZadelsMasterData.LinearZadelsDictionary)
            {
                if (zadel.Value.WhereIsUsed.Count > 0)
                {
                    foreach (var etap in zadel.Value.WhereIsUsed)
                    {
                        var opLink = SessionLink.MasterDataLinks.StagesMasterData.AllStages[etap.operID];
                        stagesPlacesCounter.TryAdd(opLink.Index, 0);
                        var modGuid = opLink.CurrentModelling;//IsCustomModelling ? opLink.CustomModelling : opLink.MesModelling.Code;
                        sw.WriteLine($"{zadel.Key};{zadel.Value.JustTara.Label};{zadel.Value.JustTara.Code};" +
                            $"{zadel.Value.SeriaCode};" +
                            $"{zadel.Value.WhereCanUsed.First().Key.Object.Code};" +
                            $"{opLink.GUID};" +
                            $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{opLink.StageVersion};" : "")}" +
                            $"{modGuid};{(opLink.GetEasyFlagToOutput())};{stagesPlacesCounter[opLink.Index] + 1};{etap.count};"); // последняя скобка это пустота
                        stagesPlacesCounter[opLink.Index]++;

                    }
                }
                else
                {
                    if (!SessionLink.SessionOptions.LaunchZadel) continue; // При свободных заделах не вывожу - апс не ищет.
                    foreach (var etap in zadel.Value.WhereCanUsed)
                    {
                        sw.WriteLine($"{zadel.Key};{zadel.Value.UsedTara.Label};{zadel.Value.UsedTara.Code};" +
                            $"{zadel.Value.SeriaCode};" +
                            $"{zadel.Value.WhereCanUsed.First().Key.Object.Code};" +
                            $"{etap.Key};" +
                            $"{Guid.Empty};{Guid.Empty};0;0;{ZadelToString(zadel.Value.ZadelStatus)}");
                    }
                }
            }

            // Ну и ещё и остатки выведем - они тоже закрывают этапы
            foreach (var zadel in SessionLink.MasterDataLinks.LeftoversMasterData.UsedLeftoversFromBegginingData)
            {
                foreach (var op in zadel.Value)
                {
                    var opLink = SessionLink.MasterDataLinks.StagesMasterData.AllStages[op.Stage];
                    sw.WriteLine($"{zadel.Key};{""};{Guid.Empty};" +
                        $"{zadel.Key};" +
                        $"{Guid.Empty};" +
                        $"{opLink.GUID};" +
                        $"{(SessionLink.SettingsController.CurrentSettings.Demos_StagesVersionsOnOutput ? $"{opLink.StageVersion};" : "")}" +
                        $"{opLink.CurrentModelling};{(opLink.GetEasyFlagToOutput())};{op.Place + 1};{opLink.Count};"); // последняя скобка это пустота
                }
            }

            sw.Close();
        }

    }
}

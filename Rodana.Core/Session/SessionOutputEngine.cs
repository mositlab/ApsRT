using Rodana.Core.DataErrors;
using Rodana.Core.Partias.Zadels;
using Rodana.Core.Planning.PlanningCalculatorEngine;
using Rodana.Core.Planning.PlanningClasses;
using Rodana.Core.Planning.PlanningClasses.PoolClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Session
{
    /// <summary>
    /// Движок вывода всяких данных в файлы. В отличие от других движков, которые статичны и содержат огромные методы, здесь скорее не движок, а просто совокупность методов для выводов в файлы и скуль-базы, к тому же ещё и привязанная к сессии.
    /// </summary>
    public class SessionOutputEngine
    {
        public string LogFile = "logs/log.txt";
        public string PartiiLogFile = "logs/partii_log.txt";
        public string ExitLogFile = "logs/exit_log.txt";
        public string OutputFile = "Result.csv";
        public string OutputFileOld = "Result_old.csv";

        public Session SessionLink { get; }

        public SessionOutputEngine(Session session)
        {
            SessionLink = session;
        }

        public void ClassicSettingsByOptions(SessionOptions options)
        {
            if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
            LogFile = "logs/log " + DateTime.Now.ToString("dd.MM.yyyy H-mm-ss") + ".txt";
            PartiiLogFile = "logs/partii_log " + DateTime.Now.ToString("dd.MM.yyyy H-mm-ss") + ".txt";
            ExitLogFile = "logs/exit_log " + DateTime.Now.ToString("dd.MM.yyyy H-mm-ss") + ".txt";
            OutputFile = $"Result_{(options.IsSilent ? "" : "see")}{(options.LaunchZadel ? "part_zadel" : "")}{(options.LaunchPartia ? "part" : "")}{(options.LaunchPlan ? "plan" : "")}" +
    //$"{(string.IsNullOrEmpty(key) ? "" : "_" + key)}" +
    $".csv";
            OutputFileOld = OutputFile.Substring(0, OutputFile.Length - 4) + "_old.csv";
        }

        public void CheckFiles()
        {
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

            CheckFile("ranges manual.csv", 500);
            CheckFile("ranges manual old.csv", 501);
            CheckFile("mods.csv", 502);
            CheckFile("mods_old.csv", 503);
            CheckFile("naladLengths.csv", 504);
            CheckFile("orders.csv", 505);
            CheckFile("orders_old.csv", 506);
            CheckFile(SessionLink.DataErrors.DatasErrorsFile, 507, true);
            //CheckFile("dataErrorsBig.csv", 508);
            CheckFile("rechanges.csv", 509);
            CheckFile(OutputFile, 510);
            CheckFile(OutputFileOld, 511);
            CheckFile(ZadelsName, 512);
            CheckFile("rechanges_old.csv", 513);
            //CheckFile(SessionLink.DataErrors.PartiasErrorsFile, 514, true);
            //CheckFile(SessionLink.DataErrors.PartiasErrorsBigFile, 515, true);

            if (list.Count > 0) SessionLink.DataErrors.AddNewPartiaError(-1, NewDataErrorType.SomeFileIsOpened, new DataErrorStruct("", string.Join(", ", list)));
            if (SessionLink.DataErrors.TryOutputDataErrors())
            {
                SessionLink.EndSession(601);
            }
        }

        public (PlanningCalculatorEngineResult result, OpersPool copy) Output(bool asOld = false)
        {
            // Вызов вывода заделов
            if (SessionLink.SessionOptions.LaunchZadel)
            {
                try
                {
                    OutputZadelsToFile(ZadelsName);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Что-то не то с выводом заделов: {e.Message}");
                }
            }

            // Вывод всего
            SessionLink.TryCreateNewPlanningAlgorithmInstance(out var algorithm); // Сначала просто получим текущий алгоритм планирования, потому что именно там хранится всё, что нам надо для вывода
            if (algorithm.BestBlocksList == null)
            {
                // Значит, алгоритм не работает. Подготовить его
                algorithm.PrepareEverything();
            }

            List<OpersBlock> blocksToCopy = algorithm.BestBlocksList;

            var localCopy = new OpersPool(blocksToCopy, new OpersPoolConstructor(blocksToCopy, algorithm.MinAcceptableDate), algorithm.MinAcceptableDate);

            //lock (locker)
            {

                var result = PlanningCalculatorEngineResult.CreateFullInstance(SessionLink, algorithm.MinAcceptableDate);

                PlanningCalculatorEngine.CalculatePlan(localCopy, algorithm.MinAcceptableDate, result);

                //OutputAllToFiles(localCopy, result, files);

                return (result, localCopy);
            }
        }


        public string ZadelsName { get; set; } = "used_zadels.csv";

        string ZadelToString(ZadelStatus x) => x switch
        {
            ZadelStatus.CannotBeFound => "Не найдены подходящие этапы для вставки.",
            ZadelStatus.SussesfullyUsed => "Успешно использовано.",
            ZadelStatus.NotEnoughCount => "Недостаточно длины.",
            ZadelStatus.AnotherUsed => "Потребность закрыта другим заделом.",
            ZadelStatus.HigherZadelUsed => "Использован вышестоящий задел.",
        };

        internal void OutputZadelsToFile(string Filename)
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
                        sw.WriteLine($"{zadel.Key};{opLink.GUID};{modGuid};{(opLink.EasyFlag.HasValue ? opLink.EasyFlag.Value : Guid.Empty)};{etap.count};"); // последняя скобка это пустота
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

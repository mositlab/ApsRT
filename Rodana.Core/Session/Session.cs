using Rodana.Core.DataErrors;
using Rodana.Core.Hoppers;
using Rodana.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using static System.Collections.Specialized.BitVector32;

namespace Rodana.Core.Session
{
    /// <summary>
    /// Класс сессии. Каждая сессия - это по сути и есть экземпляр АПС. 
    /// В ней создаются классы переналадок и прочего во время вызова конструктора.
    /// Сессия нуждается в загрузке данных через вызов метода Init в ней
    /// После того, как Родана загрузит настройки и с их помощью активирует Хопперы.
    /// Теперь, запуская алгоритмы планирования и партий, можно хавать данные из данных сессии.
    /// </summary>
    public class Session<InputDataType, OrderSubType>
    {
        public SessionOptions SessionOptions { get; init; }
        public DataErrors<InputDataType, OrderSubType> DataErrors { get; init; }
        public SettingsController SettingsController { get; init; }
        public HoppersHandler<InputDataType, OrderSubType> HoppersHandler { get; init; }
        public MasterDataLinks<InputDataType> MasterDataLinks { get; init; }

        public ConcurrentLogger<InputDataType, OrderSubType> AssignedLogger { get; init; }

        public event Action OnSessionDataLoaded;

        Stopper KillMe;
        Stopper StopMe;


        static bool FirstInvoked = false;
        static void StaticStart()
        {
            if (FirstInvoked) return;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            FirstInvoked = true;
        }

        public Session()
        {
            SettingsController = new SettingsController();
            MasterDataLinks = new();
            SessionOptions = new();
            DataErrors = new(this);
            HoppersHandler = new(this);
            AssignedLogger = new ConcurrentLogger<InputDataType, OrderSubType>(this);
        }

        static SessionOptions ParseCommandLine(string[] args)
        {
            args = args.SelectMany(x => x.Split(',')).ToArray();
            SessionOptions opts = new();

            Console.WriteLine("args:");
            for (var i = 0; i < args.Length; i++)
            {
                Console.WriteLine(args[i]);
                //ConcurrentLogger.AddToLogQueue.Enqueue($"arg: {args[i]}\r\n");
            }
            Console.WriteLine("end");
            //Guid? guid = null;
            //var key = "";

            // Использование библиотек CommandLineParser считаю излишним.

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i].Trim().ToLower();
                if (arg == "silent")
                {
                    //ConcurrentLogger.AddToLogQueue.Enqueue($"arg: is silent\r\n");
                    opts.IsSilent = true;
                    continue;
                }
                if (arg == "part")
                {
                    //ConcurrentLogger.AddToLogQueue.Enqueue($"arg: has part\r\n");
                    opts.LaunchPartia = true;
                    continue;
                }
                if (arg == "plan")
                {
                    //ConcurrentLogger.AddToLogQueue.Enqueue($"arg: has plan\r\n");
                    opts.LaunchPlan = true;
                    continue;
                }
                if (arg == "part_zadel")
                {
                    //ConcurrentLogger.AddToLogQueue.Enqueue($"arg: has zadel\r\n");
                    opts.LaunchZadel = true;
                    continue;
                }
                if (arg == "jsort")
                {
                    opts.Jsort = true;
                    opts.IsSilent = true;
                    continue;
                }
                //if (arg.StartsWith("guid:"))
                //{
                //    guid = Guid.Parse(arg.Substring(5));
                //    continue;
                //}
                //if (arg.StartsWith("key:"))
                //{
                //    key = arg.Substring(4);
                //    continue;
                //}
                //if (arg.StartsWith("path:"))
                //{
                //    Directory.SetCurrentDirectory(args[i].Substring(5));
                //    continue;
                //}
                //if (Path.GetFileNameWithoutExtension(arg).ToLower().StartsWith("settings"))
                //{
                //    Settings.SETTINGS_PATH = arg;
                //    continue;
                //}
            }

            return opts;
        }

        public Session(string[] args) : this(ParseCommandLine(args)) { }

        public Session(SessionOptions options) : this()
        {
            SessionOptions = options;
            if (!Directory.Exists("logs")) Directory.CreateDirectory("logs");
            options.LogFile = "logs/log " + DateTime.Now.ToString("dd.MM.yyyy H-mm-ss") + ".txt";
            options.PartiiLogFile = "logs/partii_log " + DateTime.Now.ToString("dd.MM.yyyy H-mm-ss") + ".txt";
            options.ExitLogFile = "logs/exit_log " + DateTime.Now.ToString("dd.MM.yyyy H-mm-ss") + ".txt";
            options.OutputFile = $"Result_{(options.IsSilent ? "" : "see")}{(options.LaunchZadel ? "part_zadel" : "")}{(options.LaunchPartia ? "part" : "")}{(options.LaunchPlan ? "plan" : "")}" +
    //$"{(string.IsNullOrEmpty(key) ? "" : "_" + key)}" +
    $".csv";
        }

        /// <summary>
        /// Запустить сессию.
        /// </summary>
        /// <param name="data">По желанию можно дать сессии собственные настройки (по умолчанию Родана грузит их из файла). Обычно в этом нет нужды.</param>
        public void Start(SettingsClass? data = null)
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            StaticStart();

            SettingsController.Load(data);

            // Проверяем все файлы
            CheckFiles();

            // Если с ними всё в порядке, то можно и начинать
            if (SettingsController.CurrentSettings.CreateKillMe) KillMe = new Stopper("killme", 100, x => EndSession(x));
            if (SettingsController.CurrentSettings.CreateStopMe) StopMe = new Stopper("stopme", 0, x => EndSession(x));
        }

        async public Task LoadDataAsync()
        {
            await Task.Run(() => HoppersHandler.LoadAllHoppersAsync(SettingsController.CurrentSettings.HoppersPaths));
            OnSessionDataLoaded?.Invoke();
        }

        void CheckFiles()
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
                            EndSession(code);
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
            CheckFile(DataErrors.DatasErrorsFile, 507, true);
            //CheckFile("dataErrorsBig.csv", 508);
            CheckFile("rechanges.csv", 509);
            CheckFile(SessionOptions.OutputFile, 510);
            CheckFile(SessionOptions.OutputFile.Substring(0, SessionOptions.OutputFile.Length - 4) + "_old.csv", 511);
            CheckFile("used_zadels.csv", 512);
            CheckFile("rechanges_old.csv", 513);
            CheckFile(DataErrors.PartiasErrorsFile, 514, true);
            CheckFile(DataErrors.PartiasErrorsBigFile, 515, true);

            if (list.Count > 0) DataErrors.AddNewPartiaError(-1, NewDataErrorType.SomeFileIsOpened, new DataErrorStruct<InputDataType>("", string.Join(", ", list)));
            if (DataErrors.TryOutputDataErrors())
            {
                EndSession(601);
            }
        }

        /// <summary>
        /// Закрыть сессию. В эти моменты обычно можно выполнить что-нибудь - сохранить настройки, или вовсе попросить выйти с определенным кодом.
        /// </summary>
        /// <param name="ExitCode">Есть возможность выйти после закрытия сессии. В таком случае надо указать код выхода.</param>
        public void EndSession(int? exitCode = null)
        {
            AppDomain.CurrentDomain.FirstChanceException -= CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;

            KillMe?.Stop();
            StopMe?.Stop();

            if (SettingsController.CurrentSettings.SaveOnSessionEnd) SettingsController.Save();
            //OnSessionEnded?.Invoke();
            if (exitCode.HasValue)
            {
                AssignedLogger.OnApplicationExit(exitCode.Value);
                Environment.Exit(exitCode.Value);
            }
        }



        internal static void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            var ex = (Exception)e.Exception;
            var t = new System.Diagnostics.StackTrace(true);
            var mes = $"=== First Chance EXCEPTION ({ex.HResult}) ===" +
                $"\r\nsender: {sender}" +
                $"\r\nMessage: {ex.Message}" +
                $"\r\nSource: {ex.Source}" +
                $"\r\nStack trace: {ex.StackTrace}" +
                $"\r\nStack trace 2: {t}";
            Console.WriteLine(mes);
            try
            {

                // ConcurrentLogger.AddToLogQueue.Enqueue(mes + "\r\n");
            }
            catch
            {

            }
        }
        internal static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            var mes = $"=== Unhandled EXCEPTION ({ex.HResult}) ===" +
                $"\r\nsender: {sender}" +
                $"\r\nMessage: {ex.Message}" +
                $"\r\nSource: {ex.Source}" +
                $"\r\nStack trace: {ex.StackTrace}";
            Console.WriteLine(mes);
            try
            {

                //ConcurrentLogger.AddToLogQueue.Enqueue(mes + "\r\n");
            }
            catch
            {

            }
        }
        internal static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            var ex = (Exception)e.Exception;
            var mes = $"=== Thread EXCEPTION ({ex.HResult}) ===" +
                $"\r\nsender: {sender}" +
                $"\r\nMessage: {ex.Message}" +
                $"\r\nSource: {ex.Source}" +
                $"\r\nStack trace: {ex.StackTrace}";
            try
            {

                //ConcurrentLogger.AddToLogQueue.Enqueue(mes + "\r\n");
            }
            catch
            {

            }
        }
    }
}

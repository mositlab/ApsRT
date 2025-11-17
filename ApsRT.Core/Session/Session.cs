using ApsRT.Core.DataErrors;
using ApsRT.Core.Hoppers;
using ApsRT.Core.Partias.PartiasAlgorithm;
using ApsRT.Core.Planning.PlanningAlgorithm;
using ApsRT.Core.Settings;
using Google.Protobuf.Compiler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

namespace ApsRT.Core.Session
{
    /// <summary>
    /// Класс сессии. Каждая сессия - это по сути и есть экземпляр АПС. 
    /// В ней создаются классы переналадок и прочего во время вызова конструктора.
    /// Сессия нуждается в загрузке данных через вызов метода Init в ней
    /// После того, как Родана загрузит настройки и с их помощью активирует Хопперы.
    /// Теперь, запуская алгоритмы планирования и партий, можно хавать данные из данных сессии.
    /// </summary>
    public class Session
    {
        public PartiasAlgorithm AssignedPartiasAlgorithm { get; private set; }
        public bool PartiasAlgorithmIsNotRunning => AssignedPartiasAlgorithm == null || AssignedPartiasAlgorithm.CurrentStage == PartiasAlgorithmStage.None
                || AssignedPartiasAlgorithm.CurrentStage == PartiasAlgorithmStage.Complete
                || AssignedPartiasAlgorithm.CurrentStage == PartiasAlgorithmStage.Failed;
        public bool TryCreateNewPartiasAlgorithmInstance(out PartiasAlgorithm algorithm)
        {
            if (PartiasAlgorithmIsNotRunning)
            {
                AssignedPartiasAlgorithm = new PartiasAlgorithm(this);
                algorithm = AssignedPartiasAlgorithm;
                return true;
            }
            algorithm = AssignedPartiasAlgorithm;
            return false;
        }

        public PlanningAlgorithm AssignedPlanningAlgorithm { get; private set; }
        public bool PlanningAlgorithmIsNotRunning => AssignedPlanningAlgorithm == null
                || AssignedPlanningAlgorithm.CurrentStage == PlanningAlgorithmStage.None
                || AssignedPlanningAlgorithm.CurrentStage == PlanningAlgorithmStage.Prepared
                || AssignedPlanningAlgorithm.CurrentStage == PlanningAlgorithmStage.Complete
                || AssignedPlanningAlgorithm.CurrentStage == PlanningAlgorithmStage.Failed;
        public bool TryCreateNewPlanningAlgorithmInstance(out PlanningAlgorithm algorithm, bool ForcedToRecreate = false)
        {
            if (!ForcedToRecreate && AssignedPlanningAlgorithm != null && AssignedPlanningAlgorithm.CurrentStage == PlanningAlgorithmStage.Prepared)
            {
                algorithm = AssignedPlanningAlgorithm;
                return true;
            }
            if (PlanningAlgorithmIsNotRunning)
            {
                AssignedPlanningAlgorithm = new PlanningAlgorithm(this);
                algorithm = AssignedPlanningAlgorithm;
                return true;
            }
            algorithm = AssignedPlanningAlgorithm;
            return false;
        }

        public SessionOptions SessionOptions { get; init; }
        public DataErrors.DataErrors DataErrors { get; init; }
        public SettingsController SettingsController { get; init; }
        public HoppersHandler HoppersHandler { get; init; }
        public MasterDataLinks MasterDataLinks { get; init; }
        public ConcurrentLogger AssignedLogger { get; init; }

        public SessionOutputEngine SessionOutputEngine { get; init; }

        public event Action<Session> OnSessionDataLoaded;

        Stopper KillMe;
        Stopper StopMe;

        public Session()
        {
            SettingsController = new SettingsController();
            MasterDataLinks = new(this);
            SessionOptions = new();
            DataErrors = new(this);
            HoppersHandler = new(this);
            AssignedLogger = new ConcurrentLogger(this);
            SessionOutputEngine = new SessionOutputEngine(this);
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
                if (arg.StartsWith("guid:"))
                {
                    opts.LaunchPath = arg.Substring(5);
                    continue;
                }
                if (arg.StartsWith("out:"))
                {
                    opts.OutputPath = arg.Substring(4);
                    continue;
                }
                if (arg.StartsWith("gamma:"))
                {
                    opts.DesiredGamma = double.Parse(arg.Substring(6).Replace('.', ','));
                    continue;
                }
                if (arg.StartsWith("count:"))
                {
                    opts.DesiredPercent = double.Parse(arg.Substring(6).Replace('.', ','));
                    continue;
                }
                if (arg.StartsWith("settings:"))
                {
                    opts.DesiredSettingsPath = arg.Substring(9);
                    continue;
                }
                if (arg.StartsWith("options:"))
                {
                    var DesiredOptionsPath = arg.Substring(8);
                    opts.SessionOptionsLoaded = JsonConvert.DeserializeObject<SessionOptionsLoaded>(File.ReadAllText(DesiredOptionsPath, Encoding.UTF8));
                    continue;
                }
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
        }

        /// <summary>
        /// Запустить сессию.
        /// </summary>
        /// <param name="data">По желанию можно дать сессии собственные настройки (по умолчанию Родана грузит их из файла). Обычно в этом нет нужды.</param>
        public void Start(SettingsClass? data = null)
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //AppDomain.CurrentDomain.ProcessExit += delegate { EndSession(0); };

            // Чекаю возможность создать папку

            if (!SessionOptions.GetPreparedLaunchPath(out var sessionPath))
            {
                if (SessionOptions.IsSilent)
                    EndSession(602);
            }

            if (!SessionOptions.GetPreparedOutputPath(out var outPath))
            {
                if (SessionOptions.IsSilent)
                    EndSession(603);
            }

            if (!Directory.Exists(SessionOptions.SessionsPath))
            {
                try
                {
                    Directory.CreateDirectory(SessionOptions.SessionsPath);
                }
                catch
                {
                    if (SessionOptions.IsSilent)
                        EndSession(602);
                }
            }

            // Запускаем логгер
            Task.Run(AssignedLogger.LogThread);
            var process = Process.GetCurrentProcess();
            AssignedLogger.LogQueue.Enqueue($"process ID: {process.Id} [session ID {process.SessionId}]\r\n");
            AssignedLogger.LogQueue.Enqueue($"flags: silent {SessionOptions.IsSilent}, part {SessionOptions.LaunchPartia}, plan {SessionOptions.LaunchPlan}, zadel {SessionOptions.LaunchZadel}\r\n");

            // Загружаем настройки
            if (!string.IsNullOrEmpty(SessionOptions.DesiredSettingsPath)) SettingsController.SavePath = SessionOptions.DesiredSettingsPath;
            SettingsController.Load(data);
            if (SessionOptions.DesiredGamma.HasValue) SettingsController.CurrentSettings.TGamma = SessionOptions.DesiredGamma.Value;
            if (SessionOptions.DesiredPercent.HasValue) SettingsController.CurrentSettings.Test_PercentOfStages = SessionOptions.DesiredPercent.Value;

            // Убираем старые сессии чтоб не засирать папку sessions
            SessionOptions.ClearSessionOldPaths(SettingsController.CurrentSettings.MaxOldSessionPathsDays);

            // Проверяем все файлы
            SessionOutputEngine.CheckFiles();

            // Если с ними всё в порядке, то можно и начинать
            if (SettingsController.CurrentSettings.CreateKillMe) KillMe = new Stopper("killme", 100, x => EndSession(x));
            if (SettingsController.CurrentSettings.CreateStopMe) StopMe = new Stopper("stopme", 0, x => EndSession(x));
        }

        public void PostDataProcess()
        {

            MasterDataLinks.StagesMasterData.FixVPZDependencies();
            MasterDataLinks.StagesMasterData.SetLivenCodes(SettingsController.SpecialSettings.LivensteinCodesCriteriaName);
            MasterDataLinks.NaladLengthsMasterData.SetNeededParamName(SettingsController.SpecialSettings.NaladLengthsCriteriaName);

            if (DataErrors.TryOutputDataErrors())
            {
                if (SessionOptions.IsSilent)
                    EndSession(601);
            }

            // После загрузки ВСЕХ данных - Можно со спокойной душой все диапазоны настраивать
            foreach (var op in MasterDataLinks.StagesMasterData.AllStages)
            {
                op.Value.CacheNaladLengths(); // Наладочные длины
                op.Value.PrepareDurations(); // Длительности
                op.Value.PrepareRottingTime(); // Гниение
                op.Value.BakeAllIndexDiapazons(null); // Диапазоны для переналадок
            }
        }

        async public Task LoadDataAndProcessAsync()
        {
            // Сначала пройдёмся по путям для хопперов
            Dictionary<string, string[]> finalPaths;
            if (SessionOptions.SessionOptionsLoaded == null
                ||
                SessionOptions.SessionOptionsLoaded.CustomHoppersPaths == null
                ||
                SessionOptions.SessionOptionsLoaded.CustomHoppersPaths.Count == 0
                )
            {
                // Просто возьмем то, что у нас есть
                finalPaths = SettingsController.CurrentSettings.HoppersPaths;
            }
            else
            {
                finalPaths = new();
                foreach (var el in SessionOptions.SessionOptionsLoaded.CustomHoppersPaths)
                {
                    finalPaths.Add(el.Key, el.Value);
                }

                // Если есть, что добавлять, то смотрим
                if (SettingsController.CurrentSettings.HoppersPaths != null && SettingsController.CurrentSettings.HoppersPaths.Count > 0)
                {
                    foreach (var el in SettingsController.CurrentSettings.HoppersPaths)
                    {
                        if (finalPaths.TryGetValue(el.Key, out var pathsHere))
                        {
                            // Здесь в теории можно сравнить и перезаписать пустые, но делать мы этого на данный момент не будем.
                        }
                        else
                        {
                            // Просто добавляем, если нету
                            finalPaths.Add(el.Key, el.Value);
                        }
                    }
                }
            }

            await Task.Run(() => HoppersHandler.LoadAllHoppersAsync(finalPaths));

            PostDataProcess();

            OnSessionDataLoaded?.Invoke(this);

            var allStages = MasterDataLinks.StagesMasterData.AllStages;
            if (allStages.Count == 0)
            {
                if (SessionOptions.IsSilent) EndSession(600); // Пустая табличка операций!
            }

            if (SessionOptions.IsSilent)
            {
                // Если она тихая. то ПРИСТУПАЕМ - ВСЕГДА начиная с партий, какие бы настройки там ни были.
                if (TryCreateNewPartiasAlgorithmInstance(out var newPartiasAlgorithmInstance)
                    && newPartiasAlgorithmInstance.Start(false))
                {
                    bool result = await newPartiasAlgorithmInstance.Process;

                    if (result)
                    {
                        if (SessionOptions.LaunchPlan && newPartiasAlgorithmInstance.CurrentStage == PartiasAlgorithmStage.Complete)
                        {
                            if (TryCreateNewPlanningAlgorithmInstance(out var newPlanningAlgorithmInstance)
                                && newPlanningAlgorithmInstance.Start())
                            {
                                bool result2 = await newPlanningAlgorithmInstance.Process;

                                if (result2)
                                {
                                    SessionOutputEngine.Output(ForcedToRecreate: false); // Выводим вообще всё.
                                    EndSession(0);
                                }
                                else
                                {
                                    EndSession(13);
                                }
                            }
                            else
                            {
                                EndSession(13);
                            }
                        }
                        else
                        {
                            EndSession(0); // В таком случае можно всё закрывать
                        }
                    }
                    else
                    {
                        EndSession(14);
                    }
                }
                else
                {
                    EndSession(14);
                }
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



        internal void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
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
                AssignedLogger.LogQueue.Enqueue(mes + "\r\n");
            }
            catch
            {

            }
        }
        internal void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
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
                AssignedLogger.LogQueue.Enqueue(mes + "\r\n");
            }
            catch
            {

            }
        }
        internal void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            var ex = (Exception)e.Exception;
            var mes = $"=== Thread EXCEPTION ({ex.HResult}) ===" +
                $"\r\nsender: {sender}" +
                $"\r\nMessage: {ex.Message}" +
                $"\r\nSource: {ex.Source}" +
                $"\r\nStack trace: {ex.StackTrace}";
            try
            {
                AssignedLogger.LogQueue.Enqueue(mes + "\r\n");
            }
            catch
            {

            }
        }
    }
}

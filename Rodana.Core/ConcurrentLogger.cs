using System.Collections.Concurrent;
using System.Text;
using Rodana.Core.Session;

namespace Rodana.Core
{
    //[Obsolete("стоит рассмотреть отказ от такого логгирования и перейти на асинхронное логгирование в sqlite")]
    public class ConcurrentLogger<InputDataType, OrderSubType>
    {

        Session<InputDataType, OrderSubType> session { get; }

        public ConcurrentLogger(Session<InputDataType, OrderSubType> session)
        {
            this.session = session;
        }

        internal ConcurrentQueue<string> LogQueue { get; } = new();

        internal ConcurrentQueue<string> PartiasLogQueue { get; } = new(); // НА ВСЯКИЙ СЛУЧАЙ

        internal void OnApplicationExit(int exitCode)
        {
            StreamWriter exitLogSW = new StreamWriter(session.SessionOptions.ExitLogFile, false, Encoding.UTF8);
            exitLogSW.WriteLine($"exit code: {exitCode}");

            try
            {
                if (!LogQueue.IsEmpty)
                {
                    exitLogSW.WriteLine($"log remains:");
                    while (LogQueue.Count > 0)
                    {
                        if (LogQueue.TryDequeue(out var res))
                        {
                            exitLogSW.Write(res);//fs.WriteLine(str);
                        }
                    }
                }

                if (!PartiasLogQueue.IsEmpty)
                {
                    exitLogSW.WriteLine($"partii log remains:");
                    while (PartiasLogQueue.Count > 0)
                    {
                        if (PartiasLogQueue.TryDequeue(out var res))
                        {
                            exitLogSW.Write(res);//fs.WriteLine(str);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var t = new System.Diagnostics.StackTrace(true);
                var mes = $"=== First Chance EXCEPTION ({ex.HResult}) ===" +
                    $"\r\nMessage: {ex.Message}" +
                    $"\r\nSource: {ex.Source}" +
                    $"\r\nStack trace: {ex.StackTrace}" +
                    $"\r\nStack trace 2: {t}";
                exitLogSW.WriteLine(mes);
            }

            exitLogSW.Close();
        }

        internal void LogThread()
        {
            //StreamWriter fs = new StreamWriter(Loader.LogFile, true);
            while (true)
            {
                Thread.Sleep(100);

                if (Environment.HasShutdownStarted)
                {
                    break;
                }

                if (!LogQueue.IsEmpty)
                {
                    if (LogQueue.TryDequeue(out var str))
                    {
                        File.AppendAllText(session.SessionOptions.LogFile, str);//fs.WriteLine(str);
                    }
                }

                if (!PartiasLogQueue.IsEmpty)
                {
                    if (PartiasLogQueue.TryDequeue(out var str))
                    {
                        File.AppendAllText(session.SessionOptions.PartiiLogFile, str);//fs.WriteLine(str);
                    }
                }
            }
            //fs.Close();
        }

    }
}

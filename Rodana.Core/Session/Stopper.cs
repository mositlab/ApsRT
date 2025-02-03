using Rodana.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Rodana.Core.Session
{
    internal class Stopper<InputDataType, OrderSubType, SettingsClassType> where SettingsClassType : SettingsClass, new()
    {
        public string CheckingFileName { get; init; }
        public int ExitCode { get; init; }
        Session<InputDataType, OrderSubType, SettingsClassType> session;

        FileSystemWatcher watcher;

        public Stopper(string checkingFileName, int exitCode, Session<InputDataType, OrderSubType, SettingsClassType> sessionLink)
        {
            CheckingFileName = checkingFileName;
            if (!File.Exists(checkingFileName)) File.WriteAllLines(checkingFileName, Array.Empty<string>());
            ExitCode = exitCode;
            watcher = new FileSystemWatcher(Directory.GetCurrentDirectory());
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                         | NotifyFilters.FileName;// | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            watcher.Deleted += Watcher_Deleted;
            watcher.EnableRaisingEvents = true;
            session = sessionLink;
        }

        public void Stop() => watcher.Dispose();

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (CheckingFileName == e.Name) session.EndSession(ExitCode);
        }
    }
}

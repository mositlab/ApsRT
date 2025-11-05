using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Session
{
    /// <summary>
    /// Класс опций запуска. Если класс настроек отвечает на многие вопросы "КАК выполнять?", то этот небольшой класс содержит только небольшое количество свойств, отвечающих на вопрос "ЧТО выполнять?" - партии, планирование, или заделы. Класс содержит данные, которые имеют отношение только к этой конкретной сессии.
    /// </summary>
    public class SessionOptions
    {

        public bool IsSilent = false,
            LaunchPartia = false,
            LaunchPlan = false,
            LaunchZadel = false,
            Jsort = false;

        public string LaunchPath { get; set; }
        public string OutputPath { get; set; }

        public const string OutsPath = "outs";
        public const string SessionsPath = "sessions";

        public double? DesiredGamma = null;
        public double? DesiredPercent = null;
        public string DesiredSettingsPath = null;

        public SessionOptionsLoaded SessionOptionsLoaded { get; set; }

        /// <summary>
        /// Дополнительный метод, который позволяет удалить старые каталоги
        /// </summary>
        public void ClearSessionOldPaths(double maxDaysDelta)
        {
            if (maxDaysDelta == 0) return; // Если 0, то пофигу, выходим
            //if (string.IsNullOrEmpty(LaunchPath)) return; // Тоже смысла особого не имеет
            var dirs = Directory.GetDirectories(SessionsPath);
            var now = DateTime.Now;
            foreach (var item in dirs)
            {
                var date = Directory.GetCreationTime(item);
                var days = (now - date).TotalDays;
                if (days > maxDaysDelta)
                {
                    Console.WriteLine(item + " ---> created: " + date + " ---> days: " + days);
                    Directory.Delete(item, true);
                }
            }
        }

        public bool GetPreparedOutputPath(out string path)
        {
            if (string.IsNullOrEmpty(OutputPath))
            {
                return GetPreparedLaunchPath(out path);
            }
            else
            {
                try
                {
                    if (!Directory.Exists(OutsPath)) Directory.CreateDirectory(OutsPath);
                    var mergedPath = $"{OutsPath}/{OutputPath}/";
                    if (!Directory.Exists(mergedPath)) Directory.CreateDirectory(mergedPath);
                    path = mergedPath;
                    return true;
                }
                catch
                {
                    path = "";
                    return false;
                }
            }
        }

        public bool GetPreparedLaunchPath(out string path)
        {
            if (string.IsNullOrEmpty(LaunchPath))
            {
                path = "";
                return true;
            }
            else
            {
                try
                {
                    if (!Directory.Exists(SessionsPath)) Directory.CreateDirectory(SessionsPath);
                    var mergedPath = $"{SessionsPath}/{LaunchPath}/";
                    if (!Directory.Exists(mergedPath)) Directory.CreateDirectory(mergedPath);
                    path = mergedPath;
                    return true;
                }
                catch
                {
                    path = "";
                    return false;
                }
            }
        }

        public bool OnlyPart => (LaunchPartia && !LaunchPlan) /*|| This.PARTCheckBox.Checked*/;

        //internal static bool IsZadel => LaunchZadel/* || This.ZadelCheckBox.Checked*/;
        public bool PartOrZadel => OnlyPart || LaunchZadel;

    }
}

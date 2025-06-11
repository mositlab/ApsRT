using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Session
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

        const string SessionsPath = "sessions";

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

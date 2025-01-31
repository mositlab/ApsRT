using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Session
{
    /// <summary>
    /// Класс опций запуска. Если класс настроек отвечает на многие вопросы "КАК выполнять?", то этот небольшой класс содержит только небольшое количество свойств, отвечающих на вопрос "ЧТО выполнять?" - партии, планирование, или заделы. Класс содержит имена файлов и данные, которые имеют отношение только к этой конкретной сессии (например имена файлов, в которых есть даты).
    /// </summary>
    public class SessionOptions
    {
        public string LogFile = "logs/log.txt";
        public string PartiiLogFile = "logs/partii_log.txt";
        public string ExitLogFile = "logs/exit_log.txt";
        public string OutputFile = "Result.csv";

        public bool IsSilent = false,
            LaunchPartia = false,
            LaunchPlan = false,
            LaunchZadel = false,
            Jsort = false;

        public bool OnlyPart => (LaunchPartia && !LaunchPlan) /*|| This.PARTCheckBox.Checked*/;

        //internal static bool IsZadel => LaunchZadel/* || This.ZadelCheckBox.Checked*/;
        public bool PartOrZadel => OnlyPart || LaunchZadel;

    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Settings
{

    /// <summary>
    /// Класс настроек. В настройках содержится масса информации, как АПС должна будет работать.
    /// </summary>
    public class SettingsClass
    {

        public static SettingsClass OriginalSettings = new SettingsClass()
        {
            OriginalDurationTime = 1,
            SaveOnSessionEnd = true,
            CacheStageDurations = true,
        };

        [JsonProperty]
        public Dictionary<string, string[]> HoppersPaths { get; set; }

        [JsonProperty]
        public bool SaveOnSessionEnd { get; set; }

        [JsonProperty]
        public int OriginalDurationTime { get; set; }

        [JsonProperty]
        public bool CacheStageDurations { get; set; }

        [JsonProperty]
        public bool CreateKillMe { get; set; }

        [JsonProperty]
        public bool CreateStopMe { get; set; }

        [JsonProperty]
        public bool HideConsole { get; set; }

        [JsonProperty]
        public bool AutoDataLoad { get; set; }

        [JsonProperty]
        public bool UsePseudoTara { get; set; }

        [JsonProperty]
        public bool LogErrors { get; set; }

        [JsonProperty]
        public double MaxDaysBetweenPartias { get; set; }

        [JsonProperty]
        public FormTheme CurrentTheme { get; set; }
    }
}

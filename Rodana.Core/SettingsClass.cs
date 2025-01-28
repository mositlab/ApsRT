using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core
{
    public class SettingsClass
    {
        public string SettingsPath = "settings.txt";

        public void Load()
        {

        }

        public void Save()
        {

        }

        [JsonProperty]
        public int OriginalDurationTime = 1;

        [JsonProperty]
        public bool CasheStageDurations = true;
    }
}

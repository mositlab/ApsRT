using Newtonsoft.Json;
using Rodana.Core.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.ZMK
{
    public partial class SettingsZMKAddon: SettingsClass
    {
        [JsonProperty]
        public Theme CurrentTheme { get; set; }

        public enum Theme
        {
            LightBlue,
            DarkBlue,
            LightRed,
            DarkRed
        }
    }
}

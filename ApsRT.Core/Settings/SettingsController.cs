using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Settings
{
    public class SettingsController
    {
        public string SavePath { get; set; } = "settings.txt";
        public SettingsClass CurrentSettings { get; set; } = new();
        public SpecialSettings SpecialSettings { get; set; } = new();
        public event Action OnSettingsLoaded;

        /// <summary>
        /// Загружает настройки из файла, указанного в пути <cref >SavePath</cref>
        /// </summary>
        /// <param name="settings">Класс уже сформированных настроек, которые можно передать сюда.</param>
        public void Load(SettingsClass? settings = null)
        {
            if (!File.Exists(SavePath))
            {
                privateSave(SettingsClass.OriginalSettings);
            }

            CurrentSettings = settings ?? JsonConvert.DeserializeObject<SettingsClass>(File.ReadAllText(SavePath, Encoding.UTF8));
            ToggleConsole(CurrentSettings.HideConsole);
            OnSettingsLoaded?.Invoke();
        }

        public void ToggleConsole(bool hideConsole)
        {
            ConsoleParams.ShowWindow(ConsoleParams.ConsoleHandle, hideConsole ? ConsoleParams.SW_HIDE : ConsoleParams.SW_SHOW);
        }

        void privateSave(SettingsClass settings)
        {
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public void Save() => privateSave(CurrentSettings);
    }
}

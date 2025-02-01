using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Settings
{
    public class SettingsController<SettingsClassType> where SettingsClassType : SettingsClass, new()
    {
        public string SavePath = "settings.txt";
        public SettingsClassType CurrentSettings;
        public event Action OnSettingsLoaded;

        /// <summary>
        /// Загружает настройки из файла, указанного в пути <cref >SavePath</cref>
        /// </summary>
        /// <param name="settings">Класс уже сформированных настроек, которые можно передать сюда.</param>
        public void Load(SettingsClassType? settings = null)
        {
            if (!File.Exists(SavePath))
            {
                privateSave(new SettingsClassType());
            }

            CurrentSettings = settings ?? JsonConvert.DeserializeObject<SettingsClassType>(File.ReadAllText(SavePath, Encoding.UTF8));
            ToggleConsole(CurrentSettings.HideConsole);
            OnSettingsLoaded?.Invoke();
        }

        public void ToggleConsole(bool hideConsole)
        {
            ConsoleParams.ShowWindow(ConsoleParams.ConsoleHandle, hideConsole ? ConsoleParams.SW_HIDE : ConsoleParams.SW_SHOW);
        }

        void privateSave(SettingsClassType settings)
        {
            File.WriteAllText(SavePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public void Save() => privateSave(CurrentSettings);
    }
}

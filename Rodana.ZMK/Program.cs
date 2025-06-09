using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rodana.Core;
using System.Configuration;
using System.Data;
using System.Windows;
using Rodana.Core.Session;

namespace Rodana.ZMK
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var sessionLink = new Rodana.Core.Session.Session(Environment.GetCommandLineArgs());
            sessionLink.SessionOutputEngine.ClassicSettingsByOptions(sessionLink.SessionOptions);
            sessionLink.Start();
            sessionLink.SettingsController.SpecialSettings.CalculateBestSubdivisionInPartias = false;
            sessionLink.SettingsController.SpecialSettings.OptimizeSubdivisionsInPartias = false;
            sessionLink.SettingsController.SpecialSettings.NaladLengthsCriteriaName = "Сечение";
            sessionLink.SettingsController.SpecialSettings.LivensteinCodesCriteriaName = "Напряжение";


            MainForm form = null;
            if (!sessionLink.SessionOptions.IsSilent)
            {
                ApplicationConfiguration.Initialize();
                form = new MainForm(sessionLink); // Подписываем тут все события
            }

            if (sessionLink.SettingsController.CurrentSettings.AutoDataLoad) sessionLink.LoadDataAndProcessAsync();

            if (sessionLink.SessionOptions.IsSilent)
            {
                //Application.Run();
            }
            else
            {
                
                Application.Run(form);
            }

        }

    }
}
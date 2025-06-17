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

namespace Rodana.SKO
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
            sessionLink.SettingsController.SpecialSettings.CalculateBestSubdivisionInPartias = true;
            sessionLink.SettingsController.SpecialSettings.OptimizeSubdivisionsInPartias = true;
            sessionLink.SettingsController.SpecialSettings.NaladLengthsCriteriaName = "";
            sessionLink.SettingsController.SpecialSettings.LivensteinCodesCriteriaName = "";


            RodanaCustomComponents.RodanaMainForm1 form = null;
            if (!sessionLink.SessionOptions.IsSilent)
            {
                var color1 = Color.Blue;
                var color2 = Color.Orange;
                ApplicationConfiguration.Initialize();
                form = new RodanaCustomComponents.RodanaMainForm1(sessionLink, new Color[]
                {
                    color1,
                    color2,
                    Color.FromArgb((int)(color1.R * 0.7), (int)(color1.G * 0.7), (int)(color1.B * 0.7)),
                    Color.FromArgb((int)(color2.R * 0.7), (int)(color2.G * 0.7), (int)(color2.B * 0.7))
                }); // Подписываем тут все события
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
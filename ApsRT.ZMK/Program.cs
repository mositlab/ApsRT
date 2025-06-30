using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApsRT.Core;
using System.Configuration;
using System.Data;
using System.Windows;
using ApsRT.Core.Session;
using UniversalPack.Common.Drawing;
using System.Reflection;

namespace ApsRT.ZMK
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var sessionLink = new ApsRT.Core.Session.Session(Environment.GetCommandLineArgs());
            sessionLink.SessionOutputEngine.ClassicSettingsByOptions(sessionLink.SessionOptions);
            sessionLink.Start();
            sessionLink.SettingsController.SpecialSettings.CalculateBestSubdivisionInPartias = false;
            sessionLink.SettingsController.SpecialSettings.OptimizeSubdivisionsInPartias = false;
            sessionLink.SettingsController.SpecialSettings.CalculateSubdivisionsTransfers = false;
            sessionLink.SettingsController.SpecialSettings.NaladLengthsCriteriaName = "Сечение";
            sessionLink.SettingsController.SpecialSettings.LivensteinCodesCriteriaName = "Напряжение";


            ApsRTCustomComponents.ApsRTMainForm1 form = null;
            if (!sessionLink.SessionOptions.IsSilent)
            {
                ApplicationConfiguration.Initialize();
                form = new ApsRTCustomComponents.ApsRTMainForm1(sessionLink, "Производство: Завод МосКабель", new Color[]
                {
                    Colors.MOSITLAB_BLUE,
                    Colors.MOSITLAB_RED,
                    Colors.MOSITLAB_BLUE_DARK,
                    Colors.MOSITLAB_RED_DARK
                }); // Подписываем тут все события
                try
                {
                    Icon appIcon = Icon.ExtractAssociatedIcon(Environment.GetCommandLineArgs()[0]);
                    form.Icon = appIcon;
                }
                catch
                {

                }
            }

            if (sessionLink.SettingsController.CurrentSettings.AutoDataLoad) sessionLink.LoadDataAndProcessAsync();

            if (sessionLink.SessionOptions.IsSilent)
            {
                Application.Run();
            }
            else
            {
                
                Application.Run(form);
            }

            return Environment.ExitCode;

        }

    }
}
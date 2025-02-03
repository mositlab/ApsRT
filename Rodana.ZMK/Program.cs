using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rodana.Core;
using System.Configuration;
using System.Data;
using System.Windows;

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
            var sessionLink = new Rodana.Core.Session.Session<Guid, int, SettingsZMKAddon>(Environment.GetCommandLineArgs());

            sessionLink.Start();
            sessionLink.OnSessionDataLoaded += On_ZMK_DataLoaded;

            MainForm form = null;
            if (!sessionLink.SessionOptions.IsSilent)
            {
                form = new MainForm(sessionLink);
                // Подписываем тут все события
            }

            if (sessionLink.SettingsController.CurrentSettings.AutoDataLoad) sessionLink.LoadDataAsync();

            if (sessionLink.SessionOptions.IsSilent)
            {
                //Application.Run();
            }
            else
            {
                ApplicationConfiguration.Initialize();
                Application.Run(form);
            }

        }


        private static void On_ZMK_DataLoaded()
        {

        }
    }
}
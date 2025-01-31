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
            var sessionLink = new Rodana.Core.Session.Session<Guid, string, Guid, string, int, Guid, string>(Environment.GetCommandLineArgs());

            sessionLink.Start();
            sessionLink.OnSessionDataLoaded += On_ZMK_DataLoaded;

            sessionLink.LoadDataAsync();

            //App app = new App();

            if (sessionLink.SessionOptions.IsSilent)
            {
                //app.Run();
                //Application.Run();
            }
            else
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm(sessionLink));
                //MainWindow window = new MainWindow(sessionLink);
                //window.Activate();
                //app.Run(window);
            }


        }


        private static void On_ZMK_DataLoaded()
        {

        }
    }
}
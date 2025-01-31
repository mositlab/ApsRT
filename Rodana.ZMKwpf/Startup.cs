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
    internal class Startup : Application
    {
        [STAThread]
        static void Main()
        {
            var sessionLink = new Rodana.Core.Session.Session<Guid, string, Guid, string, int, Guid, string>(Environment.GetCommandLineArgs());

            sessionLink.Start();
            sessionLink.OnSessionDataLoaded += On_ZMK_DataLoaded;

            sessionLink.LoadDataAsync();

            App app = new App();

            if (sessionLink.SessionOptions.IsSilent)
            {
                //app.Run();
            }
            else
            {
                MainWindow window = new MainWindow(sessionLink);
                //window.Activate();
                app.Run(window);
            }


        }

        private static void On_ZMK_DataLoaded()
        {

        }
    }
}

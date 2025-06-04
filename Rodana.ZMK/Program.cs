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

            sessionLink.Start();
            sessionLink.OnSessionDataLoaded += On_ZMK_DataLoaded;


            MainForm form = null;
            if (!sessionLink.SessionOptions.IsSilent)
            {
                ApplicationConfiguration.Initialize();
                form = new MainForm(sessionLink); // Подписываем тут все события
            }

            if (sessionLink.SettingsController.CurrentSettings.AutoDataLoad) sessionLink.LoadDataAsync();

            if (sessionLink.SessionOptions.IsSilent)
            {
                //Application.Run();
            }
            else
            {
                
                Application.Run(form);
            }

        }


        private static void On_ZMK_DataLoaded(Session session)
        {
            var options = session.SessionOptions;
            var allStages = session.MasterDataLinks.StagesMasterData.AllStages;
            if (allStages.Count == 0)
            {
                if (!options.IsSilent)
                {
                    MessageBox.Show("АПС не может продолжать работу - таблица операций пуста.", "Пустая таблица операций", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (options.IsSilent) session.EndSession(600); // Пустая табличка операций!
            }
        }
    }
}
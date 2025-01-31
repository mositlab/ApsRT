using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rodana.Core;

namespace Rodana.ZMK
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Session<Guid, string, Guid, string, int, Guid, string> sessionLink;

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            sessionLink = new Session<Guid, string, Guid, string, int, Guid, string>(Environment.GetCommandLineArgs());

            // Подписать все события сюда
            //Application.ThreadException += Application_ThreadException;

            sessionLink.Start();
        }
        private void MainWindow_Closed(object? sender, EventArgs e) => sessionLink.EndSession(0);

    }
}
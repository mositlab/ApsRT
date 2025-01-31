using System;
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
using Rodana.Core.Session;

namespace Rodana.ZMK
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(Session<Guid, string, Guid, string, int, Guid, string> sessionLink)
        {
            InitializeComponent();
            this.sessionLink = sessionLink;
        }

        Session<Guid, string, Guid, string, int, Guid, string> sessionLink;

        ResourceDictionary lightTheme, darkTheme;
        bool isDark = false;
        void ToggleTheme(bool dark)
        {
            isDark = dark;
            Application.Current.Resources.MergedDictionaries.Remove(isDark ? lightTheme : darkTheme);
            Application.Current.Resources.MergedDictionaries.Add(isDark ? darkTheme : lightTheme);
        }
        private void ThemeButton_Click(object sender, RoutedEventArgs e) => ToggleTheme(!isDark);

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            lightTheme = Application.LoadComponent(new Uri("Themes\\lightTheme.xaml", UriKind.Relative)) as ResourceDictionary;
            darkTheme = Application.LoadComponent(new Uri("Themes\\darkTheme.xaml", UriKind.Relative)) as ResourceDictionary;
            ToggleTheme(false);
        }

        private void MainWindow_Closed(object? sender, EventArgs e) => sessionLink.EndSession(0);

    }
}
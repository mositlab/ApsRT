using Rodana.Core.Session;

namespace Rodana.ZMK
{
    public partial class MainForm : Form
    {
        public MainForm(Session<Guid, string, Guid, string, int, Guid, string, SettingsAddon> sessionLink)
        {
            InitializeComponent();
            this.sessionLink = sessionLink;
            this.FormClosed += MainForm_FormClosed;
            this.ToggleThemeButton.Click += ToggleThemeButton_Click;
            SetTheme(sessionLink.SettingsController.CurrentSettings.DarkTheme);
        }

        private void ToggleThemeButton_Click(object? sender, EventArgs e)
        {
            sessionLink.SettingsController.CurrentSettings.DarkTheme = !sessionLink.SettingsController.CurrentSettings.DarkTheme;
            SetTheme(sessionLink.SettingsController.CurrentSettings.DarkTheme);
        }

        IEnumerable<T> Descendant<T>(Control control) where T : class
        {
            foreach (Control child in control.Controls)
            {
                if (child is T thisControl)
                {
                    yield return (T)thisControl;
                }

                if (child.HasChildren)
                {
                    foreach (T descendant in Descendant<T>(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }

        public static Color MOSITLAB_BLUE { get; } = Color.FromArgb(47, 184, 196);
        public static Color MOSITLAB_BLUE_DARK { get; } = Color.FromArgb(5, 95, 103);
        public static Color MOSITLAB_RED { get; } = Color.FromArgb(234, 84, 84);
        public static Color MOSITLAB_RED_DARK { get; } = Color.FromArgb(180, 60, 61);

        void SetTheme(bool isDark)
        {
            foreach (var comp in Descendant<Button>(this))
            {
                comp.BackColor = isDark ? MOSITLAB_RED : MOSITLAB_BLUE;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<CustomComponents.CustomProgressBar>(this))
            {
                comp.ForeColor = isDark ? MOSITLAB_RED : MOSITLAB_BLUE;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<CustomComponents.MaterialCheckBox>(this))
            {
                comp.ForeColor = isDark ? MOSITLAB_RED : MOSITLAB_BLUE;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<CustomComponents.MaterialRadioButton>(this))
            {
                comp.ForeColor = isDark ? MOSITLAB_RED : MOSITLAB_BLUE;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<CustomComponents.MaterialTabSelector>(this))
            {
                comp.BackColor = isDark ? MOSITLAB_RED : MOSITLAB_BLUE;
                //button.ForeColor = Color.Black;
            }
        }

        private void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            sessionLink.EndSession(0);
        }

        Session<Guid, string, Guid, string, int, Guid, string, SettingsAddon> sessionLink;

    }
}

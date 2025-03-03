using Rodana.Core.Session;
using Rodana.Core.Settings;

namespace Rodana.ZMK
{
    public partial class MainForm : Form
    {
        public MainForm(Session<Guid, int> sessionLink)
        {
            InitializeComponent();
            this.sessionLink = sessionLink;
            this.FormClosed += MainForm_FormClosed;
            this.ToggleThemeButton.Click += ToggleThemeButton_Click;
            SetTheme(sessionLink.SettingsController.CurrentSettings.CurrentTheme);
            OpenSettingsButton.Click += OpenSettingsButton_Click;
            OpenPartiasButton.Click += OpenPartiasButton_Click;
            OpenHandPartiasButton.Click += OpenHandPartiasButton_Click;
            OpenPlanningButton.Click += OpenPlanningButton_Click;
            OpenHandPlanButton.Click += OpenHandPlanButton_Click;
            OpenGraphsButton.Click += OpenGraphsButton_Click;
        }

        private void OpenGraphsButton_Click(object? sender, EventArgs e)
        {
            MainTabControl.SelectTab(PlanningTabPage);
            PlanningTabControl.SelectTab(GraphsTabPage);
        }

        private void OpenHandPlanButton_Click(object? sender, EventArgs e)
        {
            MainTabControl.SelectTab(PlanningTabPage);
            PlanningTabControl.SelectTab(HandPlanningPage);
        }

        private void OpenPlanningButton_Click(object? sender, EventArgs e)
        {
            MainTabControl.SelectTab(PlanningTabPage);
            PlanningTabControl.SelectTab(TyrantTabPage);
        }

        private void OpenHandPartiasButton_Click(object? sender, EventArgs e)
        {
            MainTabControl.SelectTab(PartiasTabPage);
            PartiasTabControl.SelectTab(HandPartiasTabControl);
        }

        private void OpenPartiasButton_Click(object? sender, EventArgs e)
        {
            MainTabControl.SelectTab(PartiasTabPage);
            PartiasTabControl.SelectTab(AllPartiasTabControl);
        }

        private void OpenSettingsButton_Click(object? sender, EventArgs e)
        {
            MainTabControl.SelectTab(SettingsTabPage);
        }

        private void ToggleThemeButton_Click(object? sender, EventArgs e)
        {
            sessionLink.SettingsController.CurrentSettings.CurrentTheme = sessionLink.SettingsController.CurrentSettings.CurrentTheme switch
            {
                FormTheme.LightBlue => FormTheme.DarkBlue,
                FormTheme.DarkBlue => FormTheme.LightRed,
                FormTheme.LightRed => FormTheme.DarkRed,
                FormTheme.DarkRed => FormTheme.LightBlue,
                _ => FormTheme.DarkRed
            };
            SetTheme(sessionLink.SettingsController.CurrentSettings.CurrentTheme);
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

        public Color MOSITLAB_BLUE { get; } = Color.FromArgb(47, 184, 196);
        public Color MOSITLAB_BLUE_DARK { get; } = Color.FromArgb(5, 95, 103);
        public Color MOSITLAB_RED { get; } = Color.FromArgb(234, 84, 84);
        public Color MOSITLAB_RED_DARK { get; } = Color.FromArgb(180, 60, 61);

        void SetTheme(FormTheme theme)
        {
            Color color = theme == FormTheme.LightRed || theme == FormTheme.DarkRed ? MOSITLAB_RED : MOSITLAB_BLUE;
            Color textColor = theme == FormTheme.DarkBlue || theme == FormTheme.DarkRed ? Color.White : Color.Black;

            Color backColor = theme == FormTheme.DarkBlue || theme == FormTheme.DarkRed ? Color.FromArgb(64,64,64) : Color.White;

            PanelNearMainLabel.BackColor = textColor;
            //mainlabel.ForeColor = textColor;

            BackColor = backColor;

            foreach (var comp in Descendant<NumericUpDown>(this))
            {
                comp.BackColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<Label>(this))
            {
                comp.ForeColor = textColor;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<TabPage>(this))
            {
                comp.BackColor = backColor;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<SplitContainer>(this))
            {
                comp.BackColor = backColor;
                //button.ForeColor = Color.Black;
            }

            foreach (var comp in Descendant<Button>(this))
            {
                comp.BackColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<CustomComponents.CustomProgressBar>(this))
            {
                comp.ForeColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<CustomComponents.MaterialCheckBox>(this))
            {
                comp.BackColor = textColor;
                comp.ForeColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<CustomComponents.MaterialRadioButton>(this))
            {
                comp.BackColor = textColor;
                comp.ForeColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<CustomComponents.MaterialTabSelector>(this))
            {
                comp.BackColor = color;
                //button.ForeColor = Color.Black;
            }
        }

        private void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            sessionLink.EndSession(0);
        }

        Session<Guid, int> sessionLink;

    }
}

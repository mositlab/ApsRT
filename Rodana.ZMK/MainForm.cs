using Rodana.Core.Hoppers;
using Rodana.Core.Session;
using Rodana.Core.Settings;
using System.Xml.Linq;
using UniversalPack.Common.Drawing;
using UniversalPack;
using static UniversalPack.Common.Drawing.Colors;
using WinFormsCustomComponents;
using static UniversalPack.Extensions.EventExtensions;
using RodanaCustomComponents;

namespace Rodana.ZMK
{
    public partial class MainForm : Form
    {
        Session sessionLink;

        public MainForm(Session sessionLink)
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
            SaveSettingsButton.Click += SaveSettingsButton_Click;

            sessionLink.OnSessionDataLoaded += delegate
            {
                StartAlgorithmPButton.Enabled = true;
                PartiasProgressLabel.Text = "...";
            };

            sessionLink.HoppersHandler.OnStartLoadingEvent += HoppersStartLoad;
            sessionLink.HoppersHandler.OnLoadingEvent += HoppersOnLoad;
            sessionLink.HoppersHandler.OnEndLoadingEvent += HoppersEndLoad;

            CustomTreeViewOnPPanel.SessionLink = sessionLink;

            CreateHoppers();

            PrepareSettings();
        }

        private void SaveSettingsButton_Click(object? sender, EventArgs e)
        {
            sessionLink.SettingsController.CurrentSettings.HoppersPaths = HoppersGetPaths();
            sessionLink.SettingsController.Save();
        }

        void PrepareSettings()
        {
            var settings = sessionLink.SettingsController.CurrentSettings;
            LogErrorsCheckBox.Checked = settings.LogErrors;
            LogErrorsCheckBox.CheckedChanged += (sender, args) => { settings.LogErrors = ((CheckBox)sender).Checked; };

            UsePseudoTaraCheckBox.Checked = settings.UsePseudoTara;
            UsePseudoTaraCheckBox.CheckedChanged += (sender, args) => { settings.UsePseudoTara = ((CheckBox)sender).Checked; };

            AutoLoadCheckBox.Checked = settings.AutoDataLoad;
            AutoLoadCheckBox.CheckedChanged += (sender, args) => { settings.AutoDataLoad = ((CheckBox)sender).Checked; };

            SaveOnExitCheckBox.Checked = settings.SaveOnSessionEnd;
            SaveOnExitCheckBox.CheckedChanged += (sender, args) => { settings.SaveOnSessionEnd = ((CheckBox)sender).Checked; };

            CreateKillMeCheckBox.Checked = settings.CreateKillMe;
            CreateKillMeCheckBox.CheckedChanged += (sender, args) => { settings.CreateKillMe = ((CheckBox)sender).Checked; };

            CreateStopMe.Checked = settings.CreateStopMe;
            CreateStopMe.CheckedChanged += (sender, args) => { settings.CreateStopMe = ((CheckBox)sender).Checked; };

            CacheDurationsCheckBox.Checked = settings.CacheStageDurations;
            CacheDurationsCheckBox.CheckedChanged += (sender, args) => { settings.CacheStageDurations = ((CheckBox)sender).Checked; };

            PartiasMaxDaysNumericUpDown.Value = (decimal)settings.MaxDaysBetweenPartias;
            PartiasMaxDaysNumericUpDown.ValueChanged += (sender, args) => { settings.MaxDaysBetweenPartias = (double)((NumericUpDown)sender).Value; };

            BackupStagesDurationNumericUpDown.Value = (decimal)settings.OriginalDurationTime;
            BackupStagesDurationNumericUpDown.ValueChanged += (sender, args) => { settings.OriginalDurationTime = (int)((NumericUpDown)sender).Value; };

            HideConsoleCheckBox.Checked = settings.HideConsole;
            HideConsoleCheckBox.CheckedChanged += (sender, args) =>
            {
                settings.HideConsole = ((CheckBox)sender).Checked;
                sessionLink.SettingsController.ToggleConsole(settings.HideConsole);
            };

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

        void SetTheme(FormTheme theme)
        {
            Color color = theme == FormTheme.LightRed || theme == FormTheme.DarkRed ? MOSITLAB_RED : MOSITLAB_BLUE;
            Color colorDark = theme == FormTheme.LightRed || theme == FormTheme.DarkRed ? MOSITLAB_RED_DARK : MOSITLAB_BLUE_DARK;
            Color textColor = theme == FormTheme.DarkBlue || theme == FormTheme.DarkRed ? Color.White : Color.Black;

            Color backColor = theme == FormTheme.DarkBlue || theme == FormTheme.DarkRed ? Color.FromArgb(64,64,64) : Color.White;

            PanelNearMainLabel.BackColor = textColor;
            //mainlabel.ForeColor = textColor;

            BackColor = backColor;

            foreach (var comp in Descendant<CustomTreeView>(this))
            {
                comp.BackColor = Color.White;
                //button.ForeColor = Color.Black;
            }

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
                comp.FlatAppearance.MouseDownBackColor = colorDark;
                comp.FlatAppearance.MouseOverBackColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<WinFormsCustomComponents.CustomProgressBar>(this))
            {
                comp.ForeColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<WinFormsCustomComponents.MaterialCheckBox>(this))
            {
                comp.BackColor = textColor;
                comp.ForeColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<WinFormsCustomComponents.MaterialRadioButton>(this))
            {
                comp.BackColor = textColor;
                comp.ForeColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<WinFormsCustomComponents.MaterialTabSelector>(this))
            {
                comp.BackColor = color;
                //button.ForeColor = Color.Black;
            }
        }

        private void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (sessionLink.SettingsController.CurrentSettings.SaveOnSessionEnd)
            {
                sessionLink.SettingsController.CurrentSettings.HoppersPaths = HoppersGetPaths();
            }
            sessionLink.EndSession(0);
        }

        List<HopperUIClass> HoppersInInterface = new();

        Dictionary<string, string[]> HoppersGetPaths()
        {
            return HoppersInInterface
                .Select((hopp, index) => (Key: sessionLink.HoppersHandler.Hoppers[index].Name, Paths: hopp.PathTextBox.Lines.ToArray()))
                .ToDictionary(x => x.Key, x => x.Paths);
        }


        internal Font ButtonFont = new Font("Montserrat", 7f, FontStyle.Regular, GraphicsUnit.Point);

        private void CreateHoppers()
        {
            var hoppersList = sessionLink.HoppersHandler.Hoppers;

            int top = 6;

            for (int i = 0; i < hoppersList.Count; i++)
            {
                var item = hoppersList[i];
                // Кнопка
                var butt = new System.Windows.Forms.Button
                {
                    BackColor = Color.FromArgb(218, 218, 218),
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(12, top),// 6 + ModulesList.Count * 35);
                    Size = new Size(200, 31),
                    Text = item.Data.Label,
                    Font = ButtonFont,
                    UseVisualStyleBackColor = false,
                    UseCompatibleTextRendering = true
                };
                butt.Enabled = true;
                butt.ForeColor = Color.White;
                butt.FlatAppearance.BorderSize = 0;
                butt.FlatAppearance.MouseDownBackColor = Colors.MOSITLAB_RED_DARK;
                butt.FlatAppearance.MouseOverBackColor = Colors.MOSITLAB_RED;
                HoppersPanel.Controls.Add(butt);

                // Бар
                var bar = new CustomProgressBar
                {
                    Location = new Point(butt.Width + butt.Left + butt.Margin.Horizontal, top),//6 + ModulesList.Count * 35);
                    Size = new Size(205, 29),
                    ForeColor = Colors.MOSITLAB_BLUE,
                    BackColor = Color.FromArgb(218, 218, 218)
                };
                HoppersPanel.Controls.Add(bar);

                // Галочка
                var label = new Label
                {
                    AutoSize = true,
                    Location = new Point(bar.Width + bar.Left + bar.Margin.Horizontal, top),//6 + ModulesList.Count * 35);
                    Margin = new Padding(2, 0, 2, 0),
                    Size = new Size(19, 21),
                    TabIndex = 91,
                    Text = "..."
                };
                HoppersPanel.Controls.Add(label);

                // Текстбокс в настройках
                var textBox = new TextBox
                {
                    Location = new Point(label.Width + label.Left + 10, top),//ModulesList.Count == 0 ? 52 : (ModulesList[^1].PathTextBox.Top + ModulesList[^1].PathTextBox.Height + 7));
                    Margin = new Padding(2, 3, 2, 3),
                    Size = new Size(562, item.Data.IsMultiple ? 92 : 26),
                    Multiline = item.Data.IsMultiple
                };
                var paths = sessionLink.SettingsController.CurrentSettings.HoppersPaths;
                if (paths != null && paths.TryGetValue(sessionLink.HoppersHandler.Hoppers[i].Name, out var lines))
                {
                    textBox.Lines = lines;
                }
                HoppersPanel.Controls.Add(textBox);

                // Кнопка в настройках
                var butt2 = new System.Windows.Forms.Button
                {
                    BackColor = Color.FromArgb(218, 218, 218),
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(textBox.Width + textBox.Left + textBox.Margin.Horizontal, top + 1),
                    Margin = new Padding(2, 3, 2, 3),
                    Size = new Size(50, 25),
                    Text = "...",
                    UseVisualStyleBackColor = false
                };
                butt2.Enabled = true;
                butt2.ForeColor = Color.White;
                butt2.FlatAppearance.BorderSize = 0;
                butt2.FlatAppearance.MouseDownBackColor = Colors.MOSITLAB_RED_DARK;
                butt2.FlatAppearance.MouseOverBackColor = Colors.MOSITLAB_RED;

                var mult = item.Data.IsMultiple;
                if (item.Data.IsDirectory)
                {
                    butt2.Click += delegate
                    {
                        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                        {
                            textBox.Text = folderBrowserDialog1.SelectedPath;
                        }
                    };
                }
                else
                {
                    butt2.Click += delegate
                    {
                        openFileDialog1.Multiselect = mult;
                        if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            textBox.Lines = openFileDialog1.FileNames;
                        }
                    };
                }
                HoppersPanel.Controls.Add(butt2);

                // кнопка открытия диалога SQL
                var butt3 = new System.Windows.Forms.Button
                {
                    BackColor = Color.FromArgb(218, 218, 218),
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(butt2.Width + butt2.Left + butt2.Margin.Horizontal, butt2.Top),
                    Margin = new Padding(2, 3, 2, 3),
                    Size = new Size(60, 25),
                    Text = "SQL",
                    UseVisualStyleBackColor = false
                };
                butt3.Enabled = true;
                butt3.ForeColor = Color.White;
                butt3.FlatAppearance.BorderSize = 0;
                butt3.FlatAppearance.MouseDownBackColor = Colors.MOSITLAB_RED_DARK;
                butt3.FlatAppearance.MouseOverBackColor = Colors.MOSITLAB_RED;

                butt3.Click += delegate
                {
                    using (var dialog = new SqlDataSourceConfigForm(textBox.Text))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            textBox.Text = dialog.CrutchPath;
                        }
                    }
                };
                HoppersPanel.Controls.Add(butt3);

                if (item.Data.IsDirectory)
                {
                    butt.Click += delegate
                    {
                        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                        {
                            sessionLink.HoppersHandler.ModuleActions(item.Data, item.Interface, new string[] { folderBrowserDialog1.SelectedPath });
                        }
                    };
                }
                else
                {
                    butt.Click += delegate
                    {
                        openFileDialog1.Multiselect = mult;
                        if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            sessionLink.HoppersHandler.ModuleActions(item.Data, item.Interface, openFileDialog1.FileNames);
                        }
                    };
                }

                top += textBox.Height + 11;

                // Финал
                HoppersInInterface.Add(new HopperUIClass()
                {
                    Bar = bar,
                    Button = butt,
                    HopperData = item.Data,
                    HopperLink = item.Interface,
                    Label = label,
                    PathTextBox = textBox
                });
            }
        
            SetTheme(sessionLink.SettingsController.CurrentSettings.CurrentTheme);
        }


        private void HoppersStartLoad(HopperAttribute attribute, IHopperInterface @interface, int arg3)
        {
            var uiClass = HoppersInInterface[attribute.HopperIndex];
            uiClass.Label.BeginInvoke(new Action(() =>
            {
                // Если хрень папочная:
                if (attribute.IsDirectory)
                {
                    uiClass.Bar.Maximum = arg3;
                    uiClass.Bar.Minimum = 0;
                    uiClass.Bar.Value = 0;
                    uiClass.Bar.IsInfinite = false;
                }
                else
                {
                    // Иначе:
                    uiClass.Bar.IsInfinite = arg3 <= 0;
                    uiClass.Bar.Maximum = arg3;
                    uiClass.Bar.Minimum = 0;
                    uiClass.Bar.Value = 0;
                }
            }));
        }

        private void HoppersOnLoad(HopperAttribute attribute, IHopperInterface @interface, int arg3)
        {
            var uiClass = HoppersInInterface[attribute.HopperIndex];
            uiClass.Label.BeginInvoke(new Action(() =>
            {
                uiClass.Bar.Value++;
            }));
        }

        private void HoppersEndLoad(HopperAttribute attribute, IHopperInterface @interface, int arg3)
        {
            var uiClass = HoppersInInterface[attribute.HopperIndex];
            uiClass.Label.BeginInvoke(new Action(() =>
            {
                uiClass.Label.Text = "✔";
                if (!uiClass.Bar.IsInfinite) uiClass.Bar.Value = uiClass.Bar.Maximum;
            }));
        }
    }
}

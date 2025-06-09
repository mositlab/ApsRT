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
using Rodana.Core.Partias.PartiasAlgorithm;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using System.Linq;
using System.Diagnostics.Metrics;
using Rodana.Core.Partias.PartiasClasses;
using Rodana.Core.Partias.PartiasGeneratorEngine;
using Rodana.Core.Partias.PartiasConnectorEngine;
using System.Security.Cryptography;
using UniversalPack.Common;
using static Google.Protobuf.WellKnownTypes.Field.Types;
using System;
using System.Windows.Forms;
using static RodanaCustomComponents.FormsExtensions;

namespace Rodana.ZMK
{
    public partial class MainForm : Form
    {
        Session sessionLink;

        private static void OnDataLoaded(Session session)
        {
            var options = session.SessionOptions;
            var allStages = session.MasterDataLinks.StagesMasterData.AllStages;
            if (allStages.Count == 0)
            {
                if (!options.IsSilent)
                {
                    MessageBox.Show("АПС не может продолжать работу - таблица операций пуста.", "Пустая таблица операций", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public MainForm(Session sessionLink)
        {
            InitializeComponent();

            ganttOLD.Dock = DockStyle.Fill;
            ganttOLD.BorderStyle = BorderStyle.FixedSingle;
            ganttOLD.BringToFront();
            ganttOLD.BackColor = Color.FromArgb(210, 210, 210);
            ganttOLD.AssignScrollBar(vScrollBarOLD);
            ganttOLD.From = DateTime.Now;
            ganttOLD.To = DateTime.Now.AddDays(1);
            ganttOLD.AssignedTextBox = BarDescTextBox;

            ganttNEW.Dock = DockStyle.Fill;
            ganttNEW.BorderStyle = BorderStyle.FixedSingle;
            ganttNEW.BringToFront();
            ganttNEW.BackColor = Color.FromArgb(210, 210, 210);
            ganttNEW.AssignScrollBar(vScrollBarNEW);
            ganttNEW.From = DateTime.Now;
            ganttNEW.To = DateTime.Now.AddDays(1);
            ganttNEW.AssignedTextBox = BarDescTextBox;

            dateTimePickerFrom.Value = DateTime.Now;
            dateTimePickerTo.Value = DateTime.Now.AddDays(1);

            ganttOLD.ChangedDatesDelegate = updateAlldateTimePicker;
            ganttNEW.ChangedDatesDelegate = updateAlldateTimePicker;

            AllForAllOrdersComboBox.ValueMember = "ValueIndex";
            AllForAllOrdersComboBox.DisplayMember = "StringLabel";
            AllForAllPartiasGuidComboBox.ValueMember = "ValueIndex";
            AllForAllPartiasGuidComboBox.DisplayMember = "StringLabel";

            TwoPartiasOrder1ComboBox.ValueMember = "ValueIndex";
            TwoPartiasOrder1ComboBox.DisplayMember = "StringLabel";
            TwoPartiasOrder2ComboBox.ValueMember = "ValueIndex";
            TwoPartiasOrder2ComboBox.DisplayMember = "StringLabel";

            TwoPartiasPartia1ComboBox.ValueMember = "ValueIndex";
            TwoPartiasPartia1ComboBox.DisplayMember = "StringLabel";
            TwoPartiasPartia2ComboBox.ValueMember = "ValueIndex";
            TwoPartiasPartia2ComboBox.DisplayMember = "StringLabel";

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

            sessionLink.OnSessionDataLoaded += OnDataLoaded;

            sessionLink.OnSessionDataLoaded += delegate
            {
                PostDataProcessInGui();
            };

            sessionLink.HoppersHandler.OnStartLoadingEvent += HoppersStartLoad;
            sessionLink.HoppersHandler.OnLoadingEvent += HoppersOnLoad;
            sessionLink.HoppersHandler.OnEndLoadingEvent += HoppersEndLoad;

            CustomTreeViewOnPPanel.SessionLink = sessionLink;
            TwoPartiasPartia1View.SessionLink = sessionLink;
            TwoPartiasPartia2View.SessionLink = sessionLink;
            TwoPartiasPartiaResView.SessionLink = sessionLink;
            CustomTreeViewOnPPanel.AssignedTextBox = AllForAllTextBox;
            TwoPartiasPartia1View.AssignedTextBox = TwoPartiasTextBox;
            TwoPartiasPartia2View.AssignedTextBox = TwoPartiasTextBox;
            TwoPartiasPartiaResView.AssignedTextBox = TwoPartiasTextBox;

            CreateHoppers();

            HoppersBarsThread = Task.Run(HoppersBarsThreadMethod);

            PartCheckBox.Checked = sessionLink.SessionOptions.LaunchPartia;
            PartCheckBox.CheckedChanged += (sender, args) => { sessionLink.SessionOptions.LaunchPartia = ((CheckBox)sender).Checked; };
            ZadelCheckBox.Checked = sessionLink.SessionOptions.LaunchZadel;
            ZadelCheckBox.CheckedChanged += (sender, args) => { sessionLink.SessionOptions.LaunchZadel = ((CheckBox)sender).Checked; };

            PrepareSettings();
        }

        void updateAlldateTimePicker(DateTime From, DateTime To)
        {
            dateTimePickerFrom.SuspendLayout();
            dateTimePickerFrom.Value = From;
            dateTimePickerFrom.ResumeLayout();

            dateTimePickerTo.SuspendLayout();
            dateTimePickerTo.Value = To;
            dateTimePickerTo.ResumeLayout();

            /*ganttAnalysis.To =*/ ganttNEW.To = ganttOLD.To = To;
            /*ganttAnalysis.From = */ganttNEW.From = ganttOLD.From = From;
            //ganttAnalysis.DrawChart();
            ganttNEW.DrawChart();
            ganttOLD.DrawChart();
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

        Color color, colorDark, textColor, backColor;
        void SetTheme(FormTheme theme)
        {
            color = theme == FormTheme.LightRed || theme == FormTheme.DarkRed ? MOSITLAB_RED : MOSITLAB_BLUE;
            colorDark = theme == FormTheme.LightRed || theme == FormTheme.DarkRed ? MOSITLAB_RED_DARK : MOSITLAB_BLUE_DARK;
            textColor = theme == FormTheme.DarkBlue || theme == FormTheme.DarkRed ? Color.White : Color.Black;
            backColor = theme == FormTheme.DarkBlue || theme == FormTheme.DarkRed ? Color.FromArgb(64, 64, 64) : Color.White;

            PanelNearMainLabel.BackColor = textColor;
            //mainlabel.ForeColor = textColor;

            this.SetFormStyle(color, colorDark, textColor, backColor);
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
        List<int> HoppersCurrentValues = new();

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
                        dialog.SetFormStyle(color, colorDark, textColor, backColor);
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
                HoppersCurrentValues.Add(0);
            }

            SetTheme(sessionLink.SettingsController.CurrentSettings.CurrentTheme);
        }


        Task HoppersBarsThread;

        void HoppersBarsThreadMethod()
        {
            while (!IsDisposed)
            {
                for (int i = 0; i < HoppersCurrentValues.Count; i++)
                {
                    HoppersInInterface[i].Bar.Value = HoppersCurrentValues[i];
                }
                Thread.Sleep(250);
            }
        }

        private void HoppersStartLoad(HopperAttribute attribute, IHopperInterface @interface, int arg3)
        {
            HoppersCurrentValues[attribute.HopperIndex] = 0;
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
            HoppersCurrentValues[attribute.HopperIndex] = arg3;
            //var uiClass = HoppersInInterface[attribute.HopperIndex];
            //uiClass.Label.BeginInvoke(new Action(() =>
            //{
            //    uiClass.Bar.Value = arg3;
            //}));

            //if (arg3 % 2000 == 0)
            //{
            //    var uiClass = HoppersInInterface[attribute.HopperIndex];
            //    uiClass.Label.BeginInvoke(new Action(() =>
            //    {
            //        uiClass.Bar.Value = arg3;
            //    }));
            //}

        }

        private void HoppersEndLoad(HopperAttribute attribute, IHopperInterface @interface, int arg3)
        {
            var uiClass = HoppersInInterface[attribute.HopperIndex];
            uiClass.Label.BeginInvoke(new Action(() =>
            {
                uiClass.Label.Text = "✔";
                if (!uiClass.Bar.IsInfinite)
                {
                    HoppersCurrentValues[attribute.HopperIndex] = uiClass.Bar.Maximum;
                    uiClass.Bar.Value = uiClass.Bar.Maximum;
                }
            }));
        }

        Task partiasAlgroithm;
        private void StartAlgorithmPButton_Click(object sender, EventArgs e)
        {
            if (sessionLink.TryCreateNewPartiasAlgorithmInstance(out var algorithm))
            {
                StartAlgorithmPButton.Enabled = false;

                partiasAlgroithm = Task.Run(() => PartiasAlgorithm(algorithm));
            }
        }

        private void Algorithm_CurrentStageChanged(PartiasAlgorithmStage arg1, int arg2)
        {
            PartiasProgressLabel.Invoke(delegate
            {
                PartiasProgressLabel.Text = arg1 switch
                {
                    PartiasAlgorithmStage.None => "...",
                    PartiasAlgorithmStage.PreparingSession => "1. Подготовка сессии",
                    PartiasAlgorithmStage.PreparingPartias => "2. Подготовка партий",
                    PartiasAlgorithmStage.PreparingLeftovers => "4. Подготовка остатков",
                    PartiasAlgorithmStage.FillingDeepTables => "5. Заполнение таблиц глубин",
                    PartiasAlgorithmStage.FiilingDeadlineTables => "6. Заполнение таблиц дедлайнов",
                    PartiasAlgorithmStage.Searching => "7. Поиск",
                    PartiasAlgorithmStage.AfterSearchErrorsCheck => "8. Проверка ошибок",
                    PartiasAlgorithmStage.InsidePartiasOptimization => "9. Объединение внутри партий",
                    PartiasAlgorithmStage.FormingEasyFlags => "11. Формирование общих этапов",
                    PartiasAlgorithmStage.Output => "12. Вывод",
                    PartiasAlgorithmStage.Complete => "13. Завершено",
                    PartiasAlgorithmStage.Failed => "14. Ошибка"
                };
                PartiasProgressBar.Maximum = arg2;
            });
        }

        void PartiasAlgorithm(PartiasAlgorithm algorithm)
        {
            algorithm.CurrentStageChanged += Algorithm_CurrentStageChanged;
            algorithm.AlgorithmCallback += Algorithm_AlgorithmCallback;

            int N = 0;

            if (algorithm.Start(LogPartiasCheckBox.Checked))
            {
                while (!algorithm.Process.IsCompleted)
                {
                    PartiasProgressBar.Invoke(delegate { PartiasProgressBar.Value = N; });
                    Thread.Sleep(250);
                }
            }

            void Algorithm_AlgorithmCallback(PartiasAlgorithmStage arg1, int arg2)
                => N = arg2;

            algorithm.CurrentStageChanged -= Algorithm_CurrentStageChanged;
            algorithm.AlgorithmCallback -= Algorithm_AlgorithmCallback;

            StartAlgorithmPButton.Invoke(delegate
            {
                SavedPartiasUpdate();
                StartAlgorithmPButton.Enabled = true;
                if (algorithm.CurrentStage == PartiasAlgorithmStage.Complete)
                {
                    PartiasProgressBar.Value = PartiasProgressBar.Maximum;
                }
            });
        }

        private void HandSessionPostDataProcessButton_Click(object sender, EventArgs e)
        {
            sessionLink.PostDataProcess();
            PostDataProcessInGui();
        }

        void PostDataProcessInGui()
        {
            StartAlgorithmPButton.Enabled = true;
            TwoPartiasStartButton.Enabled = true;
            PartiasProgressLabel.Text = "...";

            LaunchPlanningButton.Enabled = true;
            PlanningProcessLabel.Text = "...";

            var orders = sessionLink.MasterDataLinks.OrdersMasterData.AllOrders.Select(x => x.Key);

            var groups = orders.GroupBy(x => x.Object);

            var onlyFirsts = groups
                .Select(x => x.Key)
                .OrderBy(x => x.Label).ToArray();

            AllForAllOrdersComboBox.Items.Clear();
            TwoPartiasOrder1ComboBox.Items.Clear();
            TwoPartiasOrder2ComboBox.Items.Clear();
            foreach (var ord in onlyFirsts)
            {
                AllForAllOrdersComboBox.Items.Add(new CodedObjectInGui(ord));
                TwoPartiasOrder1ComboBox.Items.Add(new CodedObjectInGui(ord));
                TwoPartiasOrder2ComboBox.Items.Add(new CodedObjectInGui(ord));
            }
        }


        private void SavedPartiasUpdate()
        {
            var partiasAlgorithm = sessionLink.AssignedPartiasAlgorithm;

            if (partiasAlgorithm == null) return;

            var partias = partiasAlgorithm.SavedPartias.Select(x => x.Key).ToList();


            AllForAllPartiasGuidComboBox.Items.Clear();
            TwoPartiasPartia1ComboBox.Items.Clear();
            TwoPartiasPartia2ComboBox.Items.Clear();
            Guid? neededOrder = AllForAllOrdersComboBox.SelectedIndex == -1 ? null : ((CodedObjectInGui)AllForAllOrdersComboBox.Items[AllForAllOrdersComboBox.SelectedIndex]).ValueIndex;
            foreach (var partia in partias)
            {
                if (!neededOrder.HasValue || partiasAlgorithm.SavedPartias[partia].Any(x => sessionLink.MasterDataLinks.StagesMasterData.AllStages[x].OrderLink.OrderID.Object.Code == neededOrder.Value))
                {
                    AllForAllPartiasGuidComboBox.Items.Add(new CodedObjectInGui(partia, partia.ToString()));
                }
                TwoPartiasPartia1ComboBox.Items.Add(new CodedObjectInGui(partia, partia.ToString()));
                TwoPartiasPartia2ComboBox.Items.Add(new CodedObjectInGui(partia, partia.ToString()));
            }
        }

        private void AllForAllOrdersComboBox_SelectedIndexChanged(object sender, EventArgs e) => SavedPartiasUpdate();

        private void AllForAllPartiasGuidComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AllForAllPartiasGuidComboBox.SelectedIndex != -1)
            {
                var partiasAlgorithm = sessionLink.AssignedPartiasAlgorithm;

                if (partiasAlgorithm == null) return;

                var partia = ((CodedObjectInGui)AllForAllPartiasGuidComboBox.Items[AllForAllPartiasGuidComboBox.SelectedIndex]).ValueIndex;

                var aps = new PartiaClass(PartiasGeneratorEngine.DeepCopy(partiasAlgorithm.SavedPartiasTrees[partia].List), DateTime.Now, false, 0, partia, partiasAlgorithm.SavedPartiasTrees[partia].GPsCount);
                aps.LastK = partiasAlgorithm.SavedPartiasTrees[partia].K;
                aps.Sort();

                PartiasDataTable.Rows.Clear();
                var ALL = sessionLink.MasterDataLinks.StagesMasterData.AllStages;
                var allOrdersHere = partiasAlgorithm.SavedPartias[partia].Select(index => ALL[index].OrderLink.OrderID).Distinct().ToList();
                foreach ( var order in allOrdersHere)
                {
                    var bestStage = partiasAlgorithm.SavedPartias[partia]
                        .Where(index => ALL[index].OrderLink.OrderID == order)
                        .MinBy(index => ALL[index].Count);

                    var stage = ALL[bestStage];

                    PartiasDataTable.Rows.Add(
                        order.Object,
                        order.Version,
                        stage.Production,
                        stage.Count,
                        sessionLink.MasterDataLinks.OrdersMasterData.AllOrders[order].Deadline
                        );
                }

                CustomTreeViewOnPPanel.SetThisTree(aps);
            }
        }

        private void ClearAllSessionErrorsButton_Click(object sender, EventArgs e)
        {
            sessionLink.DataErrors.NewDataErrors.Clear();
        }

        private void TwoPartiasStartButton_Click(object sender, EventArgs e)
        {
            try
            {
                var partiasAlgorithm = sessionLink.AssignedPartiasAlgorithm;

                if (partiasAlgorithm == null) sessionLink.TryCreateNewPartiasAlgorithmInstance(out partiasAlgorithm);

                var col1 = Colors.NewRandomColor();
                var col2 = Colors.NewRandomColor();

                bool withUI = LogPartiasCheckBox.Checked;

                var Orders = sessionLink.MasterDataLinks.OrdersMasterData.AllOrders;

                Guid? GUID1 = null;
                if (TwoPartiasPartia1ComboBox.SelectedIndex != -1)
                {
                    var guid1 = ((CodedObjectInGui)TwoPartiasPartia1ComboBox.Items[TwoPartiasPartia1ComboBox.SelectedIndex]);
                    GUID1 = guid1.ValueIndex;
                }
                Guid? GUID2 = null;
                if (TwoPartiasPartia2ComboBox.SelectedIndex != -1)
                {
                    var guid2 = ((CodedObjectInGui)TwoPartiasPartia2ComboBox.Items[TwoPartiasPartia2ComboBox.SelectedIndex]);
                    GUID2 = guid2.ValueIndex;
                }

                OrderID? order1 = null;
                if (TwoPartiasOrder1ComboBox.SelectedIndex != -1)
                {
                    var ord1 = ((CodedObjectInGui)TwoPartiasOrder1ComboBox.Items[TwoPartiasOrder1ComboBox.SelectedIndex]);
                    order1 = new OrderID(new CodedObject<Guid, string>(ord1.ValueIndex, ord1.StringLabel), TwoPartiasGP1ComboBox.Text);
                }
                OrderID? order2 = null;
                if (TwoPartiasOrder2ComboBox.SelectedIndex != -1)
                {
                    var ord2 = ((CodedObjectInGui)TwoPartiasOrder2ComboBox.Items[TwoPartiasOrder2ComboBox.SelectedIndex]);
                    order2 = new OrderID(new CodedObject<Guid, string>(ord2.ValueIndex, ord2.StringLabel), TwoPartiasGP2ComboBox.Text);
                }

                List<List<PartiaModelling>> tree1;
                if (GUID1.HasValue && partiasAlgorithm.SavedPartiasTrees.TryGetValue(GUID1.Value, out var ress)) tree1 = PartiasGeneratorEngine.DeepCopy(ress.List);
                else
                {
                    var find = partiasAlgorithm.FindStagesToProcess(GUID1, order1);
                    tree1 = PartiasGeneratorEngine.GenerateTree(find, col1, withUI, Orders[find[0].OrderLink.OrderID].PartiaPF);
                }

                List<List<PartiaModelling>> tree2;
                if (GUID2.HasValue && partiasAlgorithm.SavedPartiasTrees.TryGetValue(GUID2.Value, out var ress2)) tree2 = PartiasGeneratorEngine.DeepCopy(ress2.List);
                else
                {
                    var find = partiasAlgorithm.FindStagesToProcess(GUID2, order2);
                    tree2 = PartiasGeneratorEngine.GenerateTree(find, col2, withUI, Orders[find[0].OrderLink.OrderID].PartiaPF);
                }

                var partiaClass1 = new PartiaClass(tree1, Orders[tree1[0][0].Opers[0].OrderLink.OrderID].Deadline, false, 0, Guid.NewGuid());
                partiaClass1.Sort();
                TwoPartiasPartia1View.SetThisTree(partiaClass1);
                var partiaClass2 = new PartiaClass(tree2, Orders[tree2[0][0].Opers[0].OrderLink.OrderID].Deadline, false, 0, Guid.NewGuid());
                partiaClass2.Sort();
                TwoPartiasPartia2View.SetThisTree(partiaClass2);

                PartiasConnectorEngineResult res = new PartiasConnectorEngineResult(0, 0, 0, 0);
                var areDiff = tree1[0][0].Opers[0].OrderLink.OrderID != tree2[0][0].Opers[0].OrderLink.OrderID;
                var res0 = -1;
                if (areDiff)
                {
                    res0 = PartiasConnectorEngine.Connect(partiaClass1, partiaClass2, withUI, false).Total;
                }

                if (InGPCheckBox.Checked)
                {
                    res = PartiasConnectorEngine.Connect(partiaClass1, partiaClass1, withUI, true);
                    if (areDiff && res0 == 0 && partiaClass2.OpersHere[0].Count > 0)
                    {
                        res = PartiasConnectorEngine.Connect(partiaClass2, partiaClass2, withUI, true);
                    }
                }

                if (areDiff)
                {
                    TwoPartiasPartiaResView.SetThisTree(partiaClass1, partiaClass2);
                }
                else
                {
                    TwoPartiasPartiaResView.SetThisTree(partiaClass1);
                }

                if (partiaClass1 != null)
                {
                    partiasAlgorithm.SavedPartias.Add(partiaClass1.MyGuid, PartiasConnectorEngine.SeeSdvigsAsList(partiaClass1.OpersHere).SelectMany(x => x.Opers.Select(y => y.Index)).ToHashSet());
                    partiasAlgorithm.SavedPartiasTrees.Add(partiaClass1.MyGuid, (partiaClass1.OpersHere, partiaClass1.GPsCountNow, partiaClass1.LastK));
                    if (areDiff)
                    {
                        partiasAlgorithm.SavedPartias.Add(partiaClass2.MyGuid, PartiasConnectorEngine.SeeSdvigsAsList(partiaClass2.OpersHere).SelectMany(x => x.Opers.Select(y => y.Index)).ToHashSet());
                        partiasAlgorithm.SavedPartiasTrees.Add(partiaClass2.MyGuid, (partiaClass2.OpersHere, partiaClass2.GPsCountNow, partiaClass2.LastK));
                    }
                    SavedPartiasUpdate();
                }
            }
            catch
            {
                MessageBox.Show("Что-то пошло не так!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
           
        }

        private void TwoPartiasOrder1ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TwoPartiasOrder1ComboBox.SelectedIndex != -1)
            {
                var order = ((CodedObjectInGui)TwoPartiasOrder1ComboBox.Items[TwoPartiasOrder1ComboBox.SelectedIndex]);
                var coded = new CodedObject<Guid, string>(order.ValueIndex, order.StringLabel);
                var allGPs = sessionLink.MasterDataLinks.OrdersMasterData.AllOrders.Where(x => x.Key.Object == coded).Select(x => x.Key.Version).OrderBy(x => x).ToList();

                TwoPartiasGP1ComboBox.Items.Clear();
                foreach (var gp in allGPs) TwoPartiasGP1ComboBox.Items.Add(gp);
            }
        }

        private void TwoPartiasOrder2ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TwoPartiasOrder2ComboBox.SelectedIndex != -1)
            {
                var order = ((CodedObjectInGui)TwoPartiasOrder2ComboBox.Items[TwoPartiasOrder2ComboBox.SelectedIndex]);
                var coded = new CodedObject<Guid, string>(order.ValueIndex, order.StringLabel);
                var allGPs = sessionLink.MasterDataLinks.OrdersMasterData.AllOrders.Where(x => x.Key.Object == coded).Select(x => x.Key.Version).OrderBy(x => x).ToList();

                TwoPartiasGP2ComboBox.Items.Clear();
                foreach (var gp in allGPs) TwoPartiasGP2ComboBox.Items.Add(gp);
            }
        }
    }
}

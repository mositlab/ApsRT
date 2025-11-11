using ApsRT.Core.Hoppers;
using ApsRT.Core.Session;
using ApsRT.Core.Settings;
using System.Xml.Linq;
using UniversalPack.Common.Drawing;
using UniversalPack;
using static UniversalPack.Common.Drawing.Colors;
using WinFormsCustomComponents;
using static UniversalPack.Extensions.EventExtensions;
using ApsRTCustomComponents;
using ApsRT.Core.Partias.PartiasAlgorithm;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using System.Linq;
using System.Diagnostics.Metrics;
using ApsRT.Core.Partias.PartiasClasses;
using ApsRT.Core.Partias.PartiasGeneratorEngine;
using ApsRT.Core.Partias.PartiasConnectorEngine;
using System.Security.Cryptography;
using UniversalPack.Common;
using static Google.Protobuf.WellKnownTypes.Field.Types;
using System;
using System.Windows.Forms;
using static ApsRTCustomComponents.FormsExtensions;
using ApsRT.Core.Planning.PlanningAlgorithm;
using ApsRT.Core.Centers.MasterData;
using ApsRT.Core.Rechanges.RechangesEngine;
using ZedGraph;
using Label = System.Windows.Forms.Label;
using System.Collections.Generic;
using TextBox = System.Windows.Forms.TextBox;
using ApsRT.Core.Stages.MasterData;
using ApsRT.Core.PlanSimulator;
using ApsRT.Core.Planning;
using ApsRT.Core.PlanSimulator.PlanSimulatorEngine;
using UniversalPack.Extensions;

namespace ApsRTCustomComponents
{
    public partial class ApsRTMainForm1 : Form
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

        #region Настройки и действия формы

        private void HandSessionPostDataProcessButton_Click(object sender, EventArgs e)
        {
            sessionLink.PostDataProcess();
            PostDataProcessInGui();
        }
        private void ClearAllSessionErrorsButton_Click(object sender, EventArgs e)
        {
            sessionLink.DataErrors.NewDataErrors.Clear();
            sessionLink.DataErrors.NewDataErrorsNoStages.Clear();
        }

        public ApsRTMainForm1(Session sessionLink, string versionName, Color[] colors)
        {
            InitializeComponent();
            this.colors = colors;

            if (!string.IsNullOrEmpty(versionName))
            {
                VersionLabel.Text = versionName;
            }

            CreatePlanningTable();

            PrepareGraphs();


            var analysisMenuItem = new ToolStripMenuItem("Анализировать");
            analysisMenuItem.Click += AnalysisMenuItem_Click;
            contextMenuStrip1.Items.Add(analysisMenuItem);

            ganttOLD.Dock = DockStyle.Fill;
            ganttOLD.BorderStyle = BorderStyle.FixedSingle;
            ganttOLD.BringToFront();
            ganttOLD.BackColor = Color.FromArgb(210, 210, 210);
            ganttOLD.AssignScrollBar(vScrollBarOLD);
            ganttOLD.From = DateTime.Now;
            ganttOLD.To = DateTime.Now.AddDays(1);
            ganttOLD.AssignedTextBox = BarDescTextBox;
            ganttOLD.ContextMenuStrip = contextMenuStrip1;

            ganttNEW.Dock = DockStyle.Fill;
            ganttNEW.BorderStyle = BorderStyle.FixedSingle;
            ganttNEW.BringToFront();
            ganttNEW.BackColor = Color.FromArgb(210, 210, 210);
            ganttNEW.AssignScrollBar(vScrollBarNEW);
            ganttNEW.From = DateTime.Now;
            ganttNEW.To = DateTime.Now.AddDays(1);
            ganttNEW.AssignedTextBox = BarDescTextBox;
            ganttNEW.ContextMenuStrip = contextMenuStrip1;

            dateTimePickerFrom.Value = DateTime.Now;
            dateTimePickerTo.Value = DateTime.Now.AddDays(1);

            ganttOLD.ChangedDatesDelegate = updateAlldateTimePicker;
            ganttNEW.ChangedDatesDelegate = updateAlldateTimePicker;

            //ganttOLD.Dock = DockStyle.Fill;
            //ganttANL.BorderStyle = BorderStyle.FixedSingle;
            //ganttANL.BringToFront();
            ganttANL.BackColor = Color.FromArgb(210, 210, 210);
            ganttANL.AssignScrollBar(vScrollBarANL);
            ganttANL.From = DateTime.Now;
            ganttANL.To = DateTime.Now.AddDays(1);
            ganttANL.AssignedTextBox = AnlDescTextBox;
            ganttANL.CanMoveOperations = true; // В отличие от других, эта может двигать операции
            ganttANL.OnAnalysis += UpdateAnalysis;

            dateTimePickerFromAnl.Value = DateTime.Now;
            dateTimePickerToAnl.Value = DateTime.Now.AddDays(1);

            ganttANL.ChangedDatesDelegate = updateAlldateTimePicker2;

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

            /*ganttAnalysis.To =*/
            ganttNEW.To = ganttOLD.To = To;
            /*ganttAnalysis.From = */
            ganttNEW.From = ganttOLD.From = From;
            //ganttAnalysis.DrawChart();
            ganttNEW.DrawChart();
            ganttOLD.DrawChart();
        }

        void updateAlldateTimePicker2(DateTime From, DateTime To)
        {
            dateTimePickerFromAnl.SuspendLayout();
            dateTimePickerFromAnl.Value = From;
            dateTimePickerFromAnl.ResumeLayout();

            dateTimePickerToAnl.SuspendLayout();
            dateTimePickerToAnl.Value = To;
            dateTimePickerToAnl.ResumeLayout();

            /*ganttAnalysis.To =*/
            ganttANL.To = To;
            /*ganttAnalysis.From = */
            ganttANL.From = From;
            //ganttAnalysis.DrawChart();
            ganttANL.DrawChart();
        }

        private void SaveSettingsButton_Click(object? sender, EventArgs e)
        {
            sessionLink.SettingsController.CurrentSettings.HoppersPaths = HoppersGetPaths();
            sessionLink.SettingsController.Save();
        }

        void PrepareSettings()
        {

            var settings = sessionLink.SettingsController.CurrentSettings;

            VKoeffsTextBox.Text = string.Join(", ", settings.VKoeffs);
            VKoeffsTextBox.TextChanged += (sender, args) =>
            {
                try
                {
                    settings.VKoeffs = ((TextBox)sender).Text.Replace(" ", "").Split(',').Select(x => double.Parse(x.Replace('.', ','))).ToArray();
                }
                catch
                {

                }
            };

            CriteriasListTextBox.Text = string.Join(",", settings.CriteriasList);
            CriteriasListTextBox.TextChanged += (sender, args) =>
            {
                try
                {
                    settings.CriteriasList = ((TextBox)sender).Text.Replace(" ", "").Split(',').Select(x => int.Parse(x)).ToArray();
                }
                catch
                {

                }
            };

            CriteriasPossibleWorseNumericUpDown1.Value = (decimal)settings.CriteriasPossibleWorse[0];
            CriteriasPossibleWorseNumericUpDown1.ValueChanged += (sender, args) => { settings.CriteriasPossibleWorse[0] = (int)((NumericUpDown)sender).Value; };
            CriteriasPossibleWorseNumericUpDown2.Value = (decimal)settings.CriteriasPossibleWorse[1];
            CriteriasPossibleWorseNumericUpDown2.ValueChanged += (sender, args) => { settings.CriteriasPossibleWorse[1] = (int)((NumericUpDown)sender).Value; };
            CriteriasPossibleWorseNumericUpDown3.Value = (decimal)settings.CriteriasPossibleWorse[2];
            CriteriasPossibleWorseNumericUpDown3.ValueChanged += (sender, args) => { settings.CriteriasPossibleWorse[2] = (int)((NumericUpDown)sender).Value; };
            CriteriasPossibleWorseNumericUpDown4.Value = (decimal)settings.CriteriasPossibleWorse[3];
            CriteriasPossibleWorseNumericUpDown4.ValueChanged += (sender, args) => { settings.CriteriasPossibleWorse[3] = (int)((NumericUpDown)sender).Value; };
            CriteriasPossibleWorseNumericUpDown5.Value = (decimal)settings.CriteriasPossibleWorse[4];
            CriteriasPossibleWorseNumericUpDown5.ValueChanged += (sender, args) => { settings.CriteriasPossibleWorse[4] = (int)((NumericUpDown)sender).Value; };
            CriteriasPossibleWorseNumericUpDown6.Value = (decimal)settings.CriteriasPossibleWorse[5];
            CriteriasPossibleWorseNumericUpDown6.ValueChanged += (sender, args) => { settings.CriteriasPossibleWorse[5] = (int)((NumericUpDown)sender).Value; };
            CriteriasPossibleWorseNumericUpDown7.Value = (decimal)settings.CriteriasPossibleWorse[6];
            CriteriasPossibleWorseNumericUpDown7.ValueChanged += (sender, args) => { settings.CriteriasPossibleWorse[6] = (int)((NumericUpDown)sender).Value; };

            CriteriasMinIterationsNumericUpDown1.Value = (decimal)settings.CriteriasMinIterations[0];
            CriteriasMinIterationsNumericUpDown1.ValueChanged += (sender, args) => { settings.CriteriasMinIterations[0] = (int)((NumericUpDown)sender).Value; };
            CriteriasMinIterationsNumericUpDown2.Value = (decimal)settings.CriteriasMinIterations[1];
            CriteriasMinIterationsNumericUpDown2.ValueChanged += (sender, args) => { settings.CriteriasMinIterations[1] = (int)((NumericUpDown)sender).Value; };
            CriteriasMinIterationsNumericUpDown3.Value = (decimal)settings.CriteriasMinIterations[2];
            CriteriasMinIterationsNumericUpDown3.ValueChanged += (sender, args) => { settings.CriteriasMinIterations[2] = (int)((NumericUpDown)sender).Value; };
            CriteriasMinIterationsNumericUpDown4.Value = (decimal)settings.CriteriasMinIterations[3];
            CriteriasMinIterationsNumericUpDown4.ValueChanged += (sender, args) => { settings.CriteriasMinIterations[3] = (int)((NumericUpDown)sender).Value; };
            CriteriasMinIterationsNumericUpDown5.Value = (decimal)settings.CriteriasMinIterations[4];
            CriteriasMinIterationsNumericUpDown5.ValueChanged += (sender, args) => { settings.CriteriasMinIterations[4] = (int)((NumericUpDown)sender).Value; };
            CriteriasMinIterationsNumericUpDown6.Value = (decimal)settings.CriteriasMinIterations[5];
            CriteriasMinIterationsNumericUpDown6.ValueChanged += (sender, args) => { settings.CriteriasMinIterations[5] = (int)((NumericUpDown)sender).Value; };
            CriteriasMinIterationsNumericUpDown7.Value = (decimal)settings.CriteriasMinIterations[6];
            CriteriasMinIterationsNumericUpDown7.ValueChanged += (sender, args) => { settings.CriteriasMinIterations[6] = (int)((NumericUpDown)sender).Value; };

            CriteriasMaxIterationsNumericUpDown1.Value = (decimal)settings.CriteriasMaxIterations[0];
            CriteriasMaxIterationsNumericUpDown1.ValueChanged += (sender, args) => { settings.CriteriasMaxIterations[0] = (int)((NumericUpDown)sender).Value; };
            CriteriasMaxIterationsNumericUpDown2.Value = (decimal)settings.CriteriasMaxIterations[1];
            CriteriasMaxIterationsNumericUpDown2.ValueChanged += (sender, args) => { settings.CriteriasMaxIterations[1] = (int)((NumericUpDown)sender).Value; };
            CriteriasMaxIterationsNumericUpDown3.Value = (decimal)settings.CriteriasMaxIterations[2];
            CriteriasMaxIterationsNumericUpDown3.ValueChanged += (sender, args) => { settings.CriteriasMaxIterations[2] = (int)((NumericUpDown)sender).Value; };
            CriteriasMaxIterationsNumericUpDown4.Value = (decimal)settings.CriteriasMaxIterations[3];
            CriteriasMaxIterationsNumericUpDown4.ValueChanged += (sender, args) => { settings.CriteriasMaxIterations[3] = (int)((NumericUpDown)sender).Value; };
            CriteriasMaxIterationsNumericUpDown5.Value = (decimal)settings.CriteriasMaxIterations[4];
            CriteriasMaxIterationsNumericUpDown5.ValueChanged += (sender, args) => { settings.CriteriasMaxIterations[4] = (int)((NumericUpDown)sender).Value; };
            CriteriasMaxIterationsNumericUpDown6.Value = (decimal)settings.CriteriasMaxIterations[5];
            CriteriasMaxIterationsNumericUpDown6.ValueChanged += (sender, args) => { settings.CriteriasMaxIterations[5] = (int)((NumericUpDown)sender).Value; };
            CriteriasMaxIterationsNumericUpDown7.Value = (decimal)settings.CriteriasMaxIterations[6];
            CriteriasMaxIterationsNumericUpDown7.ValueChanged += (sender, args) => { settings.CriteriasMaxIterations[6] = (int)((NumericUpDown)sender).Value; };

            UseOnlyMaxCriteriasSettingsCheckBox.Checked = settings.UseOnlyMaxCriteriasSettings;
            UseOnlyMaxCriteriasSettingsCheckBox.CheckedChanged += (sender, args) => { settings.UseOnlyMaxCriteriasSettings = ((CheckBox)sender).Checked; };

            TransferToGammaCheckBox.Checked = settings.TransferToGamma;
            TransferToGammaCheckBox.CheckedChanged += (sender, args) => { settings.TransferToGamma = ((CheckBox)sender).Checked; };

            UseRCheckBox.Checked = settings.UseR;
            UseRCheckBox.CheckedChanged += (sender, args) => { settings.UseR = ((CheckBox)sender).Checked; };

            UseRStarCheckBox.Checked = settings.UseRStar;
            UseRStarCheckBox.CheckedChanged += (sender, args) => { settings.UseRStar = ((CheckBox)sender).Checked; };

            UseTCheckBox.Checked = settings.UseT;
            UseTCheckBox.CheckedChanged += (sender, args) => { settings.UseT = ((CheckBox)sender).Checked; };

            UseGCheckBox.Checked = settings.UseG;
            UseGCheckBox.CheckedChanged += (sender, args) => { settings.UseG = ((CheckBox)sender).Checked; };

            GammaSlider.Value = (int)(settings.TGamma * 1000);
            GammaLabel.Text = settings.TGamma.ToString();
            GammaSlider.ValueChanged += (sender, args) =>
            {
                var val = ((TrackBar)sender).Value / 1000.0;
                settings.TGamma = val;
                GammaLabel.Text = val.ToString();
            };

            RGlobDeltaCheckBox.Checked = settings.RGlobalTimeDelta;
            RGlobDeltaCheckBox.CheckedChanged += (sender, args) => { settings.RGlobalTimeDelta = ((CheckBox)sender).Checked; };

            RNewDeadlineCheckBox.Checked = settings.RNewDeadlineCounter;
            RNewDeadlineCheckBox.CheckedChanged += (sender, args) => { settings.RNewDeadlineCounter = ((CheckBox)sender).Checked; };

            TMaxDaysNumericUpDown.Value = (decimal)settings.TMaxDays;
            TMaxDaysNumericUpDown.ValueChanged += (sender, args) => { settings.TMaxDays = (double)((NumericUpDown)sender).Value; };
            TMaxDaysPercentNumericUpDown.Value = (decimal)settings.TMaxDaysPercent;
            TMaxDaysPercentNumericUpDown.ValueChanged += (sender, args) => { settings.TMaxDaysPercent = (double)((NumericUpDown)sender).Value; };

            StagesPercentNumericUpDown.Value = (decimal)settings.Test_PercentOfStages;
            StagesPercentNumericUpDown.ValueChanged += (sender, args) => { settings.Test_PercentOfStages = (double)((NumericUpDown)sender).Value; };


            criteriaRadioButton1.Checked = settings.UsedCriteria == 0;
            criteriaRadioButton1.CheckedChanged += (sender, args) => { if (((RadioButton)sender).Checked) settings.UsedCriteria = 0; };
            criteriaRadioButton2.Checked = settings.UsedCriteria == 1;
            criteriaRadioButton2.CheckedChanged += (sender, args) => { if (((RadioButton)sender).Checked) settings.UsedCriteria = 1; };
            criteriaRadioButton3.Checked = settings.UsedCriteria == 2;
            criteriaRadioButton3.CheckedChanged += (sender, args) => { if (((RadioButton)sender).Checked) settings.UsedCriteria = 2; };
            criteriaRadioButton4.Checked = settings.UsedCriteria == 3;
            criteriaRadioButton4.CheckedChanged += (sender, args) => { if (((RadioButton)sender).Checked) settings.UsedCriteria = 3; };
            criteriaRadioButton5.Checked = settings.UsedCriteria == 4;
            criteriaRadioButton5.CheckedChanged += (sender, args) => { if (((RadioButton)sender).Checked) settings.UsedCriteria = 4; };
            criteriaRadioButton6.Checked = settings.UsedCriteria == 5;
            criteriaRadioButton6.CheckedChanged += (sender, args) => { if (((RadioButton)sender).Checked) settings.UsedCriteria = 5; };
            criteriaRadioButton7.Checked = settings.UsedCriteria == 6;
            criteriaRadioButton7.CheckedChanged += (sender, args) => { if (((RadioButton)sender).Checked) settings.UsedCriteria = 6; };
            materialRadioButtonCombo.Checked = settings.UsedCriteria == -1;
            materialRadioButtonCombo.CheckedChanged += (sender, args) => { if (((RadioButton)sender).Checked) settings.UsedCriteria = -1; };
            materialRadioButtonAuto.Checked = settings.UsedCriteria == -2;
            materialRadioButtonAuto.CheckedChanged += (sender, args) => { if (((RadioButton)sender).Checked) settings.UsedCriteria = -2; };

            #region Низкий приоритет
            p2l1kNumericUpDown.Value = (decimal)settings.OrderCoeffs[2].Level1.K;
            p2l1kNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[2].Level1.K = (int)((NumericUpDown)sender).Value; };
            p2l1aNumericUpDown.Value = (decimal)settings.OrderCoeffs[2].Level1.A;
            p2l1aNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[2].Level1.A = (int)((NumericUpDown)sender).Value; };
            p2l1xNumericUpDown.Value = (decimal)settings.OrderCoeffs[2].Level1.UpTo;
            p2l1xNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[2].Level1.UpTo = (int)((NumericUpDown)sender).Value; };

            p2l2kNumericUpDown.Value = (decimal)settings.OrderCoeffs[2].Level2.K;
            p2l2kNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[2].Level2.K = (int)((NumericUpDown)sender).Value; };
            p2l2aNumericUpDown.Value = (decimal)settings.OrderCoeffs[2].Level2.A;
            p2l2aNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[2].Level2.A = (int)((NumericUpDown)sender).Value; };
            p2l2xNumericUpDown.Value = (decimal)settings.OrderCoeffs[2].Level2.UpTo;
            p2l2xNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[2].Level2.UpTo = (int)((NumericUpDown)sender).Value; };

            p2l3kNumericUpDown.Value = (decimal)settings.OrderCoeffs[2].Level3.K;
            p2l3kNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[2].Level3.K = (int)((NumericUpDown)sender).Value; };
            p2l3aNumericUpDown.Value = (decimal)settings.OrderCoeffs[2].Level3.A;
            p2l3aNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[2].Level3.A = (int)((NumericUpDown)sender).Value; };
            #endregion

            #region Средний приоритет
            p1l1kNumericUpDown.Value = (decimal)settings.OrderCoeffs[1].Level1.K;
            p1l1kNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[1].Level1.K = (int)((NumericUpDown)sender).Value; };
            p1l1aNumericUpDown.Value = (decimal)settings.OrderCoeffs[1].Level1.A;
            p1l1aNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[1].Level1.A = (int)((NumericUpDown)sender).Value; };
            p1l1xNumericUpDown.Value = (decimal)settings.OrderCoeffs[1].Level1.UpTo;
            p1l1xNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[1].Level1.UpTo = (int)((NumericUpDown)sender).Value; };

            p1l2kNumericUpDown.Value = (decimal)settings.OrderCoeffs[1].Level2.K;
            p1l2kNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[1].Level2.K = (int)((NumericUpDown)sender).Value; };
            p1l2aNumericUpDown.Value = (decimal)settings.OrderCoeffs[1].Level2.A;
            p1l2aNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[1].Level2.A = (int)((NumericUpDown)sender).Value; };
            p1l2xNumericUpDown.Value = (decimal)settings.OrderCoeffs[1].Level2.UpTo;
            p1l2xNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[1].Level2.UpTo = (int)((NumericUpDown)sender).Value; };

            p1l3kNumericUpDown.Value = (decimal)settings.OrderCoeffs[1].Level3.K;
            p1l3kNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[1].Level3.K = (int)((NumericUpDown)sender).Value; };
            p1l3aNumericUpDown.Value = (decimal)settings.OrderCoeffs[1].Level3.A;
            p1l3aNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[1].Level3.A = (int)((NumericUpDown)sender).Value; };
            #endregion

            #region Высокий приоритет
            p0l1kNumericUpDown.Value = (decimal)settings.OrderCoeffs[0].Level1.K;
            p0l1kNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[0].Level1.K = (int)((NumericUpDown)sender).Value; };
            p0l1aNumericUpDown.Value = (decimal)settings.OrderCoeffs[0].Level1.A;
            p0l1aNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[0].Level1.A = (int)((NumericUpDown)sender).Value; };
            p0l1xNumericUpDown.Value = (decimal)settings.OrderCoeffs[0].Level1.UpTo;
            p0l1xNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[0].Level1.UpTo = (int)((NumericUpDown)sender).Value; };

            p0l2kNumericUpDown.Value = (decimal)settings.OrderCoeffs[0].Level2.K;
            p0l2kNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[0].Level2.K = (int)((NumericUpDown)sender).Value; };
            p0l2aNumericUpDown.Value = (decimal)settings.OrderCoeffs[0].Level2.A;
            p0l2aNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[0].Level2.A = (int)((NumericUpDown)sender).Value; };
            p0l2xNumericUpDown.Value = (decimal)settings.OrderCoeffs[0].Level2.UpTo;
            p0l2xNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[0].Level2.UpTo = (int)((NumericUpDown)sender).Value; };

            p0l3kNumericUpDown.Value = (decimal)settings.OrderCoeffs[0].Level3.K;
            p0l3kNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[0].Level3.K = (int)((NumericUpDown)sender).Value; };
            p0l3aNumericUpDown.Value = (decimal)settings.OrderCoeffs[0].Level3.A;
            p0l3aNumericUpDown.ValueChanged += (sender, args) => { settings.OrderCoeffs[0].Level3.A = (int)((NumericUpDown)sender).Value; };
            #endregion

            AfterXMinsRadioButton.Checked = settings.UseMinutes;
            AfterXMinsRadioButton.CheckedChanged += (sender, args) => { settings.UseMinutes = ((RadioButton)sender).Checked; };
            AfterXCyclesRadioButton.Checked = settings.UseCycles;
            AfterXCyclesRadioButton.CheckedChanged += (sender, args) => { settings.UseCycles = ((RadioButton)sender).Checked; };

            IterationsNumericUpDown.Value = (decimal)settings.IterationsCount;
            IterationsNumericUpDown.ValueChanged += (sender, args) => { settings.IterationsCount = (int)((NumericUpDown)sender).Value; };
            MinutesNumericUpDown.Value = (decimal)settings.MinutesCount;
            MinutesNumericUpDown.ValueChanged += (sender, args) => { settings.MinutesCount = (double)((NumericUpDown)sender).Value; };
            CyclesNumericUpDown.Value = (decimal)settings.CyclesCount;
            CyclesNumericUpDown.ValueChanged += (sender, args) => { settings.CyclesCount = (int)((NumericUpDown)sender).Value; };


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

            MatrixMergeConditionsCheckBox.Checked = settings.UseMatrixMergeConditions;
            MatrixMergeConditionsCheckBox.CheckedChanged += (sender, args) => { settings.UseMatrixMergeConditions = ((CheckBox)sender).Checked; };

            PartiasMaxDaysNumericUpDown.Value = (decimal)settings.MaxDaysBetweenPartias;
            PartiasMaxDaysNumericUpDown.ValueChanged += (sender, args) => { settings.MaxDaysBetweenPartias = (double)((NumericUpDown)sender).Value; };

            BackupStagesDurationNumericUpDown.Value = (decimal)settings.OriginalDurationTime;
            BackupStagesDurationNumericUpDown.ValueChanged += (sender, args) => { settings.OriginalDurationTime = (int)((NumericUpDown)sender).Value; };

            MaxOldSessionPathsDaysNumericUpDown.Value = (decimal)settings.MaxOldSessionPathsDays;
            MaxOldSessionPathsDaysNumericUpDown.ValueChanged += (sender, args) => { settings.MaxOldSessionPathsDays = (double)((NumericUpDown)sender).Value; };

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


        private void label49_Click(object sender, EventArgs e)
        {
            MainTabControl.SelectTab(AuthorsTabPage);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MainTabControl.SelectTab(AuthorsTabPage);
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

        Color[] colors;

        Color color, colorDark, textColor, backColor, backColor2;
        void SetTheme(FormTheme theme)
        {
            color = theme == FormTheme.LightRed || theme == FormTheme.DarkRed ? colors[1] : colors[0];
            colorDark = theme == FormTheme.LightRed || theme == FormTheme.DarkRed ? colors[3] : colors[2];
            textColor = theme == FormTheme.DarkBlue || theme == FormTheme.DarkRed ? Color.White : Color.Black;
            backColor = theme == FormTheme.DarkBlue || theme == FormTheme.DarkRed ? Color.FromArgb(64, 64, 64) : Color.White;
            backColor2 = theme == FormTheme.DarkBlue || theme == FormTheme.DarkRed ? Color.Black : Color.White;

            PanelNearMainLabel.BackColor = textColor;
            //mainlabel.ForeColor = textColor;

            this.SetFormStyle(color, colorDark, textColor, backColor, backColor2);
        }

        private void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (sessionLink.SettingsController.CurrentSettings.SaveOnSessionEnd)
            {
                sessionLink.SettingsController.CurrentSettings.HoppersPaths = HoppersGetPaths();
            }
            sessionLink.EndSession(0);
        }

        #endregion

        #region ХОППЕРЫ

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
                        dialog.SetFormStyle(color, colorDark, textColor, backColor, backColor2);
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

        #endregion

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


        #region ПАРТИИ

        Task partiasAlgroithm;
        private void StartAlgorithmPButton_Click(object sender, EventArgs e)
        {
            if (sessionLink.TryCreateNewPartiasAlgorithmInstance(out var algorithm))
            {
                StartAlgorithmPButton.Enabled = false;

                partiasAlgroithm = Task.Run(() => PartiasAlgorithm(algorithm));
            }
        }

        private void PartiasAlgorithm_CurrentStageChanged(PartiasAlgorithmStage arg1, int arg2)
        {
            PartiasProgressLabel.Invoke(delegate
            {
                PartiasProgressLabel.Text = arg1 switch
                {
                    PartiasAlgorithmStage.None => "...",
                    PartiasAlgorithmStage.PreparingSession => "1. Подготовка сессии",
                    PartiasAlgorithmStage.PreparingPartias => "2. Подготовка партий",
                    PartiasAlgorithmStage.PreparingZadelsLocked => "3. Подготовка пользовательских заделов",
                    PartiasAlgorithmStage.FiilingDeadlineTables => "4. Заполнение таблиц дедлайнов",
                    PartiasAlgorithmStage.FillingDeepTables => "5. Заполнение таблиц глубин",
                    PartiasAlgorithmStage.PreparingZadelsFree => "6. Автоматическая расстановка заделов",
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
            algorithm.CurrentStageChanged += PartiasAlgorithm_CurrentStageChanged;
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

            algorithm.CurrentStageChanged -= PartiasAlgorithm_CurrentStageChanged;
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

        private void AllForAllOrdersComboBox_TextChanged(object sender, EventArgs e) => SavedPartiasUpdate();
        private void AllForAllOrdersComboBox_SelectedIndexChanged(object sender, EventArgs e) => SavedPartiasUpdate();

        private void AllForAllPartiasGuidComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AllForAllPartiasGuidComboBox.SelectedIndex != -1)
            {
                var partiasAlgorithm = sessionLink.AssignedPartiasAlgorithm;

                if (partiasAlgorithm == null) return;

                var partia = ((CodedObjectInGui)AllForAllPartiasGuidComboBox.Items[AllForAllPartiasGuidComboBox.SelectedIndex]).ValueIndex;

                var aps = new PartiaClass(PartiasGeneratorEngine.DeepCopy(partiasAlgorithm.SavedPartiasTrees[partia].List), DateTime.Now, false, 0, partia, partiasAlgorithm.SavedPartiasTrees[partia].MaxLongest, partiasAlgorithm.SavedPartiasTrees[partia].GPsCount);
                aps.LastK = partiasAlgorithm.SavedPartiasTrees[partia].K;
                aps.Sort();

                Dictionary<int, ApsRT.Core.NaladLengths.GottenNaladLengthStruct> NaladLengthsData = new();
                PartiasGeneratorEngine.PrepareNaladLengths(aps, NaladLengthsData); // Подготавливаем налад длины

                PartiasDataTable.Rows.Clear();
                var ALL = sessionLink.MasterDataLinks.StagesMasterData.AllStages;
                var allOrdersHere = partiasAlgorithm.SavedPartias[partia].Select(index => ALL[index].OrderLink.OrderID).Distinct().ToList();
                foreach (var order in allOrdersHere)
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

                CustomTreeViewOnPPanel.SetThisTree(NaladLengthsData, aps);
            }
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
                if (TwoPartiasPartia1ComboBox.SelectedIndex == -1 && TwoPartiasPartia1ComboBox.Text.Length > 0)
                    TwoPartiasPartia1ComboBox.SelectedIndex = TwoPartiasPartia1ComboBox.FindStringExact(TwoPartiasPartia1ComboBox.Text);
                if (TwoPartiasPartia1ComboBox.SelectedIndex != -1)
                {
                    var guid1 = ((CodedObjectInGui)TwoPartiasPartia1ComboBox.Items[TwoPartiasPartia1ComboBox.SelectedIndex]);
                    GUID1 = guid1.ValueIndex;
                }
                Guid? GUID2 = null;
                if (TwoPartiasPartia2ComboBox.SelectedIndex == -1 && TwoPartiasPartia2ComboBox.Text.Length > 0)
                    TwoPartiasPartia2ComboBox.SelectedIndex = TwoPartiasPartia2ComboBox.FindStringExact(TwoPartiasPartia2ComboBox.Text);
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

                Dictionary<int, Uco> RestrictionsVia74110 = new();

                List<List<PartiaModelling>> tree1;
                if (GUID1.HasValue && partiasAlgorithm.SavedPartiasTrees.TryGetValue(GUID1.Value, out var ress)) tree1 = PartiasGeneratorEngine.DeepCopy(ress.List);
                else
                {
                    var find = partiasAlgorithm.FindStagesToProcess(GUID1, order1);
                    tree1 = PartiasGeneratorEngine.GenerateTree(new PartiasGeneratorEngineData(find, RestrictionsVia74110), col1, withUI, Orders[find[0].OrderLink.OrderID].PartiaPF);
                }

                List<List<PartiaModelling>> tree2;
                if (GUID2.HasValue && partiasAlgorithm.SavedPartiasTrees.TryGetValue(GUID2.Value, out var ress2)) tree2 = PartiasGeneratorEngine.DeepCopy(ress2.List);
                else
                {
                    var find = partiasAlgorithm.FindStagesToProcess(GUID2, order2);
                    tree2 = PartiasGeneratorEngine.GenerateTree(new PartiasGeneratorEngineData(find, RestrictionsVia74110), col2, withUI, Orders[find[0].OrderLink.OrderID].PartiaPF);
                }

                Dictionary<int, ApsRT.Core.NaladLengths.GottenNaladLengthStruct> NaladLengthsData = new();


                var partiaClass1 = new PartiaClass(tree1, Orders[tree1[0][0].Opers[0].OrderLink.OrderID].Deadline, false, 0, Guid.NewGuid(), -1);
                PartiasGeneratorEngine.PrepareNaladLengths(partiaClass1, NaladLengthsData); // Подготавливаем налад длины
                partiaClass1.Sort();
                TwoPartiasPartia1View.SetThisTree(NaladLengthsData, partiaClass1);
                var partiaClass2 = new PartiaClass(tree2, Orders[tree2[0][0].Opers[0].OrderLink.OrderID].Deadline, false, 0, Guid.NewGuid(), -1);
                PartiasGeneratorEngine.PrepareNaladLengths(partiaClass2, NaladLengthsData); // Подготавливаем налад длины
                partiaClass2.Sort();
                TwoPartiasPartia2View.SetThisTree(NaladLengthsData, partiaClass2);



                PartiasConnectorEngineResult res = new PartiasConnectorEngineResult(0, 0, 0, 0);
                var areDiff = tree1[0][0].Opers[0].OrderLink.OrderID != tree2[0][0].Opers[0].OrderLink.OrderID;
                var res0 = -1;
                if (areDiff)
                {
                    res0 = PartiasConnectorEngine.Connect(new PartiaConnectorEngineData(partiaClass1, partiaClass2, withUI, false, NaladLengthsData, RestrictionsVia74110)).Total;
                }

                if (InGPCheckBox.Checked)
                {
                    res = PartiasConnectorEngine.Connect(new PartiaConnectorEngineData(partiaClass1, partiaClass1, withUI, true, NaladLengthsData, RestrictionsVia74110));
                    if (areDiff && res0 == 0 && partiaClass2.OpersHere[0].Count > 0)
                    {
                        res = PartiasConnectorEngine.Connect(new PartiaConnectorEngineData(partiaClass2, partiaClass2, withUI, true, NaladLengthsData, RestrictionsVia74110));
                    }
                }

                if (areDiff)
                {
                    TwoPartiasPartiaResView.SetThisTree(NaladLengthsData, partiaClass1, partiaClass2);
                }
                else
                {
                    TwoPartiasPartiaResView.SetThisTree(NaladLengthsData, partiaClass1);
                }

                if (partiaClass1 != null)
                {
                    partiasAlgorithm.SavedPartias.Add(partiaClass1.MyGuid, PartiasConnectorEngine.SeeSdvigsAsList(partiaClass1.OpersHere).SelectMany(x => x.Opers.Select(y => y.Index)).ToHashSet());
                    partiasAlgorithm.SavedPartiasTrees.Add(partiaClass1.MyGuid, (partiaClass1.OpersHere, partiaClass1.GPsCountNow, partiaClass1.LastK, partiaClass1.MaxThereCountForLongestOpers));
                    //if (areDiff && partiaClass2.OpersHere)
                    //{
                    //    partiasAlgorithm.SavedPartias.Add(partiaClass2.MyGuid, PartiasConnectorEngine.SeeSdvigsAsList(partiaClass2.OpersHere).SelectMany(x => x.Opers.Select(y => y.Index)).ToHashSet());
                    //    partiasAlgorithm.SavedPartiasTrees.Add(partiaClass2.MyGuid, (partiaClass2.OpersHere, partiaClass2.GPsCountNow, partiaClass2.LastK));
                    //}
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

        #endregion

        #region ПЛАНИРОВАНИЕ
        Task planningAlgorithm;
        private void LaunchPlanningButton_Click(object sender, EventArgs e)
        {
            if (sessionLink.TryCreateNewPlanningAlgorithmInstance(out var algorithm))
            {
                LaunchPlanningButton.Enabled = false;
                StopPlanningButton.Enabled = true;
                planningAlgorithm = Task.Run(() => PlanningAlgorithm(algorithm));
            }
        }

        private void PlanningAlgorithm_CurrentStageChanged(PlanningAlgorithmStage arg1, int arg2)
        {
            PlanningProcessLabel.Invoke(delegate
            {
                if (arg1 == PlanningAlgorithmStage.RunningV)
                {
                    //OutputButton.Enabled = true;
                }
                if (arg1 == PlanningAlgorithmStage.FailedV)
                {
                    BarDescTextBox.Text = $"Шаг 1 не дошёл, осталось {arg2}.";
                }
                PlanningProcessLabel.Text = arg1 switch
                {
                    PlanningAlgorithmStage.None => "...",
                    PlanningAlgorithmStage.Preparing => "1. Подготовка",
                    PlanningAlgorithmStage.Prepared => "Данные подготовлены",
                    PlanningAlgorithmStage.RunningV => "Шаг 1...",
                    PlanningAlgorithmStage.RunningT => "Шаг 2...",
                    PlanningAlgorithmStage.RunningG => "Шаг 3...",
                    PlanningAlgorithmStage.FailedV => "V завершен с ошибкой",
                    PlanningAlgorithmStage.Complete => $"Завершено. ({sessionLink.AssignedPlanningAlgorithm.LastDate})",
                    PlanningAlgorithmStage.Failed => "Ошибка."
                };
                PlanningProcessBar.Maximum = arg2;
            });
        }

        void PlanningAlgorithm(PlanningAlgorithm algorithm)
        {
            algorithm.CurrentStageChanged += PlanningAlgorithm_CurrentStageChanged;
            algorithm.AlgorithmCallback += Algorithm_AlgorithmCallback;
            algorithm.OnFirstResultAcquried += onAcquired;
            algorithm.OnNewResult += onNewRes;
            hasNewRes = false;

            int N = 0;

            if (algorithm.Start())
            {
                while (!algorithm.Process.IsCompleted)
                {
                    PlanningProcessBar.BeginInvoke(delegate
                    {
                        PlanningProcessBar.Value = N;
                        if (algorithm.CurrentStage == PlanningAlgorithmStage.RunningV)
                        {
                            PlanningProcessLabel.Text = $"Шаг 1: {N} / {PlanningProcessBar.Maximum}";
                        }

                        if (algorithm.CurrentStage == PlanningAlgorithmStage.RunningT)
                        {
                            PlanningProcessLabel.Text = $"Шаг 2: {algorithm.UsedWorkflow.Generation} | лучшая: {algorithm.LastGen} ({algorithm.LastDate})";
                        }

                        if (algorithm.CurrentStage == PlanningAlgorithmStage.RunningG)
                        {
                            PlanningProcessLabel.Text = $"Шаг 3: {algorithm.UsedWorkflow.Generation} - критерий {algorithm.UsedWorkflow.currentMode} [ {algorithm.UsedWorkflow.currDynCounter} / {algorithm.UsedWorkflow.maxDynCounter} ] | лучшая: {algorithm.LastGen} ({algorithm.LastDate})";
                        }

                        updateUI();

                    });
                    Thread.Sleep(250);
                }

                PlanningProcessBar.Invoke(updateUI);
            }

            void updateUI()
            {

                if (hasNewRes)
                {
                    hasNewRes = false;
                    PlanningDataTable.Rows[0].Cells[2].Value = resToShow.DaysRuinedValue;
                    PlanningDataTable.Rows[1].Cells[2].Value = resToShow.RechangesSeconds;
                    PlanningDataTable.Rows[2].Cells[2].Value = resToShow.ProstoiSeconds;
                    PlanningDataTable.Rows[3].Cells[2].Value = resToShow.ProductionSeconds;
                    PlanningDataTable.Rows[4].Cells[2].Value = resToShow.TaraRedutantUsage;
                    PlanningDataTable.Rows[5].Cells[2].Value = resToShow.TaraCriticalUsage;
                    PlanningDataTable.Rows[6].Cells[2].Value = resToShow.BadOffsets;
                    PlanningDataTable.Rows[7].Cells[2].Value = resToShow.NaladLengthsTotal;
                    PlanningDataTable.Rows[8].Cells[2].Value = resToShow.GetCatchedNaladLengthsErrorsCount(algorithm.IgnoredGlobalFixedNaladLengthsErrors);

                    OrdersCurve.AddPoint(algorithm.LastGen, resToShow.DaysRuinedValue);
                    OrdersGraphControl.AxisChange();
                    OrdersGraphControl.Invalidate();

                    RechangesCurve.AddPoint(algorithm.LastGen, resToShow.RechangesSeconds.TotalHours);
                    RechangesGraphControl.AxisChange();
                    RechangesGraphControl.Invalidate();

                    ProstoiCurve.AddPoint(algorithm.LastGen, resToShow.ProstoiSeconds.TotalHours);
                    ProstoiGraphControl.AxisChange();
                    ProstoiGraphControl.Invalidate();

                    TotalTimeCurve.AddPoint(algorithm.LastGen, resToShow.ProductionSeconds.TotalDays);
                    TotalTimeGraphControl.AxisChange();
                    TotalTimeGraphControl.Invalidate();

                    TaraRedutantUsageCurve.AddPoint(algorithm.LastGen, resToShow.TaraRedutantUsage);
                    TaraRedutantUsageGraphControl.AxisChange();
                    TaraRedutantUsageGraphControl.Invalidate();

                    var v0 = Math.Round(100.0 - (resToShow.DaysRuinedValue / algorithm.OriginalValue.DaysRuinedValue * 100f), 2);
                    var v1 = Math.Round(100.0 - (resToShow.RechangesSeconds / algorithm.OriginalValue.RechangesSeconds * 100f), 2);
                    var v2 = Math.Round(100.0 - (resToShow.ProstoiSeconds / algorithm.OriginalValue.ProstoiSeconds * 100f), 2);
                    var v3 = Math.Round(100.0 - (resToShow.ProductionSeconds / algorithm.OriginalValue.ProductionSeconds * 100f), 2);
                    var v4 = Math.Round(100.0 - (resToShow.TaraRedutantUsage / algorithm.OriginalValue.TaraRedutantUsage * 100f), 2);
                    var v5 = Math.Round(100.0 - (resToShow.TaraCriticalUsage / algorithm.OriginalValue.TaraCriticalUsage * 100f), 2);
                    var v6 = Math.Round(100.0 - (resToShow.BadOffsets / algorithm.OriginalValue.BadOffsets * 100f), 2);
                    var v7 = Math.Round(100.0 - (resToShow.NaladLengthsTotal / algorithm.OriginalValue.NaladLengthsTotal * 100f), 2);
                    var v8 = Math.Round(100.0 - (resToShow.GetCatchedNaladLengthsErrorsCount(algorithm.IgnoredGlobalFixedNaladLengthsErrors) / (float)algorithm.OriginalValue.GetCatchedNaladLengthsErrorsCount(algorithm.IgnoredGlobalFixedNaladLengthsErrors) * 100f), 2);

                    OrdersCurveOnBig.AddPoint(algorithm.LastGen, v0);
                    RechangesCurveOnBig.AddPoint(algorithm.LastGen, v1);
                    ProstoiCurveOnBig.AddPoint(algorithm.LastGen, v2);
                    TotalTimeCurveOnBig.AddPoint(algorithm.LastGen, v3);
                    TaraRedutantUsageCurveOnBig.AddPoint(algorithm.LastGen, v4);

                    AllPercentagesGraphControl.AxisChange();
                    AllPercentagesGraphControl.Invalidate();

                    PlanningDataTable.Rows[0].Cells[3].Value = v0 + "%";
                    PlanningDataTable.Rows[1].Cells[3].Value = v1 + "%";
                    PlanningDataTable.Rows[2].Cells[3].Value = v2 + "%";
                    PlanningDataTable.Rows[3].Cells[3].Value = v3 + "%";
                    PlanningDataTable.Rows[4].Cells[3].Value = v4 + "%";
                    PlanningDataTable.Rows[5].Cells[3].Value = v5 + "%";
                    PlanningDataTable.Rows[6].Cells[3].Value = v6 + "%";
                    PlanningDataTable.Rows[7].Cells[3].Value = v7 + "%";
                    PlanningDataTable.Rows[8].Cells[3].Value = v8 + "%";
                    //if (!IsSilent)
                    //{

                    //    GraphsController.AddNewPointToGraphs(workflow.Generation, t.DaysRuinedValue, t.ProstoiSeconds.TotalSeconds, p1, p2, p3, t, ops);
                    //}
                    //UpdateUIThere(workflow);
                }
            }

            void Algorithm_AlgorithmCallback(PlanningAlgorithmStage arg1, int arg2) => N = arg2;

            algorithm.OnNewResult -= onNewRes;
            algorithm.CurrentStageChanged -= PlanningAlgorithm_CurrentStageChanged;
            algorithm.AlgorithmCallback -= Algorithm_AlgorithmCallback;
            algorithm.OnFirstResultAcquried -= onAcquired;

            LaunchPlanningButton.Invoke(delegate
            {
                StopPlanningButton.Enabled = false;
                //OutputButton.Enabled = false;
                LaunchPlanningButton.Enabled = true;
                if (algorithm.CurrentStage == PlanningAlgorithmStage.Complete)
                {
                    PlanningProcessBar.Value = PlanningProcessBar.Maximum;
                }
            });
        }

        bool hasNewRes = false;
        PlanSimulatorEngineResult resToShow;
        void onNewRes(PlanSimulatorEngineResult newRes)
        {
            hasNewRes = true;
            resToShow = newRes;
        }

        void onAcquired(PlanSimulatorEngineResult res, List<PlanSimulatorBlock> blocks, int poolIndex)
        {
            ganttOLD.Invoke(delegate
            {
                OutputToGanttChart(ganttOLD, blocks, poolIndex, sessionLink.SettingsController.CurrentSettings.PoolSize - 1, res);
                PlanningDataTable.Rows[0].Cells[1].Value = res.DaysRuinedValue;
                PlanningDataTable.Rows[1].Cells[1].Value = res.RechangesSeconds;
                PlanningDataTable.Rows[2].Cells[1].Value = res.ProstoiSeconds;
                PlanningDataTable.Rows[3].Cells[1].Value = res.ProductionSeconds;
                PlanningDataTable.Rows[4].Cells[1].Value = res.TaraRedutantUsage;
                PlanningDataTable.Rows[5].Cells[1].Value = res.TaraCriticalUsage;
                PlanningDataTable.Rows[6].Cells[1].Value = res.BadOffsets;
                PlanningDataTable.Rows[7].Cells[1].Value = res.NaladLengthsTotal;
                PlanningDataTable.Rows[8].Cells[1].Value = res.CatchedNaladLengthsErrorsCount;

                PrepareGraphsForPlanning(sessionLink.AssignedPlanningAlgorithm.MinAcceptableDate);

                if (OriginalProductionCurve != null)
                {
                    GraphsOutput.GrowingOutput(OriginalProductionCurve, res.FirstStagesStartDate);
                    ProductionGraphControl.AxisChange();
                    ProductionGraphControl.Invalidate();
                }

                tarasList(res, blocks);

                matsList(res, blocks);
                //PlanningDataTable.Rows[5].Cells[1].Value = res.DaysRuinedValue;
                //PlanningDataTable.Rows[6].Cells[1].Value = res.;
            });
        }

        Font smallFont = new Font("Montserrat", 7f, FontStyle.Bold, GraphicsUnit.Point);

        public void OutputToGanttChart(GanttChart diagram, List<PlanSimulatorBlock> blocks, int PoolIndex, int PoolIndexToSave, PlanSimulatorEngineResult dataForHelp)
        {

            diagram.ClearBars();
            diagram.SavedOpersLink = null;

            // Это временный словарь чтобы узнать номер линии центра
            Dictionary<Uco, int> centerIdToLineN = new();
            List<GanttChart.RowTitle> textRows = new();

            Dictionary<Uco, int> GlobalToLocalCenterRows = new();

            var currRow = 0;

            var CentersData = sessionLink.MasterDataLinks.CentersMasterData.CentersData;
            var ALL = sessionLink.MasterDataLinks.StagesMasterData.AllStages;
            var NonWorkingIntervalsMasterData = sessionLink.MasterDataLinks.WorkingIntervalsMasterData;
            var CentersFixingTime = NonWorkingIntervalsMasterData.CentersFixingTime;
            var ExistingCriterias = sessionLink.MasterDataLinks.RechangesMasterData.ExistingCriterias;
            var MODELLING = sessionLink.MasterDataLinks.StagesMasterData.MesModellingsInfo;
            var Custom_MODELLING = sessionLink.MasterDataLinks.StagesMasterData.ApsModellingsInfo;
            var MIN_DATE_TIME_AFTER_FIX = sessionLink.AssignedPlanningAlgorithm.MinAcceptableDate;
            var prevTimeDelta = sessionLink.MasterDataLinks.StagesMasterData.PrevTimeDelta;
            var pool = blocks[0].PoolLink;

            foreach (var el in pool.CentersData/*int i = 0; i < opers.CentersData.Count; i++*/)
            {
                var id = el.Key;
                //var idS = Centers.CentersIntToStr[id];
                centerIdToLineN.Add(id, currRow);
                if (true/*!CentersData[id].IsHiddenOnDiagrams*//* && !CentersData[idS].WillRemoveEverything*/)
                {
                    textRows.Add(new GanttChart.RowTitle(new GanttChart.TextInfo[]
                    {
                            new GanttChart.TextInfo($"[{CentersData[id].RowIndex}] {id}", new Point(12, 0)),
                            new GanttChart.TextInfo($"R: {el.Value[PoolIndex].RechangesTime}", new Point(2, 17), smallFont, Color.Red),
                            new GanttChart.TextInfo("", new Point(125, 17), smallFont, Color.Red),
                            new GanttChart.TextInfo($"P: {el.Value[PoolIndex].ProstoiTime}", new Point(2, 29), smallFont, Color.DarkViolet),
                            new GanttChart.TextInfo("", new Point(125, 29), smallFont, Color.DarkViolet),
                            new GanttChart.TextInfo($"T: {el.Value[PoolIndex].TotalTime}", new Point(2, 42), smallFont, Color.RoyalBlue),
                            new GanttChart.TextInfo("", new Point(125, 42), smallFont, Color.RoyalBlue)
                        //new GanttChart.TextInfo((gg+1).ToString(), new Point(2, 0), smallFont, Color.Black)
                    }, id));
                    GlobalToLocalCenterRows.Add(id, currRow);
                    currRow++;
                }
            }


            for (int i = 0; i < pool.Blocks.Count; i++)
            {
                var block = pool.Blocks[i];
                (var blockRC, _, _) = block.GetCenterData(PoolIndex);

                var groups = block.Lists.GroupBy(x => dataForHelp.FirstStagesStartDate[x.FirstOperJust.Index]).ToArray(); // Сначала отсортируем по дате. Если она одинаковая, то будет одна группа, где параллельные моделирования.

                foreach (var group in groups)
                {
                    var parallelCount = group.Count(); // Сколько их тут

                    var sortedMods = group.OrderBy(x => x.Ranks[PoolIndex]).ToArray(); // Получив их ранги, мы можем точно понять, какое моделирование было первым, и так далее. Перед каждым моделированием, мы можем посмотреть, были ли перед ним простои и переналадки, взяв у его первой операции айдишник, а ещё дату начала.

                    for (int mod = 0; mod < sortedMods.Length; mod++)
                    {
                        var currMod = sortedMods[mod];
                        var nextOper = currMod.FirstOperJust;
                        var firstID = nextOper.Index;
                        (PlanSimulatorDatesData data, CachedInterval interval) prostoi;
                        (RechangesEngineResult data, CachedInterval interval, int last) rechanges;
                        if (!dataForHelp.ProstoiBeforeFirstStages.TryGetValue(firstID, out prostoi)) prostoi = (null, CachedInterval.Empty);
                        if (!dataForHelp.RechangesBeforeFirstStages.TryGetValue(firstID, out rechanges)) rechanges = (null, CachedInterval.Empty, -1);

                        //if (firstID == 833)
                        //{
                        //    ;
                        //}
                        var centerGlobalData = CentersData[blockRC];

                        var dateTime = dataForHelp.FirstStagesStartDate[firstID];

                        if (prostoi.interval.Duration.Ticks > 0)
                        {
                            var start = dateTime - rechanges.interval.Duration - prostoi.interval.Duration;
                            //var name = CentersIntToStr[ChoosenCenter];
                            var bar = new GanttChart.Bar(start, start + prostoi.interval.Duration, Color.FromArgb(255, 243, 188, 255)/*Color.LightGray*/, Color.DimGray,//prostInd == -1 ? Color.MediumOrchid : Color.LightGray, prostInd == -1 ? Color.DarkViolet : Color.Silver,//, ,
                                "Простой." +
                                //$"\r\nСмысл: {Desc}" +
                                (prostoi.data != null && prostoi.data.LastID != -1 ? $"\r\nНазначила: {ALL[prostoi.data.LastID]} с РЦ '{pool.Blocks[prostoi.data.LastBlockIndex].GetCenterData(PoolIndex).Center}'" : "") +
                                $"\r\nСледующая: {ALL[nextOper.Index]}", // ну типа она же именно что следующая, поэтому берем эту переменную и вставляем сюда.
                                GlobalToLocalCenterRows[blockRC],
                                0, 1,
                                -2, 0);
                            bar.fixingBars = CentersFixingTime == null ? null : prostoi.interval.CatchedIntervals?.Select(x => (CentersFixingTime[blockRC][x].When, CentersFixingTime[blockRC][x].When.Add(CentersFixingTime[blockRC][x].Time))).ToList();
                            diagram.AddBar(bar);
                        }

                        if (rechanges.data != null && rechanges.last != -1)
                        {
                            var start = dateTime - rechanges.interval.Duration;

                            var reches = "";
                            var op1 = ALL[rechanges.last];
                            var op2 = ALL[firstID];
                            void add(string sss, int nnn)
                            {
                                reches += $"\r\n{sss}: {op1.RechangeParameters[nnn].ToString()} => {op2.RechangeParameters[nnn].ToString()}";
                            }

                            for (int ind = 0; ind < ExistingCriterias.Length; ind++)
                            {
                                add(ExistingCriterias[ind].Name, ind);
                            }

                            reches += $"\r\nОперация: {op1.OperationVid} => {op2.OperationVid}";
                            var results = rechanges.data.ToLinearDictionary();
                            var bar = new GanttChart.Bar(start, start.Add(rechanges.interval.Duration),
                    Colors.MOSITLAB_RED, Colors.MOSITLAB_RED_DARK,
                                $"Переналадка: {(results != null ? string.Join(", ", results.Select(x => x.Value.Name + " (" + x.Value.Duration + " с.)")) : "")}" +
                                $"\r\nИндекс (ИД): {op1.Index} ({op1.GUID}) => {op2.Index} ({op2.GUID})" +
                                reches, GlobalToLocalCenterRows[blockRC],
                                0, 1,
                                -1, 0);
                            bar.fixingBars = CentersFixingTime == null ? null : rechanges.interval.CatchedIntervals?.Select(x => (CentersFixingTime[blockRC][x].When, CentersFixingTime[blockRC][x].When.Add(CentersFixingTime[blockRC][x].Time))).ToList();
                            //bar.fixingBars = e.CatchedIntervals.Select(x => (CentersFixingTime[e.UsedRCCode][x].When, CentersFixingTime[e.UsedRCCode][x].When.Add(CentersFixingTime[e.UsedRCCode][x].Time))).ToList();
                            diagram.AddBar(bar);
                            //Diagram.DrawChart();
                        }

                        TimeSpan offset = new TimeSpan();
                        int currentRank = currMod.Ranks[PoolIndex];//dataForHelp.FirstStagesRanks[firstID];
                        var intervalForOp = dataForHelp.FirstStagesFullDurations[firstID];


                        for (int op = 0; op < currMod.StagesAsNormal.Count; op++)
                        {
                            var operLoc = currMod.StagesAsNormal[op];
                            var operLoc2 = currMod.Stages[op];
                            var duration = TimeSpan.FromSeconds(operLoc.GetDuration(blockRC)) * currMod.TimeK[blockRC];
                            var start = dateTime + offset;

                            if (intervalForOp.CatchedIntervals != null && intervalForOp.CatchedIntervals.Length > 0)
                                duration = NonWorkingIntervalsMasterData.GetDurationWithNonWorkingIntervals(blockRC, start, (int)duration.TotalSeconds, intervalForOp.CatchedIntervals.First()).Duration;

                            //if (intervalForOp.CatchedIntervals != null)
                            //    foreach (var lunch in intervalForOp.CatchedIntervals)
                            //{
                            //    var interval = NonWorkingIntervalsMasterData.CentersFixingTime[blockRC][lunch];

                            //    if (start.Add(duration) <= (interval.When + interval.Time) // Конец операции дальше конца этого интервала?
                            //        && start <= interval.When // И при этом начало этой операции должно стоять ДО него
                            //        ) duration += interval.Time;
                            //}

                            var orderlink = operLoc.OrderLink;
                            var ordern = orderlink.OrderID;
                            // Вывод на диаграмму Ганта
                            var bar = new GanttChart.Bar(start, start + duration,
                            (operLoc.IsLast ? Color.Yellow : (parallelCount > 1 ? Color.DarkGreen : Colors.MOSITLAB_BLUE)), Colors.MOSITLAB_BLUE_DARK,
                            $"Операция {operLoc.Index} ({operLoc.GUID}) (вид: {operLoc.OperationVid})" +
                            ((operLoc.Center == blockRC) ? "" : $"\r\nСтандартный РЦ: {operLoc.Center}") +
                            $"\r\nРанг: {currentRank}" +
                            $"\r\nИндекс блока: {currMod.BlockLink.BlockIndex[PoolIndex]}" +
                            (operLoc.PrevDateTime.HasValue ? $"\r\nПред. время: {operLoc.PrevDateTime.Value} + {(MIN_DATE_TIME_AFTER_FIX - prevTimeDelta.Value)} = {operLoc.PrevDateTime.Value.Add(MIN_DATE_TIME_AFTER_FIX - prevTimeDelta.Value)}" : "") +
                            $"\r\nGUID партии: {operLoc.GetOrderOrPartiaCode()}" +
                            $"\r\nНоменклатура: {operLoc.Nomenklature}" +
                            //(centerGlobalData.ParallelModelling ? $"\r\n*Сокращённая" : "") +
                            $"\r\nПродукция: {operLoc.Production}" +
                            $"\r\nНаладочная длина: {operLoc2.NaladLengthData[PoolIndex].Data.NaladLengthHere.Value}" +
                            $"\r\nРегистратор: {operLoc.Registrator}" +
                            $"\r\nКоличество: {operLoc.Count}" +
                            $"\r\nПовив: {operLoc.Poviv}" +
                            "\r\n" +
                            string.Join("\r\n", ExistingCriterias.Select((x, index) =>
                            $"{x.Name}: {operLoc.RechangeParameters[index].ToString()}"
                            )) +
                            //$"\r\nНапряжение: {operLoc.RechangesSTR[(int)RechangeCriteria.Voltage]} ({operLoc.Rechanges[(int)RechangeCriteria.Voltage]})" +
                            //$"\r\nКоличество мест: {operLoc.OECount} [st {operLoc.OEStaticCount}, ls {operLoc.OELastCount}, cl {operLoc.OECalculatedCount}, rh {operLoc.Rechanges[(byte)RechangeCriteria.OECount]}]" +
                            //$"\r\nСечение: {operLoc.RechangesSTR[(int)RechangeCriteria.Razm1Sech]}" +
                            //$"\r\nДиаметр: {operLoc.RechangesSTR[(int)RechangeCriteria.Diam]}" +
                            //$"\r\nТпж: {operLoc.RechangesSTR[(int)RechangeCriteria.TPZH]}" +
                            $"\r\nID пар. планирования: {(string.Join("; ", operLoc.ParallelPlanningIDs.Select(x => $"{x.Key} = {x.Value}")))}" +
                            $"\r\nЕд. изм.:: {operLoc.EdIzm}" +
                            $"\r\nМетод расчёта: {operLoc.UsedCalcMethod}" +
                            //$"\r\nЛиния: {e.CenterRow + 1}" +
                            (operLoc.MesModelling.Code == Guid.Empty ? "" : $"\r\nМоделирование: {operLoc.MesModelling} / custom: {(operLoc.IsCustomModelling ? operLoc.CustomModelling : "")}") +
                            $"\r\nТара: {operLoc.CurrentTara}" +
                            (ordern.Object.Label.Length == 0 ? "" : $"\r\nЗаказ: {ordern.Object}\r\nПартия ГП: {ordern.Version}\r\nДедлайн: {orderlink.Deadline:G}\r\nПриоритет: {orderlink.Priority}") +
                            //(opers_list.CustomModelling == -1 ? "" : $"\r\nКастомное моделирование: {opers_list.CustomModelling}") +
                            (operLoc.StartTimeFact.HasValue ? $"\r\nДата начала (ФАКТ.): {operLoc.StartTimeFact.Value}" : "") +
                            (operLoc.Fixed ? $"\r\nЗАФИКСИРОВАНА" : "") +
                            (operLoc.VPZ ? $"\r\nВПЗ" : "") +
                            (operLoc.Prevs.Count == 0 ? "" : $"\r\nЗависима от: {string.Join(", ", operLoc.Prevs.Select(x => $"{ALL[x].Index} / {ALL[x].GUID}"))}") +
                            (operLoc.Nexts.Count == 0 ? "" : $"\r\nВлияет на: {string.Join(", ", operLoc.Nexts.Select(x => $"{ALL[x].Index} / {ALL[x].GUID}"))}"),
                            GlobalToLocalCenterRows[blockRC],
                            parallelCount > 1 ? (1f / parallelCount) * mod : 0,
                            parallelCount > 1 ? (1f / parallelCount) : 1, // TODO
                            operLoc.Index,
                            operLoc2.NaladLengthData[PoolIndex].Data.NaladLengthHere.Value,
                            operLoc.Fixed ? Color.Maroon : (operLoc.IsFact ? Color.Gray : (operLoc.VPZ ? Color.RoyalBlue : null)),
                            operLoc.Center == blockRC ? null : Color.Orange,
                            operLoc.Prevs.Count == 0 && operLoc.Nexts.Count == 0 ? Color.Lime : (operLoc.CurrentTara.IsEmpty && !operLoc.IsVspom && !operLoc.NoCheckNamotka ? Color.Brown : (operLoc.ParallelPlanningIDs.All(x => x.Value == -1) && centerGlobalData.CanBeParallel ? Color.OrangeRed : null)),
                            operLoc.IsCustomModelling ? Custom_MODELLING[operLoc.CustomModelling].Color : (operLoc.MesModelling.Code == Guid.Empty ? (Color?)null : MODELLING[operLoc.MesModelling.Code].Color),// || opers_list.CustomModelling > -1 // opersHere.Count == 1 ||
                            operLoc.Color
                            //, opers_list.CustomModellingColor
                            );
                            bar.fixingBars = CentersFixingTime == null ? null : intervalForOp.CatchedIntervals?.Select(x => (CentersFixingTime[blockRC][x].When, CentersFixingTime[blockRC][x].When.Add(CentersFixingTime[blockRC][x].Time))).ToList();
                            diagram.AddBar(bar, operLoc.Index);




                            currentRank++;
                            offset = offset.Add(duration);
                        }

                        if (Math.Abs((offset - intervalForOp.Duration).TotalSeconds) > 1)
                        {
                            ;
                        }
                    }
                }
            }


            diagram.CurrentHoveredBar = -1;
            diagram.assignedDataPack = pool.Blocks.SelectMany(x => x.Lists.SelectMany(z => z.StagesAsNormal.Select(y => (y.Index, y.Prevs, y.Nexts, x.GetCenterData(PoolIndex).Center, y.PossibleCenters)))).ToDictionary(x => x.Index, x => (x.Prevs, x.Nexts, x.Center, x.PossibleCenters));
            diagram.SavedOpersLink = PlanSimulatorPool.CopyBlocksSwallow(pool.Blocks);
            PlanSimulatorPool.MoveCentersInAllBlocks(diagram.SavedOpersLink, PoolIndex, PoolIndexToSave);
            diagram.PoolIndex = PoolIndexToSave;
            diagram.SetRowTitles(textRows);
            diagram.FindAllPrevsHere(-1, false); // все нахрен стираем
        }

        private void ModsColorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ganttOLD.UpdateColorsInfoAndPaint(StagesColorCheckBox.Checked, ModsColorCheckBox.Checked, true, true, true);
            ganttNEW.UpdateColorsInfoAndPaint(StagesColorCheckBox.Checked, ModsColorCheckBox.Checked, true, true, true);
            ganttANL.UpdateColorsInfoAndPaint(StagesColorCheckBox.Checked, ModsColorCheckBox.Checked, true, true, true);
        }

        private void StagesColorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ganttOLD.UpdateColorsInfoAndPaint(StagesColorCheckBox.Checked, ModsColorCheckBox.Checked, true, true, true);
            ganttNEW.UpdateColorsInfoAndPaint(StagesColorCheckBox.Checked, ModsColorCheckBox.Checked, true, true, true);
            ganttANL.UpdateColorsInfoAndPaint(StagesColorCheckBox.Checked, ModsColorCheckBox.Checked, true, true, true);
        }

        private void OutputButton_Click(object sender, EventArgs e)
        {
            var algorithm = sessionLink.AssignedPlanningAlgorithm;
            if (algorithm == null) return;
            lock (algorithm.LockerObject)
            {
                var outData = sessionLink.SessionOutputEngine.Output(ForcedToRecreate: false);
                ganttNEW.Invoke(delegate
                {
                    OutputToGanttChart(ganttNEW, outData.copy, outData.PoolIndex, sessionLink.SettingsController.CurrentSettings.PoolSize - 2, outData.result);

                    if (NewProductionCurve != null && resToShow != null)
                    {
                        GraphsOutput.GrowingOutput(NewProductionCurve, outData.result.FirstStagesStartDate);
                        ProductionGraphControl.AxisChange();
                        ProductionGraphControl.Invalidate();
                    }


                    tarasList(outData.result, outData.copy);

                    matsList(outData.result, outData.copy);
                });
            }
        }

        void matsList(PlanSimulatorEngineResult result, List<PlanSimulatorBlock> Blocks)
        {
            var matsHere = GraphsOutput.UsageOutput<string>(MaterialsGraphControl.GraphPane, result.FirstStagesStartDate, result.FirstStagesFullDurations,
                Blocks.SelectMany(block =>
                block.Lists.Select(list =>
                (list.FirstOperJust.Index, (list.FirstOperJust.StageMaterialsInfo == null ? Array.Empty<string>() : list.FirstOperJust.StageMaterialsInfo.UsedMaterials.Select(x => x.Nomenklature.Label).ToArray(), list.Count))
                )).ToDictionary(x => x.Index, x => x.Item2), 10
                );
            MaterialsListOnGraphics.Items.Clear();
            MaterialsListOnGraphics.Items.AddRange(matsHere.Select(x => x.element ?? "пусто").ToArray());
            MaterialsGraphControl.AxisChange();
            MaterialsGraphControl.Invalidate();
        }

        void tarasList(PlanSimulatorEngineResult result, List<PlanSimulatorBlock> Blocks)
        {
            var tarasHere = GraphsOutput.UsageOutput<string>(TarasGraphControl.GraphPane, result.FirstStagesStartDate, result.FirstStagesFullDurations,
                Blocks.SelectMany(block =>
                block.Lists.Select(list =>
                (id: list.FirstOperJust.Index, (list.FirstOperJust.CurrentTara.Label, 1.0))
                )).ToDictionary(x => x.id, x => x.Item2), 10
                );
            TaraListOnGraphics.Items.Clear();
            TaraListOnGraphics.Items.AddRange(tarasHere.Select(x => x.element ?? "пусто").ToArray());
            TarasGraphControl.AxisChange();
            TarasGraphControl.Invalidate();
        }

        private void StopPlanningButton_Click(object sender, EventArgs e)
        {
            var algorithm = sessionLink.AssignedPlanningAlgorithm;
            if (algorithm == null) return;
            algorithm.Stop();
        }

        private void CreatePlanningTable()
        {
            CreatePlanningTableConcrete(PlanningDataTable);
            CreatePlanningTableConcrete(ManualPlanningDataTable);
        }

        private void CreatePlanningTableConcrete(DataGridView PlanningDataTable)
        {
            for (int i = 0; i < 9; i++)
            {
                PlanningDataTable.Rows.Add();
            }

            var row1 = PlanningDataTable.Rows[0];
            row1.DefaultCellStyle.ForeColor = Color.RoyalBlue;
            row1.DefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
            row1.Cells[0].Value = "Срывы сроков";

            var row2 = PlanningDataTable.Rows[1];
            row2.DefaultCellStyle.ForeColor = Color.Red;
            row2.DefaultCellStyle.Font = new Font(row1.DefaultCellStyle.Font, FontStyle.Bold);
            row2.Cells[0].Value = "Переналадки";

            var row3 = PlanningDataTable.Rows[2];
            row3.DefaultCellStyle.ForeColor = Color.Violet;
            row3.DefaultCellStyle.Font = new Font(row1.DefaultCellStyle.Font, FontStyle.Bold);
            row3.Cells[0].Value = "Простои";

            var row4 = PlanningDataTable.Rows[3];
            row4.DefaultCellStyle.ForeColor = Color.DarkGreen;
            row4.DefaultCellStyle.Font = new Font(row1.DefaultCellStyle.Font, FontStyle.Bold);
            row4.Cells[0].Value = "Длительность плана";

            var row5 = PlanningDataTable.Rows[4];
            row5.DefaultCellStyle.ForeColor = Color.DarkOrange;
            row5.DefaultCellStyle.Font = new Font(row1.DefaultCellStyle.Font, FontStyle.Bold);
            row5.Cells[0].Value = "Размотка тары";

            var row6 = PlanningDataTable.Rows[5];
            row6.DefaultCellStyle.ForeColor = Color.LightCoral;
            row6.DefaultCellStyle.Font = new Font(row1.DefaultCellStyle.Font, FontStyle.Bold);
            row6.Cells[0].Value = "Пролёживаемость тары";

            var row7 = PlanningDataTable.Rows[6];
            row7.DefaultCellStyle.ForeColor = Color.DarkBlue;
            row7.DefaultCellStyle.Font = new Font(row1.DefaultCellStyle.Font, FontStyle.Bold);
            row7.Cells[0].Value = "Недоступность материалов";

            var row8 = PlanningDataTable.Rows[7];
            row8.DefaultCellStyle.ForeColor = Color.Orange;
            row8.DefaultCellStyle.Font = new Font(row1.DefaultCellStyle.Font, FontStyle.Bold);
            row8.Cells[0].Value = "Суммарные наладочные длины";

            var row9 = PlanningDataTable.Rows[8];
            row9.DefaultCellStyle.ForeColor = Color.DarkOrange;
            row9.DefaultCellStyle.Font = new Font(row1.DefaultCellStyle.Font, FontStyle.Bold);
            row9.Cells[0].Value = "Ошибок наладочных длин";
        }

        #endregion

        #region ГРАФИКИ

        private void TaraListOnGraphics_TextChanged(object sender, EventArgs e) => ChoseTara(TaraListOnGraphics.Text);

        private void MaterialsListOnGraphics_TextChanged(object sender, EventArgs e) => ChoseMat(MaterialsListOnGraphics.Text);

        // Поставить одну из тар
        void ChoseTara(string Tara)
        {
            var all = string.IsNullOrEmpty(Tara);
            foreach (var item in TarasGraphControl.GraphPane.CurveList)
            {
                item.IsVisible = all || item.Label.Text == Tara;
            }
            //This.zedGraphControl7.GraphPane.XAxis.Scale.Max = new XDate(RESULT_G);
            TarasGraphControl.AxisChange();
            TarasGraphControl.Invalidate();
        }

        // Поставить одну из матов
        void ChoseMat(string Mat)
        {
            var all = string.IsNullOrEmpty(Mat);
            foreach (var item in MaterialsGraphControl.GraphPane.CurveList)
            {
                item.IsVisible = all || item.Label.Text == Mat;
            }
            //This.zedGraphControl7.GraphPane.XAxis.Scale.Max = new XDate(RESULT_G);
            MaterialsGraphControl.AxisChange();
            MaterialsGraphControl.Invalidate();
        }

        private void PrepareGraphs()
        {
            zedGraphControl1.GraphPane.Title.Text = "Пустой график";
            zedGraphControl2.GraphPane.Title.Text = "Пустой график";
            zedGraphControl3.GraphPane.Title.Text = "Пустой график";

            var pane = OrdersGraphControl.GraphPane;
            pane.Title.Text = "Срывы сроков";
            pane.XAxis.Title.Text = "Итерация";
            pane.YAxis.Title.Text = "Сумма срывов";

            pane = ProductionGraphControl.GraphPane;
            pane.Title.Text = "Готовая продукция";
            pane.XAxis.Title.Text = "Дата";
            pane.YAxis.Title.Text = "Количество";
            pane.XAxis.Type = AxisType.Date;
            pane.XAxis.Scale.Format = "dd-MM-yyyy";// HH:mm:ss
            pane.XAxis.Scale.MajorUnit = DateUnit.Second;


            pane = TarasGraphControl.GraphPane;
            pane.Title.Text = "Загрузка тар";
            pane.XAxis.Title.Text = "Дата";
            pane.YAxis.Title.Text = "Количество";
            pane.XAxis.Type = AxisType.Date;
            pane.XAxis.Scale.Format = "dd-MM-yyyy";// HH:mm:ss
            pane.XAxis.Scale.MajorUnit = DateUnit.Second;

            pane = MaterialsGraphControl.GraphPane;
            pane.Title.Text = "Материалы";
            pane.XAxis.Title.Text = "Дата";
            pane.YAxis.Title.Text = "Количество";
            pane.XAxis.Type = AxisType.Date;
            pane.XAxis.Scale.Format = "dd-MM-yyyy";// HH:mm:ss
            pane.XAxis.Scale.MajorUnit = DateUnit.Second;

            pane = TotalTimeGraphControl.GraphPane;
            pane.Title.Text = "Длина графика";
            pane.XAxis.Title.Text = "Итерация";
            pane.YAxis.Title.Text = "Дней";

            pane = ProstoiGraphControl.GraphPane;
            pane.Title.Text = "Простои";
            pane.XAxis.Title.Text = "Итерация";
            pane.YAxis.Title.Text = "Время (ч.)";

            pane = RechangesGraphControl.GraphPane;
            pane.Title.Text = "Переналадки";
            pane.XAxis.Title.Text = "Итерация";
            pane.YAxis.Title.Text = "Время (ч.)";

            pane = TaraRedutantUsageGraphControl.GraphPane;
            pane.Title.Text = "Размотка тары";
            pane.XAxis.Title.Text = "Итерация";
            pane.YAxis.Title.Text = "Время (ч.)";

            pane = AllPercentagesGraphControl.GraphPane;
            pane.Title.Text = "Общий процент оптимизации";
            pane.XAxis.Title.Text = "Итерация";
            pane.YAxis.Title.Text = "%";
        }

        LineItem
            OriginalProductionCurve = null, // старая продукция
            NewProductionCurve = null; // новая продукция

        LineItem OrdersCurve, RechangesCurve, ProstoiCurve, TotalTimeCurve, TaraRedutantUsageCurve;
        LineItem OrdersCurveOnBig, RechangesCurveOnBig, ProstoiCurveOnBig, TotalTimeCurveOnBig, TaraRedutantUsageCurveOnBig;

        private void PrepareGraphsForPlanning(DateTime minDateTime)
        {
            var pane = ProductionGraphControl.GraphPane;
            pane.CurveList.Clear();
            var list = new PointPairList();
            OriginalProductionCurve = pane.AddCurve("Оригинал", list, Color.Red, SymbolType.None);
            pane.XAxis.Scale.Min = new XDate(minDateTime);
            pane.XAxis.Scale.Max = new XDate(minDateTime + sessionLink.MasterDataLinks.StagesMasterData.OneDay);
            list = new PointPairList();
            NewProductionCurve = pane.AddCurve("Лучшее", list, Color.Blue, SymbolType.None);


            pane = OrdersGraphControl.GraphPane;
            pane.CurveList.Clear();
            list = new PointPairList();
            OrdersCurve = pane.AddCurve("Просрочено", list, PlanningDataTable.Rows[0].DefaultCellStyle.ForeColor, SymbolType.None);

            pane = RechangesGraphControl.GraphPane;
            pane.CurveList.Clear();
            list = new PointPairList();
            RechangesCurve = pane.AddCurve("Переналадки", list, PlanningDataTable.Rows[1].DefaultCellStyle.ForeColor, SymbolType.None);

            pane = ProstoiGraphControl.GraphPane;
            pane.CurveList.Clear();
            list = new PointPairList();
            ProstoiCurve = pane.AddCurve("Простои", list, PlanningDataTable.Rows[2].DefaultCellStyle.ForeColor, SymbolType.None);

            pane = TotalTimeGraphControl.GraphPane;
            pane.CurveList.Clear();
            list = new PointPairList();
            TotalTimeCurve = pane.AddCurve("Общее время", list, PlanningDataTable.Rows[3].DefaultCellStyle.ForeColor, SymbolType.None);

            pane = TaraRedutantUsageGraphControl.GraphPane;
            pane.CurveList.Clear();
            list = new PointPairList();
            TaraRedutantUsageCurve = pane.AddCurve("Размотка тары", list, PlanningDataTable.Rows[4].DefaultCellStyle.ForeColor, SymbolType.None);

            #region Большой график
            pane = AllPercentagesGraphControl.GraphPane;
            pane.CurveList.Clear();
            list = new PointPairList();
            OrdersCurveOnBig = pane.AddCurve("Просрочено", list, PlanningDataTable.Rows[0].DefaultCellStyle.ForeColor, SymbolType.None);

            pane = AllPercentagesGraphControl.GraphPane;
            list = new PointPairList();
            RechangesCurveOnBig = pane.AddCurve("Переналадки", list, PlanningDataTable.Rows[1].DefaultCellStyle.ForeColor, SymbolType.None);

            pane = AllPercentagesGraphControl.GraphPane;
            list = new PointPairList();
            ProstoiCurveOnBig = pane.AddCurve("Простои", list, PlanningDataTable.Rows[2].DefaultCellStyle.ForeColor, SymbolType.None);

            pane = AllPercentagesGraphControl.GraphPane;
            list = new PointPairList();
            TotalTimeCurveOnBig = pane.AddCurve("Общее время", list, PlanningDataTable.Rows[3].DefaultCellStyle.ForeColor, SymbolType.None);

            pane = AllPercentagesGraphControl.GraphPane;
            list = new PointPairList();
            TaraRedutantUsageCurveOnBig = pane.AddCurve("Размотка тары", list, PlanningDataTable.Rows[4].DefaultCellStyle.ForeColor, SymbolType.None);
            #endregion
        }

        #endregion

        #region РУЧНОЙ АНАЛИЗ ГРАФИКА

        PlanSimulatorEngineResult OriginalManualResult;


        private void AnalysisMenuItem_Click(object? sender, EventArgs e)
        {
            // Try to cast the sender to a ToolStripItem
            ToolStripItem menuItem = sender as ToolStripItem;
            if (menuItem != null)
            {
                // Retrieve the ContextMenuStrip that owns this ToolStripItem
                ContextMenuStrip owner = menuItem.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    // Get the control that is displaying this context menu
                    Control sourceControl = owner.SourceControl;
                    SetAnalysis((GanttChart)sourceControl);
                    MainTabControl.SelectTab(PlanningTabPage);
                    PlanningTabControl.SelectTab(HandPlanningPage);
                }
            }
        }

        void SetAnalysis(GanttChart chart)
        {
            var ops = chart.SavedOpersLink;

            //var pool = new OpersPool(sessionLink, ops, new OpersPoolConstructor(ops, sessionLink.AssignedPlanningAlgorithm.MinAcceptableDate), sessionLink.AssignedPlanningAlgorithm.MinAcceptableDate);

            var res = PlanSimulatorEngineResult.CreateFullInstance(sessionLink, sessionLink.AssignedPlanningAlgorithm.MinAcceptableDate);

            res = OriginalManualResult = PlanSimulatorEngine.CalculatePlan(ops, chart.PoolIndex, sessionLink.AssignedPlanningAlgorithm.MinAcceptableDate, res);

            ManualPlanningDataTable.Rows[0].Cells[1].Value = res.DaysRuinedValue;
            ManualPlanningDataTable.Rows[1].Cells[1].Value = res.RechangesSeconds;
            ManualPlanningDataTable.Rows[2].Cells[1].Value = res.ProstoiSeconds;
            ManualPlanningDataTable.Rows[3].Cells[1].Value = res.ProductionSeconds;
            ManualPlanningDataTable.Rows[4].Cells[1].Value = res.TaraRedutantUsage;
            ManualPlanningDataTable.Rows[5].Cells[1].Value = res.TaraCriticalUsage;
            ManualPlanningDataTable.Rows[6].Cells[1].Value = res.BadOffsets;
            ManualPlanningDataTable.Rows[7].Cells[1].Value = res.NaladLengthsTotal;
            ManualPlanningDataTable.Rows[8].Cells[1].Value = res.GetCatchedNaladLengthsErrorsCount(sessionLink.AssignedPlanningAlgorithm.IgnoredGlobalFixedNaladLengthsErrors);

            OutputToGanttChart(ganttANL, ops, chart.PoolIndex, sessionLink.SettingsController.CurrentSettings.PoolSize - 3, res);

        }

        void UpdateAnalysis(int OperIndex, int InsertBeforeThisIndex, Uco ActualRC)
        {
            var ops = ganttANL.SavedOpersLink;
            //var pool = new OpersPool(sessionLink, ops, new OpersPoolConstructor(ops, sessionLink.AssignedPlanningAlgorithm.MinAcceptableDate), sessionLink.AssignedPlanningAlgorithm.MinAcceptableDate);
            
            var operInd0 = ops.FindIndex(x => x.Lists.Any(y => y.StagesAsNormal.Any(z => z.Index == OperIndex)));
            var oper = ops[operInd0];
            ops.RemoveAt(operInd0);

            oper.SetCenter(ActualRC, sessionLink.SettingsController.CurrentSettings.PoolSize - 3);//.ActualRC = ActualRC;


            var operInd = ops.FindIndex(x => x.Lists.Any(y => y.StagesAsNormal.Any(z => z.Index == InsertBeforeThisIndex)));

            ops.Insert(operInd == -1 ? operInd0 : operInd, oper);

            if (operInd == -1) return;

            ganttANL.ClearBars();

            //var newDic = new Dictionary<GanttChart.DoubleID, (int R, int P, int T)>();

            var res = PlanSimulatorEngineResult.CreateFullInstance(sessionLink, sessionLink.AssignedPlanningAlgorithm.MinAcceptableDate);

            res = PlanSimulatorEngine.CalculatePlan(ops, ganttANL.PoolIndex, sessionLink.AssignedPlanningAlgorithm.MinAcceptableDate, res);

            OutputToGanttChart(ganttANL, ops, ganttANL.PoolIndex, sessionLink.SettingsController.CurrentSettings.PoolSize - 3, res);

            ganttANL.FindAllPrevsHere(OperIndex, true); // все нахрен стираем


            ManualPlanningDataTable.Rows[0].Cells[1].Value = res.DaysRuinedValue;
            ManualPlanningDataTable.Rows[1].Cells[1].Value = res.RechangesSeconds;
            ManualPlanningDataTable.Rows[2].Cells[1].Value = res.ProstoiSeconds;
            ManualPlanningDataTable.Rows[3].Cells[1].Value = res.ProductionSeconds;
            ManualPlanningDataTable.Rows[4].Cells[1].Value = res.TaraRedutantUsage;
            ManualPlanningDataTable.Rows[5].Cells[1].Value = res.TaraCriticalUsage;
            ManualPlanningDataTable.Rows[6].Cells[1].Value = res.BadOffsets;
            ManualPlanningDataTable.Rows[7].Cells[1].Value = res.NaladLengthsTotal;
            ManualPlanningDataTable.Rows[8].Cells[1].Value = res.GetCatchedNaladLengthsErrorsCount(sessionLink.AssignedPlanningAlgorithm.IgnoredGlobalFixedNaladLengthsErrors);


            ManualPlanningDataTable.Rows[0].Cells[2].Value = OriginalManualResult.DaysRuinedValue;
            ManualPlanningDataTable.Rows[1].Cells[2].Value = OriginalManualResult.RechangesSeconds;
            ManualPlanningDataTable.Rows[2].Cells[2].Value = OriginalManualResult.ProstoiSeconds;
            ManualPlanningDataTable.Rows[3].Cells[2].Value = OriginalManualResult.ProductionSeconds;
            ManualPlanningDataTable.Rows[4].Cells[2].Value = OriginalManualResult.TaraRedutantUsage;
            ManualPlanningDataTable.Rows[5].Cells[2].Value = OriginalManualResult.TaraCriticalUsage;
            ManualPlanningDataTable.Rows[6].Cells[2].Value = OriginalManualResult.BadOffsets;
            ManualPlanningDataTable.Rows[7].Cells[2].Value = OriginalManualResult.NaladLengthsTotal;
            ManualPlanningDataTable.Rows[8].Cells[2].Value = OriginalManualResult.GetCatchedNaladLengthsErrorsCount(sessionLink.AssignedPlanningAlgorithm.IgnoredGlobalFixedNaladLengthsErrors);

            ManualPlanningDataTable.Rows[0].Cells[3].Value = Math.Round(100 - (res.DaysRuinedValue * 100f / OriginalManualResult.DaysRuinedValue), 2) + "%";
            ManualPlanningDataTable.Rows[1].Cells[3].Value = Math.Round(100 - (res.RechangesSeconds * 100f / OriginalManualResult.RechangesSeconds), 2) + "%";
            ManualPlanningDataTable.Rows[2].Cells[3].Value = Math.Round(100 - (res.ProstoiSeconds * 100f / OriginalManualResult.ProstoiSeconds), 2) + "%";
            ManualPlanningDataTable.Rows[3].Cells[3].Value = Math.Round(100 - (res.ProductionSeconds * 100f / OriginalManualResult.ProductionSeconds), 2) + "%";
            ManualPlanningDataTable.Rows[4].Cells[3].Value = Math.Round(100 - (res.TaraRedutantUsage * 100f / OriginalManualResult.TaraRedutantUsage), 2) + "%";
            ManualPlanningDataTable.Rows[5].Cells[3].Value = Math.Round(100 - (res.TaraCriticalUsage * 100f / OriginalManualResult.TaraCriticalUsage), 2) + "%";
            ManualPlanningDataTable.Rows[6].Cells[3].Value = Math.Round(100 - (res.BadOffsets * 100f / OriginalManualResult.BadOffsets), 2) + "%";
            ManualPlanningDataTable.Rows[7].Cells[3].Value = Math.Round(100 - (res.NaladLengthsTotal * 100f / OriginalManualResult.NaladLengthsTotal), 2) + "%";
            ManualPlanningDataTable.Rows[8].Cells[3].Value = Math.Round(100 - (res.GetCatchedNaladLengthsErrorsCount(sessionLink.AssignedPlanningAlgorithm.IgnoredGlobalFixedNaladLengthsErrors) * 100f / OriginalManualResult.GetCatchedNaladLengthsErrorsCount(sessionLink.AssignedPlanningAlgorithm.IgnoredGlobalFixedNaladLengthsErrors)), 2) + "%";

            OriginalManualResult = res;
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {

            var allMats = sessionLink.MasterDataLinks.StagesMasterData.AllStages.Where(x => x.Value.StageMaterialsInfo != null && x.Value.StageMaterialsInfo.UsedMaterials != null).SelectMany(x => x.Value.StageMaterialsInfo.UsedMaterials).DistinctBy(x => x.Nomenklature)
                .Where(x => Random.Shared.NextDouble() < 0.75)
                .Select(x => (Mat: x, Count: Random.Shared.Next(0, 10000) / 1000.0));

            var allSubdivisions = sessionLink.MasterDataLinks.StagesMasterData.AllStages.Select(x => x.Value.Subdivision).Distinct().ToList();

            var streamWriter = new StreamWriter("ОстаткиНаСкладе.txt");
            streamWriter.WriteLine("Номенклатура\tНоменклатура_GUID\tХарактеристика\tХарактеристика_GUID\tПодразделение\tПодразделение_GUID\tКоличество");
            var arrivalInfo = new StreamWriter("ПриходМатериалов.txt");
            arrivalInfo.WriteLine("Номенклатура\tНоменклатура_GUID\tХарактеристика\tХарактеристика_GUID\tДатаПоступления\tПодразделение\tПодразделение_GUID\tКоличествоПоступления");
            foreach (var mat in allMats)
            {
                var sub = allSubdivisions[Random.Shared.Next(0, allSubdivisions.Count)];
                streamWriter.WriteLine($"{mat.Mat.Nomenklature.Label}\t{mat.Mat.Nomenklature.Code}\t{mat.Mat.Characteristics.Label}\t{mat.Mat.Characteristics.Code}\t{sub.Label}\t{sub.Code}\t{mat.Count}");
                int count = Random.Shared.Next(0, 20);
                double totalTime = 0;
                for (int i = 0; i < count; i++)
                {
                    var doubler = Random.Shared.NextDouble() * 7.0;
                    totalTime += doubler;
                    var time = DateTime.Now.AddDays(totalTime);
                    var willArrive = Random.Shared.Next(0, 10000) / 1000.0;
                    var sub2 = allSubdivisions[Random.Shared.Next(0, allSubdivisions.Count)];
                    arrivalInfo.WriteLine($"{mat.Mat.Nomenklature.Label}\t{mat.Mat.Nomenklature.Code}\t{mat.Mat.Characteristics.Label}\t{mat.Mat.Characteristics.Code}\t{time}\t{sub2.Label}\t{sub2.Code}\t{willArrive}");
                }
            }
            streamWriter.Close();
            arrivalInfo.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var algorithm = sessionLink.AssignedPlanningAlgorithm;
            if (algorithm == null || algorithm.UsedPool == null) return;
            sessionLink.MasterDataLinks.LeftoversMasterData.PrepareModsForLeftovers(algorithm.UsedPool.Blocks);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Console.WriteLine("go:");
            var centerToCopy = sessionLink.AssignedPlanningAlgorithm.UsedPool.CentersData.First().Value[0];
            for (int i = 0; i < 32; i++)
            {
                var dt0 = DateTime.Now;
                var result = PlanSimulatorEngine.CalculatePlan(sessionLink.AssignedPlanningAlgorithm.UsedPool.Blocks, 0, sessionLink.AssignedPlanningAlgorithm.MinAcceptableDate, null);
                var dt1 = DateTime.Now;
                Console.WriteLine($"plan calc time: {dt1 - dt0}");
                dt1 = DateTime.Now;
                var blocksCopyLocal = PlanSimulatorPool.CopyBlocksSwallow(sessionLink.AssignedPlanningAlgorithm.UsedPool.Blocks);
                PlanSimulatorPool.MoveCentersInAllBlocks(blocksCopyLocal, 0, 0);
                var dt2 = DateTime.Now;
                Console.WriteLine($"copy time: {dt2 - dt1}");

                var dt3 = DateTime.Now;
                var copy = centerToCopy.Copy();
                var dt4 = DateTime.Now;
                Console.WriteLine($"center copy time: {dt4 - dt3}");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var writer = new StreamWriter("МаксимальныеНамоткиНаТару.txt", false);
            writer.WriteLine("Продукция\tПродукция_GUID\tНоменклатура\tНоменклатура_GUID\tХарактеристика\tХарактеристика_GUID\tВидОперации\tВидОперации_GUID\tТипТары\tТипТары_GUID\tЕдиницаХраненияОстатков\tВеличинаНамотки");
            foreach (var level1 in sessionLink.MasterDataLinks.TarasMasterData.TarasMaxCapacities)
            {
                var production = level1.Key;
                foreach (var level2 in level1.Value)
                {
                    var nomenklature = level2.Key;
                    foreach (var level3 in level2.Value)
                    {
                        var chars = level3.Key;
                        foreach (var level4 in level3.Value)
                        {
                            var vid = level4.Key;
                            foreach (var level5 in level4.Value)
                            {
                                var tara = level5.Key;
                                foreach (var data in level5.Value)
                                {
                                    var unit = data.Format;
                                    var value = data.Value;
                                    writer.WriteLine($"{production.Label}\t{production.Code}\t{nomenklature.Label}\t{nomenklature.Code}\t{chars.Label}\t{chars.Code}\t{vid.Label}\t{vid.Code}\t{tara.Label}\t{tara.Code}\t{unit.DimensionUnitToString(false)}\t{value}");
                                }
                            }
                        }

                    }
                }
            }
            writer.Close();
            Console.WriteLine("max done");
        }
    }
}

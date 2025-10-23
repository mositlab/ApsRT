using WinFormsCustomComponents;
using ApsRTCustomComponents;
using ZedGraph;
using Label = System.Windows.Forms.Label;

namespace ApsRTCustomComponents
{
    partial class ApsRTMainForm1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApsRTMainForm1));
            OpenPlanningButton = new Button();
            OpenPartiasButton = new Button();
            materialTabSelector1 = new MaterialTabSelector();
            MainTabControl = new MaterialTabControl();
            MainTabPage = new TabPage();
            VersionLabel = new Label();
            label49 = new Label();
            pictureBox1 = new PictureBox();
            label48 = new Label();
            OpenSettingsButton = new Button();
            PanelNearMainLabel = new Panel();
            OpenGraphsButton = new Button();
            OpenHandPlanButton = new Button();
            OpenHandPartiasButton = new Button();
            mainlabel = new Label();
            PartiasTabPage = new TabPage();
            PartiasTabControl = new MaterialTabControl();
            AllPartiasTabControl = new TabPage();
            splitContainer1 = new SplitContainer();
            AllForAllPartiasGuidComboBox = new ComboBox();
            AllForAllOrdersComboBox = new ComboBox();
            label1 = new Label();
            LogPartiasCheckBox = new MaterialCheckBox();
            AllForAllTextBox = new TextBox();
            label2 = new Label();
            PartiasProgressLabel = new Label();
            StartAlgorithmPButton = new Button();
            ZadelCheckBox = new MaterialCheckBox();
            PartCheckBox = new MaterialCheckBox();
            PartiasProgressBar = new CustomProgressBar();
            splitContainer3 = new SplitContainer();
            CustomTreeViewOnPPanel = new CustomTreeView();
            PartiasDataTable = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            HandPartiasTabControl = new TabPage();
            InGPCheckBox = new MaterialCheckBox();
            splitContainer4 = new SplitContainer();
            TwoPartiasTextBox = new TextBox();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            TwoPartiasPartia1View = new CustomTreeView();
            tabPage2 = new TabPage();
            TwoPartiasPartia2View = new CustomTreeView();
            tabPage3 = new TabPage();
            TwoPartiasPartiaResView = new CustomTreeView();
            label8 = new Label();
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            TwoPartiasGP2ComboBox = new ComboBox();
            TwoPartiasGP1ComboBox = new ComboBox();
            TwoPartiasOrder2ComboBox = new ComboBox();
            TwoPartiasOrder1ComboBox = new ComboBox();
            TwoPartiasPartia2ComboBox = new ComboBox();
            TwoPartiasStartButton = new Button();
            TwoPartiasPartia1ComboBox = new ComboBox();
            materialTabSelector2 = new MaterialTabSelector();
            PlanningTabPage = new TabPage();
            PlanningTabControl = new MaterialTabControl();
            TyrantTabPage = new TabPage();
            splitContainer5 = new SplitContainer();
            splitContainer7 = new SplitContainer();
            StagesColorCheckBox = new MaterialCheckBox();
            ModsColorCheckBox = new MaterialCheckBox();
            StopPlanningButton = new Button();
            OutputButton = new Button();
            PlanningProcessLabel = new Label();
            LaunchPlanningButton = new Button();
            PlanningProcessBar = new CustomProgressBar();
            splitContainer8 = new SplitContainer();
            PlanningDataTable = new DataGridView();
            Column6 = new DataGridViewTextBoxColumn();
            Column7 = new DataGridViewTextBoxColumn();
            Column8 = new DataGridViewTextBoxColumn();
            Column9 = new DataGridViewTextBoxColumn();
            BarDescTextBox = new TextBox();
            splitContainer6 = new SplitContainer();
            ganttOLD = new GanttChart();
            vScrollBarOLD = new VScrollBar();
            ganttNEW = new GanttChart();
            vScrollBarNEW = new VScrollBar();
            panel1 = new Panel();
            label11 = new Label();
            dateTimePickerTo = new DateTimePicker();
            label10 = new Label();
            dateTimePickerFrom = new DateTimePicker();
            HandPlanningPage = new TabPage();
            splitContainer17 = new SplitContainer();
            splitContainer18 = new SplitContainer();
            ManualPlanningDataTable = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
            AnlDescTextBox = new TextBox();
            panel3 = new Panel();
            dateTimePickerFromAnl = new DateTimePicker();
            label45 = new Label();
            label44 = new Label();
            dateTimePickerToAnl = new DateTimePicker();
            ganttANL = new GanttChart();
            vScrollBarANL = new VScrollBar();
            GraphsTabPage = new TabPage();
            GraphsMaterialTabControl = new MaterialTabControl();
            tabPage8 = new TabPage();
            splitContainer14 = new SplitContainer();
            splitContainer15 = new SplitContainer();
            OrdersGraphControl = new ZedGraphControl();
            ProstoiGraphControl = new ZedGraphControl();
            splitContainer16 = new SplitContainer();
            RechangesGraphControl = new ZedGraphControl();
            TotalTimeGraphControl = new ZedGraphControl();
            tabPage9 = new TabPage();
            splitContainer11 = new SplitContainer();
            splitContainer12 = new SplitContainer();
            TaraRedutantUsageGraphControl = new ZedGraphControl();
            zedGraphControl2 = new ZedGraphControl();
            splitContainer13 = new SplitContainer();
            zedGraphControl1 = new ZedGraphControl();
            zedGraphControl3 = new ZedGraphControl();
            tabPage4 = new TabPage();
            splitContainer9 = new SplitContainer();
            splitContainer10 = new SplitContainer();
            TarasGraphControl = new ZedGraphControl();
            MaterialsGraphControl = new ZedGraphControl();
            ProductionGraphControl = new ZedGraphControl();
            panel2 = new Panel();
            label47 = new Label();
            MaterialsListOnGraphics = new ComboBox();
            label46 = new Label();
            TaraListOnGraphics = new ComboBox();
            tabPage5 = new TabPage();
            AllPercentagesGraphControl = new ZedGraphControl();
            materialTabSelector4 = new MaterialTabSelector();
            materialTabSelector3 = new MaterialTabSelector();
            SettingsTabPage = new TabPage();
            splitContainer2 = new SplitContainer();
            HoppersPanel = new Panel();
            groupBox8 = new GroupBox();
            StagesPercentNumericUpDown = new NumericUpDown();
            label58 = new Label();
            RNewDeadlineCheckBox = new MaterialCheckBox();
            UseRCheckBox = new MaterialCheckBox();
            TMaxDaysNumericUpDown = new NumericUpDown();
            RGlobDeltaCheckBox = new MaterialCheckBox();
            label57 = new Label();
            UseRStarCheckBox = new MaterialCheckBox();
            TMaxDaysPercentNumericUpDown = new NumericUpDown();
            UseTCheckBox = new MaterialCheckBox();
            label56 = new Label();
            UseGCheckBox = new MaterialCheckBox();
            label54 = new Label();
            GammaSlider = new TrackBar();
            GammaLabel = new Label();
            label55 = new Label();
            label51 = new Label();
            MaxOldSessionPathsDaysNumericUpDown = new NumericUpDown();
            button4 = new Button();
            button3 = new Button();
            button2 = new Button();
            button1 = new Button();
            groupBox7 = new GroupBox();
            UsePseudoTaraCheckBox = new MaterialCheckBox();
            CreateStopMe = new MaterialCheckBox();
            CreateKillMeCheckBox = new MaterialCheckBox();
            AutoLoadCheckBox = new MaterialCheckBox();
            SaveOnExitCheckBox = new MaterialCheckBox();
            HideConsoleCheckBox = new MaterialCheckBox();
            LogErrorsCheckBox = new MaterialCheckBox();
            groupBox6 = new GroupBox();
            VKoeffsTextBox = new TextBox();
            criteriaRadioButton7 = new MaterialRadioButton();
            materialRadioButtonAuto = new MaterialRadioButton();
            CriteriasMaxIterationsNumericUpDown7 = new NumericUpDown();
            CriteriasMinIterationsNumericUpDown7 = new NumericUpDown();
            CriteriasPossibleWorseNumericUpDown7 = new NumericUpDown();
            CriteriasMaxIterationsNumericUpDown6 = new NumericUpDown();
            CriteriasMinIterationsNumericUpDown6 = new NumericUpDown();
            CriteriasPossibleWorseNumericUpDown6 = new NumericUpDown();
            label9 = new Label();
            CriteriasMaxIterationsNumericUpDown5 = new NumericUpDown();
            CriteriasMinIterationsNumericUpDown5 = new NumericUpDown();
            CriteriasPossibleWorseNumericUpDown5 = new NumericUpDown();
            CriteriasMaxIterationsNumericUpDown4 = new NumericUpDown();
            CriteriasMinIterationsNumericUpDown4 = new NumericUpDown();
            CriteriasPossibleWorseNumericUpDown4 = new NumericUpDown();
            CriteriasMaxIterationsNumericUpDown3 = new NumericUpDown();
            CriteriasMinIterationsNumericUpDown3 = new NumericUpDown();
            CriteriasPossibleWorseNumericUpDown3 = new NumericUpDown();
            CriteriasMaxIterationsNumericUpDown2 = new NumericUpDown();
            CriteriasMinIterationsNumericUpDown2 = new NumericUpDown();
            CriteriasPossibleWorseNumericUpDown2 = new NumericUpDown();
            label41 = new Label();
            UseOnlyMaxCriteriasSettingsCheckBox = new MaterialCheckBox();
            CriteriasMaxIterationsNumericUpDown1 = new NumericUpDown();
            CriteriasMinIterationsNumericUpDown1 = new NumericUpDown();
            CriteriasPossibleWorseNumericUpDown1 = new NumericUpDown();
            label40 = new Label();
            materialRadioButtonCombo = new MaterialRadioButton();
            CriteriasListTextBox = new TextBox();
            criteriaRadioButton6 = new MaterialRadioButton();
            criteriaRadioButton5 = new MaterialRadioButton();
            criteriaRadioButton4 = new MaterialRadioButton();
            criteriaRadioButton3 = new MaterialRadioButton();
            criteriaRadioButton2 = new MaterialRadioButton();
            criteriaRadioButton1 = new MaterialRadioButton();
            label42 = new Label();
            label43 = new Label();
            groupBox2 = new GroupBox();
            groupBox5 = new GroupBox();
            label31 = new Label();
            p0l3aNumericUpDown = new NumericUpDown();
            label32 = new Label();
            p0l3kNumericUpDown = new NumericUpDown();
            label33 = new Label();
            p0l2xNumericUpDown = new NumericUpDown();
            label34 = new Label();
            p0l2aNumericUpDown = new NumericUpDown();
            label35 = new Label();
            p0l2kNumericUpDown = new NumericUpDown();
            label36 = new Label();
            p0l1xNumericUpDown = new NumericUpDown();
            label37 = new Label();
            p0l1aNumericUpDown = new NumericUpDown();
            label38 = new Label();
            p0l1kNumericUpDown = new NumericUpDown();
            label39 = new Label();
            groupBox4 = new GroupBox();
            label22 = new Label();
            p1l3aNumericUpDown = new NumericUpDown();
            label23 = new Label();
            p1l3kNumericUpDown = new NumericUpDown();
            label24 = new Label();
            p1l2xNumericUpDown = new NumericUpDown();
            label25 = new Label();
            p1l2aNumericUpDown = new NumericUpDown();
            label26 = new Label();
            p1l2kNumericUpDown = new NumericUpDown();
            label27 = new Label();
            p1l1xNumericUpDown = new NumericUpDown();
            label28 = new Label();
            p1l1aNumericUpDown = new NumericUpDown();
            label29 = new Label();
            p1l1kNumericUpDown = new NumericUpDown();
            label30 = new Label();
            label12 = new Label();
            groupBox3 = new GroupBox();
            label19 = new Label();
            p2l3aNumericUpDown = new NumericUpDown();
            label20 = new Label();
            p2l3kNumericUpDown = new NumericUpDown();
            label21 = new Label();
            p2l2xNumericUpDown = new NumericUpDown();
            label16 = new Label();
            p2l2aNumericUpDown = new NumericUpDown();
            label17 = new Label();
            p2l2kNumericUpDown = new NumericUpDown();
            label18 = new Label();
            p2l1xNumericUpDown = new NumericUpDown();
            label15 = new Label();
            p2l1aNumericUpDown = new NumericUpDown();
            label14 = new Label();
            p2l1kNumericUpDown = new NumericUpDown();
            label13 = new Label();
            groupBox1 = new GroupBox();
            CyclesNumericUpDown = new NumericUpDown();
            AfterXCyclesRadioButton = new MaterialRadioButton();
            MinutesNumericUpDown = new NumericUpDown();
            IterationsNumericUpDown = new NumericUpDown();
            AfterXMinsRadioButton = new MaterialRadioButton();
            AfterXItersRadioButton = new MaterialRadioButton();
            ClearAllSessionErrorsButton = new Button();
            HandSessionPostDataProcessButton = new Button();
            SaveSettingsButton = new Button();
            ToggleThemeButton = new Button();
            BackupStagesDurationLabel = new Label();
            BackupStagesDurationNumericUpDown = new NumericUpDown();
            PartiasMaxDaysLabel = new Label();
            PartiasMaxDaysNumericUpDown = new NumericUpDown();
            AuthorsTabPage = new TabPage();
            label53 = new Label();
            label50 = new Label();
            pictureBox2 = new PictureBox();
            label52 = new Label();
            folderBrowserDialog1 = new FolderBrowserDialog();
            openFileDialog1 = new OpenFileDialog();
            contextMenuStrip1 = new ContextMenuStrip(components);
            MainTabControl.SuspendLayout();
            MainTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            PartiasTabPage.SuspendLayout();
            PartiasTabControl.SuspendLayout();
            AllPartiasTabControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PartiasDataTable).BeginInit();
            HandPartiasTabControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer4).BeginInit();
            splitContainer4.Panel1.SuspendLayout();
            splitContainer4.Panel2.SuspendLayout();
            splitContainer4.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            PlanningTabPage.SuspendLayout();
            PlanningTabControl.SuspendLayout();
            TyrantTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer5).BeginInit();
            splitContainer5.Panel1.SuspendLayout();
            splitContainer5.Panel2.SuspendLayout();
            splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer7).BeginInit();
            splitContainer7.Panel1.SuspendLayout();
            splitContainer7.Panel2.SuspendLayout();
            splitContainer7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer8).BeginInit();
            splitContainer8.Panel1.SuspendLayout();
            splitContainer8.Panel2.SuspendLayout();
            splitContainer8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PlanningDataTable).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer6).BeginInit();
            splitContainer6.Panel1.SuspendLayout();
            splitContainer6.Panel2.SuspendLayout();
            splitContainer6.SuspendLayout();
            panel1.SuspendLayout();
            HandPlanningPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer17).BeginInit();
            splitContainer17.Panel1.SuspendLayout();
            splitContainer17.Panel2.SuspendLayout();
            splitContainer17.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer18).BeginInit();
            splitContainer18.Panel1.SuspendLayout();
            splitContainer18.Panel2.SuspendLayout();
            splitContainer18.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ManualPlanningDataTable).BeginInit();
            panel3.SuspendLayout();
            GraphsTabPage.SuspendLayout();
            GraphsMaterialTabControl.SuspendLayout();
            tabPage8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer14).BeginInit();
            splitContainer14.Panel1.SuspendLayout();
            splitContainer14.Panel2.SuspendLayout();
            splitContainer14.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer15).BeginInit();
            splitContainer15.Panel1.SuspendLayout();
            splitContainer15.Panel2.SuspendLayout();
            splitContainer15.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer16).BeginInit();
            splitContainer16.Panel1.SuspendLayout();
            splitContainer16.Panel2.SuspendLayout();
            splitContainer16.SuspendLayout();
            tabPage9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer11).BeginInit();
            splitContainer11.Panel1.SuspendLayout();
            splitContainer11.Panel2.SuspendLayout();
            splitContainer11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer12).BeginInit();
            splitContainer12.Panel1.SuspendLayout();
            splitContainer12.Panel2.SuspendLayout();
            splitContainer12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer13).BeginInit();
            splitContainer13.Panel1.SuspendLayout();
            splitContainer13.Panel2.SuspendLayout();
            splitContainer13.SuspendLayout();
            tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer9).BeginInit();
            splitContainer9.Panel1.SuspendLayout();
            splitContainer9.Panel2.SuspendLayout();
            splitContainer9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer10).BeginInit();
            splitContainer10.Panel1.SuspendLayout();
            splitContainer10.Panel2.SuspendLayout();
            splitContainer10.SuspendLayout();
            panel2.SuspendLayout();
            tabPage5.SuspendLayout();
            SettingsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            groupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)StagesPercentNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TMaxDaysNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TMaxDaysPercentNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)GammaSlider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MaxOldSessionPathsDaysNumericUpDown).BeginInit();
            groupBox7.SuspendLayout();
            groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown7).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown7).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown7).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown1).BeginInit();
            groupBox2.SuspendLayout();
            groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)p0l3aNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p0l3kNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p0l2xNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p0l2aNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p0l2kNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p0l1xNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p0l1aNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p0l1kNumericUpDown).BeginInit();
            groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)p1l3aNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p1l3kNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p1l2xNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p1l2aNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p1l2kNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p1l1xNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p1l1aNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p1l1kNumericUpDown).BeginInit();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)p2l3aNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p2l3kNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p2l2xNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p2l2aNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p2l2kNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p2l1xNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p2l1aNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)p2l1kNumericUpDown).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CyclesNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MinutesNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)IterationsNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)BackupStagesDurationNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)PartiasMaxDaysNumericUpDown).BeginInit();
            AuthorsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // OpenPlanningButton
            // 
            OpenPlanningButton.BackColor = Color.CornflowerBlue;
            OpenPlanningButton.BackgroundImage = (Image)resources.GetObject("OpenPlanningButton.BackgroundImage");
            OpenPlanningButton.BackgroundImageLayout = ImageLayout.Zoom;
            OpenPlanningButton.Cursor = Cursors.Hand;
            OpenPlanningButton.FlatAppearance.BorderSize = 0;
            OpenPlanningButton.FlatStyle = FlatStyle.Flat;
            OpenPlanningButton.Font = new Font("Microsoft Sans Serif", 10F);
            OpenPlanningButton.ForeColor = Color.White;
            OpenPlanningButton.Location = new Point(66, 295);
            OpenPlanningButton.Name = "OpenPlanningButton";
            OpenPlanningButton.Size = new Size(316, 158);
            OpenPlanningButton.TabIndex = 1;
            OpenPlanningButton.Text = "Планирование";
            OpenPlanningButton.TextAlign = ContentAlignment.BottomLeft;
            OpenPlanningButton.UseVisualStyleBackColor = false;
            // 
            // OpenPartiasButton
            // 
            OpenPartiasButton.BackColor = Color.CornflowerBlue;
            OpenPartiasButton.BackgroundImage = (Image)resources.GetObject("OpenPartiasButton.BackgroundImage");
            OpenPartiasButton.BackgroundImageLayout = ImageLayout.Zoom;
            OpenPartiasButton.Cursor = Cursors.Hand;
            OpenPartiasButton.FlatAppearance.BorderSize = 0;
            OpenPartiasButton.FlatStyle = FlatStyle.Flat;
            OpenPartiasButton.Font = new Font("Microsoft Sans Serif", 10F);
            OpenPartiasButton.ForeColor = Color.White;
            OpenPartiasButton.Location = new Point(66, 131);
            OpenPartiasButton.Name = "OpenPartiasButton";
            OpenPartiasButton.Size = new Size(316, 158);
            OpenPartiasButton.TabIndex = 2;
            OpenPartiasButton.Text = "Формирование партий";
            OpenPartiasButton.TextAlign = ContentAlignment.BottomLeft;
            OpenPartiasButton.UseVisualStyleBackColor = false;
            // 
            // materialTabSelector1
            // 
            materialTabSelector1.BackColor = Color.CornflowerBlue;
            materialTabSelector1.BaseTabControl = MainTabControl;
            materialTabSelector1.Dock = DockStyle.Top;
            materialTabSelector1.Font = new Font("Microsoft Sans Serif", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            materialTabSelector1.ForeColor = Color.White;
            materialTabSelector1.Location = new Point(0, 0);
            materialTabSelector1.Name = "materialTabSelector1";
            materialTabSelector1.Size = new Size(1179, 35);
            materialTabSelector1.TabIndex = 16;
            materialTabSelector1.Text = "materialTabSelector1";
            // 
            // MainTabControl
            // 
            MainTabControl.Controls.Add(MainTabPage);
            MainTabControl.Controls.Add(PartiasTabPage);
            MainTabControl.Controls.Add(PlanningTabPage);
            MainTabControl.Controls.Add(SettingsTabPage);
            MainTabControl.Controls.Add(AuthorsTabPage);
            MainTabControl.Dock = DockStyle.Fill;
            MainTabControl.Location = new Point(0, 35);
            MainTabControl.Name = "MainTabControl";
            MainTabControl.SelectedIndex = 0;
            MainTabControl.Size = new Size(1179, 530);
            MainTabControl.TabIndex = 17;
            // 
            // MainTabPage
            // 
            MainTabPage.BackColor = Color.White;
            MainTabPage.Controls.Add(VersionLabel);
            MainTabPage.Controls.Add(label49);
            MainTabPage.Controls.Add(pictureBox1);
            MainTabPage.Controls.Add(label48);
            MainTabPage.Controls.Add(OpenSettingsButton);
            MainTabPage.Controls.Add(PanelNearMainLabel);
            MainTabPage.Controls.Add(OpenGraphsButton);
            MainTabPage.Controls.Add(OpenHandPlanButton);
            MainTabPage.Controls.Add(OpenHandPartiasButton);
            MainTabPage.Controls.Add(mainlabel);
            MainTabPage.Controls.Add(OpenPlanningButton);
            MainTabPage.Controls.Add(OpenPartiasButton);
            MainTabPage.Location = new Point(4, 25);
            MainTabPage.Name = "MainTabPage";
            MainTabPage.Size = new Size(1171, 501);
            MainTabPage.TabIndex = 2;
            MainTabPage.Text = "Главная";
            // 
            // VersionLabel
            // 
            VersionLabel.AutoSize = true;
            VersionLabel.Font = new Font("Segoe UI Light", 10F);
            VersionLabel.Location = new Point(32, 77);
            VersionLabel.Name = "VersionLabel";
            VersionLabel.Size = new Size(93, 19);
            VersionLabel.TabIndex = 25;
            VersionLabel.Text = "Версия RT 2.0";
            // 
            // label49
            // 
            label49.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label49.AutoSize = true;
            label49.Font = new Font("Segoe UI Light", 8F);
            label49.Location = new Point(959, 401);
            label49.Name = "label49";
            label49.RightToLeft = RightToLeft.No;
            label49.Size = new Size(102, 39);
            label49.TabIndex = 24;
            label49.Text = "ООО «МОСИТЛАБ»\r\n+7 (495) 280-82-99\r\nwww.mositlab.ru";
            label49.TextAlign = ContentAlignment.MiddleRight;
            label49.Click += label49_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(1099, 398);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(64, 64);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 23;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // label48
            // 
            label48.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label48.AutoSize = true;
            label48.Font = new Font("Segoe UI Light", 10F);
            label48.Location = new Point(3, 448);
            label48.Name = "label48";
            label48.Size = new Size(93, 19);
            label48.TabIndex = 22;
            label48.Text = "Версия RT 2.0";
            // 
            // OpenSettingsButton
            // 
            OpenSettingsButton.BackColor = Color.CornflowerBlue;
            OpenSettingsButton.BackgroundImage = (Image)resources.GetObject("OpenSettingsButton.BackgroundImage");
            OpenSettingsButton.BackgroundImageLayout = ImageLayout.Zoom;
            OpenSettingsButton.Cursor = Cursors.Hand;
            OpenSettingsButton.FlatAppearance.BorderSize = 0;
            OpenSettingsButton.FlatStyle = FlatStyle.Flat;
            OpenSettingsButton.Font = new Font("Microsoft Sans Serif", 10F);
            OpenSettingsButton.ForeColor = Color.White;
            OpenSettingsButton.Location = new Point(929, 131);
            OpenSettingsButton.Name = "OpenSettingsButton";
            OpenSettingsButton.Size = new Size(158, 158);
            OpenSettingsButton.TabIndex = 21;
            OpenSettingsButton.Text = "Настройки";
            OpenSettingsButton.TextAlign = ContentAlignment.BottomLeft;
            OpenSettingsButton.UseVisualStyleBackColor = false;
            // 
            // PanelNearMainLabel
            // 
            PanelNearMainLabel.BackColor = Color.Black;
            PanelNearMainLabel.Location = new Point(32, 68);
            PanelNearMainLabel.Name = "PanelNearMainLabel";
            PanelNearMainLabel.Size = new Size(830, 6);
            PanelNearMainLabel.TabIndex = 20;
            // 
            // OpenGraphsButton
            // 
            OpenGraphsButton.BackColor = Color.CornflowerBlue;
            OpenGraphsButton.BackgroundImage = (Image)resources.GetObject("OpenGraphsButton.BackgroundImage");
            OpenGraphsButton.BackgroundImageLayout = ImageLayout.Zoom;
            OpenGraphsButton.Cursor = Cursors.Hand;
            OpenGraphsButton.FlatAppearance.BorderSize = 0;
            OpenGraphsButton.FlatStyle = FlatStyle.Flat;
            OpenGraphsButton.Font = new Font("Microsoft Sans Serif", 8F);
            OpenGraphsButton.ForeColor = Color.White;
            OpenGraphsButton.Location = new Point(662, 131);
            OpenGraphsButton.Name = "OpenGraphsButton";
            OpenGraphsButton.Size = new Size(158, 158);
            OpenGraphsButton.TabIndex = 19;
            OpenGraphsButton.Text = "Графики эффективности";
            OpenGraphsButton.TextAlign = ContentAlignment.BottomLeft;
            OpenGraphsButton.UseVisualStyleBackColor = false;
            // 
            // OpenHandPlanButton
            // 
            OpenHandPlanButton.BackColor = Color.CornflowerBlue;
            OpenHandPlanButton.BackgroundImage = (Image)resources.GetObject("OpenHandPlanButton.BackgroundImage");
            OpenHandPlanButton.BackgroundImageLayout = ImageLayout.Zoom;
            OpenHandPlanButton.Cursor = Cursors.Hand;
            OpenHandPlanButton.FlatAppearance.BorderSize = 0;
            OpenHandPlanButton.FlatStyle = FlatStyle.Flat;
            OpenHandPlanButton.Font = new Font("Microsoft Sans Serif", 8F);
            OpenHandPlanButton.ForeColor = Color.White;
            OpenHandPlanButton.Location = new Point(498, 295);
            OpenHandPlanButton.Name = "OpenHandPlanButton";
            OpenHandPlanButton.Size = new Size(158, 158);
            OpenHandPlanButton.TabIndex = 18;
            OpenHandPlanButton.Text = "Ручной анализ плана";
            OpenHandPlanButton.TextAlign = ContentAlignment.BottomLeft;
            OpenHandPlanButton.UseVisualStyleBackColor = false;
            // 
            // OpenHandPartiasButton
            // 
            OpenHandPartiasButton.BackColor = Color.CornflowerBlue;
            OpenHandPartiasButton.BackgroundImage = (Image)resources.GetObject("OpenHandPartiasButton.BackgroundImage");
            OpenHandPartiasButton.BackgroundImageLayout = ImageLayout.Zoom;
            OpenHandPartiasButton.Cursor = Cursors.Hand;
            OpenHandPartiasButton.FlatAppearance.BorderSize = 0;
            OpenHandPartiasButton.FlatStyle = FlatStyle.Flat;
            OpenHandPartiasButton.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            OpenHandPartiasButton.ForeColor = Color.White;
            OpenHandPartiasButton.Location = new Point(498, 131);
            OpenHandPartiasButton.Name = "OpenHandPartiasButton";
            OpenHandPartiasButton.Size = new Size(158, 158);
            OpenHandPartiasButton.TabIndex = 17;
            OpenHandPartiasButton.Text = "Поштуч. объединение";
            OpenHandPartiasButton.TextAlign = ContentAlignment.BottomLeft;
            OpenHandPartiasButton.UseVisualStyleBackColor = false;
            // 
            // mainlabel
            // 
            mainlabel.AutoSize = true;
            mainlabel.Font = new Font("Segoe UI Light", 22.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            mainlabel.Location = new Point(22, 15);
            mainlabel.Name = "mainlabel";
            mainlabel.Size = new Size(686, 41);
            mainlabel.TabIndex = 16;
            mainlabel.Text = "Система оптимизации графика производства (APS)";
            // 
            // PartiasTabPage
            // 
            PartiasTabPage.BackColor = Color.White;
            PartiasTabPage.Controls.Add(PartiasTabControl);
            PartiasTabPage.Controls.Add(materialTabSelector2);
            PartiasTabPage.Location = new Point(4, 24);
            PartiasTabPage.Name = "PartiasTabPage";
            PartiasTabPage.Size = new Size(1171, 502);
            PartiasTabPage.TabIndex = 0;
            PartiasTabPage.Text = "Формирование партий";
            // 
            // PartiasTabControl
            // 
            PartiasTabControl.Controls.Add(AllPartiasTabControl);
            PartiasTabControl.Controls.Add(HandPartiasTabControl);
            PartiasTabControl.Dock = DockStyle.Fill;
            PartiasTabControl.Location = new Point(0, 35);
            PartiasTabControl.Name = "PartiasTabControl";
            PartiasTabControl.SelectedIndex = 0;
            PartiasTabControl.Size = new Size(1171, 467);
            PartiasTabControl.TabIndex = 21;
            // 
            // AllPartiasTabControl
            // 
            AllPartiasTabControl.BackColor = Color.White;
            AllPartiasTabControl.Controls.Add(splitContainer1);
            AllPartiasTabControl.Location = new Point(4, 25);
            AllPartiasTabControl.Name = "AllPartiasTabControl";
            AllPartiasTabControl.Padding = new Padding(3);
            AllPartiasTabControl.Size = new Size(1163, 438);
            AllPartiasTabControl.TabIndex = 0;
            AllPartiasTabControl.Text = "Полный расчёт";
            // 
            // splitContainer1
            // 
            splitContainer1.BackColor = Color.White;
            splitContainer1.BorderStyle = BorderStyle.FixedSingle;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(3, 3);
            splitContainer1.Margin = new Padding(0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(AllForAllPartiasGuidComboBox);
            splitContainer1.Panel1.Controls.Add(AllForAllOrdersComboBox);
            splitContainer1.Panel1.Controls.Add(label1);
            splitContainer1.Panel1.Controls.Add(LogPartiasCheckBox);
            splitContainer1.Panel1.Controls.Add(AllForAllTextBox);
            splitContainer1.Panel1.Controls.Add(label2);
            splitContainer1.Panel1.Controls.Add(PartiasProgressLabel);
            splitContainer1.Panel1.Controls.Add(StartAlgorithmPButton);
            splitContainer1.Panel1.Controls.Add(ZadelCheckBox);
            splitContainer1.Panel1.Controls.Add(PartCheckBox);
            splitContainer1.Panel1.Controls.Add(PartiasProgressBar);
            splitContainer1.Panel1MinSize = 400;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer3);
            splitContainer1.Size = new Size(1157, 432);
            splitContainer1.SplitterDistance = 400;
            splitContainer1.TabIndex = 23;
            // 
            // AllForAllPartiasGuidComboBox
            // 
            AllForAllPartiasGuidComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            AllForAllPartiasGuidComboBox.FormattingEnabled = true;
            AllForAllPartiasGuidComboBox.Location = new Point(130, 143);
            AllForAllPartiasGuidComboBox.Name = "AllForAllPartiasGuidComboBox";
            AllForAllPartiasGuidComboBox.Size = new Size(265, 24);
            AllForAllPartiasGuidComboBox.TabIndex = 31;
            AllForAllPartiasGuidComboBox.SelectedIndexChanged += AllForAllPartiasGuidComboBox_SelectedIndexChanged;
            // 
            // AllForAllOrdersComboBox
            // 
            AllForAllOrdersComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            AllForAllOrdersComboBox.FormattingEnabled = true;
            AllForAllOrdersComboBox.Location = new Point(91, 112);
            AllForAllOrdersComboBox.Name = "AllForAllOrdersComboBox";
            AllForAllOrdersComboBox.Size = new Size(304, 24);
            AllForAllOrdersComboBox.TabIndex = 30;
            AllForAllOrdersComboBox.SelectedIndexChanged += AllForAllOrdersComboBox_SelectedIndexChanged;
            AllForAllOrdersComboBox.TextChanged += AllForAllOrdersComboBox_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(5, 115);
            label1.Name = "label1";
            label1.Size = new Size(63, 17);
            label1.TabIndex = 29;
            label1.Text = "Фильтр:";
            // 
            // LogPartiasCheckBox
            // 
            LogPartiasCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LogPartiasCheckBox.AutoSize = true;
            LogPartiasCheckBox.BackColor = Color.Black;
            LogPartiasCheckBox.Depth = 0;
            LogPartiasCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            LogPartiasCheckBox.ForeColor = Color.CornflowerBlue;
            LogPartiasCheckBox.Location = new Point(311, 63);
            LogPartiasCheckBox.Margin = new Padding(0);
            LogPartiasCheckBox.MouseLocation = new Point(-1, -1);
            LogPartiasCheckBox.Name = "LogPartiasCheckBox";
            LogPartiasCheckBox.Ripple = true;
            LogPartiasCheckBox.Size = new Size(53, 30);
            LogPartiasCheckBox.TabIndex = 28;
            LogPartiasCheckBox.Text = "Log";
            LogPartiasCheckBox.UseVisualStyleBackColor = false;
            // 
            // AllForAllTextBox
            // 
            AllForAllTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            AllForAllTextBox.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            AllForAllTextBox.Location = new Point(0, 177);
            AllForAllTextBox.Multiline = true;
            AllForAllTextBox.Name = "AllForAllTextBox";
            AllForAllTextBox.ScrollBars = ScrollBars.Both;
            AllForAllTextBox.Size = new Size(398, 253);
            AllForAllTextBox.TabIndex = 27;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 146);
            label2.Name = "label2";
            label2.Size = new Size(97, 17);
            label2.TabIndex = 26;
            label2.Text = "GUID партии:";
            // 
            // PartiasProgressLabel
            // 
            PartiasProgressLabel.AutoSize = true;
            PartiasProgressLabel.Location = new Point(3, 89);
            PartiasProgressLabel.Name = "PartiasProgressLabel";
            PartiasProgressLabel.Size = new Size(219, 17);
            PartiasProgressLabel.TabIndex = 25;
            PartiasProgressLabel.Text = "Расчёт недоступен без данных.";
            // 
            // StartAlgorithmPButton
            // 
            StartAlgorithmPButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            StartAlgorithmPButton.BackColor = Color.CornflowerBlue;
            StartAlgorithmPButton.BackgroundImageLayout = ImageLayout.Zoom;
            StartAlgorithmPButton.Cursor = Cursors.Hand;
            StartAlgorithmPButton.Enabled = false;
            StartAlgorithmPButton.FlatAppearance.BorderSize = 0;
            StartAlgorithmPButton.FlatStyle = FlatStyle.Flat;
            StartAlgorithmPButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            StartAlgorithmPButton.ForeColor = Color.White;
            StartAlgorithmPButton.Location = new Point(3, 3);
            StartAlgorithmPButton.Name = "StartAlgorithmPButton";
            StartAlgorithmPButton.Size = new Size(297, 51);
            StartAlgorithmPButton.TabIndex = 21;
            StartAlgorithmPButton.Text = "Расчёт";
            StartAlgorithmPButton.UseVisualStyleBackColor = false;
            StartAlgorithmPButton.Click += StartAlgorithmPButton_Click;
            // 
            // ZadelCheckBox
            // 
            ZadelCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ZadelCheckBox.AutoSize = true;
            ZadelCheckBox.BackColor = Color.Black;
            ZadelCheckBox.Depth = 0;
            ZadelCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            ZadelCheckBox.ForeColor = Color.CornflowerBlue;
            ZadelCheckBox.Location = new Point(311, 33);
            ZadelCheckBox.Margin = new Padding(0);
            ZadelCheckBox.MouseLocation = new Point(-1, -1);
            ZadelCheckBox.Name = "ZadelCheckBox";
            ZadelCheckBox.Ripple = true;
            ZadelCheckBox.Size = new Size(64, 30);
            ZadelCheckBox.TabIndex = 22;
            ZadelCheckBox.Text = "Zadel";
            ZadelCheckBox.UseVisualStyleBackColor = false;
            // 
            // PartCheckBox
            // 
            PartCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PartCheckBox.AutoSize = true;
            PartCheckBox.BackColor = Color.Black;
            PartCheckBox.Depth = 0;
            PartCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            PartCheckBox.ForeColor = Color.CornflowerBlue;
            PartCheckBox.Location = new Point(311, 3);
            PartCheckBox.Margin = new Padding(0);
            PartCheckBox.MouseLocation = new Point(-1, -1);
            PartCheckBox.Name = "PartCheckBox";
            PartCheckBox.Ripple = true;
            PartCheckBox.Size = new Size(55, 30);
            PartCheckBox.TabIndex = 20;
            PartCheckBox.Text = "Part";
            PartCheckBox.UseVisualStyleBackColor = false;
            // 
            // PartiasProgressBar
            // 
            PartiasProgressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PartiasProgressBar.BackColor = SystemColors.ControlLight;
            PartiasProgressBar.BarPadding = new Padding(0);
            PartiasProgressBar.Font = new Font("Microsoft Sans Serif", 26F, FontStyle.Bold, GraphicsUnit.Pixel);
            PartiasProgressBar.ForeColor = Color.CornflowerBlue;
            PartiasProgressBar.IsInfinite = false;
            PartiasProgressBar.Location = new Point(3, 60);
            PartiasProgressBar.Maximum = 100;
            PartiasProgressBar.Minimum = 0;
            PartiasProgressBar.Name = "PartiasProgressBar";
            PartiasProgressBar.Size = new Size(297, 26);
            PartiasProgressBar.TabIndex = 16;
            PartiasProgressBar.Text = "PartiasProgressBar";
            PartiasProgressBar.Value = 0;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Orientation = Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(CustomTreeViewOnPPanel);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(PartiasDataTable);
            splitContainer3.Size = new Size(751, 430);
            splitContainer3.SplitterDistance = 248;
            splitContainer3.TabIndex = 0;
            // 
            // CustomTreeViewOnPPanel
            // 
            CustomTreeViewOnPPanel.Dock = DockStyle.Fill;
            CustomTreeViewOnPPanel.Location = new Point(0, 0);
            CustomTreeViewOnPPanel.Name = "CustomTreeViewOnPPanel";
            CustomTreeViewOnPPanel.Size = new Size(751, 248);
            CustomTreeViewOnPPanel.TabIndex = 1;
            // 
            // PartiasDataTable
            // 
            PartiasDataTable.AllowUserToAddRows = false;
            PartiasDataTable.AllowUserToDeleteRows = false;
            PartiasDataTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            PartiasDataTable.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3, Column4, Column5 });
            PartiasDataTable.Dock = DockStyle.Fill;
            PartiasDataTable.Location = new Point(0, 0);
            PartiasDataTable.Name = "PartiasDataTable";
            PartiasDataTable.RowHeadersWidth = 51;
            PartiasDataTable.Size = new Size(751, 178);
            PartiasDataTable.TabIndex = 0;
            // 
            // Column1
            // 
            Column1.HeaderText = "Заказ";
            Column1.MinimumWidth = 6;
            Column1.Name = "Column1";
            Column1.Width = 125;
            // 
            // Column2
            // 
            Column2.HeaderText = "ГП";
            Column2.MinimumWidth = 6;
            Column2.Name = "Column2";
            Column2.Width = 125;
            // 
            // Column3
            // 
            Column3.HeaderText = "Продукция";
            Column3.MinimumWidth = 6;
            Column3.Name = "Column3";
            Column3.Width = 125;
            // 
            // Column4
            // 
            Column4.HeaderText = "Количество";
            Column4.MinimumWidth = 6;
            Column4.Name = "Column4";
            Column4.Width = 125;
            // 
            // Column5
            // 
            Column5.HeaderText = "Дата выполнения";
            Column5.MinimumWidth = 6;
            Column5.Name = "Column5";
            Column5.Width = 125;
            // 
            // HandPartiasTabControl
            // 
            HandPartiasTabControl.BackColor = Color.White;
            HandPartiasTabControl.Controls.Add(InGPCheckBox);
            HandPartiasTabControl.Controls.Add(splitContainer4);
            HandPartiasTabControl.Controls.Add(label8);
            HandPartiasTabControl.Controls.Add(label7);
            HandPartiasTabControl.Controls.Add(label6);
            HandPartiasTabControl.Controls.Add(label5);
            HandPartiasTabControl.Controls.Add(label4);
            HandPartiasTabControl.Controls.Add(label3);
            HandPartiasTabControl.Controls.Add(TwoPartiasGP2ComboBox);
            HandPartiasTabControl.Controls.Add(TwoPartiasGP1ComboBox);
            HandPartiasTabControl.Controls.Add(TwoPartiasOrder2ComboBox);
            HandPartiasTabControl.Controls.Add(TwoPartiasOrder1ComboBox);
            HandPartiasTabControl.Controls.Add(TwoPartiasPartia2ComboBox);
            HandPartiasTabControl.Controls.Add(TwoPartiasStartButton);
            HandPartiasTabControl.Controls.Add(TwoPartiasPartia1ComboBox);
            HandPartiasTabControl.Location = new Point(4, 24);
            HandPartiasTabControl.Name = "HandPartiasTabControl";
            HandPartiasTabControl.Padding = new Padding(3);
            HandPartiasTabControl.Size = new Size(1163, 439);
            HandPartiasTabControl.TabIndex = 1;
            HandPartiasTabControl.Text = "Поштучное объединение";
            // 
            // InGPCheckBox
            // 
            InGPCheckBox.AutoSize = true;
            InGPCheckBox.BackColor = Color.Black;
            InGPCheckBox.Depth = 0;
            InGPCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            InGPCheckBox.ForeColor = Color.CornflowerBlue;
            InGPCheckBox.Location = new Point(1018, 41);
            InGPCheckBox.Margin = new Padding(0);
            InGPCheckBox.MouseLocation = new Point(-1, -1);
            InGPCheckBox.Name = "InGPCheckBox";
            InGPCheckBox.Ripple = true;
            InGPCheckBox.Size = new Size(97, 30);
            InGPCheckBox.TabIndex = 45;
            InGPCheckBox.Text = "Внутри ГП";
            InGPCheckBox.UseVisualStyleBackColor = false;
            // 
            // splitContainer4
            // 
            splitContainer4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer4.Location = new Point(3, 74);
            splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            splitContainer4.Panel1.Controls.Add(TwoPartiasTextBox);
            // 
            // splitContainer4.Panel2
            // 
            splitContainer4.Panel2.Controls.Add(tabControl1);
            splitContainer4.Size = new Size(1157, 362);
            splitContainer4.SplitterDistance = 385;
            splitContainer4.TabIndex = 44;
            // 
            // TwoPartiasTextBox
            // 
            TwoPartiasTextBox.Dock = DockStyle.Fill;
            TwoPartiasTextBox.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            TwoPartiasTextBox.Location = new Point(0, 0);
            TwoPartiasTextBox.Multiline = true;
            TwoPartiasTextBox.Name = "TwoPartiasTextBox";
            TwoPartiasTextBox.ScrollBars = ScrollBars.Both;
            TwoPartiasTextBox.Size = new Size(385, 362);
            TwoPartiasTextBox.TabIndex = 28;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(768, 362);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(TwoPartiasPartia1View);
            tabPage1.Location = new Point(4, 25);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(760, 333);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Партия 1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // TwoPartiasPartia1View
            // 
            TwoPartiasPartia1View.BackColor = Color.White;
            TwoPartiasPartia1View.Dock = DockStyle.Fill;
            TwoPartiasPartia1View.Location = new Point(0, 0);
            TwoPartiasPartia1View.Margin = new Padding(0);
            TwoPartiasPartia1View.Name = "TwoPartiasPartia1View";
            TwoPartiasPartia1View.Size = new Size(760, 333);
            TwoPartiasPartia1View.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(TwoPartiasPartia2View);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new Size(760, 334);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Партия 2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // TwoPartiasPartia2View
            // 
            TwoPartiasPartia2View.BackColor = Color.White;
            TwoPartiasPartia2View.Dock = DockStyle.Fill;
            TwoPartiasPartia2View.Location = new Point(0, 0);
            TwoPartiasPartia2View.Margin = new Padding(0);
            TwoPartiasPartia2View.Name = "TwoPartiasPartia2View";
            TwoPartiasPartia2View.Size = new Size(760, 334);
            TwoPartiasPartia2View.TabIndex = 0;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(TwoPartiasPartiaResView);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(760, 334);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Результат";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // TwoPartiasPartiaResView
            // 
            TwoPartiasPartiaResView.BackColor = Color.White;
            TwoPartiasPartiaResView.Dock = DockStyle.Fill;
            TwoPartiasPartiaResView.Location = new Point(0, 0);
            TwoPartiasPartiaResView.Margin = new Padding(0);
            TwoPartiasPartiaResView.Name = "TwoPartiasPartiaResView";
            TwoPartiasPartiaResView.Size = new Size(760, 334);
            TwoPartiasPartiaResView.TabIndex = 1;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(867, 43);
            label8.Name = "label8";
            label8.Size = new Size(42, 17);
            label8.TabIndex = 43;
            label8.Text = "ГП 2:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(867, 9);
            label7.Name = "label7";
            label7.Size = new Size(42, 17);
            label7.TabIndex = 42;
            label7.Text = "ГП 1:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(464, 43);
            label6.Name = "label6";
            label6.Size = new Size(63, 17);
            label6.TabIndex = 41;
            label6.Text = "Заказ 2:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(464, 9);
            label5.Name = "label5";
            label5.Size = new Size(63, 17);
            label5.TabIndex = 40;
            label5.Text = "Заказ 1:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 43);
            label4.Name = "label4";
            label4.Size = new Size(109, 17);
            label4.TabIndex = 39;
            label4.Text = "GUID партии 2:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 9);
            label3.Name = "label3";
            label3.Size = new Size(109, 17);
            label3.TabIndex = 38;
            label3.Text = "GUID партии 1:";
            // 
            // TwoPartiasGP2ComboBox
            // 
            TwoPartiasGP2ComboBox.FormattingEnabled = true;
            TwoPartiasGP2ComboBox.Location = new Point(924, 40);
            TwoPartiasGP2ComboBox.Name = "TwoPartiasGP2ComboBox";
            TwoPartiasGP2ComboBox.Size = new Size(79, 24);
            TwoPartiasGP2ComboBox.TabIndex = 37;
            // 
            // TwoPartiasGP1ComboBox
            // 
            TwoPartiasGP1ComboBox.FormattingEnabled = true;
            TwoPartiasGP1ComboBox.Location = new Point(924, 6);
            TwoPartiasGP1ComboBox.Name = "TwoPartiasGP1ComboBox";
            TwoPartiasGP1ComboBox.Size = new Size(79, 24);
            TwoPartiasGP1ComboBox.TabIndex = 36;
            // 
            // TwoPartiasOrder2ComboBox
            // 
            TwoPartiasOrder2ComboBox.FormattingEnabled = true;
            TwoPartiasOrder2ComboBox.Location = new Point(548, 40);
            TwoPartiasOrder2ComboBox.Name = "TwoPartiasOrder2ComboBox";
            TwoPartiasOrder2ComboBox.Size = new Size(304, 24);
            TwoPartiasOrder2ComboBox.TabIndex = 35;
            TwoPartiasOrder2ComboBox.SelectedIndexChanged += TwoPartiasOrder2ComboBox_SelectedIndexChanged;
            // 
            // TwoPartiasOrder1ComboBox
            // 
            TwoPartiasOrder1ComboBox.FormattingEnabled = true;
            TwoPartiasOrder1ComboBox.Location = new Point(548, 6);
            TwoPartiasOrder1ComboBox.Name = "TwoPartiasOrder1ComboBox";
            TwoPartiasOrder1ComboBox.Size = new Size(304, 24);
            TwoPartiasOrder1ComboBox.TabIndex = 34;
            TwoPartiasOrder1ComboBox.SelectedIndexChanged += TwoPartiasOrder1ComboBox_SelectedIndexChanged;
            // 
            // TwoPartiasPartia2ComboBox
            // 
            TwoPartiasPartia2ComboBox.FormattingEnabled = true;
            TwoPartiasPartia2ComboBox.Location = new Point(147, 40);
            TwoPartiasPartia2ComboBox.Name = "TwoPartiasPartia2ComboBox";
            TwoPartiasPartia2ComboBox.Size = new Size(304, 24);
            TwoPartiasPartia2ComboBox.TabIndex = 33;
            // 
            // TwoPartiasStartButton
            // 
            TwoPartiasStartButton.BackColor = Color.CornflowerBlue;
            TwoPartiasStartButton.BackgroundImageLayout = ImageLayout.Zoom;
            TwoPartiasStartButton.Cursor = Cursors.Hand;
            TwoPartiasStartButton.Enabled = false;
            TwoPartiasStartButton.FlatAppearance.BorderSize = 0;
            TwoPartiasStartButton.FlatStyle = FlatStyle.Flat;
            TwoPartiasStartButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            TwoPartiasStartButton.ForeColor = Color.White;
            TwoPartiasStartButton.Location = new Point(1018, 6);
            TwoPartiasStartButton.Name = "TwoPartiasStartButton";
            TwoPartiasStartButton.Size = new Size(139, 32);
            TwoPartiasStartButton.TabIndex = 32;
            TwoPartiasStartButton.Text = "Расчёт";
            TwoPartiasStartButton.UseVisualStyleBackColor = false;
            TwoPartiasStartButton.Click += TwoPartiasStartButton_Click;
            // 
            // TwoPartiasPartia1ComboBox
            // 
            TwoPartiasPartia1ComboBox.FormattingEnabled = true;
            TwoPartiasPartia1ComboBox.Location = new Point(147, 6);
            TwoPartiasPartia1ComboBox.Name = "TwoPartiasPartia1ComboBox";
            TwoPartiasPartia1ComboBox.Size = new Size(304, 24);
            TwoPartiasPartia1ComboBox.TabIndex = 31;
            // 
            // materialTabSelector2
            // 
            materialTabSelector2.BackColor = Color.CornflowerBlue;
            materialTabSelector2.BaseTabControl = PartiasTabControl;
            materialTabSelector2.Dock = DockStyle.Top;
            materialTabSelector2.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            materialTabSelector2.ForeColor = Color.White;
            materialTabSelector2.Location = new Point(0, 0);
            materialTabSelector2.Margin = new Padding(0);
            materialTabSelector2.Name = "materialTabSelector2";
            materialTabSelector2.Size = new Size(1171, 35);
            materialTabSelector2.TabIndex = 22;
            materialTabSelector2.Text = "materialTabSelector2";
            // 
            // PlanningTabPage
            // 
            PlanningTabPage.Controls.Add(PlanningTabControl);
            PlanningTabPage.Controls.Add(materialTabSelector3);
            PlanningTabPage.Location = new Point(4, 24);
            PlanningTabPage.Name = "PlanningTabPage";
            PlanningTabPage.Size = new Size(1171, 502);
            PlanningTabPage.TabIndex = 1;
            PlanningTabPage.Text = "Планирование производства";
            PlanningTabPage.UseVisualStyleBackColor = true;
            // 
            // PlanningTabControl
            // 
            PlanningTabControl.Controls.Add(TyrantTabPage);
            PlanningTabControl.Controls.Add(HandPlanningPage);
            PlanningTabControl.Controls.Add(GraphsTabPage);
            PlanningTabControl.Dock = DockStyle.Fill;
            PlanningTabControl.Location = new Point(0, 35);
            PlanningTabControl.Name = "PlanningTabControl";
            PlanningTabControl.SelectedIndex = 0;
            PlanningTabControl.Size = new Size(1171, 467);
            PlanningTabControl.TabIndex = 0;
            // 
            // TyrantTabPage
            // 
            TyrantTabPage.Controls.Add(splitContainer5);
            TyrantTabPage.Location = new Point(4, 25);
            TyrantTabPage.Name = "TyrantTabPage";
            TyrantTabPage.Padding = new Padding(3);
            TyrantTabPage.Size = new Size(1163, 438);
            TyrantTabPage.TabIndex = 0;
            TyrantTabPage.Text = "Полное планирование";
            TyrantTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer5
            // 
            splitContainer5.Dock = DockStyle.Fill;
            splitContainer5.Location = new Point(3, 3);
            splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            splitContainer5.Panel1.Controls.Add(splitContainer7);
            splitContainer5.Panel1MinSize = 370;
            // 
            // splitContainer5.Panel2
            // 
            splitContainer5.Panel2.Controls.Add(splitContainer6);
            splitContainer5.Panel2.Controls.Add(panel1);
            splitContainer5.Size = new Size(1157, 432);
            splitContainer5.SplitterDistance = 385;
            splitContainer5.TabIndex = 0;
            // 
            // splitContainer7
            // 
            splitContainer7.Dock = DockStyle.Fill;
            splitContainer7.Location = new Point(0, 0);
            splitContainer7.Name = "splitContainer7";
            splitContainer7.Orientation = Orientation.Horizontal;
            // 
            // splitContainer7.Panel1
            // 
            splitContainer7.Panel1.BackColor = Color.White;
            splitContainer7.Panel1.Controls.Add(StagesColorCheckBox);
            splitContainer7.Panel1.Controls.Add(ModsColorCheckBox);
            splitContainer7.Panel1.Controls.Add(StopPlanningButton);
            splitContainer7.Panel1.Controls.Add(OutputButton);
            splitContainer7.Panel1.Controls.Add(PlanningProcessLabel);
            splitContainer7.Panel1.Controls.Add(LaunchPlanningButton);
            splitContainer7.Panel1.Controls.Add(PlanningProcessBar);
            // 
            // splitContainer7.Panel2
            // 
            splitContainer7.Panel2.Controls.Add(splitContainer8);
            splitContainer7.Size = new Size(385, 432);
            splitContainer7.SplitterDistance = 124;
            splitContainer7.TabIndex = 0;
            // 
            // StagesColorCheckBox
            // 
            StagesColorCheckBox.AutoSize = true;
            StagesColorCheckBox.BackColor = Color.Black;
            StagesColorCheckBox.Depth = 0;
            StagesColorCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            StagesColorCheckBox.ForeColor = Color.CornflowerBlue;
            StagesColorCheckBox.Location = new Point(178, 106);
            StagesColorCheckBox.Margin = new Padding(0);
            StagesColorCheckBox.MouseLocation = new Point(-1, -1);
            StagesColorCheckBox.Name = "StagesColorCheckBox";
            StagesColorCheckBox.Ripple = true;
            StagesColorCheckBox.Size = new Size(116, 30);
            StagesColorCheckBox.TabIndex = 37;
            StagesColorCheckBox.Text = "Цвета этапов";
            StagesColorCheckBox.UseVisualStyleBackColor = false;
            StagesColorCheckBox.CheckedChanged += StagesColorCheckBox_CheckedChanged;
            // 
            // ModsColorCheckBox
            // 
            ModsColorCheckBox.AutoSize = true;
            ModsColorCheckBox.BackColor = Color.Black;
            ModsColorCheckBox.Depth = 0;
            ModsColorCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            ModsColorCheckBox.ForeColor = Color.CornflowerBlue;
            ModsColorCheckBox.Location = new Point(3, 106);
            ModsColorCheckBox.Margin = new Padding(0);
            ModsColorCheckBox.MouseLocation = new Point(-1, -1);
            ModsColorCheckBox.Name = "ModsColorCheckBox";
            ModsColorCheckBox.Ripple = true;
            ModsColorCheckBox.Size = new Size(175, 30);
            ModsColorCheckBox.TabIndex = 36;
            ModsColorCheckBox.Text = "Цвета моделирований";
            ModsColorCheckBox.UseVisualStyleBackColor = false;
            ModsColorCheckBox.CheckedChanged += ModsColorCheckBox_CheckedChanged;
            // 
            // StopPlanningButton
            // 
            StopPlanningButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            StopPlanningButton.BackColor = Color.CornflowerBlue;
            StopPlanningButton.BackgroundImageLayout = ImageLayout.Zoom;
            StopPlanningButton.Cursor = Cursors.Hand;
            StopPlanningButton.Enabled = false;
            StopPlanningButton.FlatAppearance.BorderSize = 0;
            StopPlanningButton.FlatStyle = FlatStyle.Flat;
            StopPlanningButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            StopPlanningButton.ForeColor = Color.White;
            StopPlanningButton.Location = new Point(261, 3);
            StopPlanningButton.Name = "StopPlanningButton";
            StopPlanningButton.Size = new Size(121, 51);
            StopPlanningButton.TabIndex = 30;
            StopPlanningButton.Text = "Стоп";
            StopPlanningButton.UseVisualStyleBackColor = false;
            StopPlanningButton.Click += StopPlanningButton_Click;
            // 
            // OutputButton
            // 
            OutputButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            OutputButton.BackColor = Color.CornflowerBlue;
            OutputButton.BackgroundImageLayout = ImageLayout.Zoom;
            OutputButton.Cursor = Cursors.Hand;
            OutputButton.FlatAppearance.BorderSize = 0;
            OutputButton.FlatStyle = FlatStyle.Flat;
            OutputButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            OutputButton.ForeColor = Color.White;
            OutputButton.Location = new Point(130, 3);
            OutputButton.Name = "OutputButton";
            OutputButton.Size = new Size(125, 51);
            OutputButton.TabIndex = 29;
            OutputButton.Text = "Вывод";
            OutputButton.UseVisualStyleBackColor = false;
            OutputButton.Click += OutputButton_Click;
            // 
            // PlanningProcessLabel
            // 
            PlanningProcessLabel.AutoSize = true;
            PlanningProcessLabel.Location = new Point(3, 89);
            PlanningProcessLabel.Name = "PlanningProcessLabel";
            PlanningProcessLabel.Size = new Size(219, 17);
            PlanningProcessLabel.TabIndex = 28;
            PlanningProcessLabel.Text = "Расчёт недоступен без данных.";
            // 
            // LaunchPlanningButton
            // 
            LaunchPlanningButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LaunchPlanningButton.BackColor = Color.CornflowerBlue;
            LaunchPlanningButton.BackgroundImageLayout = ImageLayout.Zoom;
            LaunchPlanningButton.Cursor = Cursors.Hand;
            LaunchPlanningButton.Enabled = false;
            LaunchPlanningButton.FlatAppearance.BorderSize = 0;
            LaunchPlanningButton.FlatStyle = FlatStyle.Flat;
            LaunchPlanningButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            LaunchPlanningButton.ForeColor = Color.White;
            LaunchPlanningButton.Location = new Point(3, 3);
            LaunchPlanningButton.Name = "LaunchPlanningButton";
            LaunchPlanningButton.Size = new Size(121, 51);
            LaunchPlanningButton.TabIndex = 27;
            LaunchPlanningButton.Text = "Расчёт";
            LaunchPlanningButton.UseVisualStyleBackColor = false;
            LaunchPlanningButton.Click += LaunchPlanningButton_Click;
            // 
            // PlanningProcessBar
            // 
            PlanningProcessBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            PlanningProcessBar.BackColor = SystemColors.ControlLight;
            PlanningProcessBar.BarPadding = new Padding(0);
            PlanningProcessBar.Font = new Font("Microsoft Sans Serif", 26F, FontStyle.Bold, GraphicsUnit.Pixel);
            PlanningProcessBar.ForeColor = Color.CornflowerBlue;
            PlanningProcessBar.IsInfinite = false;
            PlanningProcessBar.Location = new Point(3, 60);
            PlanningProcessBar.Maximum = 100;
            PlanningProcessBar.Minimum = 0;
            PlanningProcessBar.Name = "PlanningProcessBar";
            PlanningProcessBar.Size = new Size(379, 26);
            PlanningProcessBar.TabIndex = 26;
            PlanningProcessBar.Text = "customProgressBar1";
            PlanningProcessBar.Value = 0;
            // 
            // splitContainer8
            // 
            splitContainer8.Dock = DockStyle.Fill;
            splitContainer8.Location = new Point(0, 0);
            splitContainer8.Name = "splitContainer8";
            splitContainer8.Orientation = Orientation.Horizontal;
            // 
            // splitContainer8.Panel1
            // 
            splitContainer8.Panel1.Controls.Add(PlanningDataTable);
            // 
            // splitContainer8.Panel2
            // 
            splitContainer8.Panel2.Controls.Add(BarDescTextBox);
            splitContainer8.Size = new Size(385, 304);
            splitContainer8.SplitterDistance = 123;
            splitContainer8.TabIndex = 0;
            // 
            // PlanningDataTable
            // 
            PlanningDataTable.AllowUserToAddRows = false;
            PlanningDataTable.AllowUserToDeleteRows = false;
            PlanningDataTable.BorderStyle = BorderStyle.None;
            PlanningDataTable.CellBorderStyle = DataGridViewCellBorderStyle.None;
            PlanningDataTable.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            PlanningDataTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            PlanningDataTable.Columns.AddRange(new DataGridViewColumn[] { Column6, Column7, Column8, Column9 });
            PlanningDataTable.Dock = DockStyle.Fill;
            PlanningDataTable.EditMode = DataGridViewEditMode.EditProgrammatically;
            PlanningDataTable.Location = new Point(0, 0);
            PlanningDataTable.Name = "PlanningDataTable";
            PlanningDataTable.ReadOnly = true;
            PlanningDataTable.RowHeadersWidth = 51;
            PlanningDataTable.Size = new Size(385, 123);
            PlanningDataTable.TabIndex = 0;
            // 
            // Column6
            // 
            Column6.HeaderText = "Критерий";
            Column6.MinimumWidth = 6;
            Column6.Name = "Column6";
            Column6.ReadOnly = true;
            Column6.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column6.Width = 125;
            // 
            // Column7
            // 
            Column7.HeaderText = "Ориг. знач.";
            Column7.MinimumWidth = 6;
            Column7.Name = "Column7";
            Column7.ReadOnly = true;
            Column7.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column7.Width = 125;
            // 
            // Column8
            // 
            Column8.HeaderText = "Лучш. знач.";
            Column8.MinimumWidth = 6;
            Column8.Name = "Column8";
            Column8.ReadOnly = true;
            Column8.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column8.Width = 125;
            // 
            // Column9
            // 
            Column9.HeaderText = "%";
            Column9.MinimumWidth = 6;
            Column9.Name = "Column9";
            Column9.ReadOnly = true;
            Column9.SortMode = DataGridViewColumnSortMode.NotSortable;
            Column9.Width = 125;
            // 
            // BarDescTextBox
            // 
            BarDescTextBox.Dock = DockStyle.Fill;
            BarDescTextBox.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            BarDescTextBox.Location = new Point(0, 0);
            BarDescTextBox.Multiline = true;
            BarDescTextBox.Name = "BarDescTextBox";
            BarDescTextBox.ScrollBars = ScrollBars.Both;
            BarDescTextBox.Size = new Size(385, 177);
            BarDescTextBox.TabIndex = 0;
            // 
            // splitContainer6
            // 
            splitContainer6.Dock = DockStyle.Fill;
            splitContainer6.Location = new Point(0, 30);
            splitContainer6.Name = "splitContainer6";
            splitContainer6.Orientation = Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            splitContainer6.Panel1.Controls.Add(ganttOLD);
            splitContainer6.Panel1.Controls.Add(vScrollBarOLD);
            splitContainer6.Panel1MinSize = 10;
            // 
            // splitContainer6.Panel2
            // 
            splitContainer6.Panel2.Controls.Add(ganttNEW);
            splitContainer6.Panel2.Controls.Add(vScrollBarNEW);
            splitContainer6.Panel2MinSize = 10;
            splitContainer6.Size = new Size(768, 402);
            splitContainer6.SplitterDistance = 218;
            splitContainer6.TabIndex = 1;
            // 
            // ganttOLD
            // 
            ganttOLD.BackColor = Color.White;
            ganttOLD.Dock = DockStyle.Fill;
            ganttOLD.Location = new Point(0, 0);
            ganttOLD.Name = "ganttOLD";
            ganttOLD.Size = new Size(742, 218);
            ganttOLD.TabIndex = 0;
            ganttOLD.TimelineHeight = 50;
            ganttOLD.TitleRowHeight = -1;
            ganttOLD.TitleRowWidth = 200;
            // 
            // vScrollBarOLD
            // 
            vScrollBarOLD.Dock = DockStyle.Right;
            vScrollBarOLD.Location = new Point(742, 0);
            vScrollBarOLD.Name = "vScrollBarOLD";
            vScrollBarOLD.Size = new Size(26, 218);
            vScrollBarOLD.TabIndex = 1;
            // 
            // ganttNEW
            // 
            ganttNEW.BackColor = Color.White;
            ganttNEW.Dock = DockStyle.Fill;
            ganttNEW.Location = new Point(0, 0);
            ganttNEW.Name = "ganttNEW";
            ganttNEW.Size = new Size(742, 180);
            ganttNEW.TabIndex = 0;
            ganttNEW.TimelineHeight = 50;
            ganttNEW.TitleRowHeight = -1;
            ganttNEW.TitleRowWidth = 200;
            // 
            // vScrollBarNEW
            // 
            vScrollBarNEW.Dock = DockStyle.Right;
            vScrollBarNEW.Location = new Point(742, 0);
            vScrollBarNEW.Name = "vScrollBarNEW";
            vScrollBarNEW.Size = new Size(26, 180);
            vScrollBarNEW.TabIndex = 1;
            // 
            // panel1
            // 
            panel1.Controls.Add(label11);
            panel1.Controls.Add(dateTimePickerTo);
            panel1.Controls.Add(label10);
            panel1.Controls.Add(dateTimePickerFrom);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(768, 30);
            panel1.TabIndex = 0;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(258, 6);
            label11.Name = "label11";
            label11.Size = new Size(31, 17);
            label11.TabIndex = 33;
            label11.Text = "До:";
            // 
            // dateTimePickerTo
            // 
            dateTimePickerTo.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            dateTimePickerTo.Format = DateTimePickerFormat.Custom;
            dateTimePickerTo.Location = new Point(301, 2);
            dateTimePickerTo.Name = "dateTimePickerTo";
            dateTimePickerTo.Size = new Size(209, 23);
            dateTimePickerTo.TabIndex = 32;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(3, 7);
            label10.Name = "label10";
            label10.Size = new Size(30, 17);
            label10.TabIndex = 31;
            label10.Text = "От:";
            // 
            // dateTimePickerFrom
            // 
            dateTimePickerFrom.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            dateTimePickerFrom.Format = DateTimePickerFormat.Custom;
            dateTimePickerFrom.Location = new Point(46, 3);
            dateTimePickerFrom.Name = "dateTimePickerFrom";
            dateTimePickerFrom.Size = new Size(209, 23);
            dateTimePickerFrom.TabIndex = 0;
            // 
            // HandPlanningPage
            // 
            HandPlanningPage.Controls.Add(splitContainer17);
            HandPlanningPage.Location = new Point(4, 24);
            HandPlanningPage.Name = "HandPlanningPage";
            HandPlanningPage.Padding = new Padding(3);
            HandPlanningPage.Size = new Size(1163, 439);
            HandPlanningPage.TabIndex = 1;
            HandPlanningPage.Text = "Ручной анализ плана";
            HandPlanningPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer17
            // 
            splitContainer17.Dock = DockStyle.Fill;
            splitContainer17.Location = new Point(3, 3);
            splitContainer17.Name = "splitContainer17";
            // 
            // splitContainer17.Panel1
            // 
            splitContainer17.Panel1.Controls.Add(splitContainer18);
            splitContainer17.Panel1.Controls.Add(panel3);
            // 
            // splitContainer17.Panel2
            // 
            splitContainer17.Panel2.Controls.Add(ganttANL);
            splitContainer17.Panel2.Controls.Add(vScrollBarANL);
            splitContainer17.Size = new Size(1157, 433);
            splitContainer17.SplitterDistance = 385;
            splitContainer17.TabIndex = 0;
            // 
            // splitContainer18
            // 
            splitContainer18.Dock = DockStyle.Fill;
            splitContainer18.Location = new Point(0, 68);
            splitContainer18.Name = "splitContainer18";
            splitContainer18.Orientation = Orientation.Horizontal;
            // 
            // splitContainer18.Panel1
            // 
            splitContainer18.Panel1.Controls.Add(ManualPlanningDataTable);
            // 
            // splitContainer18.Panel2
            // 
            splitContainer18.Panel2.Controls.Add(AnlDescTextBox);
            splitContainer18.Size = new Size(385, 365);
            splitContainer18.SplitterDistance = 147;
            splitContainer18.TabIndex = 38;
            // 
            // ManualPlanningDataTable
            // 
            ManualPlanningDataTable.AllowUserToAddRows = false;
            ManualPlanningDataTable.AllowUserToDeleteRows = false;
            ManualPlanningDataTable.BorderStyle = BorderStyle.None;
            ManualPlanningDataTable.CellBorderStyle = DataGridViewCellBorderStyle.None;
            ManualPlanningDataTable.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            ManualPlanningDataTable.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ManualPlanningDataTable.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4 });
            ManualPlanningDataTable.Dock = DockStyle.Fill;
            ManualPlanningDataTable.EditMode = DataGridViewEditMode.EditProgrammatically;
            ManualPlanningDataTable.Location = new Point(0, 0);
            ManualPlanningDataTable.Name = "ManualPlanningDataTable";
            ManualPlanningDataTable.ReadOnly = true;
            ManualPlanningDataTable.RowHeadersWidth = 51;
            ManualPlanningDataTable.Size = new Size(385, 147);
            ManualPlanningDataTable.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "Критерий";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            dataGridViewTextBoxColumn1.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTextBoxColumn1.Width = 125;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "Знач.";
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.ReadOnly = true;
            dataGridViewTextBoxColumn2.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTextBoxColumn2.Width = 125;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.HeaderText = "Пред. знач.";
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.ReadOnly = true;
            dataGridViewTextBoxColumn3.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTextBoxColumn3.Width = 125;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.HeaderText = "% улучшения";
            dataGridViewTextBoxColumn4.MinimumWidth = 6;
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            dataGridViewTextBoxColumn4.ReadOnly = true;
            dataGridViewTextBoxColumn4.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTextBoxColumn4.Width = 125;
            // 
            // AnlDescTextBox
            // 
            AnlDescTextBox.Dock = DockStyle.Fill;
            AnlDescTextBox.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 204);
            AnlDescTextBox.Location = new Point(0, 0);
            AnlDescTextBox.Multiline = true;
            AnlDescTextBox.Name = "AnlDescTextBox";
            AnlDescTextBox.ScrollBars = ScrollBars.Both;
            AnlDescTextBox.Size = new Size(385, 214);
            AnlDescTextBox.TabIndex = 0;
            // 
            // panel3
            // 
            panel3.Controls.Add(dateTimePickerFromAnl);
            panel3.Controls.Add(label45);
            panel3.Controls.Add(label44);
            panel3.Controls.Add(dateTimePickerToAnl);
            panel3.Dock = DockStyle.Top;
            panel3.Location = new Point(0, 0);
            panel3.Name = "panel3";
            panel3.Size = new Size(385, 68);
            panel3.TabIndex = 39;
            // 
            // dateTimePickerFromAnl
            // 
            dateTimePickerFromAnl.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            dateTimePickerFromAnl.Format = DateTimePickerFormat.Custom;
            dateTimePickerFromAnl.Location = new Point(50, 3);
            dateTimePickerFromAnl.Name = "dateTimePickerFromAnl";
            dateTimePickerFromAnl.Size = new Size(209, 23);
            dateTimePickerFromAnl.TabIndex = 34;
            // 
            // label45
            // 
            label45.AutoSize = true;
            label45.Location = new Point(7, 7);
            label45.Name = "label45";
            label45.Size = new Size(30, 17);
            label45.TabIndex = 35;
            label45.Text = "От:";
            // 
            // label44
            // 
            label44.AutoSize = true;
            label44.Location = new Point(7, 39);
            label44.Name = "label44";
            label44.Size = new Size(31, 17);
            label44.TabIndex = 37;
            label44.Text = "До:";
            // 
            // dateTimePickerToAnl
            // 
            dateTimePickerToAnl.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            dateTimePickerToAnl.Format = DateTimePickerFormat.Custom;
            dateTimePickerToAnl.Location = new Point(50, 35);
            dateTimePickerToAnl.Name = "dateTimePickerToAnl";
            dateTimePickerToAnl.Size = new Size(209, 23);
            dateTimePickerToAnl.TabIndex = 36;
            // 
            // ganttANL
            // 
            ganttANL.BackColor = Color.White;
            ganttANL.Dock = DockStyle.Fill;
            ganttANL.Location = new Point(0, 0);
            ganttANL.Name = "ganttANL";
            ganttANL.Size = new Size(742, 433);
            ganttANL.TabIndex = 2;
            ganttANL.TimelineHeight = 50;
            ganttANL.TitleRowHeight = -1;
            ganttANL.TitleRowWidth = 200;
            // 
            // vScrollBarANL
            // 
            vScrollBarANL.Dock = DockStyle.Right;
            vScrollBarANL.Location = new Point(742, 0);
            vScrollBarANL.Name = "vScrollBarANL";
            vScrollBarANL.Size = new Size(26, 433);
            vScrollBarANL.TabIndex = 3;
            // 
            // GraphsTabPage
            // 
            GraphsTabPage.Controls.Add(GraphsMaterialTabControl);
            GraphsTabPage.Controls.Add(materialTabSelector4);
            GraphsTabPage.Location = new Point(4, 24);
            GraphsTabPage.Name = "GraphsTabPage";
            GraphsTabPage.Size = new Size(1163, 439);
            GraphsTabPage.TabIndex = 2;
            GraphsTabPage.Text = "Графики эффективности";
            GraphsTabPage.UseVisualStyleBackColor = true;
            // 
            // GraphsMaterialTabControl
            // 
            GraphsMaterialTabControl.Controls.Add(tabPage8);
            GraphsMaterialTabControl.Controls.Add(tabPage9);
            GraphsMaterialTabControl.Controls.Add(tabPage4);
            GraphsMaterialTabControl.Controls.Add(tabPage5);
            GraphsMaterialTabControl.Dock = DockStyle.Fill;
            GraphsMaterialTabControl.Location = new Point(0, 35);
            GraphsMaterialTabControl.Name = "GraphsMaterialTabControl";
            GraphsMaterialTabControl.SelectedIndex = 0;
            GraphsMaterialTabControl.Size = new Size(1163, 404);
            GraphsMaterialTabControl.TabIndex = 2;
            // 
            // tabPage8
            // 
            tabPage8.Controls.Add(splitContainer14);
            tabPage8.Location = new Point(4, 25);
            tabPage8.Name = "tabPage8";
            tabPage8.Padding = new Padding(3);
            tabPage8.Size = new Size(1155, 375);
            tabPage8.TabIndex = 0;
            tabPage8.Text = "Основное";
            tabPage8.UseVisualStyleBackColor = true;
            // 
            // splitContainer14
            // 
            splitContainer14.Dock = DockStyle.Fill;
            splitContainer14.Location = new Point(3, 3);
            splitContainer14.Name = "splitContainer14";
            // 
            // splitContainer14.Panel1
            // 
            splitContainer14.Panel1.Controls.Add(splitContainer15);
            splitContainer14.Panel1MinSize = 10;
            // 
            // splitContainer14.Panel2
            // 
            splitContainer14.Panel2.Controls.Add(splitContainer16);
            splitContainer14.Panel2MinSize = 10;
            splitContainer14.Size = new Size(1149, 369);
            splitContainer14.SplitterDistance = 551;
            splitContainer14.TabIndex = 1;
            // 
            // splitContainer15
            // 
            splitContainer15.Dock = DockStyle.Fill;
            splitContainer15.Location = new Point(0, 0);
            splitContainer15.Name = "splitContainer15";
            splitContainer15.Orientation = Orientation.Horizontal;
            // 
            // splitContainer15.Panel1
            // 
            splitContainer15.Panel1.Controls.Add(OrdersGraphControl);
            splitContainer15.Panel1MinSize = 10;
            // 
            // splitContainer15.Panel2
            // 
            splitContainer15.Panel2.Controls.Add(ProstoiGraphControl);
            splitContainer15.Panel2MinSize = 10;
            splitContainer15.Size = new Size(551, 369);
            splitContainer15.SplitterDistance = 183;
            splitContainer15.TabIndex = 0;
            // 
            // OrdersGraphControl
            // 
            OrdersGraphControl.BackColor = Color.Transparent;
            OrdersGraphControl.Dock = DockStyle.Fill;
            OrdersGraphControl.Location = new Point(0, 0);
            OrdersGraphControl.Margin = new Padding(6, 5, 6, 5);
            OrdersGraphControl.Name = "OrdersGraphControl";
            OrdersGraphControl.ScrollGrace = 0D;
            OrdersGraphControl.ScrollMaxX = 0D;
            OrdersGraphControl.ScrollMaxY = 0D;
            OrdersGraphControl.ScrollMaxY2 = 0D;
            OrdersGraphControl.ScrollMinX = 0D;
            OrdersGraphControl.ScrollMinY = 0D;
            OrdersGraphControl.ScrollMinY2 = 0D;
            OrdersGraphControl.Size = new Size(551, 183);
            OrdersGraphControl.TabIndex = 2;
            OrdersGraphControl.UseExtendedPrintDialog = true;
            // 
            // ProstoiGraphControl
            // 
            ProstoiGraphControl.Dock = DockStyle.Fill;
            ProstoiGraphControl.Location = new Point(0, 0);
            ProstoiGraphControl.Margin = new Padding(6, 5, 6, 5);
            ProstoiGraphControl.Name = "ProstoiGraphControl";
            ProstoiGraphControl.ScrollGrace = 0D;
            ProstoiGraphControl.ScrollMaxX = 0D;
            ProstoiGraphControl.ScrollMaxY = 0D;
            ProstoiGraphControl.ScrollMaxY2 = 0D;
            ProstoiGraphControl.ScrollMinX = 0D;
            ProstoiGraphControl.ScrollMinY = 0D;
            ProstoiGraphControl.ScrollMinY2 = 0D;
            ProstoiGraphControl.Size = new Size(551, 182);
            ProstoiGraphControl.TabIndex = 2;
            ProstoiGraphControl.UseExtendedPrintDialog = true;
            // 
            // splitContainer16
            // 
            splitContainer16.Dock = DockStyle.Fill;
            splitContainer16.Location = new Point(0, 0);
            splitContainer16.Name = "splitContainer16";
            splitContainer16.Orientation = Orientation.Horizontal;
            // 
            // splitContainer16.Panel1
            // 
            splitContainer16.Panel1.Controls.Add(RechangesGraphControl);
            splitContainer16.Panel1MinSize = 10;
            // 
            // splitContainer16.Panel2
            // 
            splitContainer16.Panel2.Controls.Add(TotalTimeGraphControl);
            splitContainer16.Panel2MinSize = 10;
            splitContainer16.Size = new Size(594, 369);
            splitContainer16.SplitterDistance = 183;
            splitContainer16.TabIndex = 0;
            // 
            // RechangesGraphControl
            // 
            RechangesGraphControl.Dock = DockStyle.Fill;
            RechangesGraphControl.Location = new Point(0, 0);
            RechangesGraphControl.Margin = new Padding(6, 5, 6, 5);
            RechangesGraphControl.Name = "RechangesGraphControl";
            RechangesGraphControl.ScrollGrace = 0D;
            RechangesGraphControl.ScrollMaxX = 0D;
            RechangesGraphControl.ScrollMaxY = 0D;
            RechangesGraphControl.ScrollMaxY2 = 0D;
            RechangesGraphControl.ScrollMinX = 0D;
            RechangesGraphControl.ScrollMinY = 0D;
            RechangesGraphControl.ScrollMinY2 = 0D;
            RechangesGraphControl.Size = new Size(594, 183);
            RechangesGraphControl.TabIndex = 2;
            RechangesGraphControl.UseExtendedPrintDialog = true;
            // 
            // TotalTimeGraphControl
            // 
            TotalTimeGraphControl.Dock = DockStyle.Fill;
            TotalTimeGraphControl.Location = new Point(0, 0);
            TotalTimeGraphControl.Margin = new Padding(6, 5, 6, 5);
            TotalTimeGraphControl.Name = "TotalTimeGraphControl";
            TotalTimeGraphControl.ScrollGrace = 0D;
            TotalTimeGraphControl.ScrollMaxX = 0D;
            TotalTimeGraphControl.ScrollMaxY = 0D;
            TotalTimeGraphControl.ScrollMaxY2 = 0D;
            TotalTimeGraphControl.ScrollMinX = 0D;
            TotalTimeGraphControl.ScrollMinY = 0D;
            TotalTimeGraphControl.ScrollMinY2 = 0D;
            TotalTimeGraphControl.Size = new Size(594, 182);
            TotalTimeGraphControl.TabIndex = 2;
            TotalTimeGraphControl.UseExtendedPrintDialog = true;
            // 
            // tabPage9
            // 
            tabPage9.Controls.Add(splitContainer11);
            tabPage9.Location = new Point(4, 24);
            tabPage9.Name = "tabPage9";
            tabPage9.Padding = new Padding(3);
            tabPage9.Size = new Size(1155, 376);
            tabPage9.TabIndex = 1;
            tabPage9.Text = "Основное (ч. 2)";
            tabPage9.UseVisualStyleBackColor = true;
            // 
            // splitContainer11
            // 
            splitContainer11.Dock = DockStyle.Fill;
            splitContainer11.Location = new Point(3, 3);
            splitContainer11.Name = "splitContainer11";
            // 
            // splitContainer11.Panel1
            // 
            splitContainer11.Panel1.Controls.Add(splitContainer12);
            splitContainer11.Panel1MinSize = 10;
            // 
            // splitContainer11.Panel2
            // 
            splitContainer11.Panel2.Controls.Add(splitContainer13);
            splitContainer11.Panel2MinSize = 10;
            splitContainer11.Size = new Size(1149, 370);
            splitContainer11.SplitterDistance = 1120;
            splitContainer11.TabIndex = 0;
            // 
            // splitContainer12
            // 
            splitContainer12.Dock = DockStyle.Fill;
            splitContainer12.Location = new Point(0, 0);
            splitContainer12.Name = "splitContainer12";
            splitContainer12.Orientation = Orientation.Horizontal;
            // 
            // splitContainer12.Panel1
            // 
            splitContainer12.Panel1.Controls.Add(TaraRedutantUsageGraphControl);
            splitContainer12.Panel1MinSize = 10;
            // 
            // splitContainer12.Panel2
            // 
            splitContainer12.Panel2.Controls.Add(zedGraphControl2);
            splitContainer12.Panel2MinSize = 10;
            splitContainer12.Size = new Size(1120, 370);
            splitContainer12.SplitterDistance = 333;
            splitContainer12.TabIndex = 0;
            // 
            // TaraRedutantUsageGraphControl
            // 
            TaraRedutantUsageGraphControl.Dock = DockStyle.Fill;
            TaraRedutantUsageGraphControl.Location = new Point(0, 0);
            TaraRedutantUsageGraphControl.Margin = new Padding(6, 5, 6, 5);
            TaraRedutantUsageGraphControl.Name = "TaraRedutantUsageGraphControl";
            TaraRedutantUsageGraphControl.ScrollGrace = 0D;
            TaraRedutantUsageGraphControl.ScrollMaxX = 0D;
            TaraRedutantUsageGraphControl.ScrollMaxY = 0D;
            TaraRedutantUsageGraphControl.ScrollMaxY2 = 0D;
            TaraRedutantUsageGraphControl.ScrollMinX = 0D;
            TaraRedutantUsageGraphControl.ScrollMinY = 0D;
            TaraRedutantUsageGraphControl.ScrollMinY2 = 0D;
            TaraRedutantUsageGraphControl.Size = new Size(1120, 333);
            TaraRedutantUsageGraphControl.TabIndex = 2;
            TaraRedutantUsageGraphControl.UseExtendedPrintDialog = true;
            // 
            // zedGraphControl2
            // 
            zedGraphControl2.Dock = DockStyle.Fill;
            zedGraphControl2.Location = new Point(0, 0);
            zedGraphControl2.Margin = new Padding(6, 5, 6, 5);
            zedGraphControl2.Name = "zedGraphControl2";
            zedGraphControl2.ScrollGrace = 0D;
            zedGraphControl2.ScrollMaxX = 0D;
            zedGraphControl2.ScrollMaxY = 0D;
            zedGraphControl2.ScrollMaxY2 = 0D;
            zedGraphControl2.ScrollMinX = 0D;
            zedGraphControl2.ScrollMinY = 0D;
            zedGraphControl2.ScrollMinY2 = 0D;
            zedGraphControl2.Size = new Size(1120, 33);
            zedGraphControl2.TabIndex = 2;
            zedGraphControl2.UseExtendedPrintDialog = true;
            // 
            // splitContainer13
            // 
            splitContainer13.Dock = DockStyle.Fill;
            splitContainer13.Location = new Point(0, 0);
            splitContainer13.Name = "splitContainer13";
            splitContainer13.Orientation = Orientation.Horizontal;
            // 
            // splitContainer13.Panel1
            // 
            splitContainer13.Panel1.Controls.Add(zedGraphControl1);
            splitContainer13.Panel1MinSize = 10;
            // 
            // splitContainer13.Panel2
            // 
            splitContainer13.Panel2.Controls.Add(zedGraphControl3);
            splitContainer13.Panel2MinSize = 10;
            splitContainer13.Size = new Size(25, 370);
            splitContainer13.SplitterDistance = 183;
            splitContainer13.TabIndex = 0;
            // 
            // zedGraphControl1
            // 
            zedGraphControl1.Dock = DockStyle.Fill;
            zedGraphControl1.Location = new Point(0, 0);
            zedGraphControl1.Margin = new Padding(6, 5, 6, 5);
            zedGraphControl1.Name = "zedGraphControl1";
            zedGraphControl1.ScrollGrace = 0D;
            zedGraphControl1.ScrollMaxX = 0D;
            zedGraphControl1.ScrollMaxY = 0D;
            zedGraphControl1.ScrollMaxY2 = 0D;
            zedGraphControl1.ScrollMinX = 0D;
            zedGraphControl1.ScrollMinY = 0D;
            zedGraphControl1.ScrollMinY2 = 0D;
            zedGraphControl1.Size = new Size(25, 183);
            zedGraphControl1.TabIndex = 2;
            zedGraphControl1.UseExtendedPrintDialog = true;
            // 
            // zedGraphControl3
            // 
            zedGraphControl3.Dock = DockStyle.Fill;
            zedGraphControl3.Location = new Point(0, 0);
            zedGraphControl3.Margin = new Padding(6, 5, 6, 5);
            zedGraphControl3.Name = "zedGraphControl3";
            zedGraphControl3.ScrollGrace = 0D;
            zedGraphControl3.ScrollMaxX = 0D;
            zedGraphControl3.ScrollMaxY = 0D;
            zedGraphControl3.ScrollMaxY2 = 0D;
            zedGraphControl3.ScrollMinX = 0D;
            zedGraphControl3.ScrollMinY = 0D;
            zedGraphControl3.ScrollMinY2 = 0D;
            zedGraphControl3.Size = new Size(25, 183);
            zedGraphControl3.TabIndex = 2;
            zedGraphControl3.UseExtendedPrintDialog = true;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(splitContainer9);
            tabPage4.Controls.Add(panel2);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(1155, 376);
            tabPage4.TabIndex = 2;
            tabPage4.Text = "Особые графики";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // splitContainer9
            // 
            splitContainer9.Dock = DockStyle.Fill;
            splitContainer9.Location = new Point(0, 30);
            splitContainer9.Name = "splitContainer9";
            splitContainer9.Orientation = Orientation.Horizontal;
            // 
            // splitContainer9.Panel1
            // 
            splitContainer9.Panel1.Controls.Add(splitContainer10);
            splitContainer9.Panel1MinSize = 10;
            // 
            // splitContainer9.Panel2
            // 
            splitContainer9.Panel2.Controls.Add(ProductionGraphControl);
            splitContainer9.Panel2MinSize = 10;
            splitContainer9.Size = new Size(1155, 346);
            splitContainer9.SplitterDistance = 161;
            splitContainer9.TabIndex = 1;
            // 
            // splitContainer10
            // 
            splitContainer10.Dock = DockStyle.Fill;
            splitContainer10.Location = new Point(0, 0);
            splitContainer10.Name = "splitContainer10";
            // 
            // splitContainer10.Panel1
            // 
            splitContainer10.Panel1.Controls.Add(TarasGraphControl);
            splitContainer10.Panel1MinSize = 10;
            // 
            // splitContainer10.Panel2
            // 
            splitContainer10.Panel2.Controls.Add(MaterialsGraphControl);
            splitContainer10.Panel2MinSize = 10;
            splitContainer10.Size = new Size(1155, 161);
            splitContainer10.SplitterDistance = 584;
            splitContainer10.TabIndex = 0;
            // 
            // TarasGraphControl
            // 
            TarasGraphControl.Dock = DockStyle.Fill;
            TarasGraphControl.Location = new Point(0, 0);
            TarasGraphControl.Margin = new Padding(6, 5, 6, 5);
            TarasGraphControl.Name = "TarasGraphControl";
            TarasGraphControl.ScrollGrace = 0D;
            TarasGraphControl.ScrollMaxX = 0D;
            TarasGraphControl.ScrollMaxY = 0D;
            TarasGraphControl.ScrollMaxY2 = 0D;
            TarasGraphControl.ScrollMinX = 0D;
            TarasGraphControl.ScrollMinY = 0D;
            TarasGraphControl.ScrollMinY2 = 0D;
            TarasGraphControl.Size = new Size(584, 161);
            TarasGraphControl.TabIndex = 2;
            TarasGraphControl.UseExtendedPrintDialog = true;
            // 
            // MaterialsGraphControl
            // 
            MaterialsGraphControl.Dock = DockStyle.Fill;
            MaterialsGraphControl.Location = new Point(0, 0);
            MaterialsGraphControl.Margin = new Padding(6, 5, 6, 5);
            MaterialsGraphControl.Name = "MaterialsGraphControl";
            MaterialsGraphControl.ScrollGrace = 0D;
            MaterialsGraphControl.ScrollMaxX = 0D;
            MaterialsGraphControl.ScrollMaxY = 0D;
            MaterialsGraphControl.ScrollMaxY2 = 0D;
            MaterialsGraphControl.ScrollMinX = 0D;
            MaterialsGraphControl.ScrollMinY = 0D;
            MaterialsGraphControl.ScrollMinY2 = 0D;
            MaterialsGraphControl.Size = new Size(567, 161);
            MaterialsGraphControl.TabIndex = 2;
            MaterialsGraphControl.UseExtendedPrintDialog = true;
            // 
            // ProductionGraphControl
            // 
            ProductionGraphControl.Dock = DockStyle.Fill;
            ProductionGraphControl.Location = new Point(0, 0);
            ProductionGraphControl.Margin = new Padding(6, 5, 6, 5);
            ProductionGraphControl.Name = "ProductionGraphControl";
            ProductionGraphControl.ScrollGrace = 0D;
            ProductionGraphControl.ScrollMaxX = 0D;
            ProductionGraphControl.ScrollMaxY = 0D;
            ProductionGraphControl.ScrollMaxY2 = 0D;
            ProductionGraphControl.ScrollMinX = 0D;
            ProductionGraphControl.ScrollMinY = 0D;
            ProductionGraphControl.ScrollMinY2 = 0D;
            ProductionGraphControl.Size = new Size(1155, 181);
            ProductionGraphControl.TabIndex = 2;
            ProductionGraphControl.UseExtendedPrintDialog = true;
            // 
            // panel2
            // 
            panel2.Controls.Add(label47);
            panel2.Controls.Add(MaterialsListOnGraphics);
            panel2.Controls.Add(label46);
            panel2.Controls.Add(TaraListOnGraphics);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(1155, 30);
            panel2.TabIndex = 0;
            // 
            // label47
            // 
            label47.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label47.AutoSize = true;
            label47.Location = new Point(733, 5);
            label47.Name = "label47";
            label47.Size = new Size(88, 17);
            label47.TabIndex = 34;
            label47.Text = "Материалы:";
            // 
            // MaterialsListOnGraphics
            // 
            MaterialsListOnGraphics.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            MaterialsListOnGraphics.FormattingEnabled = true;
            MaterialsListOnGraphics.Location = new Point(849, 2);
            MaterialsListOnGraphics.Name = "MaterialsListOnGraphics";
            MaterialsListOnGraphics.Size = new Size(304, 24);
            MaterialsListOnGraphics.TabIndex = 33;
            MaterialsListOnGraphics.TextChanged += MaterialsListOnGraphics_TextChanged;
            // 
            // label46
            // 
            label46.AutoSize = true;
            label46.Location = new Point(3, 5);
            label46.Name = "label46";
            label46.Size = new Size(47, 17);
            label46.TabIndex = 32;
            label46.Text = "Тары:";
            // 
            // TaraListOnGraphics
            // 
            TaraListOnGraphics.FormattingEnabled = true;
            TaraListOnGraphics.Location = new Point(103, 2);
            TaraListOnGraphics.Name = "TaraListOnGraphics";
            TaraListOnGraphics.Size = new Size(304, 24);
            TaraListOnGraphics.TabIndex = 31;
            TaraListOnGraphics.TextChanged += TaraListOnGraphics_TextChanged;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(AllPercentagesGraphControl);
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Size = new Size(1155, 376);
            tabPage5.TabIndex = 3;
            tabPage5.Text = "Оптимизация (в %)";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // AllPercentagesGraphControl
            // 
            AllPercentagesGraphControl.Dock = DockStyle.Fill;
            AllPercentagesGraphControl.Location = new Point(0, 0);
            AllPercentagesGraphControl.Margin = new Padding(6, 5, 6, 5);
            AllPercentagesGraphControl.Name = "AllPercentagesGraphControl";
            AllPercentagesGraphControl.ScrollGrace = 0D;
            AllPercentagesGraphControl.ScrollMaxX = 0D;
            AllPercentagesGraphControl.ScrollMaxY = 0D;
            AllPercentagesGraphControl.ScrollMaxY2 = 0D;
            AllPercentagesGraphControl.ScrollMinX = 0D;
            AllPercentagesGraphControl.ScrollMinY = 0D;
            AllPercentagesGraphControl.ScrollMinY2 = 0D;
            AllPercentagesGraphControl.Size = new Size(1155, 376);
            AllPercentagesGraphControl.TabIndex = 1;
            AllPercentagesGraphControl.UseExtendedPrintDialog = true;
            // 
            // materialTabSelector4
            // 
            materialTabSelector4.BackColor = Color.CornflowerBlue;
            materialTabSelector4.BaseTabControl = GraphsMaterialTabControl;
            materialTabSelector4.Dock = DockStyle.Top;
            materialTabSelector4.ForeColor = Color.White;
            materialTabSelector4.Location = new Point(0, 0);
            materialTabSelector4.Name = "materialTabSelector4";
            materialTabSelector4.Size = new Size(1163, 35);
            materialTabSelector4.TabIndex = 1;
            materialTabSelector4.Text = "materialTabSelector4";
            // 
            // materialTabSelector3
            // 
            materialTabSelector3.BackColor = Color.CornflowerBlue;
            materialTabSelector3.BaseTabControl = PlanningTabControl;
            materialTabSelector3.Dock = DockStyle.Top;
            materialTabSelector3.ForeColor = Color.White;
            materialTabSelector3.Location = new Point(0, 0);
            materialTabSelector3.Name = "materialTabSelector3";
            materialTabSelector3.Size = new Size(1171, 35);
            materialTabSelector3.TabIndex = 1;
            materialTabSelector3.Text = "materialTabSelector3";
            // 
            // SettingsTabPage
            // 
            SettingsTabPage.BackColor = Color.White;
            SettingsTabPage.Controls.Add(splitContainer2);
            SettingsTabPage.Location = new Point(4, 25);
            SettingsTabPage.Name = "SettingsTabPage";
            SettingsTabPage.Size = new Size(1171, 501);
            SettingsTabPage.TabIndex = 3;
            SettingsTabPage.Text = "Настройки";
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(HoppersPanel);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.AutoScroll = true;
            splitContainer2.Panel2.Controls.Add(groupBox8);
            splitContainer2.Panel2.Controls.Add(label51);
            splitContainer2.Panel2.Controls.Add(MaxOldSessionPathsDaysNumericUpDown);
            splitContainer2.Panel2.Controls.Add(button4);
            splitContainer2.Panel2.Controls.Add(button3);
            splitContainer2.Panel2.Controls.Add(button2);
            splitContainer2.Panel2.Controls.Add(button1);
            splitContainer2.Panel2.Controls.Add(groupBox7);
            splitContainer2.Panel2.Controls.Add(groupBox6);
            splitContainer2.Panel2.Controls.Add(groupBox2);
            splitContainer2.Panel2.Controls.Add(groupBox1);
            splitContainer2.Panel2.Controls.Add(ClearAllSessionErrorsButton);
            splitContainer2.Panel2.Controls.Add(HandSessionPostDataProcessButton);
            splitContainer2.Panel2.Controls.Add(SaveSettingsButton);
            splitContainer2.Panel2.Controls.Add(ToggleThemeButton);
            splitContainer2.Panel2.Controls.Add(BackupStagesDurationLabel);
            splitContainer2.Panel2.Controls.Add(BackupStagesDurationNumericUpDown);
            splitContainer2.Panel2.Controls.Add(PartiasMaxDaysLabel);
            splitContainer2.Panel2.Controls.Add(PartiasMaxDaysNumericUpDown);
            splitContainer2.Size = new Size(1171, 501);
            splitContainer2.SplitterDistance = 185;
            splitContainer2.TabIndex = 37;
            // 
            // HoppersPanel
            // 
            HoppersPanel.AutoScroll = true;
            HoppersPanel.BorderStyle = BorderStyle.FixedSingle;
            HoppersPanel.Dock = DockStyle.Fill;
            HoppersPanel.Location = new Point(0, 0);
            HoppersPanel.Name = "HoppersPanel";
            HoppersPanel.Size = new Size(1171, 185);
            HoppersPanel.TabIndex = 25;
            // 
            // groupBox8
            // 
            groupBox8.Controls.Add(StagesPercentNumericUpDown);
            groupBox8.Controls.Add(label58);
            groupBox8.Controls.Add(RNewDeadlineCheckBox);
            groupBox8.Controls.Add(UseRCheckBox);
            groupBox8.Controls.Add(TMaxDaysNumericUpDown);
            groupBox8.Controls.Add(RGlobDeltaCheckBox);
            groupBox8.Controls.Add(label57);
            groupBox8.Controls.Add(UseRStarCheckBox);
            groupBox8.Controls.Add(TMaxDaysPercentNumericUpDown);
            groupBox8.Controls.Add(UseTCheckBox);
            groupBox8.Controls.Add(label56);
            groupBox8.Controls.Add(UseGCheckBox);
            groupBox8.Controls.Add(label54);
            groupBox8.Controls.Add(GammaSlider);
            groupBox8.Controls.Add(GammaLabel);
            groupBox8.Controls.Add(label55);
            groupBox8.Location = new Point(16, 300);
            groupBox8.Name = "groupBox8";
            groupBox8.Size = new Size(1151, 106);
            groupBox8.TabIndex = 86;
            groupBox8.TabStop = false;
            groupBox8.Text = "Планирование";
            // 
            // StagesPercentNumericUpDown
            // 
            StagesPercentNumericUpDown.BackColor = Color.CornflowerBlue;
            StagesPercentNumericUpDown.BorderStyle = BorderStyle.None;
            StagesPercentNumericUpDown.DecimalPlaces = 2;
            StagesPercentNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            StagesPercentNumericUpDown.ForeColor = Color.White;
            StagesPercentNumericUpDown.Location = new Point(547, 84);
            StagesPercentNumericUpDown.Margin = new Padding(0);
            StagesPercentNumericUpDown.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            StagesPercentNumericUpDown.Name = "StagesPercentNumericUpDown";
            StagesPercentNumericUpDown.Size = new Size(97, 19);
            StagesPercentNumericUpDown.TabIndex = 88;
            StagesPercentNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label58
            // 
            label58.AutoSize = true;
            label58.Font = new Font("Microsoft Sans Serif", 10F);
            label58.Location = new Point(325, 84);
            label58.Name = "label58";
            label58.Size = new Size(124, 17);
            label58.TabIndex = 87;
            label58.Text = "% этапов в плане";
            // 
            // RNewDeadlineCheckBox
            // 
            RNewDeadlineCheckBox.BackColor = Color.Black;
            RNewDeadlineCheckBox.Depth = 0;
            RNewDeadlineCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            RNewDeadlineCheckBox.ForeColor = Color.CornflowerBlue;
            RNewDeadlineCheckBox.Location = new Point(736, 59);
            RNewDeadlineCheckBox.Margin = new Padding(0);
            RNewDeadlineCheckBox.MouseLocation = new Point(-1, -1);
            RNewDeadlineCheckBox.Name = "RNewDeadlineCheckBox";
            RNewDeadlineCheckBox.Ripple = true;
            RNewDeadlineCheckBox.Size = new Size(224, 30);
            RNewDeadlineCheckBox.TabIndex = 86;
            RNewDeadlineCheckBox.Text = "Счётчик дедлайнов Анисова";
            RNewDeadlineCheckBox.UseVisualStyleBackColor = false;
            // 
            // UseRCheckBox
            // 
            UseRCheckBox.BackColor = Color.Black;
            UseRCheckBox.Depth = 0;
            UseRCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            UseRCheckBox.ForeColor = Color.CornflowerBlue;
            UseRCheckBox.Location = new Point(5, 19);
            UseRCheckBox.Margin = new Padding(0);
            UseRCheckBox.MouseLocation = new Point(-1, -1);
            UseRCheckBox.Name = "UseRCheckBox";
            UseRCheckBox.Ripple = true;
            UseRCheckBox.Size = new Size(52, 30);
            UseRCheckBox.TabIndex = 37;
            UseRCheckBox.Text = "R";
            UseRCheckBox.UseVisualStyleBackColor = false;
            // 
            // TMaxDaysNumericUpDown
            // 
            TMaxDaysNumericUpDown.BackColor = Color.CornflowerBlue;
            TMaxDaysNumericUpDown.BorderStyle = BorderStyle.None;
            TMaxDaysNumericUpDown.DecimalPlaces = 2;
            TMaxDaysNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            TMaxDaysNumericUpDown.ForeColor = Color.White;
            TMaxDaysNumericUpDown.Location = new Point(547, 64);
            TMaxDaysNumericUpDown.Margin = new Padding(0);
            TMaxDaysNumericUpDown.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            TMaxDaysNumericUpDown.Name = "TMaxDaysNumericUpDown";
            TMaxDaysNumericUpDown.Size = new Size(97, 19);
            TMaxDaysNumericUpDown.TabIndex = 85;
            TMaxDaysNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // RGlobDeltaCheckBox
            // 
            RGlobDeltaCheckBox.BackColor = Color.Black;
            RGlobDeltaCheckBox.Depth = 0;
            RGlobDeltaCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            RGlobDeltaCheckBox.ForeColor = Color.CornflowerBlue;
            RGlobDeltaCheckBox.Location = new Point(960, 57);
            RGlobDeltaCheckBox.Margin = new Padding(0);
            RGlobDeltaCheckBox.MouseLocation = new Point(-1, -1);
            RGlobDeltaCheckBox.Name = "RGlobDeltaCheckBox";
            RGlobDeltaCheckBox.Ripple = true;
            RGlobDeltaCheckBox.Size = new Size(184, 30);
            RGlobDeltaCheckBox.TabIndex = 41;
            RGlobDeltaCheckBox.Text = "R - глобальная дельта";
            RGlobDeltaCheckBox.UseVisualStyleBackColor = false;
            // 
            // label57
            // 
            label57.AutoSize = true;
            label57.Font = new Font("Microsoft Sans Serif", 10F);
            label57.Location = new Point(325, 64);
            label57.Name = "label57";
            label57.Size = new Size(219, 17);
            label57.TabIndex = 84;
            label57.Text = "T: макс. дней удлинения плана:";
            // 
            // UseRStarCheckBox
            // 
            UseRStarCheckBox.BackColor = Color.Black;
            UseRStarCheckBox.Depth = 0;
            UseRStarCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            UseRStarCheckBox.ForeColor = Color.CornflowerBlue;
            UseRStarCheckBox.Location = new Point(57, 19);
            UseRStarCheckBox.Margin = new Padding(0);
            UseRStarCheckBox.MouseLocation = new Point(-1, -1);
            UseRStarCheckBox.Name = "UseRStarCheckBox";
            UseRStarCheckBox.Ripple = true;
            UseRStarCheckBox.Size = new Size(56, 30);
            UseRStarCheckBox.TabIndex = 39;
            UseRStarCheckBox.Text = "R*";
            UseRStarCheckBox.UseVisualStyleBackColor = false;
            // 
            // TMaxDaysPercentNumericUpDown
            // 
            TMaxDaysPercentNumericUpDown.BackColor = Color.CornflowerBlue;
            TMaxDaysPercentNumericUpDown.BorderStyle = BorderStyle.None;
            TMaxDaysPercentNumericUpDown.DecimalPlaces = 2;
            TMaxDaysPercentNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            TMaxDaysPercentNumericUpDown.ForeColor = Color.White;
            TMaxDaysPercentNumericUpDown.Location = new Point(252, 65);
            TMaxDaysPercentNumericUpDown.Margin = new Padding(0);
            TMaxDaysPercentNumericUpDown.Name = "TMaxDaysPercentNumericUpDown";
            TMaxDaysPercentNumericUpDown.Size = new Size(70, 19);
            TMaxDaysPercentNumericUpDown.TabIndex = 83;
            TMaxDaysPercentNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // UseTCheckBox
            // 
            UseTCheckBox.BackColor = Color.Black;
            UseTCheckBox.Depth = 0;
            UseTCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            UseTCheckBox.ForeColor = Color.CornflowerBlue;
            UseTCheckBox.Location = new Point(113, 19);
            UseTCheckBox.Margin = new Padding(0);
            UseTCheckBox.MouseLocation = new Point(-1, -1);
            UseTCheckBox.Name = "UseTCheckBox";
            UseTCheckBox.Ripple = true;
            UseTCheckBox.Size = new Size(56, 30);
            UseTCheckBox.TabIndex = 40;
            UseTCheckBox.Text = "T";
            UseTCheckBox.UseVisualStyleBackColor = false;
            // 
            // label56
            // 
            label56.AutoSize = true;
            label56.Font = new Font("Microsoft Sans Serif", 10F);
            label56.Location = new Point(7, 65);
            label56.Name = "label56";
            label56.Size = new Size(242, 17);
            label56.TabIndex = 82;
            label56.Text = "T: макс. процент удлинения плана:";
            // 
            // UseGCheckBox
            // 
            UseGCheckBox.BackColor = Color.Black;
            UseGCheckBox.Depth = 0;
            UseGCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            UseGCheckBox.ForeColor = Color.CornflowerBlue;
            UseGCheckBox.Location = new Point(169, 19);
            UseGCheckBox.Margin = new Padding(0);
            UseGCheckBox.MouseLocation = new Point(-1, -1);
            UseGCheckBox.Name = "UseGCheckBox";
            UseGCheckBox.Ripple = true;
            UseGCheckBox.Size = new Size(55, 30);
            UseGCheckBox.TabIndex = 38;
            UseGCheckBox.Text = "G";
            UseGCheckBox.UseVisualStyleBackColor = false;
            // 
            // label54
            // 
            label54.AutoSize = true;
            label54.Font = new Font("Microsoft Sans Serif", 9F);
            label54.Location = new Point(227, 25);
            label54.Name = "label54";
            label54.Size = new Size(85, 15);
            label54.TabIndex = 79;
            label54.Text = "Переналадки";
            // 
            // GammaSlider
            // 
            GammaSlider.Location = new Point(318, 23);
            GammaSlider.Maximum = 1000;
            GammaSlider.Name = "GammaSlider";
            GammaSlider.Size = new Size(722, 45);
            GammaSlider.TabIndex = 78;
            GammaSlider.Value = 5;
            // 
            // GammaLabel
            // 
            GammaLabel.AutoSize = true;
            GammaLabel.Location = new Point(1046, 40);
            GammaLabel.Name = "GammaLabel";
            GammaLabel.Size = new Size(42, 17);
            GammaLabel.TabIndex = 81;
            GammaLabel.Text = "0.00х";
            // 
            // label55
            // 
            label55.AutoSize = true;
            label55.Font = new Font("Microsoft Sans Serif", 9F);
            label55.Location = new Point(1046, 25);
            label55.Name = "label55";
            label55.Size = new Size(51, 15);
            label55.TabIndex = 80;
            label55.Text = "Дельта";
            // 
            // label51
            // 
            label51.AutoSize = true;
            label51.Font = new Font("Microsoft Sans Serif", 8F);
            label51.Location = new Point(9, 113);
            label51.Name = "label51";
            label51.Size = new Size(211, 13);
            label51.TabIndex = 50;
            label51.Text = "Макс. дней хранения каталогов сессии:";
            // 
            // MaxOldSessionPathsDaysNumericUpDown
            // 
            MaxOldSessionPathsDaysNumericUpDown.BackColor = Color.CornflowerBlue;
            MaxOldSessionPathsDaysNumericUpDown.BorderStyle = BorderStyle.None;
            MaxOldSessionPathsDaysNumericUpDown.DecimalPlaces = 2;
            MaxOldSessionPathsDaysNumericUpDown.ForeColor = Color.White;
            MaxOldSessionPathsDaysNumericUpDown.Location = new Point(234, 110);
            MaxOldSessionPathsDaysNumericUpDown.Margin = new Padding(0);
            MaxOldSessionPathsDaysNumericUpDown.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            MaxOldSessionPathsDaysNumericUpDown.Name = "MaxOldSessionPathsDaysNumericUpDown";
            MaxOldSessionPathsDaysNumericUpDown.Size = new Size(84, 19);
            MaxOldSessionPathsDaysNumericUpDown.TabIndex = 49;
            MaxOldSessionPathsDaysNumericUpDown.TextAlign = HorizontalAlignment.Center;
            MaxOldSessionPathsDaysNumericUpDown.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // button4
            // 
            button4.BackColor = Color.CornflowerBlue;
            button4.BackgroundImageLayout = ImageLayout.Zoom;
            button4.Cursor = Cursors.Hand;
            button4.FlatAppearance.BorderSize = 0;
            button4.FlatStyle = FlatStyle.Flat;
            button4.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            button4.ForeColor = Color.White;
            button4.Location = new Point(1041, 493);
            button4.Name = "button4";
            button4.Size = new Size(126, 36);
            button4.TabIndex = 48;
            button4.Text = "Выгрузить макс намотки";
            button4.UseCompatibleTextRendering = true;
            button4.UseVisualStyleBackColor = false;
            button4.Click += button4_Click;
            // 
            // button3
            // 
            button3.BackColor = Color.CornflowerBlue;
            button3.BackgroundImageLayout = ImageLayout.Zoom;
            button3.Cursor = Cursors.Hand;
            button3.FlatAppearance.BorderSize = 0;
            button3.FlatStyle = FlatStyle.Flat;
            button3.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            button3.ForeColor = Color.White;
            button3.Location = new Point(944, 493);
            button3.Name = "button3";
            button3.Size = new Size(91, 36);
            button3.TabIndex = 47;
            button3.Text = "Симулятор";
            button3.UseCompatibleTextRendering = true;
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // button2
            // 
            button2.BackColor = Color.CornflowerBlue;
            button2.BackgroundImageLayout = ImageLayout.Zoom;
            button2.Cursor = Cursors.Hand;
            button2.FlatAppearance.BorderSize = 0;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            button2.ForeColor = Color.White;
            button2.Location = new Point(944, 452);
            button2.Name = "button2";
            button2.Size = new Size(223, 36);
            button2.TabIndex = 46;
            button2.Text = "Запустить назначение остатков";
            button2.UseCompatibleTextRendering = true;
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.BackColor = Color.CornflowerBlue;
            button1.BackgroundImageLayout = ImageLayout.Zoom;
            button1.Cursor = Cursors.Hand;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            button1.ForeColor = Color.White;
            button1.Location = new Point(944, 412);
            button1.Name = "button1";
            button1.Size = new Size(223, 36);
            button1.TabIndex = 45;
            button1.Text = "Сформулировать информацию по материалам";
            button1.UseCompatibleTextRendering = true;
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // groupBox7
            // 
            groupBox7.Controls.Add(UsePseudoTaraCheckBox);
            groupBox7.Controls.Add(CreateStopMe);
            groupBox7.Controls.Add(CreateKillMeCheckBox);
            groupBox7.Controls.Add(AutoLoadCheckBox);
            groupBox7.Controls.Add(SaveOnExitCheckBox);
            groupBox7.Controls.Add(HideConsoleCheckBox);
            groupBox7.Controls.Add(LogErrorsCheckBox);
            groupBox7.Location = new Point(321, 3);
            groupBox7.Name = "groupBox7";
            groupBox7.Size = new Size(315, 240);
            groupBox7.TabIndex = 44;
            groupBox7.TabStop = false;
            groupBox7.Text = "Основное";
            // 
            // UsePseudoTaraCheckBox
            // 
            UsePseudoTaraCheckBox.AutoSize = true;
            UsePseudoTaraCheckBox.BackColor = Color.Black;
            UsePseudoTaraCheckBox.Depth = 0;
            UsePseudoTaraCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            UsePseudoTaraCheckBox.ForeColor = Color.CornflowerBlue;
            UsePseudoTaraCheckBox.Location = new Point(3, 21);
            UsePseudoTaraCheckBox.Margin = new Padding(0);
            UsePseudoTaraCheckBox.MouseLocation = new Point(-1, -1);
            UsePseudoTaraCheckBox.Name = "UsePseudoTaraCheckBox";
            UsePseudoTaraCheckBox.Ripple = true;
            UsePseudoTaraCheckBox.Size = new Size(168, 30);
            UsePseudoTaraCheckBox.TabIndex = 35;
            UsePseudoTaraCheckBox.Text = "Внедрять псевдотару";
            UsePseudoTaraCheckBox.UseVisualStyleBackColor = false;
            // 
            // CreateStopMe
            // 
            CreateStopMe.AutoSize = true;
            CreateStopMe.BackColor = Color.Black;
            CreateStopMe.Depth = 0;
            CreateStopMe.Font = new Font("Microsoft Sans Serif", 10F);
            CreateStopMe.ForeColor = Color.CornflowerBlue;
            CreateStopMe.Location = new Point(3, 172);
            CreateStopMe.Margin = new Padding(0);
            CreateStopMe.MouseLocation = new Point(-1, -1);
            CreateStopMe.Name = "CreateStopMe";
            CreateStopMe.Ripple = true;
            CreateStopMe.Size = new Size(149, 30);
            CreateStopMe.TabIndex = 27;
            CreateStopMe.Text = "Создавать StopMe";
            CreateStopMe.UseVisualStyleBackColor = false;
            // 
            // CreateKillMeCheckBox
            // 
            CreateKillMeCheckBox.AutoSize = true;
            CreateKillMeCheckBox.BackColor = Color.Black;
            CreateKillMeCheckBox.Depth = 0;
            CreateKillMeCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            CreateKillMeCheckBox.ForeColor = Color.CornflowerBlue;
            CreateKillMeCheckBox.Location = new Point(3, 141);
            CreateKillMeCheckBox.Margin = new Padding(0);
            CreateKillMeCheckBox.MouseLocation = new Point(-1, -1);
            CreateKillMeCheckBox.Name = "CreateKillMeCheckBox";
            CreateKillMeCheckBox.Ripple = true;
            CreateKillMeCheckBox.Size = new Size(140, 30);
            CreateKillMeCheckBox.TabIndex = 26;
            CreateKillMeCheckBox.Text = "Создавать KillMe";
            CreateKillMeCheckBox.UseVisualStyleBackColor = false;
            // 
            // AutoLoadCheckBox
            // 
            AutoLoadCheckBox.AutoSize = true;
            AutoLoadCheckBox.BackColor = Color.Black;
            AutoLoadCheckBox.Depth = 0;
            AutoLoadCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            AutoLoadCheckBox.ForeColor = Color.CornflowerBlue;
            AutoLoadCheckBox.Location = new Point(3, 81);
            AutoLoadCheckBox.Margin = new Padding(0);
            AutoLoadCheckBox.MouseLocation = new Point(-1, -1);
            AutoLoadCheckBox.Name = "AutoLoadCheckBox";
            AutoLoadCheckBox.Ripple = true;
            AutoLoadCheckBox.Size = new Size(167, 30);
            AutoLoadCheckBox.TabIndex = 24;
            AutoLoadCheckBox.Text = "Автозагрузка данных";
            AutoLoadCheckBox.UseVisualStyleBackColor = false;
            // 
            // SaveOnExitCheckBox
            // 
            SaveOnExitCheckBox.BackColor = Color.Black;
            SaveOnExitCheckBox.Depth = 0;
            SaveOnExitCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            SaveOnExitCheckBox.ForeColor = Color.CornflowerBlue;
            SaveOnExitCheckBox.Location = new Point(3, 202);
            SaveOnExitCheckBox.Margin = new Padding(0);
            SaveOnExitCheckBox.MouseLocation = new Point(-1, -1);
            SaveOnExitCheckBox.Name = "SaveOnExitCheckBox";
            SaveOnExitCheckBox.Ripple = true;
            SaveOnExitCheckBox.Size = new Size(309, 30);
            SaveOnExitCheckBox.TabIndex = 23;
            SaveOnExitCheckBox.Text = "Сохр. настройки при выходе";
            SaveOnExitCheckBox.UseVisualStyleBackColor = false;
            // 
            // HideConsoleCheckBox
            // 
            HideConsoleCheckBox.AutoSize = true;
            HideConsoleCheckBox.BackColor = Color.Black;
            HideConsoleCheckBox.Depth = 0;
            HideConsoleCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            HideConsoleCheckBox.ForeColor = Color.CornflowerBlue;
            HideConsoleCheckBox.Location = new Point(3, 111);
            HideConsoleCheckBox.Margin = new Padding(0);
            HideConsoleCheckBox.MouseLocation = new Point(-1, -1);
            HideConsoleCheckBox.Name = "HideConsoleCheckBox";
            HideConsoleCheckBox.Ripple = true;
            HideConsoleCheckBox.Size = new Size(148, 30);
            HideConsoleCheckBox.TabIndex = 33;
            HideConsoleCheckBox.Text = "Скрывать консоль";
            HideConsoleCheckBox.UseVisualStyleBackColor = false;
            // 
            // LogErrorsCheckBox
            // 
            LogErrorsCheckBox.AutoSize = true;
            LogErrorsCheckBox.BackColor = Color.Black;
            LogErrorsCheckBox.Depth = 0;
            LogErrorsCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            LogErrorsCheckBox.ForeColor = Color.CornflowerBlue;
            LogErrorsCheckBox.Location = new Point(3, 52);
            LogErrorsCheckBox.Margin = new Padding(0);
            LogErrorsCheckBox.MouseLocation = new Point(-1, -1);
            LogErrorsCheckBox.Name = "LogErrorsCheckBox";
            LogErrorsCheckBox.Ripple = true;
            LogErrorsCheckBox.Size = new Size(167, 30);
            LogErrorsCheckBox.TabIndex = 36;
            LogErrorsCheckBox.Text = "Фиксировать ошибки";
            LogErrorsCheckBox.UseVisualStyleBackColor = false;
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(VKoeffsTextBox);
            groupBox6.Controls.Add(criteriaRadioButton7);
            groupBox6.Controls.Add(materialRadioButtonAuto);
            groupBox6.Controls.Add(CriteriasMaxIterationsNumericUpDown7);
            groupBox6.Controls.Add(CriteriasMinIterationsNumericUpDown7);
            groupBox6.Controls.Add(CriteriasPossibleWorseNumericUpDown7);
            groupBox6.Controls.Add(CriteriasMaxIterationsNumericUpDown6);
            groupBox6.Controls.Add(CriteriasMinIterationsNumericUpDown6);
            groupBox6.Controls.Add(CriteriasPossibleWorseNumericUpDown6);
            groupBox6.Controls.Add(label9);
            groupBox6.Controls.Add(CriteriasMaxIterationsNumericUpDown5);
            groupBox6.Controls.Add(CriteriasMinIterationsNumericUpDown5);
            groupBox6.Controls.Add(CriteriasPossibleWorseNumericUpDown5);
            groupBox6.Controls.Add(CriteriasMaxIterationsNumericUpDown4);
            groupBox6.Controls.Add(CriteriasMinIterationsNumericUpDown4);
            groupBox6.Controls.Add(CriteriasPossibleWorseNumericUpDown4);
            groupBox6.Controls.Add(CriteriasMaxIterationsNumericUpDown3);
            groupBox6.Controls.Add(CriteriasMinIterationsNumericUpDown3);
            groupBox6.Controls.Add(CriteriasPossibleWorseNumericUpDown3);
            groupBox6.Controls.Add(CriteriasMaxIterationsNumericUpDown2);
            groupBox6.Controls.Add(CriteriasMinIterationsNumericUpDown2);
            groupBox6.Controls.Add(CriteriasPossibleWorseNumericUpDown2);
            groupBox6.Controls.Add(label41);
            groupBox6.Controls.Add(UseOnlyMaxCriteriasSettingsCheckBox);
            groupBox6.Controls.Add(CriteriasMaxIterationsNumericUpDown1);
            groupBox6.Controls.Add(CriteriasMinIterationsNumericUpDown1);
            groupBox6.Controls.Add(CriteriasPossibleWorseNumericUpDown1);
            groupBox6.Controls.Add(label40);
            groupBox6.Controls.Add(materialRadioButtonCombo);
            groupBox6.Controls.Add(CriteriasListTextBox);
            groupBox6.Controls.Add(criteriaRadioButton6);
            groupBox6.Controls.Add(criteriaRadioButton5);
            groupBox6.Controls.Add(criteriaRadioButton4);
            groupBox6.Controls.Add(criteriaRadioButton3);
            groupBox6.Controls.Add(criteriaRadioButton2);
            groupBox6.Controls.Add(criteriaRadioButton1);
            groupBox6.Controls.Add(label42);
            groupBox6.Controls.Add(label43);
            groupBox6.Location = new Point(642, 3);
            groupBox6.Name = "groupBox6";
            groupBox6.Size = new Size(525, 298);
            groupBox6.TabIndex = 43;
            groupBox6.TabStop = false;
            groupBox6.Text = "Критерии оптимизации";
            // 
            // VKoeffsTextBox
            // 
            VKoeffsTextBox.Location = new Point(92, 271);
            VKoeffsTextBox.Name = "VKoeffsTextBox";
            VKoeffsTextBox.Size = new Size(227, 23);
            VKoeffsTextBox.TabIndex = 39;
            // 
            // criteriaRadioButton7
            // 
            criteriaRadioButton7.BackColor = Color.Black;
            criteriaRadioButton7.Depth = 0;
            criteriaRadioButton7.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            criteriaRadioButton7.ForeColor = Color.CornflowerBlue;
            criteriaRadioButton7.Location = new Point(4, 196);
            criteriaRadioButton7.Margin = new Padding(0);
            criteriaRadioButton7.MouseLocation = new Point(-1, -1);
            criteriaRadioButton7.Name = "criteriaRadioButton7";
            criteriaRadioButton7.Ripple = true;
            criteriaRadioButton7.Size = new Size(190, 24);
            criteriaRadioButton7.TabIndex = 77;
            criteriaRadioButton7.Text = "7. Соответствие";
            criteriaRadioButton7.UseVisualStyleBackColor = false;
            // 
            // materialRadioButtonAuto
            // 
            materialRadioButtonAuto.BackColor = Color.Black;
            materialRadioButtonAuto.Depth = 0;
            materialRadioButtonAuto.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            materialRadioButtonAuto.ForeColor = Color.CornflowerBlue;
            materialRadioButtonAuto.Location = new Point(4, 244);
            materialRadioButtonAuto.Margin = new Padding(0);
            materialRadioButtonAuto.MouseLocation = new Point(-1, -1);
            materialRadioButtonAuto.Name = "materialRadioButtonAuto";
            materialRadioButtonAuto.Ripple = true;
            materialRadioButtonAuto.Size = new Size(197, 24);
            materialRadioButtonAuto.TabIndex = 76;
            materialRadioButtonAuto.Text = "Автоматический";
            materialRadioButtonAuto.UseVisualStyleBackColor = false;
            // 
            // CriteriasMaxIterationsNumericUpDown7
            // 
            CriteriasMaxIterationsNumericUpDown7.BackColor = Color.CornflowerBlue;
            CriteriasMaxIterationsNumericUpDown7.BorderStyle = BorderStyle.None;
            CriteriasMaxIterationsNumericUpDown7.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMaxIterationsNumericUpDown7.ForeColor = Color.White;
            CriteriasMaxIterationsNumericUpDown7.Location = new Point(434, 196);
            CriteriasMaxIterationsNumericUpDown7.Margin = new Padding(0);
            CriteriasMaxIterationsNumericUpDown7.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMaxIterationsNumericUpDown7.Name = "CriteriasMaxIterationsNumericUpDown7";
            CriteriasMaxIterationsNumericUpDown7.Size = new Size(84, 19);
            CriteriasMaxIterationsNumericUpDown7.TabIndex = 75;
            CriteriasMaxIterationsNumericUpDown7.TextAlign = HorizontalAlignment.Center;
            CriteriasMaxIterationsNumericUpDown7.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // CriteriasMinIterationsNumericUpDown7
            // 
            CriteriasMinIterationsNumericUpDown7.BackColor = Color.CornflowerBlue;
            CriteriasMinIterationsNumericUpDown7.BorderStyle = BorderStyle.None;
            CriteriasMinIterationsNumericUpDown7.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMinIterationsNumericUpDown7.ForeColor = Color.White;
            CriteriasMinIterationsNumericUpDown7.Location = new Point(320, 196);
            CriteriasMinIterationsNumericUpDown7.Margin = new Padding(0);
            CriteriasMinIterationsNumericUpDown7.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMinIterationsNumericUpDown7.Name = "CriteriasMinIterationsNumericUpDown7";
            CriteriasMinIterationsNumericUpDown7.Size = new Size(84, 19);
            CriteriasMinIterationsNumericUpDown7.TabIndex = 74;
            CriteriasMinIterationsNumericUpDown7.TextAlign = HorizontalAlignment.Center;
            CriteriasMinIterationsNumericUpDown7.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // CriteriasPossibleWorseNumericUpDown7
            // 
            CriteriasPossibleWorseNumericUpDown7.BackColor = Color.CornflowerBlue;
            CriteriasPossibleWorseNumericUpDown7.BorderStyle = BorderStyle.None;
            CriteriasPossibleWorseNumericUpDown7.DecimalPlaces = 2;
            CriteriasPossibleWorseNumericUpDown7.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasPossibleWorseNumericUpDown7.ForeColor = Color.White;
            CriteriasPossibleWorseNumericUpDown7.Location = new Point(194, 196);
            CriteriasPossibleWorseNumericUpDown7.Margin = new Padding(0);
            CriteriasPossibleWorseNumericUpDown7.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            CriteriasPossibleWorseNumericUpDown7.Name = "CriteriasPossibleWorseNumericUpDown7";
            CriteriasPossibleWorseNumericUpDown7.Size = new Size(84, 19);
            CriteriasPossibleWorseNumericUpDown7.TabIndex = 73;
            CriteriasPossibleWorseNumericUpDown7.TextAlign = HorizontalAlignment.Center;
            CriteriasPossibleWorseNumericUpDown7.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // CriteriasMaxIterationsNumericUpDown6
            // 
            CriteriasMaxIterationsNumericUpDown6.BackColor = Color.CornflowerBlue;
            CriteriasMaxIterationsNumericUpDown6.BorderStyle = BorderStyle.None;
            CriteriasMaxIterationsNumericUpDown6.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMaxIterationsNumericUpDown6.ForeColor = Color.White;
            CriteriasMaxIterationsNumericUpDown6.Location = new Point(434, 172);
            CriteriasMaxIterationsNumericUpDown6.Margin = new Padding(0);
            CriteriasMaxIterationsNumericUpDown6.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMaxIterationsNumericUpDown6.Name = "CriteriasMaxIterationsNumericUpDown6";
            CriteriasMaxIterationsNumericUpDown6.Size = new Size(84, 19);
            CriteriasMaxIterationsNumericUpDown6.TabIndex = 72;
            CriteriasMaxIterationsNumericUpDown6.TextAlign = HorizontalAlignment.Center;
            CriteriasMaxIterationsNumericUpDown6.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // CriteriasMinIterationsNumericUpDown6
            // 
            CriteriasMinIterationsNumericUpDown6.BackColor = Color.CornflowerBlue;
            CriteriasMinIterationsNumericUpDown6.BorderStyle = BorderStyle.None;
            CriteriasMinIterationsNumericUpDown6.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMinIterationsNumericUpDown6.ForeColor = Color.White;
            CriteriasMinIterationsNumericUpDown6.Location = new Point(320, 172);
            CriteriasMinIterationsNumericUpDown6.Margin = new Padding(0);
            CriteriasMinIterationsNumericUpDown6.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMinIterationsNumericUpDown6.Name = "CriteriasMinIterationsNumericUpDown6";
            CriteriasMinIterationsNumericUpDown6.Size = new Size(84, 19);
            CriteriasMinIterationsNumericUpDown6.TabIndex = 71;
            CriteriasMinIterationsNumericUpDown6.TextAlign = HorizontalAlignment.Center;
            CriteriasMinIterationsNumericUpDown6.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // CriteriasPossibleWorseNumericUpDown6
            // 
            CriteriasPossibleWorseNumericUpDown6.BackColor = Color.CornflowerBlue;
            CriteriasPossibleWorseNumericUpDown6.BorderStyle = BorderStyle.None;
            CriteriasPossibleWorseNumericUpDown6.DecimalPlaces = 2;
            CriteriasPossibleWorseNumericUpDown6.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasPossibleWorseNumericUpDown6.ForeColor = Color.White;
            CriteriasPossibleWorseNumericUpDown6.Location = new Point(194, 172);
            CriteriasPossibleWorseNumericUpDown6.Margin = new Padding(0);
            CriteriasPossibleWorseNumericUpDown6.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            CriteriasPossibleWorseNumericUpDown6.Name = "CriteriasPossibleWorseNumericUpDown6";
            CriteriasPossibleWorseNumericUpDown6.Size = new Size(84, 19);
            CriteriasPossibleWorseNumericUpDown6.TabIndex = 70;
            CriteriasPossibleWorseNumericUpDown6.TextAlign = HorizontalAlignment.Center;
            CriteriasPossibleWorseNumericUpDown6.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Microsoft Sans Serif", 9F);
            label9.Location = new Point(10, 274);
            label9.Name = "label9";
            label9.Size = new Size(66, 15);
            label9.TabIndex = 40;
            label9.Text = "Коэфф. V:";
            // 
            // CriteriasMaxIterationsNumericUpDown5
            // 
            CriteriasMaxIterationsNumericUpDown5.BackColor = Color.CornflowerBlue;
            CriteriasMaxIterationsNumericUpDown5.BorderStyle = BorderStyle.None;
            CriteriasMaxIterationsNumericUpDown5.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMaxIterationsNumericUpDown5.ForeColor = Color.White;
            CriteriasMaxIterationsNumericUpDown5.Location = new Point(434, 148);
            CriteriasMaxIterationsNumericUpDown5.Margin = new Padding(0);
            CriteriasMaxIterationsNumericUpDown5.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMaxIterationsNumericUpDown5.Name = "CriteriasMaxIterationsNumericUpDown5";
            CriteriasMaxIterationsNumericUpDown5.Size = new Size(84, 19);
            CriteriasMaxIterationsNumericUpDown5.TabIndex = 69;
            CriteriasMaxIterationsNumericUpDown5.TextAlign = HorizontalAlignment.Center;
            CriteriasMaxIterationsNumericUpDown5.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // CriteriasMinIterationsNumericUpDown5
            // 
            CriteriasMinIterationsNumericUpDown5.BackColor = Color.CornflowerBlue;
            CriteriasMinIterationsNumericUpDown5.BorderStyle = BorderStyle.None;
            CriteriasMinIterationsNumericUpDown5.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMinIterationsNumericUpDown5.ForeColor = Color.White;
            CriteriasMinIterationsNumericUpDown5.Location = new Point(320, 148);
            CriteriasMinIterationsNumericUpDown5.Margin = new Padding(0);
            CriteriasMinIterationsNumericUpDown5.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMinIterationsNumericUpDown5.Name = "CriteriasMinIterationsNumericUpDown5";
            CriteriasMinIterationsNumericUpDown5.Size = new Size(84, 19);
            CriteriasMinIterationsNumericUpDown5.TabIndex = 68;
            CriteriasMinIterationsNumericUpDown5.TextAlign = HorizontalAlignment.Center;
            CriteriasMinIterationsNumericUpDown5.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // CriteriasPossibleWorseNumericUpDown5
            // 
            CriteriasPossibleWorseNumericUpDown5.BackColor = Color.CornflowerBlue;
            CriteriasPossibleWorseNumericUpDown5.BorderStyle = BorderStyle.None;
            CriteriasPossibleWorseNumericUpDown5.DecimalPlaces = 2;
            CriteriasPossibleWorseNumericUpDown5.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasPossibleWorseNumericUpDown5.ForeColor = Color.White;
            CriteriasPossibleWorseNumericUpDown5.Location = new Point(194, 148);
            CriteriasPossibleWorseNumericUpDown5.Margin = new Padding(0);
            CriteriasPossibleWorseNumericUpDown5.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            CriteriasPossibleWorseNumericUpDown5.Name = "CriteriasPossibleWorseNumericUpDown5";
            CriteriasPossibleWorseNumericUpDown5.Size = new Size(84, 19);
            CriteriasPossibleWorseNumericUpDown5.TabIndex = 67;
            CriteriasPossibleWorseNumericUpDown5.TextAlign = HorizontalAlignment.Center;
            CriteriasPossibleWorseNumericUpDown5.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // CriteriasMaxIterationsNumericUpDown4
            // 
            CriteriasMaxIterationsNumericUpDown4.BackColor = Color.CornflowerBlue;
            CriteriasMaxIterationsNumericUpDown4.BorderStyle = BorderStyle.None;
            CriteriasMaxIterationsNumericUpDown4.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMaxIterationsNumericUpDown4.ForeColor = Color.White;
            CriteriasMaxIterationsNumericUpDown4.Location = new Point(434, 124);
            CriteriasMaxIterationsNumericUpDown4.Margin = new Padding(0);
            CriteriasMaxIterationsNumericUpDown4.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMaxIterationsNumericUpDown4.Name = "CriteriasMaxIterationsNumericUpDown4";
            CriteriasMaxIterationsNumericUpDown4.Size = new Size(84, 19);
            CriteriasMaxIterationsNumericUpDown4.TabIndex = 66;
            CriteriasMaxIterationsNumericUpDown4.TextAlign = HorizontalAlignment.Center;
            CriteriasMaxIterationsNumericUpDown4.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // CriteriasMinIterationsNumericUpDown4
            // 
            CriteriasMinIterationsNumericUpDown4.BackColor = Color.CornflowerBlue;
            CriteriasMinIterationsNumericUpDown4.BorderStyle = BorderStyle.None;
            CriteriasMinIterationsNumericUpDown4.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMinIterationsNumericUpDown4.ForeColor = Color.White;
            CriteriasMinIterationsNumericUpDown4.Location = new Point(320, 124);
            CriteriasMinIterationsNumericUpDown4.Margin = new Padding(0);
            CriteriasMinIterationsNumericUpDown4.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMinIterationsNumericUpDown4.Name = "CriteriasMinIterationsNumericUpDown4";
            CriteriasMinIterationsNumericUpDown4.Size = new Size(84, 19);
            CriteriasMinIterationsNumericUpDown4.TabIndex = 65;
            CriteriasMinIterationsNumericUpDown4.TextAlign = HorizontalAlignment.Center;
            CriteriasMinIterationsNumericUpDown4.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // CriteriasPossibleWorseNumericUpDown4
            // 
            CriteriasPossibleWorseNumericUpDown4.BackColor = Color.CornflowerBlue;
            CriteriasPossibleWorseNumericUpDown4.BorderStyle = BorderStyle.None;
            CriteriasPossibleWorseNumericUpDown4.DecimalPlaces = 2;
            CriteriasPossibleWorseNumericUpDown4.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasPossibleWorseNumericUpDown4.ForeColor = Color.White;
            CriteriasPossibleWorseNumericUpDown4.Location = new Point(194, 124);
            CriteriasPossibleWorseNumericUpDown4.Margin = new Padding(0);
            CriteriasPossibleWorseNumericUpDown4.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            CriteriasPossibleWorseNumericUpDown4.Name = "CriteriasPossibleWorseNumericUpDown4";
            CriteriasPossibleWorseNumericUpDown4.Size = new Size(84, 19);
            CriteriasPossibleWorseNumericUpDown4.TabIndex = 64;
            CriteriasPossibleWorseNumericUpDown4.TextAlign = HorizontalAlignment.Center;
            CriteriasPossibleWorseNumericUpDown4.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // CriteriasMaxIterationsNumericUpDown3
            // 
            CriteriasMaxIterationsNumericUpDown3.BackColor = Color.CornflowerBlue;
            CriteriasMaxIterationsNumericUpDown3.BorderStyle = BorderStyle.None;
            CriteriasMaxIterationsNumericUpDown3.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMaxIterationsNumericUpDown3.ForeColor = Color.White;
            CriteriasMaxIterationsNumericUpDown3.Location = new Point(434, 100);
            CriteriasMaxIterationsNumericUpDown3.Margin = new Padding(0);
            CriteriasMaxIterationsNumericUpDown3.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMaxIterationsNumericUpDown3.Name = "CriteriasMaxIterationsNumericUpDown3";
            CriteriasMaxIterationsNumericUpDown3.Size = new Size(84, 19);
            CriteriasMaxIterationsNumericUpDown3.TabIndex = 63;
            CriteriasMaxIterationsNumericUpDown3.TextAlign = HorizontalAlignment.Center;
            CriteriasMaxIterationsNumericUpDown3.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // CriteriasMinIterationsNumericUpDown3
            // 
            CriteriasMinIterationsNumericUpDown3.BackColor = Color.CornflowerBlue;
            CriteriasMinIterationsNumericUpDown3.BorderStyle = BorderStyle.None;
            CriteriasMinIterationsNumericUpDown3.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMinIterationsNumericUpDown3.ForeColor = Color.White;
            CriteriasMinIterationsNumericUpDown3.Location = new Point(320, 100);
            CriteriasMinIterationsNumericUpDown3.Margin = new Padding(0);
            CriteriasMinIterationsNumericUpDown3.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMinIterationsNumericUpDown3.Name = "CriteriasMinIterationsNumericUpDown3";
            CriteriasMinIterationsNumericUpDown3.Size = new Size(84, 19);
            CriteriasMinIterationsNumericUpDown3.TabIndex = 62;
            CriteriasMinIterationsNumericUpDown3.TextAlign = HorizontalAlignment.Center;
            CriteriasMinIterationsNumericUpDown3.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // CriteriasPossibleWorseNumericUpDown3
            // 
            CriteriasPossibleWorseNumericUpDown3.BackColor = Color.CornflowerBlue;
            CriteriasPossibleWorseNumericUpDown3.BorderStyle = BorderStyle.None;
            CriteriasPossibleWorseNumericUpDown3.DecimalPlaces = 2;
            CriteriasPossibleWorseNumericUpDown3.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasPossibleWorseNumericUpDown3.ForeColor = Color.White;
            CriteriasPossibleWorseNumericUpDown3.Location = new Point(194, 100);
            CriteriasPossibleWorseNumericUpDown3.Margin = new Padding(0);
            CriteriasPossibleWorseNumericUpDown3.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            CriteriasPossibleWorseNumericUpDown3.Name = "CriteriasPossibleWorseNumericUpDown3";
            CriteriasPossibleWorseNumericUpDown3.Size = new Size(84, 19);
            CriteriasPossibleWorseNumericUpDown3.TabIndex = 61;
            CriteriasPossibleWorseNumericUpDown3.TextAlign = HorizontalAlignment.Center;
            CriteriasPossibleWorseNumericUpDown3.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // CriteriasMaxIterationsNumericUpDown2
            // 
            CriteriasMaxIterationsNumericUpDown2.BackColor = Color.CornflowerBlue;
            CriteriasMaxIterationsNumericUpDown2.BorderStyle = BorderStyle.None;
            CriteriasMaxIterationsNumericUpDown2.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMaxIterationsNumericUpDown2.ForeColor = Color.White;
            CriteriasMaxIterationsNumericUpDown2.Location = new Point(434, 76);
            CriteriasMaxIterationsNumericUpDown2.Margin = new Padding(0);
            CriteriasMaxIterationsNumericUpDown2.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMaxIterationsNumericUpDown2.Name = "CriteriasMaxIterationsNumericUpDown2";
            CriteriasMaxIterationsNumericUpDown2.Size = new Size(84, 19);
            CriteriasMaxIterationsNumericUpDown2.TabIndex = 60;
            CriteriasMaxIterationsNumericUpDown2.TextAlign = HorizontalAlignment.Center;
            CriteriasMaxIterationsNumericUpDown2.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // CriteriasMinIterationsNumericUpDown2
            // 
            CriteriasMinIterationsNumericUpDown2.BackColor = Color.CornflowerBlue;
            CriteriasMinIterationsNumericUpDown2.BorderStyle = BorderStyle.None;
            CriteriasMinIterationsNumericUpDown2.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMinIterationsNumericUpDown2.ForeColor = Color.White;
            CriteriasMinIterationsNumericUpDown2.Location = new Point(320, 76);
            CriteriasMinIterationsNumericUpDown2.Margin = new Padding(0);
            CriteriasMinIterationsNumericUpDown2.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMinIterationsNumericUpDown2.Name = "CriteriasMinIterationsNumericUpDown2";
            CriteriasMinIterationsNumericUpDown2.Size = new Size(84, 19);
            CriteriasMinIterationsNumericUpDown2.TabIndex = 59;
            CriteriasMinIterationsNumericUpDown2.TextAlign = HorizontalAlignment.Center;
            CriteriasMinIterationsNumericUpDown2.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // CriteriasPossibleWorseNumericUpDown2
            // 
            CriteriasPossibleWorseNumericUpDown2.BackColor = Color.CornflowerBlue;
            CriteriasPossibleWorseNumericUpDown2.BorderStyle = BorderStyle.None;
            CriteriasPossibleWorseNumericUpDown2.DecimalPlaces = 2;
            CriteriasPossibleWorseNumericUpDown2.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasPossibleWorseNumericUpDown2.ForeColor = Color.White;
            CriteriasPossibleWorseNumericUpDown2.Location = new Point(194, 76);
            CriteriasPossibleWorseNumericUpDown2.Margin = new Padding(0);
            CriteriasPossibleWorseNumericUpDown2.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            CriteriasPossibleWorseNumericUpDown2.Name = "CriteriasPossibleWorseNumericUpDown2";
            CriteriasPossibleWorseNumericUpDown2.Size = new Size(84, 19);
            CriteriasPossibleWorseNumericUpDown2.TabIndex = 58;
            CriteriasPossibleWorseNumericUpDown2.TextAlign = HorizontalAlignment.Center;
            CriteriasPossibleWorseNumericUpDown2.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // label41
            // 
            label41.AutoSize = true;
            label41.Font = new Font("Microsoft Sans Serif", 9F);
            label41.Location = new Point(9, 27);
            label41.Name = "label41";
            label41.Size = new Size(67, 15);
            label41.TabIndex = 45;
            label41.Text = "Критерий:";
            // 
            // UseOnlyMaxCriteriasSettingsCheckBox
            // 
            UseOnlyMaxCriteriasSettingsCheckBox.AutoSize = true;
            UseOnlyMaxCriteriasSettingsCheckBox.BackColor = Color.Black;
            UseOnlyMaxCriteriasSettingsCheckBox.Depth = 0;
            UseOnlyMaxCriteriasSettingsCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            UseOnlyMaxCriteriasSettingsCheckBox.ForeColor = Color.CornflowerBlue;
            UseOnlyMaxCriteriasSettingsCheckBox.Location = new Point(295, 220);
            UseOnlyMaxCriteriasSettingsCheckBox.Margin = new Padding(0);
            UseOnlyMaxCriteriasSettingsCheckBox.MouseLocation = new Point(-1, -1);
            UseOnlyMaxCriteriasSettingsCheckBox.Name = "UseOnlyMaxCriteriasSettingsCheckBox";
            UseOnlyMaxCriteriasSettingsCheckBox.RightToLeft = RightToLeft.No;
            UseOnlyMaxCriteriasSettingsCheckBox.Ripple = true;
            UseOnlyMaxCriteriasSettingsCheckBox.Size = new Size(176, 30);
            UseOnlyMaxCriteriasSettingsCheckBox.TabIndex = 37;
            UseOnlyMaxCriteriasSettingsCheckBox.Text = "Только максимальные";
            UseOnlyMaxCriteriasSettingsCheckBox.UseVisualStyleBackColor = false;
            // 
            // CriteriasMaxIterationsNumericUpDown1
            // 
            CriteriasMaxIterationsNumericUpDown1.BackColor = Color.CornflowerBlue;
            CriteriasMaxIterationsNumericUpDown1.BorderStyle = BorderStyle.None;
            CriteriasMaxIterationsNumericUpDown1.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMaxIterationsNumericUpDown1.ForeColor = Color.White;
            CriteriasMaxIterationsNumericUpDown1.Location = new Point(434, 52);
            CriteriasMaxIterationsNumericUpDown1.Margin = new Padding(0);
            CriteriasMaxIterationsNumericUpDown1.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMaxIterationsNumericUpDown1.Name = "CriteriasMaxIterationsNumericUpDown1";
            CriteriasMaxIterationsNumericUpDown1.Size = new Size(84, 19);
            CriteriasMaxIterationsNumericUpDown1.TabIndex = 55;
            CriteriasMaxIterationsNumericUpDown1.TextAlign = HorizontalAlignment.Center;
            CriteriasMaxIterationsNumericUpDown1.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // CriteriasMinIterationsNumericUpDown1
            // 
            CriteriasMinIterationsNumericUpDown1.BackColor = Color.CornflowerBlue;
            CriteriasMinIterationsNumericUpDown1.BorderStyle = BorderStyle.None;
            CriteriasMinIterationsNumericUpDown1.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasMinIterationsNumericUpDown1.ForeColor = Color.White;
            CriteriasMinIterationsNumericUpDown1.Location = new Point(320, 52);
            CriteriasMinIterationsNumericUpDown1.Margin = new Padding(0);
            CriteriasMinIterationsNumericUpDown1.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CriteriasMinIterationsNumericUpDown1.Name = "CriteriasMinIterationsNumericUpDown1";
            CriteriasMinIterationsNumericUpDown1.Size = new Size(84, 19);
            CriteriasMinIterationsNumericUpDown1.TabIndex = 54;
            CriteriasMinIterationsNumericUpDown1.TextAlign = HorizontalAlignment.Center;
            CriteriasMinIterationsNumericUpDown1.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // CriteriasPossibleWorseNumericUpDown1
            // 
            CriteriasPossibleWorseNumericUpDown1.BackColor = Color.CornflowerBlue;
            CriteriasPossibleWorseNumericUpDown1.BorderStyle = BorderStyle.None;
            CriteriasPossibleWorseNumericUpDown1.DecimalPlaces = 2;
            CriteriasPossibleWorseNumericUpDown1.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CriteriasPossibleWorseNumericUpDown1.ForeColor = Color.White;
            CriteriasPossibleWorseNumericUpDown1.Location = new Point(194, 52);
            CriteriasPossibleWorseNumericUpDown1.Margin = new Padding(0);
            CriteriasPossibleWorseNumericUpDown1.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            CriteriasPossibleWorseNumericUpDown1.Name = "CriteriasPossibleWorseNumericUpDown1";
            CriteriasPossibleWorseNumericUpDown1.Size = new Size(84, 19);
            CriteriasPossibleWorseNumericUpDown1.TabIndex = 53;
            CriteriasPossibleWorseNumericUpDown1.TextAlign = HorizontalAlignment.Center;
            CriteriasPossibleWorseNumericUpDown1.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // label40
            // 
            label40.AutoSize = true;
            label40.Font = new Font("Microsoft Sans Serif", 9F);
            label40.Location = new Point(325, 274);
            label40.Name = "label40";
            label40.Size = new Size(67, 15);
            label40.TabIndex = 45;
            label40.Text = "Критерии:";
            // 
            // materialRadioButtonCombo
            // 
            materialRadioButtonCombo.BackColor = Color.Black;
            materialRadioButtonCombo.Depth = 0;
            materialRadioButtonCombo.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            materialRadioButtonCombo.ForeColor = Color.CornflowerBlue;
            materialRadioButtonCombo.Location = new Point(4, 220);
            materialRadioButtonCombo.Margin = new Padding(0);
            materialRadioButtonCombo.MouseLocation = new Point(-1, -1);
            materialRadioButtonCombo.Name = "materialRadioButtonCombo";
            materialRadioButtonCombo.Ripple = true;
            materialRadioButtonCombo.Size = new Size(197, 24);
            materialRadioButtonCombo.TabIndex = 52;
            materialRadioButtonCombo.Text = "Комбинированный";
            materialRadioButtonCombo.UseVisualStyleBackColor = false;
            // 
            // CriteriasListTextBox
            // 
            CriteriasListTextBox.Location = new Point(392, 271);
            CriteriasListTextBox.Name = "CriteriasListTextBox";
            CriteriasListTextBox.Size = new Size(115, 23);
            CriteriasListTextBox.TabIndex = 44;
            // 
            // criteriaRadioButton6
            // 
            criteriaRadioButton6.BackColor = Color.Black;
            criteriaRadioButton6.Depth = 0;
            criteriaRadioButton6.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            criteriaRadioButton6.ForeColor = Color.CornflowerBlue;
            criteriaRadioButton6.Location = new Point(4, 172);
            criteriaRadioButton6.Margin = new Padding(0);
            criteriaRadioButton6.MouseLocation = new Point(-1, -1);
            criteriaRadioButton6.Name = "criteriaRadioButton6";
            criteriaRadioButton6.Ripple = true;
            criteriaRadioButton6.Size = new Size(190, 24);
            criteriaRadioButton6.TabIndex = 51;
            criteriaRadioButton6.Text = "6. Распред. РЦ";
            criteriaRadioButton6.UseVisualStyleBackColor = false;
            // 
            // criteriaRadioButton5
            // 
            criteriaRadioButton5.BackColor = Color.Black;
            criteriaRadioButton5.Depth = 0;
            criteriaRadioButton5.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            criteriaRadioButton5.ForeColor = Color.CornflowerBlue;
            criteriaRadioButton5.Location = new Point(4, 148);
            criteriaRadioButton5.Margin = new Padding(0);
            criteriaRadioButton5.MouseLocation = new Point(-1, -1);
            criteriaRadioButton5.Name = "criteriaRadioButton5";
            criteriaRadioButton5.Ripple = true;
            criteriaRadioButton5.Size = new Size(190, 24);
            criteriaRadioButton5.TabIndex = 50;
            criteriaRadioButton5.Text = "5. Размотка тары";
            criteriaRadioButton5.UseVisualStyleBackColor = false;
            // 
            // criteriaRadioButton4
            // 
            criteriaRadioButton4.BackColor = Color.Black;
            criteriaRadioButton4.Depth = 0;
            criteriaRadioButton4.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            criteriaRadioButton4.ForeColor = Color.CornflowerBlue;
            criteriaRadioButton4.Location = new Point(4, 124);
            criteriaRadioButton4.Margin = new Padding(0);
            criteriaRadioButton4.MouseLocation = new Point(-1, -1);
            criteriaRadioButton4.Name = "criteriaRadioButton4";
            criteriaRadioButton4.Ripple = true;
            criteriaRadioButton4.Size = new Size(190, 24);
            criteriaRadioButton4.TabIndex = 49;
            criteriaRadioButton4.Text = "4. Длина плана";
            criteriaRadioButton4.UseVisualStyleBackColor = false;
            // 
            // criteriaRadioButton3
            // 
            criteriaRadioButton3.BackColor = Color.Black;
            criteriaRadioButton3.Depth = 0;
            criteriaRadioButton3.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            criteriaRadioButton3.ForeColor = Color.CornflowerBlue;
            criteriaRadioButton3.Location = new Point(4, 100);
            criteriaRadioButton3.Margin = new Padding(0);
            criteriaRadioButton3.MouseLocation = new Point(-1, -1);
            criteriaRadioButton3.Name = "criteriaRadioButton3";
            criteriaRadioButton3.Ripple = true;
            criteriaRadioButton3.Size = new Size(190, 24);
            criteriaRadioButton3.TabIndex = 48;
            criteriaRadioButton3.Text = "3. Простои";
            criteriaRadioButton3.UseVisualStyleBackColor = false;
            // 
            // criteriaRadioButton2
            // 
            criteriaRadioButton2.BackColor = Color.Black;
            criteriaRadioButton2.Depth = 0;
            criteriaRadioButton2.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            criteriaRadioButton2.ForeColor = Color.CornflowerBlue;
            criteriaRadioButton2.Location = new Point(4, 76);
            criteriaRadioButton2.Margin = new Padding(0);
            criteriaRadioButton2.MouseLocation = new Point(-1, -1);
            criteriaRadioButton2.Name = "criteriaRadioButton2";
            criteriaRadioButton2.Ripple = true;
            criteriaRadioButton2.Size = new Size(190, 24);
            criteriaRadioButton2.TabIndex = 47;
            criteriaRadioButton2.Text = "2. Переналадки";
            criteriaRadioButton2.UseVisualStyleBackColor = false;
            // 
            // criteriaRadioButton1
            // 
            criteriaRadioButton1.BackColor = Color.Black;
            criteriaRadioButton1.Checked = true;
            criteriaRadioButton1.Depth = 0;
            criteriaRadioButton1.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            criteriaRadioButton1.ForeColor = Color.CornflowerBlue;
            criteriaRadioButton1.Location = new Point(4, 52);
            criteriaRadioButton1.Margin = new Padding(0);
            criteriaRadioButton1.MouseLocation = new Point(-1, -1);
            criteriaRadioButton1.Name = "criteriaRadioButton1";
            criteriaRadioButton1.Ripple = true;
            criteriaRadioButton1.Size = new Size(190, 24);
            criteriaRadioButton1.TabIndex = 46;
            criteriaRadioButton1.TabStop = true;
            criteriaRadioButton1.Text = "1. Срывы заказов";
            criteriaRadioButton1.UseVisualStyleBackColor = false;
            // 
            // label42
            // 
            label42.Font = new Font("Microsoft Sans Serif", 9F);
            label42.Location = new Point(194, 17);
            label42.Name = "label42";
            label42.Size = new Size(104, 41);
            label42.TabIndex = 56;
            label42.Text = "Допустимое отклонение";
            // 
            // label43
            // 
            label43.Font = new Font("Microsoft Sans Serif", 9F);
            label43.Location = new Point(320, 17);
            label43.Name = "label43";
            label43.Size = new Size(198, 41);
            label43.TabIndex = 57;
            label43.Text = "Количество итераций при переходе на критерий (от-до)";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(groupBox5);
            groupBox2.Controls.Add(groupBox4);
            groupBox2.Controls.Add(label12);
            groupBox2.Controls.Add(groupBox3);
            groupBox2.Location = new Point(15, 401);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(928, 136);
            groupBox2.TabIndex = 42;
            groupBox2.TabStop = false;
            groupBox2.Text = "Функции заказов";
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(label31);
            groupBox5.Controls.Add(p0l3aNumericUpDown);
            groupBox5.Controls.Add(label32);
            groupBox5.Controls.Add(p0l3kNumericUpDown);
            groupBox5.Controls.Add(label33);
            groupBox5.Controls.Add(p0l2xNumericUpDown);
            groupBox5.Controls.Add(label34);
            groupBox5.Controls.Add(p0l2aNumericUpDown);
            groupBox5.Controls.Add(label35);
            groupBox5.Controls.Add(p0l2kNumericUpDown);
            groupBox5.Controls.Add(label36);
            groupBox5.Controls.Add(p0l1xNumericUpDown);
            groupBox5.Controls.Add(label37);
            groupBox5.Controls.Add(p0l1aNumericUpDown);
            groupBox5.Controls.Add(label38);
            groupBox5.Controls.Add(p0l1kNumericUpDown);
            groupBox5.Controls.Add(label39);
            groupBox5.Location = new Point(620, 40);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(301, 91);
            groupBox5.TabIndex = 45;
            groupBox5.TabStop = false;
            groupBox5.Text = "Высокий приоритет";
            // 
            // label31
            // 
            label31.AutoSize = true;
            label31.Font = new Font("Microsoft Sans Serif", 10F);
            label31.Location = new Point(194, 63);
            label31.Name = "label31";
            label31.Size = new Size(52, 17);
            label31.TabIndex = 61;
            label31.Text = "иначе.";
            // 
            // p0l3aNumericUpDown
            // 
            p0l3aNumericUpDown.BackColor = Color.CornflowerBlue;
            p0l3aNumericUpDown.BorderStyle = BorderStyle.None;
            p0l3aNumericUpDown.DecimalPlaces = 2;
            p0l3aNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p0l3aNumericUpDown.ForeColor = Color.White;
            p0l3aNumericUpDown.Location = new Point(122, 63);
            p0l3aNumericUpDown.Margin = new Padding(0);
            p0l3aNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p0l3aNumericUpDown.Name = "p0l3aNumericUpDown";
            p0l3aNumericUpDown.Size = new Size(70, 19);
            p0l3aNumericUpDown.TabIndex = 60;
            p0l3aNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label32
            // 
            label32.AutoSize = true;
            label32.Font = new Font("Microsoft Sans Serif", 10F);
            label32.Location = new Point(100, 63);
            label32.Name = "label32";
            label32.Size = new Size(20, 17);
            label32.TabIndex = 59;
            label32.Text = "a:";
            // 
            // p0l3kNumericUpDown
            // 
            p0l3kNumericUpDown.BackColor = Color.CornflowerBlue;
            p0l3kNumericUpDown.BorderStyle = BorderStyle.None;
            p0l3kNumericUpDown.DecimalPlaces = 2;
            p0l3kNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p0l3kNumericUpDown.ForeColor = Color.White;
            p0l3kNumericUpDown.Location = new Point(28, 63);
            p0l3kNumericUpDown.Margin = new Padding(0);
            p0l3kNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p0l3kNumericUpDown.Name = "p0l3kNumericUpDown";
            p0l3kNumericUpDown.Size = new Size(70, 19);
            p0l3kNumericUpDown.TabIndex = 58;
            p0l3kNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label33
            // 
            label33.AutoSize = true;
            label33.Font = new Font("Microsoft Sans Serif", 10F);
            label33.Location = new Point(6, 63);
            label33.Name = "label33";
            label33.Size = new Size(19, 17);
            label33.TabIndex = 57;
            label33.Text = "k:";
            // 
            // p0l2xNumericUpDown
            // 
            p0l2xNumericUpDown.BackColor = Color.CornflowerBlue;
            p0l2xNumericUpDown.BorderStyle = BorderStyle.None;
            p0l2xNumericUpDown.DecimalPlaces = 2;
            p0l2xNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p0l2xNumericUpDown.ForeColor = Color.White;
            p0l2xNumericUpDown.Location = new Point(223, 41);
            p0l2xNumericUpDown.Margin = new Padding(0);
            p0l2xNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p0l2xNumericUpDown.Name = "p0l2xNumericUpDown";
            p0l2xNumericUpDown.Size = new Size(70, 19);
            p0l2xNumericUpDown.TabIndex = 56;
            p0l2xNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label34
            // 
            label34.AutoSize = true;
            label34.Font = new Font("Microsoft Sans Serif", 10F);
            label34.Location = new Point(194, 41);
            label34.Name = "label34";
            label34.Size = new Size(26, 17);
            label34.TabIndex = 55;
            label34.Text = "x <";
            // 
            // p0l2aNumericUpDown
            // 
            p0l2aNumericUpDown.BackColor = Color.CornflowerBlue;
            p0l2aNumericUpDown.BorderStyle = BorderStyle.None;
            p0l2aNumericUpDown.DecimalPlaces = 2;
            p0l2aNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p0l2aNumericUpDown.ForeColor = Color.White;
            p0l2aNumericUpDown.Location = new Point(122, 41);
            p0l2aNumericUpDown.Margin = new Padding(0);
            p0l2aNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p0l2aNumericUpDown.Name = "p0l2aNumericUpDown";
            p0l2aNumericUpDown.Size = new Size(70, 19);
            p0l2aNumericUpDown.TabIndex = 54;
            p0l2aNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label35
            // 
            label35.AutoSize = true;
            label35.Font = new Font("Microsoft Sans Serif", 10F);
            label35.Location = new Point(100, 41);
            label35.Name = "label35";
            label35.Size = new Size(20, 17);
            label35.TabIndex = 53;
            label35.Text = "a:";
            // 
            // p0l2kNumericUpDown
            // 
            p0l2kNumericUpDown.BackColor = Color.CornflowerBlue;
            p0l2kNumericUpDown.BorderStyle = BorderStyle.None;
            p0l2kNumericUpDown.DecimalPlaces = 2;
            p0l2kNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p0l2kNumericUpDown.ForeColor = Color.White;
            p0l2kNumericUpDown.Location = new Point(28, 41);
            p0l2kNumericUpDown.Margin = new Padding(0);
            p0l2kNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p0l2kNumericUpDown.Name = "p0l2kNumericUpDown";
            p0l2kNumericUpDown.Size = new Size(70, 19);
            p0l2kNumericUpDown.TabIndex = 52;
            p0l2kNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label36
            // 
            label36.AutoSize = true;
            label36.Font = new Font("Microsoft Sans Serif", 10F);
            label36.Location = new Point(6, 41);
            label36.Name = "label36";
            label36.Size = new Size(19, 17);
            label36.TabIndex = 51;
            label36.Text = "k:";
            // 
            // p0l1xNumericUpDown
            // 
            p0l1xNumericUpDown.BackColor = Color.CornflowerBlue;
            p0l1xNumericUpDown.BorderStyle = BorderStyle.None;
            p0l1xNumericUpDown.DecimalPlaces = 2;
            p0l1xNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p0l1xNumericUpDown.ForeColor = Color.White;
            p0l1xNumericUpDown.Location = new Point(223, 19);
            p0l1xNumericUpDown.Margin = new Padding(0);
            p0l1xNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p0l1xNumericUpDown.Name = "p0l1xNumericUpDown";
            p0l1xNumericUpDown.Size = new Size(70, 19);
            p0l1xNumericUpDown.TabIndex = 50;
            p0l1xNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label37
            // 
            label37.AutoSize = true;
            label37.Font = new Font("Microsoft Sans Serif", 10F);
            label37.Location = new Point(194, 19);
            label37.Name = "label37";
            label37.Size = new Size(26, 17);
            label37.TabIndex = 49;
            label37.Text = "x <";
            // 
            // p0l1aNumericUpDown
            // 
            p0l1aNumericUpDown.BackColor = Color.CornflowerBlue;
            p0l1aNumericUpDown.BorderStyle = BorderStyle.None;
            p0l1aNumericUpDown.DecimalPlaces = 2;
            p0l1aNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p0l1aNumericUpDown.ForeColor = Color.White;
            p0l1aNumericUpDown.Location = new Point(122, 19);
            p0l1aNumericUpDown.Margin = new Padding(0);
            p0l1aNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p0l1aNumericUpDown.Name = "p0l1aNumericUpDown";
            p0l1aNumericUpDown.Size = new Size(70, 19);
            p0l1aNumericUpDown.TabIndex = 48;
            p0l1aNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label38
            // 
            label38.AutoSize = true;
            label38.Font = new Font("Microsoft Sans Serif", 10F);
            label38.Location = new Point(100, 19);
            label38.Name = "label38";
            label38.Size = new Size(20, 17);
            label38.TabIndex = 47;
            label38.Text = "a:";
            // 
            // p0l1kNumericUpDown
            // 
            p0l1kNumericUpDown.BackColor = Color.CornflowerBlue;
            p0l1kNumericUpDown.BorderStyle = BorderStyle.None;
            p0l1kNumericUpDown.DecimalPlaces = 2;
            p0l1kNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p0l1kNumericUpDown.ForeColor = Color.White;
            p0l1kNumericUpDown.Location = new Point(28, 19);
            p0l1kNumericUpDown.Margin = new Padding(0);
            p0l1kNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p0l1kNumericUpDown.Name = "p0l1kNumericUpDown";
            p0l1kNumericUpDown.Size = new Size(70, 19);
            p0l1kNumericUpDown.TabIndex = 46;
            p0l1kNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label39
            // 
            label39.AutoSize = true;
            label39.Font = new Font("Microsoft Sans Serif", 10F);
            label39.Location = new Point(6, 19);
            label39.Name = "label39";
            label39.Size = new Size(19, 17);
            label39.TabIndex = 43;
            label39.Text = "k:";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(label22);
            groupBox4.Controls.Add(p1l3aNumericUpDown);
            groupBox4.Controls.Add(label23);
            groupBox4.Controls.Add(p1l3kNumericUpDown);
            groupBox4.Controls.Add(label24);
            groupBox4.Controls.Add(p1l2xNumericUpDown);
            groupBox4.Controls.Add(label25);
            groupBox4.Controls.Add(p1l2aNumericUpDown);
            groupBox4.Controls.Add(label26);
            groupBox4.Controls.Add(p1l2kNumericUpDown);
            groupBox4.Controls.Add(label27);
            groupBox4.Controls.Add(p1l1xNumericUpDown);
            groupBox4.Controls.Add(label28);
            groupBox4.Controls.Add(p1l1aNumericUpDown);
            groupBox4.Controls.Add(label29);
            groupBox4.Controls.Add(p1l1kNumericUpDown);
            groupBox4.Controls.Add(label30);
            groupBox4.Location = new Point(313, 40);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(301, 91);
            groupBox4.TabIndex = 44;
            groupBox4.TabStop = false;
            groupBox4.Text = "Средний приоритет";
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Font = new Font("Microsoft Sans Serif", 10F);
            label22.Location = new Point(194, 63);
            label22.Name = "label22";
            label22.Size = new Size(52, 17);
            label22.TabIndex = 61;
            label22.Text = "иначе.";
            // 
            // p1l3aNumericUpDown
            // 
            p1l3aNumericUpDown.BackColor = Color.CornflowerBlue;
            p1l3aNumericUpDown.BorderStyle = BorderStyle.None;
            p1l3aNumericUpDown.DecimalPlaces = 2;
            p1l3aNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p1l3aNumericUpDown.ForeColor = Color.White;
            p1l3aNumericUpDown.Location = new Point(122, 63);
            p1l3aNumericUpDown.Margin = new Padding(0);
            p1l3aNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p1l3aNumericUpDown.Name = "p1l3aNumericUpDown";
            p1l3aNumericUpDown.Size = new Size(70, 19);
            p1l3aNumericUpDown.TabIndex = 60;
            p1l3aNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Font = new Font("Microsoft Sans Serif", 10F);
            label23.Location = new Point(100, 63);
            label23.Name = "label23";
            label23.Size = new Size(20, 17);
            label23.TabIndex = 59;
            label23.Text = "a:";
            // 
            // p1l3kNumericUpDown
            // 
            p1l3kNumericUpDown.BackColor = Color.CornflowerBlue;
            p1l3kNumericUpDown.BorderStyle = BorderStyle.None;
            p1l3kNumericUpDown.DecimalPlaces = 2;
            p1l3kNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p1l3kNumericUpDown.ForeColor = Color.White;
            p1l3kNumericUpDown.Location = new Point(28, 63);
            p1l3kNumericUpDown.Margin = new Padding(0);
            p1l3kNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p1l3kNumericUpDown.Name = "p1l3kNumericUpDown";
            p1l3kNumericUpDown.Size = new Size(70, 19);
            p1l3kNumericUpDown.TabIndex = 58;
            p1l3kNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Font = new Font("Microsoft Sans Serif", 10F);
            label24.Location = new Point(6, 63);
            label24.Name = "label24";
            label24.Size = new Size(19, 17);
            label24.TabIndex = 57;
            label24.Text = "k:";
            // 
            // p1l2xNumericUpDown
            // 
            p1l2xNumericUpDown.BackColor = Color.CornflowerBlue;
            p1l2xNumericUpDown.BorderStyle = BorderStyle.None;
            p1l2xNumericUpDown.DecimalPlaces = 2;
            p1l2xNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p1l2xNumericUpDown.ForeColor = Color.White;
            p1l2xNumericUpDown.Location = new Point(223, 41);
            p1l2xNumericUpDown.Margin = new Padding(0);
            p1l2xNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p1l2xNumericUpDown.Name = "p1l2xNumericUpDown";
            p1l2xNumericUpDown.Size = new Size(70, 19);
            p1l2xNumericUpDown.TabIndex = 56;
            p1l2xNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Font = new Font("Microsoft Sans Serif", 10F);
            label25.Location = new Point(194, 41);
            label25.Name = "label25";
            label25.Size = new Size(26, 17);
            label25.TabIndex = 55;
            label25.Text = "x <";
            // 
            // p1l2aNumericUpDown
            // 
            p1l2aNumericUpDown.BackColor = Color.CornflowerBlue;
            p1l2aNumericUpDown.BorderStyle = BorderStyle.None;
            p1l2aNumericUpDown.DecimalPlaces = 2;
            p1l2aNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p1l2aNumericUpDown.ForeColor = Color.White;
            p1l2aNumericUpDown.Location = new Point(122, 41);
            p1l2aNumericUpDown.Margin = new Padding(0);
            p1l2aNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p1l2aNumericUpDown.Name = "p1l2aNumericUpDown";
            p1l2aNumericUpDown.Size = new Size(70, 19);
            p1l2aNumericUpDown.TabIndex = 54;
            p1l2aNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Font = new Font("Microsoft Sans Serif", 10F);
            label26.Location = new Point(100, 41);
            label26.Name = "label26";
            label26.Size = new Size(20, 17);
            label26.TabIndex = 53;
            label26.Text = "a:";
            // 
            // p1l2kNumericUpDown
            // 
            p1l2kNumericUpDown.BackColor = Color.CornflowerBlue;
            p1l2kNumericUpDown.BorderStyle = BorderStyle.None;
            p1l2kNumericUpDown.DecimalPlaces = 2;
            p1l2kNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p1l2kNumericUpDown.ForeColor = Color.White;
            p1l2kNumericUpDown.Location = new Point(28, 41);
            p1l2kNumericUpDown.Margin = new Padding(0);
            p1l2kNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p1l2kNumericUpDown.Name = "p1l2kNumericUpDown";
            p1l2kNumericUpDown.Size = new Size(70, 19);
            p1l2kNumericUpDown.TabIndex = 52;
            p1l2kNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Font = new Font("Microsoft Sans Serif", 10F);
            label27.Location = new Point(6, 41);
            label27.Name = "label27";
            label27.Size = new Size(19, 17);
            label27.TabIndex = 51;
            label27.Text = "k:";
            // 
            // p1l1xNumericUpDown
            // 
            p1l1xNumericUpDown.BackColor = Color.CornflowerBlue;
            p1l1xNumericUpDown.BorderStyle = BorderStyle.None;
            p1l1xNumericUpDown.DecimalPlaces = 2;
            p1l1xNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p1l1xNumericUpDown.ForeColor = Color.White;
            p1l1xNumericUpDown.Location = new Point(223, 19);
            p1l1xNumericUpDown.Margin = new Padding(0);
            p1l1xNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p1l1xNumericUpDown.Name = "p1l1xNumericUpDown";
            p1l1xNumericUpDown.Size = new Size(70, 19);
            p1l1xNumericUpDown.TabIndex = 50;
            p1l1xNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label28
            // 
            label28.AutoSize = true;
            label28.Font = new Font("Microsoft Sans Serif", 10F);
            label28.Location = new Point(194, 19);
            label28.Name = "label28";
            label28.Size = new Size(26, 17);
            label28.TabIndex = 49;
            label28.Text = "x <";
            // 
            // p1l1aNumericUpDown
            // 
            p1l1aNumericUpDown.BackColor = Color.CornflowerBlue;
            p1l1aNumericUpDown.BorderStyle = BorderStyle.None;
            p1l1aNumericUpDown.DecimalPlaces = 2;
            p1l1aNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p1l1aNumericUpDown.ForeColor = Color.White;
            p1l1aNumericUpDown.Location = new Point(122, 19);
            p1l1aNumericUpDown.Margin = new Padding(0);
            p1l1aNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p1l1aNumericUpDown.Name = "p1l1aNumericUpDown";
            p1l1aNumericUpDown.Size = new Size(70, 19);
            p1l1aNumericUpDown.TabIndex = 48;
            p1l1aNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label29
            // 
            label29.AutoSize = true;
            label29.Font = new Font("Microsoft Sans Serif", 10F);
            label29.Location = new Point(100, 19);
            label29.Name = "label29";
            label29.Size = new Size(20, 17);
            label29.TabIndex = 47;
            label29.Text = "a:";
            // 
            // p1l1kNumericUpDown
            // 
            p1l1kNumericUpDown.BackColor = Color.CornflowerBlue;
            p1l1kNumericUpDown.BorderStyle = BorderStyle.None;
            p1l1kNumericUpDown.DecimalPlaces = 2;
            p1l1kNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p1l1kNumericUpDown.ForeColor = Color.White;
            p1l1kNumericUpDown.Location = new Point(28, 19);
            p1l1kNumericUpDown.Margin = new Padding(0);
            p1l1kNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p1l1kNumericUpDown.Name = "p1l1kNumericUpDown";
            p1l1kNumericUpDown.Size = new Size(70, 19);
            p1l1kNumericUpDown.TabIndex = 46;
            p1l1kNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label30
            // 
            label30.AutoSize = true;
            label30.Font = new Font("Microsoft Sans Serif", 10F);
            label30.Location = new Point(6, 19);
            label30.Name = "label30";
            label30.Size = new Size(19, 17);
            label30.TabIndex = 43;
            label30.Text = "k:";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Microsoft Sans Serif", 9F);
            label12.Location = new Point(6, 21);
            label12.Name = "label12";
            label12.Size = new Size(231, 15);
            label12.TabIndex = 43;
            label12.Text = "Функция: y = k*x^a, x - количество дней";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(label19);
            groupBox3.Controls.Add(p2l3aNumericUpDown);
            groupBox3.Controls.Add(label20);
            groupBox3.Controls.Add(p2l3kNumericUpDown);
            groupBox3.Controls.Add(label21);
            groupBox3.Controls.Add(p2l2xNumericUpDown);
            groupBox3.Controls.Add(label16);
            groupBox3.Controls.Add(p2l2aNumericUpDown);
            groupBox3.Controls.Add(label17);
            groupBox3.Controls.Add(p2l2kNumericUpDown);
            groupBox3.Controls.Add(label18);
            groupBox3.Controls.Add(p2l1xNumericUpDown);
            groupBox3.Controls.Add(label15);
            groupBox3.Controls.Add(p2l1aNumericUpDown);
            groupBox3.Controls.Add(label14);
            groupBox3.Controls.Add(p2l1kNumericUpDown);
            groupBox3.Controls.Add(label13);
            groupBox3.Location = new Point(6, 40);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(301, 91);
            groupBox3.TabIndex = 0;
            groupBox3.TabStop = false;
            groupBox3.Text = "Низкий приоритет";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Font = new Font("Microsoft Sans Serif", 10F);
            label19.Location = new Point(194, 63);
            label19.Name = "label19";
            label19.Size = new Size(52, 17);
            label19.TabIndex = 61;
            label19.Text = "иначе.";
            // 
            // p2l3aNumericUpDown
            // 
            p2l3aNumericUpDown.BackColor = Color.CornflowerBlue;
            p2l3aNumericUpDown.BorderStyle = BorderStyle.None;
            p2l3aNumericUpDown.DecimalPlaces = 2;
            p2l3aNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p2l3aNumericUpDown.ForeColor = Color.White;
            p2l3aNumericUpDown.Location = new Point(122, 63);
            p2l3aNumericUpDown.Margin = new Padding(0);
            p2l3aNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p2l3aNumericUpDown.Name = "p2l3aNumericUpDown";
            p2l3aNumericUpDown.Size = new Size(70, 19);
            p2l3aNumericUpDown.TabIndex = 60;
            p2l3aNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Font = new Font("Microsoft Sans Serif", 10F);
            label20.Location = new Point(100, 63);
            label20.Name = "label20";
            label20.Size = new Size(20, 17);
            label20.TabIndex = 59;
            label20.Text = "a:";
            // 
            // p2l3kNumericUpDown
            // 
            p2l3kNumericUpDown.BackColor = Color.CornflowerBlue;
            p2l3kNumericUpDown.BorderStyle = BorderStyle.None;
            p2l3kNumericUpDown.DecimalPlaces = 2;
            p2l3kNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p2l3kNumericUpDown.ForeColor = Color.White;
            p2l3kNumericUpDown.Location = new Point(28, 63);
            p2l3kNumericUpDown.Margin = new Padding(0);
            p2l3kNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p2l3kNumericUpDown.Name = "p2l3kNumericUpDown";
            p2l3kNumericUpDown.Size = new Size(70, 19);
            p2l3kNumericUpDown.TabIndex = 58;
            p2l3kNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Font = new Font("Microsoft Sans Serif", 10F);
            label21.Location = new Point(6, 63);
            label21.Name = "label21";
            label21.Size = new Size(19, 17);
            label21.TabIndex = 57;
            label21.Text = "k:";
            // 
            // p2l2xNumericUpDown
            // 
            p2l2xNumericUpDown.BackColor = Color.CornflowerBlue;
            p2l2xNumericUpDown.BorderStyle = BorderStyle.None;
            p2l2xNumericUpDown.DecimalPlaces = 2;
            p2l2xNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p2l2xNumericUpDown.ForeColor = Color.White;
            p2l2xNumericUpDown.Location = new Point(223, 41);
            p2l2xNumericUpDown.Margin = new Padding(0);
            p2l2xNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p2l2xNumericUpDown.Name = "p2l2xNumericUpDown";
            p2l2xNumericUpDown.Size = new Size(70, 19);
            p2l2xNumericUpDown.TabIndex = 56;
            p2l2xNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Microsoft Sans Serif", 10F);
            label16.Location = new Point(194, 41);
            label16.Name = "label16";
            label16.Size = new Size(26, 17);
            label16.TabIndex = 55;
            label16.Text = "x <";
            // 
            // p2l2aNumericUpDown
            // 
            p2l2aNumericUpDown.BackColor = Color.CornflowerBlue;
            p2l2aNumericUpDown.BorderStyle = BorderStyle.None;
            p2l2aNumericUpDown.DecimalPlaces = 2;
            p2l2aNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p2l2aNumericUpDown.ForeColor = Color.White;
            p2l2aNumericUpDown.Location = new Point(122, 41);
            p2l2aNumericUpDown.Margin = new Padding(0);
            p2l2aNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p2l2aNumericUpDown.Name = "p2l2aNumericUpDown";
            p2l2aNumericUpDown.Size = new Size(70, 19);
            p2l2aNumericUpDown.TabIndex = 54;
            p2l2aNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Font = new Font("Microsoft Sans Serif", 10F);
            label17.Location = new Point(100, 41);
            label17.Name = "label17";
            label17.Size = new Size(20, 17);
            label17.TabIndex = 53;
            label17.Text = "a:";
            // 
            // p2l2kNumericUpDown
            // 
            p2l2kNumericUpDown.BackColor = Color.CornflowerBlue;
            p2l2kNumericUpDown.BorderStyle = BorderStyle.None;
            p2l2kNumericUpDown.DecimalPlaces = 2;
            p2l2kNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p2l2kNumericUpDown.ForeColor = Color.White;
            p2l2kNumericUpDown.Location = new Point(28, 41);
            p2l2kNumericUpDown.Margin = new Padding(0);
            p2l2kNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p2l2kNumericUpDown.Name = "p2l2kNumericUpDown";
            p2l2kNumericUpDown.Size = new Size(70, 19);
            p2l2kNumericUpDown.TabIndex = 52;
            p2l2kNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Font = new Font("Microsoft Sans Serif", 10F);
            label18.Location = new Point(6, 41);
            label18.Name = "label18";
            label18.Size = new Size(19, 17);
            label18.TabIndex = 51;
            label18.Text = "k:";
            // 
            // p2l1xNumericUpDown
            // 
            p2l1xNumericUpDown.BackColor = Color.CornflowerBlue;
            p2l1xNumericUpDown.BorderStyle = BorderStyle.None;
            p2l1xNumericUpDown.DecimalPlaces = 2;
            p2l1xNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p2l1xNumericUpDown.ForeColor = Color.White;
            p2l1xNumericUpDown.Location = new Point(223, 19);
            p2l1xNumericUpDown.Margin = new Padding(0);
            p2l1xNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p2l1xNumericUpDown.Name = "p2l1xNumericUpDown";
            p2l1xNumericUpDown.Size = new Size(70, 19);
            p2l1xNumericUpDown.TabIndex = 50;
            p2l1xNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Microsoft Sans Serif", 10F);
            label15.Location = new Point(194, 19);
            label15.Name = "label15";
            label15.Size = new Size(26, 17);
            label15.TabIndex = 49;
            label15.Text = "x <";
            // 
            // p2l1aNumericUpDown
            // 
            p2l1aNumericUpDown.BackColor = Color.CornflowerBlue;
            p2l1aNumericUpDown.BorderStyle = BorderStyle.None;
            p2l1aNumericUpDown.DecimalPlaces = 2;
            p2l1aNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p2l1aNumericUpDown.ForeColor = Color.White;
            p2l1aNumericUpDown.Location = new Point(122, 19);
            p2l1aNumericUpDown.Margin = new Padding(0);
            p2l1aNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p2l1aNumericUpDown.Name = "p2l1aNumericUpDown";
            p2l1aNumericUpDown.Size = new Size(70, 19);
            p2l1aNumericUpDown.TabIndex = 48;
            p2l1aNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Microsoft Sans Serif", 10F);
            label14.Location = new Point(100, 19);
            label14.Name = "label14";
            label14.Size = new Size(20, 17);
            label14.TabIndex = 47;
            label14.Text = "a:";
            // 
            // p2l1kNumericUpDown
            // 
            p2l1kNumericUpDown.BackColor = Color.CornflowerBlue;
            p2l1kNumericUpDown.BorderStyle = BorderStyle.None;
            p2l1kNumericUpDown.DecimalPlaces = 2;
            p2l1kNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            p2l1kNumericUpDown.ForeColor = Color.White;
            p2l1kNumericUpDown.Location = new Point(28, 19);
            p2l1kNumericUpDown.Margin = new Padding(0);
            p2l1kNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            p2l1kNumericUpDown.Name = "p2l1kNumericUpDown";
            p2l1kNumericUpDown.Size = new Size(70, 19);
            p2l1kNumericUpDown.TabIndex = 46;
            p2l1kNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Microsoft Sans Serif", 10F);
            label13.Location = new Point(6, 19);
            label13.Name = "label13";
            label13.Size = new Size(19, 17);
            label13.TabIndex = 43;
            label13.Text = "k:";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(CyclesNumericUpDown);
            groupBox1.Controls.Add(AfterXCyclesRadioButton);
            groupBox1.Controls.Add(MinutesNumericUpDown);
            groupBox1.Controls.Add(IterationsNumericUpDown);
            groupBox1.Controls.Add(AfterXMinsRadioButton);
            groupBox1.Controls.Add(AfterXItersRadioButton);
            groupBox1.Location = new Point(3, 139);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(315, 104);
            groupBox1.TabIndex = 41;
            groupBox1.TabStop = false;
            groupBox1.Text = "Условие останова";
            // 
            // CyclesNumericUpDown
            // 
            CyclesNumericUpDown.BackColor = Color.CornflowerBlue;
            CyclesNumericUpDown.BorderStyle = BorderStyle.None;
            CyclesNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            CyclesNumericUpDown.ForeColor = Color.White;
            CyclesNumericUpDown.Location = new Point(221, 73);
            CyclesNumericUpDown.Margin = new Padding(0);
            CyclesNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            CyclesNumericUpDown.Name = "CyclesNumericUpDown";
            CyclesNumericUpDown.Size = new Size(84, 19);
            CyclesNumericUpDown.TabIndex = 45;
            CyclesNumericUpDown.TextAlign = HorizontalAlignment.Center;
            CyclesNumericUpDown.Value = new decimal(new int[] { 21, 0, 0, 0 });
            // 
            // AfterXCyclesRadioButton
            // 
            AfterXCyclesRadioButton.BackColor = Color.Black;
            AfterXCyclesRadioButton.Depth = 0;
            AfterXCyclesRadioButton.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            AfterXCyclesRadioButton.ForeColor = Color.CornflowerBlue;
            AfterXCyclesRadioButton.Location = new Point(3, 73);
            AfterXCyclesRadioButton.Margin = new Padding(0);
            AfterXCyclesRadioButton.MouseLocation = new Point(-1, -1);
            AfterXCyclesRadioButton.Name = "AfterXCyclesRadioButton";
            AfterXCyclesRadioButton.Ripple = true;
            AfterXCyclesRadioButton.Size = new Size(179, 24);
            AfterXCyclesRadioButton.TabIndex = 44;
            AfterXCyclesRadioButton.Text = "После Х циклов, где Х:";
            AfterXCyclesRadioButton.UseVisualStyleBackColor = false;
            // 
            // MinutesNumericUpDown
            // 
            MinutesNumericUpDown.BackColor = Color.CornflowerBlue;
            MinutesNumericUpDown.BorderStyle = BorderStyle.None;
            MinutesNumericUpDown.DecimalPlaces = 2;
            MinutesNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            MinutesNumericUpDown.ForeColor = Color.White;
            MinutesNumericUpDown.Location = new Point(221, 46);
            MinutesNumericUpDown.Margin = new Padding(0);
            MinutesNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            MinutesNumericUpDown.Name = "MinutesNumericUpDown";
            MinutesNumericUpDown.Size = new Size(84, 19);
            MinutesNumericUpDown.TabIndex = 43;
            MinutesNumericUpDown.TextAlign = HorizontalAlignment.Center;
            MinutesNumericUpDown.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // IterationsNumericUpDown
            // 
            IterationsNumericUpDown.BackColor = Color.CornflowerBlue;
            IterationsNumericUpDown.BorderStyle = BorderStyle.None;
            IterationsNumericUpDown.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            IterationsNumericUpDown.ForeColor = Color.White;
            IterationsNumericUpDown.Location = new Point(221, 19);
            IterationsNumericUpDown.Margin = new Padding(0);
            IterationsNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            IterationsNumericUpDown.Name = "IterationsNumericUpDown";
            IterationsNumericUpDown.Size = new Size(84, 19);
            IterationsNumericUpDown.TabIndex = 42;
            IterationsNumericUpDown.TextAlign = HorizontalAlignment.Center;
            IterationsNumericUpDown.Value = new decimal(new int[] { 5000, 0, 0, 0 });
            // 
            // AfterXMinsRadioButton
            // 
            AfterXMinsRadioButton.BackColor = Color.Black;
            AfterXMinsRadioButton.Depth = 0;
            AfterXMinsRadioButton.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            AfterXMinsRadioButton.ForeColor = Color.CornflowerBlue;
            AfterXMinsRadioButton.Location = new Point(3, 46);
            AfterXMinsRadioButton.Margin = new Padding(0);
            AfterXMinsRadioButton.MouseLocation = new Point(-1, -1);
            AfterXMinsRadioButton.Name = "AfterXMinsRadioButton";
            AfterXMinsRadioButton.Ripple = true;
            AfterXMinsRadioButton.Size = new Size(207, 24);
            AfterXMinsRadioButton.TabIndex = 20;
            AfterXMinsRadioButton.Text = "После Х минут, где Х:";
            AfterXMinsRadioButton.UseVisualStyleBackColor = false;
            // 
            // AfterXItersRadioButton
            // 
            AfterXItersRadioButton.BackColor = Color.Black;
            AfterXItersRadioButton.Checked = true;
            AfterXItersRadioButton.Depth = 0;
            AfterXItersRadioButton.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            AfterXItersRadioButton.ForeColor = Color.CornflowerBlue;
            AfterXItersRadioButton.Location = new Point(3, 19);
            AfterXItersRadioButton.Margin = new Padding(0);
            AfterXItersRadioButton.MouseLocation = new Point(-1, -1);
            AfterXItersRadioButton.Name = "AfterXItersRadioButton";
            AfterXItersRadioButton.Ripple = true;
            AfterXItersRadioButton.Size = new Size(207, 24);
            AfterXItersRadioButton.TabIndex = 19;
            AfterXItersRadioButton.TabStop = true;
            AfterXItersRadioButton.Text = "После Х итераций, где Х:";
            AfterXItersRadioButton.UseVisualStyleBackColor = false;
            // 
            // ClearAllSessionErrorsButton
            // 
            ClearAllSessionErrorsButton.BackColor = Color.CornflowerBlue;
            ClearAllSessionErrorsButton.BackgroundImageLayout = ImageLayout.Zoom;
            ClearAllSessionErrorsButton.Cursor = Cursors.Hand;
            ClearAllSessionErrorsButton.FlatAppearance.BorderSize = 0;
            ClearAllSessionErrorsButton.FlatStyle = FlatStyle.Flat;
            ClearAllSessionErrorsButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            ClearAllSessionErrorsButton.ForeColor = Color.White;
            ClearAllSessionErrorsButton.Location = new Point(322, 249);
            ClearAllSessionErrorsButton.Name = "ClearAllSessionErrorsButton";
            ClearAllSessionErrorsButton.Size = new Size(315, 51);
            ClearAllSessionErrorsButton.TabIndex = 38;
            ClearAllSessionErrorsButton.Text = "Удалить все ошибки сессии";
            ClearAllSessionErrorsButton.UseCompatibleTextRendering = true;
            ClearAllSessionErrorsButton.UseVisualStyleBackColor = false;
            ClearAllSessionErrorsButton.Click += ClearAllSessionErrorsButton_Click;
            // 
            // HandSessionPostDataProcessButton
            // 
            HandSessionPostDataProcessButton.BackColor = Color.CornflowerBlue;
            HandSessionPostDataProcessButton.BackgroundImageLayout = ImageLayout.Zoom;
            HandSessionPostDataProcessButton.Cursor = Cursors.Hand;
            HandSessionPostDataProcessButton.FlatAppearance.BorderSize = 0;
            HandSessionPostDataProcessButton.FlatStyle = FlatStyle.Flat;
            HandSessionPostDataProcessButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            HandSessionPostDataProcessButton.ForeColor = Color.White;
            HandSessionPostDataProcessButton.Location = new Point(3, 249);
            HandSessionPostDataProcessButton.Name = "HandSessionPostDataProcessButton";
            HandSessionPostDataProcessButton.Size = new Size(315, 51);
            HandSessionPostDataProcessButton.TabIndex = 37;
            HandSessionPostDataProcessButton.Text = "Ручной пост-процессинг данных сессии";
            HandSessionPostDataProcessButton.UseCompatibleTextRendering = true;
            HandSessionPostDataProcessButton.UseVisualStyleBackColor = false;
            HandSessionPostDataProcessButton.Click += HandSessionPostDataProcessButton_Click;
            // 
            // SaveSettingsButton
            // 
            SaveSettingsButton.BackColor = Color.CornflowerBlue;
            SaveSettingsButton.BackgroundImageLayout = ImageLayout.Zoom;
            SaveSettingsButton.Cursor = Cursors.Hand;
            SaveSettingsButton.FlatAppearance.BorderSize = 0;
            SaveSettingsButton.FlatStyle = FlatStyle.Flat;
            SaveSettingsButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            SaveSettingsButton.ForeColor = Color.White;
            SaveSettingsButton.Location = new Point(3, 3);
            SaveSettingsButton.Name = "SaveSettingsButton";
            SaveSettingsButton.Size = new Size(254, 51);
            SaveSettingsButton.TabIndex = 34;
            SaveSettingsButton.Text = "Сохранить настройки";
            SaveSettingsButton.UseVisualStyleBackColor = false;
            // 
            // ToggleThemeButton
            // 
            ToggleThemeButton.BackColor = Color.CornflowerBlue;
            ToggleThemeButton.BackgroundImageLayout = ImageLayout.Zoom;
            ToggleThemeButton.Cursor = Cursors.Hand;
            ToggleThemeButton.FlatAppearance.BorderSize = 0;
            ToggleThemeButton.FlatStyle = FlatStyle.Flat;
            ToggleThemeButton.Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Regular, GraphicsUnit.Point, 204);
            ToggleThemeButton.ForeColor = Color.White;
            ToggleThemeButton.Location = new Point(263, 3);
            ToggleThemeButton.Name = "ToggleThemeButton";
            ToggleThemeButton.Size = new Size(55, 51);
            ToggleThemeButton.TabIndex = 22;
            ToggleThemeButton.Text = "☾\r\n ";
            ToggleThemeButton.TextAlign = ContentAlignment.TopCenter;
            ToggleThemeButton.UseVisualStyleBackColor = false;
            // 
            // BackupStagesDurationLabel
            // 
            BackupStagesDurationLabel.AutoSize = true;
            BackupStagesDurationLabel.Font = new Font("Microsoft Sans Serif", 9F);
            BackupStagesDurationLabel.Location = new Point(8, 59);
            BackupStagesDurationLabel.Name = "BackupStagesDurationLabel";
            BackupStagesDurationLabel.Size = new Size(153, 15);
            BackupStagesDurationLabel.TabIndex = 31;
            BackupStagesDurationLabel.Text = "Резерв. длит. этапов (с.):";
            // 
            // BackupStagesDurationNumericUpDown
            // 
            BackupStagesDurationNumericUpDown.BackColor = Color.CornflowerBlue;
            BackupStagesDurationNumericUpDown.BorderStyle = BorderStyle.None;
            BackupStagesDurationNumericUpDown.DecimalPlaces = 2;
            BackupStagesDurationNumericUpDown.ForeColor = Color.White;
            BackupStagesDurationNumericUpDown.Location = new Point(234, 58);
            BackupStagesDurationNumericUpDown.Margin = new Padding(0);
            BackupStagesDurationNumericUpDown.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            BackupStagesDurationNumericUpDown.Name = "BackupStagesDurationNumericUpDown";
            BackupStagesDurationNumericUpDown.Size = new Size(84, 19);
            BackupStagesDurationNumericUpDown.TabIndex = 30;
            BackupStagesDurationNumericUpDown.TextAlign = HorizontalAlignment.Center;
            BackupStagesDurationNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // PartiasMaxDaysLabel
            // 
            PartiasMaxDaysLabel.AutoSize = true;
            PartiasMaxDaysLabel.Font = new Font("Microsoft Sans Serif", 9F);
            PartiasMaxDaysLabel.Location = new Point(8, 85);
            PartiasMaxDaysLabel.Name = "PartiasMaxDaysLabel";
            PartiasMaxDaysLabel.Size = new Size(175, 15);
            PartiasMaxDaysLabel.TabIndex = 29;
            PartiasMaxDaysLabel.Text = "Макс. дней между партиями:";
            // 
            // PartiasMaxDaysNumericUpDown
            // 
            PartiasMaxDaysNumericUpDown.BackColor = Color.CornflowerBlue;
            PartiasMaxDaysNumericUpDown.BorderStyle = BorderStyle.None;
            PartiasMaxDaysNumericUpDown.DecimalPlaces = 2;
            PartiasMaxDaysNumericUpDown.ForeColor = Color.White;
            PartiasMaxDaysNumericUpDown.Location = new Point(234, 84);
            PartiasMaxDaysNumericUpDown.Margin = new Padding(0);
            PartiasMaxDaysNumericUpDown.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            PartiasMaxDaysNumericUpDown.Name = "PartiasMaxDaysNumericUpDown";
            PartiasMaxDaysNumericUpDown.Size = new Size(84, 19);
            PartiasMaxDaysNumericUpDown.TabIndex = 28;
            PartiasMaxDaysNumericUpDown.TextAlign = HorizontalAlignment.Center;
            PartiasMaxDaysNumericUpDown.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // AuthorsTabPage
            // 
            AuthorsTabPage.Controls.Add(label53);
            AuthorsTabPage.Controls.Add(label50);
            AuthorsTabPage.Controls.Add(pictureBox2);
            AuthorsTabPage.Controls.Add(label52);
            AuthorsTabPage.Location = new Point(4, 24);
            AuthorsTabPage.Name = "AuthorsTabPage";
            AuthorsTabPage.Size = new Size(1171, 502);
            AuthorsTabPage.TabIndex = 4;
            AuthorsTabPage.Text = "Разработчики";
            AuthorsTabPage.UseVisualStyleBackColor = true;
            // 
            // label53
            // 
            label53.AutoSize = true;
            label53.Font = new Font("Segoe UI Light", 14F);
            label53.Location = new Point(286, 61);
            label53.Name = "label53";
            label53.Size = new Size(279, 150);
            label53.TabIndex = 29;
            label53.Text = "Трушин Владислав Алексеевич\r\nФролов Андрей Денисович\r\nКосухина Марина Михайловна\r\nОмерова Алёна Сергеевна\r\nАнисов Ян Иванович\r\nМаракушев Алексей Васильевич";
            // 
            // label50
            // 
            label50.AutoSize = true;
            label50.Font = new Font("Segoe UI Light", 14F, FontStyle.Bold);
            label50.Location = new Point(24, 320);
            label50.Name = "label50";
            label50.RightToLeft = RightToLeft.No;
            label50.Size = new Size(190, 75);
            label50.TabIndex = 28;
            label50.Text = "ООО «МОСИТЛАБ»\r\n+7 (495) 280-82-99\r\nwww.mositlab.ru";
            label50.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(24, 61);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(256, 256);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 27;
            pictureBox2.TabStop = false;
            // 
            // label52
            // 
            label52.AutoSize = true;
            label52.Font = new Font("Segoe UI Light", 22.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            label52.Location = new Point(24, 9);
            label52.Name = "label52";
            label52.Size = new Size(686, 41);
            label52.TabIndex = 25;
            label52.Text = "Система оптимизации графика производства (APS)";
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            openFileDialog1.Filter = "Текстовые таблицы (TXT)|*.txt|Excel-файлы|*.XLSX|CSV-таблицы|*.csv|JSON-конфигураторы|*.json";
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(20, 20);
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // ApsRTMainForm1
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1179, 565);
            Controls.Add(MainTabControl);
            Controls.Add(materialTabSelector1);
            Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new Size(1195, 604);
            Name = "ApsRTMainForm1";
            Text = "Система оптимизации графика производства (APS)";
            MainTabControl.ResumeLayout(false);
            MainTabPage.ResumeLayout(false);
            MainTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            PartiasTabPage.ResumeLayout(false);
            PartiasTabControl.ResumeLayout(false);
            AllPartiasTabControl.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PartiasDataTable).EndInit();
            HandPartiasTabControl.ResumeLayout(false);
            HandPartiasTabControl.PerformLayout();
            splitContainer4.Panel1.ResumeLayout(false);
            splitContainer4.Panel1.PerformLayout();
            splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer4).EndInit();
            splitContainer4.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            PlanningTabPage.ResumeLayout(false);
            PlanningTabControl.ResumeLayout(false);
            TyrantTabPage.ResumeLayout(false);
            splitContainer5.Panel1.ResumeLayout(false);
            splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer5).EndInit();
            splitContainer5.ResumeLayout(false);
            splitContainer7.Panel1.ResumeLayout(false);
            splitContainer7.Panel1.PerformLayout();
            splitContainer7.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer7).EndInit();
            splitContainer7.ResumeLayout(false);
            splitContainer8.Panel1.ResumeLayout(false);
            splitContainer8.Panel2.ResumeLayout(false);
            splitContainer8.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer8).EndInit();
            splitContainer8.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PlanningDataTable).EndInit();
            splitContainer6.Panel1.ResumeLayout(false);
            splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer6).EndInit();
            splitContainer6.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            HandPlanningPage.ResumeLayout(false);
            splitContainer17.Panel1.ResumeLayout(false);
            splitContainer17.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer17).EndInit();
            splitContainer17.ResumeLayout(false);
            splitContainer18.Panel1.ResumeLayout(false);
            splitContainer18.Panel2.ResumeLayout(false);
            splitContainer18.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer18).EndInit();
            splitContainer18.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)ManualPlanningDataTable).EndInit();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            GraphsTabPage.ResumeLayout(false);
            GraphsMaterialTabControl.ResumeLayout(false);
            tabPage8.ResumeLayout(false);
            splitContainer14.Panel1.ResumeLayout(false);
            splitContainer14.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer14).EndInit();
            splitContainer14.ResumeLayout(false);
            splitContainer15.Panel1.ResumeLayout(false);
            splitContainer15.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer15).EndInit();
            splitContainer15.ResumeLayout(false);
            splitContainer16.Panel1.ResumeLayout(false);
            splitContainer16.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer16).EndInit();
            splitContainer16.ResumeLayout(false);
            tabPage9.ResumeLayout(false);
            splitContainer11.Panel1.ResumeLayout(false);
            splitContainer11.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer11).EndInit();
            splitContainer11.ResumeLayout(false);
            splitContainer12.Panel1.ResumeLayout(false);
            splitContainer12.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer12).EndInit();
            splitContainer12.ResumeLayout(false);
            splitContainer13.Panel1.ResumeLayout(false);
            splitContainer13.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer13).EndInit();
            splitContainer13.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            splitContainer9.Panel1.ResumeLayout(false);
            splitContainer9.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer9).EndInit();
            splitContainer9.ResumeLayout(false);
            splitContainer10.Panel1.ResumeLayout(false);
            splitContainer10.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer10).EndInit();
            splitContainer10.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            tabPage5.ResumeLayout(false);
            SettingsTabPage.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            groupBox8.ResumeLayout(false);
            groupBox8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)StagesPercentNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)TMaxDaysNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)TMaxDaysPercentNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)GammaSlider).EndInit();
            ((System.ComponentModel.ISupportInitialize)MaxOldSessionPathsDaysNumericUpDown).EndInit();
            groupBox7.ResumeLayout(false);
            groupBox7.PerformLayout();
            groupBox6.ResumeLayout(false);
            groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown7).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown7).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown7).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown6).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown6).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown6).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown5).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown5).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown5).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown4).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown4).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown4).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown3).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown3).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown3).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown2).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown2).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown2).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMaxIterationsNumericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasMinIterationsNumericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriteriasPossibleWorseNumericUpDown1).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)p0l3aNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p0l3kNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p0l2xNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p0l2aNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p0l2kNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p0l1xNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p0l1aNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p0l1kNumericUpDown).EndInit();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)p1l3aNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p1l3kNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p1l2xNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p1l2aNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p1l2kNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p1l1xNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p1l1aNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p1l1kNumericUpDown).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)p2l3aNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p2l3kNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p2l2xNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p2l2aNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p2l2kNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p2l1xNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p2l1aNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)p2l1kNumericUpDown).EndInit();
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)CyclesNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)MinutesNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)IterationsNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)BackupStagesDurationNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)PartiasMaxDaysNumericUpDown).EndInit();
            AuthorsTabPage.ResumeLayout(false);
            AuthorsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Button OpenPartiasButton;
        public Button OpenPlanningButton;
        private WinFormsCustomComponents.MaterialTabSelector materialTabSelector1;
        private WinFormsCustomComponents.MaterialTabControl MainTabControl;
        private TabPage PartiasTabPage;
        private TabPage PlanningTabPage;
        private TabPage MainTabPage;
        private TabPage SettingsTabPage;
        private Label mainlabel;
        private WinFormsCustomComponents.MaterialCheckBox PartCheckBox;
        private WinFormsCustomComponents.CustomProgressBar PartiasProgressBar;
        public Button OpenGraphsButton;
        public Button OpenHandPlanButton;
        private Button OpenHandPartiasButton;
        public Button OpenSettingsButton;
        private Panel PanelNearMainLabel;
        private WinFormsCustomComponents.MaterialTabSelector materialTabSelector2;
        private WinFormsCustomComponents.MaterialTabControl PartiasTabControl;
        private TabPage AllPartiasTabControl;
        private TabPage HandPartiasTabControl;
        private WinFormsCustomComponents.MaterialCheckBox ZadelCheckBox;
        private Button StartAlgorithmPButton;
        private WinFormsCustomComponents.MaterialRadioButton AfterXItersRadioButton;
        private Button ToggleThemeButton;
        private SplitContainer splitContainer1;
        private Label PartiasProgressLabel;
        private Label label2;
        private WinFormsCustomComponents.MaterialTabControl PlanningTabControl;
        private TabPage TyrantTabPage;
        private TabPage HandPlanningPage;
        private TabPage GraphsTabPage;
        private WinFormsCustomComponents.MaterialTabSelector materialTabSelector3;
        private Panel HoppersPanel;
        private MaterialCheckBox AutoLoadCheckBox;
        private MaterialCheckBox SaveOnExitCheckBox;
        private MaterialCheckBox CreateStopMe;
        private MaterialCheckBox CreateKillMeCheckBox;
        private Label PartiasMaxDaysLabel;
        private NumericUpDown PartiasMaxDaysNumericUpDown;
        private Label BackupStagesDurationLabel;
        private NumericUpDown BackupStagesDurationNumericUpDown;
        private MaterialCheckBox HideConsoleCheckBox;
        private Button SaveSettingsButton;
        private FolderBrowserDialog folderBrowserDialog1;
        private OpenFileDialog openFileDialog1;
        private MaterialCheckBox UsePseudoTaraCheckBox;
        private MaterialCheckBox LogErrorsCheckBox;
        private SplitContainer splitContainer2;
        private TextBox AllForAllTextBox;
        private SplitContainer splitContainer3;
        private DataGridView PartiasDataTable;
        private CustomTreeView CustomTreeViewOnPPanel;
        private MaterialCheckBox LogPartiasCheckBox;
        private Button HandSessionPostDataProcessButton;
        private ComboBox AllForAllPartiasGuidComboBox;
        private ComboBox AllForAllOrdersComboBox;
        private Label label1;
        private Button ClearAllSessionErrorsButton;
        private ComboBox TwoPartiasGP2ComboBox;
        private ComboBox TwoPartiasGP1ComboBox;
        private ComboBox TwoPartiasOrder2ComboBox;
        private ComboBox TwoPartiasOrder1ComboBox;
        private ComboBox TwoPartiasPartia2ComboBox;
        private Button TwoPartiasStartButton;
        private ComboBox TwoPartiasPartia1ComboBox;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label5;
        private Label label4;
        private Label label3;
        private SplitContainer splitContainer4;
        private TextBox TwoPartiasTextBox;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private CustomTreeView TwoPartiasPartia1View;
        private TabPage tabPage2;
        private CustomTreeView TwoPartiasPartia2View;
        private TabPage tabPage3;
        private CustomTreeView TwoPartiasPartiaResView;
        private MaterialCheckBox InGPCheckBox;
        private SplitContainer splitContainer5;
        private SplitContainer splitContainer6;
        private Panel panel1;
        private SplitContainer splitContainer7;
        private SplitContainer splitContainer8;
        private TextBox BarDescTextBox;
        private DataGridView PlanningDataTable;
        private Label PlanningProcessLabel;
        private Button LaunchPlanningButton;
        private CustomProgressBar PlanningProcessBar;
        private Button StopPlanningButton;
        private Button OutputButton;
        private GanttChart ganttOLD;
        private GanttChart ganttNEW;
        private VScrollBar vScrollBarOLD;
        private VScrollBar vScrollBarNEW;
        private DateTimePicker dateTimePickerFrom;
        private Label label11;
        private DateTimePicker dateTimePickerTo;
        private Label label10;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private Label label9;
        private TextBox VKoeffsTextBox;
        private GroupBox groupBox1;
        private NumericUpDown IterationsNumericUpDown;
        private MaterialRadioButton AfterXMinsRadioButton;
        private NumericUpDown CyclesNumericUpDown;
        private MaterialRadioButton AfterXCyclesRadioButton;
        private NumericUpDown MinutesNumericUpDown;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private Label label12;
        private Label label19;
        private NumericUpDown p2l3aNumericUpDown;
        private Label label20;
        private NumericUpDown p2l3kNumericUpDown;
        private Label label21;
        private NumericUpDown p2l2xNumericUpDown;
        private Label label16;
        private NumericUpDown p2l2aNumericUpDown;
        private Label label17;
        private NumericUpDown p2l2kNumericUpDown;
        private Label label18;
        private NumericUpDown p2l1xNumericUpDown;
        private Label label15;
        private NumericUpDown p2l1aNumericUpDown;
        private Label label14;
        private NumericUpDown p2l1kNumericUpDown;
        private Label label13;
        private GroupBox groupBox5;
        private Label label31;
        private NumericUpDown p0l3aNumericUpDown;
        private Label label32;
        private NumericUpDown p0l3kNumericUpDown;
        private Label label33;
        private NumericUpDown p0l2xNumericUpDown;
        private Label label34;
        private NumericUpDown p0l2aNumericUpDown;
        private Label label35;
        private NumericUpDown p0l2kNumericUpDown;
        private Label label36;
        private NumericUpDown p0l1xNumericUpDown;
        private Label label37;
        private NumericUpDown p0l1aNumericUpDown;
        private Label label38;
        private NumericUpDown p0l1kNumericUpDown;
        private Label label39;
        private GroupBox groupBox4;
        private Label label22;
        private NumericUpDown p1l3aNumericUpDown;
        private Label label23;
        private NumericUpDown p1l3kNumericUpDown;
        private Label label24;
        private NumericUpDown p1l2xNumericUpDown;
        private Label label25;
        private NumericUpDown p1l2aNumericUpDown;
        private Label label26;
        private NumericUpDown p1l2kNumericUpDown;
        private Label label27;
        private NumericUpDown p1l1xNumericUpDown;
        private Label label28;
        private NumericUpDown p1l1aNumericUpDown;
        private Label label29;
        private NumericUpDown p1l1kNumericUpDown;
        private Label label30;
        private GroupBox groupBox6;
        private Label label40;
        private MaterialRadioButton materialRadioButtonCombo;
        private TextBox CriteriasListTextBox;
        private MaterialRadioButton criteriaRadioButton6;
        private MaterialRadioButton criteriaRadioButton5;
        private MaterialRadioButton criteriaRadioButton4;
        private MaterialRadioButton criteriaRadioButton3;
        private MaterialRadioButton criteriaRadioButton2;
        private MaterialRadioButton criteriaRadioButton1;
        private GroupBox groupBox7;
        private MaterialCheckBox UseOnlyMaxCriteriasSettingsCheckBox;
        private NumericUpDown CriteriasMaxIterationsNumericUpDown1;
        private NumericUpDown CriteriasMinIterationsNumericUpDown1;
        private NumericUpDown CriteriasPossibleWorseNumericUpDown1;
        private NumericUpDown CriteriasMaxIterationsNumericUpDown7;
        private NumericUpDown CriteriasMinIterationsNumericUpDown7;
        private NumericUpDown CriteriasPossibleWorseNumericUpDown7;
        private NumericUpDown CriteriasMaxIterationsNumericUpDown6;
        private NumericUpDown CriteriasMinIterationsNumericUpDown6;
        private NumericUpDown CriteriasPossibleWorseNumericUpDown6;
        private NumericUpDown CriteriasMaxIterationsNumericUpDown5;
        private NumericUpDown CriteriasMinIterationsNumericUpDown5;
        private NumericUpDown CriteriasPossibleWorseNumericUpDown5;
        private NumericUpDown CriteriasMaxIterationsNumericUpDown4;
        private NumericUpDown CriteriasMinIterationsNumericUpDown4;
        private NumericUpDown CriteriasPossibleWorseNumericUpDown4;
        private NumericUpDown CriteriasMaxIterationsNumericUpDown3;
        private NumericUpDown CriteriasMinIterationsNumericUpDown3;
        private NumericUpDown CriteriasPossibleWorseNumericUpDown3;
        private NumericUpDown CriteriasMaxIterationsNumericUpDown2;
        private NumericUpDown CriteriasMinIterationsNumericUpDown2;
        private NumericUpDown CriteriasPossibleWorseNumericUpDown2;
        private Label label43;
        private Label label42;
        private Label label41;
        private MaterialRadioButton materialRadioButtonAuto;
        private MaterialRadioButton criteriaRadioButton7;
        private MaterialCheckBox StagesColorCheckBox;
        private MaterialCheckBox ModsColorCheckBox;
        private DataGridViewTextBoxColumn Column6;
        private DataGridViewTextBoxColumn Column7;
        private DataGridViewTextBoxColumn Column8;
        private DataGridViewTextBoxColumn Column9;
        private MaterialTabControl GraphsMaterialTabControl;
        private TabPage tabPage8;
        private TabPage tabPage9;
        private MaterialTabSelector materialTabSelector4;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private ZedGraphControl AllPercentagesGraphControl;
        private SplitContainer splitContainer9;
        private Panel panel2;
        private SplitContainer splitContainer10;
        private SplitContainer splitContainer14;
        private SplitContainer splitContainer15;
        private ZedGraphControl OrdersGraphControl;
        private ZedGraphControl ProstoiGraphControl;
        private SplitContainer splitContainer16;
        private ZedGraphControl RechangesGraphControl;
        private ZedGraphControl TotalTimeGraphControl;
        private SplitContainer splitContainer11;
        private SplitContainer splitContainer12;
        private ZedGraphControl TaraRedutantUsageGraphControl;
        private ZedGraphControl zedGraphControl2;
        private SplitContainer splitContainer13;
        private ZedGraphControl zedGraphControl1;
        private ZedGraphControl zedGraphControl3;
        private ZedGraphControl TarasGraphControl;
        private ZedGraphControl MaterialsGraphControl;
        private ZedGraphControl ProductionGraphControl;
        private SplitContainer splitContainer17;
        private GanttChart ganttANL;
        private VScrollBar vScrollBarANL;
        private Label label44;
        private DateTimePicker dateTimePickerToAnl;
        private Label label45;
        private DateTimePicker dateTimePickerFromAnl;
        private SplitContainer splitContainer18;
        private DataGridView ManualPlanningDataTable;
        private TextBox AnlDescTextBox;
        private Panel panel3;
        private Label label47;
        private ComboBox MaterialsListOnGraphics;
        private Label label46;
        private ComboBox TaraListOnGraphics;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private ContextMenuStrip contextMenuStrip1;
        private Button button1;
        private Label label48;
        private PictureBox pictureBox1;
        private Label label49;
        private TabPage AuthorsTabPage;
        private Label label50;
        private PictureBox pictureBox2;
        private Label label52;
        private Label label53;
        private Label VersionLabel;
        private Button button2;
        private Button button3;
        private Button button4;
        private Label label51;
        private NumericUpDown MaxOldSessionPathsDaysNumericUpDown;
        private MaterialCheckBox UseRCheckBox;
        private MaterialCheckBox UseRStarCheckBox;
        private MaterialCheckBox UseGCheckBox;
        private MaterialCheckBox UseTCheckBox;
        private Label label54;
        private TrackBar GammaSlider;
        private Label label55;
        private Label GammaLabel;
        private MaterialCheckBox RGlobDeltaCheckBox;
        private NumericUpDown TMaxDaysNumericUpDown;
        private Label label57;
        private NumericUpDown TMaxDaysPercentNumericUpDown;
        private Label label56;
        private GroupBox groupBox8;
        private MaterialCheckBox RNewDeadlineCheckBox;
        private NumericUpDown StagesPercentNumericUpDown;
        private Label label58;
    }
}

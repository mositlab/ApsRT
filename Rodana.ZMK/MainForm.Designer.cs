using WinFormsCustomComponents;

namespace Rodana.ZMK
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            OpenPlanningButton = new Button();
            OpenPartiasButton = new Button();
            materialTabSelector1 = new MaterialTabSelector();
            MainTabControl = new MaterialTabControl();
            MainTabPage = new TabPage();
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
            label2 = new Label();
            PartiasProgressLabel = new Label();
            button5 = new Button();
            materialCheckBox3 = new MaterialCheckBox();
            materialCheckBox2 = new MaterialCheckBox();
            customProgressBar3 = new CustomProgressBar();
            HandPartiasTabControl = new TabPage();
            materialTabSelector2 = new MaterialTabSelector();
            PlanningTabPage = new TabPage();
            PlanningTabControl = new MaterialTabControl();
            TyrantTabPage = new TabPage();
            HandPlanningPage = new TabPage();
            GraphsTabPage = new TabPage();
            materialTabSelector3 = new MaterialTabSelector();
            SettingsTabPage = new TabPage();
            SaveSettingsButton = new Button();
            HideConsoleCheckBox = new MaterialCheckBox();
            CacheDurationsCheckBox = new MaterialCheckBox();
            BackupStagesDurationLabel = new Label();
            BackupStagesDurationNumericUpDown = new NumericUpDown();
            PartiasMaxDaysLabel = new Label();
            PartiasMaxDaysNumericUpDown = new NumericUpDown();
            CreateStopMe = new MaterialCheckBox();
            CreateKillMeCheckBox = new MaterialCheckBox();
            HoppersPanel = new Panel();
            AutoLoadCheckBox = new MaterialCheckBox();
            SaveOnExitCheckBox = new MaterialCheckBox();
            ToggleThemeButton = new Button();
            RadioButton1 = new MaterialRadioButton();
            materialRadioButton2 = new MaterialRadioButton();
            MainTabControl.SuspendLayout();
            MainTabPage.SuspendLayout();
            PartiasTabPage.SuspendLayout();
            PartiasTabControl.SuspendLayout();
            AllPartiasTabControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.SuspendLayout();
            PlanningTabPage.SuspendLayout();
            PlanningTabControl.SuspendLayout();
            SettingsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)BackupStagesDurationNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)PartiasMaxDaysNumericUpDown).BeginInit();
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
            PanelNearMainLabel.Size = new Size(1003, 6);
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
            mainlabel.Size = new Size(839, 41);
            mainlabel.TabIndex = 16;
            mainlabel.Text = "Система оптимизации графика производства (APS) «РОДАНА»";
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
            splitContainer1.Panel1.Controls.Add(label2);
            splitContainer1.Panel1.Controls.Add(PartiasProgressLabel);
            splitContainer1.Panel1.Controls.Add(button5);
            splitContainer1.Panel1.Controls.Add(materialCheckBox3);
            splitContainer1.Panel1.Controls.Add(materialCheckBox2);
            splitContainer1.Panel1.Controls.Add(customProgressBar3);
            splitContainer1.Panel1MinSize = 400;
            splitContainer1.Size = new Size(1157, 432);
            splitContainer1.SplitterDistance = 400;
            splitContainer1.TabIndex = 23;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 123);
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
            // button5
            // 
            button5.BackColor = Color.CornflowerBlue;
            button5.BackgroundImageLayout = ImageLayout.Zoom;
            button5.Cursor = Cursors.Hand;
            button5.Enabled = false;
            button5.FlatAppearance.BorderSize = 0;
            button5.FlatStyle = FlatStyle.Flat;
            button5.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            button5.ForeColor = Color.White;
            button5.Location = new Point(3, 3);
            button5.Name = "button5";
            button5.Size = new Size(297, 51);
            button5.TabIndex = 21;
            button5.Text = "Расчёт";
            button5.UseVisualStyleBackColor = false;
            // 
            // materialCheckBox3
            // 
            materialCheckBox3.AutoSize = true;
            materialCheckBox3.BackColor = Color.Black;
            materialCheckBox3.Depth = 0;
            materialCheckBox3.Font = new Font("Microsoft Sans Serif", 10F);
            materialCheckBox3.ForeColor = Color.CornflowerBlue;
            materialCheckBox3.Location = new Point(303, 33);
            materialCheckBox3.Margin = new Padding(0);
            materialCheckBox3.MouseLocation = new Point(-1, -1);
            materialCheckBox3.Name = "materialCheckBox3";
            materialCheckBox3.Ripple = true;
            materialCheckBox3.Size = new Size(64, 30);
            materialCheckBox3.TabIndex = 22;
            materialCheckBox3.Text = "Zadel";
            materialCheckBox3.UseVisualStyleBackColor = false;
            // 
            // materialCheckBox2
            // 
            materialCheckBox2.AutoSize = true;
            materialCheckBox2.BackColor = Color.Black;
            materialCheckBox2.Depth = 0;
            materialCheckBox2.Font = new Font("Microsoft Sans Serif", 10F);
            materialCheckBox2.ForeColor = Color.CornflowerBlue;
            materialCheckBox2.Location = new Point(303, 3);
            materialCheckBox2.Margin = new Padding(0);
            materialCheckBox2.MouseLocation = new Point(-1, -1);
            materialCheckBox2.Name = "materialCheckBox2";
            materialCheckBox2.Ripple = true;
            materialCheckBox2.Size = new Size(55, 30);
            materialCheckBox2.TabIndex = 20;
            materialCheckBox2.Text = "Part";
            materialCheckBox2.UseVisualStyleBackColor = false;
            // 
            // customProgressBar3
            // 
            customProgressBar3.BackColor = SystemColors.ControlLight;
            customProgressBar3.BarPadding = new Padding(0);
            customProgressBar3.Font = new Font("Microsoft Sans Serif", 26F, FontStyle.Bold, GraphicsUnit.Pixel);
            customProgressBar3.ForeColor = Color.CornflowerBlue;
            customProgressBar3.IsInfinite = false;
            customProgressBar3.Location = new Point(3, 60);
            customProgressBar3.Maximum = 100;
            customProgressBar3.Minimum = 0;
            customProgressBar3.Name = "customProgressBar3";
            customProgressBar3.Size = new Size(273, 26);
            customProgressBar3.TabIndex = 16;
            customProgressBar3.Text = "customProgressBar3";
            customProgressBar3.Value = 6;
            // 
            // HandPartiasTabControl
            // 
            HandPartiasTabControl.Location = new Point(4, 24);
            HandPartiasTabControl.Name = "HandPartiasTabControl";
            HandPartiasTabControl.Padding = new Padding(3);
            HandPartiasTabControl.Size = new Size(1163, 438);
            HandPartiasTabControl.TabIndex = 1;
            HandPartiasTabControl.Text = "Поштучное объединение";
            HandPartiasTabControl.UseVisualStyleBackColor = true;
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
            TyrantTabPage.Location = new Point(4, 25);
            TyrantTabPage.Name = "TyrantTabPage";
            TyrantTabPage.Padding = new Padding(3);
            TyrantTabPage.Size = new Size(1163, 438);
            TyrantTabPage.TabIndex = 0;
            TyrantTabPage.Text = "Полное планирование";
            TyrantTabPage.UseVisualStyleBackColor = true;
            // 
            // HandPlanningPage
            // 
            HandPlanningPage.Location = new Point(4, 24);
            HandPlanningPage.Name = "HandPlanningPage";
            HandPlanningPage.Padding = new Padding(3);
            HandPlanningPage.Size = new Size(1163, 438);
            HandPlanningPage.TabIndex = 1;
            HandPlanningPage.Text = "Ручной анализ плана";
            HandPlanningPage.UseVisualStyleBackColor = true;
            // 
            // GraphsTabPage
            // 
            GraphsTabPage.Location = new Point(4, 24);
            GraphsTabPage.Name = "GraphsTabPage";
            GraphsTabPage.Size = new Size(1163, 438);
            GraphsTabPage.TabIndex = 2;
            GraphsTabPage.Text = "Графики эффективности";
            GraphsTabPage.UseVisualStyleBackColor = true;
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
            SettingsTabPage.Controls.Add(SaveSettingsButton);
            SettingsTabPage.Controls.Add(HideConsoleCheckBox);
            SettingsTabPage.Controls.Add(CacheDurationsCheckBox);
            SettingsTabPage.Controls.Add(BackupStagesDurationLabel);
            SettingsTabPage.Controls.Add(BackupStagesDurationNumericUpDown);
            SettingsTabPage.Controls.Add(PartiasMaxDaysLabel);
            SettingsTabPage.Controls.Add(PartiasMaxDaysNumericUpDown);
            SettingsTabPage.Controls.Add(CreateStopMe);
            SettingsTabPage.Controls.Add(CreateKillMeCheckBox);
            SettingsTabPage.Controls.Add(HoppersPanel);
            SettingsTabPage.Controls.Add(AutoLoadCheckBox);
            SettingsTabPage.Controls.Add(SaveOnExitCheckBox);
            SettingsTabPage.Controls.Add(ToggleThemeButton);
            SettingsTabPage.Controls.Add(RadioButton1);
            SettingsTabPage.Controls.Add(materialRadioButton2);
            SettingsTabPage.Location = new Point(4, 25);
            SettingsTabPage.Name = "SettingsTabPage";
            SettingsTabPage.Size = new Size(1171, 501);
            SettingsTabPage.TabIndex = 3;
            SettingsTabPage.Text = "Настройки";
            // 
            // SaveSettingsButton
            // 
            SaveSettingsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SaveSettingsButton.BackColor = Color.CornflowerBlue;
            SaveSettingsButton.BackgroundImageLayout = ImageLayout.Zoom;
            SaveSettingsButton.Cursor = Cursors.Hand;
            SaveSettingsButton.FlatAppearance.BorderSize = 0;
            SaveSettingsButton.FlatStyle = FlatStyle.Flat;
            SaveSettingsButton.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            SaveSettingsButton.ForeColor = Color.White;
            SaveSettingsButton.Location = new Point(751, 3);
            SaveSettingsButton.Name = "SaveSettingsButton";
            SaveSettingsButton.Size = new Size(297, 51);
            SaveSettingsButton.TabIndex = 34;
            SaveSettingsButton.Text = "Сохранить настройки";
            SaveSettingsButton.UseVisualStyleBackColor = false;
            // 
            // HideConsoleCheckBox
            // 
            HideConsoleCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            HideConsoleCheckBox.AutoSize = true;
            HideConsoleCheckBox.BackColor = Color.Black;
            HideConsoleCheckBox.Depth = 0;
            HideConsoleCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            HideConsoleCheckBox.ForeColor = Color.CornflowerBlue;
            HideConsoleCheckBox.Location = new Point(751, 150);
            HideConsoleCheckBox.Margin = new Padding(0);
            HideConsoleCheckBox.MouseLocation = new Point(-1, -1);
            HideConsoleCheckBox.Name = "HideConsoleCheckBox";
            HideConsoleCheckBox.Ripple = true;
            HideConsoleCheckBox.Size = new Size(148, 30);
            HideConsoleCheckBox.TabIndex = 33;
            HideConsoleCheckBox.Text = "Скрывать консоль";
            HideConsoleCheckBox.UseVisualStyleBackColor = false;
            // 
            // CacheDurationsCheckBox
            // 
            CacheDurationsCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CacheDurationsCheckBox.AutoSize = true;
            CacheDurationsCheckBox.BackColor = Color.Black;
            CacheDurationsCheckBox.Depth = 0;
            CacheDurationsCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            CacheDurationsCheckBox.ForeColor = Color.CornflowerBlue;
            CacheDurationsCheckBox.Location = new Point(918, 150);
            CacheDurationsCheckBox.Margin = new Padding(0);
            CacheDurationsCheckBox.MouseLocation = new Point(-1, -1);
            CacheDurationsCheckBox.Name = "CacheDurationsCheckBox";
            CacheDurationsCheckBox.Ripple = true;
            CacheDurationsCheckBox.Size = new Size(201, 30);
            CacheDurationsCheckBox.TabIndex = 32;
            CacheDurationsCheckBox.Text = "Кэшировать длительности";
            CacheDurationsCheckBox.UseVisualStyleBackColor = false;
            // 
            // BackupStagesDurationLabel
            // 
            BackupStagesDurationLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BackupStagesDurationLabel.AutoSize = true;
            BackupStagesDurationLabel.Location = new Point(749, 180);
            BackupStagesDurationLabel.Name = "BackupStagesDurationLabel";
            BackupStagesDurationLabel.Size = new Size(252, 17);
            BackupStagesDurationLabel.TabIndex = 31;
            BackupStagesDurationLabel.Text = "Резервная длительность этапов (с.):";
            // 
            // BackupStagesDurationNumericUpDown
            // 
            BackupStagesDurationNumericUpDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BackupStagesDurationNumericUpDown.BackColor = Color.CornflowerBlue;
            BackupStagesDurationNumericUpDown.BorderStyle = BorderStyle.None;
            BackupStagesDurationNumericUpDown.DecimalPlaces = 2;
            BackupStagesDurationNumericUpDown.ForeColor = Color.White;
            BackupStagesDurationNumericUpDown.Location = new Point(1004, 181);
            BackupStagesDurationNumericUpDown.Margin = new Padding(0);
            BackupStagesDurationNumericUpDown.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            BackupStagesDurationNumericUpDown.Name = "BackupStagesDurationNumericUpDown";
            BackupStagesDurationNumericUpDown.Size = new Size(88, 19);
            BackupStagesDurationNumericUpDown.TabIndex = 30;
            BackupStagesDurationNumericUpDown.TextAlign = HorizontalAlignment.Center;
            BackupStagesDurationNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // PartiasMaxDaysLabel
            // 
            PartiasMaxDaysLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PartiasMaxDaysLabel.AutoSize = true;
            PartiasMaxDaysLabel.Location = new Point(749, 208);
            PartiasMaxDaysLabel.Name = "PartiasMaxDaysLabel";
            PartiasMaxDaysLabel.Size = new Size(227, 17);
            PartiasMaxDaysLabel.TabIndex = 29;
            PartiasMaxDaysLabel.Text = "Максимум дней между партиями:";
            // 
            // PartiasMaxDaysNumericUpDown
            // 
            PartiasMaxDaysNumericUpDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PartiasMaxDaysNumericUpDown.BackColor = Color.CornflowerBlue;
            PartiasMaxDaysNumericUpDown.BorderStyle = BorderStyle.None;
            PartiasMaxDaysNumericUpDown.DecimalPlaces = 2;
            PartiasMaxDaysNumericUpDown.ForeColor = Color.White;
            PartiasMaxDaysNumericUpDown.Location = new Point(1004, 209);
            PartiasMaxDaysNumericUpDown.Margin = new Padding(0);
            PartiasMaxDaysNumericUpDown.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            PartiasMaxDaysNumericUpDown.Name = "PartiasMaxDaysNumericUpDown";
            PartiasMaxDaysNumericUpDown.Size = new Size(88, 19);
            PartiasMaxDaysNumericUpDown.TabIndex = 28;
            PartiasMaxDaysNumericUpDown.TextAlign = HorizontalAlignment.Center;
            PartiasMaxDaysNumericUpDown.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // CreateStopMe
            // 
            CreateStopMe.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CreateStopMe.AutoSize = true;
            CreateStopMe.BackColor = Color.Black;
            CreateStopMe.Depth = 0;
            CreateStopMe.Font = new Font("Microsoft Sans Serif", 10F);
            CreateStopMe.ForeColor = Color.CornflowerBlue;
            CreateStopMe.Location = new Point(918, 120);
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
            CreateKillMeCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            CreateKillMeCheckBox.AutoSize = true;
            CreateKillMeCheckBox.BackColor = Color.Black;
            CreateKillMeCheckBox.Depth = 0;
            CreateKillMeCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            CreateKillMeCheckBox.ForeColor = Color.CornflowerBlue;
            CreateKillMeCheckBox.Location = new Point(751, 120);
            CreateKillMeCheckBox.Margin = new Padding(0);
            CreateKillMeCheckBox.MouseLocation = new Point(-1, -1);
            CreateKillMeCheckBox.Name = "CreateKillMeCheckBox";
            CreateKillMeCheckBox.Ripple = true;
            CreateKillMeCheckBox.Size = new Size(140, 30);
            CreateKillMeCheckBox.TabIndex = 26;
            CreateKillMeCheckBox.Text = "Создавать KillMe";
            CreateKillMeCheckBox.UseVisualStyleBackColor = false;
            // 
            // HoppersPanel
            // 
            HoppersPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            HoppersPanel.AutoScroll = true;
            HoppersPanel.BorderStyle = BorderStyle.FixedSingle;
            HoppersPanel.Location = new Point(5, 3);
            HoppersPanel.Name = "HoppersPanel";
            HoppersPanel.Size = new Size(740, 291);
            HoppersPanel.TabIndex = 25;
            // 
            // AutoLoadCheckBox
            // 
            AutoLoadCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AutoLoadCheckBox.AutoSize = true;
            AutoLoadCheckBox.BackColor = Color.Black;
            AutoLoadCheckBox.Depth = 0;
            AutoLoadCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            AutoLoadCheckBox.ForeColor = Color.CornflowerBlue;
            AutoLoadCheckBox.Location = new Point(751, 90);
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
            SaveOnExitCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SaveOnExitCheckBox.AutoSize = true;
            SaveOnExitCheckBox.BackColor = Color.Black;
            SaveOnExitCheckBox.Depth = 0;
            SaveOnExitCheckBox.Font = new Font("Microsoft Sans Serif", 10F);
            SaveOnExitCheckBox.ForeColor = Color.CornflowerBlue;
            SaveOnExitCheckBox.Location = new Point(751, 60);
            SaveOnExitCheckBox.Margin = new Padding(0);
            SaveOnExitCheckBox.MouseLocation = new Point(-1, -1);
            SaveOnExitCheckBox.Name = "SaveOnExitCheckBox";
            SaveOnExitCheckBox.Ripple = true;
            SaveOnExitCheckBox.Size = new Size(245, 30);
            SaveOnExitCheckBox.TabIndex = 23;
            SaveOnExitCheckBox.Text = "Сохранять настройки при выходе";
            SaveOnExitCheckBox.UseVisualStyleBackColor = false;
            // 
            // ToggleThemeButton
            // 
            ToggleThemeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ToggleThemeButton.BackColor = Color.CornflowerBlue;
            ToggleThemeButton.BackgroundImageLayout = ImageLayout.Zoom;
            ToggleThemeButton.Cursor = Cursors.Hand;
            ToggleThemeButton.FlatAppearance.BorderSize = 0;
            ToggleThemeButton.FlatStyle = FlatStyle.Flat;
            ToggleThemeButton.Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Regular, GraphicsUnit.Point, 204);
            ToggleThemeButton.ForeColor = Color.White;
            ToggleThemeButton.Location = new Point(1054, 3);
            ToggleThemeButton.Name = "ToggleThemeButton";
            ToggleThemeButton.Size = new Size(55, 51);
            ToggleThemeButton.TabIndex = 22;
            ToggleThemeButton.Text = "☾\r\n ";
            ToggleThemeButton.TextAlign = ContentAlignment.TopCenter;
            ToggleThemeButton.UseVisualStyleBackColor = false;
            // 
            // RadioButton1
            // 
            RadioButton1.BackColor = Color.Black;
            RadioButton1.Checked = true;
            RadioButton1.Depth = 0;
            RadioButton1.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            RadioButton1.ForeColor = Color.CornflowerBlue;
            RadioButton1.Location = new Point(413, 397);
            RadioButton1.Margin = new Padding(0);
            RadioButton1.MouseLocation = new Point(-1, -1);
            RadioButton1.Name = "RadioButton1";
            RadioButton1.Ripple = true;
            RadioButton1.Size = new Size(308, 34);
            RadioButton1.TabIndex = 19;
            RadioButton1.TabStop = true;
            RadioButton1.Text = "materialRadioButton1";
            RadioButton1.UseVisualStyleBackColor = false;
            // 
            // materialRadioButton2
            // 
            materialRadioButton2.BackColor = Color.Black;
            materialRadioButton2.Depth = 0;
            materialRadioButton2.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            materialRadioButton2.ForeColor = Color.CornflowerBlue;
            materialRadioButton2.Location = new Point(413, 431);
            materialRadioButton2.Margin = new Padding(0);
            materialRadioButton2.MouseLocation = new Point(-1, -1);
            materialRadioButton2.Name = "materialRadioButton2";
            materialRadioButton2.Ripple = true;
            materialRadioButton2.Size = new Size(308, 34);
            materialRadioButton2.TabIndex = 20;
            materialRadioButton2.Text = "materialRadioButton2";
            materialRadioButton2.UseVisualStyleBackColor = false;
            // 
            // MainForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = SystemColors.Control;
            ClientSize = new Size(1179, 565);
            Controls.Add(MainTabControl);
            Controls.Add(materialTabSelector1);
            Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            MinimumSize = new Size(1195, 604);
            Name = "MainForm";
            Text = "APS «Родана»";
            MainTabControl.ResumeLayout(false);
            MainTabPage.ResumeLayout(false);
            MainTabPage.PerformLayout();
            PartiasTabPage.ResumeLayout(false);
            PartiasTabControl.ResumeLayout(false);
            AllPartiasTabControl.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            PlanningTabPage.ResumeLayout(false);
            PlanningTabControl.ResumeLayout(false);
            SettingsTabPage.ResumeLayout(false);
            SettingsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)BackupStagesDurationNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)PartiasMaxDaysNumericUpDown).EndInit();
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
        private WinFormsCustomComponents.MaterialCheckBox materialCheckBox2;
        private WinFormsCustomComponents.CustomProgressBar customProgressBar3;
        public Button OpenGraphsButton;
        public Button OpenHandPlanButton;
        private Button OpenHandPartiasButton;
        public Button OpenSettingsButton;
        private Panel PanelNearMainLabel;
        private WinFormsCustomComponents.MaterialTabSelector materialTabSelector2;
        private WinFormsCustomComponents.MaterialTabControl PartiasTabControl;
        private TabPage AllPartiasTabControl;
        private TabPage HandPartiasTabControl;
        private WinFormsCustomComponents.MaterialCheckBox materialCheckBox3;
        private Button button5;
        private WinFormsCustomComponents.MaterialRadioButton RadioButton1;
        private WinFormsCustomComponents.MaterialRadioButton materialRadioButton2;
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
        private MaterialCheckBox CacheDurationsCheckBox;
        private MaterialCheckBox HideConsoleCheckBox;
        private Button SaveSettingsButton;
    }
}

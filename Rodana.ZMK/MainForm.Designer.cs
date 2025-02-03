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
            materialTabSelector1 = new CustomComponents.MaterialTabSelector();
            MainTabControl = new CustomComponents.MaterialTabControl();
            MainTabPage = new TabPage();
            OpenSettingsButton = new Button();
            PanelNearMainLabel = new Panel();
            OpenGraphsButton = new Button();
            OpenHandPlanButton = new Button();
            OpenHandPartiasButton = new Button();
            mainlabel = new Label();
            PartiasTabPage = new TabPage();
            PartiasTabControl = new CustomComponents.MaterialTabControl();
            AllPartiasTabControl = new TabPage();
            splitContainer1 = new SplitContainer();
            label2 = new Label();
            PartiasProgressLabel = new Label();
            label1 = new Label();
            numericUpDown1 = new NumericUpDown();
            button5 = new Button();
            materialCheckBox3 = new CustomComponents.MaterialCheckBox();
            materialCheckBox2 = new CustomComponents.MaterialCheckBox();
            customProgressBar3 = new CustomComponents.CustomProgressBar();
            HandPartiasTabControl = new TabPage();
            materialTabSelector2 = new CustomComponents.MaterialTabSelector();
            PlanningTabPage = new TabPage();
            PlanningTabControl = new CustomComponents.MaterialTabControl();
            TyrantTabPage = new TabPage();
            HandPlanningPage = new TabPage();
            GraphsTabPage = new TabPage();
            materialTabSelector3 = new CustomComponents.MaterialTabSelector();
            SettingsTabPage = new TabPage();
            ToggleThemeButton = new Button();
            materialRadioButton1 = new CustomComponents.MaterialRadioButton();
            materialRadioButton2 = new CustomComponents.MaterialRadioButton();
            MainTabControl.SuspendLayout();
            MainTabPage.SuspendLayout();
            PartiasTabPage.SuspendLayout();
            PartiasTabControl.SuspendLayout();
            AllPartiasTabControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            PlanningTabPage.SuspendLayout();
            PlanningTabControl.SuspendLayout();
            SettingsTabPage.SuspendLayout();
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
            OpenPlanningButton.Font = new Font("Montserrat", 10F);
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
            OpenPartiasButton.Font = new Font("Montserrat", 10F);
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
            materialTabSelector1.Font = new Font("Montserrat", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            materialTabSelector1.ForeColor = Color.White;
            materialTabSelector1.Location = new Point(0, 0);
            materialTabSelector1.Name = "materialTabSelector1";
            materialTabSelector1.Size = new Size(1177, 35);
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
            MainTabControl.Size = new Size(1177, 522);
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
            MainTabPage.Location = new Point(4, 33);
            MainTabPage.Name = "MainTabPage";
            MainTabPage.Size = new Size(1169, 485);
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
            OpenSettingsButton.Font = new Font("Montserrat", 10F);
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
            OpenGraphsButton.Font = new Font("Montserrat", 8F);
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
            OpenHandPlanButton.Font = new Font("Montserrat", 8F);
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
            OpenHandPartiasButton.Font = new Font("Montserrat", 8F, FontStyle.Regular, GraphicsUnit.Point, 204);
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
            mainlabel.Size = new Size(1027, 50);
            mainlabel.TabIndex = 16;
            mainlabel.Text = "Система оптимизации графика производства (APS) «РОДАНА»";
            // 
            // PartiasTabPage
            // 
            PartiasTabPage.BackColor = Color.White;
            PartiasTabPage.Controls.Add(PartiasTabControl);
            PartiasTabPage.Controls.Add(materialTabSelector2);
            PartiasTabPage.Location = new Point(4, 33);
            PartiasTabPage.Name = "PartiasTabPage";
            PartiasTabPage.Size = new Size(1169, 485);
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
            PartiasTabControl.Size = new Size(1169, 450);
            PartiasTabControl.TabIndex = 21;
            // 
            // AllPartiasTabControl
            // 
            AllPartiasTabControl.BackColor = Color.White;
            AllPartiasTabControl.Controls.Add(splitContainer1);
            AllPartiasTabControl.Location = new Point(4, 33);
            AllPartiasTabControl.Name = "AllPartiasTabControl";
            AllPartiasTabControl.Padding = new Padding(3);
            AllPartiasTabControl.Size = new Size(1161, 413);
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
            splitContainer1.Panel1.Controls.Add(label1);
            splitContainer1.Panel1.Controls.Add(numericUpDown1);
            splitContainer1.Panel1.Controls.Add(button5);
            splitContainer1.Panel1.Controls.Add(materialCheckBox3);
            splitContainer1.Panel1.Controls.Add(materialCheckBox2);
            splitContainer1.Panel1.Controls.Add(customProgressBar3);
            splitContainer1.Panel1MinSize = 400;
            splitContainer1.Size = new Size(1155, 407);
            splitContainer1.SplitterDistance = 400;
            splitContainer1.TabIndex = 23;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 123);
            label2.Name = "label2";
            label2.Size = new Size(128, 24);
            label2.TabIndex = 26;
            label2.Text = "GUID партии:";
            // 
            // PartiasProgressLabel
            // 
            PartiasProgressLabel.AutoSize = true;
            PartiasProgressLabel.Location = new Point(3, 89);
            PartiasProgressLabel.Name = "PartiasProgressLabel";
            PartiasProgressLabel.Size = new Size(287, 24);
            PartiasProgressLabel.TabIndex = 25;
            PartiasProgressLabel.Text = "Расчёт недоступен без данных.";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(282, 61);
            label1.Name = "label1";
            label1.Size = new Size(113, 24);
            label1.TabIndex = 24;
            label1.Text = "Макс. дней:";
            // 
            // numericUpDown1
            // 
            numericUpDown1.BackColor = Color.CornflowerBlue;
            numericUpDown1.BorderStyle = BorderStyle.None;
            numericUpDown1.ForeColor = Color.White;
            numericUpDown1.Location = new Point(303, 87);
            numericUpDown1.Margin = new Padding(0);
            numericUpDown1.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(88, 24);
            numericUpDown1.TabIndex = 23;
            numericUpDown1.TextAlign = HorizontalAlignment.Center;
            numericUpDown1.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // button5
            // 
            button5.BackColor = Color.CornflowerBlue;
            button5.BackgroundImageLayout = ImageLayout.Zoom;
            button5.Cursor = Cursors.Hand;
            button5.Enabled = false;
            button5.FlatAppearance.BorderSize = 0;
            button5.FlatStyle = FlatStyle.Flat;
            button5.Font = new Font("Montserrat", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
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
            materialCheckBox3.Font = new Font("Montserrat", 10F);
            materialCheckBox3.ForeColor = Color.CornflowerBlue;
            materialCheckBox3.Location = new Point(303, 33);
            materialCheckBox3.Margin = new Padding(0);
            materialCheckBox3.MouseLocation = new Point(-1, -1);
            materialCheckBox3.Name = "materialCheckBox3";
            materialCheckBox3.Ripple = true;
            materialCheckBox3.Size = new Size(79, 30);
            materialCheckBox3.TabIndex = 22;
            materialCheckBox3.Text = "Zadel";
            materialCheckBox3.UseVisualStyleBackColor = false;
            // 
            // materialCheckBox2
            // 
            materialCheckBox2.AutoSize = true;
            materialCheckBox2.BackColor = Color.Black;
            materialCheckBox2.Depth = 0;
            materialCheckBox2.Font = new Font("Montserrat", 10F);
            materialCheckBox2.ForeColor = Color.CornflowerBlue;
            materialCheckBox2.Location = new Point(303, 3);
            materialCheckBox2.Margin = new Padding(0);
            materialCheckBox2.MouseLocation = new Point(-1, -1);
            materialCheckBox2.Name = "materialCheckBox2";
            materialCheckBox2.Ripple = true;
            materialCheckBox2.Size = new Size(67, 30);
            materialCheckBox2.TabIndex = 20;
            materialCheckBox2.Text = "Part";
            materialCheckBox2.UseVisualStyleBackColor = false;
            // 
            // customProgressBar3
            // 
            customProgressBar3.BackColor = SystemColors.ControlLight;
            customProgressBar3.BarPadding = new Padding(0);
            customProgressBar3.Font = new Font("Montserrat", 26F, FontStyle.Bold, GraphicsUnit.Pixel);
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
            HandPartiasTabControl.Location = new Point(4, 29);
            HandPartiasTabControl.Name = "HandPartiasTabControl";
            HandPartiasTabControl.Padding = new Padding(3);
            HandPartiasTabControl.Size = new Size(1161, 417);
            HandPartiasTabControl.TabIndex = 1;
            HandPartiasTabControl.Text = "Поштучное объединение";
            HandPartiasTabControl.UseVisualStyleBackColor = true;
            // 
            // materialTabSelector2
            // 
            materialTabSelector2.BackColor = Color.CornflowerBlue;
            materialTabSelector2.BaseTabControl = PartiasTabControl;
            materialTabSelector2.Dock = DockStyle.Top;
            materialTabSelector2.Font = new Font("Montserrat", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            materialTabSelector2.ForeColor = Color.White;
            materialTabSelector2.Location = new Point(0, 0);
            materialTabSelector2.Margin = new Padding(0);
            materialTabSelector2.Name = "materialTabSelector2";
            materialTabSelector2.Size = new Size(1169, 35);
            materialTabSelector2.TabIndex = 22;
            materialTabSelector2.Text = "materialTabSelector2";
            // 
            // PlanningTabPage
            // 
            PlanningTabPage.Controls.Add(PlanningTabControl);
            PlanningTabPage.Controls.Add(materialTabSelector3);
            PlanningTabPage.Location = new Point(4, 29);
            PlanningTabPage.Name = "PlanningTabPage";
            PlanningTabPage.Size = new Size(1169, 489);
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
            PlanningTabControl.Size = new Size(1169, 454);
            PlanningTabControl.TabIndex = 0;
            // 
            // TyrantTabPage
            // 
            TyrantTabPage.Location = new Point(4, 33);
            TyrantTabPage.Name = "TyrantTabPage";
            TyrantTabPage.Padding = new Padding(3);
            TyrantTabPage.Size = new Size(1161, 417);
            TyrantTabPage.TabIndex = 0;
            TyrantTabPage.Text = "Полное планирование";
            TyrantTabPage.UseVisualStyleBackColor = true;
            // 
            // HandPlanningPage
            // 
            HandPlanningPage.Location = new Point(4, 29);
            HandPlanningPage.Name = "HandPlanningPage";
            HandPlanningPage.Padding = new Padding(3);
            HandPlanningPage.Size = new Size(1161, 421);
            HandPlanningPage.TabIndex = 1;
            HandPlanningPage.Text = "Ручной анализ плана";
            HandPlanningPage.UseVisualStyleBackColor = true;
            // 
            // GraphsTabPage
            // 
            GraphsTabPage.Location = new Point(4, 29);
            GraphsTabPage.Name = "GraphsTabPage";
            GraphsTabPage.Size = new Size(1161, 421);
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
            materialTabSelector3.Size = new Size(1169, 35);
            materialTabSelector3.TabIndex = 1;
            materialTabSelector3.Text = "materialTabSelector3";
            // 
            // SettingsTabPage
            // 
            SettingsTabPage.BackColor = Color.White;
            SettingsTabPage.Controls.Add(ToggleThemeButton);
            SettingsTabPage.Controls.Add(materialRadioButton1);
            SettingsTabPage.Controls.Add(materialRadioButton2);
            SettingsTabPage.Location = new Point(4, 29);
            SettingsTabPage.Name = "SettingsTabPage";
            SettingsTabPage.Size = new Size(1169, 489);
            SettingsTabPage.TabIndex = 3;
            SettingsTabPage.Text = "Настройки";
            // 
            // ToggleThemeButton
            // 
            ToggleThemeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ToggleThemeButton.BackColor = Color.CornflowerBlue;
            ToggleThemeButton.BackgroundImageLayout = ImageLayout.Zoom;
            ToggleThemeButton.Cursor = Cursors.Hand;
            ToggleThemeButton.FlatAppearance.BorderSize = 0;
            ToggleThemeButton.FlatStyle = FlatStyle.Flat;
            ToggleThemeButton.Font = new Font("Montserrat", 24F, FontStyle.Regular, GraphicsUnit.Point, 204);
            ToggleThemeButton.ForeColor = Color.White;
            ToggleThemeButton.Location = new Point(1101, 9);
            ToggleThemeButton.Name = "ToggleThemeButton";
            ToggleThemeButton.Size = new Size(60, 60);
            ToggleThemeButton.TabIndex = 22;
            ToggleThemeButton.Text = "☾\r\n ";
            ToggleThemeButton.TextAlign = ContentAlignment.TopCenter;
            ToggleThemeButton.UseVisualStyleBackColor = false;
            // 
            // materialRadioButton1
            // 
            materialRadioButton1.BackColor = Color.Black;
            materialRadioButton1.Checked = true;
            materialRadioButton1.Depth = 0;
            materialRadioButton1.Font = new Font("Montserrat", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            materialRadioButton1.ForeColor = Color.CornflowerBlue;
            materialRadioButton1.Location = new Point(430, 210);
            materialRadioButton1.Margin = new Padding(0);
            materialRadioButton1.MouseLocation = new Point(-1, -1);
            materialRadioButton1.Name = "materialRadioButton1";
            materialRadioButton1.Ripple = true;
            materialRadioButton1.Size = new Size(308, 34);
            materialRadioButton1.TabIndex = 19;
            materialRadioButton1.TabStop = true;
            materialRadioButton1.Text = "materialRadioButton1";
            materialRadioButton1.UseVisualStyleBackColor = false;
            // 
            // materialRadioButton2
            // 
            materialRadioButton2.BackColor = Color.Black;
            materialRadioButton2.Depth = 0;
            materialRadioButton2.Font = new Font("Montserrat", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
            materialRadioButton2.ForeColor = Color.CornflowerBlue;
            materialRadioButton2.Location = new Point(430, 244);
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
            ClientSize = new Size(1177, 557);
            Controls.Add(MainTabControl);
            Controls.Add(materialTabSelector1);
            Font = new Font("Montserrat", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
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
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            PlanningTabPage.ResumeLayout(false);
            PlanningTabControl.ResumeLayout(false);
            SettingsTabPage.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Button OpenPartiasButton;
        public Button OpenPlanningButton;
        private CustomComponents.MaterialTabSelector materialTabSelector1;
        private CustomComponents.MaterialTabControl MainTabControl;
        private TabPage PartiasTabPage;
        private TabPage PlanningTabPage;
        private TabPage MainTabPage;
        private TabPage SettingsTabPage;
        private Label mainlabel;
        private CustomComponents.MaterialCheckBox materialCheckBox2;
        private CustomComponents.CustomProgressBar customProgressBar3;
        public Button OpenGraphsButton;
        public Button OpenHandPlanButton;
        private Button OpenHandPartiasButton;
        public Button OpenSettingsButton;
        private Panel PanelNearMainLabel;
        private CustomComponents.MaterialTabSelector materialTabSelector2;
        private CustomComponents.MaterialTabControl PartiasTabControl;
        private TabPage AllPartiasTabControl;
        private TabPage HandPartiasTabControl;
        private CustomComponents.MaterialCheckBox materialCheckBox3;
        private Button button5;
        private CustomComponents.MaterialRadioButton materialRadioButton1;
        private CustomComponents.MaterialRadioButton materialRadioButton2;
        private Button ToggleThemeButton;
        private SplitContainer splitContainer1;
        private NumericUpDown numericUpDown1;
        private Label PartiasProgressLabel;
        private Label label1;
        private Label label2;
        private CustomComponents.MaterialTabControl PlanningTabControl;
        private TabPage TyrantTabPage;
        private TabPage HandPlanningPage;
        private TabPage GraphsTabPage;
        private CustomComponents.MaterialTabSelector materialTabSelector3;
    }
}

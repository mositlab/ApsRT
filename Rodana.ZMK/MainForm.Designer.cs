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
            materialTabControl1 = new CustomComponents.MaterialTabControl();
            tabPage10 = new TabPage();
            button4 = new Button();
            panel1 = new Panel();
            button3 = new Button();
            button2 = new Button();
            button1 = new Button();
            mainlabel = new Label();
            tabPage8 = new TabPage();
            materialTabControl2 = new CustomComponents.MaterialTabControl();
            tabPage1 = new TabPage();
            materialCheckBox3 = new CustomComponents.MaterialCheckBox();
            button5 = new Button();
            customProgressBar3 = new CustomComponents.CustomProgressBar();
            materialCheckBox2 = new CustomComponents.MaterialCheckBox();
            tabPage2 = new TabPage();
            materialTabSelector2 = new CustomComponents.MaterialTabSelector();
            tabPage9 = new TabPage();
            tabPage11 = new TabPage();
            ToggleThemeButton = new Button();
            materialRadioButton1 = new CustomComponents.MaterialRadioButton();
            materialRadioButton2 = new CustomComponents.MaterialRadioButton();
            materialTabControl1.SuspendLayout();
            tabPage10.SuspendLayout();
            tabPage8.SuspendLayout();
            materialTabControl2.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage11.SuspendLayout();
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
            OpenPlanningButton.Font = new Font("Montserrat", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
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
            OpenPartiasButton.Font = new Font("Montserrat", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
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
            materialTabSelector1.BaseTabControl = materialTabControl1;
            materialTabSelector1.Dock = DockStyle.Top;
            materialTabSelector1.Font = new Font("Montserrat", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            materialTabSelector1.ForeColor = Color.White;
            materialTabSelector1.Location = new Point(0, 0);
            materialTabSelector1.Name = "materialTabSelector1";
            materialTabSelector1.Size = new Size(1177, 35);
            materialTabSelector1.TabIndex = 16;
            materialTabSelector1.Text = "materialTabSelector1";
            // 
            // materialTabControl1
            // 
            materialTabControl1.Controls.Add(tabPage10);
            materialTabControl1.Controls.Add(tabPage8);
            materialTabControl1.Controls.Add(tabPage9);
            materialTabControl1.Controls.Add(tabPage11);
            materialTabControl1.Dock = DockStyle.Fill;
            materialTabControl1.Location = new Point(0, 35);
            materialTabControl1.Name = "materialTabControl1";
            materialTabControl1.SelectedIndex = 0;
            materialTabControl1.Size = new Size(1177, 522);
            materialTabControl1.TabIndex = 17;
            // 
            // tabPage10
            // 
            tabPage10.BackColor = Color.White;
            tabPage10.Controls.Add(button4);
            tabPage10.Controls.Add(panel1);
            tabPage10.Controls.Add(button3);
            tabPage10.Controls.Add(button2);
            tabPage10.Controls.Add(button1);
            tabPage10.Controls.Add(mainlabel);
            tabPage10.Controls.Add(OpenPlanningButton);
            tabPage10.Controls.Add(OpenPartiasButton);
            tabPage10.Location = new Point(4, 29);
            tabPage10.Name = "tabPage10";
            tabPage10.Size = new Size(1169, 489);
            tabPage10.TabIndex = 2;
            tabPage10.Text = "Главная";
            // 
            // button4
            // 
            button4.BackColor = Color.CornflowerBlue;
            button4.BackgroundImage = (Image)resources.GetObject("button4.BackgroundImage");
            button4.BackgroundImageLayout = ImageLayout.Zoom;
            button4.Cursor = Cursors.Hand;
            button4.FlatAppearance.BorderSize = 0;
            button4.FlatStyle = FlatStyle.Flat;
            button4.Font = new Font("Montserrat", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            button4.ForeColor = Color.White;
            button4.Location = new Point(929, 131);
            button4.Name = "button4";
            button4.Size = new Size(158, 158);
            button4.TabIndex = 21;
            button4.Text = "Настройки";
            button4.TextAlign = ContentAlignment.BottomLeft;
            button4.UseVisualStyleBackColor = false;
            // 
            // panel1
            // 
            panel1.BackColor = Color.Black;
            panel1.Location = new Point(32, 68);
            panel1.Name = "panel1";
            panel1.Size = new Size(1003, 6);
            panel1.TabIndex = 20;
            // 
            // button3
            // 
            button3.BackColor = Color.CornflowerBlue;
            button3.BackgroundImage = (Image)resources.GetObject("button3.BackgroundImage");
            button3.BackgroundImageLayout = ImageLayout.Zoom;
            button3.Cursor = Cursors.Hand;
            button3.FlatAppearance.BorderSize = 0;
            button3.FlatStyle = FlatStyle.Flat;
            button3.Font = new Font("Montserrat", 8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            button3.ForeColor = Color.White;
            button3.Location = new Point(662, 131);
            button3.Name = "button3";
            button3.Size = new Size(158, 158);
            button3.TabIndex = 19;
            button3.Text = "Графики эффективности";
            button3.TextAlign = ContentAlignment.BottomLeft;
            button3.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            button2.BackColor = Color.CornflowerBlue;
            button2.BackgroundImage = (Image)resources.GetObject("button2.BackgroundImage");
            button2.BackgroundImageLayout = ImageLayout.Zoom;
            button2.Cursor = Cursors.Hand;
            button2.FlatAppearance.BorderSize = 0;
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("Montserrat", 8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            button2.ForeColor = Color.White;
            button2.Location = new Point(498, 295);
            button2.Name = "button2";
            button2.Size = new Size(158, 158);
            button2.TabIndex = 18;
            button2.Text = "Ручной анализ плана";
            button2.TextAlign = ContentAlignment.BottomLeft;
            button2.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            button1.BackColor = Color.CornflowerBlue;
            button1.BackgroundImage = (Image)resources.GetObject("button1.BackgroundImage");
            button1.BackgroundImageLayout = ImageLayout.Zoom;
            button1.Cursor = Cursors.Hand;
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Montserrat", 8F, FontStyle.Regular, GraphicsUnit.Point, 204);
            button1.ForeColor = Color.White;
            button1.Location = new Point(498, 131);
            button1.Name = "button1";
            button1.Size = new Size(158, 158);
            button1.TabIndex = 17;
            button1.Text = "Поштуч. объединение";
            button1.TextAlign = ContentAlignment.BottomLeft;
            button1.UseVisualStyleBackColor = false;
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
            // tabPage8
            // 
            tabPage8.BackColor = Color.White;
            tabPage8.Controls.Add(materialTabControl2);
            tabPage8.Controls.Add(materialTabSelector2);
            tabPage8.Location = new Point(4, 29);
            tabPage8.Name = "tabPage8";
            tabPage8.Size = new Size(1169, 489);
            tabPage8.TabIndex = 0;
            tabPage8.Text = "Формирование партий";
            // 
            // materialTabControl2
            // 
            materialTabControl2.Controls.Add(tabPage1);
            materialTabControl2.Controls.Add(tabPage2);
            materialTabControl2.Dock = DockStyle.Fill;
            materialTabControl2.Location = new Point(0, 35);
            materialTabControl2.Name = "materialTabControl2";
            materialTabControl2.SelectedIndex = 0;
            materialTabControl2.Size = new Size(1169, 454);
            materialTabControl2.TabIndex = 21;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = Color.White;
            tabPage1.Controls.Add(materialCheckBox3);
            tabPage1.Controls.Add(button5);
            tabPage1.Controls.Add(customProgressBar3);
            tabPage1.Controls.Add(materialCheckBox2);
            tabPage1.Location = new Point(4, 29);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1161, 421);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Полный расчёт";
            // 
            // materialCheckBox3
            // 
            materialCheckBox3.AutoSize = true;
            materialCheckBox3.BackColor = Color.Black;
            materialCheckBox3.Checked = true;
            materialCheckBox3.CheckState = CheckState.Checked;
            materialCheckBox3.Depth = 0;
            materialCheckBox3.Font = new Font("Montserrat", 10F);
            materialCheckBox3.ForeColor = Color.CornflowerBlue;
            materialCheckBox3.Location = new Point(306, 36);
            materialCheckBox3.Margin = new Padding(0);
            materialCheckBox3.MouseLocation = new Point(-1, -1);
            materialCheckBox3.Name = "materialCheckBox3";
            materialCheckBox3.Ripple = true;
            materialCheckBox3.Size = new Size(79, 30);
            materialCheckBox3.TabIndex = 22;
            materialCheckBox3.Text = "Zadel";
            materialCheckBox3.UseVisualStyleBackColor = false;
            // 
            // button5
            // 
            button5.BackColor = Color.CornflowerBlue;
            button5.BackgroundImageLayout = ImageLayout.Zoom;
            button5.Cursor = Cursors.Hand;
            button5.FlatAppearance.BorderSize = 0;
            button5.FlatStyle = FlatStyle.Flat;
            button5.Font = new Font("Montserrat", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
            button5.ForeColor = Color.White;
            button5.Location = new Point(6, 6);
            button5.Name = "button5";
            button5.Size = new Size(297, 51);
            button5.TabIndex = 21;
            button5.Text = "Расчёт";
            button5.UseVisualStyleBackColor = false;
            // 
            // customProgressBar3
            // 
            customProgressBar3.BackColor = SystemColors.ControlLight;
            customProgressBar3.BarPadding = new Padding(0);
            customProgressBar3.Font = new Font("Montserrat", 26F, FontStyle.Bold, GraphicsUnit.Pixel);
            customProgressBar3.ForeColor = Color.CornflowerBlue;
            customProgressBar3.IsInfinite = false;
            customProgressBar3.Location = new Point(6, 63);
            customProgressBar3.Maximum = 100;
            customProgressBar3.Minimum = 0;
            customProgressBar3.Name = "customProgressBar3";
            customProgressBar3.Size = new Size(297, 26);
            customProgressBar3.TabIndex = 16;
            customProgressBar3.Text = "customProgressBar3";
            customProgressBar3.Value = 6;
            // 
            // materialCheckBox2
            // 
            materialCheckBox2.AutoSize = true;
            materialCheckBox2.BackColor = Color.Black;
            materialCheckBox2.Checked = true;
            materialCheckBox2.CheckState = CheckState.Checked;
            materialCheckBox2.Depth = 0;
            materialCheckBox2.Font = new Font("Montserrat", 10F);
            materialCheckBox2.ForeColor = Color.CornflowerBlue;
            materialCheckBox2.Location = new Point(306, 6);
            materialCheckBox2.Margin = new Padding(0);
            materialCheckBox2.MouseLocation = new Point(-1, -1);
            materialCheckBox2.Name = "materialCheckBox2";
            materialCheckBox2.Ripple = true;
            materialCheckBox2.Size = new Size(67, 30);
            materialCheckBox2.TabIndex = 20;
            materialCheckBox2.Text = "Part";
            materialCheckBox2.UseVisualStyleBackColor = false;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 29);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1161, 421);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Поштучное объединение";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // materialTabSelector2
            // 
            materialTabSelector2.BackColor = Color.CornflowerBlue;
            materialTabSelector2.BaseTabControl = materialTabControl2;
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
            // tabPage9
            // 
            tabPage9.Location = new Point(4, 29);
            tabPage9.Name = "tabPage9";
            tabPage9.Size = new Size(1169, 489);
            tabPage9.TabIndex = 1;
            tabPage9.Text = "Планирование производства";
            tabPage9.UseVisualStyleBackColor = true;
            // 
            // tabPage11
            // 
            tabPage11.BackColor = Color.White;
            tabPage11.Controls.Add(ToggleThemeButton);
            tabPage11.Controls.Add(materialRadioButton1);
            tabPage11.Controls.Add(materialRadioButton2);
            tabPage11.Location = new Point(4, 29);
            tabPage11.Name = "tabPage11";
            tabPage11.Size = new Size(1169, 489);
            tabPage11.TabIndex = 3;
            tabPage11.Text = "Настройки";
            // 
            // ToggleThemeButton
            // 
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
            Controls.Add(materialTabControl1);
            Controls.Add(materialTabSelector1);
            MinimumSize = new Size(1195, 604);
            Name = "MainForm";
            Text = "APS «Родана»";
            materialTabControl1.ResumeLayout(false);
            tabPage10.ResumeLayout(false);
            tabPage10.PerformLayout();
            tabPage8.ResumeLayout(false);
            materialTabControl2.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage11.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Button OpenPartiasButton;
        public Button OpenPlanningButton;
        private CustomComponents.MaterialTabSelector materialTabSelector1;
        private CustomComponents.MaterialTabControl materialTabControl1;
        private TabPage tabPage8;
        private TabPage tabPage9;
        private TabPage tabPage10;
        private TabPage tabPage11;
        private Label mainlabel;
        private CustomComponents.MaterialCheckBox materialCheckBox2;
        private CustomComponents.CustomProgressBar customProgressBar3;
        public Button button3;
        public Button button2;
        private Button button1;
        public Button button4;
        private Panel panel1;
        private CustomComponents.MaterialTabSelector materialTabSelector2;
        private CustomComponents.MaterialTabControl materialTabControl2;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private CustomComponents.MaterialCheckBox materialCheckBox3;
        private Button button5;
        private CustomComponents.MaterialRadioButton materialRadioButton1;
        private CustomComponents.MaterialRadioButton materialRadioButton2;
        private Button ToggleThemeButton;
    }
}

namespace ApsRTCustomComponents
{
    partial class SqlDataSourceConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Label label1;
            Label label2;
            Label label3;
            Label label4;
            Label label5;
            Label label6;
            Label label7;
            Label label8;
            ButtonSubmit = new Button();
            ButtonCheck = new Button();
            TextBoxEndpoint = new TextBox();
            TextBoxLogin = new TextBox();
            TextBoxPassword = new TextBox();
            TextBoxDatabase = new TextBox();
            TextBoxFile = new TextBox();
            NumericUpDownTimeout = new NumericUpDown();
            ButtonBrowse = new Button();
            TextBoxSql = new TextBox();
            saveFileDialog1 = new SaveFileDialog();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            ((System.ComponentModel.ISupportInitialize)NumericUpDownTimeout).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 12);
            label1.Name = "label1";
            label1.Size = new Size(54, 20);
            label1.TabIndex = 2;
            label1.Text = "Адрес:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(304, 12);
            label2.Name = "label2";
            label2.Size = new Size(55, 20);
            label2.TabIndex = 2;
            label2.Text = "Логин:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(304, 51);
            label3.Name = "label3";
            label3.Size = new Size(65, 20);
            label3.TabIndex = 2;
            label3.Text = "Пароль:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(13, 51);
            label4.Name = "label4";
            label4.Size = new Size(31, 20);
            label4.TabIndex = 2;
            label4.Text = "БД:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(13, 89);
            label5.Name = "label5";
            label5.Size = new Size(48, 20);
            label5.TabIndex = 2;
            label5.Text = "Файл:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(606, 12);
            label6.Name = "label6";
            label6.Size = new Size(69, 20);
            label6.TabIndex = 2;
            label6.Text = "Таймаут:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(13, 120);
            label7.Name = "label7";
            label7.Size = new Size(138, 20);
            label7.TabIndex = 2;
            label7.Text = "Тело SQL-запроса:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(747, 12);
            label8.Name = "label8";
            label8.Size = new Size(55, 20);
            label8.TabIndex = 2;
            label8.Text = "секунд";
            // 
            // ButtonSubmit
            // 
            ButtonSubmit.BackColor = SystemColors.ActiveCaption;
            ButtonSubmit.FlatAppearance.BorderSize = 0;
            ButtonSubmit.FlatStyle = FlatStyle.Flat;
            ButtonSubmit.Font = new Font("Microsoft Sans Serif", 8F);
            ButtonSubmit.ForeColor = Color.White;
            ButtonSubmit.Location = new Point(706, 84);
            ButtonSubmit.Margin = new Padding(3, 4, 3, 4);
            ButtonSubmit.Name = "ButtonSubmit";
            ButtonSubmit.Size = new Size(98, 31);
            ButtonSubmit.TabIndex = 9;
            ButtonSubmit.Text = "Сохранить";
            ButtonSubmit.UseVisualStyleBackColor = false;
            ButtonSubmit.Click += OnSubmit;
            // 
            // ButtonCheck
            // 
            ButtonCheck.BackColor = SystemColors.ActiveCaption;
            ButtonCheck.FlatAppearance.BorderSize = 0;
            ButtonCheck.FlatStyle = FlatStyle.Flat;
            ButtonCheck.Font = new Font("Microsoft Sans Serif", 8F);
            ButtonCheck.ForeColor = Color.White;
            ButtonCheck.Location = new Point(606, 45);
            ButtonCheck.Margin = new Padding(3, 4, 3, 4);
            ButtonCheck.Name = "ButtonCheck";
            ButtonCheck.Size = new Size(197, 31);
            ButtonCheck.TabIndex = 8;
            ButtonCheck.Text = "Проверить подключение";
            ButtonCheck.UseVisualStyleBackColor = false;
            ButtonCheck.Click += OnCheck;
            // 
            // TextBoxEndpoint
            // 
            TextBoxEndpoint.Location = new Point(69, 8);
            TextBoxEndpoint.Margin = new Padding(3, 4, 3, 4);
            TextBoxEndpoint.Name = "TextBoxEndpoint";
            TextBoxEndpoint.PlaceholderText = "sqlserv.mkm.lan,1433";
            TextBoxEndpoint.Size = new Size(228, 27);
            TextBoxEndpoint.TabIndex = 1;
            TextBoxEndpoint.TextChanged += OnTextChanged;
            // 
            // TextBoxLogin
            // 
            TextBoxLogin.Location = new Point(370, 8);
            TextBoxLogin.Margin = new Padding(3, 4, 3, 4);
            TextBoxLogin.Name = "TextBoxLogin";
            TextBoxLogin.PlaceholderText = "login";
            TextBoxLogin.Size = new Size(228, 27);
            TextBoxLogin.TabIndex = 3;
            TextBoxLogin.TextChanged += OnTextChanged;
            // 
            // TextBoxPassword
            // 
            TextBoxPassword.Location = new Point(370, 47);
            TextBoxPassword.Margin = new Padding(3, 4, 3, 4);
            TextBoxPassword.Name = "TextBoxPassword";
            TextBoxPassword.PlaceholderText = "password";
            TextBoxPassword.Size = new Size(228, 27);
            TextBoxPassword.TabIndex = 4;
            TextBoxPassword.TextChanged += OnTextChanged;
            // 
            // TextBoxDatabase
            // 
            TextBoxDatabase.Location = new Point(69, 47);
            TextBoxDatabase.Margin = new Padding(3, 4, 3, 4);
            TextBoxDatabase.Name = "TextBoxDatabase";
            TextBoxDatabase.PlaceholderText = "DB_NAME";
            TextBoxDatabase.Size = new Size(228, 27);
            TextBoxDatabase.TabIndex = 2;
            TextBoxDatabase.TextChanged += OnTextChanged;
            // 
            // TextBoxFile
            // 
            TextBoxFile.Location = new Point(69, 85);
            TextBoxFile.Margin = new Padding(3, 4, 3, 4);
            TextBoxFile.Name = "TextBoxFile";
            TextBoxFile.PlaceholderText = "C:\\Users\\User\\Desktop\\infimum-data-source.json";
            TextBoxFile.Size = new Size(529, 27);
            TextBoxFile.TabIndex = 6;
            TextBoxFile.TextChanged += OnTextChanged;
            // 
            // NumericUpDownTimeout
            // 
            NumericUpDownTimeout.BorderStyle = BorderStyle.None;
            NumericUpDownTimeout.ForeColor = Color.White;
            NumericUpDownTimeout.Location = new Point(677, 8);
            NumericUpDownTimeout.Margin = new Padding(3, 4, 3, 4);
            NumericUpDownTimeout.Maximum = new decimal(new int[] { 7200, 0, 0, 0 });
            NumericUpDownTimeout.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            NumericUpDownTimeout.Name = "NumericUpDownTimeout";
            NumericUpDownTimeout.Size = new Size(64, 23);
            NumericUpDownTimeout.TabIndex = 5;
            NumericUpDownTimeout.TextAlign = HorizontalAlignment.Center;
            NumericUpDownTimeout.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // ButtonBrowse
            // 
            ButtonBrowse.BackColor = SystemColors.ActiveCaption;
            ButtonBrowse.FlatAppearance.BorderSize = 0;
            ButtonBrowse.FlatStyle = FlatStyle.Flat;
            ButtonBrowse.Font = new Font("Microsoft Sans Serif", 8F);
            ButtonBrowse.ForeColor = Color.White;
            ButtonBrowse.Location = new Point(609, 84);
            ButtonBrowse.Margin = new Padding(3, 4, 3, 4);
            ButtonBrowse.Name = "ButtonBrowse";
            ButtonBrowse.Size = new Size(91, 31);
            ButtonBrowse.TabIndex = 7;
            ButtonBrowse.Text = "Обзор...";
            ButtonBrowse.UseVisualStyleBackColor = false;
            ButtonBrowse.Click += OnBrowse;
            // 
            // TextBoxSql
            // 
            TextBoxSql.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TextBoxSql.BackColor = Color.FromArgb(30, 30, 30);
            TextBoxSql.BorderStyle = BorderStyle.FixedSingle;
            TextBoxSql.Font = new Font("Consolas", 9F);
            TextBoxSql.ForeColor = Color.Gainsboro;
            TextBoxSql.Location = new Point(0, 144);
            TextBoxSql.Margin = new Padding(3, 4, 3, 4);
            TextBoxSql.Multiline = true;
            TextBoxSql.Name = "TextBoxSql";
            TextBoxSql.PlaceholderText = "SELECT\r\n    t.[ColumnName] as [Имя_Столбца_Для_АПС]\r\nFROM\r\n    [dbo].[TableName] t\r\nORDER BY\r\n    1 ASC";
            TextBoxSql.ScrollBars = ScrollBars.Both;
            TextBoxSql.Size = new Size(816, 470);
            TextBoxSql.TabIndex = 10;
            TextBoxSql.TabStop = false;
            TextBoxSql.WordWrap = false;
            TextBoxSql.TextChanged += OnTextChanged;
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.Filter = "JSON-файлы|*.json";
            // 
            // SqlDataSourceConfigForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(816, 615);
            Controls.Add(NumericUpDownTimeout);
            Controls.Add(TextBoxPassword);
            Controls.Add(label3);
            Controls.Add(TextBoxLogin);
            Controls.Add(label8);
            Controls.Add(label6);
            Controls.Add(label2);
            Controls.Add(TextBoxSql);
            Controls.Add(TextBoxFile);
            Controls.Add(label7);
            Controls.Add(label5);
            Controls.Add(TextBoxDatabase);
            Controls.Add(label4);
            Controls.Add(TextBoxEndpoint);
            Controls.Add(label1);
            Controls.Add(ButtonCheck);
            Controls.Add(ButtonBrowse);
            Controls.Add(ButtonSubmit);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(832, 651);
            Name = "SqlDataSourceConfigForm";
            ShowIcon = false;
            Text = "Редактирование источника данных SQL";
            ((System.ComponentModel.ISupportInitialize)NumericUpDownTimeout).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button ButtonSubmit;
        private Button ButtonCheck;
        private Label label1;
        private TextBox TextBoxEndpoint;
        private TextBox TextBoxLogin;
        private TextBox TextBoxPassword;
        private TextBox TextBoxDatabase;
        private TextBox TextBoxFile;
        private NumericUpDown NumericUpDownTimeout;
        private Button ButtonBrowse;
        private TextBox TextBoxSql;
        private SaveFileDialog saveFileDialog1;
    }
}

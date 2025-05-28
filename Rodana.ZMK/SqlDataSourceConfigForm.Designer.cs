namespace Rodana.ZMK
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
            label1.Location = new Point(11, 9);
            label1.Name = "label1";
            label1.Size = new Size(43, 15);
            label1.TabIndex = 2;
            label1.Text = "Адрес:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(266, 9);
            label2.Name = "label2";
            label2.Size = new Size(44, 15);
            label2.TabIndex = 2;
            label2.Text = "Логин:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(266, 38);
            label3.Name = "label3";
            label3.Size = new Size(52, 15);
            label3.TabIndex = 2;
            label3.Text = "Пароль:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(11, 38);
            label4.Name = "label4";
            label4.Size = new Size(25, 15);
            label4.TabIndex = 2;
            label4.Text = "БД:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(11, 67);
            label5.Name = "label5";
            label5.Size = new Size(39, 15);
            label5.TabIndex = 2;
            label5.Text = "Файл:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(530, 9);
            label6.Name = "label6";
            label6.Size = new Size(55, 15);
            label6.TabIndex = 2;
            label6.Text = "Таймаут:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(11, 90);
            label7.Name = "label7";
            label7.Size = new Size(109, 15);
            label7.TabIndex = 2;
            label7.Text = "Тело SQL-запроса:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(654, 9);
            label8.Name = "label8";
            label8.Size = new Size(44, 15);
            label8.TabIndex = 2;
            label8.Text = "секунд";
            // 
            // ButtonSubmit
            // 
            ButtonSubmit.Location = new Point(616, 64);
            ButtonSubmit.Name = "ButtonSubmit";
            ButtonSubmit.Size = new Size(86, 23);
            ButtonSubmit.TabIndex = 9;
            ButtonSubmit.Text = "Сохранить";
            ButtonSubmit.UseVisualStyleBackColor = true;
            ButtonSubmit.Click += OnSubmit;
            // 
            // ButtonCheck
            // 
            ButtonCheck.Location = new Point(530, 35);
            ButtonCheck.Name = "ButtonCheck";
            ButtonCheck.Size = new Size(172, 23);
            ButtonCheck.TabIndex = 8;
            ButtonCheck.Text = "Проверить подключение";
            ButtonCheck.UseVisualStyleBackColor = true;
            ButtonCheck.Click += OnCheck;
            // 
            // TextBoxEndpoint
            // 
            TextBoxEndpoint.Location = new Point(60, 6);
            TextBoxEndpoint.Name = "TextBoxEndpoint";
            TextBoxEndpoint.PlaceholderText = "sqlserv.mkm.lan,1433";
            TextBoxEndpoint.Size = new Size(200, 23);
            TextBoxEndpoint.TabIndex = 1;
            TextBoxEndpoint.TextChanged += OnTextChanged;
            // 
            // TextBoxLogin
            // 
            TextBoxLogin.Location = new Point(324, 6);
            TextBoxLogin.Name = "TextBoxLogin";
            TextBoxLogin.PlaceholderText = "login";
            TextBoxLogin.Size = new Size(200, 23);
            TextBoxLogin.TabIndex = 3;
            TextBoxLogin.TextChanged += OnTextChanged;
            // 
            // TextBoxPassword
            // 
            TextBoxPassword.Location = new Point(324, 35);
            TextBoxPassword.Name = "TextBoxPassword";
            TextBoxPassword.PlaceholderText = "password";
            TextBoxPassword.Size = new Size(200, 23);
            TextBoxPassword.TabIndex = 4;
            TextBoxPassword.TextChanged += OnTextChanged;
            // 
            // TextBoxDatabase
            // 
            TextBoxDatabase.Location = new Point(60, 35);
            TextBoxDatabase.Name = "TextBoxDatabase";
            TextBoxDatabase.PlaceholderText = "DB_NAME";
            TextBoxDatabase.Size = new Size(200, 23);
            TextBoxDatabase.TabIndex = 2;
            TextBoxDatabase.TextChanged += OnTextChanged;
            // 
            // TextBoxFile
            // 
            TextBoxFile.Location = new Point(60, 64);
            TextBoxFile.Name = "TextBoxFile";
            TextBoxFile.PlaceholderText = "C:\\Users\\User\\Desktop\\infimum-data-source.json";
            TextBoxFile.Size = new Size(464, 23);
            TextBoxFile.TabIndex = 6;
            TextBoxFile.TextChanged += OnTextChanged;
            // 
            // NumericUpDownTimeout
            // 
            NumericUpDownTimeout.Location = new Point(592, 6);
            NumericUpDownTimeout.Maximum = new decimal(new int[] { 7200, 0, 0, 0 });
            NumericUpDownTimeout.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            NumericUpDownTimeout.Name = "NumericUpDownTimeout";
            NumericUpDownTimeout.Size = new Size(56, 23);
            NumericUpDownTimeout.TabIndex = 5;
            NumericUpDownTimeout.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // ButtonBrowse
            // 
            ButtonBrowse.Location = new Point(530, 64);
            ButtonBrowse.Name = "ButtonBrowse";
            ButtonBrowse.Size = new Size(80, 23);
            ButtonBrowse.TabIndex = 7;
            ButtonBrowse.Text = "Обзор...";
            ButtonBrowse.UseVisualStyleBackColor = true;
            ButtonBrowse.Click += OnBrowse;
            // 
            // TextBoxSql
            // 
            TextBoxSql.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TextBoxSql.BackColor = Color.FromArgb(30, 30, 30);
            TextBoxSql.BorderStyle = BorderStyle.FixedSingle;
            TextBoxSql.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            TextBoxSql.ForeColor = Color.Gainsboro;
            TextBoxSql.Location = new Point(0, 108);
            TextBoxSql.Multiline = true;
            TextBoxSql.Name = "TextBoxSql";
            TextBoxSql.PlaceholderText = "SELECT\r\n    t.[ColumnName] as [Имя_Столбца_Для_АПС]\r\nFROM\r\n    [dbo].[TableName] t\r\nORDER BY\r\n    1 ASC";
            TextBoxSql.ScrollBars = ScrollBars.Both;
            TextBoxSql.Size = new Size(714, 353);
            TextBoxSql.TabIndex = 10;
            TextBoxSql.TabStop = false;
            TextBoxSql.WordWrap = false;
            TextBoxSql.TextChanged += OnTextChanged;
            // 
            // SqlDataSourceConfigForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(714, 461);
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
            MinimumSize = new Size(730, 500);
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
    }
}

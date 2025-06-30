using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using UniversalPack.Configuration;
using UniversalPack.Extensions;

namespace ApsRTCustomComponents
{
    /// <summary>
    /// Форма редактирования источника данных SQL
    /// </summary>
    public partial class SqlDataSourceConfigForm : Form
    {
        /// <summary>
        /// Регулярка ардеса сервера
        /// </summary>
        private static readonly Regex _sqlEndpointRegex = new Regex(@"^[\w\d\-_\.]+(\,\d+)?$");

        /// <summary>
        /// Регулярка валидного пути
        /// </summary>
        private static readonly Regex _validPathRegex = new Regex(@"^(([a-zA-Z]:[\\\/])|(\\\\))?[\w\d\.\,\-_!$%\(\)\s\\\/\p{L}]+$");

        /// <summary>
        /// Цвет BG ошибки
        /// </summary>
        private static readonly Color _colorError = Color.FromArgb(255, 192, 192);

        /// <summary>
        /// Сохранённые цвета BG по умолчанию, определённые конструктором
        /// </summary>
        private readonly IDictionary<int, Color> _initialBackColor;

        /// <summary>
        /// Разница высоты поля SQL и формы - для правильного изменения размера
        /// </summary>
        //private readonly int _textBoxSqlHeightDiff;

        /// <summary>
        /// Разница ширины поля SQL и формы - для правильного изменения размера
        /// </summary>
        //private readonly int _textBoxSqlWidthDiff;

        /// <summary>
        /// Идёт проверка соединения
        /// </summary>
        private bool _isChecking;

        /// <summary>
        /// Строка пути для модуля
        /// </summary>
        public string CrutchPath => $"SQL:{TextBoxFile.Text}";

        /// <inheritdoc cref="SqlDataSourceConfigForm"/>
        public SqlDataSourceConfigForm()
        {
            InitializeComponent();

            //_textBoxSqlHeightDiff = Height - TextBoxSql.Height;
            //_textBoxSqlWidthDiff = Width - TextBoxSql.Width;
            _initialBackColor = Controls
                .OfType<Control>()
                .Select(e => new
                {
                    HashCode = e.GetHashCode(),
                    e.BackColor
                })
                .DistinctBy(e => e.HashCode)
                .ToDictionary(e => e.HashCode, e => e.BackColor);

            ValidateForm(false);
        }

        /// <summary>
        /// <inheritdoc cref="SqlDataSourceConfigForm"/>. Заполняет форму из файла
        /// </summary>
        /// <param name="crutchPath">Строка пути из текстбокса с SQL:</param>
        public SqlDataSourceConfigForm(string crutchPath) : this()
        {
            if (!crutchPath.ToLower().StartsWith("sql:"))
            {
                return;
            }

            // поскльку выше идёт проверка на .StartsWith("SQL:")
            // для получения имени файла срезаем 4 символа
            // с начала строки
            var fileName = crutchPath[4..].Trim();

            LoadConfigurationFile(fileName);
        }

        //protected override void OnSizeChanged(EventArgs e)
        //{
        //    base.OnSizeChanged(e);
        //    TextBoxSql.Height = Height - _textBoxSqlHeightDiff;
        //    TextBoxSql.Width = Width - _textBoxSqlWidthDiff;
        //}

        /// <summary>
        /// Установить доступность элементов управления
        /// </summary>
        /// <param name="enabled"></param>
        private void SetControlsEnabled(bool enabled)
        {
            Controls
                .OfType<Control>()
                .Where(e => e is Button || e is TextBox || e is NumericUpDown)
                .ForEach(e => e.Enabled = enabled);

            if (enabled)
            {
                ValidateForm();
            }
        }

        /// <summary>
        /// Проверка и загрузка конфигурационного файла
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadConfigurationFile(string fileName)
        {
            if (_validPathRegex.IsMatch(fileName))
            {
                fileName = Path.GetFullPath(fileName);
                TextBoxFile.Text = fileName;
            }
            else
            {
                MessageBox.Show(
                    "Некорректный путь к файлу",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }
            if (!CheckFileWithTimeout(fileName))
            {
                MessageBox.Show(
                    "Файл не найден",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            var file = new FileInfo(fileName);

            if (file.TryReadAsJson<MsSqlDataSourceConfig>(out var config, out var exception))
            {
                TextBoxSql.Text = config.Sql;
                TextBoxLogin.Text = config.Username;
                TextBoxPassword.Text = config.Password;
                TextBoxEndpoint.Text = config.EndPoint;
                TextBoxDatabase.Text = config.DatabaseName;
                NumericUpDownTimeout.Value = config.TimeoutSeconds;
            }
            else
            {
                MessageBox.Show(
                    $"Не удалось прочитать файл конфигурации \"{file.FullName}\"\r\n\r\n{exception.ToDetailedDataObject().ToJson(Formatting.Indented)}",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            ValidateForm(false);
        }

        /// <summary>
        /// Изменение текста в любом поле ввода
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextChanged(object sender, EventArgs e)
        {
            ValidateForm();
        }

        /// <summary>
        /// Валидация элемента управления и изменение его внешнего вида
        /// </summary>
        /// <typeparam name="TControl">Тип элемента упраления</typeparam>
        /// <param name="control">Элемент управления</param>
        /// <param name="highlight">Подсвечивать</param>
        /// <param name="validity">Признак валидности</param>
        /// <returns></returns>
        private bool ValidateControl<TControl>(
            TControl control,
            bool highlight,
            Func<TControl, bool> validity)
            where TControl : Control
        {
            if (validity.Invoke(control))
            {
                if (highlight && _initialBackColor.TryGetValue(control.GetHashCode(), out var backColor))
                {
                    control.BackColor = backColor;
                }

                return true;
            }
            else
            {
                control.BackColor = _colorError;

                return false;
            }
        }

        /// <summary>
        /// Валидация формы
        /// </summary>
        /// <param name="highlight">Подсветка элементов управления</param>
        private bool ValidateForm(bool highlight = true)
        {
            var isValid = true;

            isValid &= ValidateControl(
                TextBoxLogin,
                highlight,
                e => e.Text.Length > 0);

            isValid &= ValidateControl(
                TextBoxPassword,
                highlight,
                e => e.Text.Length >= 0);

            isValid &= ValidateControl(
                TextBoxDatabase,
                highlight,
                e => e.Text.Length > 0);

            isValid &= ValidateControl(
                TextBoxEndpoint,
                highlight,
                e => _sqlEndpointRegex.IsMatch(e.Text));

            isValid &= ValidateControl(
                TextBoxSql,
                highlight,
                e => e.Text.Length > 0);

            //isValid &= ValidateControl(
            //    TextBoxFile,
            //    highlight,
            //   e => _validPathRegex.IsMatch(e.Text));

            ButtonSubmit.Enabled = isValid;
            ButtonCheck.Enabled = isValid;

            return isValid;
        }

        private MsSqlDataSourceConfig CreateConfig()
        {
            var config = new MsSqlDataSourceConfig
            {
                Sql = TextBoxSql.Text,
                Username = TextBoxLogin.Text,
                Password = TextBoxPassword.Text,
                EndPoint = TextBoxEndpoint.Text,
                DatabaseName = TextBoxDatabase.Text,
                TimeoutSeconds = (int)NumericUpDownTimeout.Value
            };
            return config.TryUniversalValidation(true) ? config : null;
        }

        /// <summary>
        /// Нажатие кнопки подтверждения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSubmit(object sender, EventArgs e)
        {
            var isValid = ValidateForm();

            if (!isValid)
            {
                MessageBox.Show(
                    "Форма невалидна",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (TextBoxFile.TextLength == 0 || !File.Exists(TextBoxFile.Text))
                {
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        TextBoxFile.Text = saveFileDialog1.FileName;
                    }
                    else return;
                }
                var file = new FileInfo(TextBoxFile.Text);
                var config = CreateConfig();
                if (config != null)
                {
                    config.ToJsonFile(file, Formatting.Indented);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось сохранить файл\r\n\r\n{ex.ToDetailedDataObject().ToJson(Formatting.Indented)}",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Нажатие кнопки проверки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCheck(object sender, EventArgs e)
        {
            new Thread(CheckConnection).Start();
        }

        private void CheckConnection()
        {
            if (_isChecking)
            {
                return;
            }
            _isChecking = true;

            var config = CreateConfig();
            if (config == null)
            {
                _isChecking = false;
                return; // Выходим ни с чем
            }

            Invoke((MethodInvoker)(() => SetControlsEnabled(false)));

            try
            {
                var timer = new UniversalPack.Common.Timer();

                using (var connection = new System.Data.SqlClient.SqlConnection(config.ConnectionString))
                {
                    connection.SafeOpen();
                    connection.SafeClose();
                }

                var elapsed = timer.Span;

                Invoke((MethodInvoker)(() =>
                {
                    MessageBox.Show(
                        $"Подключение успешно установлено за {elapsed}",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }));
            }
            catch (Exception ex)
            {
                Invoke((MethodInvoker)(() =>
                {
                    MessageBox.Show(
                        $"Не удалось подключиться к серверу\r\n\r\n{ex.ToDetailedDataObject().ToJson(Formatting.Indented)}",
                        Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }));
            }

            Invoke((MethodInvoker)(() => SetControlsEnabled(true)));

            _isChecking = false;
        }

        /// <summary>
        /// Проверка существования файла
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static async Task<bool> CheckFile(string fileName)
        {
            try
            {
                var fileInfo = new FileInfo(fileName);

                return await Task.FromResult(fileInfo.Exists);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        /// <summary>
        /// Проверка существования файла с таймаутом в 500мс
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static bool CheckFileWithTimeout(string fileName)
        {
            var checkTask = CheckFile(fileName);
            var timeoutTask = Task.Delay(500);

            var completedTask = Task.WhenAny(checkTask, timeoutTask);
            completedTask.Wait();

            if (completedTask == timeoutTask)
            {
                return false;
            }
            else
            {
                return checkTask.Result;
            }
        }

        /// <summary>
        /// Нажатие кнопки "обзор"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBrowse(object sender, EventArgs e)
        {
            Enabled = false;

            using (var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "JSON|*.json",
                Multiselect = false
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadConfigurationFile(dialog.FileName);
                }
            }

            Enabled = true;
        }

        private void SaveAsButton_Click(object sender, EventArgs e)
        {

        }
    }
}

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace UniversalPack.Configuration
{
    // TODO: абстрагировать для возможности работы с другими СУБД
    // TODO: валидация полей
    /// <summary>
    /// Конфигурация источника данных MS SQL
    /// </summary>
    public sealed class MsSqlDataSourceConfig
    {

        /// <summary>
        /// Точка подключения
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Имя БД
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Таймаут в секундах
        /// </summary>
        [Range(10, 100)]
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// Строка SQL-запроса
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// Строка подключения
        /// </summary>
        /// <returns></returns>
        [JsonIgnore]
        public string ConnectionString
        {
            get => $"Data Source={EndPoint};Initial Catalog={DatabaseName};User ID={Username}; password={Password}; Connection Timeout={TimeoutSeconds}";
            //get
            //{
            //    var builder = new SqlConnectionStringBuilder
            //    {
            //        Authentication = SqlAuthenticationMethod.SqlPassword,
            //        PersistSecurityInfo = false,
            //        TrustServerCertificate = true,
            //        CommandTimeout = TimeoutSeconds,
            //        DataSource = EndPoint,
            //        InitialCatalog = DatabaseName,
            //        UserID = Username,
            //        Password = Password,
            //        Encrypt = false
            //        //IntegratedSecurity = true
            //        //Integrated Security=True;TrustServerCertificate=True
            //    };

            //    return builder.ToString();
            //}
        }
    }
}

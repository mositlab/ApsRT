using UniversalPack.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UniversalPack.Extensions;

namespace UniversalPack.Common
{
    public class Tools
    {
#if DEBUG
        private static string _solutionPath;

        public static string SolutionPath => _solutionPath ??= GetSolutionPath();
#endif

        public static JsonSerializerSettings JsonSerializerSettings => new JsonSerializerSettings
        {
            MaxDepth = 512,
            CheckAdditionalContent = true,
            TypeNameHandling = TypeNameHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateFormatString = DateTimeExtensions.DateTimeHumanFmt,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Пишет дамп объекта в файл
        /// </summary>
        /// <param name="filename">Имя файла</param>
        /// <param name="data">объект</param>
        /// <param name="gzip">сжать дамп в gzip</param>
        /// <returns></returns>
        public static async Task<string> WriteJsonAsync(string filename, object data, bool gzip)
        {
            if (gzip)
            {
                filename += ".gz";
            }

            using (var fs = new FileStream(filename, FileMode.Create))
            {
                using (var sw = new StreamWriter(gzip ? new GZipStream(fs, CompressionLevel.Optimal, true) : fs))
                {
                    await sw.WriteAsync(data.ToJson(Formatting.Indented));
                }

                return fs.Name;
            }
        }

        /// <summary>
        /// Пишет дамп объекта в файл
        /// </summary>
        /// <param name="filename">Имя файла</param>
        /// <param name="data">объект</param>
        /// <param name="gzip">сжать дамп в gzip</param>
        /// <returns></returns>
        public static string WriteJson(string filename, object data, bool gzip)
        {
            if (gzip)
            {
                filename += ".gz";
            }

            using (var fs = new FileStream(filename, FileMode.Create))
            {
                using (var sw = new StreamWriter(gzip ? new GZipStream(fs, CompressionLevel.Optimal, true) : fs))
                {
                    sw.Write(data.ToJson(Formatting.Indented));
                }

                return fs.Name;
            }
        }

        /// <summary>
        /// Создаёт путь к файлу <paramref name="fileName"/>
        /// </summary>
        /// <param name="fileName"></param>
        public static void EnsurePathExists(string fileName)
        {
            var dirName = Path.GetDirectoryName(fileName);

            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }

        /// <summary>
        /// Получает имя таблицы по типу сущности
        /// </summary>
        /// <param name="entityType">Тип сущности</param>
        /// <returns></returns>
        public static string GetEntityTableName(Type entityType)
        {
            var value = entityType?
                .CustomAttributes?
                .FirstOrDefault(e => e.AttributeType == typeof(TableAttribute))?
                .ConstructorArguments?
                .FirstOrDefault().Value;

            return value is string tableName ? tableName : null;
        }

        /// <inheritdoc cref="GetEntityTableName(Type)"/>
        public static string GetEntityTableName<TEntity>() where TEntity : class, new()
        {
            return GetEntityTableName(typeof(TEntity));
        }

#if DEBUG
        /// <summary>
        /// Возвращает физический путь к папке решения
        /// <br></br>
        /// <b>Использовать с осторожностью!</b>
        /// </summary>
        /// <returns></returns>
        private static string GetSolutionPath()
        {
            var thisDirectory = new DirectoryInfo(AppContext.BaseDirectory);

            while (thisDirectory?.Parent != null && !thisDirectory.GetFiles("*.sln").Any())
            {
                thisDirectory = thisDirectory.Parent;
            }

            return thisDirectory.FullName;
        }
#endif

        /// <summary>
        /// Запуск <paramref name="action"/> в дополнительном потоке
        /// </summary>
        /// <param name="action">Выполняемое действие</param>
        /// <param name="onSuccess">Событие успеха</param>
        /// <param name="onError">Событие неудачи</param>
        public static Thread RunInSeparateThread(Action action, Action onSuccess = null, Action<Exception> onError = null)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    action?.Invoke();
                    onSuccess?.Invoke();
                }
                catch (Exception e)
                {
                    onError?.Invoke(e);
                }
            });

            thread.Start();

            return thread;
        }

        /// <summary>
        /// Вычисляет максимум
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int Max(params int[] values) => values.SafeMax(e => e);
    }
}

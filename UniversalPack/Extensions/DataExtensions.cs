using UniversalPack.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UniversalPack.Common.DataLoader;

namespace UniversalPack.Extensions
{
    /// <summary>
    /// <see cref="DataRow"/> extensions
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// Get value of type <typeparamref name="T"/> from column <paramref name="columnName"/> of <paramref name="dataRow"/>
        /// </summary>
        /// <inheritdoc cref="TryGetValue{T}(DataRow, string, out T, out Exception)"/>
        public static bool TryGetValue<T>(this DataRow dataRow, string columnName, out T value) =>
            dataRow.TryGetValue(columnName, out value, out _);

        /// <summary>
        /// Get value of type <typeparamref name="T"/> from column <paramref name="columnName"/> of <paramref name="dataRow"/>
        /// </summary>
        /// <typeparam name="T">Tagret type</typeparam>
        /// <param name="dataRow">DataRow instance</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Output value</param>
        /// <param name="exception">Output exception</param>
        /// <returns><c>true</c> if cast/convert successful, <c>false</c> either</returns>
        public static bool TryGetValue<T>(this DataRow dataRow, string columnName, out T value, out Exception exception)
        {
            object rawValue;
            try
            {
                rawValue = dataRow[columnName];
            }
            catch (Exception e)
            {
                value = default;
                exception = e;
                return false;
            }

            if (rawValue is null || rawValue is DBNull)
            {
                value = default;
                exception = new Exception($"{nameof(dataRow)}[{columnName}]");
                return false;
            }

            if (rawValue.GetType() == typeof(T))
            {
                value = (T)rawValue;
                exception = default;
                return true;
            }

            try
            {
                value = (T)rawValue;
                exception = default;
                return true;
            }
            catch { }

            if (typeof(T) == typeof(bool))
            {
                try
                {
                    value = (T)(object)Convert.ToBoolean(rawValue);
                    exception = default;
                    return true;
                }
                catch { }
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));

            if (converter == null)
            {
                value = default;
                exception = new Exception(nameof(converter));
                return false;
            }

            try
            {
                value = (T)converter.ConvertFromString(rawValue.ToString());
                exception = default;
                return true;
            }
            catch (Exception e)
            {
                value = default;
                exception = e;
                return false;
            }
        }

        /// <summary>
        /// Преобразует <see cref="DataTable"/> в коллекцию моделей <typeparamref name="TModel"/>.
        /// </summary>
        public static ICollection<TModel> ToModelCollection<TModel>(this DataTable dataTable) where TModel : IDataRowLoadable, new()
        {
            var models = new List<TModel>();

            foreach (DataRow row in dataTable.Rows)
            {
                var model = new TModel();
                model.LoadFromDataRow(row);
                models.Add(model);
            }

            return models;
        }

        /// <summary>
        /// Преобразует перечисление моделей в <see cref="DataTable"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> models)
        {
            var props = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(e => e.PropertyType.IsSerializable && e.CanRead);

            var table = new DataTable();

            foreach (var prop in props)
            {
                table.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var model in models)
            {
                foreach (var prop in props)
                {
                    var row = table.NewRow();
                    row[prop.Name] = prop.GetValue(model);
                }
            }

            return table;
        }

        /// <summary>
        /// Преобразует перечисление моделей в CSV поток
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static Stream ToCsvStream<T>(this IEnumerable<T> models, char separator = ',')
        {
            var props = typeof(T)
                .GetProperties()
                .Where(e => e.PropertyType.IsSerializable && e.CanRead);

            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, encoding: Encoding.UTF8, leaveOpen: true))
            {
                writer.WriteLine(props.Select(e => e.Name).Join(separator));

                foreach (var model in models)
                {
                    writer.WriteLine(props.Select(e => e.GetValue(model)).Join(separator));
                }
            }

            stream.Position = 0;

            return stream;
        }

        /// <summary>
        /// Преобразует <see cref="IDataReader"/> в перечисление моделей <typeparamref name="TModel"/>.
        /// </summary>
        public static IEnumerable<TModel> ToModelEnumerable<TModel>(this IDataReader reader, DataLoaderOptions options) where TModel : IDataRecordLoadable, new()
        {
            while (reader.Read())
            {
                var model = new TModel();
                model.LoadFromDataRecord(reader, options);
                yield return model;
            }
        }

        /// <summary>
        /// Получает наименования колонок <see cref="IDataReader"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetColumnNames(this IDataReader reader)
        {
            using (var schema = reader.GetSchemaTable())
            {
                foreach (DataRow row in schema.Rows)
                {
                    yield return row["ColumnName"] as string;
                }
            }
        }

        /// <summary>
        /// Получение значений <see cref="IDataRecord"/>
        /// </summary>
        /// <remarks>
        /// Использовать только для получения отладочных сведений
        /// </remarks>
        /// <param name="record">Строка данных</param>
        /// <returns></returns>
        public static IReadOnlyCollection<object> GetValues(this IDataRecord record)
        {
            var values = new object[record.FieldCount];
            record.GetValues(values);

            return values;
        }
    }
}

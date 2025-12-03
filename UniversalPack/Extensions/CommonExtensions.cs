using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UniversalPack.Common;
using static System.Console;

namespace UniversalPack.Extensions
{
    public static class CommonExtensions
    {
        private static readonly NumberFormatInfo _nfi = CultureInfo.InvariantCulture.NumberFormat.Clone() as NumberFormatInfo;
        private static readonly Regex _hexRegex = new Regex("([0-9a-fA-F]{2})+");

        static CommonExtensions()
        {
            _nfi.NumberGroupSeparator = " ";
        }

        /// <summary>
        /// Changes any property of <paramref name="obj"/> using lambda <paramref name="action"/>.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        /// <param name="obj">Source object instance.</param>
        /// <param name="action">Target action to be performed.</param>
        /// <returns>Changed object.</returns>
        public static T Having<T>(this T obj, Action<T> action) where T : class
        {
            action.Invoke(obj);
            return obj;
        }

        /// <summary>
        /// Преобразует объект в <see cref="JToken"/>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static JToken ToJToken(this object data)
        {
            return JToken.FromObject(data);
        }

        /// <summary>
        /// Сериализация объекта в JSON
        /// </summary>
        /// <param name="data">Объект для сериализации</param>
        /// <param name="formatting">Форматирование</param>
        /// <returns></returns>
        public static string ToJson(this object data, Formatting formatting = Formatting.None)
        {
            if (data is System.Text.Json.JsonElement jElem)
            {
                data = jElem.ToObject();
            }

            return data is JToken jToken
                ? jToken.ToString(formatting)
                : JsonConvert.SerializeObject(
                    data,
                    Tools.JsonSerializerSettings.Having(e => e.Formatting = formatting)
                );
        }

        /// <summary>
        /// Сериализация объекта в JSON и сохранение в файл
        /// </summary>
        /// <param name="data">Объект для сериализации</param>
        /// <param name="fileInfo">Файл</param>
        /// <param name="formatting">Форматирование</param>
        /// <returns></returns>
        public static void ToJsonFile(this object data, FileInfo fileInfo, Formatting formatting = Formatting.None)
        {
            if (data is System.Text.Json.JsonElement jElem)
            {
                data = jElem.ToObject();
            }

            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            using (var stream = fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
            using (var jWriter = new JsonTextWriter(writer))
            {
                var jToken = data is JToken
                    ? data as JToken
                    : JToken.FromObject(data);

                var serializer = JsonSerializer.Create(Tools.JsonSerializerSettings.Having(e => e.Formatting = formatting));
                serializer.Serialize(jWriter, jToken);
                serializer = null;
            }
        }

        /// <summary>
        /// Попытка чтения файла как JSON и десериализации его содержимого в объект <typeparamref name="TObject"/>
        /// </summary>
        /// <typeparam name="TObject">Целевой тип объекта</typeparam>
        /// <param name="fileInfo">Файл</param>
        /// <param name="obj">Десериализованный объект</param>
        /// <param name="exception">Исключение</param>
        public static bool TryReadAsJson<TObject>(this FileInfo fileInfo, out TObject obj, out Exception exception)
        {
            if (!fileInfo.Exists)
            {
                obj = default;
                exception = new FileNotFoundException(default, fileInfo.FullName);
                return false;
            }

            try
            {
                using (var stream = fileInfo.OpenRead())
                using (var reader = new StreamReader(stream))
                using (var jReader = new JsonTextReader(reader))
                {
                    var serialiser = new JsonSerializer();
                    obj = serialiser.Deserialize<TObject>(jReader);
                }

                exception = default;
                return obj != null;
            }
            catch (Exception e)
            {
                exception = e;
                obj = default;
                return false;
            }
        }

        /// <summary>
        /// Отладочная сериализация объекта в JSON
        /// <br/>
        /// Пример выходного файла: Dump_20230220-105613_User.json
        /// </summary>
        /// <param name="data">Данные</param>
        /// <param name="name">Имя переменной. Использовать с nameof()</param>
        /// <param name="gzip">Сжать дамп с помощью GZip</param>
        public async static Task<string> DumpAsync(this object data, string name = null, bool gzip = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = $"Dump_{data?.GetType().Name}_{DateTime.Now.ToFileFmt()}.json";
            }
            else
            {
                name = $"Dump_{data?.GetType().Name}_{name}_{DateTime.Now.ToFileFmt()}.json";
            }

            foreach (var ch in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(ch, '_');
            }

            try
            {
                try
                {
                    return await Tools.WriteJsonAsync(name, data, gzip);
                }
                catch
                {
                    return await Tools.WriteJsonAsync($"/tmp/{name}", data, gzip);
                }
            }
            catch (Exception e)
            {
                await Out.WriteLineAsync($"Failed to save dump: {e.Message}\r\n{e.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Отладочная сериализация объекта в JSON
        /// <br/>
        /// Пример выходного файла: Dump_20230220-105613_User.json
        /// </summary>
        /// <param name="data">Данные</param>
        /// <param name="name">Имя переменной. Использовать с nameof()</param>
        /// <param name="gzip">Сжать дамп с помощью GZip</param>
        public static string Dump(this object data, string name = null, bool gzip = false)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = $"Dump_{data?.GetType().Name}_{DateTime.Now.ToFileFmt()}.json";
            }
            else
            {
                name = $"Dump_{data?.GetType().Name}_{name}_{DateTime.Now.ToFileFmt()}.json";
            }

            foreach (var ch in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(ch, '_');
            }

            try
            {
                try
                {
                    return Tools.WriteJson(name, data, gzip);
                }
                catch
                {
                    return Tools.WriteJson($"/tmp/{name}", data, gzip);
                }
            }
            catch (Exception e)
            {
                WriteLine("Failed to save dump: {0}\r\n{1}", e.Message, e.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Приведение JToken или объекта к заданному типу
        /// </summary>
        /// <typeparam name="T">Целевой тип</typeparam>
        /// <param name="input">"сырые" данные</param>
        /// <param name="value">Выходные данные</param>
        /// <param name="exception">Исключение</param>
        /// <returns>true если приведение успешно, иначе false</returns>
        public static bool TryCastTo<T>(this object input, out T value, out Exception exception)
        {
            if (input is T nothingToDo)
            {
                value = nothingToDo;
                exception = null;
                return true;
            }

            if (input != null)
            {
                try
                {
                    value = (T)input;
                    exception = null;
                    return true;
                }
                catch { }

                if (input is System.Text.Json.JsonElement jElem)
                {
                    input = JToken.Parse(jElem.GetRawText());
                }

                if (input is string str && str.TryParseJson(out var jToken1))
                {
                    input = jToken1;
                }
                else if (input.ToString().TryParseJson(out var jToken2))
                {
                    input = jToken2;
                }

                if (input is JToken jToken)
                {
                    try
                    {
                        value = jToken.ToObject<T>();
                        exception = null;
                        return true;
                    }
                    catch (Exception e)
                    {
                        value = default;
                        exception = e;
                        return false;
                    }
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(typeof(T));

                    if (converter != null)
                    {
                        try
                        {
                            value = (T)converter.ConvertFromString(input.ToString());
                            exception = null;
                            return value != null;
                        }
                        catch (Exception e)
                        {
                            value = default;
                            exception = e;
                            return false;
                        }
                    }
                }
            }

            value = default;
            exception = null;
            return false;
        }

        /// <summary>
        /// Приведение JToken или объекта к заданному типу
        /// </summary>
        /// <param name="input">"сырые" данные</param>
        /// <param name="type">Целевой тип</param>
        /// <param name="value">Выходные данные</param>
        /// <param name="exception">Исключение</param>
        /// <returns>true если приведение успешно, иначе false</returns>
        public static bool TryCastTo(this object input, Type type, out object value, out Exception exception)
        {
            if (input.GetType() == type || input.GetType().IsAssignableFrom(type))
            {
                value = input;
                exception = null;
                return true;
            }

            if (input != null)
            {
                if (input is string str && str.TryParseJson(out var jToken1))
                {
                    input = jToken1;
                }
                else if (input.ToString().TryParseJson(out var jToken2))
                {
                    input = jToken2;
                }

                if (input is JToken jToken)
                {
                    try
                    {
                        value = jToken.ToObject(type);
                        exception = null;
                        return true;
                    }
                    catch (Exception e)
                    {
                        value = default;
                        exception = e;
                        return false;
                    }
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(type);

                    if (converter != null)
                    {
                        try
                        {
                            value = converter.ConvertFromString(input.ToString());
                            exception = null;
                            return value != null;
                        }
                        catch (Exception e)
                        {
                            value = default;
                            exception = e;
                            return false;
                        }
                    }
                }
            }

            value = default;
            exception = null;
            return false;
        }

        /// <summary>
        /// Приведение JToken или объекта к заданному типу
        /// </summary>
        /// <typeparam name="T">Целевой тип</typeparam>
        /// <param name="input">"сырые" данные</param>
        /// <param name="value">Выходные данные</param>
        /// <returns>true если приведение успешно, иначе false</returns>
        public static bool TryCastTo<T>(this object input, out T value) =>
            input.TryCastTo(out value, out _);

        /// <summary>
        /// Приведение JToken или объекта к заданному типу
        /// </summary>
        /// <param name="input">"сырые" данные</param>
        /// <param name="type">Целевой тип</param>
        /// <param name="value">Выходные данные</param>
        /// <returns>true если приведение успешно, иначе false</returns>
        public static bool TryCastTo(this object input, Type type, out object value) =>
            input.TryCastTo(type, out value, out _);

        public static bool TryConvertTo<T>(this object input, out T converted)
        {
            if (input != null && input.TryConvertTo(typeof(T), out var objectConverted, out _))
            {
                converted = (T)objectConverted;
                return true;
            }
            else
            {
                converted = default;
                return false;
            }
        }

        /// <summary>
        /// Попытка конвертации объекта в тип
        /// </summary>
        /// <param name="input">Исходные данные</param>
        /// <param name="type">Целевой тип</param>
        /// <param name="converted">Сконвертированные данные</param>
        /// <param name="exception">Исключение</param>
        /// <returns></returns>
        public static bool TryConvertTo(this object input, Type type, out object converted, out Exception exception)
        {
            // это целевой тип
            if (input.GetType().IsAssignableFrom(type))
            {
                converted = input;
                exception = default;
                return true;
            }

            if (type.IsEnum)
            {
                // это удачно спаршенный из строки enum
                if (input.TryConvertTo<string>(out var enumAsString) && Enum.TryParse(type, enumAsString, true, out converted))
                {
                    exception = default;
                    return true;
                }
            }

            var inputString = input?.ToString() ?? string.Empty;

            if (type.IsAssignableFrom<Guid>() && string.IsNullOrEmpty(inputString))
            {
                converted = Guid.Empty;
                exception = default;
                return false;
            }

            var converter = TypeDescriptor.GetConverter(type);

            return converter.TryConvertFrom(inputString, out converted, out exception);
        }

        /// <summary>
        /// Попытка конвертации с помощью конвертера
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="value"></param>
        /// <param name="converted"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static bool TryConvertFrom(this TypeConverter converter, object value, out object converted, out Exception exception)
        {
            if (converter == null)
            {
                converted = default;
                exception = new NullReferenceException(nameof(converter));
                return false;
            }

            try
            {
                converted = converter.ConvertFromString(value?.ToString());
                exception = default;
                return true;
            }
            catch (Exception e)
            {
                converted = default;
                exception = e;
                return false;
            }
        }

        /// <inheritdoc cref="TryConvertTo(object, Type, out object, out Exception)"/>
        public static bool TryConvertTo(this object input, Type type, out object converted) =>
            input.TryConvertTo(type, out converted, out _);

        /// <summary>
        /// Попытка конвертации в тип или значение по умолчанию
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T ConvertToOrDefault<T>(this object input, T defaultValue = default)
        {
            if (input.TryConvertTo<T>(out var converted))
            {
                return converted;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Возвращает размер данных в понятном виде
        /// </summary>
        /// <param name="size">Размер данных</param>
        /// <returns></returns>
        public static string ToHumanSize(this int size)
        {
            string sizeStr, unit;

            if (size < 1 << 10)
            {
                sizeStr = size.ToString();
                unit = "B";
            }
            else if (size < 1 << 20)
            {
                sizeStr = Math.Round((double)size / (1 << 10), 2).ToString();
                unit = "KiB";
            }
            else if (size < 1 << 30)
            {
                sizeStr = Math.Round((double)size / (1 << 20), 2).ToString();
                unit = "MiB";
            }
            else
            {
                sizeStr = Math.Round((double)size / (1 << 30), 2).ToString();
                unit = "GiB";
            }

            if (sizeStr.Length > 4)
            {
                sizeStr = sizeStr[..4].Trim(',', '.');
            }

            return $"{sizeStr} {unit}";
        }

        /// <summary>
        /// Тот же TryGetValue, Только с попыткой преобразования значения в другой тип
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TOutValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetValue<TKey, TValue, TOutValue>(this IDictionary<TKey, TValue> dict, TKey key, out TOutValue value) where TOutValue : TValue
        {
            if (dict.TryGetValue(key, out var obj) && obj is TOutValue outValue)
            {
                value = outValue;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Преобразует исключение в рекурсивный объект, который можно сериализовать
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static object ToDetailedDataObject(this Exception exception)
        {
            return new
            {
                exception?.Message,
                exception?.StackTrace,
                exception?.Source,
                exception?.Data,
                Inner = exception?.InnerException?.ToDetailedDataObject()
            };
        }

        /// <summary>
        /// Получает самое вложенное исключение
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static Exception GetMostInnerException(this Exception exception)
        {
            while (exception?.InnerException != null)
            {
                exception = exception.InnerException;
            }

            return exception;
        }

        /// <summary>
        /// Конвертирует массив байт в HEX-строку
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] buffer)
        {
            return buffer.Select(b => b.ToString("x2")).Concat();
        }

        /// <summary>
        /// Конвертирует HEX-строку в массив байт
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this string hex)
        {
            if (hex == null || !_hexRegex.IsMatch(hex))
            {
                return Array.Empty<byte>();
            }

            return Enumerable.Range(0, hex.Length)
                .Where(e => e % 2 == 0)
                .Select(e => Convert.ToByte(hex.Substring(e, 2), 16))
                .ToArray();
        }

        /// <summary>
        /// Заполняет свойства объекта типа <typeparamref name="T"/> из объекта <paramref name="source"/>
        /// </summary>
        /// <typeparam name="T">Целевой тип объекта</typeparam>
        /// <typeparam name="S">Целевой тип объекта</typeparam>
        /// <param name="obj">Объект назначения</param>
        /// <param name="source">Объект-источник</param>
        /// <param name="propertyFilter">Фильтр свойств</param>
        public static T LoadFrom<T, S>(this T obj, S source, Func<PropertyInfo, bool> propertyFilter = default)
        {
            if (obj == null || source == null)
            {
                return obj;
            }

            propertyFilter ??= e => true;

            var tProps = typeof(T)
                .GetProperties()
                .Where(e => e.CanWrite)
                .Where(propertyFilter)
                .ToList();
            var sProps = typeof(S)
                .GetProperties()
                .Where(e => e.CanRead)
                .Where(propertyFilter)
                .ToList();

            var props = tProps
                .Select(e => new
                {
                    Source = sProps.FirstOrDefault(s => s.Name == e.Name && s.PropertyType == e.PropertyType),
                    Target = e
                })
                .Where(e => e.Source != null)
                .ToList();

            foreach (var prop in props)
            {
                var value = prop.Source.GetValue(source);

                if (value != null || value != default)
                {
                    prop.Target.SetValue(obj, value);
                }
            }

            tProps.Clear();
            sProps.Clear();
            props.Clear();

            return obj;
        }

        /// <summary>
        /// Преобразует поле сущности в пригодный для SQL-запроса формат
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string ToSqlCasted(this PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<ColumnAttribute>();

            if (attribute == null || string.IsNullOrWhiteSpace(attribute.TypeName))
            {
                return $"@{property.Name}";
            }
            else
            {
                return $"@{property.Name}::{attribute.TypeName}";
            }
        }

        /// <summary>
        /// Преобразует объект в словарь свойств
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyFilter"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToPropertiesDictionary(this object obj, Func<PropertyInfo, bool> propertyFilter = default)
        {
            propertyFilter ??= e => true;
            var dictionary = new Dictionary<string, object>();
            var props = obj.GetType().GetProperties().Where(propertyFilter);

            foreach (var prop in props)
            {
                try
                {
                    dictionary[prop.Name] = prop.GetValue(obj);
                }
                catch
                {
                    dictionary[prop.Name] = null;
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Переводит <see cref="System.Text.Json.JsonElement"/> в <see cref="object"/> для нормальной сериализации силами Newtonsoft.Json
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static object ToObject(this System.Text.Json.JsonElement element)
        {
            switch (element.ValueKind)
            {
                case System.Text.Json.JsonValueKind.Object:
                    var obj = new Dictionary<string, object>();

                    foreach (var property in element.EnumerateObject())
                    {
                        obj[property.Name] = property.Value.ToObject();
                    }

                    return obj;
                case System.Text.Json.JsonValueKind.Array:
                    return element
                        .EnumerateArray()
                        .Select(ToObject);
                case System.Text.Json.JsonValueKind.String:
                    return element.GetString();
                case System.Text.Json.JsonValueKind.Number:
                    return element.GetDecimal();
                case System.Text.Json.JsonValueKind.True:
                    return true;
                case System.Text.Json.JsonValueKind.False:
                    return false;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Преобразует число в человекочитаемую строку
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>Из кода Влада</remarks>
        public static string ToHumanFmt(this int value)
        {
            return value.ToString("N0", _nfi);
        }

        /// <summary>
        /// Преобразует число в человекочитаемую строку
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>Из кода Влада</remarks>
        public static string ToHumanFmt(this double value)
        {
            return value.ToString("n", _nfi);
        }

        /// <summary>
        /// Число находится между двумя другими
        /// </summary>
        /// <param name="value"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static bool IsBetween(this byte value, byte low, byte high)
        {
            if (low > high)
            {
                return value.IsBetween(high, low);
            }

            return value > low && value < high;
        }

        /// <summary>
        /// Число находится между двумя другими
        /// </summary>
        /// <param name="value"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static bool IsBetween(this double value, double low, double high)
        {
            if (low > high)
            {
                return value.IsBetween(high, low);
            }

            return value > low && value < high;
        }

        /// <summary>
        /// Число находится между двумя другими
        /// </summary>
        /// <param name="value"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static bool IsBetween(this int value, double low, double high)
        {
            if (low > high)
            {
                return value.IsBetween(high, low);
            }

            return value > low && value < high;
        }

        /// <summary>
        /// Преобразует <see cref="FileInfo"/> в объект деталей для записи в лог
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static object ToLogDetailsObject(this FileInfo file)
        {
            return new
            {
                file?.FullName,
                file?.Length,
                file?.LastWriteTime,
                file?.Exists
            };
        }

        /// <summary>
        /// Создаёт файл в директории
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FileInfo CreateFile(this DirectoryInfo directory, string name)
        {
            if (!directory.Exists)
            {
                directory.Create();
            }

            return new FileInfo(Path.Combine(directory.FullName, name));
        }

        /// <summary>
        /// Создаёт или открывает на запись существующий файл
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static FileStream OpenWriteOrCreate(this FileInfo file)
        {
            if (file.Exists)
            {
                return file.OpenWrite();
            }
            else
            {
                return file.Create();
            }
        }

        /// <summary>
        /// Делает XOR всех байт в UInt32
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static uint MakeUInt32Xor(this byte[] data)
        {
            var output = uint.MinValue;

            for (var i = 0; i < data.Length; i++)
            {
                output ^= (uint)data[i] << 8 * (i % 4);
            }

            return output;
        }

        public static float? ConvertToFloatByAnyCost(this object rawValue)
        {
            if (rawValue == null) return null;
            try
            {
                if (rawValue is float fl)
                {
                    return fl;
                }
                if (rawValue is double db)
                {
                    return (float)db;
                }
                if (rawValue is decimal dc)
                {
                    return (float)dc;
                }
                if (rawValue is int integer)
                {
                    return (float)integer;
                }
                if (rawValue is string str)
                {
                    return string.IsNullOrEmpty(str) ? null : str.ConvertToOrDefault(0.0f);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Перемешивалка битов
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //public static int ReverseInt(this int value)
        //{
        //    int k = 32;
        //    int reversed = 0;

        //    for (int i = 0; i < k; i++)
        //    {
        //        // If the ith bit of x is toggled, toggle the ith bit from the right of reversed
        //        reversed |= (value & (8 << i)) != 0 ? 1 << (k - 1 - i) : 0;
        //    }

        //    //for (int i = 0; i < k; i++)
        //    //{
        //    //    // If the ith bit of x is toggled, toggle the ith bit from the right of reversed
        //    //    reversed |= (value & (1 << i)) != 0 ? 1 << (k - 1 - i) : 0;
        //    //}
        //    return reversed;
        //}
    }
}

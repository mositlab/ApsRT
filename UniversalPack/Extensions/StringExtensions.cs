using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace UniversalPack.Extensions
{
    public static class StringExtensions
    {
        private const string _qwertyLat = "qwertyuiop[]asdfghjkl;'zxcvbnm,./QWERTYUIOP{}ASDFGHJKL:\"ZXCVBNM<>?";
        private const string _qwertyRus = "йцукенгшщзхъфывапролджэячсмитьбю.ЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ,";
        private const string _similarRus = "АВЕКМНОРСТУХавекмнорстух";
        private const string _similarLat = "ABEKMHOPCTYXabekmhopctyx";
        private const string _sqlEscapeChars = "_%()";
        private const string _valueYesDebloated = "да";
        private const string _valueNoDebloated = "нет";
        private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly Regex _whiteSpacesReplaceRegex = new Regex(@"\s+");
        private static readonly Regex _oneOrMoreLetterCharRegex = new Regex(@"\p{L}+");
        private static readonly Regex _debloatRegex = new Regex(@"[\s\,\.\-_\+\(\)\!\/\""\u0027\^\$%]");
        private static readonly HashSet<char> _bloatChars = new HashSet<char>
        {
            ' ', ',', '.', '-', '_', '+', '(', ')', '!', '/', '\\', '\u0027', '\t', '^', '$', '%'
        };
        private static readonly HashSet<char> _whitespaceChars = new HashSet<char>
        {
            ' ', ' ', '\r', '\n', '\t', '\v'
        };

        /// <summary>
        /// Валидация строки Email-адреса
        /// </summary>
        /// <returns>true если адрес валидный</returns>
        public static bool IsValidEmailAddress(this string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            return MailAddress.TryCreate(email, out _);
        }

        /// <inheritdoc cref="string.Format(string, object[])"/>
        public static string Format(this string format, params object[] args) => string.Format(format, args);

        /// <inheritdoc cref="string.Join{T}(string, IEnumerable{T})"/>
        public static string Join<T>(this IEnumerable<T> values, string separator)
        {
            return string.Join(separator, values);
        }

        /// <inheritdoc cref="string.Join{T}(string, IEnumerable{T})"/>
        public static string Join<T>(this IEnumerable<T> values, char separator)
        {
            return string.Join(separator, values);
        }

        /// <summary>
        /// Обрезает пробелы на каждом слове, заменяет все пробельные символы на обычный SP (<c>0x20</c>)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string TrimAll(this string text)
        {
            //здесь это может не потребоваться
            //text = _whiteSpacesTrimRegex.Replace(text, " ");
            text = _whiteSpacesReplaceRegex.Replace(text, " ");

            return text.Trim();
        }

        /// <summary>
        /// Обёртка для <see cref="Enum.TryParse{TEnum}(string, out TEnum)"/>. Возвращает значение enum'а при удачном парсинге, иначе null
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static TEnum? ParseEnum<TEnum>(this string str) where TEnum : struct
        {
            return Enum.TryParse<TEnum>(str, true, out var parsed) ? parsed : null;
        }

        /// <summary>
        /// Ескейпинг сипволов для подстановки в SQL запрос
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SqlEscape(this string value)
        {
            foreach (var e in _sqlEscapeChars)
            {
                value = value.Replace($"{e}", $"\\{e}");
            }

            return value;
        }

        /// <summary>
        /// Переводит раскладку с русского на английский
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Qwertify(this string input)
        {
            return new string(
                input
                .ToUpper()
                .Select(e => _qwertyRus.Contains(e) ? _qwertyLat[_qwertyRus.IndexOf(e)] : e)
                .ToArray());
        }

        /// <summary>
        /// Переводит раскладку с английского на русский
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Deqwertify(this string input)
        {
            return new string(
                input
                .ToUpper()
                .Select(e => _qwertyLat.Contains(e) ? _qwertyRus[_qwertyLat.IndexOf(e)] : e)
                .ToArray());
        }

        /// <summary>
        /// Парсим положительное целое
        /// </summary>
        /// <param name="input">Строка</param>
        /// <param name="parsed">Спаршенное значение</param>
        /// <returns></returns>
        public static bool TryParsePositiveInt(this string input, out int parsed)
        {
            return int.TryParse(input, out parsed) && parsed > 0;
        }

        /// <summary>
        /// Парсим положительное целое
        /// </summary>
        /// <param name="input">Строка</param>
        /// <returns></returns>
        public static int ParsePositiveIntOrDefault(this string input)
        {
            return int.TryParse(input, out var parsed) && parsed > 0 ? parsed : 0;
        }

        /// <summary>
        /// Парсим положительное дробное
        /// </summary>
        /// <param name="input">Строка</param>
        /// <param name="parsed">Спаршенное значение</param>
        /// <returns></returns>
        public static bool TryParseDouble(this string input, out double parsed)
        {
            return double.TryParse(input?.Replace(".", ","), out parsed);
        }

        /// <summary>
        /// Парсим положительное дробное
        /// </summary>
        /// <param name="input">Строка</param>
        /// <param name="parsed">Спаршенное значение</param>
        /// <returns></returns>
        public static bool TryParsePositiveDouble(this string input, out double parsed)
        {
            return input.TryParseDouble(out parsed) && parsed > 0.0;
        }

        /// <summary>
        /// Парсим положительное дробное
        /// </summary>
        /// <param name="input">Строка</param>
        /// <returns></returns>
        public static double ParsePositiveDoubleOrDefault(this string input)
        {
            return double.TryParse(input?.Replace(".", ","), out var parsed) && parsed > 0.0 ? parsed : 0;
        }

        /// <summary>
        /// Парсим дробное
        /// </summary>
        /// <param name="input">Строка</param>
        /// <returns></returns>
        public static double ParseDoubleOrDefault(this string input, double @default = 0.0)
        {
            return double.TryParse(input?.Replace(".", ","), out var parsed) ? parsed : @default;
        }

        /// <summary>
        /// TryParse для коллекции
        /// </summary>
        /// <typeparam name="T">Получаемый тип</typeparam>
        /// <param name="input">Входная строка</param>
        /// <param name="delimiter">Разделитель</param>
        /// <param name="result">Выходные значения</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static bool TryParseMany<T>(this string input, char delimiter, out ICollection<T> result)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T)) ?? throw new NotSupportedException($"Unconvertible type got: '{typeof(T).FullName}'");
            result = new List<T>();

            foreach (var value in input.Split(delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                try
                {
                    result.Add((T)converter.ConvertFromString(value));
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Попытка парса строки JSON в <see cref="JToken"/>
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <param name="jToken">Спаршенный <see cref="JToken"/></param>
        /// <returns>true если парсинг успешен, иначе false</returns>
        public static bool TryParseJson(this string input, out JToken jToken)
        {
            try
            {
                jToken = JToken.Parse(input);
                return true;
            }
            catch
            {
                jToken = default;
                return false;
            }
        }

        /// <summary>
        /// Транслитерация похожих символов на латиницу
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns></returns>
        public static string SimilarTransliterate(this string input)
        {
            return new string(input
                .ToUpper()
                .Select(e => _similarRus.Contains(e) ? _similarLat[_similarRus.IndexOf(e)] : e)
                .ToArray());
        }

        /// <summary>
        /// Транслитерация похожих символов на кириллицу
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns></returns>
        public static string SimilarBackTransliterate(this string input)
        {
            return new string(input
                .ToUpper()
                .Select(e => _similarLat.Contains(e) ? _similarRus[_similarLat.IndexOf(e)] : e)
                .ToArray());
        }

        public static string Concat<T>(this IEnumerable<T> values) =>
            string.Concat(values);

        public static bool TryParseEndPoint(this string input, out EndPoint endPoint)
        {
            var split = input.Split(':');

            if (split.Length != 2)
            {
                endPoint = default;

                return false;
            }

            if (!split[1].TryParsePositiveInt(out var port))
            {
                endPoint = default;

                return false;
            }

            if (IPAddress.TryParse(split[0], out var ip))
            {
                endPoint = new IPEndPoint(ip, port);
            }
            else
            {
                endPoint = new DnsEndPoint(split[0], port);
            }

            return true;
        }

        /// <summary>
        /// Возвращает количество совпадающих с начала символов
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static int StartEquality(this string source, string target)
        {
            int i;

            for (i = 0; i < source.Length && i < target.Length && source[i] == target[i]; i++)
            {
                ;
            }

            return i;
        }

        /// <summary>
        /// Преобразует строку в PascalCase
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToPascalCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var result = value[0].ToString().ToUpper();

            if (value.Length > 1)
            {
                result += value[1..];
            }

            return result;
        }

        /// <summary>
        /// Преобразует строку в cascalCase
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var result = value[0].ToString().ToLower();

            if (value.Length > 1)
            {
                result += value[1..];
            }

            return result;
        }

        /// <summary>
        /// Конвертирует строку в Base64, совместимый с URL
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToBase64Url(this string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);

            return Convert.ToBase64String(inputBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        /// <summary>
        /// Попытка спарсить base64
        /// </summary>
        /// <param name="input"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool TryParseBase64(this string input, out byte[] data)
        {
            try
            {
                data = Convert.FromBase64String(input);
                return true;
            }
            catch
            {
                data = null;
                return false;
            }
        }

        /// <summary>
        /// Соответствие строки паттерну <c>\p{L}+</c>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool HasOneOrMoreLetterCharacters(this string input)
        {
            return _oneOrMoreLetterCharRegex.IsMatch(input);
        }

        /// <summary>
        /// Чистит имя файла от недопустимых символов
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ClearFileName(this string input)
        {
            var charsToReplace = _invalidFileNameChars.Where(input.Contains).ToList();

            if (charsToReplace.Any())
            {
                foreach (var c in charsToReplace)
                {
                    input = input.Replace(c, '_');
                }
            }

            charsToReplace.Clear();

            return input;
        }

        /// <summary>
        /// Убирает пробельные символы из строки
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveWhitespaces(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var result = new StringBuilder(input.Length);

            foreach (char c in input)
            {
                if (!_whitespaceChars.Contains(c))
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Убирает лишние символы, преобразует строку в нижний регистр
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Debloat(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            //var value = _debloatRegex.Replace(input.ToLower(), string.Empty);

            // создаем новую строку без ненужных символов
            var result = new StringBuilder(input.Length);

            foreach (char c in input)
            {
                if (!_bloatChars.Contains(c))
                {
                    result.Append(c);
                }
            }

            var value = result.ToString().ToLower();

            if (value.Length > 0)
            {
                return value.Replace("ё", "е").Replace("й", "и");
            }
            else
            {
                return value;
            }
        }


        /// <summary>
        /// Эта строка содержит единственное слово "да"
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsYes(this string value)
        {
            return value?.Debloat() == _valueYesDebloated;
        }

        /// <summary>
        /// Эта строка содержит единственное слово "нет"
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNo(this string value)
        {
            return value?.Debloat() == _valueNoDebloated;
        }
    }
}

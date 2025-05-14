using System;
using System.Globalization;

namespace UniversalPack.Extensions
{
    public static class DateTimeExtensions
    {
        public const string DateDbFmt = "yyyy-MM-dd";
        public const string DateHumanFmt = "dd.MM.yyyy";
        public const string DateTimeFileFmt = "yyyyMMdd-HHmmss";
        public const string DateTimeLogFmt = "yyyy-MM-dd HH:mm:ss.fff";
        public const string DateTimeDbFmt = "yyyy-MM-dd HH:mm:ss";
        public const string DateTimeHumanFmt = "dd.MM.yyyy HH:mm";
        public const string DateTimeToNumericFmt = "yyyyMMddHHmmss";

        /// <summary>
        /// Для парса
        /// </summary>
        private static readonly string[] _dateFormats = new[]
        {
            DateHumanFmt,
            DateDbFmt
        };

        /// <summary>
        /// Для парса
        /// </summary>
        private static readonly string[] _dateTimeFormats = new[]
        {
            "dd.MM.yyyy H:mm:ss",
            DateHumanFmt,
            "dd.MM.yyyy H:mm",
            DateTimeDbFmt,
            DateDbFmt,
            DateTimeFileFmt,
            DateTimeHumanFmt,
            DateTimeToNumericFmt,
            DateTimeLogFmt
        };

        private static readonly DateTime _unixBase = new DateTime(1970, 1, 1);

        public static string ToLogFmt(this DateTime dt) => dt.ToString(DateTimeLogFmt);

        public static string ToDbFmt(this DateTime dt) => dt.ToString(DateTimeDbFmt);

        /// <summary>
        /// Время для имени файла
        /// </summary>
        public static string ToFileFmt(this DateTime dt) => dt.ToString(DateTimeFileFmt);

        public static string ToHumanFmt(this DateTime dt) => dt.ToString(DateTimeHumanFmt);

        public static string ToDbFmt(this DateOnly date) => date.ToString(DateDbFmt);

        public static string ToHumanFmt(this TimeSpan ts)
        {
            var str = string.Empty;

            if (ts.TotalDays > 1)
            {
                str += $"{(int)ts.TotalDays} ";
            }
            if (ts.TotalHours > 1)
            {
                str += $"{ts.Hours}:";
            }

            str += $"{ts.Minutes.ToString().PadLeft(2, '0')}:{ts.Seconds.ToString().PadLeft(2, '0')}";

            return str;
        }

        /// <summary>
        /// Парсим строку с датой и временем
        /// </summary>
        public static bool TryParseDateTime(this string dtStr, out DateTime parsed)
        {
            foreach (var dtFmt in _dateTimeFormats)
            {
                if (DateTime.TryParseExact(dtStr, dtFmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                {
                    return true;
                }
            }

            parsed = default;
            return false;
        }

        /// <summary>
        /// Парсим строку с датой и временем
        /// </summary>
        public static bool TryParseDateOnly(this string dtStr, out DateOnly parsed)
        {
            foreach (var dtFmt in _dateFormats)
            {
                if (DateOnly.TryParseExact(dtStr, dtFmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                {
                    return true;
                }
            }

            parsed = default;
            return false;
        }

        /// <summary>
        /// Парсим строку с датой и временем
        /// </summary>
        /// <returns>Объект даты-времени или null при неудачном парсе</returns>
        public static DateTime? ParseDateTime(this string dtStr)
        {
            return dtStr != null && dtStr.TryParseDateTime(out var parsed)
                ? parsed
                : null;
        }

        /// <summary>
        /// Парсим строку с датой 
        /// </summary>
        /// <returns>Объект даты или null при неудачном парсе</returns>
        public static DateOnly? ParseDateOnly(this string dtStr)
        {
            return dtStr != null && dtStr.TryParseDateOnly(out var parsed)
                ? parsed
                : null;
        }

        /// <summary>
        /// Преобразует дату-время в Unix-время
        /// </summary>
        public static int ToUnixTimestamp(this DateTime dt) =>
            (int)dt.ToUniversalTime().Subtract(_unixBase).TotalSeconds;

        /// <summary>
        /// Преобразует Unix-время в <see cref="DateTime"/>
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this int timestamp) =>
            _unixBase.AddSeconds(timestamp);

        /// <summary>
        /// Преобразует дату-время в формат, состоящий только из цифр
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToNumericFmt(this DateTime dt) =>
            dt.ToString(DateTimeToNumericFmt);

        /// <summary>
        /// Достаёт время из объекта даты-времени
        /// </summary>
        public static TimeOnly ToTimeOnly(this DateTime dt) =>
            TimeOnly.FromDateTime(dt);

        /// <summary>
        /// Достаёт дату из объекта даты-времени
        /// </summary>
        public static DateOnly ToDateOnly(this DateTime dt) =>
             DateOnly.FromDateTime(dt);
    }
}

using ExcelDataReader;
using InfimumMES.Core.Common;
using System;
using System.IO;

namespace InfimumMES.Core.Extensions
{
    public static class ExcelExtensions
    {
        private static readonly char[] _separators = new char[] { '\t', '|' };

        private static readonly ExcelReaderConfiguration _fasterConfig = new ExcelReaderConfiguration
        {
            // анализируем первую строку и имеем более быстрое чтение, но не имеем колво строк
            AnalyzeInitialCsvRows = 1,
            LeaveOpen = true,
            AutodetectSeparators = _separators
        };

        private static readonly ExcelReaderConfiguration _allRowsConfig = new ExcelReaderConfiguration
        {
            // анализируем всё и имеем кол-во строк
            AnalyzeInitialCsvRows = 0,
            LeaveOpen = true,
            AutodetectSeparators = _separators
        };

        /// <summary>
        /// Создаёт читалку Excel файла
        /// </summary>
        /// <param name="stream">Поток данных</param>
        /// <param name="getRowCount">Получить количество строк. Если считать строки - будет более медленная загрузка</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static ExcelDataReaderWrapper CreateExcelReader(this Stream stream, bool getRowCount = true)
        {
            if (!stream.CanRead || !stream.CanSeek)
            {
                throw new InvalidOperationException();
            }

            var reader = ExcelReaderFactory.CreateReader(stream, getRowCount ? _allRowsConfig : _fasterConfig);

            return new ExcelDataReaderWrapper(reader, true);
        }

        /// <summary>
        /// Создаёт читалку CSV файла
        /// </summary>
        /// <param name="stream">Поток данных</param>
        /// <param name="getRowCount">Получить количество строк. Если считать строки - будет более медленная загрузка</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static ExcelDataReaderWrapper CreateCsvReader(this Stream stream, bool getRowCount = true)
        {
            if (!stream.CanRead || !stream.CanSeek)
            {
                throw new InvalidOperationException();
            }

            var reader = ExcelReaderFactory.CreateCsvReader(stream, getRowCount ? _allRowsConfig : _fasterConfig);

            return new ExcelDataReaderWrapper(reader, getRowCount);
        }
    }
}

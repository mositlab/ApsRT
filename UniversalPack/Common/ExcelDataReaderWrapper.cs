using ExcelDataReader;
using UniversalPack.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace UniversalPack.Common
{
    /// <summary>
    /// Обёртка <see cref="IExcelDataReader"/>
    /// </summary>
    /// <remarks>
    /// Данная реализация разрешает имена столбцов с помощью расширения <see cref="StringExtensions.Debloat(string)"/>
    /// <br/>
    /// Присутствующая в наборе данных колонка <c>ИмЯ_СтоЛБЦа</c> будет разрешена при поиске колонки <c>Имя слолбца</c>,<c>ИмЯСтОлБцА___</c> и т.д.
    /// </remarks>
    public sealed class ExcelDataReaderWrapper : IDataReader
    {
        /// <summary>
        /// Метаданные таблицы
        /// </summary>
        private readonly DataTable _schemaTable;

        /// <summary>
        /// Пропускать пустые строки
        /// </summary>
        private readonly bool _skipEmptyRows;

        /// <summary>
        /// Экземпляр DataReader
        /// </summary>
        private readonly IExcelDataReader _reader;

        /// <summary>
        /// Кэш имён колонок. В ключе - string.GetHashCode
        /// </summary>
        private readonly IDictionary<int, int> _columnCache;

        /// <summary>
        /// Имена полей
        /// </summary>
        private IReadOnlyList<string> FieldNames { get; }

        public int Depth => _reader.Depth;

        public bool IsClosed => _reader.IsClosed;

        /// <summary>
        /// Количество строк
        /// </summary>
        public int RecordsAffected { get; }

        /// <summary>
        /// Количество колонок
        /// </summary>
        public int FieldCount => _reader.FieldCount;

        /// <summary>
        /// <inheritdoc cref="ExcelDataReaderWrapper"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="hasRowCount">reader содержит информацию о количестве строк</param>
        /// <param name="skipEmptyRows">Пропускать строки, все значения которых пусты или состоят из пробельных символов</param>
        /// <exception cref="InvalidOperationException"></exception>
        public ExcelDataReaderWrapper(IExcelDataReader reader, bool hasRowCount, bool skipEmptyRows = true)
        {
            if (!reader.Read())
            {
                throw new InvalidOperationException();
            }

            _skipEmptyRows = skipEmptyRows;

            // тот, ради кого всё делается
            _reader = reader;

            // конструируем кэши
            _columnCache = new Dictionary<int, int>();

            // первая строка - это имена колонок
            FieldNames = Enumerable.Range(0, reader.FieldCount)
                .Select(reader.GetValue)
                .Select((e, i) => e as string ?? e?.ToString() ?? $"Column{i}")
                .ToArray();

            if (hasRowCount)
            {
                try
                {
                    // количество строк - всё за исключением первой строки с заголовками
                    RecordsAffected = reader.RowCount - 1;
                }
                catch
                {
                    // кол-во строк нам неизвестно
                    RecordsAffected = -1;
                }
            }
            else
            {
                RecordsAffected = -1;
            }

            // инициализируем метаданные
            using (var temp = new DataTable())
            {
                FieldNames.ForEach(e => temp.Columns.Add(e, typeof(string)));

                using (var tempReader = temp.CreateDataReader())
                {
                    _schemaTable = tempReader.GetSchemaTable();
                }
            }
        }

        public object this[int i] => _reader.GetValue(i);

        public object this[string name]
        {
            get
            {
                var hash = name.GetHashCode();

                if (_columnCache.TryGetValue(hash, out var index))
                {
                    return this[index];
                }

                if (TryGetColumnIndex(name, out index))
                {
                    _columnCache[hash] = index;
                    return this[index];
                }

                throw new IndexOutOfRangeException($"Имя столбца '{name}' не может быть разрешено, т.к. не нашлось ни одного совпадения с присутствующими именами: {FieldNames.Select(e => $"'{e}'").Join(", ")}");
            }
        }

        /// <summary>
        /// Попытка получения индекса колонки по имени
        /// </summary>
        /// <param name="name"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool TryGetColumnIndex(string name, out int index)
        {
            var debloated = name.Debloat();

            for (index = 0; index < FieldNames.Count; index++)
            {
                if (FieldNames[index].Debloat() == debloated)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Проверка, состоит ли текущая строка только из пустых значений
        /// </summary>
        /// <returns></returns>
        private bool IsRowEmptyOrWhitespace()
        {
            for (var i = 0; i < FieldCount; i++)
            {
                if (!string.IsNullOrWhiteSpace(this[i]?.ToString()))
                {
                    return false;
                }
            }

            return true;
        }

        public void Close()
        {
            _reader.Close();
        }

        public DataTable GetSchemaTable()
        {
            return _schemaTable;
        }

        public bool NextResult()
        {
            return _reader.NextResult();
        }

        public bool Read()
        {
            if (_skipEmptyRows)
            {
                bool reader;

                // если строка пуста то переходим к следующей строке
                // пока либо не найдём непустую строку, либо данные не закончатся
                while ((reader = _reader.Read()) && IsRowEmptyOrWhitespace()) { }

                return reader;
            }

            return _reader.Read();
        }

        public bool GetBoolean(int i)
        {
            return Convert.ToBoolean(this[i]);
        }

        public byte GetByte(int i)
        {
            return Convert.ToByte(this[i]);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return Convert.ToChar(this[i]);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            return typeof(string).Name;
        }

        public DateTime GetDateTime(int i)
        {
            return GetString(i).TryParseDateTime(out var parsed) ? parsed : default;
        }

        public decimal GetDecimal(int i)
        {
            return Convert.ToDecimal(this[i]);
        }

        public double GetDouble(int i)
        {
            return Convert.ToDouble(this[i]);
        }

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        public Type GetFieldType(int i)
        {
            return typeof(string);
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(this[i]);
        }

        public Guid GetGuid(int i)
        {
            return Guid.TryParse(GetString(i), out var guid) ? guid : default;
        }

        public short GetInt16(int i)
        {
            return Convert.ToInt16(this[i]);
        }

        public int GetInt32(int i)
        {
            return Convert.ToInt32(this[i]);
        }

        public long GetInt64(int i)
        {
            return Convert.ToInt64(this[i]);
        }

        public string GetName(int i)
        {
            return FieldNames[i];
        }

        public int GetOrdinal(string name)
        {
            return TryGetColumnIndex(name, out var index) ? index : -1;
        }

        public string GetString(int i)
        {
            var obj = this[i];

            return this[i] is string strValue ? strValue : obj?.ToString();
        }

        public object GetValue(int i)
        {
            return _reader.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            for (var i = 0; i < FieldCount; i++)
            {
                values[i] = this[i];
            }

            return FieldCount;
        }

        public bool IsDBNull(int i)
        {
            return _reader.IsDBNull(i);
        }

        public void Dispose()
        {
            _schemaTable.Dispose();
            _columnCache.Clear();
            _reader.Dispose();
        }
    }
}

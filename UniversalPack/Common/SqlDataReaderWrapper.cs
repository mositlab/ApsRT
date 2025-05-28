using UniversalPack.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace UniversalPack.Common
{
    /// <summary>
    /// Обёртка <see cref="SqlDataReader"/>
    /// </summary>
    /// <remarks>
    /// Данная реализация разрешает имена столбцов с помощью расширения <see cref="StringExtensions.Debloat(string)"/>
    /// <br/>
    /// Присутствующая в наборе данных колонка <c>ИмЯ_СтоЛБЦа</c> будет разрешена при поиске колонки <c>Имя слолбца</c>,<c>ИмЯСтОлБцА___</c> и т.д.
    /// </remarks>
    public sealed class SqlDataReaderWrapper : IDataReader
    {
        /// <summary>
        /// Метаданные таблицы
        /// </summary>
        private readonly DataTable _schemaTable;

        /// <summary>
        /// Экземпляр DataReader
        /// </summary>
        private readonly SqlDataReader _reader;

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
        public int RecordsAffected => _reader.RecordsAffected;

        /// <summary>
        /// Количество колонок
        /// </summary>
        public int FieldCount => _reader.FieldCount;

        /// <summary>
        /// <inheritdoc cref="SqlDataReaderWrapper"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="hasRowCount">reader содержит информацию о количестве строк</param>
        /// <param name="skipEmptyRows">Пропускать строки, все значения которых пусты или состоят из пробельных символов</param>
        /// <exception cref="InvalidOperationException"></exception>
        public SqlDataReaderWrapper(System.Data.SqlClient.SqlDataReader reader)
        {
            // тот, ради кого всё делается
            _reader = reader;

            // конструируем кэши
            _columnCache = new Dictionary<int, int>();

            // инициализируем метаданные
            _schemaTable = _reader.GetSchemaTable();

            // имена колонок
            FieldNames = _schemaTable.Rows
                .Cast<DataRow>()
                .Select(e => e["ColumnName"]?.ToString())
                .ToList();
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

        public void Dispose()
        {
            _schemaTable.Dispose();
            _columnCache.Clear();
            _reader.Dispose();
        }

        public bool NextResult()
        {
            return _reader.NextResult();
        }

        public bool Read()
        {
            return _reader.Read();
        }

        public bool GetBoolean(int i)
        {
            return _reader.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return _reader.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return _reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return _reader.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return _reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public IDataReader GetData(int i)
        {
            return _reader.GetData(i);
        }

        public string GetDataTypeName(int i)
        {
            return _reader.GetDataTypeName(i);
        }

        public DateTime GetDateTime(int i)
        {
            return _reader.GetDateTime(i);
        }

        public decimal GetDecimal(int i)
        {
            return _reader.GetDecimal(i);
        }

        public double GetDouble(int i)
        {
            return _reader.GetDouble(i);
        }

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        public Type GetFieldType(int i)
        {
            return _reader.GetFieldType(i);
        }

        public float GetFloat(int i)
        {
            return _reader.GetFloat(i);
        }

        public Guid GetGuid(int i)
        {
            return _reader.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return _reader.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return _reader.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return _reader.GetInt64(i);
        }

        public string GetName(int i)
        {
            return _reader.GetName(i);
        }

        public int GetOrdinal(string name)
        {
            return _reader.GetOrdinal(name);
        }

        public string GetString(int i)
        {
            return _reader.GetString(i);
        }

        public object GetValue(int i)
        {
            return _reader.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            return _reader.GetValues(values);
        }

        public bool IsDBNull(int i)
        {
            return _reader.IsDBNull(i);
        }
    }
}

using System;
using System.Data;
using System.Reflection;

namespace UniversalPack.Common.DataLoader
{
    /// <summary>
    /// Информация о конвертере поля
    /// </summary>
    public readonly struct FieldConverterInfo
    {
        /// <summary>
        /// Инфо о свойстве
        /// </summary>
        public readonly PropertyInfo PropertyInfo { get; init; }

        /// <summary>
        /// Функция конвертера
        /// </summary>
        public readonly Func<IDataRecord, object> Converter { get; init; }
    }
}

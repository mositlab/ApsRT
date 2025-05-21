using System;
using System.Data;

namespace UniversalPack.Attributes
{
    /// <summary>
    /// Помечает связь поля модели с полем <see cref="IDataRecord"/> по имени
    /// </summary>
    /// <remarks>
    /// При использовании этого атрибута конвертер значения поля определяется автоматически
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DataRecordColumnAttribute : Attribute
    {
        /// <summary>
        /// Типы дополнительной конвертации
        /// </summary>
        public enum CustomConversionModes
        {
            /// <summary>
            /// Конвертация по умолчанию. Конвертер определяется автоматически в соответствии с типом поля.
            /// </summary>
            None = default,

            /// <summary>
            /// Конвертация строки в <see cref="bool"/> по содержащемуся в ней "да".
            /// </summary>
            StringToBooleanIsYes
        }

        /// <summary>
        /// Имя поля в <see cref="IDataRecord"/>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Тип дополнительной конвертации
        /// </summary>
        public CustomConversionModes CustomConversionMode { get; }

        /// <summary>
        /// Присваивать значение по умолчанию для типа, если не удалось сконвертировать
        /// </summary>
        public bool DefaultIfNull { get; }

        /// <summary>
        /// <inheritdoc cref="DataRecordColumnAttribute"/>
        /// </summary>
        /// <param name="name">Имя поля в <see cref="IDataRecord"/></param>
        /// <param name="mode">Кастомный конвертер</param>
        /// <param name="defaultIfFail">Присваивать значение по умолчанию для типа, если не удалось сконвертировать</param>
        public DataRecordColumnAttribute(string name, CustomConversionModes mode = CustomConversionModes.None, bool defaultIfFail = false)
        {
            Name = name;
            CustomConversionMode = mode;
            DefaultIfNull = defaultIfFail;
        }

        public override string ToString()
        {
            return $"{Name}, {CustomConversionMode}";
        }
    }
}

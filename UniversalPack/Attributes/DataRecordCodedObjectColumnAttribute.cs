using System;
using System.Data;

namespace UniversalPack.Attributes
{
    /// <summary>
    /// Помечает связь поля модели с полем <see cref="IDataRecord"/> по имени
    /// </summary>
    /// <remarks>
    /// Этот атрибут предназначен только для использования с полем <see cref="Common.CodedObject"/>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DataRecordCodedObjectColumnAttribute : Attribute
    {
        /// <summary>
        /// Имя поля с GUID в <see cref="IDataRecord"/>
        /// </summary>
        public string GuidName { get; }

        /// <summary>
        /// Имя поля с наименованием в <see cref="IDataRecord"/>
        /// </summary>
        public string ValueName { get; }

        /// <summary>
        /// Присваивать значение по умолчанию для типа, если не удалось сконвертировать
        /// </summary>
        public bool DefaultIfNull { get; }

        /// <summary>
        /// <inheritdoc cref="DataRecordCodedObjectColumnAttribute"/>
        /// </summary>
        /// <param name="guidName">Имя поля с GUID в <see cref="IDataRecord"/></param>
        /// <param name="valueName">Имя поля с наименованием в <see cref="IDataRecord"/></param>
        /// <param name="defaultIfFail">Присваивать значение по умолчанию для типа, если не удалось сконвертировать</param>
        /// <remarks>
        /// <inheritdoc cref="DataRecordCodedObjectColumnAttribute"/>
        /// </remarks>
        public DataRecordCodedObjectColumnAttribute(string guidName, string valueName, bool defaultIfFail = false)
        {
            GuidName = guidName;
            ValueName = valueName;
            DefaultIfNull = defaultIfFail;
        }

        public override string ToString()
        {
            return $"{GuidName}, {ValueName}";
        }
    }
}

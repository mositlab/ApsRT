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
    public sealed class DataRecordCodedObjectVersionedColumnAttribute : Attribute
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
        /// Имя поля с версией (партией ГП) в <see cref="IDataRecord"/>
        /// </summary>
        public string VersionName { get; }

        /// <summary>
        /// Присваивать значение по умолчанию для типа, если не удалось сконвертировать
        /// </summary>
        public bool DefaultIfNull { get; }

        public bool VersionIsInteger { get; }

        /// <summary>
        /// <inheritdoc cref="DataRecordCodedObjectVersionedColumnAttribute"/>
        /// </summary>
        /// <param name="guidName">Имя поля с GUID в <see cref="IDataRecord"/></param>
        /// <param name="valueName">Имя поля с наименованием в <see cref="IDataRecord"/></param>
        /// <param name="versionName">Имя поля с версией в <see cref="IDataRecord"/></param>
        /// <param name="defaultIfFail">Присваивать значение по умолчанию для типа, если не удалось сконвертировать</param>
        /// <remarks>
        /// <inheritdoc cref="DataRecordCodedObjectVersionedColumnAttribute"/>
        /// </remarks>
        public DataRecordCodedObjectVersionedColumnAttribute(string guidName, string valueName, string versionName = null, bool defaultIfFail = false, bool versionIsInt = false)
        {
            GuidName = guidName;
            ValueName = valueName;
            VersionName = versionName;
            DefaultIfNull = defaultIfFail;
            VersionIsInteger = versionIsInt;
        }

        public override string ToString()
        {
            return $"{GuidName}, {ValueName} ({VersionName})";
        }
    }
}

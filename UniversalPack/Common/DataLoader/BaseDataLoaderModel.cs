using UniversalPack.Attributes;
using UniversalPack.Common;
using UniversalPack.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using UniversalPack.Models;
using Microsoft.Extensions.Options;
using static UniversalPack.Extensions.EnumExtensions;
using Newtonsoft.Json.Linq;

namespace UniversalPack.Common.DataLoader
{
    /// <summary>
    /// Базовая модель загрузки данных из <see cref="IDataRecord"/>
    /// </summary>
    public abstract class BaseDataLoaderModel : IDataRecordLoadable
    {
        /// <summary>
        /// Конвертеры полей
        /// </summary>
        protected abstract IEnumerable<FieldConverterInfo> FieldConverters { get; }

        /// <summary>
        /// Определить конвертеры для полей модели
        /// </summary>
        /// <remarks>
        /// Предполагается разовое использование в статическом конструкторе дочерней модели.
        /// </remarks>
        /// <typeparam name="TDataLoaderModel">Тип модели загрузчика</typeparam>
        /// <returns></returns>
        protected static ICollection<FieldConverterInfo> DefineFieldConverters<TDataLoaderModel>()
            where TDataLoaderModel : BaseDataLoaderModel
        {
            // конвертеры для простых свойств из одного поля
            var simpleConverters = typeof(TDataLoaderModel)

                // получаем свойства
                .GetProperties()

                // у которых есть атрибут
                .Where(e => e.CustomAttributes.Any(f => f.AttributeType.IsAssignableTo<DataRecordColumnAttribute>()))

                // промеуточные данные: сопоставление поля и атрибута
                .Select(e => new
                {
                    PropertyInfo = e,
                    Attribute = e.CustomAttributes
                        .FirstOrDefault(f => f.AttributeType.IsAssignableTo<DataRecordColumnAttribute>())?
                        .ToAttributeInstance<DataRecordColumnAttribute>()
                })

                // конечные данные: связь поля и конвертера
                .Select(e => new FieldConverterInfo
                {
                    PropertyInfo = e.PropertyInfo,
                    Converter = DetermineConverter(e.PropertyInfo, e.Attribute)
                });

            // конвертеры для получения CodedObject
            var codedObjectConverters = typeof(TDataLoaderModel)

                // получаем свойства
                .GetProperties()

                // у которых есть атрибут
                .Where(e => e.CustomAttributes.Any(f => f.AttributeType.IsAssignableTo<DataRecordCodedObjectColumnAttribute>()))

                // промеуточные данные: сопоставление поля и атрибута
                .Select(e => new
                {
                    PropertyInfo = e,
                    Attribute = e.CustomAttributes
                        .FirstOrDefault(f => f.AttributeType.IsAssignableTo<DataRecordCodedObjectColumnAttribute>())?
                        .ToAttributeInstance<DataRecordCodedObjectColumnAttribute>()
                })

                // конечные данные: связь поля и конвертера
                .Select(e => new FieldConverterInfo
                {
                    PropertyInfo = e.PropertyInfo,
                    Converter = (r, opts) => ConvertCodedObject(r, opts, e.Attribute)
                });

            // конвертеры для получения CodedObjectVersioned
            var codedObjectVersionedConverters = typeof(TDataLoaderModel)

                // получаем свойства
                .GetProperties()

                // у которых есть атрибут
                .Where(e => e.CustomAttributes.Any(f => f.AttributeType.IsAssignableTo<DataRecordCodedObjectVersionedColumnAttribute>()))

                // промеуточные данные: сопоставление поля и атрибута
                .Select(e => new
                {
                    PropertyInfo = e,
                    Attribute = e.CustomAttributes
                        .FirstOrDefault(f => f.AttributeType.IsAssignableTo<DataRecordCodedObjectVersionedColumnAttribute>())?
                        .ToAttributeInstance<DataRecordCodedObjectVersionedColumnAttribute>()
                })

                // конечные данные: связь поля и конвертера
                .Select(e => new FieldConverterInfo
                {
                    PropertyInfo = e.PropertyInfo,
                    Converter = (r, opts) => ConvertCodedObjectVersioned(r, opts, e.Attribute)
                });

            // всё соединяем и отдаём
            return simpleConverters
                .Concat(codedObjectConverters)
                .Concat(codedObjectVersionedConverters)
                .ToList();
        }

        /// <summary>
        /// Сконвертировать <see cref="IDataRecord"/> в <see cref="CodedObject"/>
        /// </summary>
        /// <param name="record">Строка данных</param>
        /// <param name="attribute">Атрибут</param>
        /// <returns></returns>
        private static object ConvertCodedObject(IDataRecord record, DataLoaderOptions options, DataRecordCodedObjectColumnAttribute attribute)
        {
            var guidValue = (Guid?)ConvertGuid(record, options, attribute.GuidName, attribute.DefaultIfNull);

            if (!guidValue.HasValue)
            {
                if (attribute.IsDimensionUnit)
                    return (CodedObject<Guid, DimensionUnit>?)null;
                else
                    return (CodedObject<Guid, string>?)null;
            }

            if (attribute.IsDimensionUnit)
            {
                var nameValue = (record[attribute.ValueName]?.ToString() ?? string.Empty)
                    .ParseDimensionUnit();

                return new CodedObject<Guid, DimensionUnit>(guidValue.Value, nameValue);
            }

            else
            {
                var nameValue = record[attribute.ValueName].ConvertToOrDefault(string.Empty);

                return new CodedObject<Guid, string>(guidValue.Value, nameValue);
            }

        }

        /// <summary>
        /// Сконвертировать <see cref="IDataRecord"/> в <see cref="CodedObjectVersioned"/>
        /// </summary>
        /// <param name="record">Строка данных</param>
        /// <param name="attribute">Атрибут</param>
        /// <returns></returns>
        private static object ConvertCodedObjectVersioned(IDataRecord record, DataLoaderOptions options, DataRecordCodedObjectVersionedColumnAttribute attribute)
        {
            var guidValue = (Guid)ConvertGuid(record, options, attribute.GuidName, attribute.DefaultIfNull);
            var nameValue = record[attribute.ValueName].ConvertToOrDefault(string.Empty);
            var versionValue = record[attribute.VersionName].ConvertToOrDefault(string.Empty);

            return new CodedObjectVersioned<Guid, string, string>(new CodedObject<Guid, string>(guidValue, nameValue), versionValue);
        }
        /// <summary>
        /// Определить конвертер для поля
        /// </summary>
        /// <returns>Функция конвертера</returns>
        private static Func<IDataRecord, DataLoaderOptions, object> DetermineConverter(PropertyInfo propertyInfo, DataRecordColumnAttribute attribute)
        {
            switch (attribute.CustomConversionMode)
            {
                // автоматический выбор
                case DataRecordColumnAttribute.CustomConversionModes.None:
                    // отдельно определяем типы для парсинга
                    if (propertyInfo.PropertyType.IsAssignableFrom<DateTime>())
                    {
                        return (e, opts) => ConvertDateTime(e, attribute);
                    }
                    if (propertyInfo.PropertyType.IsAssignableFrom<DateOnly>())
                    {
                        return (e, opts) => ConvertDateOnly(e, attribute);
                    }
                    if (propertyInfo.PropertyType.IsAssignableFrom<double>())
                    {
                        return (e, opts) => ConvertDouble(e, attribute);
                    }
                    if (propertyInfo.PropertyType.IsAssignableFrom<int>())
                    {
                        return (e, opts) => ConvertInt(e, attribute);
                    }
                    if (propertyInfo.PropertyType.IsAssignableFrom<Guid>())
                    {
                        return (e, opts) => ConvertGuid(e, opts, attribute);
                    }

                    // иначе ищем конвертер
                    var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);

                    if (converter == null)
                    {
                        return (e, opts) => CommonConvertOneField(e, propertyInfo, attribute);
                    }
                    else
                    {
                        return (e, opts) => ConvertOneFieldWithConverter(e, converter, propertyInfo, attribute);
                    }

                // обработка кастомных правил конвертации
                case DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes:
                    return (e, opts) => ConvertIsYesField(e, attribute);
                case DataRecordColumnAttribute.CustomConversionModes.StringToTypeIndex:
                    return (e, opts) => ConvertTypeIndexField(e, attribute);
                case DataRecordColumnAttribute.CustomConversionModes.StringToPriority:
                    return (e, opts) => ConvertPriorityField(e, attribute);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Конвертация <see cref="Guid"/>
        /// </summary>
        /// <param name="record">Строка</param>
        /// <param name="name">Имя поля в наборе данных</param>
        /// <param name="defaultIfNull"></param>
        /// <returns></returns>
        private static object ConvertGuid(IDataRecord record, DataLoaderOptions options, string name, bool defaultIfNull)
        {
            var value = record[name];

            if (value is null || value == DBNull.Value)
            {
                return defaultIfNull
                    ? default(Guid?)
                    : null;
            }

            if (value.GetType().IsAssignableTo<Guid>())
            {
                return value.ProcessGuid(options.GuidArrrangeType, true);
            }

            var strValue = (value?.ToString() ?? string.Empty).RemoveWhitespaces();
            var isUnparsable = strValue.Length == 0;

            if (!isUnparsable && Guid.TryParse(strValue, out var parsed))
            {
                return parsed.ProcessGuid(options.GuidArrrangeType, true);
            }
            else if (defaultIfNull)
            {
                return default(Guid?);
            }
            else
            {
                //return (Guid?)null;
                throw new DataLoaderException($"Недопустимое значение GUID '{value}' для поля '{name}'", record, record.GetValues());
            }
        }

        /// <summary>
        /// Конвертация <see cref="Guid"/>
        /// </summary>
        /// <param name="record"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        /// <exception cref="DataLoaderException"></exception>
        private static object ConvertGuid(IDataRecord record, DataLoaderOptions options, DataRecordColumnAttribute attribute)
        {
            return ConvertGuid(record, options, attribute.Name, attribute.DefaultIfNull);
        }

        /// <summary>
        /// Конвертация <see cref="int"/>
        /// </summary>
        /// <param name="record"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        /// <exception cref="DataLoaderException"></exception>
        private static object ConvertInt(IDataRecord record, DataRecordColumnAttribute attribute)
        {
            var value = record[attribute.Name];

            if (value is null || value == DBNull.Value)
            {
                return attribute.DefaultIfNull
                    ? default(int?)
                    : null;
            }

            if (value.GetType().IsAssignableTo<int>())
            {
                return value;
            }

            var strValue = (value?.ToString() ?? string.Empty).RemoveWhitespaces();

            if (int.TryParse(strValue, out var parsed))
            {
                return parsed;
            }
            else if (attribute.DefaultIfNull)
            {
                return default(int?);
            }
            else
            {
                //return (int?)null;
                throw new DataLoaderException($"Недопустимое значение целого числа '{value}' для поля '{attribute.Name}'", record, record.GetValues());
            }
        }

        /// <summary>
        /// Конвертация <see cref="double"/>
        /// </summary>
        /// <param name="record"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        /// <exception cref="DataLoaderException"></exception>
        private static object ConvertDouble(IDataRecord record, DataRecordColumnAttribute attribute)
        {
            var value = record[attribute.Name];

            if (value is null || value == DBNull.Value)
            {
                return attribute.DefaultIfNull
                    ? default(double?)
                    : null;
            }

            if (value.GetType().IsAssignableTo<double>())
            {
                return value;
            }

            var strValue = (value?.ToString() ?? string.Empty).RemoveWhitespaces();

            if (strValue.TryParseDouble(out var parsed))
            {
                return parsed;
            }
            else if (attribute.DefaultIfNull)
            {
                return default(double?);
            }
            else
            {
                //return (double?)null;
                throw new DataLoaderException($"Недопустимое значение дробного числа '{value}' для поля '{attribute.Name}'", record, record.GetValues());
            }
        }

        /// <summary>
        /// Конвертация поля в <see cref="bool"/> по содержанию в строке единственного слова "да"
        /// </summary>
        /// <param name="record"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private static object ConvertIsYesField(IDataRecord record, DataRecordColumnAttribute attribute)
        {
            var value = record[attribute.Name];

            if (value is null || value == DBNull.Value)
            {
                return attribute.DefaultIfNull
                    ? default(bool)
                    : null;
            }

            if (value is bool booleanValue)
            {
                return booleanValue;
            }

            var strValue = value?.ToString() ?? value?.ToString() ?? string.Empty;

            return strValue.IsYes();
        }

        private static object ConvertTypeIndexField(IDataRecord record, DataRecordColumnAttribute attribute)
        {
            var value = record[attribute.Name];

            if (value is null || value == DBNull.Value)
            {
                return attribute.DefaultIfNull
                    ? 2
                    : null;
            }

            if (value is byte byteValue)
            {
                return byteValue;
            }

            var strValue = value?.ToString() ?? value?.ToString() ?? string.Empty;

            return (byte)(strValue switch
            {
                "Число" => 0,
                "Идентификатор" => 1,
                _ => 2
            });
        }

        private static object ConvertPriorityField(IDataRecord record, DataRecordColumnAttribute attribute)
        {
            var value = record[attribute.Name];

            if (value is null || value == DBNull.Value)
            {
                return attribute.DefaultIfNull
                    ? 2
                    : null;
            }

            if (value is int byteValue)
            {
                return byteValue;
            }

            var strValue = value?.ToString() ?? value?.ToString() ?? string.Empty;

            return (int)(strValue switch
            {
                "Высокий" => 0,
                "Средний" => 1,
                _ => 2
            });
        }

        /// <summary>
        /// Конвертация в <see cref="DateTime"/>
        /// </summary>
        /// <param name="record"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        /// <exception cref="DataLoaderException"></exception>
        private static object ConvertDateTime(IDataRecord record, DataRecordColumnAttribute attribute)
        {
            var value = record[attribute.Name];

            if (value is null || value == DBNull.Value)
            {
                return attribute.DefaultIfNull
                    ? default(DateTime?)
                    : null;
            }

            if (value.GetType().IsAssignableTo<DateTime>())
            {
                return value;
            }
            else
            {
                var strValue = value?.ToString().Trim() ?? string.Empty;

                if (strValue.TryParseDateTime(out var parsed))
                {
                    return parsed;
                }
                else if (attribute.DefaultIfNull)
                {
                    return default(DateTime?);
                }
                else
                {
                    //return default(DateTime?);
                    throw new DataLoaderException($"Недопустимое значение даты-времени '{value}' для поля '{attribute.Name}'", record, record.GetValues());
                }
            }
        }

        /// <summary>
        /// Конвертация в <see cref="DateOnly"/>
        /// </summary>
        /// <param name="record"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        /// <exception cref="DataLoaderException"></exception>
        private static object ConvertDateOnly(IDataRecord record, DataRecordColumnAttribute attribute)
        {
            var value = record[attribute.Name];

            if (value is null || value == DBNull.Value)
            {
                return attribute.DefaultIfNull
                    ? default(DateOnly?)
                    : null;
            }

            if (value.GetType().IsAssignableTo<DateOnly>())
            {
                return value;
            }
            else
            {
                var strValue = value?.ToString().Trim() ?? string.Empty;

                if (strValue.TryParseDateOnly(out var parsed))
                {
                    return parsed;
                }
                else if (attribute.DefaultIfNull)
                {
                    return default(DateOnly?);
                }
                else
                {
                    throw new DataLoaderException($"Недопустимое значение даты '{value}' для поля '{attribute.Name}'", record, record.GetValues());
                }
            }
        }

        /// <summary>
        /// Конвертирование значение с использованием определённого конвертера
        /// </summary>
        /// <param name="record"></param>
        /// <param name="converter"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        /// <exception cref="DataLoaderException"></exception>
        private static object ConvertOneFieldWithConverter(IDataRecord record, TypeConverter converter, PropertyInfo propertyInfo, DataRecordColumnAttribute attribute)
        {
            var value = record[attribute.Name];

            if (value is null || value == DBNull.Value)
            {
                return attribute.DefaultIfNull
                    ? propertyInfo.PropertyType.GetDefaultValue()
                    : null;
            }

            if (value.GetType().IsAssignableTo(propertyInfo.PropertyType))
            {
                return value;
            }
            else
            {
                if (converter.TryConvertFrom(value, out var converted, out var exception))
                {
                    return converted;
                }
                else if (attribute.DefaultIfNull)
                {
                    return propertyInfo.PropertyType.GetDefaultValue();
                }
                else
                {
                    throw new DataLoaderException($"Некорректное значение '{value}' для поля '{attribute.Name}' для типа {propertyInfo.PropertyType.Name}", innerException: exception, record, record.GetValues());
                }
            }
        }

        /// <summary>
        /// Конвертация одного поля через TryCast
        /// </summary>
        /// <param name="record">Объект строки данных</param>
        /// <param name="propertyInfo">Инво о свойстве модели</param>
        /// <param name="attribute">Атрибут свойства модели</param>
        /// <returns></returns>
        /// <exception cref="DataLoaderException"></exception>
        private static object CommonConvertOneField(IDataRecord record, PropertyInfo propertyInfo, DataRecordColumnAttribute attribute)
        {
            var value = record[attribute.Name];

            if (value is null || value == DBNull.Value)
            {
                return attribute.DefaultIfNull
                    ? propertyInfo.PropertyType.GetDefaultValue()
                    : null;
            }

            if (value.GetType().IsAssignableTo(propertyInfo.PropertyType))
            {
                return value;
            }
            else
            {
                if (value.TryCastTo(propertyInfo.PropertyType, out var converted, out var exception))
                {
                    return converted;
                }
                else if (attribute.DefaultIfNull)
                {
                    return propertyInfo.PropertyType.GetDefaultValue();
                }
                else
                {
                    throw new DataLoaderException($"Некорректное значение '{value}' для поля '{attribute.Name}' для типа {propertyInfo.PropertyType.Name}", innerException: exception, record, record.GetValues());
                }
            }
        }

        public virtual void LoadFromDataRecord(IDataRecord record, DataLoaderOptions options)
        {
            foreach (var info in FieldConverters)
            {
                try
                {
                    var value = info.Converter.Invoke(record, options);
                    info.PropertyInfo.SetValue(this, value);
                }
                catch (Exception ex)
                {
                    ;
                }
            }
        }
    }
}

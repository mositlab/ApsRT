using UniversalPack.Extensions;
using System;
using System.ComponentModel;
using System.Globalization;

namespace UniversalPack.Converters
{
    /// <summary>
    /// Конвертер для <see cref="DimensionUnit"/>
    /// </summary>
    public sealed class DimensionUnitConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return (value?.ToString() ?? string.Empty).ParseDimensionUnit();
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is DimensionUnit unit)
            {
                return unit.DimensionUnitToString();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => true;

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType.IsAssignableFrom<string>();
        }
    }
}

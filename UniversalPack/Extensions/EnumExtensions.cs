namespace UniversalPack.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Парсим единицу измерения из строки
        /// </summary>
        /// <param name="value">Значение</param>
        /// <returns></returns>
        public static DimensionUnit ParseDimensionUnit(this string value)
        {
            return value?.Debloat() switch
            {
                "мм" => DimensionUnit.mm,
                "см" => DimensionUnit.cm,
                "м" or "пометражу" => DimensionUnit.m,
                "км" => DimensionUnit.km,
                "м2" => DimensionUnit.m2,
                "г" => DimensionUnit.g,
                "кг" or "повесу" => DimensionUnit.kg,
                "шт" or "штук" => DimensionUnit.sht,
                _ => DimensionUnit.Unknown,
            };
        }

        /// <summary>
        /// Превращаем единицу измерения в строку
        /// </summary>
        /// <param name="value">Единица измерения</param>
        /// <param name="fullname">Вернуть полное наименование</param>
        /// <returns></returns>
        public static string DimensionUnitToString(this DimensionUnit value, bool fullname = false)
        {
            return value switch
            {
                DimensionUnit.mm => "мм",
                DimensionUnit.cm => "см",
                DimensionUnit.m => fullname ? "по метражу" : "м",
                DimensionUnit.km => "км",
                DimensionUnit.m2 => "м2",
                DimensionUnit.g => "г",
                DimensionUnit.sht => "шт",
                DimensionUnit.kg => fullname ? "по весу" : "кг",
                _ => string.Empty,
            };
        }
    }
}

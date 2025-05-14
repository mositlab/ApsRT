using InfimumMES.Core.Common.Planning;

namespace InfimumMES.Core.Extensions
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

        /// <summary>
        /// Превращаем строку в критерий переналадки
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RechangeCriteria ParseRechangeCriteria(this string value)
        {
            return value.Debloat() switch
            {
                "диаметр" => RechangeCriteria.Diam,
                "изоляция" => RechangeCriteria.Isolation,
                "количествожил" => RechangeCriteria.ZhilaCount,
                "краситель" => RechangeCriteria.Color,
                "марка" => RechangeCriteria.Mark,
                "напряжение" => RechangeCriteria.Voltage,
                "сечение" => RechangeCriteria.Razm1Sech,
                "технологическаяоперация" => RechangeCriteria.Operation,
                "типпродукции" => RechangeCriteria.TPZH,
                _ => RechangeCriteria.Unknown
            };
        }

        /// <summary>
        /// Превращаем критерий переналадки в строку
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RechangeCriteriaToString(this RechangeCriteria value)
        {
            return value switch
            {
                RechangeCriteria.Diam => "Диаметр",
                RechangeCriteria.Isolation => "Изоляция",
                RechangeCriteria.ZhilaCount => "Количество жил",
                RechangeCriteria.Color => "Краситель",
                RechangeCriteria.Mark => "Марка",
                RechangeCriteria.Voltage => "Напряжение",
                RechangeCriteria.Razm1Sech => "Сечение",
                RechangeCriteria.Operation => "Технологическая операция",
                RechangeCriteria.TPZH => "Тип продукции",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Парсим объект перехода переналадки
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RechangeObject ParseRechangeObject(this string value)
        {
            return value.Debloat() switch
            {
                null or "" => RechangeObject.Undefined,
                "отсутствует" => RechangeObject.None,
                "любоидругои" => RechangeObject.Any,
                "круглая" or "мк" or "ок" => RechangeObject.Round,
                "секторная" or "мс" => RechangeObject.Sector,
                _ => RechangeObject.OtherDefined
            };
        }

        /// <summary>
        /// Превращаем объект переналадки в строку
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RechangeObjectToString(this RechangeObject value)
        {
            return value switch
            {
                RechangeObject.None => "Отсутствует",
                RechangeObject.Any => "Любой другой",
                RechangeObject.Round => "Круглая",
                RechangeObject.Sector => "Секторная",
                RechangeObject.OtherDefined => "Опредено другое наименование",
                _ => string.Empty
            };
        }
    }
}

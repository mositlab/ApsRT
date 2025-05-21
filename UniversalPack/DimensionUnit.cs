using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Converters;

namespace UniversalPack
{
    /// <summary>
    /// Единицы измерения
    /// </summary>
    [TypeConverter(typeof(DimensionUnitConverter))]
    public enum DimensionUnit
    {
        /// <summary>
        /// Метры
        /// </summary>
        m,

        /// <summary>
        /// Килограммы
        /// </summary>
        kg,

        /// <summary>
        /// Миллиметры
        /// </summary>
        mm,

        /// <summary>
        /// Сантиметры
        /// </summary>
        cm,

        /// <summary>
        /// Километры
        /// </summary>
        km,

        /// <summary>
        /// Метры квадратные
        /// </summary>
        m2,

        /// <summary>
        /// Граммы
        /// </summary>
        g,

        /// <summary>
        /// Штук
        /// </summary>
        sht,

        /// <summary>
        /// Неизвестно
        /// </summary>
        Unknown
    }
}

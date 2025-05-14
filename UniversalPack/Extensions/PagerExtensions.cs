using InfimumMES.Core.Common;
using InfimumMES.Core.Models;
using System.Collections.Generic;

namespace UniversalPack.Extensions
{
    /// <summary>
    /// Расширения для пагинации
    /// </summary>
    internal static class PagerExtensions
    {
        /// <summary>
        /// Создаёт пейджер для перечисления типа <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="data">Коллекция объектов</param>
        /// <param name="query">Параметры пагинации</param>
        public static Pager<T> CreatePager<T>(this IEnumerable<T> data, PagedQueryModel query)
        {
            var pager = new Pager<T>(data, query);

            return pager;
        }
    }
}

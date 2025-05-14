using InfimumMES.Core.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static InfimumMES.Core.Models.BaseQueryModel;

namespace UniversalPack.Extensions
{
    /// <summary>
    /// Помощь в сортировке запросов
    /// </summary>
    internal static class SortingExtensions
    {
        /// <summary>
        /// Добавляем в строку ключевое слово направление сортировки
        /// </summary>
        /// <param name="direct"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string SetDirection(SortDirections direct, string query = null)
        {
            if (!string.IsNullOrEmpty(query))
            {
                return query
                    .Split(',')
                    .Select(e => $"{e.Trim()} {direct.ToString().ToUpper()}")
                    .Join(", ");
            }

            return string.Empty;
        }

        /// <summary>
        /// Сортируем набор моделей по параметрам из <see cref="BaseQueryModel"/>
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="source"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<TModel> OrderBy<TModel>(this IQueryable<TModel> source, BaseQueryModel query) =>
            source.OrderBy(SetDirection(query.SortDirectParsed ?? SortDirections.Asc, query.SortBy));

        /// <summary>
        /// Сортируем набор моделей по переданной строке
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="source"></param>
        /// <param name="orderByStrValues"></param>
        /// <returns></returns>
        public static IQueryable<TModel> OrderBy<TModel>(this IQueryable<TModel> source, string orderByStrValues)
        {
            if (string.IsNullOrEmpty(orderByStrValues))
            {
                return source;
            }

            var q = source.AsQueryable();
            var queryExpr = q.Expression;
            var methodAsc = "OrderBy";
            var methodDesc = "OrderByDescending";

            var orderByValues = orderByStrValues.Trim().Split(',').Select(x => x.Trim()).ToList();

            foreach (var orderPairCommand in orderByValues)
            {
                var command = orderPairCommand.ToUpper().EndsWith("DESC") ? methodDesc : methodAsc;

                //Get propertyname and remove optional ASC or DESC
                var propertyName = orderPairCommand.Split(' ')[0].Trim();

                var type = typeof(TModel);
                var parameter = Expression.Parameter(type, "p");

                PropertyInfo property;
                MemberExpression propertyAccess;

                if (propertyName.Contains('.'))
                {
                    // support to be sorted on child fields. 
                    var childProperties = propertyName.Split('.');

                    property = typeof(TModel).GetProperty(childProperties[0]);

                    if (property == null)
                    {
                        continue;
                    }

                    propertyAccess = Expression.MakeMemberAccess(parameter, property);

                    for (var i = 1; i < childProperties.Length; i++)
                    {
                        property = property.PropertyType.GetProperty(childProperties[i]);

                        if (property == null)
                        {
                            continue;
                        }

                        propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                    }
                }
                else
                {
                    property = type.GetProperty(propertyName);

                    if (property == null)
                    {
                        continue;
                    }

                    propertyAccess = Expression.MakeMemberAccess(parameter, property);
                }

                var orderByExpression = Expression.Lambda(propertyAccess, parameter);

                queryExpr = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType }, queryExpr, Expression.Quote(orderByExpression));

                methodAsc = "ThenBy";
                methodDesc = "ThenByDescending";
            }

            return q.Provider.CreateQuery<TModel>(queryExpr);
        }
    }
}

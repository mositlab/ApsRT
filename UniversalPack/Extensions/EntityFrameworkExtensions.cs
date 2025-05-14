using InfimumMES.Core.Attributes;
using InfimumMES.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace InfimumMES.Core.Extensions
{
    internal static class EntityFrameworkExtensions
    {
        /// <summary>
        /// Registers an entity <typeparamref name="TEntity"/> as part of the model.
        /// </summary>
        /// <typeparam name="TEntity">Type to be registered.</typeparam>
        /// <param name="model"></param>
        public static void AddEntityType<TEntity>(this IMutableModel model)
        {
            if (!model.GetEntityTypes().Any(e => e.ClrType == typeof(TEntity)))
            {
                model.AddEntityType(typeof(TEntity));
            }
        }

        /// <summary>
        /// Applies default field values to the model set up by <see cref="DefaultValueAttribute"/> and <see cref="DefaultValueSqlAttribute"/>
        /// </summary>
        /// <param name="builder"></param>
        public static void ApplyDefaultValueAttributes(this ModelBuilder builder)
        {
            var entityTypes = builder.Model.GetEntityTypes().ToList();

            if (!entityTypes.Any())
            {
                return;
            }

            foreach (var entityType in entityTypes)
            {
                foreach (var property in entityType.GetProperties())
                {
                    var defaultValue = property.PropertyInfo?.GetCustomAttribute<DefaultValueAttribute>();

                    if (defaultValue != null && property.ValueGenerated == ValueGenerated.Never)
                    {
                        property.SetDefaultValue(defaultValue.Value);
                    }

                    var defaultValueSql = property.PropertyInfo?.GetCustomAttribute<DefaultValueSqlAttribute>();

                    if (defaultValueSql != null && property.ValueGenerated == ValueGenerated.Never)
                    {
                        property.SetDefaultValueSql(defaultValueSql.Expression);
                    }
                }
            }

            entityTypes.Clear();
        }

        /// <summary>
        /// Applies default collation for string field
        /// </summary>
        /// <param name="builder"></param>
        public static void ApplyUnicode(this ModelBuilder builder)
        {
            var properties = builder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetProperties())
                .Where(e => e.PropertyInfo != null && e.PropertyInfo.PropertyType.IsAssignableTo<string>())
                .ToList();

            if (!properties.Any())
            {
                return;
            }

            foreach (var property in properties)
            {
                property.SetIsUnicode(true);
            }

            properties.Clear();
        }

        /// <summary>
        /// Define project-provided EF functions for Microsoft SQL Server database
        /// </summary>
        /// <param name="builder"></param>
        public static ModelBuilder DefineSqlDbFunctions(this ModelBuilder builder)
        {
            return builder;
        }

        /// <summary>
        /// Define project-provided EF functions for SQLite database
        /// </summary>
        /// <param name="builder"></param>
        public static ModelBuilder DefineSqliteDbFunctions(this ModelBuilder builder)
        {
            builder
                .HasDbFunction(typeof(SqliteDbFunctions).GetMethod(nameof(SqliteDbFunctions.Random)))
                .HasTranslation(e => new SqlFunctionExpression("RANDOM", e, false, e.Select(b => false), typeof(double), null));

            return builder;
        }

        /// <summary>
        /// Create SQLite database context of type <typeparamref name="TContext"/> from an existing connection.
        /// </summary>
        /// <typeparam name="TContext">Target database context type</typeparam>
        /// <param name="connection">Existing connextion</param>
        /// <returns></returns>
        public static TContext ToSqliteDbContext<TContext>(this DbConnection connection)
            where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>()
                 .UseSqlite(connection, e => e.CommandTimeout(1800));

            return typeof(TContext).Instantiate<TContext>(builder.Options);
        }
    }
}

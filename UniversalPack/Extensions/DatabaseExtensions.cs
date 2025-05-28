using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace UniversalPack.Extensions
{
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Выполняет SQL запрос, возвращает все строки в <see cref="DataTable"/>
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="sql">SQL</param>
        /// <returns></returns>
        public static DataTable ExecuteSqlDataTable(this IDbConnection connection, string sql)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;

                connection.SafeOpen();

                var table = new DataTable();

                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    table.Load(reader);
                }

                connection.SafeClose();

                return table;
            }
        }

        /// <summary>
        /// Выполняет SQL запрос, возвращает первую строку в массиве объектов
        /// </summary>
        /// <param name="connection">Соединение</param>
        /// <param name="sql">SQL</param>
        /// <returns></returns>
        public static object[] ExecuteSqlFirstRow(this IDbConnection connection, string sql)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;

                connection.SafeOpen();

                var row = Array.Empty<object>();

                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (reader.Read())
                    {
                        row = new object[reader.FieldCount];
                        reader.GetValues(row);
                    }
                }

                connection.SafeClose();

                return row;
            }
        }

        /// <summary>
        /// Выполняет SQL запрос, возвращает объект типа <typeparamref name="T"/> из первой строки
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="facade">Фасад БД</param>
        /// <param name="sql">SQL</param>
        /// <param name="value">Полученное значение</param>
        /// <param name="args">Параметры запроса</param>
        /// <returns>true если удачно</returns>
        public static bool TryExecuteSqlRaw<T>(this DatabaseFacade facade, string sql, out T value, params IDbDataParameter[] args)
        {
            value = facade.FromSqlRaw<T>(sql, args).FirstOrDefault();

            return value != null;
        }

        /// <summary>
        /// Выполняет SQL запрос, возвращает объекты типа <typeparamref name="T1"/>, <typeparamref name="T2"/> из первой строки
        /// </summary>
        /// <typeparam name="T1">Тип объекта 1</typeparam>
        /// <typeparam name="T2">Тип объекта 1</typeparam>
        /// <param name="facade">Фасад БД</param>
        /// <param name="sql">SQL</param>
        /// <param name="value1">Полученное значение из первого столбца</param>
        /// <param name="value2">Полученное значение из второго столбца</param>
        /// <param name="args">Параметры запроса</param>
        /// <returns>true если удачно</returns>
        public static bool TryExecuteSqlRaw<T1, T2>(this DatabaseFacade facade, string sql, out T1 value1, out T2 value2, params IDbDataParameter[] args)
        {
            var result = facade.FromSqlRaw<T1, T2>(sql, args).FirstOrDefault();

            try
            {
                value1 = result.Item1;
                value2 = result.Item2;

                return true;
            }
            catch
            {

                value1 = default;
                value2 = default;

                return false;
            }
        }

        /// <summary>
        /// Получить колонку типа <typeparamref name="T"/> по запросу
        /// </summary>
        /// <typeparam name="T">Тип колонки</typeparam>
        /// <param name="facade">Фасад БД</param>
        /// <param name="sql">Запрос</param>
        /// <param name="args">Параметры запроса</param>
        /// <returns></returns>
        public static IEnumerable<T> FromSqlRaw<T>(this DatabaseFacade facade, string sql, params IDbDataParameter[] args)
        {
            var conn = facade.GetDbConnection();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;

                if (args != null && args.Any())
                {
                    cmd.Parameters.AddRange(args);
                }

                cmd.Connection.SafeOpen();

                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        yield return (T)reader.GetValue(0);
                    }
                }
            }
        }

        /// <summary>
        /// Получить колонки типа <typeparamref name="T1"/>, <typeparamref name="T2"/> по запросу
        /// </summary>
        /// <typeparam name="T1">Тип колонки 1</typeparam>
        /// <typeparam name="T2">Тип колонки 2</typeparam>
        /// <param name="facade">Фасад БД</param>
        /// <param name="sql">Запрос</param>
        /// <param name="args">Параметры запроса</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<T1, T2>> FromSqlRaw<T1, T2>(this DatabaseFacade facade, string sql, params IDbDataParameter[] args)
        {
            var conn = facade.GetDbConnection();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                if (args != null && args.Any())
                {
                    cmd.Parameters.AddRange(args);
                }

                cmd.Connection.SafeOpen();

                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        yield return new Tuple<T1, T2>((T1)reader.GetValue(0), (T2)reader.GetValue(1));
                    }
                }
            }
        }

        /// <summary>
        /// Получить колонки типа <typeparamref name="T1"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/> по запросу
        /// </summary>
        /// <typeparam name="T1">Тип колонки 1</typeparam>
        /// <typeparam name="T2">Тип колонки 2</typeparam>
        /// <typeparam name="T3">Тип колонки 3</typeparam>
        /// <param name="facade">Фасад БД</param>
        /// <param name="sql">Запрос</param>
        /// <param name="args">Параметры запроса</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<T1, T2, T3>> FromSqlRaw<T1, T2, T3>(this DatabaseFacade facade, string sql, params IDbDataParameter[] args)
        {
            var conn = facade.GetDbConnection();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                if (args != null && args.Any())
                {
                    cmd.Parameters.AddRange(args);
                }

                cmd.Connection.Open();

                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        yield return new Tuple<T1, T2, T3>((T1)reader.GetValue(0), (T2)reader.GetValue(1), (T3)reader.GetValue(2));
                    }
                }
            }
        }

        public static void SafeOpen(this IDbConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
        }

        public static void SafeClose(this IDbConnection conn)
        {
            if (conn.State != ConnectionState.Closed)
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Добавить множество параметров в коллекцию
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="values"></param>
        public static void AddRange(this DbParameterCollection collection, IEnumerable<DbParameter> values)
        {
            foreach (var item in values)
            {
                collection.Add(item);
            }
        }
    }
}

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace UniversalPack.Extensions
{
    public static class ReflectionExtentions
    {
        /// <summary>
        /// Создаёт экземпляр объекта типа <paramref name="type"/>, вызывая конструктор с параметрами <paramref name="args"/>
        /// </summary>
        /// <param name="type">Тип объекта</param>
        /// <param name="args">Параметры конструктора</param>
        /// <returns></returns>
        public static object Instantiate(this Type type, params object[] args)
        {
            var types = args
                .Select(e => e.GetType())
                .ToArray();
            var constructor = type.GetConstructor(types);

            return constructor.Invoke(args);
        }

        /// <summary>
        /// Создаёт экземпляр объекта типа <paramref name="type"/>, вызывая конструктор с параметрами <paramref name="args"/>
        /// </summary>
        /// <param name="type">Тип объекта</param>
        /// <param name="args">Параметры конструктора</param>
        /// <returns></returns>
        public static T Instantiate<T>(this Type type, params object[] args) where T : class
        {
            return type.Instantiate(args) as T;
        }

        /// <summary>
        /// Определяет, является ли метод асинхронным
        /// </summary>
        /// <param name="methodInfo">Метод</param>
        /// <returns></returns>
        public static bool IsAsync(this MethodInfo methodInfo)
        {
            return typeof(Task).IsAssignableFrom(methodInfo.ReturnType) ||
                  methodInfo.ReturnType.IsGenericType && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
        }

        ///<inheritdoc cref="Type.IsAssignableTo(Type?)"/>
        public static bool IsAssignableTo<T>(this Type type) => type.IsAssignableTo(typeof(T));

        ///<inheritdoc cref="Type.IsAssignableFrom(Type?)"/>
        public static bool IsAssignableFrom<T>(this Type type) => type != null && type.IsAssignableFrom(typeof(T));


        /// <summary>
        /// Создаёт атрибут из данных атрибута
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="customAttributeData"></param>
        /// <returns></returns>
        public static TAttribute ToAttributeInstance<TAttribute>(this CustomAttributeData customAttributeData) where TAttribute : Attribute
        {
            var args = customAttributeData.ConstructorArguments
                .Select(e => e.Value)
                .ToArray();

            var instance = customAttributeData.Constructor.Invoke(args);

            if (instance is TAttribute targetAttribute)
            {
                foreach (var arg in customAttributeData.NamedArguments)
                {
                    typeof(TAttribute)
                        .GetProperty(arg.MemberName)?
                        .SetValue(targetAttribute, arg.TypedValue.Value);
                }

                return targetAttribute;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Получение значения по умолчанию для типа <paramref name="type"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetDefaultValue(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}

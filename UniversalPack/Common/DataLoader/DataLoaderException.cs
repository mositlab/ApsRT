using UniversalPack.Extensions;
using System;

namespace UniversalPack.Common.DataLoader
{
    /// <summary>
    /// Исключение загрузчика данных
    /// </summary>
    public class DataLoaderException : InvalidOperationException
    {
        /// <summary>
        /// <inheritdoc cref="DataLoaderException"/>
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="data">Дополнительные данные</param>
        public DataLoaderException(string message, params object[] data) : base(message)
        {
            for (var i = 0; i < data.Length; i++)
            {
                Data[i] = data[i].ToJToken();
            }
        }

        /// <summary>
        /// <inheritdoc cref="DataLoaderException"/>
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="data">Дополнительные данные</param>
        /// <param name="innerException">Вложенное исключение</param>
        public DataLoaderException(string message, Exception innerException, params object[] data) : base(message, innerException)
        {
            for (var i = 0; i < data.Length; i++)
            {
                Data[i] = data[i].ToJToken();
            }
        }
    }
}

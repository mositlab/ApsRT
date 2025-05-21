using UniversalPack.Models.DataLoader;
using System;

namespace UniversalPack.Common.DataLoader
{
    /// <summary>
    /// Данные события ошибки
    /// </summary>
    /// <typeparam name="TLoaderModel"></typeparam>
    public class DataLoaderErrorEventArgs<TLoaderModel> where TLoaderModel : BaseDataLoaderModel
    {
        /// <summary>
        /// Сколько загружено
        /// </summary>
        public int ModelsProcessed { get; init; }

        /// <summary>
        /// Сколько всего
        /// </summary>
        public int ModelsTotal { get; init; }

        /// <summary>
        /// Прогресс загрузки
        /// </summary>
        public double Progress
        {
            get
            {
                if (ModelsTotal <= 0)
                {
                    return 0;
                }

                return (double)ModelsTotal / ModelsProcessed;
            }
        }

        /// <summary>
        /// Исключение
        /// </summary>
        public Exception Exception { get; init; }

        /// <summary>
        /// Загруженная модель
        /// </summary>
        public TLoaderModel Model { get; init; }
    }
}

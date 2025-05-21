using UniversalPack.Extensions;
using System;
using System.Linq;

namespace UniversalPack.Common.DataLoader
{
    /// <summary>
    /// Данные события загрузки
    /// </summary>
    public class DataLoaderProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Завершена ли загрузка источника
        /// </summary>
        private bool[] _isSourceProcessed;
        private int _sourcesTotal;

        /// <summary>
        /// Сколько всего было загружено из каждого источника
        /// </summary>
        public int[] ModelsProcessed { get; internal set; }

        /// <summary>
        /// Всего загружено из всех источников
        /// </summary>
        public int ModelsProcessedSum => ModelsProcessed.Sum();

        /// <summary>
        /// Сколько записей всего в каждом источнике
        /// </summary>
        public int[] ModelsTotal { get; internal set; }

        /// <summary>
        /// Всего записей во всех источниках
        /// </summary>
        public int ModelsTotalSum => ModelsTotal.Sum();

        /// <summary>
        /// Всего источников
        /// </summary>
        public int SourcesTotal
        {
            get => _sourcesTotal;
            internal set
            {
                var target = value > 0 ? value : 1;

                if (target != _sourcesTotal)
                {
                    _sourcesTotal = target;
                    ModelsProcessed = new int[_sourcesTotal];
                    ModelsTotal = new int[_sourcesTotal];
                    _isSourceProcessed = new bool[_sourcesTotal];
                }
            }
        }

        /// <summary>
        /// Загружено источников
        /// </summary>
        public int SourcesProcessed => _isSourceProcessed.Count(e => e);

        /// <summary>
        /// Прогресс загрузки источников
        /// </summary>
        public double SourcesProgress
        {
            get
            {
                if (SourcesTotal <= 0)
                {
                    return 0;
                }
                else
                {
                    return (double)SourcesProcessed / SourcesTotal;
                }
            }
        }

        /// <summary>
        /// Полный прогресс загрузки
        /// </summary>
        public double Progress
        {
            get
            {
                var progress = 0.0;

                for (var i = 0; i < _sourcesTotal; i++)
                {
                    if (_isSourceProcessed[i])
                    {
                        progress += 1;
                    }
                    else
                    {
                        progress += GetOneSourceProgress(i);
                    }
                }

                return progress / _sourcesTotal;
            }
        }

        public DataLoaderProgressEventArgs()
        {
            SourcesTotal = 1;
        }

        /// <summary>
        /// Прогресс загрузки одного источника
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private double GetOneSourceProgress(int index)
        {
            if (index >= _sourcesTotal)
            {
                index = _sourcesTotal - 1;
            }
            if (index < 0)
            {
                index = 0;
            }

            if (ModelsTotal[index] <= 0)
            {
                return 0;
            }
            else
            {
                return (double)ModelsProcessed[index] / ModelsTotal[index];
            }
        }

        /// <summary>
        /// Пометка источника по индексу как загруженного
        /// </summary>
        /// <param name="index"></param>
        internal void MarkSourceProcessed(int index)
        {
            _isSourceProcessed[index] = true;
            ModelsTotal[index] = ModelsProcessed[index];
        }
    }
}

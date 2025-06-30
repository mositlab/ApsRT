using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Partias.TaraSearchEngine
{
    public interface IAbleForTaraCalcInterface
    {
        /// <summary>
        /// Первая операция
        /// </summary>
        public Stages.Stage FirstOperLink { get; }

        /// <summary>
        /// ИД моделирования
        /// </summary>
        public int ModId { get; }

        /// <summary>
        /// Все операции объединения
        /// </summary>
        public List<Stages.Stage> AllOpers { get; }

        public double Sum { get; }

        public int ZadelCode { get; }
    }
}

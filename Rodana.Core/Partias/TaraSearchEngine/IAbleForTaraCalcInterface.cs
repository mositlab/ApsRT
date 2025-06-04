using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Partias.TaraSearchEngine
{
    internal interface IAbleForTaraCalcInterface
    {
        /// <summary>
        /// Первая операция
        /// </summary>
        internal Stages.Stage FirstOperLink { get; }

        /// <summary>
        /// ИД моделирования
        /// </summary>
        internal int ModId { get; }

        /// <summary>
        /// Все операции объединения
        /// </summary>
        internal List<Stages.Stage> AllOpers { get; }

        internal double Sum { get; }

        internal int ZadelCode { get; }
    }
}

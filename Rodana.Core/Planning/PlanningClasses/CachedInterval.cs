using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Planning.PlanningClasses
{
    /// <summary>
    /// Универсальный интервал времени, с хранением пойманных нерабочих интервалов на оборудовании. Юзается для кэширования и хранения инфы.
    /// </summary>
    internal class CachedInterval
    {
        internal static CachedInterval Empty => new CachedInterval(new TimeSpan(), null, -1/*, -1*/);

        public CachedInterval(TimeSpan duration, IEnumerable<int> catchedIntervals, int nextInterval/*, int prevInterval*/)
        {
            Duration = duration;
            CatchedIntervals = catchedIntervals?.ToArray();
            NextInterval = nextInterval;
            //LastInterval = prevInterval;
        }

        /// <summary>
        /// Длительность, уже с учетом интервалов
        /// </summary>
        internal TimeSpan Duration { get; }

        /// <summary>
        /// Индексы попавших нерабочих интервалов
        /// </summary>
        internal int[] CatchedIntervals { get; }

        /// <summary>
        /// Индекс следующего нерабочего интервала (может быть -1, значит на этом РЦ в принципе нихрена нет).
        /// </summary>
        internal int NextInterval { get; }

        /// <summary>
        /// Предыдущий пройденный тут интервал. Редко юзается, и равен -1, если NextInverval равен -1.
        /// </summary>
        //internal int LastInterval { get; }
    }
}

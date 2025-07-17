using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Planning
{
    /// <summary>
    /// Универсальный интервал времени, с хранением пойманных нерабочих интервалов на оборудовании. Юзается для кэширования и хранения инфы.
    /// </summary>
    public class CachedInterval
    {
        public static CachedInterval Empty => new CachedInterval(new TimeSpan(), null, -1/*, -1*/);

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
        public TimeSpan Duration { get; }

        /// <summary>
        /// Индексы попавших нерабочих интервалов
        /// </summary>
        public int[] CatchedIntervals { get; }

        /// <summary>
        /// Индекс следующего нерабочего интервала (может быть -1, значит на этом РЦ в принципе нихрена нет).
        /// </summary>
        public int NextInterval { get; }

        /// <summary>
        /// Предыдущий пройденный тут интервал. Редко юзается, и равен -1, если NextInverval равен -1.
        /// </summary>
        //public int LastInterval { get; }
    }
}

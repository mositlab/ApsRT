using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Planning.ParallelPlanning
{
    internal class ParallelPlanningParamClass
    {
        public override string ToString()
        {
            return $"{Delta}, не повышать тару: {DoNotIncreaseTara}";
        }

        public ParallelPlanningParamClass(double delta, bool doNotIncreaseTara)
        {
            Delta = delta;
            DoNotIncreaseTara = doNotIncreaseTara;
        }

        /// <summary>
        /// Максимальное отклонение по ВРЕМЕНИ
        /// </summary>
        internal double Delta { get; init; }

        /// <summary>
        /// Нельзя повышать тару у этого типа пар. планирования
        /// </summary>
        internal bool DoNotIncreaseTara { get; init; }
    }
}

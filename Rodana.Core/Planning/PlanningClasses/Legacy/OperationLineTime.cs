using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Planning.PlanningClasses.Legacy
{
    /// <summary>
    /// Класс, который привязывается к моделированию во время создания плана, и он нужен будет, чтобы находить временные отрезки, когда занята тара.
    /// Это по факту "временной отрезок", он же временной отрезок тары, пока юзается только для этого.
    /// </summary>
    internal class OperationLineTime
    {
        internal DateTime start;
        internal int uncheckedKids;
        internal DateTime end;
        internal string comment;

        public override string ToString()
        {
            return $"{start} -> {end} [{uncheckedKids}] = '{comment}'";
        }

        internal OperationLineTime(DateTime time, int kids, string comment)
        {
            start = time;
            uncheckedKids = kids;
            end = DateTime.MinValue;
            this.comment = comment;
        }
    }
}

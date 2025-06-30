using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Planning.PlanningClasses.PoolClasses
{
    public class DatesPoolClass
    {
        public DateTime MinValue;
        public int Flag;
        public int LastBlockIndex, LastID;

        public DatesPoolClass(DatesPoolClass fromBase) : this(fromBase.MinValue, fromBase.Flag, fromBase.LastBlockIndex, fromBase.LastID) { }

        public DatesPoolClass(DateTime minValue, int flag, int lastBlockIndex, int lastID)
        {
            MinValue = minValue;
            Flag = flag;
            LastBlockIndex = lastBlockIndex;
            LastID = lastID;
        }
        // информация Last пригодится при формировании списка для алгоритма G, поэтому не трогать
        // а ID понадобится для приставки "назначен операцией Х"
    }
}

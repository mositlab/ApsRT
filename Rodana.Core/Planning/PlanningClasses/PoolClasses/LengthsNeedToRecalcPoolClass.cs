using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Planning.PlanningClasses.PoolClasses
{
    public class LengthsNeedToRecalcPoolClass
    {
        public bool WillRecalc;
        public int Flag;

        public LengthsNeedToRecalcPoolClass(bool willRecalc, int flag)
        {
            WillRecalc = willRecalc;
            Flag = flag;
        }
    }
}

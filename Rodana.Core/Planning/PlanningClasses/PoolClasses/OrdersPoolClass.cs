using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Planning.PlanningClasses.PoolClasses
{
    public class OrdersPoolClass
    {
        public DateTime MinValue;
        public int Flag;
        public double Time;
        public double TimeWithK;

        public OrdersPoolClass(DateTime minValue, int flag, double time, double timeWithK)
        {
            MinValue = minValue;
            Flag = flag;
            Time = time;
            TimeWithK = timeWithK;
        }
    }
}

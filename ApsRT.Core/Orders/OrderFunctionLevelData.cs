using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Orders
{
    public class OrderFunctionLevelData
    {
        public OrderFunctionLevelData(double upto, double k, double a)
        {
            UpTo = upto;
            K = k;
            A = a;
        }

        public double UpTo { get; set; } // Обозначение с какого дня по какой

        //KX ^ A
        public double K { get; set; }
        public double A { get; set; }
    }
}

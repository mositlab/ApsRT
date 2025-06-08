using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Orders
{
    public struct OrderFunctionLevelData
    {
        public OrderFunctionLevelData(double from, double to, double k, double a)
        {
            From = from;
            To = to;
            K = k;
            A = a;
        }

        public double From { get; set; }
        public double To { get; set; } // Обозначение с какого дня по какой

        //KX ^ A
        public double K { get; set; }
        public double A { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Orders
{
    public struct OrderFunctionData
    {
        public OrderFunctionLevelData Level1 { get; set; }
        public OrderFunctionLevelData Level2 { get; set; }
        public OrderFunctionLevelData Level3 { get; set; }
    }
}

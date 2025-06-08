using Rodana.Core.NaladLengths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Planning.PlanningClasses.PoolClasses
{
    public class GottenNaladLengthPoolClass
    {
        public int Flag;
        public GottenNaladLengthStruct data;

        public GottenNaladLengthPoolClass(int flag, GottenNaladLengthStruct data)
        {
            Flag = flag;
            this.data = data;
        }
    }
}

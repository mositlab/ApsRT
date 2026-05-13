using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Stages.MasterData
{
    internal class TypicalDurationVariant
    {
        internal HashSet<int> UsedOpersNumbersOnLoad;

        internal double Duraion;

        public TypicalDurationVariant()
        {
            UsedOpersNumbersOnLoad = new HashSet<int>();
            Duraion = 1;
        }
    }
}

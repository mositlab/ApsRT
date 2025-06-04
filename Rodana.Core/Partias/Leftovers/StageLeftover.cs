using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.Leftovers
{
    public class StageLeftover
    {
        public StageLeftover(Uco nomenklature, double calculatedCount)
        {
            Nomenklature = nomenklature;
            CalculatedCount = calculatedCount;
        }

        public Uco Nomenklature { get; set; }
        public double CalculatedCount { get; set; }
    }
}

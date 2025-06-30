using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using LeftoverUco = (UniversalPack.Common.CodedObject<System.Guid, string> Nomenklature, UniversalPack.Common.CodedObject<System.Guid, string> Characteristics);

namespace ApsRT.Core.Partias.Leftovers
{
    public class StageLeftover
    {
        public StageLeftover(LeftoverUco leftoverData, double calculatedCount)
        {
            LeftoverData = leftoverData;
            CalculatedCount = calculatedCount;
        }

        public LeftoverUco LeftoverData { get; set; }
        public double CalculatedCount { get; set; }
    }
}

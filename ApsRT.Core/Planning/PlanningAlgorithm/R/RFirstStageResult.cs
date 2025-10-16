using ApsRT.Core.PlanSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;

namespace ApsRT.Core.Planning.PlanningAlgorithm.R
{
    class RFirstStageResult
    {
        public PlanSimulatorBlock Block;
        public Uco RC, Tara;
        public double Max;
        public bool ItsBadResult;
        public double Cost;

        public RFirstStageResult(PlanSimulatorBlock block, Uco rC, Uco tara, double max, bool itsBadResult, float cost)
        {
            Block = block;
            RC = rC;
            Tara = tara;
            Max = max;
            ItsBadResult = itsBadResult;
            Cost = cost;
        }
    }
}

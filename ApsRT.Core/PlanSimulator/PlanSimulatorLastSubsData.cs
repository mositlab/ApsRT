using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;

namespace ApsRT.Core.PlanSimulator
{
    public class PlanSimulatorLastSubsData
    {
        internal Uco LastSubdivision;
        internal int Flag;
        internal PlanSimulatorMod LastMod;

        public PlanSimulatorLastSubsData(Uco sub, int flag)
        {
            LastSubdivision = sub;
            Flag = flag;
        }
    }
}

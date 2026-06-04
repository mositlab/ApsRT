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
    class RSearcher
    {
        public RTempResult minResult = new RTempResult(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, int.MaxValue, false, int.MaxValue, float.MaxValue, int.MaxValue, double.MinValue, int.MaxValue, false, TimeSpan.Zero, false, null, TimeSpan.Zero);
        public Uco ChoosenCenter = Uco.EmptyObject;
        public Uco ChoosenTara = Uco.EmptyObject;
        public double MaxNamotka = 0;
        public bool ThisResultIsBad = true;
        public PlanSimulatorMod ChoosenMod = null;
        public bool WillBeParr = false;
        public PlanSimulatorBlock parrBlock = null;
    }
}

using ApsRT.Core.NaladLengths;
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
    public class PlanSimulatorNaladLengthData
    {
        public int Flag;
        public GottenNaladLengthStruct Data;
        public Uco LastRC;
        public bool LastLaunchWasFirst;
        public bool HadBrokenFreeNaladLength;
        public bool HadBrokenFixedNaladLength;

        public PlanSimulatorNaladLengthData(int flag, GottenNaladLengthStruct data)
        {
            Flag = flag;
            Data = data;
        }
    }
}

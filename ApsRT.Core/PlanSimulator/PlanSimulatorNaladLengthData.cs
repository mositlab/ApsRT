using ApsRT.Core.NaladLengths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.PlanSimulator
{
    public class PlanSimulatorNaladLengthData
    {
        public int Flag;
        public GottenNaladLengthStruct Data;

        public PlanSimulatorNaladLengthData(int flag, GottenNaladLengthStruct data)
        {
            Flag = flag;
            Data = data;
        }
    }
}

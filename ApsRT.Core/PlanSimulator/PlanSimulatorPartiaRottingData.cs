using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.PlanSimulator
{
    public class PlanSimulatorPartiaRottingData
    {
        public PlanSimulatorMod WorstMod;
        public int Flag;

        public PlanSimulatorPartiaRottingData(PlanSimulatorMod worstMod, int flag)
        {
            WorstMod = worstMod;
            Flag = flag;
        }

        public override string ToString()
        {
            return String.Format("#{0:X}", GetHashCode());
        }
    }
}

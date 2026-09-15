using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.PlanSimulator
{
    public class PlanSimulatorOneOtdTaraRstrData
    {
        // Ещё какие-нибудь данные можно

        //public int[] CurrentModFlag;

        public List<PlanSimulatorMod> AllModsInGroup;

        public PlanSimulatorOneOtdTaraRstrData(int PoolSize)
        {
            AllModsInGroup = new();
            //CurrentModFlag = new int[PoolSize];
        }
    }
}

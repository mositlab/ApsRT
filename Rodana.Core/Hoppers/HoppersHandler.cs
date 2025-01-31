using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace Rodana.Core.Hoppers
{
    public class HoppersHandler<RCCode, RCLabel, OrderCode, OrderLabel, OrderSubType, CommonCode, CommonLabel>
    {
        //public  { get; init; }

        public Dictionary<string, (HopperAttribute Data, IHopperInterface Interface)> Hoppers { get; init; }
        public HoppersHandler()
        {
            Hoppers = new();
            FindAllHoppers();
        }
    void FindAllHoppers()
    {
    }

        public void LoadAllHoppers(Session<RCCode, RCLabel, OrderCode, OrderLabel, OrderSubType, CommonCode, CommonLabel> session)
        {
            Dictionary<string, string[]> Paths = session.SettingsController.CurrentSettings.HoppersPaths;
        }
    }
}

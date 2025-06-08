using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Planning.PlanningAlgorithm
{
    public enum PlanningAlgorithmStage
    {
        None = 0,
        Preparing = 1,
        Prepared = 2,
        RunningV = 3,
        RunningG = 4,
        Complete = 5,
        Failed = 6
    }
}

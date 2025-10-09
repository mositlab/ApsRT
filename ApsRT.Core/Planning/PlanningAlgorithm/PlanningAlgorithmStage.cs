using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Planning.PlanningAlgorithm
{
    public enum PlanningAlgorithmStage
    {
        None = 0,
        Preparing = 1,
        Prepared = 2,
        RunningV = 3,
        FailedV = 4,
        RunningT = 5,
        RunningG = 6,
        Complete = 7,
        Failed = 8
    }
}

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
        RunningG = 5,
        Complete = 6,
        Failed = 7
    }
}

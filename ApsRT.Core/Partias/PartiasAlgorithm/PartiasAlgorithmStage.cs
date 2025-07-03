using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Partias.PartiasAlgorithm
{
    public enum PartiasAlgorithmStage
    {
        None = 0,
        PreparingSession = 1,
        PreparingPartias = 2,
        PreparingZadelsLocked = 3,
        FiilingDeadlineTables = 4,
        FillingDeepTables = 5,
        PreparingZadelsFree = 6,
        Searching = 7,
        AfterSearchErrorsCheck = 8,
        InsidePartiasOptimization = 9,
        //SubdivisionsOptimization = 10,
        FormingEasyFlags = 11,
        Output = 12,
        Complete = 13,
        Failed = 14
    }
}

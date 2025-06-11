using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Partias.PartiasAlgorithm
{
    public enum PartiasAlgorithmStage
    {
        None = 0,
        PreparingSession = 1,
        PreparingPartias = 2,
        //PreapringZadels = 3,
        PreparingLeftovers = 4,
        FiilingDeadlineTables = 5,
        FillingDeepTables = 6,
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

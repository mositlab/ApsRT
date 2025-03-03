using Rodana.Core.MasterData.Stages;
using Rodana.Core.MasterData.Taras;
using Rodana.Core.MasterData.Centers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.MasterData
{
    public class MasterDataLinks<InputDataType>
    {
        public CentersMasterData CentersMasterData { get; set; } = new();
        public ParallelCentersVariantsMasterData ParallelCentersVariantsMasterData { get; set; } = new();
        public WorkingIntervalsMasterData WorkingIntervalsMasterData { get; set; } = new();

        // Переналадки
        // Наладочные длины

        public StagesMasterData StagesMasterData { get; init; } = new();
        public StagesTypicalDurationsMasterData<InputDataType> StageTypicalDurations { get; init; } = new();

        public UsedTaraMasterData UsedTaraMasterData { get; init; } = new();
        public MaxTaraCapacityMasterData MaxTaraCapacityMasterData { get; init; } = new();
        public TaraCountMasterData TaraCountMasterData { get; init; } = new();

    }
}

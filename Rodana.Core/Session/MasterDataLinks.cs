using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rodana.Core.Stages;
using Rodana.Core.Taras;
using Rodana.Core.Centers;
using Rodana.Core.Props;

namespace Rodana.Core.Session
{
    public class MasterDataLinks<InputDataType>
    {
        public CentersMasterData CentersMasterData { get; set; } = new();
        public ParallelCentersVariantsMasterData ParallelCentersVariantsMasterData { get; set; } = new();
        public WorkingIntervalsMasterData WorkingIntervalsMasterData { get; set; } = new();

        // Переналадки
        // Наладочные длины

        /// <summary>
        /// НСИ "Операции" (к формированию партий и планированию)
        /// </summary>
        public StagesMasterData StagesMasterData { get; init; } = new();

        /// <summary>
        /// НСИ "Длительности операций"
        /// </summary>
        public StagesTypicalDurationsMasterData<InputDataType> StagesTypicalDurationsMasterData { get; init; } = new();

        /// <summary>
        /// НСИ "Применяемая тара"
        /// </summary>
        public UsedTaraMasterData UsedTaraMasterData { get; init; } = new();

        /// <summary>
        /// НСИ "Максимальные намотки"
        /// </summary>
        public MaxTaraCapacityMasterData MaxTaraCapacityMasterData { get; init; } = new();
        public TaraCountMasterData TaraCountMasterData { get; init; } = new();

        public PropsMasterData PropsMasterData { get; init; } = new();

    }
}

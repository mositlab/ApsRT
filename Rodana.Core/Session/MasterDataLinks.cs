using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rodana.Core.Taras;
using Rodana.Core.Centers;
using Rodana.Core.Props;
using Rodana.Core.Rechanges.MasterData;
using Rodana.Core.Centers.MasterData;
using Rodana.Core.Partias.MergeConditions;
using Rodana.Core.Partias.Leftovers;
using Rodana.Core.Stages.MasterData;
using Rodana.Core.Planning.ParallelPlanning;
using Rodana.Core.Orders;
using Rodana.Core.Partias.SubdivisionsTransfers;
using Rodana.Core.Partias.Zadels;
using Rodana.Core.NaladLengths;

namespace Rodana.Core.Session
{
    public class MasterDataLinks
    {
        public MasterDataLinks(Session SessionLink)
        {
            CentersMasterData = new CentersMasterData(SessionLink);
            RechangesMasterData = new RechangesMasterData(SessionLink);
            AdditionalConditionsMasterData = new AdditionalConditionsMasterData(SessionLink);
            WorkingIntervalsMasterData = new WorkingIntervalsMasterData(SessionLink);
            ParallelPlanningMasterData = new ParallelPlanningMasterData(SessionLink);
            TarasMasterData = new TarasMasterData(SessionLink);
            StagesMasterData = new StagesMasterData(SessionLink);
            ZadelsMasterData = new ZadelsMasterData(SessionLink);
            OrdersMasterData = new OrdersMasterData(SessionLink);
        }

        public NaladLengthsMasterData NaladLengthsMasterData { get; set; }

        public ZadelsMasterData ZadelsMasterData { get; set; }

        public SubdivisionsTransfersMasterData SubdivisionsTransfersMasterData { get; set; } = new();

        public OrdersMasterData OrdersMasterData { get; set; }

        public LeftoversMasterData LeftoversMasterData { get; set; } = new();

        public MergeConditionsMasterData MergeConditionsMasterData { get; set; } = new();

        public CentersMasterData CentersMasterData { get; set; }
        public ParallelPlanningMasterData ParallelPlanningMasterData { get; set; }
        public WorkingIntervalsMasterData WorkingIntervalsMasterData { get; set; }

        /// <summary>
        /// НСИ "Переналадки"
        /// </summary>
        public RechangesMasterData RechangesMasterData { get; set; }

        /// <summary>
        /// НСИ "Дополнительные условия для переналадок"
        /// </summary>
        public AdditionalConditionsMasterData AdditionalConditionsMasterData { get; set; }

        // Наладочные длины

        /// <summary>
        /// НСИ "Операции" (к формированию партий и планированию)
        /// </summary>
        public StagesMasterData StagesMasterData { get; init; }

        /// <summary>
        /// НСИ "Длительности операций"
        /// </summary>
        public StagesTypicalDurationsMasterData StagesTypicalDurationsMasterData { get; init; } = new();

        /// <summary>
        /// НСИ "Применяемая тара", "Максимальные намотки"
        /// </summary>
        public TarasMasterData TarasMasterData { get; init; }

        public PropsMasterData PropsMasterData { get; init; } = new();

    }
}

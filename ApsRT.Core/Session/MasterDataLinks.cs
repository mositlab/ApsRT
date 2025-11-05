using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApsRT.Core.Taras;
using ApsRT.Core.Centers;
using ApsRT.Core.Props;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Centers.MasterData;
using ApsRT.Core.Partias.MergeConditions;
using ApsRT.Core.Stages.MasterData;
using ApsRT.Core.Planning.ParallelPlanning;
using ApsRT.Core.Orders;
using ApsRT.Core.Partias.Zadels;
using ApsRT.Core.NaladLengths;
using ApsRT.Core.Leftovers;
using ApsRT.Core.SubdivisionsTransfers;
using ApsRT.Core.RottingTime;

namespace ApsRT.Core.Session
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
            NaladLengthsMasterData = new NaladLengthsMasterData(SessionLink);
            LeftoversMasterData = new LeftoversMasterData(SessionLink);
            PropsMasterData = new PropsMasterData(SessionLink);
        }

        public NaladLengthsMasterData NaladLengthsMasterData { get; set; }

        public ZadelsMasterData ZadelsMasterData { get; set; }

        public SubdivisionsTransfersMasterData SubdivisionsTransfersMasterData { get; set; } = new();

        public RottingTimeMasterData RottingTimeMasterData { get; set; } = new();

        public OrdersMasterData OrdersMasterData { get; set; }

        public LeftoversMasterData LeftoversMasterData { get; set; }

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

        public PropsMasterData PropsMasterData { get; init; }

    }
}

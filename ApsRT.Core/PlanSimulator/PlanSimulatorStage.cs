using ApsRT.Core.NaladLengths;

namespace ApsRT.Core.PlanSimulator
{
    public class PlanSimulatorStage
    {
        public PlanSimulatorMod ModLink;

        public Stages.Stage Stage;
        public PlanSimulatorNaladLengthData[] NaladLengthData;
        public PlanSimulatorOrderData[] OrderData;
        public PlanSimulatorPartiaRecalcData[] PartiaRecalcData;

        public List<PlanSimulatorStage> PreviousStages;
        public List<PlanSimulatorStage> NextStages;
    }
}
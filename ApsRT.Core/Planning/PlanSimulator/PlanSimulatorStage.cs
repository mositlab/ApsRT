using ApsRT.Core.NaladLengths;

namespace ApsRT.Core.Planning.PlanSimulator
{
    public class PlanSimulatorStage
    {
        public PlanSimulatorMod ModLink;

        public Stages.Stage Stage;
        public GottenNaladLengthStruct NaladLength;
        public PlanSimulatorOrderClass OrderClass;
        public PlanSimulatorPartiaRecalcClass PartiaClass;

        public override int GetHashCode()
        {
            return Stage.GetHashCode();
        }
    }
}
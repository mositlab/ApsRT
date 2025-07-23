namespace ApsRT.Core.PlanSimulator
{
    public class PlanSimulatorPartiaRecalcData
    {
        public bool WillRecalc;
        public int Flag;

        public PlanSimulatorPartiaRecalcData(bool willRecalc, int flag)
        {
            WillRecalc = willRecalc;
            Flag = flag;
        }

        public override string ToString()
        {
            return String.Format("#{0:X}", GetHashCode());
        }
    }
}
namespace ApsRT.Core.PlanSimulator
{
    public class PlanSimulatorOrderData
    {
        public DateTime MinValue;
        public int Flag;
        public double Time;
        public double TimeWithK;

        public PlanSimulatorOrderData(DateTime minValue, int flag, double time, double timeWithK)
        {
            MinValue = minValue;
            Flag = flag;
            Time = time;
            TimeWithK = timeWithK;
        }
    }
}
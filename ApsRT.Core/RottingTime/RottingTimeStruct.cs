using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.RottingTime
{
    public class RottingTimeStruct
    {
        public RottingTimeStruct(double acceptableTime, double maxTime)
        {
            AcceptableTime = acceptableTime;
            MaxTime = maxTime;
        }

        public double AcceptableTime { get; }
        public double MaxTime { get; }

        static RottingTimeStruct _maxValue = new RottingTimeStruct(double.MaxValue, double.MaxValue);
        public static RottingTimeStruct MaxValue
        {
            get => _maxValue;
        }
    }
}

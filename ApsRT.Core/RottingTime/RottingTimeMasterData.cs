using ApsRT.Core.Stages.Hoppers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.RottingTime
{
    public class RottingTimeMasterData
    {
        internal Dictionary<Uco, Dictionary<Uco, RottingTimeStruct>> TimeInDays;

        public void ClearAll() => TimeInDays = null;

        public RottingTimeStruct GetRottingTime(Uco Nomenklature, Uco Characteristics)
        {
            if (TimeInDays == null) return RottingTimeStruct.MaxValue;
            if (TimeInDays.TryGetValue(Nomenklature, out var lvl1))
            {
                if (lvl1.TryGetValue(Characteristics, out var val)) return val;
                if (lvl1.TryGetValue(Uco.EmptyObject, out var val2)) return val2;
            }
            return RottingTimeStruct.MaxValue;
        }

        public bool TryAddRottingTime(RottingTimeDataLoaderModel model)
        {
            return TryAddRottingTime(model.Nomenklature, model.Characteristics, model.AcceptableTime, model.MaxTime);
        }

        public bool TryAddRottingTime(Uco Nomenklature, Uco Characteristics, double AcceptableTime, double MaxTime)
        {
            if (TimeInDays == null) TimeInDays = new();
            TimeInDays.TryAdd(Nomenklature, new());
            return TimeInDays[Nomenklature].TryAdd(Characteristics, new RottingTimeStruct(AcceptableTime, MaxTime));
        }
    }
}

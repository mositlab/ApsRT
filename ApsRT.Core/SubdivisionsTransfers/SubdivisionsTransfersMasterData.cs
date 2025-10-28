using ApsRT.Core.Stages.Hoppers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.SubdivisionsTransfers
{
    public class SubdivisionsTransfersMasterData
    {
        internal Dictionary<Uco, Dictionary<Uco, int>> TimeInSeconds;

        public void ClearAll() => TimeInSeconds = null;

        internal int GetSubdivisionTransferTime(Uco From, Uco To)
        {
            if (TimeInSeconds == null) return 0;
            if (TimeInSeconds.TryGetValue(From, out var lvl1) && lvl1 != null && lvl1.TryGetValue(To, out var val))
            {
                return val;
            }
            return 0;
        }

        public bool TryAddTypicalDuration(SubdivisionsTransferDataLoaderModel model)
        {
            return TryAddTypicalDuration(model.SubFrom, model.SubTo, model.Duration);
        }

        public bool TryAddTypicalDuration(Uco SubFrom, Uco SubTo, int Duration)
        {
            if (TimeInSeconds == null) TimeInSeconds = new();
            TimeInSeconds.TryAdd(SubFrom, new());
            return TimeInSeconds[SubFrom].TryAdd(SubTo, Duration);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Partias.SubdivisionsTransfers
{
    public class SubdivisionsTransfersMasterData
    {
        internal Dictionary<Uco, Dictionary<Uco, int>> TimeInSeconds;

        internal int GetSubdivisionTransferTime(Uco From, Uco To)
        {
            if (TimeInSeconds == null) return 0;
            if (TimeInSeconds.TryGetValue(From, out var lvl1) && lvl1 != null && lvl1.TryGetValue(To, out var val))
            {
                return val;
            }
            return 0;
        }
    }
}

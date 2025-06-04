using UniversalPack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Stages.MasterData
{

    /// <summary>
    /// НСИ "Длительности операций"
    /// </summary>
    public class StagesTypicalDurationsMasterData
    {
        Dictionary<Uco,
            Dictionary<Uco,
                int>
            > TypicalDurations;

        public bool AnyDurations => TypicalDurations != null;

        public bool TryGetTypicalDuration(Uco RC, Uco OperationType, out int duration)
        {
            if (TypicalDurations.TryGetValue(RC, out var opers))
            {
                if (opers.TryGetValue(OperationType, out var dur))
                {
                    duration = dur;
                    return true;
                }
                else
                {
                    duration = 0;
                    return false;
                }
            }
            else
            {
                duration = 0;
                return false;
            }
        }

        public bool TryAddTypicalDuration(Uco RC, Uco OperationType, int DurationSecs)
        {
            if (TypicalDurations == null) TypicalDurations = new();
            if (!TypicalDurations.ContainsKey(RC)) TypicalDurations.Add(RC, new());
            if (TypicalDurations[RC].ContainsKey(OperationType))
            {
                return false;
            }
            else
            {
                TypicalDurations[RC].Add(OperationType, DurationSecs);
                return true;
            }
        }
    }
}

using Rodana.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.MasterData.Stages
{

    /// <summary>
    /// НСИ "Длительности операций"
    /// </summary>
    public class StagesTypicalDurationsMasterData<InputDataType>
    {
        Dictionary<CodedObject<InputDataType, string>,
            Dictionary<CodedObject<InputDataType, string>,
                int>
            > TypicalDurations;

        public bool AnyDurations => TypicalDurations != null;

        public bool TryGetTypicalDuration(CodedObject<InputDataType, string> RC, CodedObject<InputDataType, string> OperationType, out int duration)
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

        public bool TryAddTypicalDuration(CodedObject<InputDataType, string> RC, CodedObject<InputDataType, string> OperationType, int DurationSecs)
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

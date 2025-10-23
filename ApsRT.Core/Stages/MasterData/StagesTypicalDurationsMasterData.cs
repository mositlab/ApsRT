using ApsRT.Core.Stages.Hoppers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Stages.MasterData
{

    /// <summary>
    /// НСИ "Длительности операций"
    /// </summary>
    public class StagesTypicalDurationsMasterData
    {
        Dictionary<Uco, // РЦ
            Dictionary<Uco, // Номенклатура
                Dictionary<Uco, // Вид
                    Dictionary<Uco, // харка
                        double>
                    >
                >
            > TypicalDurations;

        public bool AnyDurations => TypicalDurations != null;

        public void ClearAll() => TypicalDurations = null;

        public bool TryGetTypicalDuration(Uco RC, Stage stage, out double DurationNorm)
        {
            Uco Nomenklature = stage.Nomenklature;
            Uco OperationType = stage.OperationVid;
            Uco Characteristics = stage.Characteristics;
            if (TypicalDurations.TryGetValue(RC, out var nomens)
                && nomens.TryGetValue(Nomenklature, out var opers))
            {
                if (opers.TryGetValue(OperationType, out var chars))
                {
                    if (chars.TryGetValue(Characteristics, out var dur))
                    {
                        DurationNorm = dur;
                        return true;
                    }
                    else
                    {
                        if (chars.TryGetValue(Uco.EmptyObject, out var dur2))
                        {
                            DurationNorm = dur2;
                            return true;
                        }
                    }
                }
                else
                {
                    if (opers.TryGetValue(Uco.EmptyObject, out var chars2))
                    {
                        if (chars2.TryGetValue(Characteristics, out var dur))
                        {
                            DurationNorm = dur;
                            return true;
                        }
                        else
                        {
                            if (chars2.TryGetValue(Uco.EmptyObject, out var dur2))
                            {
                                DurationNorm = dur2;
                                return true;
                            }
                        }
                    }
                }
            }
            DurationNorm = 0;
            return false;
        }

        public bool TryAddTypicalDuration(TypicalDurationDataLoaderModel model)
        {
            return TryAddTypicalDuration(model.Center, model.Nomenklature, model.OperationVid, model.Characteristics, model.Duration);
        }

        public bool TryAddTypicalDuration(Uco RC, Uco Nomenklature, Uco OperationType, Uco Characteristics, double DurationNorm)
        {
            if (TypicalDurations == null) TypicalDurations = new();
            TypicalDurations.TryAdd(RC, new());
            TypicalDurations[RC].TryAdd(Nomenklature, new());
            TypicalDurations[RC][Nomenklature].TryAdd(OperationType, new());
            return TypicalDurations[RC][Nomenklature][OperationType].TryAdd(Characteristics, DurationNorm);
        }
    }
}

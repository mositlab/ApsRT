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
                        TypicalDurationData>
                    >
                >
            > TypicalDurations;

        public void PostProcessTypicalDurations()
        {
            if (!AnyTypicalDurations) return;

            foreach (var rcs in TypicalDurations)
            {
                foreach (var nomens in rcs.Value)
                {
                    foreach (var charas in nomens.Value)
                    {
                        foreach (var val in charas.Value)
                        {
                            val.Value.PrepareClassicDuration();
                            val.Value.RememberMinDuration();
                        }
                    }
                }
            }
        }

        public bool AnyTypicalDurations => TypicalDurations != null;

        public void ClearTypicalDurations() => TypicalDurations = null;

        public void ClearOrderDurations() => OrderDurations = null;

        public bool AnyOrderDurations => OrderDurations != null;


        Dictionary<Uco, // РЦ
                Dictionary<Uco, // Вид
                    Dictionary<bool, // ОО
                        double>
                    >
                > OrderDurations;

        public void PostProcessOrderDurations()
        {
            if (!AnyOrderDurations) return;
            // Пока тут ничего нет, но мб в будущем понадобится
            ;
        }

        public bool TryAddOrderDuration(Uco RC, Uco OperationType, bool order, double DurationNorm)
        {
            if (OrderDurations == null) OrderDurations = new();
            OrderDurations.TryAdd(RC, new());
            OrderDurations[RC].TryAdd(OperationType, new());
            return OrderDurations[RC][OperationType].TryAdd(order, DurationNorm);
        }

        public bool TryAddOrderDuration(OrderDurationDataLoaderModel model)
        {
            return TryAddOrderDuration(model.Center, model.OperationVid, model.ItsOrder, model.Duration);
        }

        public bool TryGetTypicalDuration(Uco RC, Stage stage, out double DurationNorm)
        {
            Uco OperationType = stage.OperationVid;

            if (AnyTypicalDurations)
            {
                Uco Nomenklature = stage.Nomenklature;
                Uco Characteristics = stage.Characteristics;
                if (TypicalDurations.TryGetValue(RC, out var nomens)
                    && nomens.TryGetValue(Nomenklature, out var opers))
                {
                    if (opers.TryGetValue(OperationType, out var chars))
                    {
                        if (chars.TryGetValue(Characteristics, out var dur))
                        {
                            DurationNorm = dur.GetDuration(stage);
                            return true;
                        }
                        else
                        {
                            if (chars.TryGetValue(Uco.EmptyObject, out var dur2))
                            {
                                DurationNorm = dur2.GetDuration(stage);
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
                                DurationNorm = dur.GetDuration(stage);
                                return true;
                            }
                            else
                            {
                                if (chars2.TryGetValue(Uco.EmptyObject, out var dur2))
                                {
                                    DurationNorm = dur2.GetDuration(stage);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            if (AnyOrderDurations)
            {
                bool order = stage.ItsOrder;
                if (OrderDurations.TryGetValue(RC, out var opers))
                {
                    if (opers.TryGetValue(OperationType, out var orders))
                    {
                        if (orders.TryGetValue(order, out var res))
                        {
                            DurationNorm = res;
                            return true;
                        }
                        else
                        {
                            // Если нет по Ордеру, ищем среднее, у нее нет галки ОО
                            if (orders.TryGetValue(false, out var res2))
                            {
                                DurationNorm = res2;
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
            return TryAddTypicalDuration(model.Center, model.Nomenklature, model.OperationVid, model.Characteristics, model.Variant, model.OperNumber, model.Duration);
        }

        public bool TryAddTypicalDuration(Uco RC, Uco Nomenklature, Uco OperationType, Uco Characteristics, Uco variant, int operNumber, double DurationNorm)
        {
            if (TypicalDurations == null) TypicalDurations = new();
            TypicalDurations.TryAdd(RC, new());
            TypicalDurations[RC].TryAdd(Nomenklature, new());
            TypicalDurations[RC][Nomenklature].TryAdd(OperationType, new());
            TypicalDurations[RC][Nomenklature][OperationType].TryAdd(Characteristics, new());
            return TypicalDurations[RC][Nomenklature][OperationType][Characteristics].AddData(variant, operNumber, DurationNorm);
        }
    }
}

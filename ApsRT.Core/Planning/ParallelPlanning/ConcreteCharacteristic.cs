using ApsRT.Core.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Extensions;

namespace ApsRT.Core.Planning.ParallelPlanning
{
    /// <summary>
    /// Одна характеристика из набора
    /// </summary>
    internal class ConcreteCharacteristic
    {
        internal double Min;
        internal double Max;
        internal string Value;
        internal Guid ValueGuid;

        internal PropsLinkerClass UsedProp;

        public override string ToString()
        {
            return $"{Min}-{Max}/ '{Value}' / {ValueGuid} [{UsedProp}]";
        }

        public ConcreteCharacteristic(double min, double max, PropsLinkerClass prop, string value, Guid valueGuid)
        {
            Min = min;
            Max = max;
            UsedProp = prop;
            Value = value;
            ValueGuid = valueGuid;
        }


        internal bool IsEqualWith(Stages.Stage stage)
        {
            if (UsedProp.NumberIndex > -1)
            {
                // Смотрим - эта характеристика - это диапазон?
                var data = stage.NumberProps[UsedProp.NumberIndex];
                if (!data.HasValue) return false;
                return (float)Min <= data.Value && data.Value <= (float)Max;
            }
            else
            {
                if (UsedProp.GuidIndex > -1)
                {
                    // Это гуид?
                    var data = stage.GuidProps[UsedProp.GuidIndex][0];
                    return data == ValueGuid;
                }
                else
                {
                    if (UsedProp.StringIndex > -1)
                    {
                        // Значит, это НЕ диапазон. Смотрим втупую
                        var data = stage.StringProps[UsedProp.StringIndex];
                        return data == Value;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}

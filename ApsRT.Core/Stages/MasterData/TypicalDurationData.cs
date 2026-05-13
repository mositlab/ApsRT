using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Stages.MasterData
{
    internal class TypicalDurationData
    {
        internal double? ClassicDuration;

        internal Dictionary<Uco, TypicalDurationVariant> Variants;

        internal Uco? MinimumDurationVariant;

        internal double GetDurationByVariant(Stage stage)
        {

            return ClassicDuration.Value;
        }

        internal bool PrepareClassicDuration()
        {
            if (Variants != null && Variants.Count == 1)
            {
                var val = Variants.First().Value;
                if (val.UsedOpersNumbersOnLoad.Count == 1
                        && val.UsedOpersNumbersOnLoad.First() == 1)
                {
                    ClassicDuration = val.Duraion;
                    return true;
                }
            }
            return false;
        }

        internal bool RememberMinDuration()
        {
            if (Variants != null)
            {
                MinimumDurationVariant = Variants.MinBy(x => x.Value.Duraion).Key;
                return true;
            }
            return false;
        }

        internal bool AddData(Uco Variant, int LaunchIndex, double duration)
        {
            if (Variant.IsEmpty)
            {
                bool wasEmpty = !ClassicDuration.HasValue;

                ClassicDuration = duration;

                return wasEmpty;
            }
            else
            {
                Variants ??= new();
                Variants.TryAdd(Variant, new TypicalDurationVariant());
                Variants[Variant].Duraion = duration;
                return Variants[Variant].UsedOpersNumbersOnLoad.Add(LaunchIndex);
            }
        }

        internal double GetDuration(Stage stage)
        {
            int number = stage.StageVersion;

            return ClassicDuration.HasValue ? ClassicDuration.Value : GetDurationByVariant(stage);
        }
    }
}

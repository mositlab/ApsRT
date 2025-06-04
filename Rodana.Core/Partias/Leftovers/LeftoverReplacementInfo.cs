using Rodana.Core.Partias.Leftovers;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.Leftovers
{
    internal class LeftoverReplacementInfo
    {
        public Uco Original { get; }
        public double OriginalCount { get; }
        public Uco Analogue { get; }
        public double AnalogueCount { get; }

        public LeftoverReplacementInfo(LeftoverGroupDataLoaderModel model)
        {
            Original = model.OriginalMaterial;
            OriginalCount = model.OriginalCount;
            Analogue = model.AnalogueMaterial;
            AnalogueCount = model.AnalogueCount;
        }

        public LeftoverReplacementInfo(Uco original, double originalCount, Uco analogue, double analogueCount)
        {
            Original = original;
            OriginalCount = originalCount;
            Analogue = analogue;
            AnalogueCount = analogueCount;
        }
    }
}

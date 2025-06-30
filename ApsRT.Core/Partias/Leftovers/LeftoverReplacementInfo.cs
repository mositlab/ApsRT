using ApsRT.Core.Partias.Leftovers;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using LeftoverUco = (UniversalPack.Common.CodedObject<System.Guid, string> Nomenklature, UniversalPack.Common.CodedObject<System.Guid, string> Characteristics);

namespace ApsRT.Core.Partias.Leftovers
{
    internal class LeftoverReplacementInfo
    {
        public LeftoverUco Original { get; }
        public double OriginalCount { get; }
        public LeftoverUco Analogue { get; }
        public double AnalogueCount { get; }

        public LeftoverReplacementInfo(LeftoverGroupDataLoaderModel model)
        {
            Original = (model.OriginalMaterialNomenklature, model.OriginalMaterialCharacteristics);
            OriginalCount = model.OriginalCount;
            Analogue = (model.AnalogueMaterialNomenklature, model.AnalogueMaterialCharacteristics);
            AnalogueCount = model.AnalogueCount;
        }

        public LeftoverReplacementInfo(LeftoverUco original, double originalCount, LeftoverUco analogue, double analogueCount)
        {
            Original = original;
            OriginalCount = originalCount;
            Analogue = analogue;
            AnalogueCount = analogueCount;
        }
    }
}

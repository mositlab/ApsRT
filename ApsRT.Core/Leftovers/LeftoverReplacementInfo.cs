using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using LeftoverUco = (UniversalPack.Common.CodedObject<System.Guid, string> Nomenklature, UniversalPack.Common.CodedObject<System.Guid, string> Characteristics);

namespace ApsRT.Core.Leftovers
{
    public class LeftoverReplacementInfo
    {
        public LeftoverUco Original { get; }
        public double OriginalCount { get; }
        public LeftoverUco Analogue { get; }
        public double AnalogueCount { get; }

        int _hashCode;

        void SetHashCode()
        {
            _hashCode = HashCode.Combine(Original.Nomenklature.Code, Original.Characteristics.Code, Analogue.Nomenklature.Code, Analogue.Characteristics.Code);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public LeftoverReplacementInfo(LeftoverGroupDataLoaderModel model)
        {
            Original = (model.OriginalMaterialNomenklature, model.OriginalMaterialCharacteristics);
            OriginalCount = model.OriginalCount;
            Analogue = (model.AnalogueMaterialNomenklature, model.AnalogueMaterialCharacteristics);
            AnalogueCount = model.AnalogueCount;
            SetHashCode();
        }

        public LeftoverReplacementInfo(LeftoverUco original, double originalCount, LeftoverUco analogue, double analogueCount)
        {
            Original = original;
            OriginalCount = originalCount;
            Analogue = analogue;
            AnalogueCount = analogueCount;
            SetHashCode();
        }
    }
}

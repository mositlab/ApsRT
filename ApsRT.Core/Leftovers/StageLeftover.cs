using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using LeftoverUco = (UniversalPack.Common.CodedObject<System.Guid, string> Nomenklature, UniversalPack.Common.CodedObject<System.Guid, string> Characteristics, UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string> Seria);
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;

namespace ApsRT.Core.Leftovers
{
    public class StageLeftover
    {
        public StageLeftover(LeftoverUco leftoverData, double calculatedCount, double originalCount, UcoDU edIzm)
        {
            LeftoverData = leftoverData;
            CalculatedCount = calculatedCount;
            OriginalCount = originalCount;
            EdIzm = edIzm;
        }

        public LeftoverUco LeftoverData { get; set; }
        public double CalculatedCount { get; set; }
        public double OriginalCount { get; set; }
        public UcoDU EdIzm { get; set; }

        public override string ToString()
        {
            return $"ОСТАТОК: {LeftoverData.Nomenklature} - {LeftoverData.Characteristics} [{LeftoverData.Seria}] ({CalculatedCount} {EdIzm})";
        }
    }
}

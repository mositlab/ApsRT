using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Partias.PartiasClasses
{
    public class PartiaModelling : PartiaModellingSmall
    {
        public Uco Tara; // И информация
        public double Max;
        public DimensionUnit Format;
        public Uco RC;
        public bool RCIsChecked = false; // TODO: тестовая хрень, которая из-за ошибок в НСИ себя проявляет

        public int? PlacesCountMultiplierToRewrite;

        public PartiaModelling(List<Stages.Stage> opers, Uco tara, double max, DimensionUnit format, Uco rC, int? placesCountMultiplierToRewrite, int zadelCode, List<Guid> zadelGuids) : base(opers, zadelCode, zadelGuids)
        {
            Tara = tara;
            Max = max;
            Format = format;
            RC = rC;
            PlacesCountMultiplierToRewrite = placesCountMultiplierToRewrite;
        }

        public override string ToString() => String.Format("#{0:X} {1} {2}", GetHashCode(), ZadelCode, RCIsChecked ? "T" : "U");
    }
}

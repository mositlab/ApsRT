using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.PartiasClasses
{
    internal class PartiaModelling : PartiaModellingSmall
    {
        internal Uco Tara; // И информация
        internal double Max;
        internal DimensionUnit Format;
        internal Uco RC;
        internal bool RCIsChecked = false; // TODO: тестовая хрень, которая из-за ошибок в НСИ себя проявляет

        public PartiaModelling(List<Stages.Stage> opers, Uco tara, double max, DimensionUnit format, Uco rC, int zadelCode, List<Guid> zadelGuids) : base(opers, zadelCode, zadelGuids)
        {
            Tara = tara;
            Max = max;
            Format = format;
            RC = rC;
        }

        public override string ToString() => String.Format("#{0:X} {1} {2}", GetHashCode(), ZadelCode, RCIsChecked ? "T" : "U");
    }
}

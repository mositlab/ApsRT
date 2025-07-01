using ApsRT.Core.DataErrors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Taras.TaraSearchEngine
{
    public class TemporaryTaraSearchData
    {
        internal (Uco RC, Uco Tara)[] PossibleCentersAndTaras;
        internal (Uco Tara, DimensionUnit Format, double Namot)[] BARABANAS;
        internal StringBuilder errorMessage;
        internal double sum;
        internal DimensionUnit opersFormat;
        internal int OpersCount;

        internal bool WillVspom;

        internal DataErrorStruct dataErrorMore;
        //internal int NotTaraCount;
        //internal bool notChangeTara;
    }
}

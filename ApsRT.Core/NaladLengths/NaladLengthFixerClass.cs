using ApsRT.Core.Partias.TaraSearchEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.NaladLengths
{
    public class NaladLengthFixerClass
    {
        internal IAbleForTaraCalcInterface ops;
        internal int type;
        internal Uco tara;
        internal double max;
    }
}

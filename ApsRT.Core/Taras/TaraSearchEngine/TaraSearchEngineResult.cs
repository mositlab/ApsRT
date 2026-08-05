using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Taras.TaraSearchEngine
{
    public class TaraSearchEngineResult
    {
        public Uco TaraName, FinalRC;
        public double Max;
        public DimensionUnit used_format;
        public bool notChangeTara;

        public int CurrentPlacesCountK = 1;

        public int ExceptTheseTarasNonErrorsCount = int.MaxValue;

        public TaraSearchEngineResult()
        {
            TaraName = Uco.EmptyObject;
            FinalRC = Uco.EmptyObject;
            used_format = DimensionUnit.Unknown;
            Max = 0;
            notChangeTara = false;
            CurrentPlacesCountK = 1;
        }
    }
}

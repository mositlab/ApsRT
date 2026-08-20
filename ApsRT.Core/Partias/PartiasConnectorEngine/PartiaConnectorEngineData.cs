using ApsRT.Core.NaladLengths;
using ApsRT.Core.Partias.PartiasClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;

namespace ApsRT.Core.Partias.PartiasConnectorEngine
{
    public class PartiaConnectorEngineData
    {
        public PartiaClass firstPartia;
        public PartiaClass secondPartia;
        public bool withLog;
        public bool withHerself;
        public Dictionary<int, NaladLengths.GottenNaladLengthStruct> NaladLengthsData;
        public Dictionary<int, Uco> RestrictionsVia74110;
        public Dictionary<int, HashSet<Uco>> ExceptTheseTarasGlobal;

        public Dictionary<int, HashSet<Uco>> PossibleRCsGlobal;

        public void Deconstruct(out PartiaClass partia1, out PartiaClass partia2, out bool withLog, out bool withHerself)
        {
            partia1 = firstPartia;
            partia2 = secondPartia;
            withLog = this.withLog;
            withHerself = this.withHerself;
        }

        public PartiaConnectorEngineData(PartiaClass firstPartia, PartiaClass secondPartia, bool withLog, bool withHerself,
            Dictionary<int, GottenNaladLengthStruct> naladLengthsData,
            Dictionary<int, Uco> restrictionsVia74110,
            Dictionary<int, HashSet<Uco>> exceptTheseTarasGlobal,
            Dictionary<int, HashSet<Uco>> possibleRCs
            )
        {
            this.firstPartia = firstPartia;
            this.secondPartia = secondPartia;
            this.withLog = withLog;
            this.withHerself = withHerself;
            NaladLengthsData = naladLengthsData;
            RestrictionsVia74110 = restrictionsVia74110;
            ExceptTheseTarasGlobal = exceptTheseTarasGlobal;
            PossibleRCsGlobal = possibleRCs;
        }
    }
}

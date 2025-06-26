using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using LeftoverUco = (UniversalPack.Common.CodedObject<System.Guid, string> Nomenklature, UniversalPack.Common.CodedObject<System.Guid, string> Characteristics);

namespace Rodana.Core.Partias.Leftovers
{
    public class LeftoverLackData
    {
        public LeftoverLackData(LeftoverUco leftoverData, Guid partiaPF, double countNeeded, double countExists, DateTime dateWhenNeeded, DateTime? nextArrivalDate, Guid stageGuid, Guid modellingGuid, Guid easyLifeGuid)
        {
            LeftoverData = leftoverData;
            PartiaPF = partiaPF;
            CountNeeded = countNeeded;
            CountExists = countExists;
            DateWhenNeeded = dateWhenNeeded;
            NextArrivalDate = nextArrivalDate;
            StageGuid = stageGuid;
            ModellingGuid = modellingGuid;
            EasyLifeGuid = easyLifeGuid;
        }

        public LeftoverUco LeftoverData { get; set; }
        public Guid PartiaPF { get; set; }
        public double CountNeeded { get; set; }
        public double CountExists { get; set; }
        public DateTime DateWhenNeeded { get; set; }
        public DateTime? NextArrivalDate { get; set; }
        public Guid StageGuid { get; set; }
        public Guid ModellingGuid { get; set; }
        public Guid EasyLifeGuid { get; set; }
    }
}

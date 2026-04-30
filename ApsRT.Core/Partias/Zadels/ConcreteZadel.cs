using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using StageGUID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, int>;

namespace ApsRT.Core.Partias.Zadels
{
    public class ConcreteZadel
    {
        public ConcreteZadel(Uco usedTara, Uco justTara, int level, int poviv, Uco nomen, Uco chara, StageGUID guid, Guid partiaPF, string? seriaCode)
        {
            UsedTara = usedTara;
            JustTara = justTara;
            Count = 0;
            Level = level;
            Poviv = poviv;
            Nomenklature = nomen;
            Characteristics = chara;
            StageGuid = guid;
            PartiaPF = partiaPF;
            SeriaCode = seriaCode;
        }

        public Uco Nomenklature { get; set; }
        public Uco Characteristics { get; set; }
        public int Poviv { get; set; }

        public int Level { get; }

        public double Count { get; set; }

        internal Uco UsedTara { get; } // на какой таре задел намотан
                                       //internal double Count { get; init; } // количество этого задела

        internal Uco JustTara { get; } // тупо надо выводить эту тару

        internal string? SeriaCode { get; }

        internal Guid PartiaPF {get; set;}
        internal StageGUID StageGuid { get; set; }
        //internal List<OrderID> RelatedGPs { get; set; } // пока не уверен надо ли

        internal ZadelStatus ZadelStatus { get; set; }

        //internal double CalcCount;
        internal List<(int operID, double count)> WhereIsUsed = new(); // ссылка идет на первую же операцию, потому что сослаться потом на эту операцию  самом конце вывода 

        /// <summary>
        /// Список операций, которые в заделе есть. И сколько они будут жрать.
        /// </summary>
        internal Dictionary<StageGUID, double> WhereCanUsed = new();
    }
}

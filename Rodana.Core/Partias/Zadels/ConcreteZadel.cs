using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.Zadels
{
    internal class ConcreteZadel
    {
        public ConcreteZadel(Uco usedTara/*, double count*/)
        {
            UsedTara = usedTara;
            //Count = count;
            //CalcCount = count;
            //RelatedGPs = new List<OrderID>(); // пока хз, насколько это тут надо
        }

        internal Uco UsedTara { get; init; } // на какой таре задел намотан
                                                             //internal double Count { get; init; } // количество этого задела

        //internal List<OrderID> RelatedGPs { get; set; } // пока не уверен надо ли

        internal ZadelStatus ZadelStatus { get; set; }

        //internal double CalcCount;
        internal List<(int operID, double count)> WhereIsUsed = new(); // ссылка идет на первую же операцию, потому что сослаться потом на эту операцию  самом конце вывода 

        /// <summary>
        /// Список операций, которые в заделе есть. И сколько они будут жрать.
        /// </summary>
        internal Dictionary<Guid, double> WhereCanUsed = new();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Planning.ParallelPlanning
{
    public struct ParallelPlanningTaraInfo
    {
        /// <summary>
        /// Если макс вместимость > 0, то алгоритм игнорирует четкое количество тары, а просто смотрит на вместимость
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Общее количество тары
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Тара и её количество
        /// </summary>
        public Dictionary<Uco, int> List { get; set; }

        public ParallelPlanningTaraInfo(int totalCount, Dictionary<Uco, int> list, int max)
        {
            TotalCount = totalCount;
            List = list;
            MaxCapacity = max;
        }

        public ParallelPlanningTaraInfo() : this(0, new Dictionary<Uco, int>(), 0) { }
    }
}

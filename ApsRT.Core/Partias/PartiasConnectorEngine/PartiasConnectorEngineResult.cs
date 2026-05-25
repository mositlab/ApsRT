using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Partias.PartiasConnectorEngine
{
    public class PartiasConnectorEngineResult
    {
        public int Total;
        public float FilledK;
        public int Diff;
        public int LongestModsMaxCount;

        public int mergedVspom = 0;
        public int mergedNorm = 0;

        public int LastLevel = 0;

        public Dictionary<int, Uco> IDToTara;

        public HashSet<int> EmptyErroredTarasInfo;

        public Dictionary<int, int> IDmergedPrevs;

        public Dictionary<int, int> IDmergedGroups;

        public Uco? RecommendedSubdivision;

        public Dictionary<int, int> ForOneRCGroups;

        public Dictionary<int, Uco?> ForOneRCGroupAlreadyUsed;

        public Dictionary<int, List<Uco>> PossibleCentersForOneRCGroups;

        public PartiasConnectorEngineResult(int Total, float K, int Diff, int LongestModsMaxCount)
        {
            LastLevel = 0;
            mergedVspom = 0;
            mergedNorm = 0;
            this.Total = Total;
            this.FilledK = K;
            this.Diff = Diff;
            this.LongestModsMaxCount = LongestModsMaxCount;
        }

        public PartiasConnectorEngineResult(int total, float filledK, int diff, int longestModsMaxCount, int iDToTara, int iDmergedPrevs, int iDmergedGroups) : this(total, filledK, diff, longestModsMaxCount)
        {
            IDToTara = new (iDToTara);
            EmptyErroredTarasInfo = new (iDToTara);
            IDmergedPrevs = new(iDmergedPrevs);
            IDmergedGroups = new(iDmergedGroups);
        }
    }
}

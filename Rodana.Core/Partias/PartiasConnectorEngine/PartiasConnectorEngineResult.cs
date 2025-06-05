using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.PartiasConnectorEngine
{
    internal class PartiasConnectorEngineResult
    {
        internal int Total;
        internal float FilledK;
        internal int Diff;
        internal int LongestModsMaxCount;

        internal int mergedVspom = 0;
        internal int mergedNorm = 0;

        internal int LastLevel = 0;

        internal Dictionary<int, Uco> IDToTara;

        internal Dictionary<int, int> IDmergedPrevs;

        internal Dictionary<int, int> IDmergedGroups;

        internal Uco? RecommendedSubdivision;

        internal Dictionary<int, int> ForOneRCGroups;

        internal Dictionary<int, Uco?> ForOneRCGroupAlreadyUsed;

        internal Dictionary<int, List<Uco>> PossibleCentersForOneRCGroups;

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
            IDmergedPrevs = new(iDmergedPrevs);
            IDmergedGroups = new(iDmergedGroups);
        }
    }
}

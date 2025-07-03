using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Leftovers
{
    public class LeftoversReplacementClass
    {
        public List<LeftoverReplacementInfo> List { get; private set; }
        public int Hash { get; private set; }

        public override int GetHashCode()
        {
            return Hash;
        }

        public void PostProcess()
        {
            List = List.OrderBy(x => x.Original)
                .ThenBy(x => x.Analogue)
                .ThenBy(x => x.OriginalCount)
                .ThenBy(x => x.AnalogueCount)
                .ToList();
            Hash = 0;
            foreach (LeftoverReplacementInfo replacement in List)
            {
                var hash = replacement.GetHashCode();
                Hash = HashCode.Combine(Hash, hash);
            }
        }

        public LeftoversReplacementClass()
        {
            List = new List<LeftoverReplacementInfo>();
            Hash = 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ApsRT.Core.NaladLengths
{
    public class NaladLengthDataNode
    {
        public Dictionary<Vector2, NaladLengthDataNode> NextLevel;
        public NaladLengthValue? Value;
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Session
{
    public class SessionOptionsLoaded
    {
        [JsonProperty]
        public Dictionary<string, string[]> CustomHoppersPaths { get; set; }
    }
}

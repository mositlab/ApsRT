using Rodana.Core.Partias.TaraSearchEngine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Session
{
    internal class SessionTempStorage
    {

        public ConcurrentDictionary<int, TemporaryTaraSearchData> ModToTaraInfo = new();

    }
}

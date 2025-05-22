using Rodana.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;

using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Centers.MasterData
{
    public class CentersMasterData
    {
        public Session.Session SessionLink { get; }

        public CentersMasterData(Session.Session session)
        {
            SessionLink = session;
        }

        public Dictionary<Uco, CenterClass> CentersData { get; } = new();
    }
}

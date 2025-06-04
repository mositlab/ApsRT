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

        public Dictionary<Uco, List<Uco>> RCRoutesToListDic = new(); // Список создаётся один раз

        public Dictionary<Uco, CenterBaseModel> CentersData { get; } = new();

        public void ClearAll()
        {
            RCRoutesToListDic.Clear();
            CentersData.Clear();
        }

        public void AddCenter(CenterBaseModel model)
        {
            if (!CentersData.ContainsKey(model.CenterName))
            {
                model.RowIndex = CentersData.Count;

                CentersData.Add(model.CenterName, model);

                // TO DO: Надо или нет?
                //AddAlternativeCenterData(model.CenterType, model.CenterName);
            }
        }

        public void AddAlternativeCenterData(Uco rc_type, Uco center)
        {
            if (!RCRoutesToListDic.ContainsKey(rc_type))
            {
                RCRoutesToListDic.Add(rc_type, new()); // Если нет вида, то создаем
            }

            if (!RCRoutesToListDic[rc_type].Contains(center))
            {
                RCRoutesToListDic[rc_type].Add(center); // Если нет этого центра то добавляем
            }
        }
    }
}

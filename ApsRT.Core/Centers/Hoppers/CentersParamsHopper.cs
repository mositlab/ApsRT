using ApsRT.Core.Centers.MasterData;
using ApsRT.Core.Hoppers;
using ApsRT.Core.Partias.Leftovers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;

namespace ApsRT.Core.Centers.Hoppers
{
    [Hopper(Label = "Характеристики РЦ", IsDirectory = false, IsMultiple = false, UsesGuid = false)]
    public class CentersParamsHopper : IHopperInterface
    {

        CentersMasterData CentersMasterData;

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {

        }

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            CentersMasterData = session.MasterDataLinks.CentersMasterData;
            CentersMasterData.ClearAll();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new CenterBaseModel();
            model.LoadFromDataRecord(data, options);
            CentersMasterData.AddCenter(model);
        }
    }
}

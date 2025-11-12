using ApsRT.Core.Hoppers;
using ApsRT.Core.Session;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;

namespace ApsRT.Core.Props
{
    [Hopper(Label = "Список реквизитов", IsDirectory = false, IsMultiple = true, UsesGuid = false, MustBeFilledWithData = true)]
    internal class PropsListHopper : IHopperInterface
    {

        PropsMasterData PropsMasterData;

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {

        }

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            PropsMasterData = session.MasterDataLinks.PropsMasterData;
            PropsMasterData.MatsPropNumberToIndex.Clear();
            PropsMasterData.MatsPropGuidToIndex.Clear();
            PropsMasterData.MatsPropStringToIndex.Clear();
            PropsMasterData.OpersPropNumberToIndex.Clear();
            PropsMasterData.OpersPropGuidToIndex.Clear();
            PropsMasterData.OpersPropStringToIndex.Clear();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new PropsDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            PropsMasterData.AddProp(model);
        }
    }
}

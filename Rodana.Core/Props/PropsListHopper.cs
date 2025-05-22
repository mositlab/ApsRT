using Rodana.Core.Hoppers;
using Rodana.Core.Session;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;

namespace Rodana.Core.Props
{
    [Hopper(Label = "Список реквизитов", LoadWhenOnlyPart = true, IsDirectory = false, IsMultiple = true, UsesGuid = false)]
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
            PropsMasterData.PropNumberToIndex.Clear();
            PropsMasterData.PropGuidToIndex.Clear();
            PropsMasterData.PropStringToIndex.Clear();
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

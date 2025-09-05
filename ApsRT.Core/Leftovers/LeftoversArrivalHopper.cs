using ApsRT.Core.Hoppers;
using ApsRT.Core.Session;
using System;
using System.Data;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Leftovers
{
    [Hopper(Label = "Поступление остатков", IsDirectory = false, IsMultiple = true, UsesGuid = true )]
    internal class LeftoversArrivalHopper : IHopperInterface
    {
        LeftoversMasterData LeftoversMasterData;

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {

        }

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            LeftoversMasterData = session.MasterDataLinks.LeftoversMasterData;
            LeftoversMasterData.ClearLeftoversArrivalData();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new LeftoverArrivalDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            LeftoversMasterData.AddLeftoverArrivalData(model);
        }
    }
}

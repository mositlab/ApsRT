using InfimumMES.Rechanges.Hoppers;
using ApsRT.Core.Centers.Hoppers;
using ApsRT.Core.Hoppers;
using ApsRT.Core.Orders;
using ApsRT.Core.Planning.ParallelPlanning;
using ApsRT.Core.Props;
using ApsRT.Core.Stages.MasterData;
using System;
using System.Data;
using UniversalPack;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using ApsRT.Core.Stages.Hoppers;

namespace ApsRT.Core.Partias.SubdivisionsTransfers
{
    [Hopper(Label = "Перемещение по цехам", IsDirectory = false, IsMultiple = false, UsesGuid = false, SessionPathAvailable = false)]
    public class SubdivisionsTransfersHopper : IHopperInterface
    {
        SubdivisionsTransfersMasterData SubdivisionsTransfersMasterData;

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            SubdivisionsTransfersMasterData = session.MasterDataLinks.SubdivisionsTransfersMasterData;
            SubdivisionsTransfersMasterData.ClearAll();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int RowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new SubdivisionsTransferDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            SubdivisionsTransfersMasterData.TryAddTypicalDuration(model);
        }

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {

        }
    }

}

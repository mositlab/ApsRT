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

namespace ApsRT.Core.RottingTime
{
    [Hopper(Label = "Время гниения", IsDirectory = false, IsMultiple = false, UsesGuid = false, SessionPathAvailable = false)]
    public class RottingTimeHopper : IHopperInterface
    {
        RottingTimeMasterData RottingTimeMasterData;

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            RottingTimeMasterData = session.MasterDataLinks.RottingTimeMasterData;
            RottingTimeMasterData.ClearAll();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int RowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new RottingTimeDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            RottingTimeMasterData.TryAddTypicalDuration(model);
        }

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {

        }
    }

}

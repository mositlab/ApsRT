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

namespace ApsRT.Core.Stages.Hoppers
{
    [Hopper(Label = "Длительности опытного ордера", IsDirectory = false, IsMultiple = false, UsesGuid = false, SessionPathAvailable = false)]
    public class StagesOrderDurationsHopper : IHopperInterface
    {
        StagesTypicalDurationsMasterData StagesTypicalDurationsMasterData;

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            StagesTypicalDurationsMasterData = session.MasterDataLinks.StagesTypicalDurationsMasterData;
            StagesTypicalDurationsMasterData.ClearOrderDurations();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int RowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new OrderDurationDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            //Console.Write(data.FieldCount);
            //Console.Write(": ");
            //for (int i = 0; i < data.FieldCount; i++)
            //{
            //    Console.Write(data[i].ToString());
            //    if (i < data.FieldCount - 1) Console.Write("; ");
            //}
            //model.Characteristics = Uco.EmptyObject;
            StagesTypicalDurationsMasterData.TryAddOrderDuration(model);
        }

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {
            StagesTypicalDurationsMasterData.PostProcessOrderDurations();
        }
    }

}

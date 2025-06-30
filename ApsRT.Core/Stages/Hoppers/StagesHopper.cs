using InfimumMES.Rechanges.Hoppers;
using ApsRT.Core.Centers.Hoppers;
using ApsRT.Core.Hoppers;
using ApsRT.Core.Orders;
using ApsRT.Core.Partias.Leftovers;
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
    [Hopper(Label = "Операции", IsDirectory = false, IsMultiple = false, UsesGuid = true,
        NeededHoppers = new[] { nameof(CentersParamsHopper), nameof(RechangesHopper), nameof(ParallelPlanningHopper), nameof(AlternativeCentersHopper), nameof(PropsListHopper) })]
    public class StagesHopper : IHopperInterface
    {
        Session.Session sessionLink;
        StagesMasterData StagesMasterData;

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            StagesMasterData = session.MasterDataLinks.StagesMasterData;
            sessionLink = session;
            StagesMasterData.ClearAll();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int RowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new Stage(sessionLink);
            var modelo = new OrderBaseModel();

            model.LoadFromDataRecord(data, options);
            modelo.LoadFromDataRecord(data, options);

            StagesMasterData.AddNewStage(model, modelo, data, options);
        }

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {

        }
    }

}

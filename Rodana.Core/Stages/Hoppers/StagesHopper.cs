using InfimumMES.Rechanges.Hoppers;
using Rodana.Core.Centers.Hoppers;
using Rodana.Core.Hoppers;
using Rodana.Core.Orders;
using Rodana.Core.Partias.Leftovers;
using Rodana.Core.Planning.ParallelPlanning;
using Rodana.Core.Stages.MasterData;
using System;
using System.Data;
using UniversalPack;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Stages.Hoppers
{
    [Hopper(Label = "Операции", IsDirectory = false, IsMultiple = false, UsesGuid = true,
        NeededHoppers = new[] { nameof(CentersParamsHopper), nameof(RechangesHopper), nameof(ParallelPlanningHopper), nameof(AlternativeCentersHopper) })]
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

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Rodana.Core.Centers.Hoppers;
using Rodana.Core.Centers.MasterData;
using Rodana.Core.Hoppers;
using Rodana.Core.Props;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Planning.ParallelPlanning
{
    [Hopper(Label = "Варианты загрузки пар. РЦ", LoadWhenOnlyPart = false, IsDirectory = false, IsMultiple = false, UsesGuid = false,
        NeededHoppers = new[] { nameof(CentersParamsHopper) })]
    public class ParallelPlanningHopper : IHopperInterface
    {
        ParallelPlanningMasterData ParallelPlanningMasterData;

        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {

        }

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            ParallelPlanningMasterData = session.MasterDataLinks.ParallelPlanningMasterData;
            ParallelPlanningMasterData.ClearAll();
        }

        public void BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        public void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new ParallelPlanningDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            ParallelPlanningMasterData.AddData(model);
        }
    }
}

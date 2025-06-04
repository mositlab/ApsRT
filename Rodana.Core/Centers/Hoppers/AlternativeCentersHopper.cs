using Rodana.Core.Centers.MasterData;
using Rodana.Core.Hoppers;
using Rodana.Core.Partias.Leftovers;
using Rodana.Core.Stages.Hoppers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Centers.Hoppers
{
    [Hopper(Label = "Альт. РЦ", IsDirectory = false, IsMultiple = false, UsesGuid = false, LoadWhenOnlyZadel = false,
        NeededHoppers = new[] { nameof(CentersParamsHopper) })]
    public class AlternativeCentersHopper : IHopperInterface
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
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new AlternativeCenterDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            CentersMasterData.AddAlternativeCenterData(model.CenterType, model.CenterName);
        }
    }
}

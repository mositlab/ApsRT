using Microsoft.Data.SqlClient;
using Rodana.Core.Hoppers;
using Rodana.Core.Stages.Hoppers;
using System.Data;
using System.Linq;
using UniversalPack.Common.DataLoader;

namespace Rodana.Core.Taras
{
    [Hopper(Label = "Макс намотки", IsDirectory = false,
        IsMultiple = false, UsesGuid = false, NeededHoppers = new[] { nameof(StagesHopper), nameof(UsedTaraHopper) /*"Opers", "UsedTara"*/ })]
    internal class MaxTaraHopper : IHopperInterface
    {

        TarasMasterData TarasMasterData;

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {

        }

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            TarasMasterData = session.MasterDataLinks.TarasMasterData;
            TarasMasterData.ClearMaxCapacities();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {
            if (options.SourceType == SourceType.SQL) options.GuidArrrangeType = GuidArrrangeType.CustomZMK;
        }

        void IHopperInterface.OnLoading(int RowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new MaxCapacityDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            TarasMasterData.AddMaxCapacityData(model);
        }
    }
}

using Microsoft.Data.SqlClient;
using ApsRT.Core.Hoppers;
using ApsRT.Core.Stages.Hoppers;
using System.Data;
using System.Linq;
using UniversalPack.Common.DataLoader;

namespace ApsRT.Core.Taras
{
    [Hopper(Label = "Макс намотки", IsDirectory = false, MustBeFilledWithData = true,
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

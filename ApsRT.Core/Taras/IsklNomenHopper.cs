using Microsoft.Data.SqlClient;
using ApsRT.Core.Centers.Hoppers;
using ApsRT.Core.Hoppers;
using ApsRT.Core.Stages.Hoppers;
using System.Data;
using UniversalPack.Common.DataLoader;
using UniversalPack.Extensions;

namespace ApsRT.Core.Taras
{
    [Hopper(Label = "Исключения номенклатур", IsDirectory = false, IsMultiple = false, UsesGuid = false,
        NeededHoppers = new[] { nameof(StagesHopper)/*пусть будет*/ })]
    internal class IsklNomenHopper : IHopperInterface
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
            TarasMasterData.IsklNomen.Clear();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int RowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new IsklNomenDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            TarasMasterData.AddIsklNomenData(model);
        }
    }
}

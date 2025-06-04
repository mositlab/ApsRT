using Microsoft.Data.SqlClient;
using Rodana.Core.Centers.Hoppers;
using Rodana.Core.Hoppers;
using Rodana.Core.Stages.Hoppers;
using System.Data;
using UniversalPack.Common.DataLoader;
using UniversalPack.Extensions;

namespace Rodana.Core.Taras
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

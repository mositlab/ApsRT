using Microsoft.Data.SqlClient;
using ApsRT.Core.Centers.Hoppers;
using ApsRT.Core.Hoppers;
using System.Data;
using UniversalPack.Common.DataLoader;
using UniversalPack.Extensions;

namespace ApsRT.Core.Taras
{
    [Hopper(Label = "Применяемая тара", IsDirectory = false,
        IsMultiple = false, UsesGuid = false, NeededHoppers = new[] { nameof(CentersParamsHopper), nameof(AlternativeCentersHopper)/*"CharsRC", "AltRCs"*/ })]
    internal class UsedTaraHopper : IHopperInterface
    {

        TarasMasterData TarasMasterData;
        Session.Session session;

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {
            TarasMasterData.UsedTaraPostProcess(session.SettingsController.CurrentSettings.UsePseudoTara);
        }

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            this.session = session;
            TarasMasterData = session.MasterDataLinks.TarasMasterData;
            TarasMasterData.FromAndToTara.Clear();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int RowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new UsedTaraDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            TarasMasterData.AddUsedTaraData(model, session.SettingsController.CurrentSettings.UsePseudoTara);
        }
    }
}

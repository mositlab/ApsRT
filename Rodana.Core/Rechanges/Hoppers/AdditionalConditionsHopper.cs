using UniversalPack.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rodana.Core.Rechanges.Hoppers;
using Rodana.Core.Hoppers;
using UniversalPack.Common.DataLoader;
using Rodana.Core.Session;
using Rodana.Core.Rechanges.MasterData;

namespace Rodana.Core.Rechanges.Hoppers
{
    [Hopper(Label = "Доп условия для переналадок", LoadWhenOnlyPart = false, IsDirectory = false, IsMultiple = false, UsesGuid = false,
        NeededHoppers = new[] {nameof(AdditionalParamsHopper) })]
    internal class AdditionalConditionsHopper : IHopperInterface
    {
        AdditionalConditionsMasterData AdditionalConditionsMasterData;

        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {
            AdditionalConditionsMasterData.BakeAdditionalConditions();
        }

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            AdditionalConditionsMasterData = session.MasterDataLinks.AdditionalConditionsMasterData;
        }

        public void BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        public void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new AdditionalConditionsDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            AdditionalConditionsMasterData.AddAdditionalCondition(model);
        }
    }
}

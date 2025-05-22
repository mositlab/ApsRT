using UniversalPack.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rodana.Core.Hoppers;
using UniversalPack.Common.DataLoader;
using Rodana.Core.Session;
using Rodana.Core.Rechanges.MasterData;

namespace Rodana.Core.Rechanges.Hoppers
{
    [Hopper(Label = "Список доп параметров", LoadWhenOnlyPart = false, IsDirectory = false, IsMultiple = false, UsesGuid = false)]
    internal class AdditionalParamsHopper : IHopperInterface
    {
        AdditionalConditionsMasterData AdditionalConditionsMasterData;
        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {
            AdditionalConditionsMasterData.BakeAdditionalParams();
        }

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            AdditionalConditionsMasterData = session.MasterDataLinks.AdditionalConditionsMasterData;
        }

        public void BeforeLoading(string filename, DataLoaderOptions options)
        {
            AdditionalConditionsMasterData.ClearAdditionalParams();
        }

        public void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new AdditionalParamsDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            AdditionalConditionsMasterData.AddAdditionalParam(model);
        }
    }
}

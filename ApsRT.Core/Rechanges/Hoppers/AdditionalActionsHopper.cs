using UniversalPack.Common;
using UniversalPack.Extensions;
using System.Data;
using System.Numerics;
using System;
using ApsRT.Core.Hoppers;
using UniversalPack.Common.DataLoader;
using ApsRT.Core.Session;
using Newtonsoft.Json.Linq;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Centers.Hoppers;

namespace ApsRT.Core.Rechanges.Hoppers
{
    [Hopper(Label = "Доп действия", LoadWhenOnlyPart = false, IsDirectory = false, IsMultiple = true, UsesGuid = false,
        NeededHoppers = new[] { nameof(CentersParamsHopper), nameof(RechangesCriteriasHopper), nameof(AdditionalParamsHopper) })]
    internal class AdditionalActionsHopper : IHopperInterface
    {
        AdditionalConditionsMasterData AdditionalConditionsMasterData;

        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {
            AdditionalConditionsMasterData.BakeAdditionalActions();
        }

        IEnumerable<string> rowNames;

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            AdditionalConditionsMasterData = session.MasterDataLinks.AdditionalConditionsMasterData;
            AdditionalConditionsMasterData.ClearAdditionalActions();
        }

        public void BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        public void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new AdditionalActionsDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            AdditionalConditionsMasterData.AddAdditionalAction(model);
        }
    }
}

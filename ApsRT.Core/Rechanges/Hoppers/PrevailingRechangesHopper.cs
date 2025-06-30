using UniversalPack.Common;
using UniversalPack.Extensions;
using System.Data;
using System.Numerics;
using System;
using InfimumMES.Rechanges.Hoppers;
using ApsRT.Core.Hoppers;
using UniversalPack.Common.DataLoader;
using ApsRT.Core.Session;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Centers.Hoppers;

namespace ApsRT.Core.Rechanges.Hoppers
{
    [Hopper(Label = "Превалирующие переналадки", LoadWhenOnlyPart = false, IsDirectory = false, IsMultiple = false, UsesGuid = false,
        NeededHoppers = new[] { nameof(CentersParamsHopper), nameof(RechangesHopper)/*"CharsRC", "Opers", "AltRCs"*/ })]
    internal class PrevailingRechangesHopper : IHopperInterface
    {
        RechangesMasterData RechangesMasterData;
        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {
            RechangesMasterData.BakePrevailingRechanges();
        }

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            RechangesMasterData = session.MasterDataLinks.RechangesMasterData;
            RechangesMasterData.ClearPrevailingRechanges();
        }

        public void BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        public void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new PrevailingRechangesDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            RechangesMasterData.AddPrevailingRechange(model);
        }
    }
}
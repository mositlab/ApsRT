using UniversalPack.Extensions;
using System.Data;
using System.Numerics;
using System;
using ApsRT.Core.Hoppers;
using ApsRT.Core.Session;
using UniversalPack.Common.DataLoader;
using ApsRT.Core.Rechanges.Hoppers;
using UniversalPack.Common;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Rechanges;
using ApsRT.Core.Centers.Hoppers;

namespace InfimumMES.Rechanges.Hoppers
{
    [Hopper(Label = "Переналадки", LoadWhenOnZadel = false, LoadWhenOnPart = false, IsDirectory = false, IsMultiple = true, MustBeFilledWithData = true, UsesGuid = false,
        NeededHoppers = new[] { nameof(CentersParamsHopper), nameof(RechangesCriteriasHopper)/*"CharsRC", "Opers", "AltRCs"*/ })]
    internal class RechangesHopper : IHopperInterface
    {

        RechangesMasterData RechangesMasterData;

        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {
            RechangesMasterData.BakeRechangesInfo();
            RechangesMasterData.BakeMultiCriterialRechangesInfo();
        }

        public void BeforeAllLoading(Session session, string[] filenames)
        {
            RechangesMasterData = session.MasterDataLinks.RechangesMasterData;
            RechangesMasterData.ClearRechangesInfo();
            RechangesMasterData.EraseMultiCriterialRechangesInfo();
        }

        public void BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        public void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new RechangeDataLoaderModel();
            model.LoadFromDataRecord(data, options);

            // Добавляем данные по переналадке в список условий
            RechangesMasterData.AddRechangeData(model);

            // Здесь не должно быть поиска критерия по идее, но пусть.
            if (model.MergeCriterias)
            {
                var index = RechangesMasterData.GetCriteriaIndexByName(model.Criteria);
                if (index > -1) RechangesMasterData.AddMultiCriterialRechangeInfo(model, index);
            }
        }
    }
}

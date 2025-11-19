using ApsRT.Core.Centers.Hoppers;
using ApsRT.Core.Hoppers;
using ApsRT.Core.Rechanges;
using ApsRT.Core.Rechanges.Hoppers;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Session;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.Numerics;
using UniversalPack.Common;
using UniversalPack.Common.DataLoader;
using UniversalPack.Extensions;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

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



            if ((string.IsNullOrEmpty(model.From) && !model.FromGuid.HasValue)
                || (string.IsNullOrEmpty(model.To) && !model.ToGuid.HasValue))
            {
                RechangesMasterData.SessionLink.AssignedLogger.LogQueue.Enqueue($"ЗАГРУЗКА (переналадки): {model.From} и {model.To} на строке {rowIndex}.\r\n");
                Console.WriteLine($"ЗАГРУЗКА (переналадки): {model.From} и {model.To} на строке {rowIndex}.\r\n");
                return;
            }

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

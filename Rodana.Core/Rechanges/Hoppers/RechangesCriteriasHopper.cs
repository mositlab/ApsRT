using Rodana.Core.Hoppers;
using Rodana.Core.Rechanges;
using Rodana.Core.Rechanges.MasterData;
using Rodana.Core.Session;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;
using UniversalPack.Common.DataLoader;
using UniversalPack.Extensions;

namespace Rodana.Core.Rechanges.Hoppers
{
    [Hopper(Label = "Критерии переналадок", LoadWhenOnlyPart = true, IsDirectory = false, IsMultiple = false, UsesGuid = false)]
    internal class RechangesCriteriasHopper : IHopperInterface
    {
        RechangesMasterData RechangesMasterData;

        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {
            RechangesMasterData.ExistingCriterias = list.ToArray();
        }

        List<RechangeUniversalCriteria> list;

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            RechangesMasterData = session.MasterDataLinks.RechangesMasterData;
            list = new();
        }

        public void BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        public void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new RechangesCriteriaDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            var criteria = new RechangeUniversalCriteria(model);
            list.Add(criteria);
        }
    }
}

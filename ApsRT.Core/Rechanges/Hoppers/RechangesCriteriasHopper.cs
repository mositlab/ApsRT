using ApsRT.Core.Hoppers;
using ApsRT.Core.Rechanges;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Session;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;
using UniversalPack.Common.DataLoader;
using UniversalPack.Extensions;

namespace ApsRT.Core.Rechanges.Hoppers
{
    [Hopper(Label = "Критерии переналадок", IsDirectory = false, IsMultiple = false, UsesGuid = false, MustBeFilledWithData = true)]
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

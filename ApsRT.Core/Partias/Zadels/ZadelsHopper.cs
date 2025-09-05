using ApsRT.Core.Hoppers;
using ApsRT.Core.Stages.Hoppers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;

namespace ApsRT.Core.Partias.Zadels
{
    [Hopper(Label = "Заделы", IsDirectory = false, IsMultiple = false, UsesGuid = true, LoadWhenOnPart = false, SessionPathAvailable = true,
        NeededHoppers = new[] { nameof(StagesHopper) })]
    internal class ZadelsHopper : IHopperInterface
    {
        ZadelsMasterData masterData;

        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {
            
        }

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            masterData = session.MasterDataLinks.ZadelsMasterData;
            masterData.ClearAll();
        }

        public void BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        public void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new ZadelDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            masterData.AddZadelData(model);
        }
    }
}

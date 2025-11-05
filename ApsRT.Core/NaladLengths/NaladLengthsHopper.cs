using ApsRT.Core.Centers.Hoppers;
using ApsRT.Core.Hoppers;
using ApsRT.Core.Stages.Hoppers;
using ApsRT.Core.Stages.MasterData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;
using UniversalPack.Extensions;

namespace ApsRT.Core.NaladLengths
{
    [Hopper(Label = "Наладочные длины", IsDirectory = false, IsMultiple = false, UsesGuid = false, MustBeFilledWithData = true, /*LoadWhenOnlyZadel = false,*/
        NeededHoppers = new[] { nameof(StagesHopper), nameof(AlternativeCentersHopper) })]
    public class NaladLengthsHopper : IHopperInterface
    {

        NaladLengthsMasterData NaladLengthsMasterData;

        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {

        }

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            NaladLengthsMasterData = session.MasterDataLinks.NaladLengthsMasterData;
            NaladLengthsMasterData.ClearAll();
        }

        public void BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        public void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new NaladLengthDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            NaladLengthsMasterData.AddNaladLengthData(model);
        }
    }
}

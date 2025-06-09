using Rodana.Core.Centers.Hoppers;
using Rodana.Core.Hoppers;
using Rodana.Core.Stages.Hoppers;
using Rodana.Core.Stages.MasterData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;
using UniversalPack.Extensions;

namespace Rodana.Core.NaladLengths
{
    [Hopper(Label = "Наладочные длины", IsDirectory = false, IsMultiple = false, UsesGuid = false, LoadWhenOnlyZadel = false,
        NeededHoppers = new[] { nameof(StagesHopper), nameof(AlternativeCentersHopper) })]
    public class NaladLengthsHopper : IHopperInterface
    {

        StagesMasterData StagesMasterData;
        NaladLengthsMasterData NaladLengthsMasterData;

        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {
            foreach (var op in StagesMasterData.AllStages) op.Value.CacheNaladLengths();
        }

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            StagesMasterData = session.MasterDataLinks.StagesMasterData;
            NaladLengthsMasterData = session.MasterDataLinks.NaladLengthsMasterData;
            NaladLengthsMasterData.ClearAll();
            foreach (var op in StagesMasterData.AllStages) op.Value.ClearNaladLengths();
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

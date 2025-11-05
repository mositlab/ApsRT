using ApsRT.Core.Hoppers;
using ApsRT.Core.Stages.MasterData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;

namespace ApsRT.Core.Stages.Hoppers
{
    [Hopper(Label = "Зависимости", IsDirectory = false, IsMultiple = false, UsesGuid = true, SessionPathAvailable = true, MustBeFilledWithData = true,
        NeededHoppers = new[] { nameof(StagesHopper) })]
    internal class DependenciesHopper : IHopperInterface
    {

        StagesMasterData StagesMasterData;

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {

        }

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            StagesMasterData = session.MasterDataLinks.StagesMasterData;
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new DependencyDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            StagesMasterData.AddDependenciesConnection(model);
        }
    }
}

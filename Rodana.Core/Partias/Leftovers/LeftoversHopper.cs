using Rodana.Core.Hoppers;
using Rodana.Core.Partias.Leftovers;
using Rodana.Core.Session;
using System;
using System.Data;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.Leftovers
{
    [Hopper(Label = "Остатки", LoadWhenOnlyPart = true, IsDirectory = false, IsMultiple = true, UsesGuid = false )]
    internal class LeftoversHopper : IHopperInterface
    {
        LeftoversMasterData LeftoversMasterData;

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {
            LeftoversMasterData.PostProcess();
        }

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            LeftoversMasterData = session.MasterDataLinks.LeftoversMasterData;
            LeftoversMasterData.ClearLeftoversInfo();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new LeftoverBaseModel();
            model.LoadFromDataRecord(data, options);
            LeftoversMasterData.AddLeftover(model);
        }
    }
}

using Rodana.Core.Hoppers;
using System;
using System.Data;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.Leftovers
{
    [Hopper(Label = "Группы заменяемости остатков", LoadWhenOnlyPart = true, IsDirectory = false, IsMultiple = true, UsesGuid = false )]
    internal class LeftoversGroupsHopper : IHopperInterface
    {
        LeftoversMasterData LeftoversMasterData;

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {

        }

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            LeftoversMasterData = session.MasterDataLinks.LeftoversMasterData;
            LeftoversMasterData.LeftoversGroupsInfo.Clear();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new LeftoverGroupDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            LeftoversMasterData.AddLeftoversGroupsInfo(model);
        }
    }
}

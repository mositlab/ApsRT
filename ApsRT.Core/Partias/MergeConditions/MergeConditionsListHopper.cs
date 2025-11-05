using ApsRT.Core.Hoppers;
using ApsRT.Core.Props;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;

namespace ApsRT.Core.Partias.MergeConditions
{
    [Hopper(Label = "Список усл. объединений", IsDirectory = false, IsMultiple = false, UsesGuid = false, MustBeFilledWithData = true, NeededHoppers = new string[] { nameof(PropsListHopper) })]
    internal class MergeConditionsListHopper : IHopperInterface
    {

        MergeConditionsMasterData MergeConditionsMasterData;
        PropsMasterData PropsMasterData;
        Session.Session session;

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {

        }

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            this.session = session;
            MergeConditionsMasterData = session.MasterDataLinks.MergeConditionsMasterData;
            PropsMasterData = session.MasterDataLinks.PropsMasterData;
            MergeConditionsMasterData.ClearMergeConditionsList();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new MergeConditionsListDataLoaderModel();
            model.LoadFromDataRecord(data, options);
            if (PropsMasterData.GenerateLinks(model.PropName, out var linker))
            {
                MergeConditionsMasterData.MergeConditionsGlobalList.Add(model.MergeConditionParamName, linker);
            }
            else
            {
                Console.WriteLine($"can't generate prop link for '{model.PropName}'");
                session.DataErrors.AddNewPartiaError(DataErrors.NewDataErrorType.CantLinkProp, model.MergeConditionParamName);
            }
        }
    }
}

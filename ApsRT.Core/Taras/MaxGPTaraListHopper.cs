using ApsRT.Core.Centers.Hoppers;
using ApsRT.Core.Hoppers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;

namespace ApsRT.Core.Taras
{
    [Hopper(Label = "Тара для фильрации", IsDirectory = false, MustBeFilledWithData = false,
        IsMultiple = false, UsesGuid = false)]
    internal class MaxGPTaraListHopper: IHopperInterface
    {

        TarasMasterData TarasMasterData;
        //Session.Session session;

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {

        }

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            //this.session = session;
            TarasMasterData = session.MasterDataLinks.TarasMasterData;
            TarasMasterData.MaxGPTaraList1.Clear();
            TarasMasterData.MaxGPTaraList2.Clear();
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int RowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new MaxGPTaraListModel();
            model.LoadFromDataRecord(data, options);
            if (model.Otbor) TarasMasterData.MaxGPTaraList1.Add(model.TaraType);
            if (model.Filter) TarasMasterData.MaxGPTaraList2.Add(model.TaraType);
        }


    }
}

using Rodana.Core.Hoppers;
using Rodana.Core.Session;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Props
{
    [Hopper(Label = "Список реквизитов", LoadWhenOnlyPart = true, IsDirectory = false, IsMultiple = true, UsesGuid = false)]
    internal class PropsListHopper<InputDataType, OrderSubType> : IHopperInterface<InputDataType, OrderSubType>
    {

        PropsMasterData PropsMasterData;

        void IHopperInterface<InputDataType, OrderSubType>.ActionFile(string filename)
        {

        }

        void IHopperInterface<InputDataType, OrderSubType>.AfterLoading()
        {

        }

        void IHopperInterface<InputDataType, OrderSubType>.BeforeAllLoading(Session<InputDataType, OrderSubType> session, string[] filenames)
        {
            PropsMasterData = session.MasterDataLinks.PropsMasterData;
            PropsMasterData.PropNumberToIndex.Clear();
            PropsMasterData.PropGuidToIndex.Clear();
            PropsMasterData.PropStringToIndex.Clear();
        }

        void IHopperInterface<InputDataType, OrderSubType>.BeforeLoading(string filename, HopperReadOptions options)
        {

        }

        void IHopperInterface<InputDataType, OrderSubType>.OnLoading(int rowIndex, IDataReader data, HopperReadOptions options)
        {
            //PropsMasterData.AddProp(
                
            //    data["ИмяРеквизита"].ConvertToOrDefault(string.Empty),
            //    data["Тип"].ConvertToOrDefault(string.Empty) switch
            //    {
            //        "Число" => 0,
            //        "Идентификатор" => 1,
            //        _ => 2
            //    }

            //    );
        }
    }
}

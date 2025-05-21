using UniversalPack.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfimumMES
{
    [Hopper(Label = "Список доп параметров", LoadWhenOnlyPart = false, IsDirectory = false, IsMultiple = false, UsesGuid = false)]
    internal class AdditionalParamsHopper : IHopperInterface
    {
        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {
            AdditionalConditionsMasterData.BakeAdditionalParams();
            ;
        }

        void IHopperInterface.BeforeAllLoading(string[] filenames)
        {

        }

        void IHopperInterface.BeforeLoading(string filename, HopperReadOptions options)
        {
            AdditionalConditionsMasterData.ClearAdditionalParams();
        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, HopperReadOptions options)
        {
            AdditionalConditionsMasterData.AddAdditionalParam(data["Параметр"].ConvertToOrDefault(string.Empty), data["Значение"].ConvertToOrDefault(string.Empty));
        }
    }
}

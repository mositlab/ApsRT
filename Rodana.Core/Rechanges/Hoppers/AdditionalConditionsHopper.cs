using UniversalPack.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfimumMES
{
    [Hopper(Label = "Доп условия для переналадок", LoadWhenOnlyPart = false, IsDirectory = false, IsMultiple = false, UsesGuid = false,
        NeededHoppers = new[] {nameof(AdditionalParamsHopper) })]
    internal class AdditionalConditionsHopper : IHopperInterface
    {
        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {
            AdditionalConditionsMasterData.BakeAdditionalConditions();
            ;
        }

        void IHopperInterface.BeforeAllLoading(string[] filenames)
        {

        }

        void IHopperInterface.BeforeLoading(string filename, HopperReadOptions options)
        {

        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, HopperReadOptions options)
        {
            AdditionalConditionsMasterData.AddAdditionalCondition(data["Переналадка_GUID"].ProcessGuid(options.GuidArrrangeType, true), data["Параметр"].ConvertToOrDefault(string.Empty), data["Значение"].ConvertToOrDefault(string.Empty));
        }
    }
}

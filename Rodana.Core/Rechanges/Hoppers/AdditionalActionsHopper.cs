using UniversalPack.Common;
using UniversalPack.Common.Planning;
using UniversalPack.Extensions;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Numerics;
using static InfimumMES.CentersMasterData;
using static InfimumMES.RechangesMasterData;
using System;
using Rodana.Core.Hoppers;

namespace InfimumMES
{
    [Hopper(Label = "Доп действия", LoadWhenOnlyPart = false, IsDirectory = false, IsMultiple = true, UsesGuid = false,
        NeededHoppers = new[] { nameof(CharsRCHopper), nameof(RechangesCriteriasHopper), nameof(AdditionalParamsHopper) })]
    internal class AdditionalActionsHopper<T, O> : IHopperInterface<T, O>
    {

        internal static void LoadActions(int I, 
            string criteria,
            string from, Guid fromGuid, string to, Guid toGuid,
            string Param, string Value,
            string RCCode, string RC)
        {
            CodedObject<string> center_int = new(RCCode, RC);
            if (!CentersData.ContainsKey(center_int))
            {
                return;//Centers.AddCenter(center);
            }

            AdditionalConditionsMasterData.AddAdditionalAction(center_int, criteria, from, fromGuid, to, toGuid, Param, Value);
        }

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {
            AdditionalConditionsMasterData.BakeAdditionalActions();
        }

        IEnumerable<string> rowNames;

        void IHopperInterface.BeforeAllLoading(string[] filenames)
        {
            AdditionalConditionsMasterData.ClearAdditionalActions();
        }

        void IHopperInterface.BeforeLoading(string filename, HopperReadOptions options)
        {

        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, HopperReadOptions options)
        {

            LoadActions(rowIndex,
            data["КритерийПереналадки"].ConvertToOrDefault(string.Empty),

            data["ПереходС"].ConvertToOrDefault(string.Empty),
            data["ПереходС_GUID"].ProcessGuid(options.GuidArrrangeType, true),
            data["ПереходНа"].ConvertToOrDefault(string.Empty),
            data["ПереходНа_GUID"].ProcessGuid(options.GuidArrrangeType, true),

            data["Параметр"].ConvertToOrDefault(string.Empty),
            data["Значение"].ConvertToOrDefault(string.Empty),

            data["РабочийЦентр_GUID"].ConvertToOrDefault(string.Empty),
            data["РабочийЦентр"].ConvertToOrDefault(string.Empty));
        }
    }
}

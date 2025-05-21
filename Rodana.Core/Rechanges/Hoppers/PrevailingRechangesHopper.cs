using UniversalPack.Common;
using UniversalPack.Common.Planning;
using UniversalPack.Extensions;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Numerics;
using static InfimumMES.CentersMasterData;
using static InfimumMES.RechangesMasterData;
using System;
using InfimumMES.Rechanges.Hoppers;

namespace InfimumMES
{
    [Hopper(Label = "Превалирующие переналадки", LoadWhenOnlyPart = false, IsDirectory = false, IsMultiple = false, UsesGuid = false,
        NeededHoppers = new[] { nameof(CharsRCHopper), nameof(RechangesHopper)/*"CharsRC", "Opers", "AltRCs"*/ })]
    internal class PrevailingRechangesHopper : IHopperInterface
    {
        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {
            RechangesMasterData.BakePrevailingRechanges();
        }

        public void BeforeAllLoading(string[] filenames)
        {
            RechangesMasterData.ClearPrevailingRechanges();
        }

        public void BeforeLoading(string filename, HopperReadOptions options)
        {

        }

        public void OnLoading(int rowIndex, IDataReader data, HopperReadOptions options)
        {
            RechangesMasterData.AddPrevailingRechange(data["Переналадка_GUID"].ProcessGuid(options.GuidArrrangeType, true), data["ПревалируетНад_GUID"].ProcessGuid(options.GuidArrrangeType, true), data["Уровень"].ConvertToOrDefault(1));
        }
    }
}
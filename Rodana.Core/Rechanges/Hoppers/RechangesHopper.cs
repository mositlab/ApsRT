using InfimumMES.Core.Common;
using InfimumMES.Core.Common.Planning;
using UniversalPack.Extensions;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Numerics;
using static InfimumMES.CentersMasterData;
using static InfimumMES.RechangesMasterData;
using System;

namespace InfimumMES.Rechanges.Hoppers
{
    [Hopper(Label = "Переналадки", LoadWhenOnlyPart = false, IsDirectory = false, IsMultiple = true, UsesGuid = false,
        NeededHoppers = new[] { nameof(CharsRCHopper), nameof(RechangesCriteriasHopper)/*"CharsRC", "Opers", "AltRCs"*/ })]
    internal class RechangesHopper : IHopperInterface
    {

        internal static void LoadRechanges(int I, string from, Guid fromGuid, string to, Guid toGuid, string criteria, Guid RechangeCode, string RechangeName, string TechOperCode, string TechOper, string RCCode, string RC, bool SpecReq, bool IsLaunch, int Time, bool MultiCriterial)
        {
            CodedObject<string> center_int = new(RCCode, RC);
            if (!CentersData.ContainsKey(center_int))
            {
                return;//Centers.AddCenter(center);
            }

            CodedObject<string> oper_int = new(TechOperCode, TechOper);

            var resultName = RechangeName.Trim();//.Replace(" ", "").Replace(" ", "");
            var result = new CodedObject<string>(RechangeCode, RechangeName);

            AddRechangeData(center_int, oper_int, criteria, from, fromGuid, to, toGuid, new Rechange(result, Time, SpecReq, IsLaunch, MultiCriterial));

            // Добавляем данные по переналадке в список условий

            // Здесь не должно быть поиска критерия по идее, но пусть.
            if (MultiCriterial)
            {
                var index = GetCriteriaIndexByName(criteria);
                if (index > -1)
                {
                    //if (center_int.Label == "EEL-40" && oper_int.Label == "010213 Наложение наруж.оболочки ПВХ мелких сеч.")
                    //{
                    //    ;
                    //}
                    AddMultiCriterialRechangeInfo(center_int, oper_int, result, index);
                }
            }
        }

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {
            BakeRechangesInfo();
            BakeMultiCriterialRechangesInfo();
        }

        IEnumerable<string> rowNames;

        void IHopperInterface.BeforeAllLoading(string[] filenames)
        {
            ClearRechangesInfo();
            EraseMultiCriterialRechangesInfo();
            rowNames = null;
        }

        void IHopperInterface.BeforeLoading(string filename, HopperReadOptions options)
        {

        }

        void IHopperInterface.OnLoading(int rowIndex, IDataReader data, HopperReadOptions options)
        {
            rowNames ??= data.GetColumnNames();
            if (string.IsNullOrEmpty(data["ТехнологическаяОперация"].ConvertToOrDefault(string.Empty)))
            {
                return;
            }

            LoadRechanges(rowIndex,
            data["ПереходС"].ConvertToOrDefault(string.Empty),
            data["ПереходС_GUID"].ProcessGuid(options.GuidArrrangeType, true),
            data["ПереходНа"].ConvertToOrDefault(string.Empty),
            data["ПереходНа_GUID"].ProcessGuid(options.GuidArrrangeType, true),
            data["КритерийПереналадки"].ConvertToOrDefault(string.Empty),
            data["ОперацияПереналадки_GUID"].ProcessGuid(options.GuidArrrangeType, true),
            data["ОперацияПереналадки"].ConvertToOrDefault(string.Empty),
            data["ТехнологическаяОперация_GUID"].ConvertToOrDefault(string.Empty),
            data["ТехнологическаяОперация"].ConvertToOrDefault(string.Empty),
            data["РабочийЦентр_GUID"].ConvertToOrDefault(string.Empty),
            data["РабочийЦентр"].ConvertToOrDefault(string.Empty),
            data["ДополнительноеУсловие"].ConvertToOrDefault(string.Empty).IsYes(),
            data["ПровоцируетЗапуск"].ConvertToOrDefault(string.Empty).IsYes(),
            data["ДлительностьПереналадки"].ConvertToOrDefault(string.Empty).RemoveWhitespaces().ParsePositiveIntOrDefault(),
            rowNames.Contains("ОбъединятьКритерии") ? data["ОбъединятьКритерии"].ConvertToOrDefault(string.Empty).IsYes() : false);
        }
    }
}

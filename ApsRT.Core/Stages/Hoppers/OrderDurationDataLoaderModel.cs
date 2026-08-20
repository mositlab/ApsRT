using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;
using ApsRT.Core.Session;
using ApsRT.Core.Settings;
using InfimumMES;
using ApsRT.Core.Rechanges.RechangesEngine;
using ApsRT.Core.Rechanges;
using ApsRT.Core.Rechanges.MasterData;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using UniversalPack.Common.DataLoader;
using UniversalPack.Attributes;
using UniversalPack;
using ApsRT.Core.Orders;
using ApsRT.Core.NaladLengths;
using System.Drawing;

namespace ApsRT.Core.Stages.Hoppers
{
    public class OrderDurationDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static OrderDurationDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<OrderDurationDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordCodedObjectColumn("РабочийЦентр_GUID", "РабочийЦентр")]
        public Uco Center { get; set; }

        [DataRecordCodedObjectColumn("ВидОперации_GUID", "ВидОперации")]
        public Uco OperationVid { get; set; }

        [DataRecordColumn("ОпытныйОрдер", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool ItsOrder { get; set; }

        [DataRecordColumn("СредняяДлительность")]
        public double Duration { get; set; }
    }
}

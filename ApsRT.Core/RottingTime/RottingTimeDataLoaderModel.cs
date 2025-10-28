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

namespace ApsRT.Core.RottingTime
{
    public class RottingTimeDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static RottingTimeDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<RottingTimeDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordCodedObjectColumn("Номенклатура_GUID", "Номенклатура")]
        public Uco Nomenklature { get; set; }

        [DataRecordCodedObjectColumn("Характеристика_GUID", "Характеристика")]
        public Uco Characteristics { get; set; }

        [DataRecordColumn("ДнейДоПотериСвойств")]
        public double AcceptableTime { get; set; }

        [DataRecordColumn("МаксимальныйСрокХранения")]
        public double MaxTime { get; set; }
    }
}

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
    public class DependencyDataLoaderModel: BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static DependencyDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<DependencyDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordColumn("ИдентификаторОперации")]
        public Guid ThisGuid { get; set; }

        [DataRecordColumn("СледующийИдентификаторОперации")]
        public Guid NextGuid { get; set; }

        [DataRecordColumn("ОперацияУчаствуетВРасчете", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool ThisIsInPlan { get; set; }

        [DataRecordColumn("СледующаяОперацияУчаствуетВРасчете", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool NextIsInPlan { get; set; }

        [DataRecordColumn("КоличествоМест", defaultIfFail: true)]
        public int PlacesCount { get; set; }

        [DataRecordColumn("ВспомогательнаяТара", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, defaultIfFail: true)]
        public bool VspomTara { get; set; }

        [DataRecordColumn("ДатаЗавершения", defaultIfFail: true)]
        public DateTime? WhenWasReady { get; set; }
    }
}

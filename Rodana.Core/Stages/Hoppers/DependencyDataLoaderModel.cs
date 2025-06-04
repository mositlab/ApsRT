using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;
using Rodana.Core.Session;
using Rodana.Core.Settings;
using InfimumMES;
using Rodana.Core.Rechanges.RechangesEngine;
using Rodana.Core.Rechanges;
using Rodana.Core.Rechanges.MasterData;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using Rodana.Core.Partias.Materials;
using UniversalPack.Common.DataLoader;
using UniversalPack.Attributes;
using UniversalPack;
using Rodana.Core.Orders;
using Rodana.Core.NaladLengths;
using System.Drawing;

namespace Rodana.Core.Stages.Hoppers
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
    }
}

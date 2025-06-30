using System;
using System.Collections.Generic;
using UniversalPack;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Taras
{
    /// <summary>
    /// Модель макс. намотки
    /// </summary>
    public sealed class MaxCapacityDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static MaxCapacityDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<MaxCapacityDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;


        [DataRecordCodedObjectColumn("Продукция_GUID", "Продукция")]
        public Uco Production { get; set; }

        [DataRecordCodedObjectColumn("Номенклатура_GUID", "Номенклатура")]
        public Uco Nomenklature { get; set; }

        [DataRecordCodedObjectColumn("ВидОперации_GUID", "ВидОперации_GUID")]
        public Uco OperationVid { get; set; }

        [DataRecordCodedObjectColumn("ТипТары_GUID", "ТипТары")]
        public Uco TaraType { get; set; }

        [DataRecordColumn("ЕдиницаХраненияОстатков", defaultIfFail: true)]
        public DimensionUnit DimensionUnit { get; set; }

        [DataRecordColumn("ВеличинаНамотки")]
        public double Amount { get; set; }

    }
}

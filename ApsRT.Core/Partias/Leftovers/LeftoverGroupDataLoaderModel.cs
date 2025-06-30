using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;

namespace ApsRT.Core.Partias.Leftovers
{
    /// <summary>
    /// Модель материала для МЕС 1.2
    /// </summary>
    internal class LeftoverGroupDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static LeftoverGroupDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<LeftoverGroupDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        /// <summary>
        /// Характеристика
        /// </summary>
        [DataRecordCodedObjectColumn("ОсновнойМатериалНоменклатура_GUID", "ОсновнойМатериалНоменклатура")]
        public Uco OriginalMaterialNomenklature { get; set; }
        [DataRecordCodedObjectColumn("ОсновнойМатериалХарактеристика_GUID", "ОсновнойМатериалХарактеристика")]
        public Uco OriginalMaterialCharacteristics { get; set; }

        [DataRecordColumn("КоличествоОсновногоМатериала")]
        public double OriginalCount { get; set; }

        /// <summary>
        /// Характеристика
        /// </summary>
        [DataRecordCodedObjectColumn("АналогНоменклатура_GUID", "АналогНоменклатура")]
        public Uco AnalogueMaterialNomenklature { get; set; }
        [DataRecordCodedObjectColumn("АналогХарактеристика_GUID", "АналогХарактеристика")]
        public Uco AnalogueMaterialCharacteristics { get; set; }

        [DataRecordColumn("КоличествоАналога")]
        public double AnalogueCount { get; set; }

        /// <summary>
        /// Номенклатура
        /// </summary>
        [DataRecordCodedObjectColumn("Номенклатура_GUID", "Номенклатура")]
        public Uco Nomenklature { get; set; }

        [DataRecordCodedObjectColumn("Спецификация_GUID", "Спецификация")]
        public Uco Specification { get; set; }

        [DataRecordCodedObjectVersionedColumn("Заказ_GUID", "Заказ")]
        public OrderID Order { get; set; }

        [DataRecordColumn("ГруппаЗаменяемостиМатериалов_GUID")]
        public Guid MaterialsGroup { get; set; }

    }
}

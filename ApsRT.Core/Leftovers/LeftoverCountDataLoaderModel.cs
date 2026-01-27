using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using UniversalPack.Common;

namespace ApsRT.Core.Leftovers
{
    /// <summary>
    /// Модель материала для МЕС 1.2
    /// </summary>
    public class LeftoverCountDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static LeftoverCountDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<LeftoverCountDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        /// <summary>
        /// Характеристика
        /// </summary>
        [DataRecordCodedObjectColumn("Характеристика_GUID", "Характеристика")]
        public Uco Characteristics { get; set; }

        /// <summary>
        /// Номенклатура
        /// </summary>
        [DataRecordCodedObjectColumn("Номенклатура_GUID", "Номенклатура")]
        public Uco Nomenklature { get; set; }

        [DataRecordColumn("Количество")]
        public double CountNow { get; set; }

        [DataRecordCodedObjectColumn("ЕдиницаИзмерения_GUID", "ЕдиницаИзмерения", isDimensionUnit: true)]
        public UcoDU EdIzm { get; set; }

        [DataRecordCodedObjectColumn("Подразделение_GUID", "Подразделение")]
        public Uco Subdivision { get; set; }

        [DataRecordCodedObjectVersionedColumn("Серия_GUID", "Серия", "ШтрихкодСерии", defaultIfFail: true)]
        public CodedObjectVersioned<Guid, string, string> Seria { get; set; }

    }
}

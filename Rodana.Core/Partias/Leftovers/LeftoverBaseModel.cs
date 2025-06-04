using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.Leftovers
{
    /// <summary>
    /// Модель материала для МЕС 1.2
    /// </summary>
    internal class LeftoverBaseModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static LeftoverBaseModel()
        {
            _fieldConverters = DefineFieldConverters<LeftoverBaseModel>();
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

        [DataRecordCodedObjectColumn("Подразделение_GUID", "Подразделение")]
        public Uco Subdivision { get; set; }

        [DataRecordColumn("ДатаПоступления", defaultIfFail: true)]
        public DateTime? DateArrival { get; set; }

        [DataRecordColumn("КоличествоПоступления", defaultIfFail: true)]
        public double CountArrival { get; set; }

    }
}

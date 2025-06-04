using Rodana.Core.Hoppers;
using System;
using System.Data;
using UniversalPack;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.Materials
{
    /// <summary>
    /// Модель материала для МЕС 1.2
    /// </summary>
    public class MaterialDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static MaterialDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<MaterialDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;


        /// <summary>
        /// Идентификатор операции
        /// </summary>
        [DataRecordColumn("ИдентификаторОперации")]
        public Guid StageGuid { get; set; }

        /// <summary>
        /// Код спецификации
        /// </summary>
        [DataRecordCodedObjectColumn("Спецификация_GUID", "Спецификация")]
        public Uco Specification { get; set; }

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

        /// <summary>
        /// Вид воспроизводства
        /// </summary>
        [DataRecordColumn("ВидВоспроизводства")]
        public string WorkType { get; set; }

        /// <summary>
        /// Количество
        /// </summary>
        [DataRecordColumn("Количество", defaultIfFail: true)]
        public double Count { get; set; }

        /// <summary>
        /// Единица измерения
        /// </summary>
        /// <remarks>
        /// Для этого поля определён другой конвертер
        /// </remarks>
        [DataRecordColumn("ЕдиницаИзмерения")]
        public DimensionUnit Unit { get; set; }
    }
}

using ApsRT.Core.Props;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using static UniversalPack.Attributes.DataRecordColumnAttribute;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Partias.MergeConditions
{
    /// <summary>
    /// Модель параметров объединения
    /// </summary>
    public class MergeConditionsFlowedDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static MergeConditionsFlowedDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<MergeConditionsFlowedDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        /// <summary>
        /// Подразделение
        /// </summary>
        /// <remarks>
        /// А это вообще нам нужно?
        /// </remarks>
        [DataRecordCodedObjectColumn("Подразделение_GUID", "Подразделение", true)]
        public Uco Subdivision { get; set; }

        /// <summary>
        /// GUID вида тех. операции
        /// </summary>
        [DataRecordCodedObjectColumn("ВидТехнологическойОперации_GUID", "ВидТехнологическойОперации", true)]
        public Uco OperationType { get; set; }

        /// <summary>
        /// Учитывать исходные комплектующие - проверять все материалы двух операций
        /// </summary>
        [DataRecordColumn("Настройка")]
        public string Param { get; set; }
    }
}

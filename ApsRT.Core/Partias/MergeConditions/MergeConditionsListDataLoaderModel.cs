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
    internal class MergeConditionsListDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static MergeConditionsListDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<MergeConditionsListDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordColumn("ИмяНастройки")]
        public string MergeConditionParamName { get; set; }

        [DataRecordColumn("Реквизит")]
        public string PropName { get; set; }
    }
}

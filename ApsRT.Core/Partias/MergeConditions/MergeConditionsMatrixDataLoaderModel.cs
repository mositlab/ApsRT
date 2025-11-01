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
    public class MergeConditionsMatrixDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static MergeConditionsMatrixDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<MergeConditionsMatrixDataLoaderModel>();
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
        [DataRecordColumn("УчитыватьИсходныеКомплектующие", CustomConversionModes.StringToBooleanIsYes)]
        public bool CheckMaterials { get; set; }

        /// <summary>
        /// Не учитывать макс намотки
        /// </summary>
        [DataRecordColumn("НеУчитыватьМаксимальныеНамотки", CustomConversionModes.StringToBooleanIsYes)]
        public bool DoNotCheckWindings { get; set; }

        /// <summary>
        /// Проверять РЦ маршрута
        /// </summary>
        [DataRecordColumn("УчитыватьПересеченияРЦМаршрута", CustomConversionModes.StringToBooleanIsYes)]
        public bool CheckWorkCenterRoute { get; set; }

        public List<PropsLinkerClass> PropsConditions { get; set; }
    }
}

using Rodana.Core.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Planning.ParallelPlanning
{
    public class ParallelPlanningDataLoaderModel: BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static ParallelPlanningDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<ParallelPlanningDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordCodedObjectColumn("РабочийЦентр_GUID", "РабочийЦентр")]
        public Uco CenterName { get; set; }

        [DataRecordCodedObjectColumn("ВидТехнологическойОперации_GUID", "ВидТехнологическойОперации")]
        public Uco OperationVid { get; set; }

        [DataRecordColumn("ИдентификаторПараллельнойЗагрузки")]
        public int ParallelID { get; set; }

        [DataRecordColumn("ДопустимаяДельта", defaultIfFail: true)]
        public double? Delta { get; set; }

        [DataRecordColumn("НеПовышатьТару", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool NotIncreaseTara { get; set; }

        [DataRecordCodedObjectColumn("ВариантЗагрузки_GUID", "ВариантЗагрузки")]
        public Uco Variant { get; set; }

        [DataRecordCodedObjectColumn("ТипТары_GUID", "ТипТары")]
        public Uco Tara { get; set; }

        [DataRecordColumn("КоличествоТарыШт", defaultIfFail: true)]
        public int? TaraCount { get; set; }

        [DataRecordColumn("МаксКоличество", defaultIfFail: true)]
        public int? MaxCapacity { get; set; }

        [DataRecordCodedObjectColumn("НаборХарактеристикОдновременнойЗагрузки_GUID", "НаборХарактеристикОдновременнойЗагрузки")]
        public Uco CharsList { get; set; }

        [DataRecordCodedObjectColumn("Характеристика_GUID", "Характеристика")]
        public Uco Char { get; set; }

        [DataRecordColumn("МинимальноеЗначение", defaultIfFail: true)]
        public double? Min { get; set; }

        [DataRecordColumn("МаксимальноеЗначение", defaultIfFail: true)]
        public double? Max { get; set; }

        [DataRecordColumn("Значение")]
        public string Value { get; set; }

        [DataRecordColumn("Значение_GUID", defaultIfFail: true)]
        public Guid? ValueGUID { get; set; }

        [DataRecordColumn("ИмяПоляТаблицыОперацийAPS")]
        public string ColumnName { get; set; }
    }
}

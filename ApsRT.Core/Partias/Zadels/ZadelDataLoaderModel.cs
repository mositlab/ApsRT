using ApsRT.Core.Rechanges.Hoppers;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using StageGUID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, int>;

namespace ApsRT.Core.Partias.Zadels
{
    internal class ZadelDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static ZadelDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<ZadelDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;


        [DataRecordColumn("ИдентификаторПартииПФ", defaultIfFail: true)]
        public Guid? PartiaPF { get; set; }

        //[DataRecordColumn("ИдентификаторЭтапа")]
        [DataRecordCodedObjectVersionedColumn("ИдентификаторЭтапа", "Регистратор", "ВерсияЭтапа", versionIsInt: true)]
        public StageGUID StageGuid { get; set; }

        [DataRecordColumn("Уровень", defaultIfFail: true)]
        public int? Level { get; set; }

        [DataRecordCodedObjectColumn("Номенклатура_GUID", "Номенклатура")]
        public Uco Nomenklature { get; set; }
        [DataRecordCodedObjectColumn("Характеристика_GUID", "Характеристика")]
        public Uco Characteristics { get; set; }

        [DataRecordColumn("НомерПовива", defaultIfFail: true)]
        public int? Poviv { get; set; }

        [DataRecordColumn("ИдентификаторЗадела", defaultIfFail: true)]
        public Guid? ZadelGuid { get; set; }

        [DataRecordColumn("ШтрихкодТары", defaultIfFail: true)]
        public Guid? SeriaCode { get; set; }

        [DataRecordCodedObjectColumn("ТипТары_GUID", "ТипТары")]
        public Uco Tara { get; set; }

        [DataRecordColumn("КоличествоЗадела", defaultIfFail: true)]
        public double Count { get; set; }
    }
}
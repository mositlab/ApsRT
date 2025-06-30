using ApsRT.Core.Rechanges.Hoppers;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

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


        [DataRecordColumn("ИдентификаторПартииПФ")]
        public Guid? PartiaPF { get; set; }

        [DataRecordColumn("ИдентификаторЭтапа")]
        public Guid StageGuid { get; set; }

        [DataRecordColumn("Уровень")]
        public int Level { get; set; }

        [DataRecordCodedObjectColumn("Номенклатура_GUID", "Номенклатура")]
        public Uco Nomenklature { get; set; }

        [DataRecordColumn("НомерПовива", defaultIfFail: true)]
        public int Poviv { get; set; }

        [DataRecordColumn("ИдентификаторЗадела")]
        public Guid ZadelGuid { get; set; }

        [DataRecordCodedObjectColumn("ТипТары_GUID", "ТипТары")]
        public Uco Tara { get; set; }

        [DataRecordColumn("КоличествоЗадела", defaultIfFail: true)]
        public double Count { get; set; }
    }
}
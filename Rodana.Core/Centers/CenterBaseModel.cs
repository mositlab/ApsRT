using Newtonsoft.Json;
using Rodana.Core.Partias.Leftovers;
using UniversalPack.Attributes;
using UniversalPack.Common;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Centers
{
    public class CenterBaseModel: BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static CenterBaseModel()
        {
            _fieldConverters = DefineFieldConverters<CenterBaseModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;


        [DataRecordCodedObjectColumn("РабочийЦентр_GUID", "РабочийЦентр")]
        public Uco CenterName { get; set; }

        [DataRecordCodedObjectColumn("ВидРабочегоЦентра_GUID", "ВидРабочегоЦентра")]
        public Uco CenterType { get; set; }

        [DataRecordCodedObjectColumn("Подразделение_GUID", "Подразделение")]
        public Uco Subdivision { get; set; }

        [DataRecordColumn("ПланированиеВспомогательнойОперации", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        public bool AbleToVspom { get; set; } // умеет ли обрабатывать вспомогательные

        [DataRecordColumn("НеМенятьТару", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        internal bool NotChangeTara { get; set; }


        internal bool IsWithRechanges;
        internal UniversalBinaryStorage PossibleRechanges;
        internal bool HasAdditionalActions;
        internal UniversalBinaryStorage PossibleAdditionalActions;

        internal bool CanBeParallel;
        public int? MaxMinutesAwaiting; // макс время ожидания

        public int RowIndex;

    }
}

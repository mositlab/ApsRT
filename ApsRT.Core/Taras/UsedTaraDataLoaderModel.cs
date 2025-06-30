using System;
using System.Collections.Generic;
using UniversalPack;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using static UniversalPack.Attributes.DataRecordColumnAttribute;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Taras
{
    public sealed class UsedTaraDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static UsedTaraDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<UsedTaraDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordCodedObjectColumn("РабочийЦентр_GUID", "РабочийЦентр")]
        public Uco CenterName { get; set; }

        [DataRecordCodedObjectColumn("ТехнологическаяОперация_GUID", "ТехнологическаяОперация")]
        public Uco TechOperation { get; set; }

        [DataRecordCodedObjectColumn("ТипТары_GUID", "ТипТары")]
        public Uco TaraType { get; set; }

        /// <summary>
        /// Отдающая
        /// </summary>
        [DataRecordColumn("Отдающая", CustomConversionModes.StringToBooleanIsYes)]
        public bool IsOutput { get; set; }

        /// <summary>
        /// Приёмная
        /// </summary>
        [DataRecordColumn("Приемная", CustomConversionModes.StringToBooleanIsYes)]
        public bool IsInput { get; set; }

        /// <summary>
        /// Приёмная
        /// </summary>
        [DataRecordColumn("Вспомогательная", CustomConversionModes.StringToBooleanIsYes)]
        public bool IsVspom { get; set; }
    }
}

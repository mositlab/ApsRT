using System;
using System.Collections.Generic;
using UniversalPack;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Taras
{
    /// <summary>
    /// Модель макс. намотки
    /// </summary>
    public sealed class IsklNomenDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static IsklNomenDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<IsklNomenDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordCodedObjectColumn("Номенклатура_GUID", "Номенклатура")]
        public Uco Nomenklature { get; set; }

        [DataRecordCodedObjectColumn("ВидТехнологическойОперации_GUID", "ВидТехнологическойОперации")]
        public Uco OperationVid { get; set; }

        [DataRecordColumn("НеПроверятьРасценку", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool NotCheckPrice { get; set; }

        [DataRecordColumn("НеПроверятьНамотку", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool NotCheckMax { get; set; }

    }
}

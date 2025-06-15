using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Rechanges.Hoppers
{
    public class RechangeDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static RechangeDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<RechangeDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordColumn("КритерийПереналадки")]
        public string Criteria { get; set; }

        [DataRecordColumn("ПереходС")]
        public string From { get; set; }

        [DataRecordColumn("ПереходС_GUID", defaultIfFail: true)]
        public Guid? FromGuid { get; set; }

        [DataRecordColumn("ПереходНа")]
        public string To { get; set; }

        [DataRecordColumn("ПереходНа_GUID", defaultIfFail: true)]
        public Guid? ToGuid { get; set; }

        [DataRecordCodedObjectColumn("РабочийЦентр_GUID", "РабочийЦентр")]
        public Uco Center { get; set; }

        [DataRecordCodedObjectColumn("ОперацияПереналадки_GUID", "ОперацияПереналадки")]
        public Uco RechangeOperation { get; set; }

        [DataRecordCodedObjectColumn("ТехнологическаяОперация_GUID", "ТехнологическаяОперация")]
        public Uco Operation { get; set; }

        [DataRecordColumn("ДополнительноеУсловие", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool AdditionalCondition { get; set; }

        [DataRecordColumn("ПровоцируетЗапуск", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool ProvokesLaunch { get; set; }

        [DataRecordColumn("ОбъединятьКритерии", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool MergeCriterias { get; set; }

        [DataRecordColumn("ДлительностьПереналадки")]
        public int Duration { get; set; }

    }
}

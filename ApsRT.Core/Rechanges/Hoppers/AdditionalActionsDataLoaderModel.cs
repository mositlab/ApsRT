using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;
using UniversalPack.Common;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Rechanges.Hoppers
{
    public class AdditionalActionsDataLoaderModel: BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static AdditionalActionsDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<AdditionalActionsDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordColumn("КритерийПереналадки")]
        public string Criteria { get; set; }

        [DataRecordColumn("ПереходС")]
        public string From { get; set; }

        [DataRecordColumn("ПереходС_GUID")]
        public Guid FromGuid { get; set; }

        [DataRecordColumn("ПереходНа")]
        public string To { get; set; }

        [DataRecordColumn("ПереходНа_GUID")]
        public Guid ToGuid { get; set; }

        [DataRecordColumn("Параметр")]
        public string Parameter { get; set; }

        [DataRecordColumn("Значение")]
        public string Value { get; set; }

        [DataRecordCodedObjectColumn("РабочийЦентр_GUID", "РабочийЦентр")]
        public Uco Center { get; set; }
    }
}

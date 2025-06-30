using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;

namespace ApsRT.Core.Rechanges.Hoppers
{
    public class AdditionalConditionsDataLoaderModel: BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static AdditionalConditionsDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<AdditionalConditionsDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordColumn("Переналадка_GUID")]
        public Guid Rechange { get; set; }

        [DataRecordColumn("Параметр")]
        public string Parameter { get; set; }

        [DataRecordColumn("Значение")]
        public string Value { get; set; }
    }
}

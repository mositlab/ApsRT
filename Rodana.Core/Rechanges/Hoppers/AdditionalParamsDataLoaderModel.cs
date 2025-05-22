using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;

namespace Rodana.Core.Rechanges.Hoppers
{
    public class AdditionalParamsDataLoaderModel: BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static AdditionalParamsDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<AdditionalParamsDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordColumn("Параметр")]
        public string Parameter { get; set; }

        [DataRecordColumn("Значение")]
        public string Value { get; set; }
    }
}

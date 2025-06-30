using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;

namespace ApsRT.Core.Props
{
    public class PropsDataLoaderModel: BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static PropsDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<PropsDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordColumn("ИмяРеквизита")]
        public string PropName { get; set; }

        [DataRecordColumn("Тип", DataRecordColumnAttribute.CustomConversionModes.StringToTypeIndex)]
        public byte PropType { get; set; }

    }
}

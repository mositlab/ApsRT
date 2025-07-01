using Newtonsoft.Json;
using UniversalPack.Attributes;
using UniversalPack.Common;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Centers.Hoppers
{
    public class AlternativeCenterDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static AlternativeCenterDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<AlternativeCenterDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;


        [DataRecordCodedObjectColumn("РабочийЦентр_GUID", "РабочийЦентр")]
        public Uco CenterName { get; set; }

        [DataRecordCodedObjectColumn("ВидРабочегоЦентра_GUID", "ВидРабочегоЦентра")]
        public Uco CenterType { get; set; }

    }
}

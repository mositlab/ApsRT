using Newtonsoft.Json;
using Rodana.Core.Partias.Leftovers;
using UniversalPack.Attributes;
using UniversalPack.Common;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Centers.Hoppers
{
    public class WorkingIntervalDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static WorkingIntervalDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<WorkingIntervalDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;


        [DataRecordCodedObjectColumn("РабочийЦентр_GUID", "РабочийЦентр")]
        public Uco CenterName { get; set; }

        [DataRecordColumn("Начало")]
        public DateTime Start { get; set; }

        [DataRecordColumn("Окончание")]
        public DateTime End { get; set; }

    }
}

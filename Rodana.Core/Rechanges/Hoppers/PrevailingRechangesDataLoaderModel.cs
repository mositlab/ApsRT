using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;

namespace Rodana.Core.Rechanges.Hoppers
{
    internal class PrevailingRechangesDataLoaderModel: BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static PrevailingRechangesDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<PrevailingRechangesDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordColumn("Переналадка_GUID")]
        public Guid PrevailingRechange { get; set; }

        [DataRecordColumn("ПревалируетНад_GUID")]
        public Guid SubRechange { get; set; }

        [DataRecordColumn("Уровень")]
        public int Level { get; set; }

    }
}

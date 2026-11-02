using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using static UniversalPack.Attributes.DataRecordColumnAttribute;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Taras
{
    /// <summary>
    /// Это модель для загрузки ПЕРВОГО списка - на какие тары реагировать АПС.
    /// </summary>
    internal class MaxGPTaraListModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static MaxGPTaraListModel()
        {
            _fieldConverters = DefineFieldConverters<MaxGPTaraListModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;


        [DataRecordCodedObjectColumn("ТипТары_GUID", "ТипТары")]
        public Uco TaraType { get; set; }

        [DataRecordColumn("ИспользуетсяДляОтбора", CustomConversionModes.StringToBooleanIsYes)]
        public bool Otbor { get; set; }

        [DataRecordColumn("ИспользуетсяПриФильтрации", CustomConversionModes.StringToBooleanIsYes)]
        public bool Filter { get; set; }
    }
}

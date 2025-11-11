using ApsRT.Core.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;

namespace ApsRT.Core.NaladLengths
{
    public class NaladLengthDataLoaderModel : BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static NaladLengthDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<NaladLengthDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordCodedObjectColumn("РабочийЦентр_GUID", "РабочийЦентр")]
        public Uco Center { get; set; }

        [DataRecordCodedObjectColumn("ВидТехнологическойОперации_GUID", "ВидТехнологическойОперации")]
        public Uco OperationType { get; set; }

        [DataRecordCodedObjectColumn("Номенклатура_GUID", "Номенклатура")]
        public Uco Nomenklature { get; set; }
        [DataRecordCodedObjectColumn("Характеристика_GUID", "Характеристика")]
        public Uco Characteristics { get; set; }

        [DataRecordColumn("НаладочнаяДлина")]
        public double Length { get; set; }

        [DataRecordCodedObjectColumn("ЕдиницаИзмеренияКлассификатор_GUID", "ЕдиницаИзмеренияКлассификатор", isDimensionUnit: true)]
        public UcoDU DimensionUnitClassificator { get; set; }

        [DataRecordColumn("СечениеОт", defaultIfFail: true)]
        public double ParameterFrom { get; set; }

        [DataRecordColumn("СечениеДо", defaultIfFail: true)]
        public double ParameterTo { get; set; }

        [DataRecordColumn("ПерваяТараЗапуска", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool IsFirst { get; set; }

        [DataRecordColumn("ПоследующиеТарыЗапуска", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool IsNext { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;

namespace ApsRT.Core.Rechanges.Hoppers
{
    public class RechangesCriteriaDataLoaderModel: BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static RechangesCriteriaDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<RechangesCriteriaDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;


        [DataRecordColumn("Критерий")]
        public string Name { get; set; }

        [DataRecordColumn("ЭтоЧисло", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool CanBeNumber { get; set; }

        [DataRecordColumn("ЭтоДиапазон", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool CanBeDiapazon { get; set; }

        [DataRecordColumn("ЭтоСтрока", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool CanBeString { get; set; }

        [DataRecordColumn("ЭтоИдентификатор", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool CanBeGuid { get; set; }

        [DataRecordColumn("Многозначный", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool CanBeMultiple { get; set; }

        [DataRecordColumn("Сортировать", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool Sort { get; set; }

        [DataRecordColumn("ЭтоМетодAPS", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool IsLink { get; set; }

        [DataRecordColumn("Динамичный", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool IsDynamic { get; set; }

        [DataRecordColumn("ИспользуетсяВНаладочныхДлинах", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool UsedInNaladLengths { get; set; }

        [DataRecordColumn("ИндексМетода", defaultIfFail: true)]
        public int? MethodLinkIndex { get; set; }

        [DataRecordColumn("ИмяМетодаAPS")]
        public string MethodLinkName { get; set; }

        [DataRecordColumn("ИменаСтолбцовТаблицыОперацийЗначения")]
        public string ColumnValueNames { get; set; }

        [DataRecordColumn("ИменаСтолбцовТаблицыОперацийМетки")]
        public string ColumnLabelNames { get; set; }

    }
}

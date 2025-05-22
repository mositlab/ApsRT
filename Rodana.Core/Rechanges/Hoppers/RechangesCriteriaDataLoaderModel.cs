using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;

namespace Rodana.Core.Rechanges.Hoppers
{
    public class RechangesCriteriaDataLoaderModel: BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static RechangesCriteriaDataLoaderModel()
        {
            _fieldConverters = DefineFieldConverters<RechangesCriteriaDataLoaderModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;


        [DataRecordColumn("Наименование")]
        internal string Name { get; set; }

        [DataRecordColumn("ЭтоЧисло", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        internal bool CanBeNumber { get; set; }

        [DataRecordColumn("ЭтоДиапазон", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        internal bool CanBeDiapazon { get; set; }

        [DataRecordColumn("ЭтоСтрока", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        internal bool CanBeString { get; set; }

        [DataRecordColumn("ЭтоИдентификатор", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        internal bool CanBeGuid { get; set; }

        [DataRecordColumn("Многозначный", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        internal bool CanBeMultiple { get; set; }

        [DataRecordColumn("Сортировать", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        internal bool Sort { get; set; }

        [DataRecordColumn("ЭтоСсылка", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        internal bool IsLink { get; set; }

        [DataRecordColumn("Динамичный", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        internal bool IsDynamic { get; set; }

        [DataRecordColumn("ИндексМетода")]
        internal int MethodLinkIndex { get; set; }

        [DataRecordColumn("ИменаСтолбцовТаблицыОперацийЗначения")]
        internal string CalumnValueNames { get; set; }

        [DataRecordColumn("ИменаСтолбцовТаблицыОперацийМетки")]
        internal string CalumnLabelNames { get; set; }

    }
}

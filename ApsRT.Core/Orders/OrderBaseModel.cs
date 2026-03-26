using ApsRT.Core.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;

namespace ApsRT.Core.Orders
{
    public class OrderBaseModel: BaseDataLoaderModel
    {
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static OrderBaseModel()
        {
            _fieldConverters = DefineFieldConverters<OrderBaseModel>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;

        [DataRecordCodedObjectVersionedColumn("Заказ_GUID", "Заказ", "ПартияГП")]
        public OrderID OrderID { get; set; }

        public string ShortName { get; set; }

        [DataRecordColumn("ДатаИсполненияПоЗаказу")]
        public DateTime Deadline { get; set; }

        [DataRecordColumn("ПриоритетЗаказа", DataRecordColumnAttribute.CustomConversionModes.StringToPriority)]
        public int Priority { get; set; } // приоритет, нужен в планировании

        [DataRecordColumn("ИдентификаторПартииПФ")]
        public Guid? PartiaPF { get; set; } // Если этот айдишник не указан, то эта партия - элементарная

        public HashSet<int> RelatedOpers = new();
    }
}

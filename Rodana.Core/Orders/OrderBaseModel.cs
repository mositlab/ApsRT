using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Attributes;

namespace Rodana.Core.Orders
{
    public class OrderBaseModel
    {
        //Заказ, ПартияГП
        public OrderID OrderID { get; set; }

        [DataRecordColumn("ДатаИсполненияПоЗаказу")]
        public DateTime Deadline { get; set; }

        [DataRecordColumn("ПриоритетЗаказа", DataRecordColumnAttribute.CustomConversionModes.StringToPriority)]
        public int Priority { get; set; } // приоритет, нужен в планировании

        [DataRecordColumn("ИдентификаторПартииПФ")]
        public Guid? PartiaPF { get; set; } // Если этот айдишник не указан, то эта партия - элементарная

        public HashSet<int> RelatedOpers = new();
    }
}

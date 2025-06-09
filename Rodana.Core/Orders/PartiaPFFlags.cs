using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rodana.Core.Orders.OrderBaseModel;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;

namespace Rodana.Core.Orders
{
    /// <summary>
    /// Флаги партии полуфабриката
    /// </summary>
    public class PartiaPFFlags
    {
        /// <summary>
        /// Зафиксирована
        /// </summary>
        public bool Fixed { get; set; }

        /// <summary>
        /// Включена в ПЗ
        /// </summary>
        public bool VPZ { get; set; }

        /// <summary>
        /// Ручное формирование партии
        /// </summary>
        public bool IsUserHandled { get; set; }

        /// <summary>
        /// Отражение факта
        /// </summary>
        public bool IsFact { get; set; }

        /// <summary>
        /// Партия заблокирована
        /// </summary>
        public bool IsNotFree => Fixed || VPZ || IsUserHandled || IsFact;

        /// <summary>
        /// Партия особая - но только если речь про OnlyPart, там другие условия.
        /// </summary>
        public bool IsNotOnlyPartFree => Fixed || VPZ || IsFact;

        /// <summary>
        /// Это список ГПшек, которые к этой связи относятся. Нужно для быстрых вычислений в конце вывода партий.
        /// </summary>
        public HashSet<OrderID> RelatedGPs;

        public override string ToString()
        {
            return (Fixed ? "N" : "!N") + "+" + (VPZ ? "V" : "!V") + "+" + (IsUserHandled ? "R" : "!R") + "+" + (IsFact ? "F" : "!F");
        }
    }
}

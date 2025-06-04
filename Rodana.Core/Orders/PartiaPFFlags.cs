using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rodana.Core.Orders.OrderBaseModel;

namespace Rodana.Core.Orders
{
    /// <summary>
    /// Флаги партии полуфабриката
    /// </summary>
    internal class PartiaPFFlags
    {
        /// <summary>
        /// Зафиксирована
        /// </summary>
        internal bool Fixed { get; set; }

        /// <summary>
        /// Включена в ПЗ
        /// </summary>
        internal bool VPZ { get; set; }

        /// <summary>
        /// Ручное формирование партии
        /// </summary>
        internal bool IsUserHandled { get; set; }

        /// <summary>
        /// Отражение факта
        /// </summary>
        internal bool IsFact { get; set; }

        /// <summary>
        /// Партия заблокирована
        /// </summary>
        internal bool IsNotFree => Fixed || VPZ || IsUserHandled || IsFact;

        /// <summary>
        /// Партия особая - но только если речь про OnlyPart, там другие условия.
        /// </summary>
        internal bool IsNotOnlyPartFree => Fixed || VPZ || IsFact;

        /// <summary>
        /// Это список ГПшек, которые к этой связи относятся. Нужно для быстрых вычислений в конце вывода партий.
        /// </summary>
        internal HashSet<OrderID> RelatedGPs;

        public override string ToString()
        {
            return (Fixed ? "N" : "!N") + "+" + (VPZ ? "V" : "!V") + "+" + (IsUserHandled ? "R" : "!R") + "+" + (IsFact ? "F" : "!F");
        }
    }
}

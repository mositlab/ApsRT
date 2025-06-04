using UniversalPack.Common;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Orders
{
    public struct OrderID
    {
        private Uco _part1;
        private string _part2;
        private int _hashCode;

        public Uco OrderName
        {
            readonly get => _part1;
            set
            {
                _part1 = value;
                UpdateHashCode();
            }
        }

        public string Version
        {
            readonly get => _part2;
            set
            {
                _part2 = value;
                UpdateHashCode();
            }
        }

        public OrderID(Uco part1, string part2)
        {
            _part1 = part1;
            _part2 = part2;
            _hashCode = default;

            UpdateHashCode();
        }

        /// <summary>
        /// Однократное обновление хеша объекта
        /// </summary>
        private void UpdateHashCode()
        {
            _hashCode = HashCode.Combine(_part1, _part2);
        }

        public override readonly string ToString()
        {
            if (OrderName.IsEmpty)
            {
                return string.Empty;
            }
            else
            {
                return $"{OrderName}, версия {Version}";
            }
        }

        public static bool operator ==(OrderID left, OrderID right)
        {
            return left._hashCode == right._hashCode;
        }

        public static bool operator !=(OrderID left, OrderID right)
        {
            return left._hashCode != right._hashCode;
        }

        //public static explicit operator OrderID(CodedObject a) => new OrderID(a, -1);

        public override readonly bool Equals(object obj)
        {
            return obj is OrderID orderId && _hashCode == orderId._hashCode;
        }

        public override readonly int GetHashCode()
        {
            return _hashCode;
        }

        static OrderID _emptyOrder = new OrderID(Uco.EmptyObject, "");
        public static OrderID EmptyOrder
        {
            get => _emptyOrder;
        }
    }
}

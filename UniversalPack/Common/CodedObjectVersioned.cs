using UniversalPack.Common;

namespace UniversalPack.Common
{
    public struct CodedObjectVersioned<InputDataTupe, LabelType, VersionType>
    {
        private CodedObject<InputDataTupe, LabelType> _part1;
        private VersionType _part2;
        private int _hashCode;

        public CodedObject<InputDataTupe, LabelType> Object
        {
            readonly get => _part1;
            set
            {
                _part1 = value;
                UpdateHashCode();
            }
        }

        public VersionType Version
        {
            readonly get => _part2;
            set
            {
                _part2 = value;
                UpdateHashCode();
            }
        }

        public CodedObjectVersioned(CodedObject<InputDataTupe, LabelType> part1, VersionType part2)
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
            if (Object.IsEmpty)
            {
                return string.Empty;
            }
            else
            {
                return $"{Object}, версия {Version}";
            }
        }

        public static bool operator ==(CodedObjectVersioned<InputDataTupe, LabelType, VersionType> left, CodedObjectVersioned<InputDataTupe, LabelType, VersionType> right)
        {
            return left._hashCode == right._hashCode;
        }

        public static bool operator !=(CodedObjectVersioned<InputDataTupe, LabelType, VersionType> left, CodedObjectVersioned<InputDataTupe, LabelType, VersionType> right)
        {
            return left._hashCode != right._hashCode;
        }

        //public static explicit operator OrderID(CodedObject a) => new OrderID(a, -1);

        public override readonly bool Equals(object obj)
        {
            return obj is CodedObjectVersioned<InputDataTupe, LabelType, VersionType> orderId && _hashCode == orderId._hashCode;
        }

        public override readonly int GetHashCode()
        {
            return _hashCode;
        }

        static CodedObjectVersioned<InputDataTupe, LabelType, VersionType> _emptyObject = new CodedObjectVersioned<InputDataTupe, LabelType, VersionType>(CodedObject<InputDataTupe, LabelType>.EmptyObject, default(VersionType));
        public static CodedObjectVersioned<InputDataTupe, LabelType, VersionType> EmptyObject
        {
            get => _emptyObject;
        }
    }
}

using Newtonsoft.Json;

namespace UniversalPack.Common
{
    public struct CodedObject<InputDataType, ValueType>
    {
        private InputDataType _code;
        private ValueType _label;
        private int _hashCode;
        private bool _isEmpty;

        [JsonProperty]
        public InputDataType Code
        {
            get => _code;
            set
            {
                _code = value;
                _isEmpty = EqualityComparer<InputDataType>.Default.Equals(_code, default);
                UpdateHashCode();
            }
        }

        [JsonProperty]
        public ValueType Label
        {
            get => _label;
            set
            {
                _label = value;
                UpdateHashCode();
            }
        }

        public CodedObject(InputDataType code, ValueType label)
        {
            _code = code;
            _isEmpty = EmptyCheck();
            _label = label;
            _hashCode = default;
            UpdateHashCode();
        }

        /// <summary>
        /// Обновление хэша
        /// </summary>
        private void UpdateHashCode()
        {
            _hashCode = HashCode.Combine(_code);
        }

        public bool EmptyCheck()
        {
            _isEmpty = EqualityComparer<InputDataType>.Default.Equals(_code, default);
            return _isEmpty;
        }

        public readonly bool IsEmpty => _isEmpty;

        public static bool operator ==(CodedObject<InputDataType, ValueType> a, CodedObject<InputDataType, ValueType> b) => a._hashCode == b._hashCode;

        public static bool operator !=(CodedObject<InputDataType, ValueType> a, CodedObject<InputDataType, ValueType> b) => a._hashCode != b._hashCode;

        public override string ToString()
        {
            return $"{Label} [{Code}]" + (IsEmpty ? " [пуст.]" : "");
        }

        public override bool Equals(object obj)
        {
            //return _code.Equals(obj);
            return obj is CodedObject<InputDataType, ValueType> codedObject && codedObject.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            return _code.GetHashCode();
            //UpdateHashCode();
            return _hashCode;
        }

        static CodedObject<InputDataType, ValueType> _emptyObject = new CodedObject<InputDataType, ValueType>(default(InputDataType), default(ValueType));
        public static CodedObject<InputDataType, ValueType> EmptyObject
        {
            get => _emptyObject;
        }
    }

}

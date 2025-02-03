using Newtonsoft.Json;

namespace Rodana.Core.Common
{
    public struct CodedObject<InputDataType>
    {
        private InputDataType _code;
        private string _label;
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
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                UpdateHashCode();
            }
        }

        public CodedObject(InputDataType code, string label)
        {
            _code = code;
            _isEmpty = EqualityComparer<InputDataType>.Default.Equals(_code, default);
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

        public readonly bool IsEmpty => _isEmpty;

        public static bool operator ==(CodedObject<InputDataType> a, CodedObject<InputDataType> b) => a._hashCode == b._hashCode;

        public static bool operator !=(CodedObject<InputDataType> a, CodedObject<InputDataType> b) => a._hashCode != b._hashCode;

        public override string ToString()
        {
            return $"{Label} [{Code}]";
        }

        public override bool Equals(object obj)
        {
            return obj is CodedObject<InputDataType> codedObject && codedObject._hashCode == _hashCode;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }

}

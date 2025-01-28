using Newtonsoft.Json;

namespace Rodana.Core.Common
{
    public struct CodedObject<C, L>
    {
        private C _code;
        private L _label;
        private int _hashCode;
        private bool _isEmpty;

        [JsonProperty]
        public C Code
        {
            get => _code;
            set
            {
                _code = value;
                _isEmpty = EqualityComparer<C>.Default.Equals(_code, default);
                UpdateHashCode();
            }
        }

        [JsonProperty]
        public L Label
        {
            get => _label;
            set
            {
                _label = value;
                UpdateHashCode();
            }
        }

        public CodedObject(C code, L label)
        {
            _code = code;
            _isEmpty = EqualityComparer<C>.Default.Equals(_code, default);
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

        public static bool operator ==(CodedObject<C, L> a, CodedObject<C, L> b) => a._hashCode == b._hashCode;

        public static bool operator !=(CodedObject<C, L> a, CodedObject<C, L> b) => a._hashCode != b._hashCode;

        public override string ToString()
        {
            return $"{Label} [{Code}]";
        }

        public override bool Equals(object obj)
        {
            return obj is CodedObject<C, L> codedObject && codedObject._hashCode == _hashCode;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }

}

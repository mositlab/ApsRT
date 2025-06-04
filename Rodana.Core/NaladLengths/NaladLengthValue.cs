using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;

namespace Rodana.Core.NaladLengths
{
    public struct NaladLengthValue
    {
        public NaladLengthValue(float value, UcoDU edIzm)
        {
            Value = value;
            EdIzm = edIzm;
        }

        public float Value { get; set; }
        public UcoDU EdIzm { get; set; }

        public override string ToString()
        {
            return $"{Value} {EdIzm}";
        }

        public static bool operator ==(NaladLengthValue left, NaladLengthValue right)
        {
            return left.Value == right.Value && left.EdIzm == right.EdIzm;
        }
        public static bool operator !=(NaladLengthValue left, NaladLengthValue right)
        {
            return !(left == right);
        }

        public static NaladLengthValue operator +(NaladLengthValue left, NaladLengthValue right)
        {
            left.Value += right.Value;
            return left;
        }

        public static NaladLengthValue operator +(NaladLengthValue left, float right)
        {
            left.Value += right;
            return left;
        }

    }
}

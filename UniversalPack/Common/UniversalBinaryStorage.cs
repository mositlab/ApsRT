using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace UniversalPack.Common
{
    /// <summary>
    /// Хранилище битов. Быстрее, проще, функциональнее BitArray.
    /// </summary>
    public class UniversalBinaryStorage
    {
        public UniversalBinaryStorage(UniversalBinaryStorage BinaryBase)
        {
            BinaryData = BinaryBase.BinaryData;
            BinaryDataMask = BinaryBase.BinaryDataMask;
            UpdateHashCode();
        }

        public UniversalBinaryStorage(int binaryData, int binaryDataMask)
        {
            BinaryData = binaryData;
            BinaryDataMask = binaryDataMask;
            UpdateHashCode();
        }

        public static int AvailableSize => 8 * sizeof(int);

        public override string ToString()
        {
            return Convert.ToString(BinaryData, 2) + $" [{Convert.ToString(BinaryDataMask, 2)}]";
        }

        public UniversalBinaryStorage(): this(0,0) { }

        int hashCode { get; set; }
        void UpdateHashCode()
        {
            hashCode = BinaryData.GetHashCode();
        }

        int BinaryData { get; set; }
        int BinaryDataMask { get; set; }

        public bool IsEmpty { get => BinaryDataMask == 0; }

        public bool FullEquality(UniversalBinaryStorage other)
        {
            return BinaryData == other.BinaryData && BinaryDataMask == other.BinaryDataMask;
        }

        /// <summary>
        /// Проверяет, равны ли данные в этом хранилище и данные из другого, причём с учётом альфа-маски другого. То есть, проверяются только те биты, которые нужны другому.
        /// </summary>
        /// <param name="other">Другое хранилище</param>
        /// <returns></returns>
        public bool IsEqualWithThis(UniversalBinaryStorage other)
        {
            return (BinaryData & other.BinaryDataMask) == other.BinaryData;
        }

        public override bool Equals(object obj)
        {
            return obj is UniversalBinaryStorage other && hashCode == other.hashCode;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public void DeactivateBit(int BitIndex)
        {
            BinaryDataMask &= ~(1 << BitIndex);
        }

        public void DeactivateByte(int BitIndex)
        {
            for (int i = 0; i < 8; i++) DeactivateBit(BitIndex + i);
        }

        public void DeactivateQarter(int BitIndex)
        {
            for (int i = 0; i < 2; i++) DeactivateBit(BitIndex + i);
        }

        public void SetData(int BitIndex, byte Value, BinaryStorageDataSize Size)
        {
            switch (Size)
            {
                case BinaryStorageDataSize.Bit: SetDataBit(BitIndex, Value == 1); break;
                case BinaryStorageDataSize.Quarter: SetDataQuarter(BitIndex, Value); break;
                case BinaryStorageDataSize.Byte: SetDataByte(BitIndex, Value); break;
            }
        }

        public void SetDataQuarter(int BitIndex, byte Value)
        {
            for (int i = 0; i < 2; i++)
                SetDataBit(BitIndex + i, ((Value >> i) & 1) != 0);
        }

        public void SetDataByte(int BitIndex, byte Value)
        {
            for (int i = 0; i < 8; i++)
                SetDataBit(BitIndex + i, ((Value >> i) & 1) != 0);
        }

        /// <summary>
        /// Задать значение бита по нужному адресу
        /// </summary>
        /// <param name="BitIndex"></param>
        /// <param name="Value"></param>
        public void SetDataBit(int BitIndex, bool Value)
        {
            if (Value)
                BinaryData |= (1 << BitIndex);
            else
                BinaryData &= ~(1 << BitIndex);

            BinaryDataMask |= (1 << BitIndex);

            UpdateHashCode();
        }

        public byte GetDataQarter(int BitIndex)
        {
            int data = 0;
            for (int i = 0; i < 2; i++)
                data |= ((GetDataBit(BitIndex + i) ? 1 : 0) << i);
            return (byte)data;
        }

        public byte GetDataByte(int BitIndex)
        {
            int data = 0;
            for (int i = 0; i < 8; i++)
                data |= ((GetDataBit(BitIndex + i) ? 1 : 0) << i);
            return (byte)data;
        }

        /// <summary>
        /// Получить значение бита по нужному адресу
        /// </summary>
        /// <param name="BitIndex"></param>
        /// <returns></returns>
        public bool GetDataBit(int BitIndex)
        {
            return ((BinaryData >> BitIndex) & 1) != 0;
        }
    }

}

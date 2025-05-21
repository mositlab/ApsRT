using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using UniversalPack.Common;

namespace Rodana.Core.Rechanges
{
    public static class UniversalBinaryStorageExtensions
    {

        internal static void SetData(this UniversalBinaryStorage storage, AdditionalAction action)
        {
            var BitIndex = action.BitIndex;
            var Value = action.ValueToSet;
            switch (action.DataSize)
            {
                case BinaryStorageDataSize.Bit: storage.SetDataBit(BitIndex, Value == 1); break;
                case BinaryStorageDataSize.Quarter: storage.SetDataQuarter(BitIndex, Value); break;
                case BinaryStorageDataSize.Byte: storage.SetDataByte(BitIndex, Value); break;
            }
        }
    }
}

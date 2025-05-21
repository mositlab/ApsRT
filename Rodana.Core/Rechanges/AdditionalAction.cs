using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;

namespace Rodana.Core.Rechanges
{
    internal class AdditionalAction
    {
        public AdditionalAction(byte bitIndex, byte valueToSet, BinaryStorageDataSize dataSize)
        {
            BitIndex = bitIndex;
            ValueToSet = valueToSet;
            DataSize = dataSize;
        }

        internal byte BitIndex { get; }
        internal byte ValueToSet { get; }
        internal BinaryStorageDataSize DataSize { get; }
    }
}

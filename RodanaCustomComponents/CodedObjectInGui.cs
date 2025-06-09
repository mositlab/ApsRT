using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;

namespace RodanaCustomComponents
{
    public class CodedObjectInGui
    {
        public Guid ValueIndex { get; set; }
        public string StringLabel { get; set; }

        public CodedObjectInGui(CodedObject<Guid, string> obj)
        {
            ValueIndex = obj.Code;
            StringLabel = obj.Label;
        }

        public CodedObjectInGui(Guid valueIndex, string stringLabel)
        {
            ValueIndex = valueIndex;
            StringLabel = stringLabel;
        }
    }
}

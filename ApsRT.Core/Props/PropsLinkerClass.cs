using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Props
{

    /// <summary>
    /// Простенький класс для хранения в настройках партий и прочем, чтоб проще обращаться с реквизитами.
    /// </summary>
    public class PropsLinkerClass
    {
        public PropsLinkerClass(string propLabel, int table, int numberIndex, int guidIndex, int stringIndex)
        {
            NumberIndex = numberIndex;
            TableType = table;
            GuidIndex = guidIndex;
            StringIndex = stringIndex;
            PropLabel = propLabel;
        }

        public override string ToString()
        {
            return $"{PropLabel} [{NumberIndex},{GuidIndex},{StringIndex}]";
        }

        internal string PropLabel { get; }

        internal int TableType { get; }

        internal int NumberIndex { get; } = -1;
        internal int GuidIndex { get; } = -1;
        internal int StringIndex { get; } = -1;
    }
}

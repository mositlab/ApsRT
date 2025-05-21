using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Hoppers
{
    public class HopperReadOptions
    {

        public SourceType SourceType { get; }

        public GuidArrrangeType GuidArrrangeType { get; set; }

        public HopperReadOptions(SourceType type)
        {
            SourceType = type;
        }

    }

    public enum GuidArrrangeType
    {
        DoNothing,
        Endian,
        CustomZMK
    }

    public enum SourceType
    {
        TXT,
        CSV,
        XLSX,
        SQL,
        ADO
    }
}

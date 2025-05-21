using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalPack.Common.DataLoader
{
    public class DataLoaderOptions
    {

        public SourceType SourceType { get; }

        public GuidArrrangeType GuidArrrangeType { get; set; }

        public DataLoaderOptions(SourceType type)
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

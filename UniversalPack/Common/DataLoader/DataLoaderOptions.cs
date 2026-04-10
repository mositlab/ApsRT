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

        public IColumnNameProvider UsedColumnNameProvider { get; }

        public DataLoaderOptions(SourceType type, IColumnNameProvider provider)
        {
            SourceType = type;
            UsedColumnNameProvider = provider;
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

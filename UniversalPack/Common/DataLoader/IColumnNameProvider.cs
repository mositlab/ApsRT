using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalPack.Common.DataLoader
{
    public interface IColumnNameProvider
    {
        public string GetName(string rawPropertyColumnName);
    }
}

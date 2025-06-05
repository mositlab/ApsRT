using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Partias.Zadels
{
    internal enum ZadelStatus
    {
        CannotBeFound,
        SussesfullyUsed,
        NotEnoughCount,
        AnotherUsed, // закрыто другим заделом
        HigherZadelUsed // более высокий задел его перебил
    }
}

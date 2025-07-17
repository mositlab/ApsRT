using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.NaladLengths
{
    public enum RequiredNaladCondition: int
    {
        FirstLaunch = 0, // первый запуск
        //OtherLaunch, // второй запуск
        OtherLaunch = 1 // любой запуск
    }
}

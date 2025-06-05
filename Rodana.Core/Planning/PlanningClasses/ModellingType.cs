using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Planning.PlanningClasses
{
    public enum ModellingType
    {
        Elementar, // Если в нём лишь исходный этап
        FromMES, // Он пришёл из меса
        AfterPartii, // Его создала сама апс
        Obch_ETAP_FromMES, // ЭТО ОБЩИЙ ЭТАП мать его за ногу, причем из меса
        Obch_ETAP_AfterPartii // ЭТО ОБЩИЙ ЭТАП мать его за ногу, пришедший из меса
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.PlanSimulator
{
    public enum PlanningModellingType
    {
        Elementar, // Если в нём лишь исходный этап
        FromMES, // Он пришёл из меса
        AfterPartias, // Его создала сама апс
        EasyFlagFromMES, // ЭТО ОБЩИЙ ЭТАП мать его за ногу, причем из меса
        EasyFlagAfterPartias // ЭТО ОБЩИЙ ЭТАП мать его за ногу, пришедший из меса
    }
}

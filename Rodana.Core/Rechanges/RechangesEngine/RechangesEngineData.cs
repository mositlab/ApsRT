using UniversalPack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Rechanges.RechangesEngine
{
    /// <summary>
    /// Данные для движка переналадок, где может содержаться много чего интересного, как минимум - данные о рабочем центре, об операциях и прочем
    /// </summary>
    public class RechangesEngineData
    {
        public RechangesEngineData(CenterInPlanData centerData, DatesDataClass prostoiData, Stages.Stage operationA, Stages.Stage operationB, Uco actualRC)
        {
            CenterData = centerData;
            ProstoiData = prostoiData;
            ObjectA = operationA;
            ObjectB = operationB;
            ActualRC = actualRC;
        }

        public CenterInPlanData CenterData { get; }
        public DatesDataClass ProstoiData { get; }
        public Stages.Stage ObjectA { get; }
        public Stages.Stage ObjectB { get; }
        public Uco ActualRC { get; }
    }
}

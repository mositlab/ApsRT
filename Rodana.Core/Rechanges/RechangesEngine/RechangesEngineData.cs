using UniversalPack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Rechanges.RechangesEngine
{
    /// <summary>
    /// Данные для движка переналадок, где может содержаться много чего интересного, как минимум - данные о рабочем центре, об операциях и прочем
    /// </summary>
    public class RechangesEngineData<InputDataType, OrderSubType>
    {
        public RechangesEngineData(CenterInPlanData centerData, DatesDataClass prostoiData, Stages.Stage<InputDataType, OrderSubType> operationA, Stages.Stage<InputDataType, OrderSubType> operationB, CodedObject<InputDataType, string> actualRC)
        {
            CenterData = centerData;
            ProstoiData = prostoiData;
            ObjectA = operationA;
            ObjectB = operationB;
            ActualRC = actualRC;
        }

        public CenterInPlanData CenterData { get; }
        public DatesDataClass ProstoiData { get; }
        public Stages.Stage<InputDataType, OrderSubType> ObjectA { get; }
        public Stages.Stage<InputDataType, OrderSubType> ObjectB { get; }
        public CodedObject<InputDataType, string> ActualRC { get; }
    }
}

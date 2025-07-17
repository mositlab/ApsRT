using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;

namespace ApsRT.Core.PlanSimulator
{
    /// <summary>
    /// РЦ, который уже в плане считается, это данные для него
    /// </summary>
    public class PlanSimulatorCenterData // struct
    {
        public PlanSimulatorMod LastMod;
        public PlanSimulatorBlock LastBlock;
        public DateTime LastDate; // дата конца загрузки
        public DateTime LastStartDate; // дата начала последнего блока

        public int NextNonWorkingInterval;

        //public CashedInterval NonWorkingIntervalData;

        public UniversalBinaryStorage AdditinalConditions;

        public int Flag;

        public int LocalFlag;

        public TimeSpan OpersTime, RechangesTime, ProstoiTime;

        public TimeSpan TotalTime => OpersTime + RechangesTime + ProstoiTime;

        public PlanSimulatorCenterData Copy() // Копируем структуру. При подсчёте это нужно
        {
            var copy = new PlanSimulatorCenterData()
            {
                LastMod = LastMod,
                LastBlock = LastBlock,
                LastDate = LastDate,
                LastStartDate = LastStartDate,
                NextNonWorkingInterval = NextNonWorkingInterval,
                //NextFixingIndex = this.NextFixingIndex,
                //HasToProcessAdditionalConditions = this.HasToProcessAdditionalConditions,
                //Params = this.Params == null ? null : new(this.Params),
                Flag = Flag,
                LocalFlag = LocalFlag,
                OpersTime = OpersTime,
                RechangesTime = RechangesTime,
                ProstoiTime = ProstoiTime,
                AdditinalConditions = new UniversalBinaryStorage(AdditinalConditions)
            };
            return copy;
        }
    }
}

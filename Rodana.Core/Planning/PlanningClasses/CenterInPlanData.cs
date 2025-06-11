using Rodana.Core.Planning.PlanningClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;

namespace Rodana.Core.Planning.PlanningClasses
{
    /// <summary>
    /// РЦ, который уже в плане считается, это данные для него
    /// </summary>
    public class CenterInPlanData // struct
    {
        public PlanningModelling LastMod;
        public OpersBlock LastBlock;
        public DateTime LastDate; // дата конца загрузки
        public DateTime LastStartDate; // дата начала последнего блока

        public int NextNonWorkingInterval;

        //public CashedInterval NonWorkingIntervalData;

        public UniversalBinaryStorage AdditinalConditions;

        public int Flag;

        public int LocalFlag;

        public TimeSpan OpersTime, RechangesTime, ProstoiTime;

        public TimeSpan TotalTime => OpersTime + RechangesTime + ProstoiTime;

        public CenterInPlanData Copy() // Копируем структуру. При подсчёте это нужно
        {
            var copy = new CenterInPlanData()
            {
                LastMod = this.LastMod,
                LastBlock = this.LastBlock,
                LastDate = this.LastDate,
                LastStartDate = this.LastStartDate,
                //NextFixingIndex = this.NextFixingIndex,
                //HasToProcessAdditionalConditions = this.HasToProcessAdditionalConditions,
                //Params = this.Params == null ? null : new(this.Params),
                Flag = this.Flag,
                LocalFlag = this.LocalFlag,
                OpersTime = this.OpersTime,
                RechangesTime = this.RechangesTime,
                ProstoiTime = this.ProstoiTime,
                AdditinalConditions = new UniversalBinaryStorage(this.AdditinalConditions)
            };
            return copy;
        }
    }
}

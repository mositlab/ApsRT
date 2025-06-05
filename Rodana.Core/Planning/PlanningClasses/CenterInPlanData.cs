using Rodana.Core.Planning.PlanningClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;

namespace Rodana.Core.PlanningClasses
{
    /// <summary>
    /// РЦ, который уже в плане считается, это данные для него
    /// </summary>
    public class CenterInPlanData // struct
    {
        internal PlanningModelling LastMod;
        internal OpersBlock LastBlock;
        internal DateTime LastDate; // дата конца загрузки
        internal DateTime LastStartDate; // дата начала последнего блока

        internal int NextNonWorkingInterval;

        //internal CashedInterval NonWorkingIntervalData;

        internal UniversalBinaryStorage AdditinalConditions;

        internal int Flag;

        internal int LocalFlag;

        internal TimeSpan TotalTime, RechangesTime, ProstoiTime;

        internal CenterInPlanData Copy() // Копируем структуру. При подсчёте это нужно
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
                TotalTime = this.TotalTime,
                RechangesTime = this.RechangesTime,
                ProstoiTime = this.ProstoiTime,
                AdditinalConditions = new UniversalBinaryStorage(this.AdditinalConditions)
            };
            return copy;
        }
    }
}

using ApsRT.Core.Planning.PlanningClasses;
using ApsRT.Core.Planning.PlanningClasses.PoolClasses;
using ApsRT.Core.Rechanges.RechangesEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;

namespace ApsRT.Core.Planning.PlanningCalculatorEngine
{
    public class PlanningCalculatorEngineResult
    {
        public int GlobalFlag;

        public int CurrentRank = 0;

        public TimeSpan RechangesSeconds { get; set; } // ЕСТЬ

        /// <summary>
        /// Дата окончания самого загруженного по переналадкам РЦ
        /// </summary>
        public DateTime GlobalPlanEndDate { get; set; } // ЕСТЬ

        /// <summary>
        /// Дата окончания всего плана вообще
        /// </summary>
        public DateTime MostRechangedRCEndDate { get; set; } // ЕСТЬ
        public TimeSpan ProstoiSeconds { get; set; } // ЕСТЬ
        public double DaysRuinedValue { get; set; } // ЕСТЬ
        public double DaysRuinedLinearValue { get; set; } // ЕСТЬ
        public double RuinedCount { get; set; } // ЕСТЬ

        public float NaladLengthsTotal { get; set; } // ЕСТЬ

        public double TaraRedutantUsage { get; set; } // 5 - размотка тары
        public double BadOffsets { get; set; } // Это разница от предыдущего времени

        public int TaraErrorsCount { get; set; }


        /// <summary>
        /// Сколько было переходов из цеха в цех
        /// </summary>
        public int SubdivisionsTransfer { get; set; }

        public int CatchedNaladLengthsErrorsCount { get => (ModellingsWithBrokenFreeNaladLengths?.Count ?? 0) + (ModellingsWithBrokenFixedNaladLengths?.Count ?? 0); }

        /// <summary>
        /// Здесь записаны все моделирования, которые имеют длины больше допустимого
        /// </summary>
        public HashSet<Guid> ModellingsWithBrokenFixedNaladLengths;

        /// <summary>
        /// Здесь записаны все моделирования, которые не влезают на тару
        /// </summary>
        public HashSet<Guid> ModellingsWithBrokenFreeNaladLengths;


        //public IDictionary<CodedObject<string>, CenterTimesClass> CentersTimes;

        public IDictionary<int, (RechangesEngineResult RechangeItself, CachedInterval Interval, int LastOperID)> RechangesBeforeFirstStages;

        public IDictionary<int, (DatesPoolClass ProstoiItself, CachedInterval Interval)> ProstoiBeforeFirstStages;

        public IDictionary<int, DateTime> FirstStagesStartDate;

        public IDictionary<int, CachedInterval> FirstStagesFullDurations;

        public IDictionary<int, int> FirstStagesRanks;

        public IDictionary<int, int> FirstStagesPositions;

        public IDictionary<int, CenterInPlanData> FirstStagesAdditionalInfos;

        /// <summary>
        /// Дата начала плана. Должна быть только для чтения, чтобы не ломать логику.
        /// </summary>
        public DateTime TimeOfStart { get; }

        /// <summary>
        /// Позволяет получить время, затраченное на изготовление продукции. По факту, это длительность всего плана.
        /// </summary>
        public TimeSpan ProductionSeconds => GlobalPlanEndDate - TimeOfStart;

        /// <summary>
        /// Позволяет получить время занятости от окончания самого загруженного по переналадкам РЦ.
        /// </summary>
        public TimeSpan MostRCSeconds => GlobalPlanEndDate - TimeOfStart;

        public Session.Session Session { get; }

        public PlanningCalculatorEngineResult(Session.Session session, DateTime timeOfStart)
        {
            Session = session;
            TimeOfStart = timeOfStart;
            GlobalPlanEndDate = timeOfStart;
            MostRechangedRCEndDate = timeOfStart;
            ModellingsWithBrokenFixedNaladLengths = new();
            ModellingsWithBrokenFreeNaladLengths = new();
        }

        public static PlanningCalculatorEngineResult CreateFullInstance(Session.Session session, DateTime StartTime)
        {
            PlanningCalculatorEngineResult result = new PlanningCalculatorEngineResult(session, StartTime);
            //result.CentersTimes = new Dictionary<CodedObject<string>, CenterTimesClass>();
            result.FirstStagesFullDurations = new Dictionary<int, CachedInterval>();
            result.FirstStagesRanks = new Dictionary<int, int>();
            result.FirstStagesStartDate = new Dictionary<int, DateTime>();
            result.ProstoiBeforeFirstStages = new Dictionary<int, (DatesPoolClass, CachedInterval)>();
            result.RechangesBeforeFirstStages = new Dictionary<int, (RechangesEngineResult, CachedInterval, int)>();
            result.FirstStagesPositions = new Dictionary<int, int>();
            result.FirstStagesAdditionalInfos = new Dictionary<int, CenterInPlanData>();
            return result;
        }

        public PlanningCalculatorEngineResult Copy()
        {
            var result = new PlanningCalculatorEngineResult(Session, TimeOfStart);
            result.CurrentRank = CurrentRank;
            result.RechangesSeconds = RechangesSeconds;
            result.GlobalPlanEndDate = GlobalPlanEndDate;
            result.MostRechangedRCEndDate = MostRechangedRCEndDate;
            result.ProstoiSeconds = ProstoiSeconds;
            result.DaysRuinedValue = DaysRuinedValue;
            result.DaysRuinedLinearValue = DaysRuinedLinearValue;
            result.RuinedCount = RuinedCount;
            result.NaladLengthsTotal = NaladLengthsTotal;
            result.TaraRedutantUsage = TaraRedutantUsage;
            result.BadOffsets = BadOffsets;
            result.TaraErrorsCount = TaraErrorsCount;
            result.FirstStagesPositions = FirstStagesPositions;
            return result;
        }
    }
}

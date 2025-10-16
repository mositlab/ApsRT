using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Planning.PlanningAlgorithm.R
{
    struct RTempResult
    {
        public TimeSpan Rechanges;
        public TimeSpan DeltaFromStart;
        public TimeSpan DeltaFromStartNoOp;
        public TimeSpan DeltaFromDeadline;
        //public TimeSpan DeltaFromDeadlineHeuristic;
        public float TotalCost;
        public bool IsFromSameOrder;
        public int OrderPriority;
        public int Level;

        public int ErrorsCount;

        public RTempResult(TimeSpan rechanges, TimeSpan deltaFromStart, TimeSpan deltaFromStartNoOp, TimeSpan deltaFromDeadline/*, TimeSpan deltaFromDeadlineHeuristic*/, int OrderPriority, bool isFromSameOrder, int level, float totalCost, int errorsCount)
        {
            Rechanges = rechanges;
            DeltaFromStart = deltaFromStart;
            TotalCost = totalCost;
            DeltaFromDeadline = deltaFromDeadline;
            IsFromSameOrder = isFromSameOrder;
            this.OrderPriority = 0;//OrderPriority;
            Level = level;
            ErrorsCount = errorsCount;
            DeltaFromStartNoOp = deltaFromStartNoOp;
            //DeltaFromDeadlineHeuristic = deltaFromDeadlineHeuristic;
        }

        public bool IsBetterThanOther(RTempResult otherResult, bool NoSteps)
        {
            if (otherResult.TotalCost == float.MaxValue && TotalCost < float.MaxValue) return true;

            if (ErrorsCount > otherResult.ErrorsCount) return false;

            if (NoSteps) return DeltaFromStart < otherResult.DeltaFromStart;

            //return
            //    Rechanges < otherResult.Rechanges
            //    || (Rechanges == otherResult.Rechanges && DeltaFromStart < otherResult.DeltaFromStart);

            return
                OrderPriority < otherResult.OrderPriority
                || OrderPriority == otherResult.OrderPriority && TotalCost < otherResult.TotalCost
                || OrderPriority == otherResult.OrderPriority && TotalCost == otherResult.TotalCost && Rechanges < otherResult.Rechanges
                || OrderPriority == otherResult.OrderPriority && TotalCost == otherResult.TotalCost && Rechanges == otherResult.Rechanges && DeltaFromDeadline < otherResult.DeltaFromDeadline
                || OrderPriority == otherResult.OrderPriority && TotalCost == otherResult.TotalCost && Rechanges == otherResult.Rechanges && DeltaFromDeadline == otherResult.DeltaFromDeadline && IsFromSameOrder && !otherResult.IsFromSameOrder
                || OrderPriority == otherResult.OrderPriority && TotalCost == otherResult.TotalCost && Rechanges == otherResult.Rechanges && DeltaFromDeadline == otherResult.DeltaFromDeadline && IsFromSameOrder == otherResult.IsFromSameOrder && Level < otherResult.Level
                ;

            //return Rechanges < otherResult.Rechanges || (Rechanges == otherResult.Rechanges && TotalCost < otherResult.TotalCost);

            //if (otherResult.Rechanges > Rechanges) return true;

            //return DeltaFromStart < otherResult.DeltaFromStart;
        }

    }
}

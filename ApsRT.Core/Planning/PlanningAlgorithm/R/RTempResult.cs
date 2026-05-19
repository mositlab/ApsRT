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
        public TimeSpan Prostoi;
        public TimeSpan DeltaFromStart;
        public TimeSpan DeltaFromStartNoOp;
        public TimeSpan DeltaFromDeadline;
        //public TimeSpan DeltaFromDeadlineHeuristic;
        public float TotalCost;
        public bool IsFromSameOrder;
        public int OrderPriority;
        public int Level;

        public int ErrorsCount;

        public double OperCount; // В зависимости от количества этапа, может влиять

        public int TransferTime;

        public bool ItsParr;

        public bool NeedsToBeFirst;

        public RTempResult(TimeSpan rechanges, TimeSpan deltaFromStart, TimeSpan deltaFromStartNoOp, TimeSpan deltaFromDeadline/*, TimeSpan deltaFromDeadlineHeuristic*/, int OrderPriority, bool isFromSameOrder, int level, float totalCost, int errorsCount, double operCount, int transferTime, bool itsParr, TimeSpan prostoi, bool needsToBeFirst)
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
            OperCount = operCount;
            TransferTime = transferTime;
            ItsParr = itsParr;
            Prostoi = prostoi;
            NeedsToBeFirst = needsToBeFirst;
            //DeltaFromDeadlineHeuristic = deltaFromDeadlineHeuristic;
        }

        public bool IsBetterThanOther(RTempResult otherResult, bool NoSteps)
        {
            if (otherResult.TotalCost == float.MaxValue && TotalCost < float.MaxValue) return true;

            if (ErrorsCount < otherResult.ErrorsCount) return true;
            if (ErrorsCount > otherResult.ErrorsCount) return false;

            if (TransferTime < otherResult.TransferTime) return true;
            if (TransferTime > otherResult.TransferTime) return false;

            //if (ItsParr && !otherResult.ItsParr) return true;
            //if (!ItsParr && otherResult.ItsParr) return false;

            if (NeedsToBeFirst && !otherResult.NeedsToBeFirst) return true;
            if (!NeedsToBeFirst && otherResult.NeedsToBeFirst) return false;

            if (NoSteps) return DeltaFromStart < otherResult.DeltaFromStart;

            //return
            //    Rechanges < otherResult.Rechanges
            //    || (Rechanges == otherResult.Rechanges && DeltaFromStart < otherResult.DeltaFromStart);

            //return
            //    OrderPriority < otherResult.OrderPriority
            //    || OrderPriority == otherResult.OrderPriority && TotalCost < otherResult.TotalCost
            //    || OrderPriority == otherResult.OrderPriority && TotalCost == otherResult.TotalCost && Rechanges < otherResult.Rechanges
            //    || OrderPriority == otherResult.OrderPriority && TotalCost == otherResult.TotalCost && Rechanges == otherResult.Rechanges && DeltaFromDeadline < otherResult.DeltaFromDeadline
            //    || OrderPriority == otherResult.OrderPriority && TotalCost == otherResult.TotalCost && Rechanges == otherResult.Rechanges && DeltaFromDeadline == otherResult.DeltaFromDeadline && IsFromSameOrder && !otherResult.IsFromSameOrder
            //    || OrderPriority == otherResult.OrderPriority && TotalCost == otherResult.TotalCost && Rechanges == otherResult.Rechanges && DeltaFromDeadline == otherResult.DeltaFromDeadline && IsFromSameOrder == otherResult.IsFromSameOrder && Level < otherResult.Level
            //    ;

            /*
            Ступени улучшения:
                НаОдномРЦ выполнено ИСТИНА
                Рекомендуемая тара ИСТИНА
                Ошибки (длин) минимум
                Этот вариант ошибочен ЛОЖЬ
                Простои+Переналадки минимум
                Переналадки минимум
                Дедлайн минимум
                Из одного заказа ИСТИНА
                Уровень минимум
             */

            //if (TaraIsRecommended && !otherResult.TaraIsRecommended) return true;
            //if (!TaraIsRecommended && otherResult.TaraIsRecommended) return false;

            if (TotalCost < otherResult.TotalCost) return true;
            if (TotalCost > otherResult.TotalCost) return false;

            if (Rechanges < otherResult.Rechanges) return true;
            if (Rechanges > otherResult.Rechanges) return false;

            if (DeltaFromDeadline < otherResult.DeltaFromDeadline) return true;
            if (DeltaFromDeadline > otherResult.DeltaFromDeadline) return false;

            if (OperCount > otherResult.OperCount) return true;
            if (OperCount < otherResult.OperCount) return false;

            if (DeltaFromStart < otherResult.DeltaFromStart) return true;
            if (DeltaFromStart > otherResult.DeltaFromStart) return false;

            if (IsFromSameOrder && !otherResult.IsFromSameOrder) return true;
            if (!IsFromSameOrder && otherResult.IsFromSameOrder) return false;

            if (Level < otherResult.Level) return true;
            if (Level > otherResult.Level) return false;

            return false;


            //return Rechanges < otherResult.Rechanges || (Rechanges == otherResult.Rechanges && TotalCost < otherResult.TotalCost);

            //if (otherResult.Rechanges > Rechanges) return true;

            //return DeltaFromStart < otherResult.DeltaFromStart;
        }

    }
}


using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using ZLinq;
using ApsRT.Core.Planning.PlanningClasses;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Stages;

namespace ApsRT.Core.Planning.PlanSimulator
{
    public class PlanSimulatorBlock
    {
        public PlanSimulatorTable TableLink;
        public int BlockIndexInLine;
        public Uco ActualCenter;

        public PlanSimulatorBlock PreviousBlockInLine;
        public PlanSimulatorBlock NextBlockInLine;

        public DateTime StartTime;
        public DateTime EndTime;

        public Rechanges.RechangesEngine.RechangesEngineResult RechangesEngineResult;

        public List<PlanSimulatorBlock> PreviousBlocksInChronology;
        public List<PlanSimulatorBlock> NextBlocksInChronology;

        public List<PlanSimulatorMod> Modellings;

        public HashSet<int> AllStagesHere;

        public void RememberAllStagesHere()
        {
            AllStagesHere = Modellings.AsValueEnumerable().SelectMany(x => x.Stages.Select(y => y.Stage.Index)).Distinct().ToHashSet();
        }

        public DateTime? WorstMinDateOfStart { get; private set; }

        //public DateTime GetWorstMinDateOfStart()
        //{
        //    return TableLink.StartDate;
        //    var res = PreviousBlocksInChronology.Count > 0 ? PreviousBlocksInChronology.AsParallel().Max(x => x.EndTime) : TableLink.StartDate;
        //    if (PreviousBlockInLine != null && PreviousBlockInLine.EndTime > res) res = PreviousBlockInLine.EndTime;
        //    return res;
        //}

        DateTime? WorstMinDateOfStartToUpdate;

        public void WantThisWorstMinDateOfStart(DateTime start)
        {
            if (!WorstMinDateOfStartToUpdate.HasValue || WorstMinDateOfStartToUpdate.Value < start) WorstMinDateOfStartToUpdate = start;
        }

        /// <summary>
        /// Эта функция нужна, чтобы обновить блок извне.
        /// </summary>
        /// <param name="newStartDate">У какого-то предыдущего блока сменилась дата окончания, и это может повлиять на текущий, поэтому надо поработать с датами.</param>
        /// <param name="newRechanges">У кого-то из пред. блоков возникло что-то, из-за чего следует перепроверить переналадку.</param>
        /// <param name="newAdditionalParams">У предыдущего блока изменились доп. параметры.</param>
        public void NeedToRecalc(bool newStartDate, bool newRechanges, bool newAdditionalParams)
        {
            bool willNeedToRecalcNextStartDates = false;
            bool willNeedToRecalcNextRechanges = false;
            bool willNeedToRecalcNextAdditionalParams = false;

            if (newStartDate)
            {
                if (WorstMinDateOfStartToUpdate.HasValue)
                {
                    WorstMinDateOfStart = WorstMinDateOfStartToUpdate.Value;
                    WorstMinDateOfStartToUpdate = null;
                }

                var lastTimeOnThatLine = PreviousBlockInLine != null ? PreviousBlockInLine.EndTime : TableLink.StartDate;
                var worstTime = WorstMinDateOfStart ?? TableLink.StartDate;
                if (worstTime > lastTimeOnThatLine)
                {
                    // Вот здесь простой
                    var prostoiLength = worstTime - lastTimeOnThatLine;

                    lastTimeOnThatLine += prostoiLength; // По итогу добавляем
                }

                StartTime = lastTimeOnThatLine;


                DateTime newEndTime = StartTime.AddHours(1);

                // По итогу, если что-то обновилось, то мы смотрим:

                if (newEndTime != EndTime)
                {
                    EndTime = newEndTime;
                    willNeedToRecalcNextStartDates = true;
                }
            }

            if (newRechanges)
            {
                Stage operationA = PreviousBlockInLine?.Modellings[0].Stages[0].Stage;
                var currentMod = Modellings[0];
                Stage operationB = currentMod.Stages[0].Stage;

                // Найдём переналадочку
                var rechangesResult = operationA == null ? null : operationB.SessionLink.MasterDataLinks.RechangesMasterData.FindRechanges(new(PlanningCalculatorEngine.PlanningCalculatorEngine.FillCenterWithSomeData(null, ActualCenter, TableLink.StartDate), new PlanningClasses.PoolClasses.DatesPoolClass(WorstMinDateOfStart ?? DateTime.Now, 0, -1, -1), operationA, operationB, ActualCenter));
            }


            // У следующих блоков придется что-нибудь пересчитать?
            bool needToRecalcNextBlocksWithAnything = willNeedToRecalcNextStartDates || willNeedToRecalcNextRechanges || willNeedToRecalcNextAdditionalParams;
            if (needToRecalcNextBlocksWithAnything)
            {
                if (NextBlockInLine != null)
                {
                    // Следующему блоку в линии придется пересчитать всё, если есть потребность.
                    //NextBlockInLine.NeedToRecalc(willNeedToRecalcNextStartDates, willNeedToRecalcNextRechanges, willNeedToRecalcNextAdditionalParams);
                    TableLink.AddNeedToRecalc(NextBlockInLine, willNeedToRecalcNextStartDates, willNeedToRecalcNextStartDates, willNeedToRecalcNextAdditionalParams);
                    NextBlockInLine.WantThisWorstMinDateOfStart(EndTime);
                }

                //return;

                // Теперь смотрим детей
                for (int i = 0; i < NextBlocksInChronology.Count; i++)
                {
                    var nextBlock = NextBlocksInChronology[i];
                    if (nextBlock == NextBlockInLine) continue; // Этот же самый блок, если такое когда-нибудь произойдет, не считаем - для него выше есть чек.

                    if (nextBlock.ActualCenter == ActualCenter)
                    {
                        // Если ребенок находится на том же РЦ, то во-первых, это довольно странно, а во вторых, ему кроме даты и переналадок, следует в том числе проверить доп. условия, потому что они меняются.
                        // На всякий случай я это вынес в отдельные фигурные скобки.
                        TableLink.AddNeedToRecalc(nextBlock, willNeedToRecalcNextStartDates, willNeedToRecalcNextStartDates, willNeedToRecalcNextAdditionalParams);
                    }
                    else
                    {
                        // У блоков из другого РЦ максимум что надо проверить - это начало работы, ну и переналадку - на всякий случай, если прогревы сработают. Доп условия чекать не надо, потому что они там подтягиваются с другого РЦ.
                        TableLink.AddNeedToRecalc(nextBlock, willNeedToRecalcNextStartDates, willNeedToRecalcNextStartDates, false);
                    }
                    nextBlock.WantThisWorstMinDateOfStart(EndTime);
                }
            }

        }
    }
}
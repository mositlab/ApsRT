using ApsRT.Core.Centers;
using ApsRT.Core.Centers.MasterData;
using ApsRT.Core.Planning;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Rechanges.RechangesEngine;
using ApsRT.Core.Stages;
using Azure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;

namespace ApsRT.Core.PlanSimulator.PlanSimulatorEngine
{
    public class PlanSimulatorEngine
    {
        public static PlanSimulatorCenterData FillCenterWithSomeData(PlanSimulatorCenterData centerLink, Uco CenterCode, DateTime TimeOfStart)
        {
            centerLink ??= new PlanSimulatorCenterData();
            centerLink.LastDate = TimeOfStart; //MinAcceptableDate,
            centerLink.LastStartDate = TimeOfStart; //MinAcceptableDate,
            centerLink.AdditinalConditions = new UniversalBinaryStorage();
            centerLink.OpersTime = centerLink.RechangesTime = centerLink.ProstoiTime = TimeSpan.Zero;
            centerLink.NextNonWorkingInterval = -1;
            centerLink.LastBlock = null;
            centerLink.LastMod = null;
            return centerLink;
        }

        public static PlanSimulatorEngineResult CalculatePlan(List<PlanSimulatorBlock> Blocks, int PoolIndex, DateTime StartTime,
            PlanSimulatorEngineResult resultToFill, PlanSimulatorEngineResult prevResult = null, int StartFrom = 0, int? uniqueCalcFlag = null/*,  bool LinearRC = true*//*, int AdditionalBlockIndex = -1, OpersBlock AdditionalBlock = null*/)
        {
            resultToFill ??= new PlanSimulatorEngineResult(Blocks[0].Lists[0].FirstOperJust.SessionLink, StartTime, uniqueCalcFlag); // Начинаем хотя бы так
            if (StartFrom == 0)
            {
                resultToFill.GlobalPlanEndDate = resultToFill.TimeOfStart;// Orders.MinAcceptableDate; // В любом случае на всякий случай задаем это
                resultToFill.MostRechangedRCEndDate = resultToFill.TimeOfStart;// Orders.MinAcceptableDate;
            }

            //if (prevResult != null) // Мы копируем их, чтобы не сломать пред. результат + сохранить данные с предыдущего расчёта
            //{
            //    resultToFill.StagesWithBrokenFixedNaladLengths = new HashSet<int>(prevResult.StagesWithBrokenFixedNaladLengths);
            //    resultToFill.ModellingsWithBrokenFreeNaladLengths = new HashSet<Guid>(prevResult.ModellingsWithBrokenFreeNaladLengths);
            //}

            // Проходимся по всем блокам
            for (int blockIndex = StartFrom/*StartFrom*/; blockIndex < Blocks.Count; blockIndex++)
            {
                var block = Blocks[blockIndex];
                //if (PoolIndex != MoveDataFromThisIndex) block.MoveCenterData(MoveDataFromThisIndex, PoolIndex);
                
                ProcessBlock(block, PoolIndex, blockIndex, resultToFill/*, LinearRC, StartFrom*/);
                //if (AdditionalBlockIndex == blockIndex)
                //{
                //    ProcessBlock(pool, blockIndex, currentFlag, resultToFill, AdditionalBlock);
                //}
                //else
                //{
                //    ProcessBlock(pool, blockIndex, currentFlag, resultToFill, null);
                //}
            }

            //foreach (var block in Blocks)
            //{
            //    foreach (var mod in block.Lists)
            //    {
            //        foreach (var stage in mod.Stages)
            //        {
            //            if (stage.NaladLengthData[PoolIndex].HadBrokenFixedNaladLength && stage.NaladLengthData[PoolIndex].Data.NaladLengthHere.Value == 0)
            //            {
            //                ;
            //            }
            //        }
            //    }
            //}

            return resultToFill;
        }


        /// <summary>
        /// Основная функция для заполнения плана
        /// </summary>
        /// <param name="pool">Пул с которым работаем</param>
        /// <param name="blockIndex">Индекс блока</param>
        /// <param name="currentFlag">Текущий флаг расчёта</param>
        /// <param name="resultToFill">Поле для заполнения даннымми</param>
        /// <param name="previousResult"></param>
        static void ProcessBlock(PlanSimulatorBlock currentBlock, int PoolIndex, int blockIndex, PlanSimulatorEngineResult resultToFill/*, bool LinearRC, int startFrom*//*, OpersBlock blockToCheckFirst*/)
        {
            int currentFlag = resultToFill.UniqueCalcFlag;
            //var currentBlock = /*blockToCheckFirst ?? */pool.Blocks[blockIndex];
            currentBlock.BlockIndex[PoolIndex] = blockIndex;
            var NonWorkingIntervalsMasterData = resultToFill.Session.MasterDataLinks.WorkingIntervalsMasterData;
            var CentersMasterData = resultToFill.Session.MasterDataLinks.CentersMasterData;
            var RechangesMasterData = resultToFill.Session.MasterDataLinks.RechangesMasterData;
            var AdditionalConditionsMasterData = resultToFill.Session.MasterDataLinks.AdditionalConditionsMasterData;

            // Получаем данные по РЦ
            (var ChoosenCenter, var centGlobalStruct, var centerGlobalData) = currentBlock.GetCenterData(PoolIndex);

            if (true/*LinearRC*/)
            {
                // У нас один РЦ на весь график будет, будет браться из основного класса
                if (centGlobalStruct.Flag != currentFlag)
                {
                    FillCenterWithSomeData(centGlobalStruct, ChoosenCenter, resultToFill.TimeOfStart);
                    centGlobalStruct.Flag = currentFlag;

                    // Двигаем график если надо, и сохраняем след. индекс
                    if (NonWorkingIntervalsMasterData.PreparedRCFixingData.TryGetValue(ChoosenCenter, out var val))
                    {
                        centGlobalStruct.LastDate += val.Duration;
                        centGlobalStruct.NextNonWorkingInterval = val.NextInterval;
                        centGlobalStruct.LastStartDate = centGlobalStruct.LastDate;
                    }
                }

                currentBlock.PreviousBlockOnThisRC[PoolIndex] = centGlobalStruct.LastBlock;
                if (centGlobalStruct.LastBlock != null) centGlobalStruct.LastBlock.NextBlockOnThisRC[PoolIndex] = currentBlock;
                currentBlock.NextBlockOnThisRC[PoolIndex] = null;
            }
            else
            {
                // Мы возьмём КОПИЮ РЦ из предыдущего кубика.
                //var prevBlock = currentBlock.PreviousBlockOnThisRC[PoolIndex];
                //if (prevBlock != null)
                //{
                //    centGlobalStruct = prevBlock.CenterDataLocalCopy[PoolIndex].Copy();
                //}
                //else
                //{
                //    centGlobalStruct = FillCenterWithSomeData(null, ChoosenCenter, resultToFill.TimeOfStart);
                //    centGlobalStruct.Flag = currentFlag;

                //    // Двигаем график если надо, и сохраняем след. индекс
                //    if (NonWorkingIntervalsMasterData.PreparedRCFixingData.TryGetValue(ChoosenCenter, out var val))
                //    {
                //        centGlobalStruct.LastDate += val.Duration;
                //        centGlobalStruct.NextNonWorkingInterval = val.NextInterval;
                //        centGlobalStruct.LastStartDate = centGlobalStruct.LastDate;
                //    }
                //}
                ;
                //currentBlock.PreviousBlockOnThisRC[PoolIndex] = centGlobalStruct.LastBlock;
            }



            // Получаем инфу по всем данным со всех моделирований
            (bool Passed, PlanSimulatorDatesData Data)[] ParallelData = new (bool Passed, PlanSimulatorDatesData)[currentBlock.Lists.Count];
            int RemainToProcessModellings = currentBlock.Lists.Count; // Сколько ещё моделирований надо прогнать?

            // Сначала просто получаем сырые данные. Тут двигать пока ничего НЕ НАДО из-за нераб интервалов, это случится позже. Тут просто нужно данные получить, и разницу узнать
            for (int i = 0; i < ParallelData.Length; i++)
            {
                ParallelData[i] = (false, FindMinTime(PoolIndex, currentBlock.Lists[i], currentFlag, resultToFill.Session.AssignedPlanningAlgorithm.MinAcceptableDate/*, LinearRC*/));
            }

            // Теперь бродим по этим моделированиям
            List<int> ChoosenModellings = new(currentBlock.Lists.Count); // Создаем список, куда записываем все подходящие моделирования

            // Зацикливаемся, пока не найдём.
            while (RemainToProcessModellings > 0)
            {
                // Получаем данные.
                var DateOfStart = ProcessParallelLists(ParallelData, centerGlobalData, ChoosenModellings);

                // Список получен. Теперь эту дату мы двигаем, если случайно попали в диапазон
                int firstIndex = ChoosenModellings[0];
                var prostoiData = ParallelData[firstIndex].Data; // Спокойно с ним работаем, потому что это все равно копия. Понадобится для переналадок и доп. условий
                var interval = NonWorkingIntervalsMasterData.CheckIfCatchedNonWorkingIntervalCached(ChoosenCenter, DateOfStart, centGlobalStruct.NextNonWorkingInterval); // Попробуем подвинуть его, чтоб он в случае попадания на нерабочий интервал, сдвинулся
                var intervalLast = interval.Duration;
                var prostoiLast = prostoiData.MinValue;
                var lastStartDate = DateOfStart;

                prostoiData.MinValue = DateOfStart + interval.Duration; // Это поле будет юзаться для минимальной даты начала всех моделирований здесь. Тут минимальная дата начала + нужный остаток, чтоб выйти из интервала. Получается минимально требуемая дата у простоя. Это будет заюзано в переналадках, доп. условиях, ну и для расчётов дальше.
                var nextPossibleIntervalWillBe = interval.NextInterval; // Запоминаем последний индекс, который с которого можно начинать счёт интервалов для переналадки (справа налево).

                // Вот нужные мне данные по этапам
                var lastMod = centGlobalStruct.LastMod;
                var operationA = lastMod?.FirstOper;
                var currentMod = currentBlock.Lists[firstIndex];
                var operationB = currentMod.FirstOper;

                // Найдём переналадочку
                var rechangesResult = operationA == null ? null : RechangesMasterData.FindRechanges(new(centGlobalStruct, prostoiData, operationA.Stage, operationB.Stage, ChoosenCenter));

                // Устанавливаем простои и переналадку здесь
                //CenterTimesClass cts = resultToFill.CentersTimes == null ? null : (resultToFill.CentersTimes.TryGetValue(ChoosenCenter, out var cts2) ? cts2 : null); // Если надо, то мы берем класс РЦ для заполнения
                SetProstoiAndRechanges(ChoosenCenter, centGlobalStruct, prostoiData, rechangesResult, nextPossibleIntervalWillBe, resultToFill, operationB.Stage.Index, operationA?.Stage.Index ?? -1);

                // Заполняем инфу о всех моделированиях здесь.
                // Здесь мы уже не вызываем чекер, попал ли я в интервал, потому что предыдущий код должен был подвинуть переналадку так, чтобы в худшем случае операция начиналась сразу же после нерабочего интервала. А в лучшем и в принципе где-то посередине.
                TimeSpan LongestMod = TimeSpan.Zero;
                for (int i = 0; i < ChoosenModellings.Count; i++)
                {
                    var index = ChoosenModellings[i];
                    var list = currentBlock.Lists[index];
                    var durationHere = list.AllTime[ChoosenCenter];
                    if (list.WasPassed[PoolIndex]) durationHere = TimeSpan.Zero; // Если был пропущен, то 0.
                    if (durationHere > LongestMod) LongestMod = durationHere;
                }

                /*
                Итого, у нас есть:
                ChoosenCenter - текущий рц
                centGlobalStruct.LastDate - дата начала
                LongestMod - длительность.
                Этого достаточно, чтобы начать расчёт, как надо подвинуть операцию, и как изменится LongestMod
                 */

                var intervalHere = NonWorkingIntervalsMasterData.GetDurationWithNonWorkingIntervals(ChoosenCenter, centGlobalStruct.LastDate, (int)LongestMod.TotalSeconds, centGlobalStruct.NextNonWorkingInterval);

                // Вставляем собственно моделирование
                centGlobalStruct.LastDate += intervalHere.Duration;
                var lastDateFinally = centGlobalStruct.LastDate;

                PlanSimulatorCenterData binaryCopy = resultToFill.FirstStagesAdditionalInfos != null ? centGlobalStruct.Copy() : null;

                // Заполнение данных всяких
                for (int i = 0; i < ChoosenModellings.Count; i++)
                {
                    var index = ChoosenModellings[i];
                    var MOD = currentBlock.Lists[index];
                    var firstID = MOD.FirstOperJust.Index;

                    var dates = MOD.MinDatesData[PoolIndex];
                    if (dates.Flag == currentFlag)
                        resultToFill.TaraRedutantUsage += (centGlobalStruct.LastDate - dates.MinValue).TotalDays / MOD.FirstOperJust.Level;

                    // Попробуем заполнить длительности центров, если надо
                    centGlobalStruct.OpersTime += intervalHere.Duration;

                    // Попробуем заполнить даты начала, если надо
                    if (resultToFill.FirstStagesStartDate != null)
                        resultToFill.FirstStagesStartDate.TryAdd(firstID, centGlobalStruct.LastStartDate);

                    MOD.DateOfStart[PoolIndex] = centGlobalStruct.LastStartDate;

                    // Попробуем заполнить интервалы длительностей, если надо
                    if (resultToFill.FirstStagesFullDurations != null) resultToFill.FirstStagesFullDurations.TryAdd(firstID, intervalHere);

                    MOD.DateOfEnd[PoolIndex] = lastDateFinally;


                    // Попробуем заполнить ранги, если надо
                    //if (resultToFill.FirstStagesRanks != null) resultToFill.FirstStagesRanks.TryAdd(firstID, resultToFill.CurrentRank);
                    MOD.Ranks[PoolIndex] = resultToFill.CurrentRank;

                    // Попробуем заполнить ранги, если надо
                    //if (resultToFill.FirstStagesPositions != null) resultToFill.FirstStagesPositions.TryAdd(firstID, blockIndex);

                    if (resultToFill.FirstStagesAdditionalInfos != null) resultToFill.FirstStagesAdditionalInfos.TryAdd(firstID, binaryCopy);

                    resultToFill.CurrentRank += MOD.Stages.Count; // В ЛЮБОМ СЛУЧАЕ добавляем
                }

                // Обновляем результаты:
                if (centerGlobalData.IsWithRechanges && resultToFill.MostRechangedRCEndDate < lastDateFinally) resultToFill.MostRechangedRCEndDate = lastDateFinally;
                if (resultToFill.GlobalPlanEndDate < lastDateFinally) resultToFill.GlobalPlanEndDate = lastDateFinally;

                // Следующий интервал теперь такой
                centGlobalStruct.NextNonWorkingInterval = intervalHere.NextInterval;

                // Смотрим на "доп. условия"
                if (centerGlobalData.HasAdditionalActions && operationA != null)
                {
                    // Попробуем дополнительные условия.
                    AdditionalConditionsMasterData.UpdateAdditionalParamsOnCenter(new RechangesEngineData(centGlobalStruct, prostoiData, operationA.Stage, operationB.Stage, ChoosenCenter));
                }

                // Смотрим на наладочные длины
                ProcessNaladLengths(PoolIndex, currentBlock, ChoosenModellings, currentFlag, rechangesResult, ChoosenCenter, lastMod, resultToFill);

                // Смотрим на отклонения от предыдущего графика
                ProcessPrevTimes(PoolIndex, currentBlock, ChoosenModellings, resultToFill);

                // Если эта операция последняя, то:
                // а. Она будет одна
                // б. Стоит чекнуть заказ
                if (operationB.Stage.Nexts.Count == 0/*IsLast*/)
                {
                    // Считаем заказы
                    ProcessOrder(PoolIndex, operationB, lastDateFinally, currentFlag, resultToFill);
                }

                // Попробуем подсчитать переход из цеха в цех
                var ordPfClass = currentMod.LastSubsData[PoolIndex];
                if (ordPfClass.Flag != currentFlag)
                {
                    // Просто задаем РЦ
                    ordPfClass.Flag = currentFlag;
                    ordPfClass.LastSubdivision = centerGlobalData.Subdivision;
                    ordPfClass.LastMod = currentMod;
                }
                else
                {
                    // Проверяем, совпало ли подразделение
                    if (ordPfClass.LastSubdivision != centerGlobalData.Subdivision)
                    {
                        ordPfClass.LastSubdivision = centerGlobalData.Subdivision;
                        ordPfClass.LastMod = currentMod;
                        resultToFill.SubdivisionsTransfer++;
                    }
                }
                

                // Задаём минимальные даты начала детям
                SetMinDatesToChildsAndRotting(PoolIndex, currentBlock, blockIndex, ChoosenModellings, lastDateFinally, currentFlag);

                ProcessRottingData(PoolIndex, currentBlock, blockIndex, ChoosenModellings, currentFlag, resultToFill);

                // По итогу:
                centGlobalStruct.LastMod = currentMod;
                //int hashCodeToAdd = operationB.GetHashCode();
                //centGlobalStruct.LocalFlag = HashCode.Combine(centGlobalStruct.LocalFlag, hashCodeToAdd); // локальный флаг
                //resultToFill.GlobalFlag = HashCode.Combine(resultToFill.GlobalFlag, hashCodeToAdd); // глобальный флаг
                RemainToProcessModellings -= ChoosenModellings.Count;
                ChoosenModellings.Clear();
            }

            centGlobalStruct.LastBlock = currentBlock;

            //if (LinearRC)
            //{
            //    currentBlock.CenterDataLocalCopy[PoolIndex] = centGlobalStruct.Copy(); // Копируем в текущую
            //}
            //else
            //{
            //    currentBlock.CenterDataLocalCopy[PoolIndex] = centGlobalStruct.Copy(); // Поскольку я уже работаю с копией, то заново копировать её резона нет.
            //}
        }

        private static void ProcessPrevTimes(int poolIndex, PlanSimulatorBlock block, List<int> ChoosenModellings, PlanSimulatorEngineResult resultToFill)
        {
            for (int i = 0; i < ChoosenModellings.Count; i++)
            {
                var mod = block.Lists[ChoosenModellings[i]]; // Берём моделирование
                var start = mod.DateOfStart[poolIndex];
                for (int j = 0; j < mod.StagesAsNormal.Count; j++)
                {
                    var prevDate = mod.StagesAsNormal[j].PrevDateTime;
                    if (prevDate.HasValue)
                    {
                        var delta = Math.Abs((prevDate.Value - start).TotalSeconds);
                        resultToFill.BadOffsets += delta;
                    }
                }
            }
        }

        /// <summary>
        /// Устанавливает в плане простой и переналадку
        /// </summary>
        /// <param name="ChoosenCenter">Центр на который встает все это</param>
        /// <param name="centGlobalStruct">Структура этого центра</param>
        /// <param name="prostoiData">Информация о простоях</param>
        /// <param name="rechange">Результат движка переналадок</param>
        /// <param name="nextPossibleIntervalWillBe">Самый крайний интервал, с которого по идее можно начать пересчет переналадки с учетом нерабочих интервалов</param>
        /// <param name="resultToFill">Куда записывать результаты (имеются в виду суммы по переналадкам и простоям)</param>
        /// <param name="classToFill">Сюда можно записать переналадки и простои, если класс не пустой</param>
        static void SetProstoiAndRechanges(Uco ChoosenCenter, PlanSimulatorCenterData centGlobalStruct, PlanSimulatorDatesData prostoiData, RechangesEngineResult rechange, int nextPossibleIntervalWillBe, PlanSimulatorEngineResult resultToFill, int nextStageID, int lastStageID)
        {
            int rechangeDuration = rechange?.Duration ?? 0;

            var rechangeDurAsTimeSpan = TimeSpan.FromSeconds(rechangeDuration);

            var NonWorkingIntervalsMasterData = resultToFill.Session.MasterDataLinks.WorkingIntervalsMasterData;

            // Теперь есть несколько вариков.
            var ProstoiDuration = prostoiData.MinValue - centGlobalStruct.LastDate; // Получаем длину простоя

            if (
                false
                //||
                //centGlobalStruct.NextNonWorkingInterval == -1
                //|| // Если у меня не задан след. интервал, либо потенциальный следующий в точности равен текущему - значит между датами никаких интервалов и не было
                //nextPossibleIntervalWillBe == centGlobalStruct.NextNonWorkingInterval
                //|| nextPossibleIntervalWillBe == -1
                )
            {
                // Если последнего интервала не задано (между ними нет никакого другого), то я в принципе не парюсь
                if (ProstoiDuration.TotalSeconds < rechangeDuration)
                {
                    // Если простой мал, то можно тупо вставить перенладку, и не париться
                    AlreadyFillRechanges(centGlobalStruct, rechange, new CachedInterval(rechangeDurAsTimeSpan, null, -1), resultToFill, nextStageID, lastStageID);

                }
                else
                {
                    var prostoiNow = ProstoiDuration.Add(-rechangeDurAsTimeSpan); // Вычитаем из простоя переналадку

                    if (prostoiNow.Ticks < 0) prostoiNow = TimeSpan.Zero; // Обнуляем

                    // Вставляем этот простой
                    AlreadyFillProstoi(centGlobalStruct, prostoiData, new CachedInterval(prostoiNow, null, -1), resultToFill, nextStageID);

                    // Вставляем переналадку
                    AlreadyFillRechanges(centGlobalStruct, rechange, new CachedInterval(rechangeDurAsTimeSpan, null, -1), resultToFill, nextStageID, lastStageID);
                }
            }
            else
            {
                if (ProstoiDuration.TotalSeconds < rechangeDuration)
                {
                    // Если простой мал, то можно тупо вставить перенладку, но с учетом интервалов
                    /*
                    Итого, у нас есть:
                    ChoosenCenter - текущий рц
                    centGlobalStruct.LastDate - дата начала
                    rechangeDuration - длительность.
                    Этого достаточно, чтобы начать расчёт, как надо подвинуть операцию, и как изменится rechangeDuration
                     */

                    var intervalHereLoc = NonWorkingIntervalsMasterData.GetDurationWithNonWorkingIntervals(ChoosenCenter, centGlobalStruct.LastDate, rechangeDuration, centGlobalStruct.NextNonWorkingInterval);

                    // Вставляем переналадку
                    AlreadyFillRechanges(centGlobalStruct, rechange, intervalHereLoc, resultToFill, nextStageID, lastStageID);

                    // Следующий интервал теперь такой
                    centGlobalStruct.NextNonWorkingInterval = intervalHereLoc.NextInterval;
                }
                else
                {
                    // А вот тут уже придется выложиться.
                    /*
                    Итого, у нас есть:
                    ChoosenCenter - текущий рц
                    centGlobalStruct.LastDate - дата конца последнего выполнения
                    prostoiData.MinValue - дата начала следующей операции, пофикшенная, но мы ещё не знаем, хватит ли ее чтоб вместить переналадку
                    rechangeDuration - длительность переналадки.
                    Этого достаточно, чтобы начать расчёт, как надо подвинуть переналадку, и как изменится DateOfStart
                     */

                    (CachedInterval Prostoi, CachedInterval Rechanges) = NonWorkingIntervalsMasterData.RechangesNonWorkingIntervalsCorrelation(ChoosenCenter, centGlobalStruct.LastDate, prostoiData.MinValue, rechangeDuration, centGlobalStruct.NextNonWorkingInterval, nextPossibleIntervalWillBe);

                    // Вставляем простой
                    AlreadyFillProstoi(centGlobalStruct, prostoiData, Prostoi, resultToFill, nextStageID);

                    // Вставляем переналадку
                    AlreadyFillRechanges(centGlobalStruct, rechange, Rechanges, resultToFill, nextStageID, lastStageID);

                    // Следующий интервал теперь такой
                    centGlobalStruct.NextNonWorkingInterval = Rechanges.NextInterval;
                }
            }
        }

        static void AlreadyFillProstoi(PlanSimulatorCenterData centGlobalStruct, PlanSimulatorDatesData prostoiData, CachedInterval prostoiDur, PlanSimulatorEngineResult resultToFill, int NextStageID)
        {
            centGlobalStruct.LastDate += prostoiDur.Duration;
            resultToFill.ProstoiSeconds += prostoiDur.Duration;
            centGlobalStruct.ProstoiTime += prostoiDur.Duration;

            // Задаём информацию если надо
            if (resultToFill.ProstoiBeforeFirstStages != null)
                resultToFill.ProstoiBeforeFirstStages.TryAdd(NextStageID, (prostoiData, prostoiDur));
        }

        static void AlreadyFillRechanges(PlanSimulatorCenterData centGlobalStruct, RechangesEngineResult result, CachedInterval rechangeDur, PlanSimulatorEngineResult resultToFill, int NextStageID, int lastStageID)
        {
            centGlobalStruct.LastDate += rechangeDur.Duration;
            resultToFill.RechangesSecondsWithoutLunches = resultToFill.RechangesSecondsWithoutLunches.Add(TimeSpan.FromSeconds(result?.Duration ?? 0));
            resultToFill.RechangesSeconds = resultToFill.RechangesSeconds.Add(rechangeDur.Duration);
            centGlobalStruct.RechangesTime = centGlobalStruct.RechangesTime.Add(rechangeDur.Duration);
            centGlobalStruct.LastStartDate = centGlobalStruct.LastDate;

            // Задаём информацию если надо
            if (resultToFill.RechangesBeforeFirstStages != null)
                resultToFill.RechangesBeforeFirstStages.TryAdd(NextStageID, (result, rechangeDur, lastStageID));
        }

        /// <summary>
        /// Устанавливает следующим моделированиям в пуле минимальную дату начала.
        /// </summary>
        /// <param name="pool">Пул с которым работаем</param>
        /// <param name="block">Блок из пула с которым работаем</param>
        /// <param name="blockIndex">Индекс блока (чтобы задать значение)</param>
        /// <param name="ChoosenModellings">Выбранные моделирования из блока</param>
        /// <param name="LastDate">Собственно дата назначения</param>
        /// <param name="currentFlag">Текущий "флаг расчёта"</param>
        static void SetMinDatesToChildsAndRotting(int PoolIndex, PlanSimulatorBlock block, int blockIndex, List<int> ChoosenModellings, DateTime LastDate, int currentFlag)
        {
            for (int i = 0; i < ChoosenModellings.Count; i++) // Проходим по всем вставшим моделированиям
            {
                var mod = block.Lists[ChoosenModellings[i]]; // Берём моделирование

                for (int j = 0; j < mod.NextMods.Count; j++) // Смотрим на детей-модов
                {
                    var nextModHere = mod.NextMods[j];
                    var hh = nextModHere.MinDatesData[PoolIndex];
                    var LastDateHere = LastDate;

                    // Подразделения
                    var subdivsMasterData = nextModHere.FirstOperJust.SessionLink.MasterDataLinks.SubdivisionsTransfersMasterData;
                    if (subdivsMasterData.HasAnyData)
                    {
                        var subDiv1 = block.GetCenterData(PoolIndex).CenterModelDataLink.Subdivision;
                        var subDiv2 = nextModHere.BlockLink.GetCenterData(PoolIndex).CenterModelDataLink.Subdivision;
                        var transferTime = subdivsMasterData.GetSubdivisionTransferTime(subDiv1, subDiv2);
                        LastDateHere = LastDateHere.AddSeconds(transferTime);
                    }

                    if (hh.Flag != currentFlag || hh.MinValue < LastDateHere)
                    {
                        //if (hh.Flag == currentFlag)
                        //{
                        //    tempData.TaraRedutantUsage += (LastDate - hh.MinValue).TotalDays / thisMod.FirstOper.Level;
                        //}
                        hh.MinValue = LastDateHere;
                        hh.Flag = currentFlag;
                        hh.LastBlockIndex = blockIndex;
                        hh.LastID = mod.FirstOperJust.Index;
                        //opers.DatesData[next] = hh;
                    }

                    // Назначаем детям инфу о себе
                    var rottingData = nextModHere.PartiaRottingData[PoolIndex];
                    if (rottingData.Flag != currentFlag || rottingData.WorstMod == null || rottingData.WorstMod.DateOfEnd[PoolIndex] > LastDate)
                    {
                        rottingData.Flag = currentFlag;
                        rottingData.WorstMod = mod;
                    }
                }
            }
        }


        static void ProcessRottingData(int PoolIndex, PlanSimulatorBlock block, int blockIndex, List<int> ChoosenModellings, int currentFlag, PlanSimulatorEngineResult resultToWrite)
        {
            for (int i = 0; i < ChoosenModellings.Count; i++) // Проходим по всем вставшим моделированиям
            {
                var mod = block.Lists[ChoosenModellings[i]]; // Берём моделирование

                // Получим информацию о худшем моде из существующих
                var rottingData = mod.PartiaRottingData[PoolIndex];

                // Возьмем данные с обоих
                DateTime? worstFromGhosts = mod.WorstGhostPrevsDate;
                DateTime? worstFromPool = rottingData.WorstMod?.DateOfEnd[PoolIndex] ?? null;


                if (!worstFromGhosts.HasValue && !worstFromPool.HasValue)
                {
                    mod.RottingTimeInDays[PoolIndex] = 0;
                    continue;
                }

                double acceptableTime = 0;
                double timeDelta = 0;
                DateTime timeOfStart = mod.DateOfStart[PoolIndex];

                if (worstFromGhosts.HasValue && !worstFromPool.HasValue)
                {
                    acceptableTime = mod.FirstOperJust.GhostRottingTimeInfo.AcceptableTime;
                    timeDelta = (timeOfStart - worstFromGhosts.Value).TotalDays;
                }

                if (!worstFromGhosts.HasValue && worstFromPool.HasValue)
                {
                    acceptableTime = mod.FirstOperJust.RottingTimeInfo.AcceptableTime;
                    timeDelta = (timeOfStart - worstFromPool.Value).TotalDays;
                }

                if (worstFromGhosts.HasValue && worstFromPool.HasValue)
                {
                    bool ghostIsBetter = worstFromGhosts.Value < worstFromPool.Value;
                    acceptableTime = ghostIsBetter ? mod.FirstOperJust.GhostRottingTimeInfo.AcceptableTime : mod.FirstOperJust.RottingTimeInfo.AcceptableTime;
                    timeDelta = (timeOfStart - (ghostIsBetter ? worstFromGhosts.Value : worstFromPool.Value)).TotalDays;
                }

                // >0 - гниение, <=0 - запас
                var delta = timeDelta - acceptableTime;
                mod.RottingTimeInDays[PoolIndex] = delta;
                
                if (delta > 0) resultToWrite.TaraCriticalUsage += delta;
            }
        }

        /// <summary>
        /// Получает минимально возможную дату для какого-либо моделирования
        /// </summary>
        /// <param name="pool">Пул из которого вытягивать данные</param>
        /// <param name="modelling">Моделирование для которого получаем данные</param>
        /// <param name="neededFlag">Текущий "флаг расчёта"</param>
        /// <returns></returns>
        static PlanSimulatorDatesData FindMinTime(int PoolIndex, PlanSimulatorMod modelling, int neededFlag, DateTime minDate/*, bool LinearRC*/)
        {
            var neededDate = modelling.FirstOperJust.Fixed ? modelling.FirstOperJust.StartTimeFact.Value : minDate;

            //var timeBcsOfTransfers = neededDate;
            //var subsMasterData = modelling.FirstOperJust.SessionLink.MasterDataLinks.SubdivisionsTransfersMasterData;
            //if (subsMasterData.HasAnyData)
            //{
            //    var subsData = modelling.LastSubsData[PoolIndex];
            //    if (subsData.Flag == neededFlag && subsData.LastMod != null && subsData.LastMod != modelling)
            //    {
            //        var nextSub = modelling.BlockLink.GetCenterData(PoolIndex).CenterModelDataLink.Subdivision;
            //        if (nextSub != subsData.LastSubdivision)
            //        {
            //            var transferTime = subsMasterData.GetSubdivisionTransferTime(subsData.LastSubdivision, nextSub);
            //            timeBcsOfTransfers = subsData.LastMod.DateOfEnd[PoolIndex].AddSeconds(transferTime); // Прибавляем к предыдущему моду из этой партии время перехода
            //        }
            //    }
            //}

            var timeBcsOfPrevs = modelling.MinDatesData[PoolIndex];//pool.DatesData[modelling.ModID];

            if (timeBcsOfPrevs.Flag == neededFlag // Если это время актуально и 
                && timeBcsOfPrevs.MinValue > neededDate
                /*&& timeBcsOfPrevs.MinValue > timeBcsOfTransfers*/)
            {
                return new PlanSimulatorDatesData(timeBcsOfPrevs);
            }

            //if (timeBcsOfTransfers > neededDate
            //    && (timeBcsOfPrevs.Flag != neededFlag || timeBcsOfTransfers > timeBcsOfPrevs.MinValue))
            //{
            //    return new PlanSimulatorDatesData(timeBcsOfTransfers, neededFlag, -1, -1);
            //}

            return new PlanSimulatorDatesData(neededDate, neededFlag, -1, -1);

        }

        /// <summary>
        /// Вынесенный в отдельный метод обработчик параллельных моделирований, чтоб выявить у них самую минимально возможную дату + самый длинный из них, + пометить выполненными часть
        /// </summary>
        /// <param name="ParallelData">Чтообрабатываем</param>
        /// <param name="centerGlobalData">На каком это центре</param>
        /// <param name="ChoosenModellings">Куда вписываем результат</param>
        /// <returns>Минимально возможное начало этой кучи</returns>
        static DateTime ProcessParallelLists((bool Passed, PlanSimulatorDatesData Data)[] ParallelData, CenterBaseModel centerGlobalData, List<int> ChoosenModellings)
        {
            DateTime? MinTime = null;

            // Сначала найдем минимально допустимое время
            for (int i = 0; i < ParallelData.Length; i++)
            {
                if (ParallelData[i].Passed) continue;
                var data = ParallelData[i].Data;
                if (!MinTime.HasValue || data.MinValue < MinTime.Value) MinTime = data.MinValue; // Задаем минимальное значение когда я могу начаться
            }

            // А вот теперь ищем какие можно вместить
            DateTime DateOfStart = MinTime.Value;
            for (int i = 0; i < ParallelData.Length; i++)
            {
                if (ParallelData[i].Passed) continue;
                var data = ParallelData[i].Data;
                if (!centerGlobalData.MaxMinutesAwaiting.HasValue // У центра вообще нет ожидания?
                    ||
                    (data.MinValue - MinTime.Value).TotalMinutes < centerGlobalData.MaxMinutesAwaiting.Value // Ну либо есть, но оно поменьше
                    )
                {
                    if (DateOfStart < data.MinValue) DateOfStart = data.MinValue; // Узнаем, когда можно начать, из-за того, что они друг от друга отстают. Вот здесь примерно можно задать 
                    ParallelData[i].Passed = true; // Помечаем его, как уже прошедшего
                    ChoosenModellings.Add(i);
                }
            }

            return DateOfStart;
        }

        #region Подсчёт дедлайнов

        static void ProcessOrder(int PoolIndex, PlanSimulatorStage oper, DateTime final, int currentFlag, PlanSimulatorEngineResult resultToFill)
        {
            var ordersMasterData = resultToFill.Session.MasterDataLinks.OrdersMasterData;
            var ordern = oper.Stage.OrderLink.OrderID;
            var ord = oper.OrderData[PoolIndex];
            var ordG = oper.Stage.OrderLink;
            var dead = ordG.Deadline;
            var priority = ordG.Priority;

            if (final > dead) // Если заказ сорван
            {
                if (ord.Flag != currentFlag) // Если он ещё даже не помечен
                {
                    ord.MinValue = final;
                    ord.Flag = currentFlag;
                    ord.Time = (final - dead).TotalDays;
                    ord.TimeWithK = ordersMasterData.CalcDeadline(ord.Time, ordern, priority);
                    //opers.OrdersData[ordern] = ord;
                    resultToFill.RuinedCount++;
                    resultToFill.DaysRuinedValue += ord.TimeWithK;
                    resultToFill.DaysRuinedLinearValue += ord.Time;
                }
                else
                {
                    // Уже был помечен
                    if (final > ord.MinValue) // Если он даже хуже, чем было ранее
                    {
                        ord.MinValue = final;
                        resultToFill.DaysRuinedValue -= ord.TimeWithK; // старое значение отнимаем
                        resultToFill.DaysRuinedLinearValue -= ord.Time; // старое значение отнимаем
                        ord.Time = (final - dead).TotalDays;
                        ord.TimeWithK = ordersMasterData.CalcDeadline(ord.Time, ordern, priority); // Пересчитываем
                                                                                                   //opers.OrdersData[ordern] = ord;
                        resultToFill.DaysRuinedValue += ord.TimeWithK;
                        resultToFill.DaysRuinedLinearValue += ord.Time;
                    }
                }
            }
            else
            {
                if (ord.Flag != currentFlag) // Если он ещё даже не помечен, но и не хуже, он помечается, просто заполняется пустыми значениями
                {
                    ord.MinValue = final;
                    ord.Flag = currentFlag;
                    ord.Time = 0;
                    ord.TimeWithK = 0;
                    //opers.OrdersData[ordern] = ord;
                }
            }


        }

        #endregion

        #region Наладочные длины

        /// <summary>
        /// Обработка наладочных длин (для ВСЕХ этапов здесь)
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="block"></param>
        /// <param name="ChoosenModellings"></param>
        /// <param name="currentFlag"></param>
        /// <param name="RechangesList"></param>
        /// <param name="choosenCenter"></param>
        /// <param name="lastMod"></param>
        /// <param name="result"></param>
        static void ProcessNaladLengths(
            int PoolIndex, PlanSimulatorBlock block, List<int> ChoosenModellings, int currentFlag,
            RechangesEngineResult RechangesList, Uco choosenCenter, PlanSimulatorMod lastMod,
            PlanSimulatorEngineResult result
            )
        {
            //var ALL = result.Session.MasterDataLinks.StagesMasterData.AllStages;
            var NaladLengthsMasterData = result.Session.MasterDataLinks.NaladLengthsMasterData;
            var GlobalOrders = result.Session.MasterDataLinks.OrdersMasterData.AllOrders;
            var PFPartiasData = result.Session.MasterDataLinks.OrdersMasterData.PFPartiasData;

            for (int i = 0; i < ChoosenModellings.Count; i++) // Проходим по всем вставшим моделированиям
            {
                var mod = block.Lists[ChoosenModellings[i]]; // Берём моделирование

                Guid? lastModGuid = null;

                bool itsFirstMod = true;

                for (int j = 0; j < mod.Stages.Count; j++) // Смотрим на детей-модов
                {
                    var op1 = mod.Stages[j];
                    var info = op1.NaladLengthData[PoolIndex];//pool.NaladLengthsData[etapID];
                    var thatEtap = op1.Stage;//ALL[etapID]; // всё, что дальше, сделано по приказу алены и марины, 23.07.24.
                    int etapID = thatEtap.Index;
                    var modGuid = thatEtap.CurrentModelling;

                    if (!lastModGuid.HasValue) lastModGuid = modGuid;

                    if (modGuid != lastModGuid)
                    {
                        itsFirstMod = false;
                        lastModGuid = modGuid;
                    }

                    bool firstLaunch = NaladLengthsMasterData.ItsFirstLaunch(RechangesList, lastMod);

                    firstLaunch &= itsFirstMod; // Если это не первый мод в общем этапе, то дальше считаем иначе.

                    var clas = op1.PartiaRecalcData[PoolIndex];//pool.NaladLengthsNeedToRecalc[guid];



                    //var operationB = op1;
                    //var data = operationB.NaladLengthData[PoolIndex].Data;
                    //if (result.WhatWas.ContainsKey(operationB.Stage.Index))
                    //{
                    //    ;
                    //}
                    //else
                    //{
                    //    result.WhatWas.Add(operationB.Stage.Index, (new PlanSimulatorNaladLengthData(0, new())
                    //    {
                    //        HadBrokenFixedNaladLength = operationB.NaladLengthData[PoolIndex].HadBrokenFixedNaladLength,
                    //        HadBrokenFreeNaladLength = operationB.NaladLengthData[PoolIndex].HadBrokenFreeNaladLength,
                    //        LastLaunchWasFirst = operationB.NaladLengthData[PoolIndex].LastLaunchWasFirst,
                    //        LastRC = operationB.NaladLengthData[PoolIndex].LastRC,
                    //        Data = new NaladLengths.GottenNaladLengthStruct(data.NaladLengthFirst, data.NaladLengthOther, data.NaladLengthNexts, data.NaladLengthHere, data.IsFirst)
                    //    }, 0));
                    //}

                    //var infoToWrite = result.WhatWas[thatEtap.Index];

                    if (info.LastLaunchWasFirst == firstLaunch && info.LastRC == choosenCenter)
                    {
                        // У них одинаковое всё. Нет нужды
                        info.Flag = currentFlag;
                        result.NaladLengthsTotal += info.Data.NaladLengthHere.Value; // Просто критерий для G, S - пока роли не играет.
                        //infoToWrite.info = 1;
                        // НО, Следует всё равно проверить, были ли здесь проблемы.
                        if (info.HadBrokenFixedNaladLength)
                            result.StagesWithBrokenFixedNaladLengths.Add(thatEtap.Index);
                        //else
                        //    result.StagesWithBrokenFixedNaladLengths.Remove(thatEtap.Index);
                        //if (info.HadBrokenFreeNaladLength)
                        //    result.ModellingsWithBrokenFreeNaladLengths.Add(thatEtap.Index);
                    }
                    else
                    {
                        var nalads = //NaladLengthsMasterData.GetNaladLengthsForCurrentStageOnly(thatEtap, choosenCenter, firstLaunch);
                        thatEtap.GetNaladFullLengthFromCache(choosenCenter, firstLaunch ? NaladLengths.RequiredNaladCondition.FirstLaunch : NaladLengths.RequiredNaladCondition.OtherLaunch);//
                        info.Flag = currentFlag;
                        result.NaladLengthsTotal += nalads.NaladLengthHere.Value; // Просто критерий для G, S - пока роли не играет.

                        info.Data.NaladLengthFirst = nalads.NaladLengthFirst; // длина первой катушки
                        info.Data.NaladLengthOther = nalads.NaladLengthOther; // длина остальныъ катушек
                        info.Data.IsFirst = nalads.IsFirst; // первый ли это запуск?

                        //var guid = thatEtap.GetOrderOrPartiaCode();
                        info.LastLaunchWasFirst = firstLaunch;
                        info.LastRC = choosenCenter;

                        if (info.Data.NaladLengthHere.Value != nalads.NaladLengthHere.Value)
                        {
                            //infoToWrite.info = 4;
                            clas.Flag = currentFlag;
                            clas.WillRecalc = true;
                            info.Data.prevKoeff = 1;
                            info.Data.NaladLengthHere = nalads.NaladLengthHere; // сумма всех мест для текущего этапа
                        }
                        else
                        {
                            if (clas.Flag == -1)
                            {
                                //infoToWrite.info = 3;
                                clas.Flag = currentFlag;
                                clas.WillRecalc = true;
                                info.Data.prevKoeff = 1;
                            }
                            //else
                            //{
                            //    infoToWrite.info = 2;
                            //}

                            // НО, Следует всё равно проверить, были ли здесь проблемы.
                            if (info.HadBrokenFixedNaladLength)
                                result.StagesWithBrokenFixedNaladLengths.Add(thatEtap.Index);
                            //else
                            //    result.StagesWithBrokenFixedNaladLengths.Remove(thatEtap.Index);
                        }
                    }

                    //result.WhatWas[thatEtap.Index] = infoToWrite;

                    //if (thatEtap.CustomPartiaGuid.Value.ToString() == "95d16a05-c747-438e-a112-db685beb7ac8")
                    //{
                    //    ;
                    //}

                    if (thatEtap.Nexts.Count == 0 && clas.Flag == currentFlag && clas.WillRecalc)
                    {
                        //bool isLocked = GlobalOrders.TryGetValue(thatEtap.OrderLink.OrderID, out var order)
                        //                && order.PartiaPF.HasValue
                        //                && PFPartiasData.TryGetValue(order.PartiaPF.Value, out var partia)
                        //                && partia.IsNotFree;
                        NaladLengthsRecursiveSet(PoolIndex, op1, 1/*, true*/, currentFlag, result);
                    }
                }



            }
        }

        static void NaladLengthsRecursiveSet(int PoolIndex, PlanSimulatorStage stage, float prevKoeff, int currentFlag, PlanSimulatorEngineResult result)
        {
            var ALL = result.Session.MasterDataLinks.StagesMasterData.AllStages;

            var thatEtap = stage.Stage;//ALL[etapID];

            if (thatEtap.ClosedByZadels) return; // Заглушка для закрытых заделом этапов
            if (thatEtap.IsVolokno) return; // Заглушка для волокон
            //if (thatEtap.CurrentTara.IsEmpty) return; // Заглушка для пустых тар вообще

            // всё, что дальше, сделано по приказу алены и марины, 23.07.24.
            var stageNaladInfo = stage.NaladLengthData[PoolIndex];//pool.NaladLengthsData[etapID];
            ref var stageData = ref stageNaladInfo.Data;
            stageData.prevKoeff = prevKoeff;

            //if (result.WhatWas.TryGetValue(thatEtap.Index, out var infoToWrite))
            //{
            //    infoToWrite.info += 10;
            //    result.WhatWas[thatEtap.Index] = infoToWrite;
            //}
            //else
            //{
            //    result.foundNot.Add(thatEtap.Index);
            //}

                // след. налад. длина
                double nextNalad = stageData.prevKoeff * thatEtap.Count - thatEtap.Count; // "план. потребность + след. длина"
            stageData.NaladLengthNexts.Value = (float)nextNalad; // уже в нужном нам измерении (было назначено в функции выше)

            var koeff = (nextNalad + thatEtap.Count + stageData.NaladLengthHere.Value) / thatEtap.Count; // к длине даём текущую длину, и делим на плановую потребность, получая новый коээфициент

            #region Проверка моделирований

            if (thatEtap.EndTara.IsEmpty && !thatEtap.NoChangeTara && !thatEtap.NoCheckNamotka && !thatEtap.IsVspom && thatEtap.NeededNaladLength.HasValue)
            {
                bool hasError = stageData.NaladLengthHere.Value > 0 && stageData.NaladLengthHere.Value - thatEtap.NeededNaladLength.Value.Length.Value >= 0.001f;
                stageNaladInfo.HadBrokenFixedNaladLength = hasError;
                if (hasError)
                    result.StagesWithBrokenFixedNaladLengths.Add(thatEtap.Index);
                //else
                //    result.StagesWithBrokenFixedNaladLengths.Remove(thatEtap.Index);
            }

            var modGuid = thatEtap.CurrentModelling;//IsCustomModelling ? thatEtap.CustomModelling : thatEtap.MesModelling.Code;

            if (modGuid == Guid.Empty)
            {
                // Это просто этап
                //var naladsHere = stageData.NaladLengthHere + stageNaladInfo.Data.NaladLengthNexts;
                //finalNaladLengthsCheck(naladsHere.Value, thatEtap.Count, thatEtap, modId, tempData);

                NaladLengthsCheckErrors(stageData.NaladLengthHere.Value, stageData.NaladLengthNexts.Value, thatEtap.Count, stage, PoolIndex, modGuid, result);
            }
            else
            {
                // Это моделирование, а то и общий этап. Посмотрим его.
                var thatMod = stage.ModLink;// pool.IDsToMods[modId];

                var countHere = thatMod.Count;

                PlanSimulatorNaladLengthData modNaladInfo;

                List<Stage> modStages;

                if (thatMod.EasyLifeData != null) // Значит это общий этап. Вытаскиваем из него данные по конкретному моделированию.
                {
                    var aseasy = thatMod.EasyLifeData[modGuid];
                    countHere = aseasy.Sum;
                    thatEtap = aseasy.FirstOper;
                    modNaladInfo = aseasy.NaladLengthData[PoolIndex];//pool.NaladLengthsDataAsMods[modGuid];
                    modStages = aseasy.AsMod.AllOpers;
                }
                else
                {
                    thatEtap = thatMod.FirstOperJust;
                    modNaladInfo = thatMod.NaladLengthData[PoolIndex];//pool.NaladLengthsDataAsMods[modGuid];
                    modStages = thatMod.StagesAsNormal;
                }

                ref var modData = ref modNaladInfo.Data;
                modData.prevKoeff = prevKoeff;// у моделирования пред коэфф нафиг не нужен особо, он чисто для отладки сгодится

                if (modNaladInfo.Flag != currentFlag) // Если след. длины были подсчитаны в другом расчете, то надо их сбросить и заполнить заново.
                {
                    modData.NaladLengthFirst = new(0, stageData.NaladLengthFirst.EdIzm); // длина первой катушки
                    modData.NaladLengthOther = new(0, stageData.NaladLengthOther.EdIzm); // длина остальныъ катушек
                    modData.NaladLengthHere = new(0, stageData.NaladLengthHere.EdIzm); // сумма всех мест для текущего этапа
                    modData.NaladLengthNexts = new(0, stageData.NaladLengthNexts.EdIzm); // сумма всех мест пока пуста
                    modNaladInfo.Flag = currentFlag;
                }

                if (stageData.NaladLengthHere.Value > modData.NaladLengthHere.Value)
                {
                    modData.NaladLengthFirst = stageData.NaladLengthFirst; // длина первой катушки
                    modData.NaladLengthOther = stageData.NaladLengthOther; // длина остальныъ катушек
                    modData.NaladLengthHere = stageData.NaladLengthHere; // сумма всех мест для текущего этапа

                }

                var stagesPlanned = modStages.Select(stage => thatMod.BlockLink.PoolLink.StagesLinks[stage.Index]); // Превращаем в стадии из пула - потом ОПТИМИЗИРОВАТЬ
                var stagesWithKids = stagesPlanned.Where(x => x.ModLink.NextMods.Count > 0);
                var grouped = stagesWithKids.GroupBy(x => x.ModLink.NextMods[0]);
                var lengths = grouped.Sum(group => group.Max(stage => stage.NaladLengthData[PoolIndex].Data.NaladLengthNexts.Value));

                modData.NaladLengthNexts.Value = lengths;

                //double nextNaladMod = modData.prevKoeff * countHere - countHere; // "план. потребность + след. длина"

                //if (nextNaladMod > modData.NaladLengthNexts.Value)
                //    modData.NaladLengthNexts.Value = (float)nextNaladMod;// прибавляем к моделированию (к след длине) длину текущего этапа, только здесь можно ещё посчитать без проблем


                modData.IsFirst |= stageData.IsFirst; // моду эта инфа, как и коэфф, тоже не оч нужны, но для отладки пусть будет

                NaladLengthsCheckErrors(modData.NaladLengthHere.Value, modData.NaladLengthNexts.Value, countHere, stage, PoolIndex, modGuid, result);

            }

            #endregion

            for (int i = 0; i < stage.PreviousStages.Count; i++)
            {
                NaladLengthsRecursiveSet(PoolIndex, stage.PreviousStages[i], (float)koeff, currentFlag, result);
            }

        }

        static void NaladLengthsCheckErrors(float naladLengthHere, float naladLengthNext, double modLength, PlanSimulatorStage stage, int PoolIndex, Guid modGuid, PlanSimulatorEngineResult resultToFill)
        {
            var firstEtap = stage.Stage;
            if (!firstEtap.EndTara.IsEmpty) return;
            if (firstEtap.NoChangeTara) return;
            if (firstEtap.IsVspom) return;
            if (firstEtap.NoCheckNamotka) return;
            if (firstEtap.DoNotCheckWindings) return;

            if (firstEtap.CurrentTaraMaxCapacity == 0) return; // а это для тар из ручного РЦ

            //resultToFill.Session.MasterDataLinks.MergeConditionsMasterData.DoNotCheckWindingsSafe(firstEtap.OperationVid.Code, firstEtap.Subdivision.Code)
            var length = naladLengthHere + naladLengthNext;
            length = MathF.Round(length, 3);

            //if (Planning.IgnoredModellingsByFreeNaladLengths.TryGetValue(modGuid, out var lengths) && lengths.ContainsKey(length))
            //{
            //    resultToFill.ModellingsWithBrokenFreeNaladLengths.Remove(modGuid);
            //    return;
            //}

            var hasError = modLength + length > firstEtap.CurrentTaraMaxCapacity;
            stage.NaladLengthData[PoolIndex].HadBrokenFreeNaladLength = hasError;
            if (hasError)
            {
                //resultToFill.ModellingsWithBrokenFreeNaladLengths.Add(modGuid); ВЕРНУТЬ!!!!!!!!!
            }
            else
            {
                //resultToFill.ModellingsWithBrokenFreeNaladLengths.Remove(modGuid); А ВОТ УЖЕ НЕ НАДО
            }
        }

        #endregion
    }
}

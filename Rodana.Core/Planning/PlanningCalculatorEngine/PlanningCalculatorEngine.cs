using Rodana.Core.Planning.PlanningClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Azure;
using Rodana.Core.Centers.MasterData;
using Rodana.Core.Rechanges.MasterData;
using Rodana.Core.Rechanges.RechangesEngine;
using Rodana.Core.Planning.PlanningClasses.PoolClasses;
using Rodana.Core.Stages;
using Rodana.Core.Centers;

namespace Rodana.Core.Planning.PlanningCalculatorEngine
{
    public class PlanningCalculatorEngine
    {
        public static CenterInPlanData FillCenterWithSomeData(CenterInPlanData centerLink, Uco CenterCode, DateTime TimeOfStart)
        {
            centerLink ??= new CenterInPlanData();
            centerLink.LastDate = TimeOfStart; //MinAcceptableDate,
            centerLink.LastStartDate = TimeOfStart; //MinAcceptableDate,
            centerLink.AdditinalConditions = new UniversalBinaryStorage();
            centerLink.OpersTime = centerLink.RechangesTime = centerLink.ProstoiTime = TimeSpan.Zero;
            return centerLink;
        }

        public static PlanningCalculatorEngineResult CalculatePlan(OpersPool pool, DateTime StartTime, PlanningCalculatorEngineResult resultToFill, PlanningCalculatorEngineResult prevResult = null)
        {
            int currentFlag = Random.Shared.Next();


            resultToFill ??= new PlanningCalculatorEngineResult(pool.Blocks[0].Lists[0].FirstOper.SessionLink, StartTime); // Начинаем хотя бы так
            resultToFill.GlobalPlanEndDate = resultToFill.TimeOfStart;// Orders.MinAcceptableDate; // В любом случае на всякий случай задаем это
            resultToFill.MostRechangedRCEndDate = resultToFill.TimeOfStart;// Orders.MinAcceptableDate;

            if (prevResult != null) // Мы копируем их, чтобы не сломать пред. результат + сохранить данные с предыдущего расчёта
            {
                resultToFill.ModellingsWithBrokenFixedNaladLengths = new HashSet<Guid>(prevResult.ModellingsWithBrokenFixedNaladLengths);
                resultToFill.ModellingsWithBrokenFreeNaladLengths = new HashSet<Guid>(prevResult.ModellingsWithBrokenFreeNaladLengths);
            }

            // Проходимся по всем блокам
            for (int blockIndex = 0; blockIndex < pool.Blocks.Count; blockIndex++)
            {
                ProcessBlock(pool, blockIndex, currentFlag, resultToFill);
            }

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
        static void ProcessBlock(OpersPool pool, int blockIndex, int currentFlag, PlanningCalculatorEngineResult resultToFill)
        {
            var currentBlock = pool.Blocks[blockIndex];

            var NonWorkingIntervalsMasterData = resultToFill.Session.MasterDataLinks.WorkingIntervalsMasterData;
            var CentersMasterData = resultToFill.Session.MasterDataLinks.CentersMasterData;
            var RechangesMasterData = resultToFill.Session.MasterDataLinks.RechangesMasterData;
            var AdditionalConditionsMasterData = resultToFill.Session.MasterDataLinks.AdditionalConditionsMasterData;

            // Получаем данные по РЦ
            var ChoosenCenter = currentBlock.ActualRC;
            var centGlobalStruct = pool.CentersData[ChoosenCenter];
            var centerGlobalData = CentersMasterData.CentersData[ChoosenCenter];
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

            // Получаем инфу по всем данным со всех моделирований
            (bool Passed, DatesPoolClass Data)[] ParallelData = new (bool Passed, DatesPoolClass)[currentBlock.Lists.Count];
            int RemainToProcessModellings = currentBlock.Lists.Count; // Сколько ещё моделирований надо прогнать?

            // Сначала просто получаем сырые данные. Тут двигать пока ничего НЕ НАДО из-за нераб интервалов, это случится позже. Тут просто нужно данные получить, и разницу узнать
            for (int i = 0; i < ParallelData.Length; i++)
            {
                ParallelData[i] = (false, FindMinTime(pool, currentBlock.Lists[i], currentFlag, resultToFill.Session.AssignedPlanningAlgorithm.MinAcceptableDate));
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
                PlanningModelling lastMod = centGlobalStruct.LastMod;
                Stage operationA = lastMod?.FirstOper;
                PlanningModelling currentMod = currentBlock.Lists[firstIndex];
                Stage operationB = currentMod.FirstOper;

                // Найдём переналадочку
                var rechangesResult = operationA == null ? null : RechangesMasterData.FindRechanges(new(centGlobalStruct, prostoiData, operationA, operationB, ChoosenCenter));

                // Устанавливаем простои и переналадку здесь
                //CenterTimesClass cts = resultToFill.CentersTimes == null ? null : (resultToFill.CentersTimes.TryGetValue(ChoosenCenter, out var cts2) ? cts2 : null); // Если надо, то мы берем класс РЦ для заполнения
                SetProstoiAndRechanges(ChoosenCenter, centGlobalStruct, prostoiData, rechangesResult, nextPossibleIntervalWillBe, resultToFill, operationB.Index, operationA?.Index ?? -1);

                // Заполняем инфу о всех моделированиях здесь.
                // Здесь мы уже не вызываем чекер, попал ли я в интервал, потому что предыдущий код должен был подвинуть переналадку так, чтобы в худшем случае операция начиналась сразу же после нерабочего интервала. А в лучшем и в принципе где-то посередине.
                TimeSpan LongestMod = TimeSpan.Zero;
                for (int i = 0; i < ChoosenModellings.Count; i++)
                {
                    var index = ChoosenModellings[i];
                    var list = currentBlock.Lists[index];
                    var durationHere = list.AllTime[ChoosenCenter];
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

                // Заполнение данных всяких
                for (int i = 0; i < ChoosenModellings.Count; i++)
                {
                    var index = ChoosenModellings[i];
                    var MOD = currentBlock.Lists[index];
                    var firstID = MOD.FirstOper.Index;

                    // Попробуем заполнить длительности центров, если надо
                    centGlobalStruct.OpersTime += intervalHere.Duration;

                    // Попробуем заполнить даты начала, если надо
                    if (resultToFill.FirstStagesStartDate != null)
                        resultToFill.FirstStagesStartDate.TryAdd(firstID, centGlobalStruct.LastStartDate);

                    // Попробуем заполнить интервалы длительностей, если надо
                    if (resultToFill.FirstStagesFullDurations != null) resultToFill.FirstStagesFullDurations.TryAdd(firstID, intervalHere);

                    // Попробуем заполнить ранги, если надо
                    if (resultToFill.FirstStagesRanks != null) resultToFill.FirstStagesRanks.TryAdd(firstID, resultToFill.CurrentRank);

                    // Попробуем заполнить ранги, если надо
                    if (resultToFill.FirstStagesPositions != null) resultToFill.FirstStagesPositions.TryAdd(firstID, blockIndex);

                    resultToFill.CurrentRank += MOD.OpersList.Count; // В ЛЮБОМ СЛУЧАЕ добавляем
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
                    AdditionalConditionsMasterData.UpdateAdditionalParamsOnCenter(new RechangesEngineData(centGlobalStruct, prostoiData, operationA, operationB, ChoosenCenter));
                }

                // Смотрим на наладочные длины
                ProcessNaladLengths(pool, currentBlock, ChoosenModellings, currentFlag, rechangesResult, ChoosenCenter, lastMod, resultToFill);

                // Если эта операция последняя, то:
                // а. Она будет одна
                // б. Стоит чекнуть заказ
                if (operationB.IsLast)
                {
                    // Считаем заказы
                    ProcessOrder(pool, operationB, lastDateFinally, currentFlag, resultToFill);
                }

                // Попробуем подсчитать переход из цеха в цех
                if (resultToFill.Session.SettingsController.SpecialSettings.CalculateSubdivisionsTransfers)
                {
                    var ordPf = operationB.GetOrderOrPartiaCode();
                    var ordPfClass = pool.LastSubdivisionsData[ordPf];
                    if (ordPfClass.Flag != currentFlag)
                    {
                        // Просто задаем РЦ
                        ordPfClass.Flag = currentFlag;
                        ordPfClass.LastSubdivision = centerGlobalData.Subdivision;
                    }
                    else
                    {
                        // Проверяем, совпало ли подразделение
                        if (ordPfClass.LastSubdivision != centerGlobalData.Subdivision)
                        {
                            ordPfClass.LastSubdivision = centerGlobalData.Subdivision;
                            resultToFill.SubdivisionsTransfer++;
                        }
                    }
                }

                // Задаём минимальные даты начала детям
                SetMinDatesToChilds(pool, currentBlock, blockIndex, ChoosenModellings, lastDateFinally, currentFlag);

                // По итогу:
                centGlobalStruct.LastMod = currentMod;
                //int hashCodeToAdd = operationB.GetHashCode();
                //centGlobalStruct.LocalFlag = HashCode.Combine(centGlobalStruct.LocalFlag, hashCodeToAdd); // локальный флаг
                //resultToFill.GlobalFlag = HashCode.Combine(resultToFill.GlobalFlag, hashCodeToAdd); // глобальный флаг
                RemainToProcessModellings -= ChoosenModellings.Count;
                ChoosenModellings.Clear();
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
        static void SetProstoiAndRechanges(Uco ChoosenCenter, CenterInPlanData centGlobalStruct, DatesPoolClass prostoiData, RechangesEngineResult rechange, int nextPossibleIntervalWillBe, PlanningCalculatorEngineResult resultToFill, int nextStageID, int lastStageID)
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

        static void AlreadyFillProstoi(CenterInPlanData centGlobalStruct, DatesPoolClass prostoiData, CachedInterval prostoiDur, PlanningCalculatorEngineResult resultToFill, int NextStageID)
        {
            centGlobalStruct.LastDate += prostoiDur.Duration;
            resultToFill.ProstoiSeconds += prostoiDur.Duration;
            centGlobalStruct.ProstoiTime += prostoiDur.Duration;

            // Задаём информацию если надо
            if (resultToFill.ProstoiBeforeFirstStages != null)
                resultToFill.ProstoiBeforeFirstStages.TryAdd(NextStageID, (prostoiData, prostoiDur));
        }

        static void AlreadyFillRechanges(CenterInPlanData centGlobalStruct, RechangesEngineResult result, CachedInterval rechangeDur, PlanningCalculatorEngineResult resultToFill, int NextStageID, int lastStageID)
        {
            centGlobalStruct.LastDate += rechangeDur.Duration;
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
        static void SetMinDatesToChilds(OpersPool pool, OpersBlock block, int blockIndex, List<int> ChoosenModellings, DateTime LastDate, int currentFlag)
        {
            for (int i = 0; i < ChoosenModellings.Count; i++) // Проходим по всем вставшим моделированиям
            {
                var mod = block.Lists[ChoosenModellings[i]]; // Берём моделирование

                for (int j = 0; j < mod.NextMods.Count; j++) // Смотрим на детей-модов
                {
                    if (pool.DatesData.TryGetValue(mod.NextMods[j], out var hh)) // Иногда их может не оказаться. Например, если часть партий выкинул.
                    {
                        if (hh.Flag != currentFlag || hh.MinValue < LastDate)
                        {
                            //if (hh.Flag == currentFlag)
                            //{
                            //    tempData.TaraRedutantUsage += (LastDate - hh.MinValue).TotalDays / thisMod.FirstOper.Level;
                            //}
                            hh.MinValue = LastDate;
                            hh.Flag = currentFlag;
                            hh.LastBlockIndex = blockIndex;
                            hh.LastID = mod.FirstOper.Index;
                            //opers.DatesData[next] = hh;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Получает минимально возможную дату для какого-либо моделирования
        /// </summary>
        /// <param name="pool">Пул из которого вытягивать данные</param>
        /// <param name="modelling">Моделирование для которого получаем данные</param>
        /// <param name="neededFlag">Текущий "флаг расчёта"</param>
        /// <returns></returns>
        static DatesPoolClass FindMinTime(OpersPool pool, PlanningModelling modelling, int neededFlag, DateTime minDate)
        {
            var neededDate = modelling.FirstOper.Fixed ? modelling.FirstOper.StartTimeFact.Value : minDate;

            var kortez = pool.DatesData[modelling.ModID];
            if (kortez.Flag == neededFlag)
            {
                if (kortez.MinValue > neededDate)
                {
                    return new DatesPoolClass(kortez);
                }
            }
            return new DatesPoolClass(neededDate, neededFlag, -1, -1);

        }

        /// <summary>
        /// Вынесенный в отдельный метод обработчик параллельных моделирований, чтоб выявить у них самую минимально возможную дату + самый длинный из них, + пометить выполненными часть
        /// </summary>
        /// <param name="ParallelData">Чтообрабатываем</param>
        /// <param name="centerGlobalData">На каком это центре</param>
        /// <param name="ChoosenModellings">Куда вписываем результат</param>
        /// <returns>Минимально возможное начало этой кучи</returns>
        static DateTime ProcessParallelLists((bool Passed, DatesPoolClass Data)[] ParallelData, CenterBaseModel centerGlobalData, List<int> ChoosenModellings)
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

        static void ProcessOrder(OpersPool pool, Stage oper, DateTime final, int currentFlag, PlanningCalculatorEngineResult resultToFill)
        {
            var ordersMasterData = resultToFill.Session.MasterDataLinks.OrdersMasterData;
            var ordern = oper.OrderLink.OrderID;
            // Заказы. Этот метод обрабатывает заказ. Ради удобства вынесен в отдельный локальный метод
            if (ordern.Object.Label.Length > 0)
            {
                var ord = pool.OrdersData[ordern];
                var ordG = ordersMasterData.AllOrders[ordern];
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
            OpersPool pool, OpersBlock block, List<int> ChoosenModellings, int currentFlag,
            RechangesEngineResult RechangesList, Uco choosenCenter, PlanningModelling lastMod,
            PlanningCalculatorEngineResult result
            )
        {
            var ALL = result.Session.MasterDataLinks.StagesMasterData.AllStages;
            var NaladLengthsMasterData = result.Session.MasterDataLinks.NaladLengthsMasterData;
            var GlobalOrders = result.Session.MasterDataLinks.OrdersMasterData.AllOrders;
            var PFPartiasData = result.Session.MasterDataLinks.OrdersMasterData.PFPartiasData;

            for (int i = 0; i < ChoosenModellings.Count; i++) // Проходим по всем вставшим моделированиям
            {
                var mod = block.Lists[ChoosenModellings[i]]; // Берём моделирование

                for (int j = 0; j < mod.OpersList.Count; j++) // Смотрим на детей-модов
                {
                    var op1 = mod.OpersList[j];
                    int etapID = op1.Index;
                    var nalads = NaladLengthsMasterData.GetNaladLengthsForCurrentStageOnly(RechangesList, op1, choosenCenter, lastMod);

                    var info = pool.NaladLengthsData[etapID];
                    var thatEtap = ALL[etapID]; // всё, что дальше, сделано по приказу алены и марины, 23.07.24.
                    info.Flag = currentFlag;
                    info.data.NaladLengthFirst = nalads.NaladLengthFirst; // длина первой катушки
                    info.data.NaladLengthOther = nalads.NaladLengthOther; // длина остальныъ катушек

                    info.data.IsFirst = nalads.IsFirst; // первый ли это запуск?

                    result.NaladLengthsTotal += nalads.NaladLengthHere.Value; // Просто критерий для G, S - пока роли не играет.

                    var guid = thatEtap.GetOrderOrPartiaCode();
                    var clas = pool.NaladLengthsNeedToRecalc[guid];

                    if (info.data.NaladLengthHere != nalads.NaladLengthHere)
                    {
                        clas.Flag = currentFlag;
                        clas.WillRecalc = true;
                        info.data.prevKoeff = 1;
                        info.data.NaladLengthHere = nalads.NaladLengthHere; // сумма всех мест для текущего этапа
                    }
                    else
                    {
                        if (clas.Flag == -1)
                        {
                            clas.Flag = currentFlag;
                            clas.WillRecalc = true;
                            info.data.prevKoeff = 1;
                        }
                    }

                    if (thatEtap.Nexts.Count == 0 && clas.Flag == currentFlag && clas.WillRecalc)
                    {
                        bool isLocked = GlobalOrders.TryGetValue(thatEtap.OrderLink.OrderID, out var order)
                                        && order.PartiaPF.HasValue
                                        && PFPartiasData.TryGetValue(order.PartiaPF.Value, out var partia)
                                        && partia.IsNotFree;
                        NaladLengthsRecursiveSet(pool, etapID, 1/*, true*/, currentFlag, result, isLocked);
                    }
                }
            }
        }

        static void NaladLengthsRecursiveSet(OpersPool pool, int etapID, float prevKoeff, int currentFlag, PlanningCalculatorEngineResult result, bool isLocked)
        {
            var ALL = result.Session.MasterDataLinks.StagesMasterData.AllStages;

            // всё, что дальше, сделано по приказу алены и марины, 23.07.24.
            var stageNaladInfo = pool.NaladLengthsData[etapID];
            ref var stageData = ref stageNaladInfo.data;
            var thatEtap = ALL[etapID];
            stageData.prevKoeff = prevKoeff;

            // след. налад. длина
            double nextNalad = (stageData.prevKoeff * thatEtap.Count) - thatEtap.Count; // "план. потребность + след. длина"
            stageData.NaladLengthNexts.Value = (float)nextNalad; // уже в нужном нам измерении (было назначено в функции выше)

            var koeff = (nextNalad + thatEtap.Count + stageData.NaladLengthHere.Value) / thatEtap.Count; // к длине даём текущую длину, и делим на плановую потребность, получая новый коээфициент

            #region Проверка моделирований

            var modId = pool.OperIDsToModIDs[etapID];

            var modGuid = thatEtap.IsCustomModelling ? thatEtap.CustomModelling : thatEtap.MesModelling.Code;

            if (modGuid == Guid.Empty)
            {
                // Это просто этап
                var naladsHere = stageData.NaladLengthHere + stageNaladInfo.data.NaladLengthNexts;
                //finalNaladLengthsCheck(naladsHere.Value, thatEtap.Count, thatEtap, modId, tempData);
            }
            else
            {
                // Это моделирование, а то и общий этап. Посмотрим его.
                var thatMod = pool.IDsToMods[modId];

                var countHere = thatMod.Count;
                if (thatMod.ObshEtapData != null) // Значит это общий этап. Вытаскиваем из него данные по конкретному моделированию.
                {
                    countHere = thatMod.ObshEtapData[modGuid].Sum;
                    thatEtap = thatMod.ObshEtapData[modGuid].FirstOper;
                }
                else
                    thatEtap = thatMod.FirstOper;

                var modNaladInfo = pool.NaladLengthsDataAsMods[modGuid];
                ref var modData = ref modNaladInfo.data;
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

                double nextNaladMod = (modData.prevKoeff * countHere) - countHere; // "план. потребность + след. длина"

                if (nextNaladMod > modData.NaladLengthNexts.Value)
                    modData.NaladLengthNexts.Value = (float)nextNaladMod;// прибавляем к моделированию (к след длине) длину текущего этапа, только здесь можно ещё посчитать без проблем

                if (thatEtap.EndTara.IsEmpty && !thatEtap.NoChangeTara && !thatEtap.NoCheckNamotka && !thatEtap.IsVspom && thatEtap.NeededNaladLength.HasValue)
                {
                    if (stageData.NaladLengthHere.Value > thatEtap.NeededNaladLength.Value.Length.Value)
                        result.ModellingsWithBrokenFixedNaladLengths.Add(modGuid);
                    else
                        result.ModellingsWithBrokenFixedNaladLengths.Remove(modGuid);
                }

                modData.IsFirst |= stageData.IsFirst; // моду эта инфа, как и коэфф, тоже не оч нужны, но для отладки пусть будет

                NaladLengthsCheckErrors(modData.NaladLengthHere.Value, modData.NaladLengthNexts.Value, countHere, thatEtap, modGuid, result, isLocked);

            }

            #endregion

            for (int i = 0; i < ALL[etapID].Prevs.Count; i++)
            {
                NaladLengthsRecursiveSet(pool, ALL[etapID].Prevs[i], (float)koeff, currentFlag, result, isLocked);
            }

        }

        static void NaladLengthsCheckErrors(float naladLengthHere, float naladLengthNext, double modLength, Stage firstEtap, Guid modGuid, PlanningCalculatorEngineResult resultToFill, bool isLocked)
        {
            if (!firstEtap.EndTara.IsEmpty) return;
            if (firstEtap.NoChangeTara) return;
            if (firstEtap.IsVspom) return;
            if (firstEtap.NoCheckNamotka) return;
            if (resultToFill.Session.MasterDataLinks.MergeConditionsMasterData.DoNotCheckWindingsSafe(firstEtap.OperationVid.Code, firstEtap.Subdivision.Code)) return;

            var length = naladLengthHere + (isLocked ? 0 : naladLengthNext);
            length = MathF.Round(length, 3);

            //if (Planning.IgnoredModellingsByFreeNaladLengths.TryGetValue(modGuid, out var lengths) && lengths.ContainsKey(length))
            //{
            //    resultToFill.ModellingsWithBrokenFreeNaladLengths.Remove(modGuid);
            //    return;
            //}

            if (modLength + length > firstEtap.CurrentTaraMaxCapacity)
            {
                resultToFill.ModellingsWithBrokenFreeNaladLengths.Add(modGuid);
            }
            else
            {
                resultToFill.ModellingsWithBrokenFreeNaladLengths.Remove(modGuid);
            }
        }

        #endregion
    }
}

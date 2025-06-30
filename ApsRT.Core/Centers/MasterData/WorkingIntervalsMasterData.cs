using ApsRT.Core.Planning.PlanningClasses;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Centers.MasterData
{
    public class WorkingIntervalsMasterData
    {
        public Session.Session SessionLink { get; }

        public WorkingIntervalsMasterData(Session.Session session)
        {
            SessionLink = session;
        }

        public void ClearAll()
        {
            //PreparedRCFixingData.Clear();
            //CentersFixingTime.Clear();
            CachedModellingsFullDurations.Clear();
        }

        internal FrozenDictionary<Uco, CachedInterval> PreparedRCFixingData;// = new(); // начало. Позволяет узнать, какой РЦ в какой интервал попал (либо какой будет следующий).

        // 0 - ремонт (черный тогда), 1 - нерабочее время (белый)
        public FrozenDictionary<Uco, List<(DateTime When, TimeSpan Time, int Type)>> CentersFixingTime;// = new(/*new CodedObjectEqualityComparer()*/); // Список ремонтов для каждого из РЦ.

        #region Калькулятор полной длительности любого интервала, с учетом обедов и ремонтов

        /// <summary>
        /// Кэшированные длительности, с учётом нерабочих интервалов. Зависят от РЦ, даты начала и длительности
        /// </summary>
        ConcurrentDictionary<Uco,
            ConcurrentDictionary<DateTime, // начало
                ConcurrentDictionary<int, CachedInterval> // и интервал в зависимости от длительности моделирования
                >
            > CachedModellingsFullDurations = new();

        object lockerCachedModellingsFullDurations = new();
        /// <summary>
        /// Получить интервал с полным временем (с учетом обедов и ремонтов), плюс следующий интервал, плюс все попавшие в меня интервалы
        /// </summary>
        /// <param name="RC">На каком рц</param>
        /// <param name="start">начало интервала</param>
        /// <param name="duration">сколько длится</param>
        /// <returns>Интервал с информацией о попавших в него обедах и ремонтах (они включаются в длительность)</returns>
        public CachedInterval GetDurationWithNonWorkingIntervals(Uco RC, DateTime start, int duration, int startSearchFrom)
        {
            //if (start.TimeOfDay.Hours == 23)
            //{
            //    ;
            //}
            if (startSearchFrom == -1 || CentersFixingTime == null || !CentersFixingTime.TryGetValue(RC, out var list) || startSearchFrom >= list.Count) return new CachedInterval(TimeSpan.FromSeconds(duration), null, -1/*, -1*/);

            //try
            //{
            //    if (CachedModellingsFullDurations.TryGetValue(RC, out var dic1)
            //        && dic1 != null
            //        && dic1.TryGetValue(start, out var durs)
            //        && durs != null
            //        && durs.TryGetValue(duration, out var res)) return res;
            //}
            //catch (Exception ex)
            //{
            //    ;
            //}

            DateTime currentEnd = start.AddSeconds(duration);
            List<int> catchedIntervals = new List<int>();
            int next = list.Count;
            for (int i = startSearchFrom; i < list.Count; i++)
            {
                //if (i >= list.Count) break; // Уже не попаду никуда
                var interval = list[i];
                var intervalEnd = interval.When + interval.Time;
                if (start > intervalEnd) continue; // Если этот интервал закончился раньше меня, то очевидно он не сможет попасть ко мне

                if (currentEnd < interval.When)
                {
                    next = i;
                    break; // Уже не надо.
                }

                if (currentEnd >= interval.When)
                {
                    currentEnd = currentEnd.Add(interval.Time);
                    catchedIntervals.Add(i);
                    next = i + 1; // Сохраняем следующий интервал
                }

            }

            var finalInterval = new CachedInterval(currentEnd - start, catchedIntervals, next);

            //lock (lockerCachedModellingsFullDurations)
            //{
            //    CachedModellingsFullDurations.TryAdd(RC, new());
            //    var rcs = CachedModellingsFullDurations[RC];
            //    rcs.TryAdd(start, new());
            //    var durations = rcs[start];
            //    durations.TryAdd(duration, finalInterval);
            //}

            return finalInterval;
        }

        #endregion

        /// <summary>
        /// Фиксер, который нужен на случай, если дата начала моделирования попала в нерабочий интервал. Этот кэш позволит её подвинуть.
        /// </summary>
        //static Dictionary<Uco,
        //    Dictionary<DateTime, // конец предыдущей
        //        Dictionary<DateTime, // начало следующеей
        //        Dictionary<int, // длительность переналадки
        //        (CachedInterval Prostoi, CachedInterval Rechanges)
        //        >
        //        >
        //        >
        //    > RechangesNonWorkingIntervalsCorrelationCache;

        internal (CachedInterval Prostoi, CachedInterval Rechanges) RechangesNonWorkingIntervalsCorrelation(Uco RC, DateTime endLast, DateTime startNext, int Duration, int startSearchFrom, int lastInterval)
        {
            var durRech = TimeSpan.FromSeconds(Duration);
            if (/*true || */CentersFixingTime == null || !CentersFixingTime.TryGetValue(RC, out var list) || startSearchFrom >= list.Count || lastInterval >= list.Count)
            {
                var prostoiAll = startNext - endLast - durRech;
                if (prostoiAll.Ticks < 0) prostoiAll = TimeSpan.Zero;
                return (new CachedInterval(prostoiAll, null, lastInterval), new CachedInterval(durRech, null, lastInterval));
            }
            else
            {
                // startNext используем, как конец переналадки.
                // lastInterval - это последний индекс интервала, который можно юзать.

                DateTime RechStartDate = startNext - durRech; // Получаем дату начала идеальной переналадки

                List<int> catchedIntervals = new();

                // А вот тут всё есть. Сначала справа налево

                // Если начать с lastInterval, то он считает даже тот интервал, который дальше. Это не совсем правильно.

                for (int i = lastInterval - 1; i >= startSearchFrom; i--)
                {
                    if (i >= list.Count) continue; // Это на всякий случай, если я начал с даты, где уже нет интервалов, но они в теории могли бы быть между нами
                    var interval = list[i];

                    if (interval.When > startNext)
                    {
                        // Слишком далеко, не надо
                        continue;
                    }

                    var intervalEnd = interval.When + interval.Time;
                    if (RechStartDate > intervalEnd) break; // Уже не попадает
                    RechStartDate -= interval.Time; // Попал. Вычитаю.
                    catchedIntervals.Add(i); // Добавляю.

                    if (RechStartDate <= endLast)
                    {
                        // Если я своей переналадок разожрался и убил простои, то выходим
                        // Предварительно отрезок этот двигаем в конец.
                        var delta = endLast - RechStartDate;
                        startNext += delta;
                        RechStartDate -= delta; // А из начала убираем.
                    }
                }

                TimeSpan prostoiSpan = RechStartDate - endLast; // Просто из переналадки (начала её) отнимаем конец пред. моделирования
                if (prostoiSpan.Ticks < 0) prostoiSpan = TimeSpan.Zero;

                var next = lastInterval;
                // А вот теперь - слева направо. Следующие интервалы посмотрим
                for (int i = lastInterval; i < list.Count; i++)
                {
                    //if (i >= list.Count) break; // Уже бессмысленно смотреть дальше, потому что на этом кончился список
                    var interval = list[i];
                    if (startNext <= interval.When) break; // Уже не попадает
                    startNext = startNext.Add(interval.Time);
                    catchedIntervals.Add(i); // Добавляем в список пойманных
                    next = i + 1; // Задаём следующий получается. Потому что текущий оказался внутри интервала переналадки
                    //next = i;
                }


                CachedInterval prostoiInterval = new CachedInterval(prostoiSpan, null, -1);

                CachedInterval rechInterval = new CachedInterval(startNext - RechStartDate, catchedIntervals, next);

                return (prostoiInterval, rechInterval);
            }
        }

        internal void PrepareCentersForMinDataTime(IEnumerable<Uco> centers_choosen, DateTime MinAcceptableDate)
        {
            PreparedRCFixingData = centers_choosen.ToFrozenDictionary(x => x, x => CheckIfCatchedNonWorkingIntervalCached(x, MinAcceptableDate, 0));
            //PreparedRCFixingData.Clear();
            //var TM = /*SessionLink.MasterDataLinks.StagesMasterData.*/MinAcceptableDate;//FIXED_START_TIME.HasValue ? FIXED_START_TIME.Value : MIN_DATE_TIME_AFTER_FIX;
            //foreach (var center in centers_choosen)
            //{
            //    PreparedRCFixingData[center] = CheckIfCatchedNonWorkingIntervalCached(center, TM, 0);
            //}
        }


        #region Фиксер даты начала если дата попала в обед

        /// <summary>
        /// Фиксер, который нужен на случай, если дата начала моделирования попала в нерабочий интервал. Этот кэш позволит её подвинуть.
        /// </summary>
        //static Dictionary<Uco,
        //    Dictionary<DateTime, // начало
        //        CachedInterval
        //        >
        //    > ModellingsStartDateFixers = new();

        //static object lockerModellingsStartDateFixers = new();

        CachedInterval CheckIfCatchedNonWorkingInterval(Uco RC, DateTime start, int startSearchFrom)
        {
            if (CentersFixingTime == null || !CentersFixingTime.TryGetValue(RC, out var list))
            {
                return CachedInterval.Empty;
            }
            else
            {
                // Пройдёмся по всем
                var nextShouldBe = startSearchFrom;
                for (int i = startSearchFrom; i < list.Count; i++)
                {
                    var element = list[i];
                    var endDate = element.When + element.Time;
                    if (start >= element.When && start < endDate)
                    {
                        // О! попал!
                        CachedInterval res = new CachedInterval(endDate - start, // дата конца минус кусок, сколько пройти надо
                            null,
                            //(i == list.Count - 1) ? -1 : (i + 1) // Если это был последний диапазон, то дальше только -1, в противном случае следующий будет этот + 1 (буквально следующий).
                            i + 1 // Самое простое решение - тупо на следующий интервал показать.
                                  //i // Текущий является последним, ведь я в него попал же.
                            );
                        return res;
                    }
                    else
                    {
                        // Не попал. Почему?
                        if (start < element.When)
                        {
                            // Он оказался раньше, чем этот интервал. Значит этот интервал и будет следующим.
                            return new CachedInterval(TimeSpan.Zero, null, i/*, i - 1*/);
                        }
                        // В противном случае - если start >= endDate, просто продолжаем дальше смотреть по списку, другие интервалы.
                    }
                }

                CachedInterval newResult = new CachedInterval(TimeSpan.Zero, null, list.Count/*, list.Count - 1*/);
                return newResult;
            }
        }


        internal CachedInterval CheckIfCatchedNonWorkingIntervalCached(Uco RC, DateTime start, int startSearchFrom)
        {
            if (startSearchFrom == -1)
                return CachedInterval.Empty;
            //if (ModellingsStartDateFixers.TryGetValue(RC, out var dics) && dics.TryGetValue(start, out var res))
            //    return res;

            var newResult = CheckIfCatchedNonWorkingInterval(RC, start, startSearchFrom);

            //if (ModellingsStartDateFixers.TryGetValue(RC, out var dics) && dics.TryGetValue(start, out var res))
            //{
            //    if (res.NextInterval != newResult.NextInterval)
            //    {
            //        ;
            //    }
            //}

            //lock (lockerModellingsStartDateFixers)
            //{
            //    ModellingsStartDateFixers.TryAdd(RC, new());
            //    var dics2 = ModellingsStartDateFixers[RC];
            //    dics2.TryAdd(start, newResult);
            //}

            return newResult;
        }

        #endregion

    }
}

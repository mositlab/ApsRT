using UniversalPack.Common;
using UniversalPack.Data.Entities;
using UniversalPack.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Rodana.Core.Rechanges.RechangesEngine.RechangesEngineExtensions;
using Rodana.Core.Session;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Rechanges.RechangesEngine
{
    // Класс должен быть статичным. Сюда надо перетащить сам поиск переналадок, а так же функцию загрузки доп условий (они оч похожи по смыслу)
    internal static class RechangesEngine
    {

        internal static Vector2? TryConvert(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            value = value.RemoveWhitespaces();
            if (string.IsNullOrEmpty(value)) return null;
            var els = value.Split('-');
            if (els.Length != 2) return null;
            if (string.IsNullOrEmpty(els[0])) return null;
            if (string.IsNullOrEmpty(els[1])) return null;

            if (!els[0].TryConvertTo<float>(out var f1)) return null;
            if (!els[1].TryConvertTo<float>(out var f2)) return null;

            return new Vector2(f1, f2);
        }

        internal static void UniversalDataLoader<A>(Session.Session sessionLink, RechangeCriteriaStorage<A> storage, int index, string from, Guid fromGuid, string to, Guid toGuid, A item)
        {
            // Типовые переналадки

            // С ничего на ничего
            if (from.IsNothingRechange() && to.IsNothingRechange())
            {
                storage.FromNothingToNothing ??= new();
                storage.FromNothingToNothing.Add(item);
            }

            // С ничего на любое
            if (from.IsNothingRechange() && to.IsAnyElseOrAnythingRechange())
            {
                storage.FromNothingToAnyting ??= new();
                storage.FromNothingToAnyting.Add(item);
            }

            // С любого на ничего
            if (from.IsAnyElseOrAnythingRechange() && to.IsNothingRechange())
            {
                storage.FromAnytingToNothing ??= new();
                storage.FromAnytingToNothing.Add(item);
            }

            // С любого на любое другое
            if (from.IsAnyElseOrAnythingRechange() && to.IsAnyElseRechange())
            {
                storage.FromAnythingToAnyElse ??= new();
                storage.FromAnythingToAnyElse.Add(item);
            }

            // С любого на любое
            if (from.IsAnythingRechange() && to.IsAnythingRechange())
            {
                storage.FromAnythingToAnything ??= new();
                storage.FromAnythingToAnything.Add(item);
            }

            // А вот теперь - предметно
            var parameters = sessionLink.MasterDataLinks.RechangesMasterData.ExistingCriterias[index];

            if (parameters.CanBeString)
            {
                storage.StringSubStorage ??= new();
                UniversalAddToSubStorage<string, A>(storage.StringSubStorage, from, from, to, to, item);
            }

            if (parameters.CanBeGuid)
            {
                storage.GuidSubStorage ??= new();
                UniversalAddToSubStorage<Guid?, A>(storage.GuidSubStorage, from, fromGuid, to, toGuid, item);
            }
            if (parameters.CanBeNumber)
            {
                storage.NumberSubStorage ??= new();
                UniversalAddToSubStorage<float?, A>(storage.NumberSubStorage, from, float.TryParse(from.Replace('.', ','), out var valueFrom) ? valueFrom : 0, to, float.TryParse(to.Replace('.', ','), out var valueTo) ? valueTo : 0, item);
            }

            if (parameters.CanBeDiapazon)
            {
                storage.DiapazonSubStorage ??= new();
                Vector2? fromV = TryConvert(from);
                Vector2? toV = TryConvert(to);
                int? fromIndex = fromV.HasValue ? storage.AddOrGetDiapazonByVector(fromV.Value) : null;
                int? toIndex = toV.HasValue ? storage.AddOrGetDiapazonByVector(toV.Value) : null;
                UniversalAddToSubStorage<int?, A>(storage.DiapazonSubStorage, from, fromIndex, to, toIndex, item);

                // На всякий случай ещё с числами поработаем
                if (parameters.CanBeNumber)
                {
                    if (from != NothingLabel && from != AnyElseLabel && float.TryParse(from.Replace('.', ','), out var valueFrom) && toIndex.HasValue)
                    {
                        storage.FromNumberToDiapazon ??= new();
                        storage.FromNumberToDiapazon.TryAdd(valueFrom, new());
                        storage.FromNumberToDiapazon[valueFrom].TryAdd(toIndex.Value, new());
                        storage.FromNumberToDiapazon[valueFrom][toIndex.Value].Add(item);
                    }

                    if (to != NothingLabel && to != AnyElseLabel && float.TryParse(to.Replace('.', ','), out var valueTo) && fromIndex.HasValue)
                    {
                        storage.FromDiapazonToNumber ??= new();
                        storage.FromDiapazonToNumber.TryAdd(fromIndex.Value, new());
                        storage.FromDiapazonToNumber[fromIndex.Value].TryAdd(valueTo, new());
                        storage.FromDiapazonToNumber[fromIndex.Value][valueTo].Add(item);
                    }
                }
            }
        }

        static void UniversalAddToSubStorage<T, A>(RechangeCriteriaSubStorage<T, A> subStorage, string from, T fromValue, string to, T toValue, A item)
        {
            if (from.IsNothingRechange())
            {
                if (to.IsSomeValueRechange() && toValue != null)
                {
                    subStorage.FromNothingToValue ??= new();
                    subStorage.FromNothingToValue.TryAdd(toValue, new());
                    subStorage.FromNothingToValue[toValue].Add(item);
                }
            }
            else
            {
                // С любого (втч этого) на значение
                if (from.IsAnythingRechange() && to.IsSomeValueRechange() && toValue != null)
                {
                    subStorage.FromAnythingToValue ??= new();
                    subStorage.FromAnythingToValue.TryAdd(toValue, new());
                    subStorage.FromAnythingToValue[toValue].Add(item);
                }

                // C значения на любое (втч это)
                if (from.IsSomeValueRechange() && to.IsAnythingRechange() && fromValue != null)
                {
                    subStorage.FromValueToAnything ??= new();
                    subStorage.FromValueToAnything.TryAdd(fromValue, new());
                    subStorage.FromValueToAnything[fromValue].Add(item);
                }

                // С любого другого на значения
                if (from.IsAnyElseRechange() && to.IsSomeValueRechange() && toValue != null)
                {
                    subStorage.FromAnyElseToValue ??= new();
                    subStorage.FromAnyElseToValue.TryAdd(toValue, new());
                    subStorage.FromAnyElseToValue[toValue].Add(item);
                }

                // Со значения на любой другой
                if (from.IsSomeValueRechange() && to.IsAnyElseRechange() && fromValue != null)
                {
                    subStorage.FromValueToAnyElse ??= new();
                    subStorage.FromValueToAnyElse.TryAdd(fromValue, new());
                    subStorage.FromValueToAnyElse[fromValue].Add(item);
                }

                // Со значения на ничего
                if (from.IsSomeValueRechange() && to.IsNothingRechange() && fromValue != null)
                {
                    subStorage.FromValueToNothing ??= new();
                    subStorage.FromValueToNothing.TryAdd(fromValue, new());
                    subStorage.FromValueToNothing[fromValue].Add(item);
                }

                // Со значения на значение
                if (to.IsSomeValueRechange() && from.IsSomeValueRechange() && toValue != null && fromValue != null)
                {
                    subStorage.FromValueToValue ??= new();
                    subStorage.FromValueToValue.TryAdd(fromValue, new());
                    subStorage.FromValueToValue[fromValue].TryAdd(toValue, new());
                    subStorage.FromValueToValue[fromValue][toValue].Add(item);
                }
            }
        }


        /// <summary>
        /// Обрабатываем два этапа (точнее, их параметры) в зависимости от того, как выбранный критерий переналадки настроен
        /// </summary>
        /// <typeparam name="A">Тип действия (переналадка или доп. условия?)</typeparam>
        /// <typeparam name="Result">Тип результата, который передастся в финальный метод</typeparam>
        /// <param name="CriteriaIndex">Индекс критерия</param>
        /// <param name="engineData">Данные по движку переналадок, нужны для получения значения критерия</param>
        /// <param name="storageToWorkWith">Хранилище данных, с которым работает метод (найдено ранее, просто передаётся в него)</param>
        /// <param name="result">Ячейка с результатом, просто передаётся в финальный метод для записи</param>
        /// <param name="DiapazionsDataIndexes">Уточняемый номер хранилища диапазонов (0 - переналадки, 1 - доп. условия)</param>
        /// <param name="MethodToWrite">Финальный метод, используемый для записи результатов. В частных случаях используется уже здесь.</param>
        /// <param name="methodToCalcDiapazons">Метод, который иногда может быть использован для подсчёта диапазонов (используется уже здесь)</param>
        internal static void ProcessRechangeGlobalParam<A, Result>(
            int CriteriaIndex,
            RechangesEngineData engineData,
            RechangeCriteriaStorage<A> storageToWorkWith,
            int DiapazionsDataIndexes,
            Result result,
            Action<Result, List<A>, RechangesEngineData, int> MethodToWrite,
            Func<Uco, Uco, int, float?, int?> methodToCalcDiapazons)
        {
            var MasterData = engineData.ObjectA.SessionLink.MasterDataLinks.RechangesMasterData;

            var GlobalParams = MasterData.ExistingCriterias[CriteriaIndex];

            var paramsA = engineData.ObjectA.RechangeParameters[CriteriaIndex];
            var paramsB = engineData.ObjectB.RechangeParameters[CriteriaIndex];

            var operToWorkWith = DiapazionsDataIndexes == Rechanges.MasterData.RechangesMasterData.RECHANGES_INDEX_FOR_DIAPAZONS ? engineData.ObjectB.OperationVid : Uco.EmptyObject;

            var count1 = paramsA.GetValuesCount();
            var count2 = paramsB.GetValuesCount();
            var howMuchCount = (GlobalParams.CanBeMultiple ? Math.Min(count1, count2) : 1); // Раньше тут было MAX, но если так подумать, это спровоцирует лишние переналадки например у красителей (ЗЕЛ-КРАС, ЖЕЛТ-NULL => переналадка, ибо один отсутствует).

            // Может быть числом? Проверим.
            if (GlobalParams.CanBeNumber)
            {
                for (int i = 0; i < howMuchCount; i++)
                {
                    float? val1 = paramsA.GetFloat(i, engineData);
                    float? val2 = paramsB.GetFloat(i, engineData);
                    ProcessDataTypeAndWrite<float?, A, Result>(engineData, CriteriaIndex, storageToWorkWith, result, storageToWorkWith.NumberSubStorage, val1, val2, MethodToWrite);
                }
            }

            int?[] arrayToCheckA = null;
            int?[] arrayToCheckB = null;
            // Может быть диапазоном? Проверим.
            if (GlobalParams.CanBeDiapazon)
            {
                if (GlobalParams.IsDynamic)
                {
                    // Если я динамичный, то придется создать собственный массив - если я буду сидеть в массиве диапазона, то может сломаться многопоточность (когда его начнут читать сразу несколько потоков)
                    arrayToCheckA = new int?[howMuchCount];
                    arrayToCheckB = new int?[howMuchCount];
                }
                else
                {
                    arrayToCheckA = paramsA.GetDiapazonIndexes(DiapazionsDataIndexes, engineData.ActualRC, operToWorkWith);
                    arrayToCheckB = paramsB.GetDiapazonIndexes(DiapazionsDataIndexes, engineData.ActualRC, operToWorkWith);
                }

                for (int i = 0; i < howMuchCount; i++)
                {
                    if (GlobalParams.IsDynamic)
                    {
                        // Если я динамичен, то придется задать им значение здесь
                        arrayToCheckA[i] = methodToCalcDiapazons(engineData.ActualRC, operToWorkWith, CriteriaIndex, paramsA.GetFloat(i, engineData));
                        arrayToCheckB[i] = methodToCalcDiapazons(engineData.ActualRC, operToWorkWith, CriteriaIndex, paramsB.GetFloat(i, engineData));
                    }
                    int? val1 = (i >= arrayToCheckA.Length) ? null : arrayToCheckA[i];
                    int? val2 = (i >= arrayToCheckB.Length) ? null : arrayToCheckB[i];
                    ProcessDataTypeAndWrite<int?, A, Result>(engineData, CriteriaIndex, storageToWorkWith, result, storageToWorkWith.DiapazonSubStorage, val1, val2, MethodToWrite);
                }
            }

            // Может быть гуидом? Проверим.
            if (GlobalParams.CanBeGuid)
            {
                for (int i = 0; i < howMuchCount; i++)
                {
                    Guid? val1 = paramsA.GetGuid(i, engineData);
                    Guid? val2 = paramsB.GetGuid(i, engineData);
                    ProcessDataTypeAndWrite<Guid?, A, Result>(engineData, CriteriaIndex, storageToWorkWith, result, storageToWorkWith.GuidSubStorage, val1, val2, MethodToWrite);
                }
            }

            // Может быть строкой? Проверим.
            if (GlobalParams.CanBeString)
            {
                for (int i = 0; i < howMuchCount; i++)
                {
                    string val1 = paramsA.GetString(i, engineData);
                    string val2 = paramsB.GetString(i, engineData);
                    ProcessDataTypeAndWrite<string, A, Result>(engineData, CriteriaIndex, storageToWorkWith, result, storageToWorkWith.StringSubStorage, val1, val2, MethodToWrite);
                }
            }

            // Попробуем ещё и переход число-диапазон
            if (GlobalParams.CanBeNumber && GlobalParams.CanBeDiapazon)
            {
                if (storageToWorkWith.FromNumberToDiapazon != null)
                {
                    for (int i = 0; i < howMuchCount; i++)
                    {
                        float? val1 = paramsA.GetFloat(i, engineData);
                        int? val2 = (i >= count2) ? null : arrayToCheckB[i];
                        if (val1.HasValue && val2.HasValue && storageToWorkWith.FromNumberToDiapazon.TryGetValue(val1.Value, out var lvl1) && lvl1.TryGetValue(val2.Value, out var rech)) MethodToWrite(result, rech, engineData, CriteriaIndex);
                    }
                }
                if (storageToWorkWith.FromDiapazonToNumber != null)
                {
                    for (int i = 0; i < howMuchCount; i++)
                    {
                        int? val1 = (i >= count1) ? null : arrayToCheckA[i];
                        float? val2 = paramsB.GetFloat(i, engineData);
                        if (val1.HasValue && val2.HasValue && storageToWorkWith.FromDiapazonToNumber.TryGetValue(val1.Value, out var lvl1) && lvl1.TryGetValue(val2.Value, out var rech)) MethodToWrite(result, rech, engineData, CriteriaIndex);
                    }

                }
            }
        }


        /// <summary>
        /// Обрабатываем определенный тип данных (строку, гуид, число, здесь это уже неважно), обязательно заходя в под-хранилище данных, сравнивая данные, и перенаправляя в метод установки значений, если для этой пары значений есть переналадки/доп. условия.
        /// </summary>
        /// <typeparam name="T">Тип данных (число, строка, гуид?)</typeparam>
        /// <typeparam name="A">тип действия (это переналадка или установка доп. условий для РЦ?)</typeparam>
        /// <typeparam name="Result">Тип результата, с которым будет работать представленный здесь же метод</typeparam>
        /// <param name="engineData">Ссылка на данные движка переналадок для текущего поиска</param>
        /// <param name="CriteriaIndex">Индекс критерия, по которому идёт работа</param>
        /// <param name="mainStorage">Основное хранилище списков переналадок/доп. условий</param>
        /// <param name="result">Переменная-результат, сюда метод запишет результат</param>
        /// <param name="subStorage">Доп. хранилище, уже для конкретного типа данных</param>
        /// <param name="val1">Значение "переход с" для сравнения</param>
        /// <param name="val2">Значение "переход на" для сравнения</param>
        /// <param name="MethodToWrite">Метод, который вызывается, если движок что-то нашёл в хранилищах, и надо записать результат. Как результат запишется, уже вопрос к методу, в него лишь передаются данные.</param>
        static void ProcessDataTypeAndWrite<T, A, Result>(
            RechangesEngineData engineData,
            int CriteriaIndex,
            RechangeCriteriaStorage<A> mainStorage,
            Result result,
            RechangeCriteriaSubStorage<T, A> subStorage,
            T val1, T val2,
            Action<Result, List<A>, RechangesEngineData, int> MethodToWrite
            )
        {
            // ЕСЛИ ОТСУТСТВУЕТ:
            // Отсутствует - отсутствует ЕСТЬ
            // Отсутствует - любой ЕСТЬ
            // Отсутствует - конкретный ЕСТЬ

            // ИНАЧЕ:
            // Любой - отсутствует ЕСТЬ
            // Конкретный - отсутствует ЕСТЬ

            // Конкретный - конкретный ЕСТЬ

            // Конкретный - на любой ЕСТЬ
            // Любой - конкретный ЕСТЬ
            // Любой - Любой ЕСТЬ

            // Конкретный - на любой другой ЕСТЬ
            // Любой другой - конкретный ЕСТЬ
            // Любой другой - на любой другой ЕСТЬ

            // С ничего на ничего
            if (val1 == null && val2 == null && mainStorage.FromNothingToNothing != null) MethodToWrite(result, mainStorage.FromNothingToNothing, engineData, CriteriaIndex);

            // Если переход С пуст, а переход НА нет
            if (val1 == null && val2 != null)
            {
                // С ничего на любой
                if (mainStorage.FromNothingToAnyting != null) MethodToWrite(result, mainStorage.FromNothingToAnyting, engineData, CriteriaIndex);

                // С ничего на значение
                if (subStorage != null
                    && subStorage.FromNothingToValue != null
                    && subStorage.FromNothingToValue.TryGetValue(val2, out var rech)) MethodToWrite(result, rech, engineData, CriteriaIndex);
            }

            // Если переход С имеется
            if (val1 != null)
            {
                // Если переход НА пустой
                if (val2 == null)
                {
                    // С любого на ничего
                    if (mainStorage.FromAnytingToNothing != null) MethodToWrite(result, mainStorage.FromAnytingToNothing, engineData, CriteriaIndex);

                    // Со значения на ничего
                    if (subStorage != null
                    && subStorage.FromValueToNothing != null
                    && subStorage.FromValueToNothing.TryGetValue(val1, out var rech)) MethodToWrite(result, rech, engineData, CriteriaIndex);
                }

                // Если есть значение (отличается или нет, увидим ниже)
                if (val2 != null)
                {
                    // Со значения на значение (всегда проверяется)
                    if (subStorage != null
                     && subStorage.FromValueToValue != null
                     && subStorage.FromValueToValue.TryGetValue(val1, out var level1)
                     && level1 != null && level1.TryGetValue(val2, out var rech3)) MethodToWrite(result, rech3, engineData, CriteriaIndex);

                    // Они отличаются?
                    bool isEqual = val1.Equals(val2);

                    if (isEqual)
                    {
                        // Если НЕ отличаются, то мы юзаем "любой"

                        // Со любого на значение
                        if (subStorage != null
                         && subStorage.FromAnythingToValue != null
                         && subStorage.FromAnythingToValue.TryGetValue(val2, out var rech1)) MethodToWrite(result, rech1, engineData, CriteriaIndex);

                        // Со значения на любой
                        if (subStorage != null
                         && subStorage.FromValueToAnything != null
                         && subStorage.FromValueToAnything.TryGetValue(val1, out var rech2)) MethodToWrite(result, rech2, engineData, CriteriaIndex);

                        // С любого на любой
                        if (mainStorage.FromAnythingToAnything != null) MethodToWrite(result, mainStorage.FromAnythingToAnything, engineData, CriteriaIndex);
                    }
                    else
                    {
                        // Если отличаются, то нужно юзать "любой другой"

                        // Со любого другого на значение
                        if (subStorage != null
                         && subStorage.FromAnyElseToValue != null
                         && subStorage.FromAnyElseToValue.TryGetValue(val2, out var rech1)) MethodToWrite(result, rech1, engineData, CriteriaIndex);

                        // Со значения на любой другой
                        if (subStorage != null
                         && subStorage.FromValueToAnyElse != null
                         && subStorage.FromValueToAnyElse.TryGetValue(val1, out var rech2)) MethodToWrite(result, rech2, engineData, CriteriaIndex);

                        // С любого на любой другой
                        if (mainStorage.FromAnythingToAnyElse != null) MethodToWrite(result, mainStorage.FromAnythingToAnyElse, engineData, CriteriaIndex);
                    }

                }

            }
        }

    }
}

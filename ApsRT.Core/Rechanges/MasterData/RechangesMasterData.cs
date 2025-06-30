using UniversalPack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfimumMES;
using UniversalPack.Data.Entities;
using System.Diagnostics.PerformanceData;
using UniversalPack.Extensions;
using System.Numerics;
using System.Collections.Frozen;
using ApsRT.Core.Rechanges.RechangesEngine;
using ApsRT.Core.Session;
using ApsRT.Core.Rechanges.Hoppers;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Rechanges.MasterData
{

    /// <summary>
    /// Класс-синглтон для хранения и расчёта переналадок
    /// </summary>
    public class RechangesMasterData
    {
        public Session.Session SessionLink { get; }

        public RechangesMasterData(Session.Session session)
        {
            SessionLink = session;
        }

        internal const int RECHANGES_INDEX_FOR_DIAPAZONS = 0;

        #region Данные и их загрузка

        Dictionary<
            Uco, // РЦ
            Dictionary<Uco, // Вид операции
                RechangeCriteriaStorage<Rechange>[]
                >
            > AllRechangesOnLoad = new();

        FrozenDictionary<Uco, // РЦ
            FrozenDictionary<Uco, // Вид операции
                RechangeCriteriaStorage<Rechange>[]
                >
            > AllRechanges;

        internal void AddRechangeData(RechangeDataLoaderModel model)
        {
            Uco RC = model.Center; // РЦ
            Uco VidOper = model.Operation; // Вид операции
            string rawCriteriaName = model.Criteria;
            string from = model.From;
            Guid? fromGuid = model.FromGuid;
            string to = model.To;
            Guid? toGuid = model.ToGuid;
            Rechange rechange = new Rechange(model);
            var index = GetCriteriaIndexByName(rawCriteriaName);
            if (index == -1)
            {
                SessionLink.AssignedLogger.LogQueue.Enqueue($"ЗАГРУЗКА (переналадки): Неизвестный критерий '{rawCriteriaName}' переналадки.\r\n");
            }
            else
            {
                if (!SessionLink.MasterDataLinks.CentersMasterData.CentersData.TryGetValue(RC, out var rc))
                {
                    // Можно сюда вывести лог.
                    return;
                }
                rc.PossibleRechanges ??= new();
                rc.PossibleRechanges.SetDataBit(index, true);
                rc.IsWithRechanges = true;
                if (rechange.HasAdditionalCondition) rc.HasAdditionalActions = true;

                AllRechangesOnLoad.TryAdd(RC, new());
                var RCs = AllRechangesOnLoad[RC];
                RCs.TryAdd(VidOper, new RechangeCriteriaStorage<Rechange>[ExistingCriterias.Length]);
                var storages = RCs[VidOper];
                storages[index] ??= new RechangeCriteriaStorage<Rechange>();
                var storage = storages[index];

                RechangesCashe.TryAdd(RC, new());

                RechangesEngine.RechangesEngine.UniversalDataLoader<Rechange>(SessionLink, storage, index, from, fromGuid, to, toGuid, rechange);
            }
        }


        internal int? CalculateDiapazonIndex(
            Uco RC, // РЦ
            Uco VidOper, // Вид операции
            int CriteriaIndex,
            float? value
            )
        {
            if (!value.HasValue) return null;
            if (AllRechanges == null
                || !AllRechanges.TryGetValue(RC, out var opers) 
                || opers == null
                || !opers.TryGetValue(VidOper, out var array)
                || array == null
                || array[CriteriaIndex] == null) return null;
            return array[CriteriaIndex].GetDiapazonByValue(value.Value);
        }

        internal Uco[] GetAvailableOperVids(Uco RC)
        {
            if (AllRechanges == null || !AllRechanges.TryGetValue(RC, out var opers) || opers == null) return null;
            return opers.Keys.ToArray();
        }

        Dictionary<Uco, // РЦ
            Dictionary<int, // этап 1
                Dictionary<int, // этап 2
                    Dictionary<UniversalBinaryStorage, RechangesEngineResult> // В зависимости от условий ещё
                    >
                >
            > RechangesCashe = new();

        internal void ClearRechangesInfo()
        {
            RechangesCashe.Clear();
            AllRechangesOnLoad.Clear();
            AllRechanges = null;
        }

        internal void BakeRechangesInfo()
        {
            AllRechanges = AllRechangesOnLoad.ToFrozenDictionary(x => x.Key, x => x.Value.ToFrozenDictionary(y => y.Key, y => y.Value));
        }

        object rechangeCashLocker = new();

        public RechangeUniversalCriteria[] ExistingCriterias;

        internal int GetCriteriaIndexByName(string rawCriteriaName)
        {
            if (ExistingCriterias == null) return -1;
            var index = Array.FindIndex(ExistingCriterias, x => x.Name == rawCriteriaName);
            return index;
        }

        #endregion

        #region Превалирующие переналадки
        /// <summary>
        /// Переналадки, которые превалируют над другими
        /// </summary>
        Dictionary<Guid, (int Level, HashSet<Guid> PrevailsList)> PrevailingRechangesOnLoad = new();

        FrozenDictionary<Guid, (int Level, HashSet<Guid> PrevailsList)> PrevailingRechanges;

        /// <summary>
        /// Максимальное количество уровней, которые позволяют располагаться переналадкам (от 0 до данного числа ИСКЛЮЧИТЕЛЬНО). Чем выше, тем больше уровней возможно, но тем медленнее будет работать движое переналадок.
        /// </summary>
        internal int MaxPrevailingLevel { get; set; } = 2;

        internal void AddPrevailingRechange(PrevailingRechangesDataLoaderModel model)
        {
            var prevailing = model.PrevailingRechange;
            var level = model.Level;
            var sub = model.SubRechange;
            PrevailingRechangesOnLoad.TryAdd(prevailing, (level, new()));
            PrevailingRechangesOnLoad[prevailing].PrevailsList.Add(sub);
        }

        internal void BakePrevailingRechanges()
        {
            PrevailingRechanges = PrevailingRechangesOnLoad.ToFrozenDictionary(y => y.Key, y => y.Value);
        }

        internal void ClearPrevailingRechanges()
        {
            PrevailingRechanges = null;
            PrevailingRechangesOnLoad.Clear();
        }

        #endregion

        #region Многокритериальные переналадки

        Dictionary<Uco, // РЦ
            Dictionary<Uco, // Вид операции
                Dictionary<Uco, // Собственно сама переналадка
                UniversalBinaryStorage
                >
                >
            > RechangesMultiCriterialDataOnLoad = new();

        FrozenDictionary<Uco, // РЦ
            FrozenDictionary<Uco, // Вид операции
                FrozenDictionary<Uco, // Собственно сама переналадка
                UniversalBinaryStorage
                >
                >
            > RechangesMultiCriterialData;

        internal void AddMultiCriterialRechangeInfo(RechangeDataLoaderModel model, int NeededCriteria)
        {
            Uco RC = model.Center;
            Uco OperVid = model.Operation;
            Uco rechange = model.RechangeOperation;
            RechangesMultiCriterialDataOnLoad.TryAdd(RC, new());
            RechangesMultiCriterialDataOnLoad[RC].TryAdd(OperVid, new());
            RechangesMultiCriterialDataOnLoad[RC][OperVid].TryAdd(rechange, new UniversalBinaryStorage());
            RechangesMultiCriterialDataOnLoad[RC][OperVid][rechange].SetDataBit(NeededCriteria, true);
        }

        internal void BakeMultiCriterialRechangesInfo()
        {
            RechangesMultiCriterialData = RechangesMultiCriterialDataOnLoad
                .ToFrozenDictionary(x => x.Key,
                    x => x.Value.ToFrozenDictionary(y => y.Key,
                        y => y.Value.ToFrozenDictionary(z => z.Key, z => z.Value)
                        )
                    );
        }

        internal void EraseMultiCriterialRechangesInfo()
        {
            RechangesMultiCriterialData = null;
            RechangesMultiCriterialDataOnLoad.Clear();
        }

        #endregion

        #region Поиск переналадок

        internal RechangesEngineResult FindRechanges(RechangesEngineData engineData)
        {
            if (RechangesCashe.TryGetValue(engineData.ActualRC, out var lvl10) &&
                lvl10.TryGetValue(engineData.ObjectA.Index, out var lvl20) &&
                lvl20.TryGetValue(engineData.ObjectB.Index, out var lvl30) &&
                lvl30.TryGetValue(engineData.CenterData.AdditinalConditions, out var result0)) return result0;

            SessionLink.MasterDataLinks.CentersMasterData.CentersData.TryGetValue(engineData.ActualRC, out var centerClass);

            var hasRechanges = centerClass != null ? centerClass.IsWithRechanges : false;

            if (!hasRechanges)
            {
                return new RechangesEngineResult(null, null, 0, false);
            }

            var mask = centerClass.PossibleRechanges;

            if (mask == null || mask.IsEmpty)
            {
                return new RechangesEngineResult(null, null, 0, false);
            }

            //SortedDictionary<int, Dictionary<Guid, Rechange>>
            RechangesEngineResult result = new(new Dictionary<Guid, Rechange>[MaxPrevailingLevel], new(), 0, false);
            var arraysRech = AllRechanges.TryGetValue(engineData.ActualRC, out var opers) ? (opers.TryGetValue(engineData.ObjectB.OperationVid, out var arrays) ? arrays :
                (opers.TryGetValue(Uco.EmptyObject, out var arrays2) ? arrays2 : null)
                ) : null;
            for (int cr = 0; cr < ExistingCriterias.Length; cr++)
            {
                if (mask.GetDataBit(cr))
                {
                    GetRechangesListByCriteria(engineData, cr, result, arraysRech);
                }
            }

            for (int i = 0; i < MaxPrevailingLevel; i++)
            {
                if (result.Result[i] != null) result.Result[i] = result.Result[i].Where(x =>
                {
                    var rechange = x.Value;
                    if (!rechange.IsMultiCriterial) return true;

                    // Берём данные, полученные от движка
                    var b = result.MultiCriterialInfo.TryGetValue(rechange.Name.Code, out var HerBinary);

                    if (!b) return true; // Если у нее нет данных (ПОЧЕМУ-ТО), то выходим

                    UniversalBinaryStorage NeededBinary = null;
                    // Берем глобальные данные
                    // Кэшировать это бесполезно, ведь внутри класса переналадки всё равно придется хранить словарь с РЦ+видом, потому что она может на разные вставать же, у нее в зависимости от РЦ и вида разные маски. Ну и смысла 0 по факту.
                    if (RechangesMultiCriterialData.TryGetValue(engineData.ActualRC, out var operVids))
                    {
                        if (operVids.TryGetValue(engineData.ObjectB.OperationVid, out var rechanges))
                        {
                            if (rechanges.TryGetValue(rechange.Name, out var neededBinary))
                                NeededBinary = neededBinary;
                        }
                        else
                        {
                            if (operVids.TryGetValue(Uco.EmptyObject, out var rechanges2))
                            {
                                if (rechanges2.TryGetValue(rechange.Name, out var neededBinary))
                                    NeededBinary = neededBinary;
                            }
                        }
                    }

                    if (NeededBinary == null)
                    {
                        // Если переналадка многокритериальная, но для нее не заданы параметры, то придется пропустить её
                        return true;
                    }

                    return HerBinary.IsEqualWithThis(NeededBinary); // Применив к полученным данным глобальную маску (т.е. сравнивать только то что надо), можно узнать, все ли критерии выполнены

                }).ToDictionary(x => x.Key, x => x.Value);
            }

            for (int i = MaxPrevailingLevel - 1; i > 0; i--)
            {
                if (result.Result[i - 1] != null && result.Result[i] != null) // Следующий список есть (и текущий тоже)? В котором На удаление
                {
                    var listHere = result.Result[i]; // Беру текущий список
                    var NextList = result.Result[i - 1]; // Получаю след список
                    foreach (var key in listHere) // Прохожусь по текущему списку
                    {
                        if (PrevailingRechanges.TryGetValue(key.Key, out var data) && data.PrevailsList != null) // Попробуем получить список переналадок, над которыми я удерживаю превосходство
                        {
                            foreach (var toDel in data.PrevailsList) NextList.Remove(toDel); // Удаляем все переналадки, над которыми мы властны
                        }
                    }
                }
            }

            result.UpdateFields();

            if (result.IsDynamicSolution)
            {
                // В динамический словарь
            }
            else
            {
                // В статичный
                lock (rechangeCashLocker)
                {
                    var lvl1 = RechangesCashe[engineData.ActualRC];
                    lvl1.TryAdd(engineData.ObjectA.Index, new Dictionary<int, Dictionary<UniversalBinaryStorage, RechangesEngine.RechangesEngineResult>>());
                    var lvl2 = lvl1[engineData.ObjectA.Index];
                    lvl2.TryAdd(engineData.ObjectB.Index, new Dictionary<UniversalBinaryStorage, RechangesEngineResult>());
                    var lvl3 = lvl2[engineData.ObjectB.Index];
                    lvl3.TryAdd(engineData.CenterData.AdditinalConditions, result);
                }
            }


            return result;
        }

        internal void GetRechangesListByCriteria(RechangesEngineData engineData, int CriteriaIndex, RechangesEngineResult result, RechangeCriteriaStorage<Rechange>[] RechangesToAcquireArray = null)
        {
            RechangesToAcquireArray ??= AllRechanges.TryGetValue(engineData.ActualRC, out var opers) ?
                (opers.TryGetValue(engineData.ObjectB.OperationVid, out var arrays) ? arrays :
                (opers.TryGetValue(Uco.EmptyObject, out var arrays2) ? arrays2 : null) // Попробуем ещё пустую операцию поискать
                )
                : null;

            if (RechangesToAcquireArray == null) return;

            var RechangesToAcquire = RechangesToAcquireArray[CriteriaIndex];

            if (RechangesToAcquire == null) return;

            result ??= new RechangesEngineResult(new Dictionary<Guid, Rechange>[MaxPrevailingLevel], new(), 0, false);//SortedDictionary<int, Dictionary<Guid, Rechange>>();

            var GlobalParams = ExistingCriterias[CriteriaIndex];

            // Указываем, динамичный ли критерий
            result.IsDynamicSolution |= GlobalParams.IsDynamic;

            // Обрабатывам данные согласно настройкам выбранного критерия переналадки
            RechangesEngine.RechangesEngine.ProcessRechangeGlobalParam<Rechange, RechangesEngineResult>(CriteriaIndex, engineData, RechangesToAcquire, RECHANGES_INDEX_FOR_DIAPAZONS, result, AddRechangeWithAdditionalConditionsCheck, CalculateDiapazonIndex);
        }


        void AddRechangeWithAdditionalConditionsCheck(RechangesEngineResult whereToAdd, List<Rechange> RechangeToCheckList, RechangesEngineData engineData, int CriteriaIndex)
        {
            var centerData = engineData.CenterData;
            for (int i = 0; i < RechangeToCheckList.Count; i++)
            {
                var RechangeToCheck = RechangeToCheckList[i];
                int RechangeLevel = 0;
                var rechangeGuid = RechangeToCheck.Name.Code;
                if (PrevailingRechanges != null && PrevailingRechanges.TryGetValue(rechangeGuid, out var data)) RechangeLevel = data.Level;

                var addMasterData = SessionLink.MasterDataLinks.AdditionalConditionsMasterData;

                if (!RechangeToCheck.HasAdditionalCondition
                    || addMasterData.AdditionalConditions == null // Доп условие вообще пустое? ЛИБО - выполняется
                    || !addMasterData.AdditionalConditions.TryGetValue(rechangeGuid, out var adds)
                    || adds.IsEmpty
                    || centerData.AdditinalConditions.IsEqualWithThis(adds))
                //|| (centerData.AdditinalConditions.BinaryData & adds.BinaryDataMask) == adds.BinaryData)
                {
                    whereToAdd.Result[RechangeLevel] ??= new();// .TryAdd(RechangeLevel, new());
                    var currLevel = whereToAdd.Result[RechangeLevel];
                    if (currLevel.TryGetValue(rechangeGuid, out var rech) && rech.Duration < RechangeToCheck.Duration)
                    {
                        // Удаляем переналадку, у которой длительность меньше, и которая по каким-то причинам оказалась у нас
                        currLevel.Remove(rechangeGuid);
                    }
                    currLevel.TryAdd(rechangeGuid, RechangeToCheck); // Добавляем переналадку

                    // А теперь запаримся ещё и с многокритериальной установкой
                    if (RechangeToCheck.IsMultiCriterial)
                    {
                        whereToAdd.MultiCriterialInfo.TryAdd(rechangeGuid, new UniversalBinaryStorage());
                        whereToAdd.MultiCriterialInfo[rechangeGuid].SetDataBit(CriteriaIndex, true);
                    }
                }
                else
                {
                    // Не выполнено. Ничего не происходит, ничего не делаем.
                }
            }
        }
        
        #endregion


    }

}

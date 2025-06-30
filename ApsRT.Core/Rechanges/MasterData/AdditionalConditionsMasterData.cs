using UniversalPack.Common;
using UniversalPack.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApsRT.Core.Rechanges.RechangesEngine;
using ApsRT.Core.Session;
using ApsRT.Core.Rechanges.Hoppers;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Rechanges.MasterData
{
    /// <summary>
    /// Есть три важных пункта.
    /// Параметры (Params) - списки тех или иных атрибутов оборудования, определенных текстом, а так же их значения. Например, параметр "ГоловаЭкструдера", со значениями "Малая" и "Большая". После загрузки превращаются в набор индексов, позволяя АПС в дальнейшем оперировать ими быстрее.
    /// Условия (Conditions) - непосредственно условия для переналадок. Для каждой переналадки создаётся своё бинарное хранилище, где записываются параметры, нужные для срабатывания.
    /// Действия (Actions) - словарь с данными, который подсказывает, когда и как менять атрибут у оборудования. К примеру, когда менять малую голову на большую. Основан на том, что внутри каждого РЦ для плана есть своё бинарное хранилище, где записываются его текущие атрибуты (параметры), и после каждой установки этапа в плане вызывается функция проверки и переназначения атрибутов, в зависимости от данных. Действие представляет из себя класс, сформированный с помощью Параметров, в нем указано, что куда записывать в бинарное хранилище РЦ, это хранилище потом попросту сравнивается с бинарным хранилищем переналадки, давая ответ, нужно ли её ставить сейчас.
    /// Поиск и назначение доп. условий очень похож, и был взят с движка переналадок. Собственно, ради этих доп. условий в движке переналадок и содержатся универсальные функции загрузки и поиска.
    /// </summary>
    public class AdditionalConditionsMasterData
    {
        public Session.Session SessionLink { get; }

        public AdditionalConditionsMasterData(Session.Session session)
        {
            SessionLink = session;
        }

        internal const int ADD_CONDITIONS_INDEX_FOR_DIAPAZONS = 1;

        #region Список параметров, определенных текстом (для удобства), на них зиждется всё остальное

        Dictionary<string, HashSet<string>> additionalParamsList;

        Dictionary<string, (byte Index, Dictionary<string, byte> ParamsIndexes, BinaryStorageDataSize DataSize)> AdditionalParams_NameToInt;

        /// <summary>
        /// Очищает всё, связанное с доп. условиями
        /// </summary>
        public void ClearAdditionalParams()
        {
            additionalParamsList = null;
            AdditionalParams_NameToInt = null;
        }

        /// <summary>
        /// Добавить в память существующий параметр чего либо на РЦ
        /// </summary>
        /// <param name="param">Параметр (напр.: ГоловаЭкструдера)</param>
        /// <param name="value">Значение этого параметра (напр.: малая/большая)</param>
        public void AddAdditionalParam(AdditionalParamsDataLoaderModel model)
        {
            additionalParamsList ??= new Dictionary<string, HashSet<string>>();
            additionalParamsList.TryAdd(model.Parameter, new());
            additionalParamsList[model.Parameter].Add(model.Value);
        }

        /// <summary>
        /// Запекает считанные списки параметров в FrozenDictionary, делая их быстрее и только для чтения.
        /// </summary>
        public void BakeAdditionalParams()
        {
            if (additionalParamsList == null) return;
            AdditionalParams_NameToInt ??= new();
            foreach (var element in additionalParamsList)
            {
                AdditionalParams_NameToInt.Add(element.Key, (Index: (byte)AdditionalParams_NameToInt.Count, ParamsIndexes: new(), BinaryStorageDataSize.Bit));
                var elHere = AdditionalParams_NameToInt[element.Key];
                var list = elHere.ParamsIndexes;
                foreach (var param in element.Value)
                {
                    list.Add(param, (byte)list.Count);
                }
                elHere.DataSize = DefineDataSize((byte)list.Count);
                AdditionalParams_NameToInt[element.Key] = elHere;
            }

        }

        BinaryStorageDataSize DefineDataSize(byte count)
        {
            if (count < 3) return BinaryStorageDataSize.Bit;
            if (count < 5) return BinaryStorageDataSize.Quarter;
            return BinaryStorageDataSize.Byte;
        }

        #endregion

        #region Бинарные хранилища для самих переналадок, хранящие уже условия для срабатывания

        public FrozenDictionary<Guid, UniversalBinaryStorage> AdditionalConditions;

        Dictionary<Guid, UniversalBinaryStorage> AdditionalConditionsOnLoad;

        public void ClearAdditionalConditions()
        {
            AdditionalConditions = null;
            AdditionalConditionsOnLoad = null;
        }

        public void AddAdditionalCondition(AdditionalConditionsDataLoaderModel model)
        {
            var rechangeGuid = model.Rechange;
            var param = model.Parameter;
            var value = model.Value;
            AdditionalConditionsOnLoad ??= new();
            AdditionalConditionsOnLoad.TryAdd(rechangeGuid, new());
            var binary = AdditionalConditionsOnLoad[rechangeGuid];
            if (AdditionalParams_NameToInt.TryGetValue(param, out var datas))
            {
                var paramIndex = datas.Index;
                if (datas.ParamsIndexes.TryGetValue(value, out var valueIndex))
                {
                    binary.SetData(paramIndex, valueIndex, datas.DataSize);
                }
            }
        }

        /// <summary>
        /// Запекает считанные условия для доп. параметров в FrozenDictionary, делая их быстрее и только для чтения.
        /// Зависим от ранее считанных самих доп. параметров.
        /// </summary>
        public void BakeAdditionalConditions()
        {
            if (AdditionalConditionsOnLoad != null)
            {
                AdditionalConditions = AdditionalConditionsOnLoad.ToFrozenDictionary();
            }
        }

        #endregion

        #region Непосредственно действия (смена голов и прочего)
        Dictionary<
           Uco, // РЦ
               RechangeCriteriaStorage<AdditionalAction>[] // Критерии
           > AllActionsOnLoad = new();

        FrozenDictionary<Uco, // РЦ
                RechangeCriteriaStorage<AdditionalAction>[]
            > AllActions;

        Dictionary<Uco, // РЦ
            Dictionary<int, // этап 1
                Dictionary<int, // этап 2
                    Dictionary<UniversalBinaryStorage, UniversalBinaryStorage> // В зависимости от условий ещё
                    >
                >
            > AdditionalActionsCashe = new();

        internal void ClearAdditionalActions()
        {
            AdditionalActionsCashe.Clear();
            AllActionsOnLoad.Clear();
            AllActions = null;
        }

        internal void BakeAdditionalActions()
        {
            AllActions = AllActionsOnLoad.ToFrozenDictionary(x => x.Key, x => x.Value);
        }

        internal void AddAdditionalAction(AdditionalActionsDataLoaderModel model)
        {
            var rawCriteriaName = model.Criteria;
            var RC = model.Center;
            var paramName = model.Parameter;
            var valueName = model.Value;
            var from = model.From;
            var to = model.To;
            var toGuid = model.ToGuid;
            var fromGuid = model.FromGuid;
            var masterDatas = SessionLink.MasterDataLinks;
            var index = masterDatas.RechangesMasterData.GetCriteriaIndexByName(rawCriteriaName);
            if (index == -1)
            {
                SessionLink.AssignedLogger.LogQueue.Enqueue($"ЗАГРУЗКА (переналадки): Неизвестный критерий '{rawCriteriaName}' переналадки.\r\n");
            }
            else
            {
                var rc = masterDatas.CentersMasterData.CentersData[RC];
                rc.PossibleAdditionalActions ??= new();
                rc.PossibleAdditionalActions.SetDataBit(index, true);
                rc.IsWithRechanges = true;
                rc.HasAdditionalActions = true;

                AllActionsOnLoad.TryAdd(RC, new RechangeCriteriaStorage<AdditionalAction>[masterDatas.RechangesMasterData.ExistingCriterias.Length]);
                var storages = AllActionsOnLoad[RC];
                storages[index] ??= new RechangeCriteriaStorage<AdditionalAction>();
                var storage = storages[index];

                if (AdditionalParams_NameToInt.TryGetValue(paramName, out var datas))
                {
                    var paramIndex = datas.Index;
                    if (datas.ParamsIndexes.TryGetValue(valueName, out var valueIndex))
                    {
                        AdditionalAction action = new(paramIndex, valueIndex, datas.DataSize);

                        AdditionalActionsCashe.TryAdd(RC, new());

                        RechangesEngine.RechangesEngine.UniversalDataLoader<AdditionalAction>(SessionLink, storage, index, from, fromGuid, to, toGuid, action);
                    }
                }

            }
        }

        internal int? CalculateDiapazonIndex(
            Uco RC, // РЦ
            Uco OperVid, // Вид операции, пока не юзается, но нужен для одинаковости методов + задел на будущее, вдруг пригодится
            int CriteriaIndex,
            float? value
            )
        {
            if (!value.HasValue) return null;
            if (AllActions == null
                || !AllActions.TryGetValue(RC, out var array)
                || array == null
                || array[CriteriaIndex] == null) return null;
            return array[CriteriaIndex].GetDiapazonByValue(value.Value);
        }

        object actionsCashLocker = new();

        internal UniversalBinaryStorage UpdateAdditionalParamsOnCenter(RechangesEngineData engineData)
        {
            if (AdditionalActionsCashe.TryGetValue(engineData.ActualRC, out var lvl10) &&
                lvl10.TryGetValue(engineData.ObjectA.Index, out var lvl20) &&
                lvl20.TryGetValue(engineData.ObjectB.Index, out var lvl30) &&
                lvl30.TryGetValue(engineData.CenterData.AdditinalConditions, out var result0))
            {
                return new UniversalBinaryStorage(result0); // Возвращаем КОПИЮ, чтобы не ломалось хранилище
            }

            var masterDatas = SessionLink.MasterDataLinks;

            masterDatas.CentersMasterData.CentersData.TryGetValue(engineData.ActualRC, out var centerClass);

            var hasActions = centerClass != null ? centerClass.HasAdditionalActions : false;

            if (!hasActions)
            {
                return engineData.CenterData.AdditinalConditions;
            }

            var mask = centerClass.PossibleAdditionalActions;

            if (mask == null || mask.IsEmpty)
            {
                return engineData.CenterData.AdditinalConditions;
            }

            UniversalBinaryStorage result = engineData.CenterData.AdditinalConditions;

            UniversalBinaryStorage copyToSave = new UniversalBinaryStorage(result); // Создаём тут копию, которую позже сохраним

            var arraysRech = AllActions.TryGetValue(engineData.ActualRC, out var arrays) ? arrays : null;
            bool IsDynamic = false;
            for (int cr = 0; cr < masterDatas.RechangesMasterData.ExistingCriterias.Length; cr++)
            {
                if (mask.GetDataBit(cr))
                {
                    IsDynamic |= masterDatas.RechangesMasterData.ExistingCriterias[cr].IsDynamic;
                    GetAdditionalActionsByCriteria(engineData, cr, result, arraysRech);
                }
            }

            if (IsDynamic)
            {
                // В динамический словарь
            }
            else
            {
                // В статичный
                lock (actionsCashLocker)
                {
                    var lvl1 = AdditionalActionsCashe[engineData.ActualRC];
                    lvl1.TryAdd(engineData.ObjectA.Index, new Dictionary<int, Dictionary<UniversalBinaryStorage, UniversalBinaryStorage>>());
                    var lvl2 = lvl1[engineData.ObjectA.Index];
                    lvl2.TryAdd(engineData.ObjectB.Index, new Dictionary<UniversalBinaryStorage, UniversalBinaryStorage>());
                    var lvl3 = lvl2[engineData.ObjectB.Index];
                    lvl3.TryAdd(copyToSave, new UniversalBinaryStorage(result)); // Сохраняем тут КОПИИ, потому что оригинал сейчас вернется центру, и тот пойдет работать дальше
                }
            }


            return result;
        }

        void GetAdditionalActionsByCriteria(RechangesEngineData engineData, int CriteriaIndex, UniversalBinaryStorage result, RechangeCriteriaStorage< AdditionalAction>[] RechangesToAcquireArray = null)
        {
            RechangesToAcquireArray ??= AllActions.TryGetValue(engineData.ActualRC, out var arrays) ? arrays : null;

            if (RechangesToAcquireArray == null) return;

            var RechangesToAcquire = RechangesToAcquireArray[CriteriaIndex];

            if (RechangesToAcquire == null) return;

            // Обрабатывам данные согласно настройкам выбранного критерия переналадки
            RechangesEngine.RechangesEngine.ProcessRechangeGlobalParam<AdditionalAction, UniversalBinaryStorage>(CriteriaIndex, engineData, RechangesToAcquire, ADD_CONDITIONS_INDEX_FOR_DIAPAZONS, result, AddAdditionalsFinally, CalculateDiapazonIndex);
        }


        void AddAdditionalsFinally(UniversalBinaryStorage whereToAdd, IEnumerable<AdditionalAction> ActionsList, RechangesEngineData engineData, int CriteriaIndex)
        {
            var centerData = engineData.CenterData;
            foreach (var action in ActionsList)
            {
                whereToAdd.SetData(action);
            }
        }

        #endregion

    }
}

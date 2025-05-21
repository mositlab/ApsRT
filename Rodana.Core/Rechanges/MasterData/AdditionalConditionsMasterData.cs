using UniversalPack.Common;
using UniversalPack.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfimumMES
{
    /// <summary>
    /// Есть три важных пункта.
    /// Параметры (Params) - списки тех или иных атрибутов оборудования, определенных текстом, а так же их значения. Например, параметр "ГоловаЭкструдера", со значениями "Малая" и "Большая". После загрузки превращаются в набор индексов, позволяя АПС в дальнейшем оперировать ими быстрее.
    /// Условия (Conditions) - непосредственно условия для переналадок. Для каждой переналадки создаётся своё бинарное хранилище, где записываются параметры, нужные для срабатывания.
    /// Действия (Actions) - словарь с данными, который подсказывает, когда и как менять атрибут у оборудования. К примеру, когда менять малую голову на большую. Основан на том, что внутри каждого РЦ для плана есть своё бинарное хранилище, где записываются его текущие атрибуты (параметры), и после каждой установки этапа в плане вызывается функция проверки и переназначения атрибутов, в зависимости от данных. Действие представляет из себя класс, сформированный с помощью Параметров, в нем указано, что куда записывать в бинарное хранилище РЦ, это хранилище потом попросту сравнивается с бинарным хранилищем переналадки, давая ответ, нужно ли её ставить сейчас.
    /// Поиск и назначение доп. условий очень похож, и был взят с движка переналадок. Собственно, ради этих доп. условий в движке переналадок и содержатся универсальные функции загрузки и поиска.
    /// </summary>
    internal class AdditionalConditionsMasterData
    {

        internal const int ADD_CONDITIONS_INDEX_FOR_DIAPAZONS = 1;

        #region Список параметров, определенных текстом (для удобства), на них зиждется всё остальное

        static Dictionary<string, HashSet<string>> additionalParamsList;

        static Dictionary<string, (byte Index, Dictionary<string, byte> ParamsIndexes, BinaryStorageDataSize DataSize)> AdditionalParams_NameToInt;

        /// <summary>
        /// Очищает всё, связанное с доп. условиями
        /// </summary>
        public static void ClearAdditionalParams()
        {
            additionalParamsList = null;
            AdditionalParams_NameToInt = null;
        }

        /// <summary>
        /// Добавить в память существующий параметр чего либо на РЦ
        /// </summary>
        /// <param name="param">Параметр (напр.: ГоловаЭкструдера)</param>
        /// <param name="value">Значение этого параметра (напр.: малая/большая)</param>
        public static void AddAdditionalParam(string param, string value)
        {
            additionalParamsList ??= new Dictionary<string, HashSet<string>>();
            additionalParamsList.TryAdd(param, new());
            additionalParamsList[param].Add(value);
        }

        /// <summary>
        /// Запекает считанные списки параметров в FrozenDictionary, делая их быстрее и только для чтения.
        /// </summary>
        public static void BakeAdditionalParams()
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

        static BinaryStorageDataSize DefineDataSize(byte count)
        {
            if (count < 3) return BinaryStorageDataSize.Bit;
            if (count < 5) return BinaryStorageDataSize.Quarter;
            return BinaryStorageDataSize.Byte;
        }

        #endregion

        #region Бинарные хранилища для самих переналадок, хранящие уже условия для срабатывания

        public static FrozenDictionary<Guid, UniversalBinaryStorage> AdditionalConditions;

        static Dictionary<Guid, UniversalBinaryStorage> AdditionalConditionsOnLoad;

        public static void ClearAdditionalConditions()
        {
            AdditionalConditions = null;
            AdditionalConditionsOnLoad = null;
        }

        public static void AddAdditionalCondition(Guid rechangeGuid, string param, string value)
        {
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
        public static void BakeAdditionalConditions()
        {
            if (AdditionalConditionsOnLoad != null)
            {
                AdditionalConditions = AdditionalConditionsOnLoad.ToFrozenDictionary();
            }
        }

        #endregion

        #region Непосредственно действия (смена голов и прочего)
        static Dictionary<
           CodedObject<string>, // РЦ
               RechangeCriteriaStorage<AdditionalAction>[] // Критерии
           > AllActionsOnLoad = new();

        static FrozenDictionary<CodedObject<string>, // РЦ
                RechangeCriteriaStorage<AdditionalAction>[]
            > AllActions;

        static Dictionary<CodedObject<string>, // РЦ
            Dictionary<int, // этап 1
                Dictionary<int, // этап 2
                    Dictionary<UniversalBinaryStorage, UniversalBinaryStorage> // В зависимости от условий ещё
                    >
                >
            > AdditionalActionsCashe = new();

        internal static void ClearAdditionalActions()
        {
            AdditionalActionsCashe.Clear();
            AllActionsOnLoad.Clear();
            AllActions = null;
        }

        internal static void BakeAdditionalActions()
        {
            AllActions = AllActionsOnLoad.ToFrozenDictionary(x => x.Key, x => x.Value);
        }

        internal static void AddAdditionalAction(
            CodedObject<string> RC, // РЦ
                string rawCriteriaName,
                string from, Guid fromGuid,
                string to, Guid toGuid,
                string paramName, string valueName
            )
        {
            var index = RechangesMasterData.GetCriteriaIndexByName(rawCriteriaName);
            if (index == -1)
            {
                ConcurrentLogger.AddToLogQueue.Enqueue($"ЗАГРУЗКА (переналадки): Неизвестный критерий '{rawCriteriaName}' переналадки.\r\n");
            }
            else
            {
                var rc = CentersMasterData.CentersData[RC];
                rc.PossibleAdditionalActions ??= new();
                rc.PossibleAdditionalActions.SetDataBit(index, true);
                rc.IsWithRechanges = true;
                rc.HasAdditionalActions = true;

                AllActionsOnLoad.TryAdd(RC, new RechangeCriteriaStorage<AdditionalAction>[RechangesMasterData.ExistingCriterias.Length]);
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

                        RechangesEngine.UniversalDataLoader<AdditionalAction>(storage, index, from, fromGuid, to, toGuid, action);
                    }
                }

            }
        }

        internal static int? CalculateDiapazonIndex(
            CodedObject<string> RC, // РЦ
            CodedObject<string> OperVid, // Вид операции, пока не юзается, но нужен для одинаковости методов + задел на будущее, вдруг пригодится
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

        static object actionsCashLocker = new();

        internal static UniversalBinaryStorage UpdateAdditionalParamsOnCenter(RechangesEngineData engineData)
        {
            if (AdditionalActionsCashe.TryGetValue(engineData.ActualRC, out var lvl10) &&
                lvl10.TryGetValue(engineData.ObjectA.ID, out var lvl20) &&
                lvl20.TryGetValue(engineData.ObjectB.ID, out var lvl30) &&
                lvl30.TryGetValue(engineData.CenterData.AdditinalConditions, out var result0))
            {
                return new UniversalBinaryStorage(result0); // Возвращаем КОПИЮ, чтобы не ломалось хранилище
            }


            InfimumMES.CentersMasterData.CentersData.TryGetValue(engineData.ActualRC, out var centerClass);

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
            for (int cr = 0; cr < RechangesMasterData.ExistingCriterias.Length; cr++)
            {
                if (mask.GetDataBit(cr))
                {
                    IsDynamic |= RechangesMasterData.ExistingCriterias[cr].IsDynamic;
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
                    lvl1.TryAdd(engineData.ObjectA.ID, new());
                    var lvl2 = lvl1[engineData.ObjectA.ID];
                    lvl2.TryAdd(engineData.ObjectB.ID, new());
                    var lvl3 = lvl2[engineData.ObjectB.ID];
                    lvl3.TryAdd(copyToSave, new UniversalBinaryStorage(result)); // Сохраняем тут КОПИИ, потому что оригинал сейчас вернется центру, и тот пойдет работать дальше
                }
            }


            return result;
        }

        internal static void GetAdditionalActionsByCriteria(RechangesEngineData engineData, int CriteriaIndex, UniversalBinaryStorage result, RechangeCriteriaStorage<AdditionalAction>[] RechangesToAcquireArray = null)
        {
            RechangesToAcquireArray ??= AllActions.TryGetValue(engineData.ActualRC, out var arrays) ? arrays : null;

            if (RechangesToAcquireArray == null) return;

            var RechangesToAcquire = RechangesToAcquireArray[CriteriaIndex];

            if (RechangesToAcquire == null) return;

            // Обрабатывам данные согласно настройкам выбранного критерия переналадки
            RechangesEngine.ProcessRechangeGlobalParam<AdditionalAction, UniversalBinaryStorage>(CriteriaIndex, engineData, RechangesToAcquire, ADD_CONDITIONS_INDEX_FOR_DIAPAZONS, result, AddAdditionalsFinally, CalculateDiapazonIndex);
        }


        static void AddAdditionalsFinally(UniversalBinaryStorage whereToAdd, IEnumerable<AdditionalAction> ActionsList, RechangesEngineData engineData, int CriteriaIndex)
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

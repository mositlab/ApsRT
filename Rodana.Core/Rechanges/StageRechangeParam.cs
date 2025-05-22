using UniversalPack.Common;
using UniversalPack.Extensions;
using Rodana.Core.Stages;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Rodana.Core.Rechanges.MasterData;
using Rodana.Core.Rechanges.RechangesEngine;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Rechanges
{
    /// <summary>
    /// Лежащая внутри ЭТАПА в виде массива структура, значение для каждой из переналадок
    /// ПРАВИЛО 1. Если набор работает с диапазонами, то он всё равно обращается к хранилищу целых чисел пару раз, потому что ему иногда нужно обновить данные. Именно поэтому они создаются вместе с диапазонами, и поэтому задание ссылок происходит ещё в конструкторе.
    /// ПРАВИЛО 2. Для диапазонов НЕ вызываются и не создаются ссылки (речь о делегатах), которые позволяют получить индекс диапазона откуда-то извне. АПС обращается к числу [Которое либо хранится здесь, либо получается по ссылке], получает из него индекс (обратившись к своим НСИ), и хранит его у себя. Система уже создана, не надо где-то создавать её копию и подсовывать системе свои значения, или пытаться "перехватить" значения индексов диапазонов. Они привязаны к числу.
    /// ПРАВИЛО 3. Изначально у индексов диапазонов есть один вид операции в словаре, вид собственно этапа. При этом, в переналадках юзаются порой виды операций от другой операции (ObjectA.VidOper не всегда равно ObjectB.VidOper), поэтому было принято решение сделать двухуровневый словарь, а добавляются данные по посторонним видам в процессе работы движка переналадок. Работает это только, если критерий не динамичный (для динамичных критериев движок в принципе диапазоны считает внутри себя).
    /// </summary>
    public class StageRechangeParam
    {
        /// <summary>
        /// Ссылка на этап, к которому принадлежит этот набор параметров
        /// </summary>
        Stage StageLink;

        /// <summary>
        /// Ссылка на критерий для получения данных
        /// </summary>
        RechangeUniversalCriteria CriteriaProperties;

        #region Данные, лежат ПРИВАТНО

        float?[] AsNumber; // Представление в виде числа
        Guid?[] AsGuid; // Представление в виде кодированного объекта
        string[] AsString; // Представление в виде строки, она же строка
        //int?[] AsStringHashCode; // Представление строки выше в качестве хеша, что нужно для скорости обработки

        #endregion

        #region Работа с диапазонами, связана жёстко с числами

        /// <summary>
        /// Функция запекания статичных диапазонов, вызывается дважды,
        /// один раз - когда я впервые задаю данные в хоппере операций,
        /// во второй раз её можно вызвать после работы партий, там как минимум меняется количество мест, что тоже переналадка
        /// Если критерий динамичный, то он не вызывается (это просто бессмысленно, постоянно в него пихать значения), вместо этого его массивы используются движком переналадок (потому что туда данные записываются для простого хранения)
        /// </summary>
        /// <param name="masterDataLink">Ссылка на НСИ переналадок, откуда ему брать индексы диапазонов</param>
        internal void BakeIndexDiapazons(int criteriaIndex, RechangesEngineData engineData)
        {
            var list = StageLink.PossibleCenters ?? new() { StageLink.OrigCenter };
            var masterDataLink = StageLink.SessionLink.MasterDataLinks.RechangesMasterData;
            foreach (var rc in list)
            {
                var opersList = masterDataLink.GetAvailableOperVids(rc);
                if (opersList == null) continue;
                int length = GetValuesCount();
                for (int i = 0; i < length; i++)
                {
                    foreach (var operVid in opersList)
                    {
                        SetDiapazonIndex(MasterData.RechangesMasterData.RECHANGES_INDEX_FOR_DIAPAZONS, rc, operVid, i, masterDataLink.CalculateDiapazonIndex(rc, operVid, criteriaIndex, GetFloat(i, engineData)));
                    }
                    SetDiapazonIndex(AdditionalConditionsMasterData.ADD_CONDITIONS_INDEX_FOR_DIAPAZONS, rc, Uco.EmptyObject, i, StageLink.SessionLink.MasterDataLinks.AdditionalConditionsMasterData.CalculateDiapazonIndex(rc, Uco.EmptyObject, criteriaIndex, GetFloat(i, engineData)));
                }
            }
        }

        Dictionary<int, // Тип данных, для чего хранятся диапазоны (для переналадок (0) или доп. условий (1)?)
        Dictionary<Uco, // Вид операции
            Dictionary<Uco, // РЦ
                int?[]>>> AsDiapazonIndex; // Представление числа в качестве номера диапазона, тоже для скорости. Неотрывно связано с самим числом.

        /// <summary>
        /// Устанавливает в список диапазонов уже подобранный диапазон.
        /// </summary>
        /// <param name="DataType">Для каких данных дипазон (переналадка или доп условие)</param>
        /// <param name="RC">На какой это РЦ</param>
        /// <param name="valueIndex">Какое это число из существующих в значении этапа (размер1, размер2...)</param>
        /// <param name="index">Какой у него получился индекс?</param>
        public void SetDiapazonIndex(int DataType, Uco RC, Uco operVid, int valueIndex, int? index)
        {
            if (AsDiapazonIndex == null) AsDiapazonIndex = new();

            AsDiapazonIndex.TryAdd(DataType, new());

            var asDiapazonIndex = AsDiapazonIndex[DataType];

            asDiapazonIndex.TryAdd(operVid, new Dictionary<Uco, int?[]>());

            var rcs = asDiapazonIndex[operVid];

            rcs.TryAdd(RC, new int?[GetValuesCount()]);

            rcs[RC][valueIndex] = index;
        }

        public int?[] GetDiapazonIndexes(int DataType, Uco RC, Uco OperVid)
        {
            if (AsDiapazonIndex == null) return null;
            return AsDiapazonIndex.TryGetValue(DataType, out var operVids)
                && operVids.TryGetValue(OperVid, out var RCS)
                && RCS.TryGetValue(RC, out var array)
                ? array : null;
        }

        #endregion

        #region Ссылки и получение данных

        int _valuesCount = 0;

        /// <summary>
        /// Получить количество значений.
        /// </summary>
        /// <returns>количество значений</returns>
        public int GetValuesCount()
        {
            return _valuesCount;
        }


        private T GetValue<T>(int valueIndex, RechangesEngineData engineData, T[] array)
        {
            if (array != null) return (valueIndex >= array.Length ? default(T) : array[valueIndex]);
            return (T)CriteriaProperties.LinkDataObtainer?.Invoke(StageLink, engineData, typeof(T), valueIndex);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            int count = GetValuesCount();
            for (int i = 0; i < count; i++)
            {
                sb.Append(GetFloat(i));
                sb.Append(" / ");
                sb.Append(GetGuid(i));
                sb.Append(" / ");
                sb.Append(GetString(i));
                if (i < count - 1) sb.Append(", ");
            }
            sb.Append("]");
            return sb.ToString();
        }

        public float? GetFloat(int valueIndex, RechangesEngineData engineData = null) => GetValue<float?>(valueIndex, engineData, AsNumber);
        public Guid? GetGuid(int valueIndex, RechangesEngineData engineData = null) => GetValue<Guid?>(valueIndex, engineData, AsGuid);
        public string GetString(int valueIndex, RechangesEngineData engineData = null) => GetValue<string>(valueIndex, engineData, AsString);

        #endregion

        #region Подготовка и установка данных

        /// <summary>
        /// Конструктор набора параметров. Задаём важные ссылки
        /// </summary>
        /// <param name="stageLink">Ссылка на этап, к которому принадлежит параметры</param>
        /// <param name="valuesCountObtainer">Ссылка на метод, который позволит получить количество параметров</param>
        /// <param name="dataObtainer">Ссылка на метод, который позволит получить параметры</param>
        public StageRechangeParam(Stage stageLink, RechangeUniversalCriteria criteria)
        {
            StageLink = stageLink;
            AsNumber = null;
            AsDiapazonIndex = null;
            AsGuid = null;
            AsString = null;
            CriteriaProperties = criteria;
        }

        public StageRechangeParam(Stage stageLink): this(stageLink, null) { }


        /// <summary>
        /// Назначаем все данные
        /// </summary>
        /// <param name="source">Откуда брать данные</param>
        /// <param name="criteriaProperties">Свойства критерия, собственно способ хранения</param>
        /// <param name="masterDataLink">Ссылка на НСИ, пригодится при чтении диапазонов</param>
        /// <param name="readOptions">Свойства парсинга, из Хоппера Этапов, пригодятся при парсинге гуида.</param>
        internal void SetData(IDataReader source, DataLoaderOptions readOptions)
        {
            // Вызываю функцию, либо беру из базы данных инфу, сколько тут всего значений
            int valuesCount = CriteriaProperties.IsLink ? CriteriaProperties.LinkValuesCountObtainer(StageLink) : CriteriaProperties.StagesTableColumnsNames.Length;

            // Даже если я юзаю только диапазоны, мне все равно надо создать массив чисел, чтобы где-то хранить значение, на основании которого я выдаю потом индекс диапазона.
            // Интересно то, что если я юзаю ссылки, то мне на AsNumber плевать, потому что я не беру оттуда данные.
            // Поэтому вторым условием здесь у всего является наличие ссылки
            if (CriteriaProperties.CanBeNumber || CriteriaProperties.CanBeDiapazon)
                if (!CriteriaProperties.IsLink)
                    AsNumber = new float?[valuesCount];

            if (CriteriaProperties.CanBeGuid)
                if (!CriteriaProperties.IsLink)
                    AsGuid = new Guid?[valuesCount];

            // Диапазоны ВНЕ ЗАВИСИМОСТИ ОТ ССЫЛОК создаём - потому что они по этим ссылкам получают данные.
            if (CriteriaProperties.CanBeDiapazon)
                AsDiapazonIndex = new Dictionary<int, Dictionary<Uco, Dictionary<Uco, int?[]>>>();

            // Строки тожк создаём ТОЛЬКО, если это не ссылки. Потому что я точно так же смогу обращаться к ним исключительно по ссылке.
            // Доп. условий здесь нет, потому что строки часто используются для представления значения в ГУЕ.
            if (!CriteriaProperties.IsLink)
                AsString = new string[valuesCount];

            // Если это ссылка, то дальнейшие действия не нужны - там берутся данные из столбцов и сохраняются там. В то время, как при работе со ссылками получение данных, строк и сортировка - ложатся на плечи кодера этих ссылок.
            if (CriteriaProperties.IsLink)
            {
                _valuesCount = valuesCount; // Задаем тут количество
                return;
            }

            for (int i = 0; i < valuesCount; i++)
            {
                var valueColumnName = CriteriaProperties.StagesTableColumnsNames[i].ValueColumn;
                var rawValue = source[valueColumnName];

                if (CriteriaProperties.CanBeNumber || CriteriaProperties.CanBeDiapazon)
                {
                    if (rawValue is float fl)
                    {
                        AsNumber[i] = fl;
                    }
                    if (rawValue is double db)
                    {
                        AsNumber[i] = (float)db;
                    }
                    if (rawValue is decimal dc)
                    {
                        AsNumber[i] = (float)dc;
                    }
                    if (rawValue is int integer)
                    {
                        AsNumber[i] = (float)integer;
                    }
                    if (rawValue is string str)
                    {
                        AsNumber[i] = str.ConvertToOrDefault(0.0f);
                    }
                }

                if (CriteriaProperties.CanBeGuid)
                {
                    // Нужно задать значение, причем если я могу быть многозначным, а столбцов 1, то стоит попытаться разделить запятой
                    if (CriteriaProperties.CanBeMultiple && valuesCount == 1)
                        AsGuid = rawValue.ConvertToOrDefault(string.Empty).Split(',', '/').Select(X => (Guid?)X.Trim().ProcessGuid(readOptions.GuidArrrangeType, true)).ToArray();
                    else
                        AsGuid[i] = rawValue.ProcessGuid(readOptions.GuidArrrangeType, true);

                    var rawLabel = source[CriteriaProperties.StagesTableColumnsNames[i].LabelColumn ?? valueColumnName].ConvertToOrDefault(string.Empty);

                    // Нужно задать значение, причем если я могу быть многозначным, а столбцов 1, то стоит попытаться разделить запятой
                    if (CriteriaProperties.CanBeMultiple && valuesCount == 1)
                        AsString = rawLabel.Split(',', '/').Select(X => X.Trim()).ToArray();
                    else
                        AsString[i] = rawLabel;
                }
                else
                {
                    if (CriteriaProperties.CanBeString)
                    {
                        // Нужно задать значение, причем если я могу быть многозначным, а столбцов 1, то стоит попытаться разделить запятой
                        if (CriteriaProperties.CanBeMultiple && valuesCount == 1)
                            AsString = rawValue.ConvertToOrDefault(string.Empty).Split(',', '/').Select(X =>
                            {
                                var x = X.Trim();
                                return string.IsNullOrEmpty(x) ? null : x; // Для движка переналадок полностью стираем
                            }).ToArray();
                        else
                        {
                            AsString[i] = rawValue.ConvertToOrDefault(string.Empty);
                            if (string.IsNullOrEmpty(AsString[i])) AsString[i] = null; // Для движка переналадок полностью стираем
                        }
                    }
                    else
                    {
                        // Просто задаем строковое представление для удобства
                        AsString[i] = source[CriteriaProperties.StagesTableColumnsNames[i].LabelColumn ?? valueColumnName].ConvertToOrDefault(string.Empty);
                    }
                }


            }

            if (CriteriaProperties.Sort)
            {
                if (CriteriaProperties.CanBeGuid)
                {
                    var str = AsString;
                    var array = AsGuid.Select((x, index) => (guid: x, label: str[index])).OrderBy(x => x.guid);
                    AsGuid = array.Select(x => x.guid).ToArray();
                    AsString = array.Select(x => x.label).ToArray();
                }
                else
                {
                    if (CriteriaProperties.CanBeString)
                    {
                        AsString = AsString.OrderBy(x => x).ToArray();
                    }
                }
            }

            _valuesCount = AsNumber?.Length ?? AsGuid?.Length ?? AsString?.Length ?? 0;
        }

        #endregion

    }

}

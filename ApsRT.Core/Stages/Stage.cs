using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;
using ApsRT.Core.Session;
using ApsRT.Core.Settings;
using InfimumMES;
using ApsRT.Core.Rechanges.RechangesEngine;
using ApsRT.Core.Rechanges;
using ApsRT.Core.Rechanges.MasterData;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using ApsRT.Core.Partias.Materials;
using UniversalPack.Common.DataLoader;
using UniversalPack.Attributes;
using UniversalPack;
using ApsRT.Core.Orders;
using ApsRT.Core.NaladLengths;
using System.Drawing;
using System.Collections.Frozen;
using Google.Protobuf.WellKnownTypes;

namespace ApsRT.Core.Stages
{
    /// <summary>
    /// Элементарный этап, основная единица планирования.
    /// </summary>
    public class Stage: BaseDataLoaderModel
    {
        #region Код от BaseLoader
        private static readonly ICollection<FieldConverterInfo> _fieldConverters;

        static Stage()
        {
            _fieldConverters = DefineFieldConverters<Stage>();
        }

        protected override IEnumerable<FieldConverterInfo> FieldConverters => _fieldConverters;
        #endregion

        #region Основы
        public Session.Session SessionLink { get; }

        public Stage(Session.Session session)
        {
            SessionLink = session;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Index, CurrentTara.GetHashCode());
        }

        public override string ToString()
        {
            return $"Оп. {(GetEasyFlagToOutput() != Guid.Empty ? "E" : (IsCustomModelling ? "C" : ""))}{(Fixed ? "F" : "")}{(VPZ ? "V" : "")}:{Index} [{GUID}]" + (StartTimeFact.HasValue ? $" - {StartTimeFact}" : "");
        }

        /// <summary>
        /// Индекс, который проще кода, используется при быстрых расчётах повсюду.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Уникальный идентификатор этапа.
        /// </summary>

        [DataRecordColumn("ИдентификаторОперации")]
        public Guid GUID { get; set; }

        #endregion

        #region Номенклатурные данные

        /// <summary>
        /// Вид операции
        /// </summary>
        [DataRecordCodedObjectColumn("ВидОперации_GUID", "ВидОперации")]
        public Uco OperationVid { get; set; }

        /// <summary>
        /// Номенклатура
        /// </summary>
        [DataRecordCodedObjectColumn("Номенклатура_GUID", "Номенклатура")]
        public Uco Nomenklature { get; set; }

        /// <summary>
        /// Продукция
        /// </summary>
        [DataRecordCodedObjectColumn("Продукция_GUID", "Продукция")]
        public Uco Production { get; set; }

        /// <summary>
        /// Подразделение
        /// </summary>
        [DataRecordCodedObjectColumn("Подразделение_GUID", "Подразделение")]
        public Uco Subdivision { get; set; }

        /// <summary>
        /// Спецификация
        /// </summary>
        [DataRecordCodedObjectColumn("Спецификация_GUID", "Спецификация")]
        public Uco Specification { get; set; }

        /// <summary>
        /// Характеристика
        /// </summary>
        [DataRecordCodedObjectColumn("Характеристика_GUID", "Характеристика", true)]
        public Uco Characteristics { get; set; }

        #endregion

        #region Основные флаги

        [DataRecordColumn("НеПерепланировать", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        public bool Fixed { get; set; }

        [DataRecordColumn("ВключенаВПЗ", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        public bool VPZ { get; set; }

        [DataRecordColumn("ОтражениеФакта", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        public bool IsFact { get; set; }

        [DataRecordColumn("РучноеФормированиеПартииПФ", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        public bool IsUserHandled { get; set; }

        [DataRecordColumn("НеОбъединять", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        public bool IsLast { get; set; }

        [DataRecordColumn("ФиктивнаяОперация", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        public bool IsFictive { get; set; }

        [DataRecordColumn("НеМенятьТару", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        public bool NoChangeTara { get; set; }

        [DataRecordColumn("НаВспомогательнуюТару", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        public bool UsesVspomTara { get; set; }

        [DataRecordColumn("ВспомогательнаяОперация", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        public bool IsVspom { get; set; }

        [DataRecordColumn("НаОдномРЦ", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        public bool OnOneRC { get; set; }
        #endregion

        #region Рабочие центры
        public Uco OriginalCenter { get; set; }
        public Uco Center { get; set; }
        public List<Uco> PossibleCenters { get; set; }

        [DataRecordCodedObjectColumn("РабочийЦентрМаршрута_GUID", "РабочийЦентрМаршрута")]
        public Uco RCRoute { get; set; }

        [DataRecordColumn("ЭтоРЦ", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, true)]
        public bool RCRouteIsRC { get; set; }
        #endregion

        #region Тара и намотка
        [DataRecordCodedObjectColumn("КонечнаяТара_GUID", "КонечнаяТара")]
        public Uco EndTara { get; set; }
        public Uco CurrentTara { get; set; }
        public double CurrentTaraMaxCapacity { get; set; }
        private bool? noCheckNamotka;

        public bool NoCheckNamotka
        {
            get
            {
                if (!noCheckNamotka.HasValue)
                {
                    noCheckNamotka = SessionLink.MasterDataLinks.TarasMasterData.NotCheckNamotka(this);
                }
                return noCheckNamotka.Value;
            }
        }
        #endregion

        #region Реквизиты
        public float?[] NumberProps { get; set; }
        public Guid?[] GuidProps { get; set; }
        public string[] StringProps { get; set; }
        #endregion

        #region Важные Гуиды и их получение

        [DataRecordColumn("ИдентификаторЗабракованныхПозиций", defaultIfFail: true)]
        public Guid? RejectedPositionsGuid { get; set; }


        public Guid GetOrderOrPartiaCode()
        {
            return GetPartiaCode() ?? OrderLink.OrderID.Object.Code;
        }

        public Guid? GetPartiaCode()
        {
            return CustomPartiaGuid ?? PartiaPF;
        }
        public Guid? PartiaPF => OrderLink?.PartiaPF ?? null;
        /// <summary>
        /// Это тоже флаг для вывода - общий этап, для упрощения. В вычислении не используется
        /// </summary>
        public Guid? CustomPartiaGuid;


        public bool IsCustomModelling; // Если да - то будет юзать номер партии и моделирования ниже
                                   // а так как схлопнуться может что угодно с чем угодно, то у каждой операции есть эти поля
        public Guid CustomModelling;
        /// <summary>
        /// гуид моделирования, он из меса приходит
        /// </summary>
        [DataRecordCodedObjectColumn("ИдентификаторОбъединенияПФ", "ОбъединениеПФ", defaultIfFail: true)]
        public Uco MesModelling { get; set; }

        /// <summary>
        /// Этот общий флаг пришёл из МЕСа?
        /// </summary>
        //public bool EasyFlagIsFromMes;

        public Guid GetEasyFlagToOutput() => EasyFlag ?? EasyFlagOriginal ?? Guid.Empty;

        /// <summary>
        /// Это флаг для вывода - общий этап, для упрощения.
        /// </summary>
        public Guid? EasyFlag { get; set; }

        /// <summary>
        /// А это бекап флага на случай стирания всего и вся
        /// </summary>
        [DataRecordColumn("ИдентификаторОбщегоЭтапа", defaultIfFail: true)]
        public Guid? EasyFlagOriginal { get; set; }

        #endregion

        #region Важные данные

        public Color Color { get; set; }

        public OrderBaseModel OrderLink { get; set; }

        /// <summary>
        /// Ссылка на материал - отдельный класс, нужен только в партиях
        /// </summary>
        public StageMaterialsInfo StageMaterialsInfo { get; set; }

        [DataRecordColumn("Регистратор")]
        public string Registrator { get; set; }

        //
        [DataRecordCodedObjectColumn("ЕдиницаИзмеренияКлассификатор_GUID", "ЕдиницаИзмеренияКлассификатор", isDimensionUnit: true)]
        public UcoDU EdIzmClassificator { get; set; }

        // 
        [DataRecordColumn("Уровень")]
        public int Level { get; set; }

        // 
        [DataRecordColumn("НомерПовива")]
        public int Poviv { get; set; }

        //
        [DataRecordColumn("ВремяНачалаФакт")]
        public DateTime? StartTimeFact { get; set; }

        //
        [DataRecordColumn("ПовивыОбъединены", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes)]
        public bool PovivsMerged { get; set; }

        //
        [DataRecordColumn("КоличествоМест")]
        public int PlacesCount { get; set; }

        #endregion

        #region Информация из предыдущего плана

        //ДатаНачалаПлан - предыдущая дата из предыдущего расчёта
        [DataRecordColumn("ДатаНачалаПлан", defaultIfFail: true)]
        public DateTime? PrevDateTime { get; set; }

        [DataRecordCodedObjectColumn("РабочийЦентрПлан_GUID", "РабочийЦентрПлан", defaultIfFail: true)]
        public Uco? RecommendedRC { get; set; }
        #endregion

        #region Переналадки
        public StageRechangeParam[] RechangeParameters;

        /// <summary>
        /// Функция вызывается дважды,
        /// один раз - когда я впервые задаю данные в хоппере операций,
        /// во второй раз её можно вызвать после работы партий, там как минимум меняется количество мест, что тоже переналадка
        /// </summary>
        public void BakeAllIndexDiapazons(RechangesEngineData engineData)
        {
            var rechangesMasterData = SessionLink.MasterDataLinks.RechangesMasterData;
            for (int i = 0; i < RechangeParameters.Length; i++)
            {
                if (rechangesMasterData.ExistingCriterias[i].CanBeDiapazon && !rechangesMasterData.ExistingCriterias[i].IsDynamic) // Если в этом конечно есть смысл
                {
                    RechangeParameters[i].BakeIndexDiapazons(i, engineData);
                }
            }
        }

        #endregion

        #region Количество этапа и ед. изм.

        [DataRecordColumn("Количество")]
        public double CountMES { get; set; }

        [DataRecordColumn("КоличествоИсходногоЭтапа")]
        public double Count { get; set; }

        [DataRecordColumn("ЕдиницаИзмерения")]
        public DimensionUnit EdIzm { get; set; }

        [DataRecordColumn("МетодРасчетаНамоток")]
        public DimensionUnit UsedCalcMethod { get; set; }

        [DataRecordColumn("ВесКгКм", defaultIfFail: true)]
        public float? KgKm { get; set; }
        #endregion

        #region Длительность

        [DataRecordColumn("ДлительностьВыполненияНорма", defaultIfFail: true)]
        public float DurationNorm { get; set; }

        /// <summary>
        /// Длительность этого этапа вне зависимости от РЦ
        /// </summary>
        int? StaticDuration;

        public void SetStaticDuration(int seconds) => StaticDuration = seconds;

        /// <summary>
        /// Длительность конкретно этого этапа в зависимости от РЦ
        /// </summary>
        FrozenDictionary<Uco, int> CachedDurations;
        public int LongestDuration { get; private set; }

        public void PrepareDurations()
        {
            var list = PossibleCenters ?? new List<Uco>() { OriginalCenter };
            CachedDurations = list.ToFrozenDictionary(x => x, x => getDurationDynamic(x));
            LongestDuration = CachedDurations.Max(x => x.Value);
        }

        int getDurationDynamic(Uco RC)
        {
            if (StaticDuration.HasValue) return StaticDuration.Value;
            var masterData = SessionLink.MasterDataLinks.StagesTypicalDurationsMasterData;
            var currSets = SessionLink.SettingsController.CurrentSettings;

            // Если нет информации там, ищу в базах данных
            if (masterData.AnyDurations && masterData.TryGetTypicalDuration(RC, this, out var time))
            {
                return time;
            }

            return currSets.OriginalDurationTime;
        }

        public int GetDuration(Uco RC)
        {
            return CachedDurations.TryGetValue(RC, out var seconds) ? seconds : SessionLink.SettingsController.CurrentSettings.OriginalDurationTime;
        }
        #endregion

        #region Данные зависимостей
        /// <summary>
        /// Количество мест у предыдущих этапов, не попавших в план, но имевших информацию в таблице зависимостей (основная тара)
        /// </summary>
        public int GhostNormPrevsPlacesCount;
        /// <summary>
        /// Количество мест у предыдущих этапов, не попавших в план, но имевших информацию в таблице зависимостей (вспомогательная тара тара)
        /// </summary>
        public int GhostVspomPrevsPlacesCount;
        /// <summary>
        /// список пред этапов
        /// </summary>
        public List<int> Prevs;

        /// <summary>
        /// Список предков, которые НЕ вспомогательные. Этот счётчик заполняется при загрузке и выручает при вычислении партий
        /// </summary>
        public int PrevsNormCount;

        /// <summary>
        /// список дочерних этапов
        /// </summary>
        public List<int> Nexts;
        #endregion

        #region Наладочные длины
        [DataRecordColumn("НаладочнаяДлина", defaultIfFail: true)]
        public float NeededNaladLengthTableValue { get; set; }

        [DataRecordColumn("ПервыйВЗапуске", DataRecordColumnAttribute.CustomConversionModes.StringToBooleanIsYes, defaultIfFail: true)]
        public bool NeededNaladIsFirstInLaunch { get; set; }

        public (NaladLengthValue Length, bool IsFirst)? NeededNaladLength { get; set; }

        FrozenDictionary<Uco,
            FrozenDictionary<
            RequiredNaladCondition, NaladLengthValue
                >
            > NaladLengthsCache;

        Dictionary<Uco,
            Dictionary<
            RequiredNaladCondition, NaladLengthValue
                >
            > NaladLengthsCacheLocal = new();

        internal void CacheNaladLengths()
        {
            NaladLengthsCacheLocal.Clear();
            if (PossibleCenters != null)
            {
                foreach (var rc in PossibleCenters) CacheNaladLengthsRc(rc);
            }
            CacheNaladLengthsRc(OriginalCenter);

            NaladLengthsCache = NaladLengthsCacheLocal.ToFrozenDictionary(x => x.Key, x => x.Value.ToFrozenDictionary(y => y.Key, y => y.Value));

            //var _items = Enumerable
            //    .Range(0, 100)
            //    .Select(_ => new KeyValuePair<string, int>(Guid.NewGuid().ToString(), 0))
            //    .ToArray();

            //var _keys = _items.Select(k => k.Key).ToArray();

            //var _dictionary = new Dictionary<string, int>(_items);
            //var _readOnlyDictionary = new ReadOnlyDictionary<string, int>(_items.ToDictionary(i => i.Key, i => i.Value));
            //var _immutableDictionary = ImmutableDictionary.ToImmutableDictionary(_items);
            //var _frozenDictionary = FrozenDictionary.ToFrozenDictionary(_items, false);
            //var _frozenDictionaryReadOptimized = FrozenDictionary.ToFrozenDictionary(_items, true); //optimizedForReads

        }

        void CacheNaladLengthsRc(Uco rc)
        {
            NaladLengthsCacheLocal.TryAdd(rc, new());
            foreach (var whichCondition in System.Enum.GetValues(typeof(RequiredNaladCondition)))
            {
                var cond = (RequiredNaladCondition)whichCondition;
                NaladLengthsCacheLocal[rc].TryAdd(cond,
                    SessionLink.MasterDataLinks.NaladLengthsMasterData.NaladLengthsConverted(
                        SessionLink.MasterDataLinks.NaladLengthsMasterData.GetNaladLengthFromMasterData(this, rc, cond), EdIzmClassificator, KgKm) // Заодно и конвертируем здесь же
                    );
            }
        }

        internal NaladLengthValue GetNaladLengthFromCache(Uco chosenRC, RequiredNaladCondition whichCondition)
        {
            //var key = (chosenRC.Code, whichCondition);
            //if (!NaladLengthsCash.ContainsKey(key)) NaladLengthsCash.TryAdd(key, GetNaladLengthByData(this, chosenRC, whichCondition));
            return NaladLengthsCache[chosenRC][whichCondition];
        }
        #endregion

        #region Параллельное планирование
        /// <summary>
        /// ID параллельного планирования, нужный для разделения операций
        /// </summary>
        public Dictionary<Uco, int> ParallelPlanningIDs;
        #endregion
    }

}

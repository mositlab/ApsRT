using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common;
using Rodana.Core.Session;
using Rodana.Core.Settings;
using InfimumMES;
using Rodana.Core.Rechanges.RechangesEngine;
using Rodana.Core.Rechanges;
using Rodana.Core.Rechanges.MasterData;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Stages
{
    /// <summary>
    /// Элементарный этап, основная единица планирования.
    /// </summary>
    public class Stage
    {
        #region Основы
        public Session.Session SessionLink { get; }

        public Stage(Session.Session session)
        {
            SessionLink = session;
        }

        /// <summary>
        /// Индекс, который проще кода, используется при быстрых расчётах повсюду.
        /// </summary>
        public int Index { get; init; }

        /// <summary>
        /// Уникальный идентификатор этапа.
        /// </summary>
        public Guid GUID { get; init; }

        #endregion

        #region Номенклатурные данные

        /// <summary>
        /// Вид операции
        /// </summary>
        public Uco OperationVid { get; set; }
        public Uco Nomenklature { get; set; }
        public Uco Production { get; set; }
        public Uco Subdivision { get; set; }
        public Uco Specification { get; set; }
        public Uco Characteristics { get; set; }

        #endregion

        #region Основные флаги
        public bool Fixed { get; set; }
        public bool VPZ { get; set; }
        public bool IsFact { get; set; }
        public bool IsLast { get; set; }
        public bool IsFictive { get; set; }
        bool NoChangeTara { get; set; }
        bool UsesVspomTara { get; set; }
        bool IsVspom { get; set; }
        public bool OnOneRC { get; set; }
        #endregion

        #region Рабочие центры
        public Uco OriginalCenter { get; set; }
        public Uco Center { get; set; }
        public List<Uco> PossibleCenters { get; set; }
        public Uco RCRoute { get; set; }
        public bool RCRouteIsRC { get; set; }
        #endregion

        #region Тара
        public Uco EndTara { get; set; }
        public Uco CurrentTara { get; set; }
        public double CurrentTaraMaxCapacity { get; set; }
        #endregion

        #region Реквизиты
        public float?[] NumberProps { get; set; }
        public Guid?[] GuidProps { get; set; }
        public string[] StringProps { get; set; }
        #endregion

        #region Важные данные

        #endregion

        #region Переналадки
        internal StageRechangeParam[] RechangeParameters;

        /// <summary>
        /// Функция вызывается дважды,
        /// один раз - когда я впервые задаю данные в хоппере операций,
        /// во второй раз её можно вызвать после работы партий, там как минимум меняется количество мест, что тоже переналадка
        /// </summary>
        internal void BakeAllIndexDiapazons(RechangesEngineData engineData)
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

        #region Длительность

        /// <summary>
        /// Длительность этого этапа вне зависимости от РЦ
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// Длительность конкретно этого этапа в зависимости от РЦ
        /// </summary>
        Dictionary<Uco, int> SpecifiedDuration;


        int? CashedDuration;
        Dictionary<Uco, int> CashedDurations;

        [Obsolete("В старой версии АПС длительность этапов не зависела от РЦ, данный метод позволяет получить длительность из таблицы операций, но такой подход зачастую не подходит для планирования, используйте длительности, зависящие от РЦ.")]
        public int GetDuration() => Duration.Value;

        public int GetDuration(Uco RC, Session.Session sessionLink)
        {
            if (Duration.HasValue) return Duration.Value;

            if (CashedDuration.HasValue) return CashedDuration.Value;


            if (SpecifiedDuration != null && SpecifiedDuration.TryGetValue(RC, out var specifiedDuration)) return specifiedDuration;

            if (CashedDurations != null && CashedDurations.TryGetValue(RC, out specifiedDuration)) return specifiedDuration;

            var currSets = sessionLink.SettingsController.CurrentSettings;

            // Если нет информации там, ищу в базах данных
            if (sessionLink.MasterDataLinks.StagesTypicalDurationsMasterData.AnyDurations && sessionLink.MasterDataLinks.StagesTypicalDurationsMasterData.TryGetTypicalDuration(RC, OperationVid, out var time))
            {
                if (currSets.CacheStageDurations)
                {
                    if (CashedDurations == null) CashedDurations = new();
                    CashedDurations.TryAdd(RC, time);
                }
                return time;
            }

            if (currSets.CacheStageDurations) CashedDuration = currSets.OriginalDurationTime;
            return currSets.OriginalDurationTime;
        }
        #endregion
    }

}

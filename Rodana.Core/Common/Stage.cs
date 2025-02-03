using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rodana.Core.Session;
using Rodana.Core.Settings;

namespace Rodana.Core.Common
{
    /// <summary>
    /// Элементарный этап, основная единица планирования.
    /// </summary>
    /// <typeparam name="RCCode">Тип кода РЦ</typeparam>
    /// <typeparam name="RCLabel"></typeparam>
    /// <typeparam name="OrderCode"></typeparam>
    /// <typeparam name="OrderLabel"></typeparam>
    /// <typeparam name="CommonCode"></typeparam>
    /// <typeparam name="CommonLabel"></typeparam>
    public class Stage<InputDataType, OrderSubType, SettingsClassType> where SettingsClassType : SettingsClass, new()
    {
        public int ID;

        /// <summary>
        /// Вид операции
        /// </summary>
        public CodedObject<InputDataType> OperationType;


        #region Длительность

        /// <summary>
        /// Длительность этого этапа вне зависимости от РЦ
        /// </summary>
        int? Duration;

        /// <summary>
        /// Длительность конкретно этого этапа в зависимости от РЦ
        /// </summary>
        Dictionary<CodedObject<InputDataType>, int> SpecifiedDuration;


        int? CashedDuration;
        Dictionary<CodedObject<InputDataType>, int> CashedDurations;

        [Obsolete("В старой версии АПС длительность этапов не зависела от РЦ, данный метод позволяет получить длительность из таблицы операций, но такой подход зачастую не подходит для планирования, используйте длительности, зависящие от РЦ.")]
        public int GetDuration() => Duration.Value;

        public int GetDuration(CodedObject<InputDataType> RC, Session<InputDataType, OrderSubType, SettingsClassType> sessionLink)
        {
            if (Duration.HasValue) return Duration.Value;

            if (CashedDuration.HasValue) return CashedDuration.Value;


            if (SpecifiedDuration != null && SpecifiedDuration.TryGetValue(RC, out var specifiedDuration)) return specifiedDuration;

            if (CashedDurations != null && CashedDurations.TryGetValue(RC, out specifiedDuration)) return specifiedDuration;

            var currSets = sessionLink.SettingsController.CurrentSettings;

            // Если нет информации там, ищу в базах данных
            if (sessionLink.StageTypicalDurations.AnyDurations && sessionLink.StageTypicalDurations.TryGetTypicalDuration(RC, OperationType, out var time))
            {
                if (currSets.CasheStageDurations)
                {
                    if (CashedDurations == null) CashedDurations = new();
                    CashedDurations.TryAdd(RC, time);
                }
                return time;
            }

            if (currSets.CasheStageDurations) CashedDuration = currSets.OriginalDurationTime;
            return currSets.OriginalDurationTime;
        }
        #endregion
    }
}

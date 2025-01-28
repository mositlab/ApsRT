using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class Stage<RCCode, RCLabel, OrderCode, OrderLabel, CommonCode, CommonLabel>
    {
        public int ID;

        /// <summary>
        /// Вид операции
        /// </summary>
        public CodedObject<CommonCode, CommonLabel> OperationType;


        #region Длительность

        /// <summary>
        /// Длительность этого этапа вне зависимости от РЦ
        /// </summary>
        int? Duration;

        /// <summary>
        /// Длительность конкретно этого этапа в зависимости от РЦ
        /// </summary>
        Dictionary<CodedObject<RCCode, RCLabel>, int> SpecifiedDuration;

        public int GetDuration(CodedObject<RCCode, RCLabel> RC, Session sessionLink)
        {
            if (Duration.HasValue) return Duration.Value;
            if (SpecifiedDuration != null && SpecifiedDuration.TryGetValue(RC, out var specifiedDuration)) return specifiedDuration;
            // Если нет информации там, ищу в базах данных
            return sessionLink.Settings.OriginalDurationTime;
        }
        #endregion
    }
}

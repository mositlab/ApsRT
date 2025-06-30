using ApsRT.Core.Rechanges.Hoppers;
using UniversalPack.Common;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Rechanges
{
    /// <summary>
    /// Класс переналадки, содержащий непосредственно значения
    /// </summary>
    public class Rechange
    {
        public Uco Name { get; }
        public int Duration { get; }

        public bool ProvokesNaladLengthLaunch { get; }

        public bool HasAdditionalCondition { get; }

        /// <summary>
        /// Многокритериальная переналадка после формирования словаря изучает маску многокритериальности, и если совпали не все критерии (каких-то нет), то она не ставится
        /// </summary>
        public bool IsMultiCriterial { get; }

        public Rechange(Uco name, int duration, bool hasAdditionalCondition, bool provokesNaladLengthLaunch, bool isMultiCriterial)
        {
            Name = name;
            Duration = duration;
            HasAdditionalCondition = hasAdditionalCondition;
            ProvokesNaladLengthLaunch = provokesNaladLengthLaunch;
            IsMultiCriterial = isMultiCriterial;
        }

        public Rechange(RechangeDataLoaderModel model)
        {
            Name = model.RechangeOperation;
            Duration = model.Duration;
            HasAdditionalCondition = model.AdditionalCondition;
            ProvokesNaladLengthLaunch = model.ProvokesLaunch;
            IsMultiCriterial = model.MergeCriterias;
        }

        public override int GetHashCode() => HashCode.Combine(Name.GetHashCode(), Duration, ProvokesNaladLengthLaunch, HasAdditionalCondition);

        public override string ToString() => $"{Name.Label} ({Duration}) с. [{Name.Code}]";
    }

}

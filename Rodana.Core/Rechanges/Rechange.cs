using UniversalPack.Common;

namespace Rodana.Core.Rechanges
{
    /// <summary>
    /// Класс переналадки, содержащий непосредственно значения
    /// </summary>
    internal class Rechange<InputDataType>
    {
        internal CodedObject<InputDataType, string> Name { get; }
        internal int Duration { get; }

        internal bool ProvokesNaladLengthLaunch { get; }

        internal bool HasAdditionalCondition { get; }

        /// <summary>
        /// Многокритериальная переналадка после формирования словаря изучает маску многокритериальности, и если совпали не все критерии (каких-то нет), то она не ставится
        /// </summary>
        internal bool IsMultiCriterial { get; }

        public Rechange(CodedObject<InputDataType, string> name, int duration, bool hasAdditionalCondition, bool provokesNaladLengthLaunch, bool isMultiCriterial)
        {
            Name = name;
            Duration = duration;
            HasAdditionalCondition = hasAdditionalCondition;
            ProvokesNaladLengthLaunch = provokesNaladLengthLaunch;
            IsMultiCriterial = isMultiCriterial;
        }

        public override int GetHashCode() => HashCode.Combine(Name.GetHashCode(), Duration, ProvokesNaladLengthLaunch, HasAdditionalCondition);

        public override string ToString() => $"{Name.Label} ({Duration}) с. [{Name.Code}]";
    }

}

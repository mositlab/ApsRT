using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Settings
{
    /// <summary>
    /// Особый класс настроек, которые скрыты и в интерфейсе, и в файле настроек. Они задаются в коде, и пользователь о них знать не должен. Например, благодаря ним не получится отключить "учёт подразделений" в СКО, и в ЗМК они будут гарантированно отключены, потому что нигде нет галочки, чтобы это изменить.
    /// </summary>
    public class SpecialSettings
    {
        public bool CalculateBestSubdivisionInPartias { get; set; }

        public bool OptimizeSubdivisionsInPartias { get; set; }

        //public string NaladLengthsCriteriaName { get; set; }

        public string LivensteinCodesCriteriaName { get; set; }

        //public bool CalculateSubdivisionsTransfers { get; set; }

        public bool UseOnlyWeightMethodWhenSearchWindings { get; set; }

        public bool UseLegacySettingsWhenNotFound { get; set; }

        public enum PartiaMergingRestrictionType
        {
            LongestOpers_LessThan3,
            Level2_Multiplies2
        }

        public PartiaMergingRestrictionType UsedPartiaMergingRestriction { get; set; }

    }
}

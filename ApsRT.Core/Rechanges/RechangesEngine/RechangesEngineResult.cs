using UniversalPack.Data.Entities;
using System;
using System.Collections.Generic;
using UniversalPack.Common;

namespace ApsRT.Core.Rechanges.RechangesEngine
{
    public class RechangesEngineResult
    {
        public Dictionary<Guid, Rechange>[] Result { get; }

        public Dictionary<Guid, UniversalBinaryStorage> MultiCriterialInfo { get; }

        public int Duration { get; private set; }
        public bool ProvokesNaladLengthLaunch { get; private set; }

        public bool IsDynamicSolution { get; set; }

        public Dictionary<Guid, Rechange> ToLinearDictionary()
        {
            if (Result == null) return null;
            return Result.SelectMany(x => x != null ? (x ?? new()) : new()).ToDictionary(x => x.Key, x => x.Value);
        }

        public void UpdateFields()
        {
            if (Result == null)
            {
                Duration = 0;
                ProvokesNaladLengthLaunch = false;
                return;
            }

            Duration = Result.Length > 0 ? Result.Sum(x => x == null ? 0 : x.Sum(y => y.Value.Duration)) : 0;
            ProvokesNaladLengthLaunch = Result.Length > 0 ? Result.Any(x => x == null ? false : x.Any(y => y.Value.ProvokesNaladLengthLaunch)) : false;
        }

        public RechangesEngineResult(Dictionary<Guid, Rechange>[] result, Dictionary<Guid, UniversalBinaryStorage> multiCriterialInfo, int duration, bool provokesNaladLengthLaunch)
        {
            Result = result;
            Duration = duration;
            ProvokesNaladLengthLaunch = provokesNaladLengthLaunch;
            MultiCriterialInfo = multiCriterialInfo;
        }
    }

}

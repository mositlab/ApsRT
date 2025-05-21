using UniversalPack.Data.Entities;
using System;
using System.Collections.Generic;
using UniversalPack.Common;

namespace Rodana.Core.Rechanges.RechangesEngine
{
    internal class RechangesEngineResult<InputDataType>
    {
        internal Dictionary<Guid, Rechange>[] Result { get; }

        internal Dictionary<Guid, UniversalBinaryStorage> MultiCriterialInfo { get; }

        internal int Duration { get; private set; }
        internal bool ProvokesNaladLengthLaunch { get; private set; }

        internal bool IsDynamicSolution { get; set; }

        internal Dictionary<Guid, Rechange> ToLinearDictionary()
        {
            if (Result == null) return null;
            return Result.SelectMany(x => x != null ? (x ?? new()) : new()).ToDictionary(x => x.Key, x => x.Value);
        }

        internal void UpdateFields()
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

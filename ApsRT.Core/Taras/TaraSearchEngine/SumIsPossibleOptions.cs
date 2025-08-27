using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Taras.TaraSearchEngine
{
    public class SumIsPossibleOptions
    {
        public bool WithLog; // в лог писать ли (текстовик для меня)
        public Dictionary<int, Uco> PrevsInfo = null; // в такой структуре он нуждается из-за поиска РЦ с отдающей тарой
        public HashSet<Uco> NextRCsMustBeThese = null; // ограничение, что у детей должны быть только эти РЦ, пришлось юзать тут
        public bool NextRCsExcludeNoChangeTara = false;// если да, то в NextRCsMustBeThese не должно быть с признаком не менять тару
        public HashSet<Uco>? RCMustBeThis = null; // ограничение, что РЦ должен быть именно этот (для V)
        public HashSet<Uco>? TaraMustBeThis = null; // ограничение, что тара по велению V должна быть именно эта
        public Uco? RecommendedSubdivision = null; // ограничение что рекомендуется тот или иной цех при подборе РЦ
        public Uco? RecommendedTara = null; // рекомендую именно эту тару в первую очередь
        public HashSet<Uco> ExceptTheseTaras = null; // ограничение, что вот эти тары юзать нельзя (в G исползьовалось)
        public bool LogSomeErrors = true;
        public float NaladLength = 0;
        public bool SearchForMaxTara = false;
        public ConcurrentDictionary<int, TemporaryTaraSearchData> ModToTaraInfoLink;

        internal bool SaveAvailableResults = false;

        public SumIsPossibleOptions(bool WithLog)
        {
            this.WithLog = WithLog;
        }
    }
}

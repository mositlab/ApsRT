using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.TaraSearchEngine
{
    internal class SumIsPossibleOptions
    {
        internal bool WithLog; // в лог писать ли (текстовик для меня)
        internal Dictionary<int, Uco> PrevsInfo = null; // в такой структуре он нуждается из-за поиска РЦ с отдающей тарой
        internal HashSet<Uco> NextRCsMustBeThese = null; // ограничение, что у детей должны быть только эти РЦ, пришлось юзать тут
        internal bool NextRCsExcludeNoChangeTara = false;// если да, то в NextRCsMustBeThese не должно быть с признаком не менять тару
        internal Uco? RCMustBeThis = null; // ограничение, что РЦ должен быть именно этот (для V)
        internal Uco? TaraMustBeThis = null; // ограничение, что тара по велению V должна быть именно эта
        internal Uco? RecommendedSubdivision = null; // ограничение что рекомендуется тот или иной цех при подборе РЦ
        internal HashSet<Uco> ExceptTheseTaras = null; // ограничение, что вот эти тары юзать нельзя (в G исползьовалось)
        internal bool LogSomeErrors = true;
        internal float NaladLength = 0;
        internal bool SearchForMaxTara = false;
        internal ConcurrentDictionary<int, TemporaryTaraSearchData> ModToTaraInfoLink;

        public SumIsPossibleOptions(bool WithLog)
        {
            this.WithLog = WithLog;
        }
    }
}

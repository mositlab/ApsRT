using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Planning.PlanningClasses
{

    public class OpersBlock
    {
        public List<PlanningModelling> Lists;
        public Uco ActualRC;

        FrozenDictionary<Uco, TimeSpan> _maxTime;
        int lastCount = 0;

        public TimeSpan MaxTime(Uco center)
        {
            if (lastCount != Lists.Count)
            {
                _maxTime = Lists[0].PossibleRCsHere.ToFrozenDictionary(center => center, center => Lists.Max(x => x.AllTime[center]));
                lastCount = Lists.Count;
            }
            return _maxTime[center];
            //Lists.Count == 1 ? Lists [0].AllTime : ; // Макс. время для вывода надо, вместо разных, в случае, если тут параллелизм
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", Lists.Select(x => x.ModID))}] на РЦ: {ActualRC}";
        }

        public OpersBlock(List<PlanningModelling> lists, Uco ActualRC, float ParrKoeff, bool CreateTara)
        {
            Lists = lists;
            this.ActualRC = ActualRC;
            FillKoeff = ParrKoeff;
            if (CreateTara)
            {
                var tmp1 = lists.Select(x => x.FirstOper.CurrentTara).Where(x => !x.IsEmpty);//.GroupBy(x => x);
                ExistingTaraHere = new();
                foreach (var el in tmp1)
                {
                    if (!ExistingTaraHere.ContainsKey(el))
                    {
                        ExistingTaraHere.Add(el, 0);
                    }

                    ExistingTaraHere[el]++;
                }
            }
        }

        /// <summary>
        /// Какие тут тары и в каком количестве есть.
        /// </summary>
        public Dictionary<Uco, int> ExistingTaraHere = new();

        /// <summary>
        /// Насколько заполнен блок. От 0 до 1.
        /// -1 значит, что этот блок в принципе не поддерживает параллелизм, бладаря этому лезть в словарь рабочих центров не надо - микрооптимизация.
        /// </summary>
        public float FillKoeff = -1;

        /// <summary>
        /// Сколько ещё тар можно вместить.
        /// Понадобится для небольшой оптимизации - если количество моделирований (оно же количество тар в блоке) больше свободного места тут, то блок просто не будет предложен.
        /// </summary>
        public int HowMuchPlaceHere = 0;

        // TODO: для моделирований списки детей и предков есть. Теперь не мешало бы устроить оптимизацию и поблочно.
    }
}

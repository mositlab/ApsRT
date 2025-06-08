using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.NaladLengths
{
    public struct GottenNaladLengthStruct
    {
        public NaladLengthValue NaladLengthFirst; // первая тара
        public NaladLengthValue NaladLengthOther; // остальные тары
        public NaladLengthValue NaladLengthNexts; // по факту сумма - для след. уровней
        public NaladLengthValue NaladLengthHere; // сумма только текущего уровня (1, 7, 12 мест)
        public bool IsFirst; // первый ли это запуск оказался
        public float prevKoeff;

        public GottenNaladLengthStruct(
            NaladLengthValue naladLengthFirst,
            NaladLengthValue naladLengthOther,
            NaladLengthValue naladLengthNexts,
            NaladLengthValue naladLengthHere,
            bool isFirst)
        {
            NaladLengthFirst = naladLengthFirst;
            NaladLengthOther = naladLengthOther;
            NaladLengthNexts = naladLengthNexts;
            NaladLengthHere = naladLengthHere;
            IsFirst = isFirst;
            prevKoeff = 0;
        }

        public override string ToString()
        {
            return $"{NaladLengthFirst}, {NaladLengthOther}: {NaladLengthHere} (all {NaladLengthNexts}) [{IsFirst}]";
        }
    }
}

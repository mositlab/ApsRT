using Azure;
using ApsRT.Core.Partias.TaraSearchEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Partias.PartiasClasses
{
    public class PartiaModellingSmall : IAbleForTaraCalcInterface
    {
        public List<Stages.Stage> Opers; // Внутри сдвига - моделирование (несколько операций может быть

        Stages.Stage IAbleForTaraCalcInterface.FirstOperLink => Opers[0];

        int IAbleForTaraCalcInterface.ModId => -1;

        List<Stages.Stage> IAbleForTaraCalcInterface.AllOpers => Opers;

        double IAbleForTaraCalcInterface.Sum => //Opers[0].Modelling != Guid.Empty ? (Opers.GroupBy(x => x.Modelling).Sum(x => x.First().Count)) // группируем и берём первую же операцию ДО 01.08.24
                                                //: 
            Opers.Sum(x => x.Count); // Если задано моделирование из меса - значит это заблокированная партия в любом случае, и надо смотреть не сумму, а количество первой же (ибо там количество - суммарное). TODO: как то это ускорить можно по идее

        int IAbleForTaraCalcInterface.ZadelCode => ZadelCode;


        /// <summary>
        /// Число задела.
        /// 0 = пустые заделы. В остальном это нечто вроде хэш кода для сравнения, хранения и ограничения.
        /// </summary>
        public int ZadelCode = 0;

        /// <summary>
        /// Гуид задела. Сравниваются и схлопываются только сдвиги с одинаковым гуидом
        /// </summary>
        public List<Guid> ZadelsGUIDs = new();

        public PartiaModellingSmall(List<Stages.Stage> opers)
        {
            Opers = opers;
            ZadelCode = 0;
            ZadelsGUIDs = null;
        }

        public PartiaModellingSmall(List<Stages.Stage> opers, int zadelCode, List<Guid> zadelGuids)
        {
            Opers = opers;
            ZadelCode = zadelCode;
            ZadelsGUIDs = zadelGuids;
        }


        public override string ToString() => String.Format("#{0:X} {1}", GetHashCode(), ZadelCode);
    }
}

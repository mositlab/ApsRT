using Azure;
using Rodana.Core.Partias.TaraSearchEngine;
using Rodana.Core.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Planning.PlanningClasses
{
    public class PlanningModelling : IAbleForTaraCalcInterface
    {
        public List<Stage> OpersList;
        public TimeSpan AllTime;
        //public TimeSpan MaxTime;
        public double TimeK;
        public double Count; // Тут эта фигня нужна ДЛЯ: а) расчёта тары в Вишке (пригодится для макс намоток) и б) в будущем пригодится, тк 04.06.2024: количество для модов, пришедших из меса, уже задано, как сумма всех исходных этапов.
        public ModellingType ModType;

        public List<Uco> PossibleRCsHere; // иногда РЦ могут немного отличаться у того или иного этапа, ибо нет проверки на совпадение маршрутов РЦ. Поэтому создан отдельный список, чтоб можно было чекнуть.

        public Stage FirstOper;

        Stage IAbleForTaraCalcInterface.FirstOperLink => FirstOper;

        int IAbleForTaraCalcInterface.ModId => ModID;

        List<Stage> IAbleForTaraCalcInterface.AllOpers => OpersList;

        double IAbleForTaraCalcInterface.Sum => Count; // Тут другая реализация, так как все равно пришлось клепать коэффициенты для последующего красивого отображения в гуе.

        int IAbleForTaraCalcInterface.ZadelCode => 0;

        public Dictionary<Guid, (Stage FirstOper, int Count, double Sum)> ObshEtapData;

        public int ModID; // чисто номер мода. Попытка оптимизации
        public List<int> NextMods = new();
        public List<int> PrevMods = new();

        public PlanningModelling(List<Stage> list, bool parrRC, ModellingType type, int modId,
            List<int> prevs, List<int> nexts)
        {
            if (type == ModellingType.Obch_ETAP_FromMES || type == ModellingType.Obch_ETAP_AfterPartii)
            {
                // если это общий этап, то стоит отсортировать операции, чтобы в выходной таблице они не перемешивались (моделирования перемешивались до 04.06)
                list = list.OrderBy(x => x.EasyFlag).ThenBy(x => x.IsCustomModelling ? x.CustomModelling : x.MesModelling.Code).ToList();
                var modsHere = list.GroupBy(x => x.IsCustomModelling ? x.CustomModelling : x.MesModelling.Code);
                ObshEtapData = modsHere.ToDictionary(x => x.Key, x => (x.First(), x.Count(), /*type == ModellingType.Obch_ETAP_FromMES ? x.First().Count : */x.Sum(x => x.Count))); // создаем словарь чтоб он помогал нам понять, что в этом общем этапе есть
            }

            OpersList = list;
            FirstOper = list[0];
            ModType = type;
            ModID = modId;
            PrevMods = prevs;
            NextMods = nexts;

            void classic()
            {
                // Если это после формирования партий, то суммируем (по старинке). Если РЦ параллельный, то вместо суммы нужен отжиг.
                var timeSum = list.Sum(x => x.GetDuration());
                var timeMax = parrRC ? list.Max(x => x.GetDuration()) : timeSum;
                AllTime = TimeSpan.FromSeconds(timeMax);
                TimeK = (double)timeMax / timeSum;
                Count = list.Sum(x => x.Count);
            }

            void justSum()
            {
                // Пришедший из МЕС-а мод, согласно Алёне (04.06) содержит количество и время, которое уже является суммарным. Поэтому мы берём инфу из первой же операции, и работаем с ней
                AllTime = TimeSpan.FromSeconds(FirstOper.GetDuration());
                TimeK = 1.0 / list.Count; // делим коэффициент на количество, так как нужно будет потом юзать при отрисовке ганта
                //Count = FirstOper.Count; // совсем недавно была лишь первая но 01.08.24 поправили
                Count = list.Sum(x => x.Count);
            }

            switch (type)
            {
                // Исходный этап
                case ModellingType.Elementar: // Это исходный этапчик
                    {
                        AllTime = TimeSpan.FromSeconds(FirstOper.GetDuration());
                        TimeK = 1;
                        Count = FirstOper.Count;
                        break;
                    }

                // Обработка модов, сформированных АПС-кой
                case ModellingType.AfterPartii:
                    {
                        classic();
                        break;
                    }
                case ModellingType.Obch_ETAP_AfterPartii: // что общий этап, что моделирования - после партий суммируются по старинке
                    {
                        classic();
                        break;
                    }

                // Обработка моделирований из меса
                case ModellingType.FromMES:
                    {
                        justSum();
                        break;
                    }
                case ModellingType.Obch_ETAP_FromMES:
                    {
                        justSum();
                        break;
                    }
            }

            foreach (var oper in list) // Создаём массив тех РЦ, которые однозначно есть на всех этапах
            {
                var rcs = oper.PossibleCenters ?? new() { oper.Center };
                if (PossibleRCsHere == null)
                    PossibleRCsHere = rcs;
                else
                    PossibleRCsHere = PossibleRCsHere.Intersect(rcs).ToList();
            }
        }
    }
}

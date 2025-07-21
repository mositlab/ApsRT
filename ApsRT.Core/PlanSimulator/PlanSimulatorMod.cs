using ApsRT.Core.Stages;
using ApsRT.Core.Taras.TaraSearchEngine;
using System.Collections.Frozen;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.PlanSimulator
{
    public class PlanSimulatorMod : IAbleForTaraCalcInterface
    {
        public PlanSimulatorBlock BlockLink;

        public List<PlanSimulatorStage> Stages;
        public List<Stage> StagesAsNormal;

        public PlanSimulatorNaladLengthData[] NaladLengthData; // ТОЛЬКО для моделирований простых, для общих этапов там свои
        public PlanSimulatorDatesData[] MinDatesData;
        public int[] Ranks;

        public FrozenDictionary<Uco, TimeSpan> AllTime;
        //public TimeSpan MaxTime;
        public FrozenDictionary<Uco, double> TimeK;
        public double Count; // Тут эта фигня нужна ДЛЯ: а) расчёта тары в Вишке (пригодится для макс намоток) и б) в будущем пригодится, тк 04.06.2024: количество для модов, пришедших из меса, уже задано, как сумма всех исходных этапов.
        public PlanningModellingType ModType;

        public List<Uco> PossibleRCsHere; // иногда РЦ могут немного отличаться у того или иного этапа, ибо нет проверки на совпадение маршрутов РЦ. Поэтому создан отдельный список, чтоб можно было чекнуть.

        public Stage FirstOperJust;
        public PlanSimulatorStage FirstOper;

        Stage IAbleForTaraCalcInterface.FirstOperLink => FirstOperJust;

        int IAbleForTaraCalcInterface.ModId => ModID;

        List<Stage> IAbleForTaraCalcInterface.AllOpers => StagesAsNormal;

        double IAbleForTaraCalcInterface.Sum => Count; // Тут другая реализация, так как все равно пришлось клепать коэффициенты для последующего красивого отображения в гуе.

        int IAbleForTaraCalcInterface.ZadelCode => 0;

        public Dictionary<Guid, (Stage FirstOper, IAbleForTaraCalcInterface AsMod, int FirstElementIndex, int Count, double Sum, PlanSimulatorNaladLengthData[] NaladLengthData)> EasyLifeData;

        public int ModID; // чисто номер мода. Попытка оптимизации

        public List<PlanSimulatorMod> NextMods = new();
        public List<PlanSimulatorMod> PrevMods = new();

        public PlanSimulatorMod(List<Stage> list, bool parrRC, PlanningModellingType type, int modId, int poolSize)
        {
            if (type == PlanningModellingType.EasyFlagFromMES || type == PlanningModellingType.EasyFlagAfterPartias)
            {
                // если это общий этап, то стоит отсортировать операции, чтобы в выходной таблице они не перемешивались (моделирования перемешивались до 04.06)
                list = list.OrderBy(x => x.EasyFlag).ThenBy(x => x.CurrentModelling/*IsCustomModelling ? x.CustomModelling : x.MesModelling.Code*/).ToList();
                var modsHere = list.GroupBy(x => x.CurrentModelling/*IsCustomModelling ? x.CustomModelling : x.MesModelling.Code*/);

                Dictionary<Guid, (Stage FirstOper, IAbleForTaraCalcInterface AsMod, int FirstElementIndex, int Count, double Sum, PlanSimulatorNaladLengthData[] NaladLengthData)> final = new();
                var listFinal = new List<Stage>();
                foreach (var group in modsHere)
                {
                    PlanSimulatorNaladLengthData[] naladLengths = new PlanSimulatorNaladLengthData[poolSize];
                    for (int i = 0; i < poolSize; i++)
                    {
                        naladLengths[i] = new PlanSimulatorNaladLengthData(-1, new NaladLengths.GottenNaladLengthStruct());
                    }
                    var listToAdd = group.ToList();
                    final.Add(group.Key, (listToAdd[0], new Partias.PartiasClasses.PartiaModellingSmall(listToAdd), listFinal.Count, listToAdd.Count, listToAdd.Sum(x => x.Count), naladLengths));
                    listFinal.AddRange(listToAdd);
                }

                EasyLifeData = final;
                list = listFinal;

                //modsHere.ToDictionary(x => x.Key, x => (x.First(), x.Count(), /*type == ModellingType.Obch_ETAP_FromMES ? x.First().Count : */x.Sum(x => x.Count))); // создаем словарь чтоб он помогал нам понять, что в этом общем этапе есть
            }

            StagesAsNormal = list;
            Stages = list.Select(x => new PlanSimulatorStage()
            {
                Stage = x,
                ModLink = this
            }).ToList();
            FirstOperJust = list[0];
            FirstOper = Stages[0];
            ModType = type;
            ModID = modId;


            foreach (var oper in list) // Создаём массив тех РЦ, которые однозначно есть на всех этапах
            {
                var rcs = oper.PossibleCenters ?? new() { oper.OriginalCenter };
                if (PossibleRCsHere == null)
                    PossibleRCsHere = rcs;
                else
                    PossibleRCsHere = PossibleRCsHere.Intersect(rcs).ToList();
            }

            Count = list.Sum(x => x.Count);

            switch (type)
            {
                // Исходный этап
                case PlanningModellingType.Elementar: // Это исходный этапчик
                    {
                        AllTime = PossibleRCsHere.ToFrozenDictionary(x => x, x => TimeSpan.FromSeconds(FirstOperJust.GetDuration(x)));
                        TimeK = AllTime.ToFrozenDictionary(x => x.Key, x => 1.0);
                        Count = FirstOperJust.Count;
                        break;
                    }

                default:
                    {
                        // Если это после формирования партий, то суммируем (по старинке). Если РЦ параллельный, то вместо суммы нужен отжиг.
                        var timeSum = PossibleRCsHere.ToFrozenDictionary(x => x, x => list.Sum(y => y.GetDuration(x)));
                        var timeMax = parrRC ? PossibleRCsHere.ToFrozenDictionary(x => x, x => list.Max(y => y.GetDuration(x))) : timeSum;
                        AllTime = timeMax.ToFrozenDictionary(x => x.Key, x => TimeSpan.FromSeconds(x.Value));
                        TimeK = timeMax.ToFrozenDictionary(x => x.Key, x => (double)timeMax[x.Key] / timeSum[x.Key]);
                        break;
                    }

                    // Обработка модов, сформированных АПС-кой
                    //case PlanningModellingType.AfterPartias:
                    //    {
                    //        classic();
                    //        break;
                    //    }
                    //case PlanningModellingType.EasyFlagAfterPartias: // что общий этап, что моделирования - после партий суммируются по старинке
                    //    {
                    //        classic();
                    //        break;
                    //    }

                    //// Обработка моделирований из меса
                    //case PlanningModellingType.FromMES:
                    //    {
                    //        justSum();
                    //        break;
                    //    }
                    //case PlanningModellingType.EasyFlagFromMES:
                    //    {
                    //        justSum();
                    //        break;
                    //    }
            }



            //void classic()
            //{
            //    // Если это после формирования партий, то суммируем (по старинке). Если РЦ параллельный, то вместо суммы нужен отжиг.
            //    var timeSum = list.Sum(x => x.GetDuration());
            //    var timeMax = parrRC ? list.Max(x => x.GetDuration()) : timeSum;
            //    AllTime = TimeSpan.FromSeconds(timeMax);
            //    TimeK = (double)timeMax / timeSum;
            //}

            //void justSum()
            //{
            //    // Пришедший из МЕС-а мод, согласно Алёне (04.06) содержит количество и время, которое уже является суммарным. Поэтому мы берём инфу из первой же операции, и работаем с ней
            //    AllTime = TimeSpan.FromSeconds(FirstOper.GetDuration());
            //    TimeK = 1.0 / list.Count; // делим коэффициент на количество, так как нужно будет потом юзать при отрисовке ганта
            //    //Count = FirstOper.Count; // совсем недавно была лишь первая но 01.08.24 поправили
            //}
        }
    }
}
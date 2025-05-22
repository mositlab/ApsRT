using Newtonsoft.Json;
using UniversalPack.Common;

namespace Rodana.Core.Centers
{
    public class CenterClass
    {

        internal bool CanBeParallel;

        // Сначала - динамичные данные
        internal bool HasAdditionalActions;
        //internal UsedSystem EdIzm;
        internal bool NotChangeTara;
        internal UniversalBinaryStorage PossibleRechanges;
        internal UniversalBinaryStorage PossibleAdditionalActions;
        internal bool IsFromTable;
        internal bool IsWithRechanges;
        public int? MaxMinutesAwaiting; // макс время ожидания

        public int RowIndex;


        // А это - сохраняемые
        [JsonProperty]
        public bool IsHiddenOnDiagrams { get; set; } // скрыт ли на диаграмме
        [JsonProperty]
        public bool IsUsedInFilter { get; set; } // используется ли фильтре в начале расчётов
        [JsonProperty]
        public bool IncludesInProstoi { get; set; } // входит ли в простои
        [JsonProperty]
        public bool UnusedInProduction { get; set; } // исключен из расчёта продукции
        [JsonProperty]
        public bool WillRemoveEverything { get; set; } // удалит ли из расчёта всё, что с ним связано
        [JsonProperty]
        public bool ParallelModelling { get; set; } // есть ли тут параллельное моделирование
        [JsonProperty]
        public bool AbleToVspom { get; set; } // умеет ли обрабатывать вспомогательные
        [JsonProperty]
        public int MaxParallelOpersCount { get; set; } // сколько операций может выполнить одновременно
        [JsonProperty]
        public List<string> UsedLabels;
        [JsonProperty]
        public decimal TimeK { get; set; } // коэф времени
    }
}

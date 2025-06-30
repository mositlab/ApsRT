using System.Numerics;

namespace ApsRT.Core.Rechanges
{
    /// <summary>
    /// Класс, где хранится часть НСИ об одном из критериев переналадки, уже в разрезе РЦ и вида
    /// </summary>
    internal class RechangeCriteriaStorage<ActionType>
    {
        // Три переналадки ниже проще, потому что они не делятся на типы (float?, Coded?), и поэтому к ним обращаться ЛУЧШЕ.

        /// <summary>
        /// Переналадка с ничего на ничего, может быть null.
        /// </summary>
        internal List<ActionType> FromNothingToNothing { get; set; }

        /// <summary>
        /// Переналадка с ничего на любой, может быть null;
        /// </summary>
        internal List<ActionType> FromNothingToAnyting { get; set; }

        /// <summary>
        /// Переналадка с любого на ничего, может быть null;
        /// </summary>
        internal List<ActionType> FromAnytingToNothing { get; set; }

        /// <summary>
        /// Переналадка с любого на любой другой, может быть null
        /// </summary>
        internal List<ActionType> FromAnythingToAnyElse { get; set; }

        /// <summary>
        /// Переналадка с любого на любой, может быть null
        /// </summary>
        internal List<ActionType> FromAnythingToAnything { get; set; }

        /// <summary>
        /// Дополнительный словарь, особый, который можно использовать для перехода с числа на диапазон. Активен, если критерий ЧИСЛО И ДИАПАЗОН
        /// </summary>
        internal Dictionary<float, Dictionary<int, List<ActionType>>> FromNumberToDiapazon { get; set; }

        /// <summary>
        /// Дополнительный словарь, особый, который можно использовать для перехода с диапазона на число. Активен, если критерий ЧИСЛО И ДИАПАЗОН
        /// </summary>
        internal Dictionary<int, Dictionary<float, List<ActionType>>> FromDiapazonToNumber { get; set; }

        Dictionary<Vector2, int> ExistingDiapazons;

        internal int? AddOrGetDiapazonByVector(Vector2 Value)
        {
            ExistingDiapazons ??= new();
            //if (ExistingDiapazons == null) return null;
            foreach (var vec in ExistingDiapazons)
            {
                if (vec.Key == Value) return vec.Value;
            }

            // Ну тогда добавляем
            ExistingDiapazons.Add(Value, ExistingDiapazons.Count);
            return ExistingDiapazons.Count - 1;
        }

        internal int? GetDiapazonByValue(float? Value)
        {
            if (ExistingDiapazons == null || !Value.HasValue) return null;
            foreach (var vec in ExistingDiapazons)
            {
                if (vec.Key.X <= Value.Value && Value.Value <= vec.Key.Y) return vec.Value;
            }
            return null;
        }

        internal RechangeCriteriaSubStorage<float?, ActionType> NumberSubStorage { get; set; }

        internal RechangeCriteriaSubStorage<Guid?, ActionType> GuidSubStorage { get; set; }

        internal RechangeCriteriaSubStorage<string, ActionType> StringSubStorage { get; set; }

        internal RechangeCriteriaSubStorage<int?, ActionType> DiapazonSubStorage { get; set; }
    }

}

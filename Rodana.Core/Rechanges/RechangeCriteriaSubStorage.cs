namespace Rodana.Core.Rechanges
{
    internal class RechangeCriteriaSubStorage<DataType, ActionType>
    {
        /// <summary>
        /// С ничего - на конкретное значение
        /// </summary>
        internal Dictionary<DataType, List<ActionType>> FromNothingToValue { get; set; }

        /// <summary>
        /// С конкретного значения - на любое
        /// </summary>
        internal Dictionary<DataType, List<ActionType>> FromValueToAnything { get; set; }

        /// <summary>
        /// С конкретного значения - на любое другое
        /// </summary>
        internal Dictionary<DataType, List<ActionType>> FromValueToAnyElse { get; set; }

        /// <summary>
        /// С любого другого на конкретное значение
        /// </summary>
        internal Dictionary<DataType, List<ActionType>> FromAnyElseToValue { get; set; }

        /// <summary>
        /// С любого на конкретное значение
        /// </summary>
        internal Dictionary<DataType, List<ActionType>> FromAnythingToValue { get; set; }

        /// <summary>
        /// С конкретного значения - на ничего
        /// </summary>
        internal Dictionary<DataType, List<ActionType>> FromValueToNothing { get; set; }

        /// <summary>
        /// С конкретного значения - на конкретное
        /// </summary>
        internal Dictionary<DataType, Dictionary<DataType, List<ActionType>>> FromValueToValue { get; set; }
    }

}

using WinFormsCustomComponents;
using Rodana.Core.Hoppers;

namespace Rodana.ZMK
{
    /// <summary>
    /// Дополнительный класс Хоппера, который содержит в себе в UI элементы для формы.
    /// </summary>
    internal class HopperUIClass
    {
        internal CustomProgressBar Bar { get; set; }
        internal Label Label { get; set; }
        internal Button Button { get; set; }
        internal TextBox PathTextBox { get; set; }
        internal IHopperInterface HopperLink { get; set; }
        internal HopperAttribute HopperData { get; set; }
    }
}

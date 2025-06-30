using WinFormsCustomComponents;
using ApsRT.Core.Hoppers;

namespace ApsRTCustomComponents
{
    /// <summary>
    /// Дополнительный класс Хоппера, который содержит в себе в UI элементы для формы.
    /// </summary>
    public class HopperUIClass
    {
        public CustomProgressBar Bar { get; set; }
        public Label Label { get; set; }
        public Button Button { get; set; }
        public TextBox PathTextBox { get; set; }
        public IHopperInterface HopperLink { get; set; }
        public HopperAttribute HopperData { get; set; }
    }
}

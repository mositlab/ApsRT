using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Hoppers
{
    /// <summary>
    /// Атрибут для Хоппера (устар. Модуля, Загрузчика), если он висит над классом, значит это класс-Хоппер, и будет использоваться в загрузке данных.
    /// Атрибут позволяет хранить информацию, как конкретно работает Хоппер, и какое у него имя и метка в интерфейсе.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HopperAttribute : Attribute
    {
        public string Label { get; set; }
        public bool LoadWhenOnlyPart { get; set; } = true; // загружать при ТОЛЬКО ПАРТИЯХ?
        public bool LoadWhenOnlyZadel { get; set; } // загружать, только если я в режиме задела?
        public bool IsMultiple { get; set; }
        public bool IsDirectory { get; set; }
        public bool UsesGuid { get; set; }
        public string[] NeededHoppers { get; set; }
        public bool LoadedFlag { get; set; }
        public int HopperIndex { get; set; }
    }
}

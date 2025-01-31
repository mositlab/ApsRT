using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Hoppers
{
    /// <summary>
    /// Атрибут для Хоппера (устар. Модуля, Загрузчика), если он висит над классом, значит это класс-Хоппер, и будет использоваться в загрузке данных.
    /// Атрибут позволяет хранить информацию, как конкретно работает Хоппер, и какое у него имя и метка в интерфейсе.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HopperAttribute : Attribute
    {
        public string Label;
        public bool LoadWhenOnlyPart = true; // загружать при ТОЛЬКО ПАРТИЯХ?
        public bool LoadWhenOnlyZadel = false; // загружать, только если я в режиме задела?
        public bool IsMultiple;
        public bool IsDirectory;
        public bool UsesGuid;
        public string[] NeededHoppers;
    }
}

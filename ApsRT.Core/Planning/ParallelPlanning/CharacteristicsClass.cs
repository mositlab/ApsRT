using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Planning.ParallelPlanning
{
    /// <summary>
    /// Класс, который содержит в себе кучу характеристик и позволяет операции сравниться с собой. Нужен для словаря ниже
    /// </summary>
    internal class CharacteristicsClass
    {
        internal Uco MyName { get; init; } // TODO: можно как нибудь в принципе убрать это. Там два словаря слить в один.
        public override string ToString() => MyName.ToString();
        /// <summary>
        /// Список характеристик внутри набора. Тут в принципе кодед обж не нужен, но по красоте сотавил
        /// </summary>
        internal Dictionary<Uco, ConcreteCharacteristic> ListOfChars = new();

        public CharacteristicsClass(Uco myName)
        {
            MyName = myName;
            ListOfChars = new();
        }

        /// <summary>
        /// Позволяет сравнить операцию с характеристиками этого класса. Понадобится при поиске айдишника пар. моделирования
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        internal bool IsEqualWith(Stages.Stage stage)
        {
            return ListOfChars.All(x => x.Value.IsEqualWith(stage));
        }
    }
}

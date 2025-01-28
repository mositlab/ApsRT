using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core
{
    /// <summary>
    /// Класс сессии. Каждая сессия - это по сути и есть экземпляр АПС. 
    /// В ней создаются классы переналадок и прочего во время вызова конструктора.
    /// Сессия нуждается в загрузке данных через вызов метода Init в ней
    /// После того, как Родана загрузит настройки и с их помощью активирует Хопперы.
    /// Теперь, запуская алгоритмы планирования и партий, можно хавать данные из данных сессии.
    /// </summary>
    public class Session
    {
        public SettingsClass Settings;
        /*
         internal RechangeEngine RechangeEngine;
         */
    }
}

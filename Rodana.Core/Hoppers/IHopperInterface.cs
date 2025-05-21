using Rodana.Core.Session;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;

namespace Rodana.Core.Hoppers
{
    /// <summary>
    /// Интерфейс Хоппера (устар. Модуля, Загрузчика), содержащий все функции для загрузки данных из файлов
    /// </summary>
    /// <remarks>
    /// <b>ВСЕ</b> данные, импортируемые реализациями этого интерфейса, необходимо привести к загрузке в модель, наследуемую от <see cref="Models.DataLoader.BaseDataLoaderModel"/>
    /// <br/>
    /// Механизм загрузки данных нужно отвязать от UI и использовать для этого события, а также методы из <see cref="Extensions.EventExtensions"/>
    /// </remarks>
    public interface IHopperInterface<InputDataType, OrderSubType>
    {
        /// <summary>
        /// Вызывается перед началом загрузки всех файлов.
        /// </summary>
        /// <param name="filenames">Список файлов к загрузке.</param>
        void BeforeAllLoading(Session<InputDataType, OrderSubType> session, string[] filenames);

        /// <summary>
        /// Вызывается перед началом загрузки конкретного файла.
        /// </summary>
        /// <param name="filename">Путь до файла.</param>
        void BeforeLoading(string filename, DataLoaderOptions options);

        /// <summary>
        /// Вызывается при чтении каждой строки.
        /// </summary>
        /// <param name="rowIndex">Номер строки.</param>
        /// <param name="data">Одна строка табличных данных</param>
        void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options);

        /// <summary>
        /// Вызывается при просмотре каждого файла в случае, если этот Хоппер занимается чтением папки.
        /// </summary>
        /// <param name="filename">Путь до файла.</param>
        void ActionFile(string filename);

        /// <summary>
        /// Вызывается после загрузки каждого файла.
        /// </summary>
        void AfterLoading();
    }
}

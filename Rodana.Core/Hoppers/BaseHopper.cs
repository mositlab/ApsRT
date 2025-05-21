using Rodana.Core.Session;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rodana.Core.Hoppers
{
    public abstract class BaseHopper<InputDataType, OrderSubType>
    {
        protected Session<InputDataType, OrderSubType> session;
        public BaseHopper(Session<InputDataType, OrderSubType> session)
        {
            this.session = session;
        }

        /// <summary>
        /// Вызывается перед началом загрузки всех файлов.
        /// </summary>
        /// <param name="filenames">Список файлов к загрузке.</param>
        public abstract void BeforeAllLoading(string[] filenames);

        /// <summary>
        /// Вызывается перед началом загрузки конкретного файла.
        /// </summary>
        /// <param name="filename">Путь до файла.</param>
        public abstract void BeforeLoading(string filename, HopperReadOptions options);

        /// <summary>
        /// Вызывается при чтении каждой строки.
        /// </summary>
        /// <param name="rowIndex">Номер строки.</param>
        /// <param name="data">Одна строка табличных данных</param>
        public abstract void OnLoading(int rowIndex, IDataReader data, HopperReadOptions options);

        /// <summary>
        /// Вызывается при просмотре каждого файла в случае, если этот Хоппер занимается чтением папки.
        /// </summary>
        /// <param name="filename">Путь до файла.</param>
        public abstract void ActionFile(string filename);

        /// <summary>
        /// Вызывается после загрузки каждого файла.
        /// </summary>
        public abstract void AfterLoading();
    }
}

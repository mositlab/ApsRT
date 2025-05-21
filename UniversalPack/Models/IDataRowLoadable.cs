using System.Data;

namespace UniversalPack.Models
{
    public interface IDataRowLoadable
    {
        /// <summary>
        /// Загружает данные модели из строки табицы
        /// </summary>
        /// <param name="row"></param>
        void LoadFromDataRow(DataRow row);
    }
}

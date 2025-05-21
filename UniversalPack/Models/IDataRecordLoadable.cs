using System.Data;

namespace UniversalPack.Models
{
    /// <summary>
    /// Модель поддерживает загрузку данных из <see cref="IDataRecord"/>
    /// </summary>
    public interface IDataRecordLoadable
    {
        /// <summary>
        /// Заполняет модель данными из <see cref="IDataRecord"/>
        /// </summary>
        /// <param name="record">Объект строки данных</param>
        void LoadFromDataRecord(IDataRecord record);
    }
}

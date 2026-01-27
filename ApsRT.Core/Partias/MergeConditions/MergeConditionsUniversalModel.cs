using ApsRT.Core.Props;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using static UniversalPack.Attributes.DataRecordColumnAttribute;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Partias.MergeConditions
{
    /// <summary>
    /// Модель параметров объединения
    /// </summary>
    public class MergeConditionsUniversalModel
    {
        public MergeConditionsUniversalModel(Uco OperationType, Uco Subdivision)
        {
            this.Subdivision = Subdivision;
            this.OperationType = OperationType;
        }

        public MergeConditionsUniversalModel(MergeConditionsMatrixDataLoaderModel baseModel)
        {
            Subdivision = baseModel.Subdivision;
            OperationType = baseModel.OperationType;
            CheckMaterials = baseModel.CheckMaterials;
            DoNotCheckWindings = baseModel.DoNotCheckWindings;
            CheckWorkCenterRoute = baseModel.CheckWorkCenterRoute;
            PropsConditions = baseModel.PropsConditions;
        }

        /// <summary>
        /// Подразделение
        /// </summary>
        public Uco Subdivision { get; set; }

        /// <summary>
        /// GUID вида тех. операции
        /// </summary>
        public Uco OperationType { get; set; }

        /// <summary>
        /// Учитывать исходные комплектующие - проверять все материалы двух операций
        /// </summary>
        public bool CheckMaterials { get; set; }

        /// <summary>
        /// Не учитывать макс намотки
        /// </summary>
        public bool DoNotCheckWindings { get; set; }

        /// <summary>
        /// Проверять РЦ маршрута
        /// </summary>
        public bool CheckWorkCenterRoute { get; set; }

        public List<PropsLinkerClass> PropsConditions { get; set; }
    }
}

using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Partias.Materials
{
    public class StageOriginalMaterial
    {
        internal Uco Nomenklature; // Номенклатура
        internal Uco Specification; // Спецификация
        internal Uco Characteristics; // Характеристика
        internal double Count; // Количество
        internal DimensionUnit DataFormat; // ЕдиницаИзмерения
        internal string WorkType; // ВидВоспроизводства

        public override string ToString()
        {
            return $"[{Specification}] [{Nomenklature}] (хар. {Characteristics}, вид воспр. {WorkType}) (кол-во {Count} в {DataFormat})";
        }

        internal bool IsEqualTo(StageOriginalMaterial other)
        {

            return
                //Material == other.Material
                //&& Code == other.Code
                Nomenklature == other.Nomenklature
                && Characteristics == other.Characteristics
            //&& WorkType == other.WorkType
            //&& Count == other.Count
            //&& DataFormat == other.DataFormat
                ;
        }

        public StageOriginalMaterial(MaterialDataLoaderModel model)
        {
            Nomenklature = model.Nomenklature;
            Specification = model.Specification;
            Characteristics = model.Characteristics;
            Count = model.Count;
            DataFormat = model.Unit;
            WorkType = model.WorkType;
        }
    }
}

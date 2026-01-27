using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using MaterialUco = (UniversalPack.Common.CodedObject<System.Guid, string> Nomenklature, UniversalPack.Common.CodedObject<System.Guid, string> Characteristics);
using LeftoverUco = (UniversalPack.Common.CodedObject<System.Guid, string> Nomenklature, UniversalPack.Common.CodedObject<System.Guid, string> Characteristics, UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string> Seria);

namespace ApsRT.Core.Materials
{
    public class StageOriginalMaterial
    {
        public Uco Nomenklature; // Номенклатура
        public Uco Specification; // Спецификация
        public Uco Characteristics; // Характеристика
        public double Count; // Количество
        public DimensionUnit DataFormat; // ЕдиницаИзмерения
        //public string WorkType; // ВидВоспроизводства
        public bool IsBuying;


        #region Реквизиты
        public float?[] NumberProps { get; set; }
        public Guid?[][] GuidProps { get; set; }
        public string[] StringProps { get; set; }
        #endregion



        public MaterialUco AsMaterialUcoData() => (Nomenklature, Characteristics);

        public LeftoverUco AsLeftoverUcoData() => (Nomenklature, Characteristics, UniversalPack.Common.CodedObjectVersioned<Guid, string, string>.EmptyObject);

        public override string ToString()
        {
            return $"[{Specification}] [{Nomenklature}] (хар. {Characteristics}, покупка: {IsBuying}) (кол-во {Count} в {DataFormat})";
        }

        public bool IsEqualTo(StageOriginalMaterial other)
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
            IsBuying = model.IsBuying || model.WorkType == "Покупка";
        }

        public StageOriginalMaterial(Uco nomenklature, Uco specification, Uco characteristics, double count, DimensionUnit dataFormat, bool isBuying)
        {
            Nomenklature = nomenklature;
            Specification = specification;
            Characteristics = characteristics;
            Count = count;
            DataFormat = dataFormat;
            IsBuying = isBuying;
        }
    }
}

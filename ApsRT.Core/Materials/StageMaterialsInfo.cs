using ApsRT.Core.Leftovers;
using ApsRT.Core.Stages;

namespace ApsRT.Core.Materials
{
    public class StageMaterialsInfo
    {
        public Stage StageLink { get; }

        public StageMaterialsInfo(Stage stageLink)
        {
            StageLink = stageLink;
        }

        public List<StageOriginalMaterial> UsedMaterials { get; set; } = new();
        public List<StageLeftover> UsedLeftovers { get; set; } = new();

        public override string ToString()
        {
            return $"Исх. комплектующие: {string.Join(",\r\n", UsedMaterials.Select(x => x.ToString()))}";
        }

        public string AsLongString() => string.Join(";", UsedMaterials/*.Where(x => x.WorkType == "Производство")*/.Select(x => x.Nomenklature).OrderBy(x => x.Code));
        //public IEnumerable<string> AsShortStrings() => UsedMaterials.Select(x => x.Nomenklature).OrderBy(x => x);

        public bool IsEqualTo(StageMaterialsInfo other)
        {
            if (other.UsedMaterials.Count != UsedMaterials.Count)
            {
                return false;
            }
            for (int i = 0; i < UsedMaterials.Count; i++)
            {
                if (UsedLeftovers[i] != null || other.UsedLeftovers[i] != null)
                {
                    if (UsedLeftovers[i] == null && other.UsedLeftovers[i] != null) return false;
                    if (UsedLeftovers[i] != null && other.UsedLeftovers[i] == null) return false;
                    if (UsedLeftovers[i].LeftoverData != other.UsedLeftovers[i].LeftoverData) return false;

                    // Количество мы НЕ чекаем получается, оно было чекнуто ещё в самом начале OR-Toolовским алгоритмом

                }
                if (
                    //!UsedMaterials[i].IsBuying && !other.UsedMaterials[i].IsBuying &&
                    !UsedMaterials[i].IsEqualTo(other.UsedMaterials[i])) return false;
            }
            return true;
            //return UsedMaterials.All(x => other.UsedMaterials.Any(y => y.IsEqualTo(x)));
        }
    }
}

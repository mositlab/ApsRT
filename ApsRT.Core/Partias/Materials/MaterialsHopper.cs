using ApsRT.Core.Hoppers;
using ApsRT.Core.Partias.Leftovers;
using ApsRT.Core.Stages.Hoppers;
using ApsRT.Core.Stages.MasterData;
using System;
using System.Data;
using UniversalPack;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Partias.Materials
{
    [Hopper(Label = "Материалы", IsDirectory = false, IsMultiple = false, UsesGuid = true,
        NeededHoppers = new[] { nameof(StagesHopper)/* "Opers"*/ })]
    public class MaterialsHopper : IHopperInterface
    {

        StagesMasterData StagesMasterData;

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            StagesMasterData = session.MasterDataLinks.StagesMasterData;
        }

        void IHopperInterface.BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        void IHopperInterface.OnLoading(int RowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new MaterialDataLoaderModel();

            model.LoadFromDataRecord(data, options);

            //ИдентификаторОперации Номенклатура    Характеристика  Количество  ЕдиницаИзмерения    ВидВоспроизводства  Спецификация
            if (StagesMasterData.GUIDToIndex.TryGetValue(model.StageGuid, out var id))
            {
                var op = StagesMasterData.AllStages[id];
                StageMaterialsInfo mat;

                if (op.StageMaterialsInfo == null)
                {
                    mat = new StageMaterialsInfo(op);
                    op.StageMaterialsInfo = mat;
                }
                else
                {
                    mat = op.StageMaterialsInfo;
                }

                // Добавляем инфы
                mat.UsedMaterials.Add(new StageOriginalMaterial(model));
            }
        }

        void IHopperInterface.ActionFile(string filename)
        {

        }

        void IHopperInterface.AfterLoading()
        {
            foreach (var op in StagesMasterData.AllStages)
            {
                op.Value.StageMaterialsInfo ??= new(op.Value);
                op.Value.StageMaterialsInfo.UsedMaterials = op.Value.StageMaterialsInfo.UsedMaterials
                    .OrderBy(mat => mat.Nomenklature.Code)
                    .ThenBy(mat => mat.Specification.Code)
                    .ThenBy(mat => mat.Characteristics.Code)
                    .ThenBy(mat => mat.IsBuying)
                    .ToList();

                op.Value.StageMaterialsInfo.UsedLeftovers = op.Value.StageMaterialsInfo.UsedMaterials.Select(mat => (StageLeftover)null).ToList();
            }
        }
    }

}

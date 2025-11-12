using ApsRT.Core.Hoppers;
using ApsRT.Core.Leftovers;
using ApsRT.Core.Materials;
using ApsRT.Core.Props;
using ApsRT.Core.Stages;
using ApsRT.Core.Stages.Hoppers;
using ApsRT.Core.Stages.MasterData;
using System;
using System.Data;
using UniversalPack;
using UniversalPack.Attributes;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UniversalPack.Extensions;

namespace ApsRT.Core.Materials
{
    [Hopper(Label = "Материалы", IsDirectory = false, IsMultiple = false, UsesGuid = true, SessionPathAvailable = true, MustBeFilledWithData = true,
        NeededHoppers = new[] { nameof(StagesHopper)/* "Opers"*/ })]
    public class MaterialsHopper : IHopperInterface
    {

        StagesMasterData StagesMasterData;
        PropsMasterData PropsMasterData;

        void IHopperInterface.BeforeAllLoading(Session.Session session, string[] filenames)
        {
            StagesMasterData = session.MasterDataLinks.StagesMasterData;
            PropsMasterData = session.MasterDataLinks.PropsMasterData;
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
                var matElement = new StageOriginalMaterial(model);

                #region Реквизиты, или ПРОПСЫ
                matElement.NumberProps = new float?[PropsMasterData.MatsPropNumberToIndex.Count];
                foreach (var prop in PropsMasterData.MatsPropNumberToIndex) matElement.NumberProps[prop.Value] = data[prop.Key].ConvertToFloatByAnyCost();

                matElement.GuidProps = new Guid?[PropsMasterData.MatsPropGuidToIndex.Count][];
                foreach (var prop in PropsMasterData.MatsPropGuidToIndex)
                {
                    var str = data[prop.Key] == null ? "" : data[prop.Key].ToString();
                    matElement.GuidProps[prop.Value] = string.IsNullOrEmpty(str) ? new Guid?[] {null} : str.Split(',').Select(x => (Guid?)x.ProcessGuid(options.GuidArrrangeType, true)).ToArray();
                }

                matElement.StringProps = new string[PropsMasterData.MatsPropStringToIndex.Count];
                foreach (var prop in PropsMasterData.MatsPropStringToIndex) matElement.StringProps[prop.Value] = data[prop.Key].ConvertToOrDefault(string.Empty);
                #endregion

                mat.UsedMaterials.Add(matElement);
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

                // Группируем, удаля дубликаты, и просто сумируя их
                var groups = op.Value.StageMaterialsInfo.UsedMaterials.GroupBy(mat => (mat.Nomenklature.Code, mat.Characteristics.Code/*, mat.Specification.Code*/, mat.IsBuying));


                op.Value.StageMaterialsInfo.UsedMaterials = groups.Select(group =>
                {
                    var first = group.First();
                    first.Count = group.Sum(x => x.Count);
                    return first;//new StageOriginalMaterial();
                }).ToList();

                op.Value.StageMaterialsInfo.UsedMaterials = op.Value.StageMaterialsInfo.UsedMaterials
                    .OrderBy(mat => mat.Nomenklature.Code)
                    .ThenBy(mat => mat.Characteristics.Code)
                    .ThenBy(mat => mat.Specification.Code)
                    .ThenBy(mat => mat.IsBuying)
                    .ToList();

                op.Value.StageMaterialsInfo.UsedLeftovers = op.Value.StageMaterialsInfo.UsedMaterials.Select(mat => (StageLeftover)null).ToList();
            }
        }
    }

}

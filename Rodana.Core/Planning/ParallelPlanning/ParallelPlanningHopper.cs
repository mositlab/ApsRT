using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Rodana.Core.Centers.Hoppers;
using Rodana.Core.Centers.MasterData;
using Rodana.Core.Hoppers;
using Rodana.Core.Props;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.DataLoader;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace Rodana.Core.Planning.ParallelPlanning
{
    [Hopper(Label = "Варианты загрузки пар. РЦ", LoadWhenOnlyPart = false, IsDirectory = false, IsMultiple = false, UsesGuid = false,
        NeededHoppers = new[] { nameof(CentersParamsHopper) })]
    public class ParallelPlanningHopper : IHopperInterface
    {
        ParallelPlanningMasterData ParallelPlanningMasterData;
        CentersMasterData CentersMasterData;
        PropsMasterData PropsMasterData;

        public void ActionFile(string filename)
        {

        }

        public void AfterLoading()
        {

        }

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            ParallelPlanningMasterData = session.MasterDataLinks.ParallelPlanningMasterData;
            CentersMasterData = session.MasterDataLinks.CentersMasterData;
            PropsMasterData = session.MasterDataLinks.PropsMasterData;
            ParallelPlanningMasterData.ClearAll();
        }

        public void BeforeLoading(string filename, DataLoaderOptions options)
        {

        }

        public void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new ParallelPlanningDataLoaderModel();
            model.LoadFromDataRecord(data, options);

            // 1. Сначала РЦ. Если он не задан, значит это глюк таблицы - пустая строка.
            if (!model.CenterName.IsEmpty)
            {
                if (!ParallelPlanningMasterData.ParallelLoadingsVariants.ContainsKey(model.CenterName))
                {
                    ParallelPlanningMasterData.ParallelLoadingsVariants.Add(model.CenterName, new Dictionary<Uco, Dictionary<int, Dictionary<Uco, ParallelPlanningTaraInfo>>>());
                }

                // 1.1 - добавляем флаг возможности параллелизма в характеристики РЦ
                if (CentersMasterData.CentersData.TryGetValue(model.CenterName, out var cls))
                {
                    cls.CanBeParallel = true;
                }

                // 2. Вид операции
                if (!ParallelPlanningMasterData.ParallelLoadingsVariants[model.CenterName].ContainsKey(model.OperationVid))
                {
                    ParallelPlanningMasterData.ParallelLoadingsVariants[model.CenterName].Add(model.OperationVid, new Dictionary<int, Dictionary<Uco, ParallelPlanningTaraInfo>>());
                }

                // 3. Идентификатор параллельной загрузки
                if (!ParallelPlanningMasterData.ParallelLoadingsVariants[model.CenterName][model.OperationVid].ContainsKey(model.ParallelID))
                {
                    ParallelPlanningMasterData.ParallelLoadingsVariants[model.CenterName][model.OperationVid].Add(model.ParallelID, new Dictionary<Uco, ParallelPlanningTaraInfo>());
                }

                // 3.1 Дельта и "Не Повышать Тару"
                if (!ParallelPlanningMasterData.ParIDParams.ContainsKey(model.ParallelID))
                {
                    ParallelPlanningMasterData.ParIDParams.Add(model.ParallelID, new ParallelPlanningParamClass(model.Delta, model.NotIncreaseTara));
                }

                // 4. Вариант загрузки
                if (!ParallelPlanningMasterData.ParallelLoadingsVariants[model.CenterName][model.OperationVid][model.ParallelID].ContainsKey(model.Variant))
                {
                    var taraInfo = new ParallelPlanningTaraInfo();

                    //if (Settings.LoadedData.UsePseudoTara)
                    //{
                    //    taraInfo.List.Add(TaraDataSet.Pseudotara1, 1);
                    //    taraInfo.List.Add(TaraDataSet.Pseudotara2, 1);
                    //    taraInfo.List.Add(TaraDataSet.Pseudotara3, 1);
                    //    taraInfo.TotalCount += 3;
                    //}

                    ParallelPlanningMasterData.ParallelLoadingsVariants[model.CenterName][model.OperationVid][model.ParallelID].Add(model.Variant, taraInfo);
                }

                // 5. Тара и её количество для варианта загрузки
                var vari = ParallelPlanningMasterData.ParallelLoadingsVariants[model.CenterName][model.OperationVid][model.ParallelID][model.Variant];
                if (!vari.List.ContainsKey(model.Tara))
                {
                    vari.List.Add(model.Tara, model.TaraCount);
                    vari.TotalCount += model.TaraCount;
                    vari.MaxCapacity = model.MaxCapacity;
                }
                ParallelPlanningMasterData.ParallelLoadingsVariants[model.CenterName][model.OperationVid][model.ParallelID][model.Variant] = vari;

                // 6. Теперь набор характеристик
                var nabor = model.CharsList;
                var rc = model.CenterName;
                var operVid = model.OperationVid;
                if (!ParallelPlanningMasterData.GuidToCharactersList.ContainsKey(nabor)) // Если характеристики такой нет, добавляем
                {
                    var chr = new CharacteristicsClass(nabor);
                    ParallelPlanningMasterData.GuidToCharactersList.Add(nabor, chr);

                }

                if (!ParallelPlanningMasterData.CharacteristicsToParID.ContainsKey(rc))
                {
                    ParallelPlanningMasterData.CharacteristicsToParID.Add(rc, new());
                }

                if (!ParallelPlanningMasterData.CharacteristicsToParID[rc].ContainsKey(operVid))
                {
                    ParallelPlanningMasterData.CharacteristicsToParID[rc].Add(operVid, new());
                }

                var characters = ParallelPlanningMasterData.GuidToCharactersList[nabor]; // Получаем эту характеристику в любом случае - её надо заполнить

                ParallelPlanningMasterData.CharacteristicsToParID[rc][operVid].TryAdd(characters, model.ParallelID); // Добавляем и в этот словарь - его там точно не было.

                // 7. В характеристику пихаем конкретную характеристику

                // 7.1 Оставшиеся столбцы - для характеристики
                if (!characters.ListOfChars.ContainsKey(model.Char))
                {
                    characters.ListOfChars.Add(model.Char, new ConcreteCharacteristic(model.Min, model.Max, PropsMasterData.GenerateLinks(model.ColumnName), model.Value, model.ValueGUID)); // Добавляем
                }
            }
        }
    }
}

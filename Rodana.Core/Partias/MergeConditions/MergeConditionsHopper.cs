using Microsoft.Data.SqlClient;
using Rodana.Core.Hoppers;
using Rodana.Core.Props;
using System.Data;
using UniversalPack.Common.DataLoader;
using UniversalPack.Extensions;

namespace Rodana.Core.Partias.MergeConditions
{
    /// <summary>
    /// Загрузчик условий объединения
    /// </summary>
    [Hopper(Label = "Усл. объединения", IsDirectory = false, IsMultiple = false, UsesGuid = true, NeededHoppers = new string[] {nameof(MergeConditionsListHopper) })]
    internal sealed class MergeConditionsHopper : IHopperInterface
    {

        MergeConditionsMasterData MergeConditionsMasterData;

        public void ActionFile(string filename) { }

        public void AfterLoading()
        {
            MergeConditionsMasterData.MakeIndexes();
        }

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            MergeConditionsMasterData = session.MasterDataLinks.MergeConditionsMasterData;
            MergeConditionsMasterData.ClearMergeConditions();
        }

        public void BeforeLoading(string filename, DataLoaderOptions options) { }

        public void OnLoading(int rowIndex, IDataReader data, DataLoaderOptions options)
        {
            var model = new MergeConditionsBaseModel();
            model.LoadFromDataRecord(data, options);

            model.PropsConditions = new List<PropsLinkerClass>();
            foreach (var prop in MergeConditionsMasterData.MergeConditionsGlobalList)
            {
                if (data[prop.Key].ConvertToOrDefault(string.Empty).IsYes()) model.PropsConditions.Add(prop.Value); // Добавляем проп который надо сравнить
            }

            if (model.PropsConditions.Count == 0) model.PropsConditions = null;

            // дополнительные проверки

            MergeConditionsMasterData.MergeConditions.Add((model.OperationType.Code, model.Subdivision.Code), model);
        }
    }
}

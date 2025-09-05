using Microsoft.Data.SqlClient;
using ApsRT.Core.Hoppers;
using ApsRT.Core.Props;
using System.Data;
using UniversalPack.Common.DataLoader;
using UniversalPack.Extensions;
using ApsRT.Core.Stages.Hoppers;

namespace ApsRT.Core.Partias.MergeConditions
{
    /// <summary>
    /// Загрузчик условий объединения
    /// </summary>
    [Hopper(Label = "Усл. объединения", IsDirectory = false, IsMultiple = false, UsesGuid = false, NeededHoppers = new string[] {nameof(MergeConditionsListHopper), nameof(StagesHopper) })]
    internal sealed class MergeConditionsHopper : IHopperInterface
    {
        Session.Session session;
        MergeConditionsMasterData MergeConditionsMasterData;

        public void ActionFile(string filename) { }

        public void AfterLoading()
        {
            MergeConditionsMasterData.MakeIndexes();
            MergeConditionsMasterData.Prepare(session);
        }

        public void BeforeAllLoading(Session.Session session, string[] filenames)
        {
            this.session = session;
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

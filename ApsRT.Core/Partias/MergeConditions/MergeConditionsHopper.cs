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
    [Hopper(Label = "Усл. объединения", IsDirectory = false, IsMultiple = false, UsesGuid = false, MustBeFilledWithData = true, NeededHoppers = new string[] {nameof(MergeConditionsListHopper), nameof(StagesHopper) })]
    internal sealed class MergeConditionsHopper : IHopperInterface
    {
        Session.Session session;
        MergeConditionsMasterData MergeConditionsMasterData;

        public const string DoNotCheckWindingsConst = "НеУчитыватьМаксимальныеНамотки";
        public const string CheckMaterialsConst = "УчитыватьИсходныеКомплектующие";
        public const string CheckWorkCenterRouteConst = "УчитыватьПересеченияРЦмаршрута";

        public void ActionFile(string filename) { }

        public void AfterLoading()
        {
            MergeConditionsMasterData.MakeIndexes();
            //MergeConditionsMasterData.Prepare(session);
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
            if (session.SettingsController.CurrentSettings.UseMatrixMergeConditions)
            {
                var model = new MergeConditionsMatrixDataLoaderModel();
                model.LoadFromDataRecord(data, options);

                foreach (var prop in MergeConditionsMasterData.MergeConditionsGlobalList)
                {
                    if (model.PropsConditions == null) model.PropsConditions = new List<PropsLinkerClass>();
                    if (data[prop.Key].ConvertToOrDefault(string.Empty).IsYes()) model.PropsConditions.Add(prop.Value); // Добавляем проп который надо сравнить
                }

                // дополнительные проверки

                MergeConditionsMasterData.MergeConditionsOnLoad.Add((model.OperationType.Code, model.Subdivision.Code), new MergeConditionsUniversalModel(model));

            }
            else
            {
                var model = new MergeConditionsFlowedDataLoaderModel();
                model.LoadFromDataRecord(data, options);

                var key = (model.OperationType.Code, model.Subdivision.Code);
                if (!MergeConditionsMasterData.MergeConditionsOnLoad.ContainsKey(key)) MergeConditionsMasterData.MergeConditionsOnLoad.Add(key, new MergeConditionsUniversalModel(model.OperationType, model.Subdivision));

                var mergeCondClass = MergeConditionsMasterData.MergeConditionsOnLoad[key];

                if (MergeConditionsMasterData.MergeConditionsGlobalList.TryGetValue(model.Param, out var prop))
                {
                    if (mergeCondClass.PropsConditions == null) mergeCondClass.PropsConditions = new List<PropsLinkerClass>();
                    mergeCondClass.PropsConditions.Add(prop);
                }
                else
                {
                    switch (model.Param.Trim())
                    {
                        case DoNotCheckWindingsConst:
                            {
                                mergeCondClass.DoNotCheckWindings = true;
                                break;
                            }
                        case CheckMaterialsConst:
                            {
                                mergeCondClass.CheckMaterials = true;
                                break;
                            }
                        case CheckWorkCenterRouteConst:
                            {
                                mergeCondClass.CheckWorkCenterRoute = true;
                                break;
                            }
                        default:
                            {
                                Console.WriteLine($"unknown '{model.Param}'");
                                session.DataErrors.AddNewPartiaError(DataErrors.NewDataErrorType.UnknownPartiaParam, model.Param);
                                break;
                            }
                    }
                }
            }

        }
    }
}

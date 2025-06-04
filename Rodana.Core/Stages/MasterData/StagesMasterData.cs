using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Rodana.Core.Centers.MasterData;
using Rodana.Core.DataErrors;
using Rodana.Core.NaladLengths;
using Rodana.Core.Orders;
using Rodana.Core.Rechanges.MasterData;
using Rodana.Core.Rechanges;
using Rodana.Core.Session;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UniversalPack.Common.Drawing;
using System.Data;
using UniversalPack.Common.DataLoader;

namespace Rodana.Core.Stages.MasterData
{

    /// <summary>
    /// НСИ "Операции" (к формированию партий и планированию)
    /// </summary>
    public class StagesMasterData
    {
        public Dictionary<int, Stage> AllStages { get; private set; } = new();

        public Dictionary<Guid, int> GUIDToIndex { get; private set; } = new();

        public void ClearAll()
        {
            AllStages.Clear();
            GUIDToIndex.Clear();
            MesModellingsInfo.Clear();
        }

        public Dictionary<Guid, (Color Color, List<int> RelatedOpers)> MesModellingsInfo = new();

        // А это доп. дата, которая была введена для того, чтобы апс разграничила выполнение Fixed-операций и других
        // Благодаря этой сохраненной дате АПС даёт простою
        internal DateTime? FIXED_START_TIME { get; private set; }

        internal DateTime MIN_DATE_TIME_AFTER_FIX { get; set; } = DateTime.MaxValue; //  Это всегда было временем начала

        internal TimeSpan OneDay { get; private set; } = TimeSpan.FromDays(1);

        internal DateTime MinAcceptableDate { get; set; }// => FIXED_START_TIME.HasValue ? FIXED_START_TIME.Value : MIN_DATE_TIME_AFTER_FIX;

        internal DateTime? PrevTimeDelta { get; private set; }

        void ProcessTimeData(Stage stage)
        {
            var dataErrors = stage.SessionLink.DataErrors;

            #region Проверим время начала факт
            if (stage.StartTimeFact.HasValue && stage.Fixed && (!FIXED_START_TIME.HasValue || FIXED_START_TIME.Value > stage.StartTimeFact.Value))
            {
                if (stage.StartTimeFact.Value.Year < 2000)
                {
                    dataErrors.AddNewPartiaError(stage.Index, DataErrors.NewDataErrorType.BadStartTimeFact, new DataErrorStruct(stage.StartTimeFact.Value.ToString()));
                    //MainLoader.WannaExit(601); // Длительности < 0!
                }
                else
                {
                    FIXED_START_TIME = stage.StartTimeFact.Value;
                }
            }
            #endregion

            // А ещё предыдущую дату
            if (!PrevTimeDelta.HasValue || PrevTimeDelta.Value > stage.PrevDateTime) PrevTimeDelta = stage.PrevDateTime;

            // Теперь смотрим обычное время
            if (stage.DurationNorm < 0)
            {
                dataErrors.AddNewPartiaError(stage.Index, DataErrors.NewDataErrorType.NonPositiveLength, new DataErrorStruct(stage.DurationNorm.ToString()));
                //MainLoader.WannaExit(601); // Длительности < 0!
            }
        }

        void ProcessRCData(Stage stage)
        {
            var session = stage.SessionLink;
            var masterDatas = session.MasterDataLinks;
            var centersMasterData = masterDatas.CentersMasterData;
            #region Назначаем РЦ
            if (stage.RCRouteIsRC)
            {
                if (centersMasterData.CentersData.TryGetValue(stage.RCRoute, out var cls))
                {
                    stage.Center = stage.RCRoute;
                    stage.OriginalCenter = stage.Center;
                    stage.RCRoute = Uco.EmptyObject; // очищаем в таком случае
                }
                else
                {
                    session.AssignedLogger.LogQueue.Enqueue($"ЗАГРУЗКА (операции): Центр {stage.RCRoute} не существует.\r\n");
                }
            }
            else
            {
                stage.Center = Uco.EmptyObject;
                stage.OriginalCenter = Uco.EmptyObject;
                if (centersMasterData.RCRoutesToListDic.TryGetValue(stage.RCRoute, out var LIST))
                {
                    stage.PossibleCenters = LIST;
                    if (stage.Center.IsEmpty)
                    {
                        stage.Center = LIST[0];
                        stage.OriginalCenter = LIST[0];
                    }
                }
            }
            #endregion

            #region Параллельное планирование
            stage.ParallelPlanningIDs = new();

            if (stage.PossibleCenters != null)
            {
                foreach (var rc in stage.PossibleCenters)
                {
                    stage.ParallelPlanningIDs.Add(rc, ParallelPlanningHelper.GetParPlanID(rc, stage.OperVid, stage.RawData));
                }
            }
            else
            {
                stage.ParallelPlanningIDs.Add(stage.Center, ParallelPlanningHelper.GetParPlanID(stage.Center, stage.OperVid, stage.RawData));
            }
            #endregion
        }

        public void AddNewStage(Stage stage, OrderBaseModel orderModel, IDataReader data, DataLoaderOptions options)
        {
            var session = stage.SessionLink;
            var masterDatas = session.MasterDataLinks;
            #region Проверка на дубликат
            if (GUIDToIndex.ContainsKey(stage.GUID))
            {
                session.AssignedLogger.LogQueue.Enqueue($"{stage.GUID} уже есть в списке этапов!");
                return;
            }
            #endregion

            stage.Index = GUIDToIndex.Count;
            GUIDToIndex.Add(stage.GUID, stage.Index);

            ProcessRCData(stage);

            ProcessTimeData(stage);

            stage.OrderLink = masterDatas.OrdersMasterData.ProcessOrderData(stage, orderModel);

            if (stage.DurationNorm == 0)
            {
                // Инфы нет! Придётся делать по другому
            }
            else
            {
                stage.Duration = (int)(stage.DurationNorm * stage.Count); // Умножаем ДО количества мест, потому что нужно время ЭТАПА, а не одной КАТУШКИ.
                if (stage.Duration == 0) stage.Duration = 1; // Поставим 1 если внезапно
            }

            if (stage.PovivsMerged)
            {
                stage.Count /= stage.PlacesCount;
            }
            else
            {
                stage.PlacesCount = 1;
            }

            // Посмотрим на наладочную длину
            if (stage.NeededNaladLengthTableValue != -1) // решено, из-за работы экселя, сделать так, что -1 это пустота, а пустая клетка в экселе означает 0.
            {
                stage.NeededNaladLength = (new NaladLengthValue(stage.NeededNaladLengthTableValue, stage.EdIzmClassificator), stage.NeededNaladIsFirstInLaunch);
            }

            // Моделирование
            if (!stage.MesModelling.IsEmpty)
            {
                MesModellingsInfo.TryAdd(stage.MesModelling.Code, (Colors.NewRandomColor(), new List<int>()));
                MesModellingsInfo[stage.MesModelling.Code].RelatedOpers.Add(stage.Index);
            }

            // Общий этап
            if (stage.EasyFlagOriginal.HasValue)
            {
                stage.EasyFlagIsFromMes = true;
                stage.EasyFlag = stage.EasyFlagOriginal;
            }

            // Переналадки
            stage.RechangeParameters = new StageRechangeParam[masterDatas.RechangesMasterData.ExistingCriterias.Length];
            for (int i = 0; i < stage.RechangeParameters.Length; i++)
            {
                stage.RechangeParameters[i] = new StageRechangeParam(stage, masterDatas.RechangesMasterData.ExistingCriterias[i]);
                stage.RechangeParameters[i].SetData(data, options);
            }

            stage.Color = Colors.NewRandomColor();
        }
    }
}

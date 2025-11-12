using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using ApsRT.Core.Centers.MasterData;
using ApsRT.Core.DataErrors;
using ApsRT.Core.NaladLengths;
using ApsRT.Core.Orders;
using ApsRT.Core.Rechanges.MasterData;
using ApsRT.Core.Rechanges;
using ApsRT.Core.Session;
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
using Google.OrTools.ConstraintSolver;
using ApsRT.Core.Stages.Hoppers;
using System.Numerics;
using ApsRT.Core.Props;
using UniversalPack.Extensions;
using Azure;
using System.Collections;

namespace ApsRT.Core.Stages.MasterData
{

    /// <summary>
    /// НСИ "Операции" (к формированию партий и планированию)
    /// </summary>
    public class StagesMasterData
    {

        public Session.Session SessionLink { get; }

        public StagesMasterData(Session.Session session)
        {
            SessionLink = session;
        }


        public Dictionary<int, Stage> AllStages { get; private set; } = new();

        public Dictionary<Guid, int> GUIDToIndex { get; private set; } = new();

        public void ClearAll()
        {
            AllStages.Clear();
            GUIDToIndex.Clear();
            MesModellingsInfo.Clear();
            ApsModellingsInfo.Clear();
            //MIN_DATE_TIME_AFTER_FIX = DateTime.MaxValue;
            FIXED_START_TIME = null;
            PrevTimeDelta = null;
            //MinAcceptableDate = DateTime.Now;

            SavedOpersLivenCode.Clear();
        }

        public Dictionary<Guid, (Color Color, List<int> RelatedOpers)> MesModellingsInfo = new();

        public Dictionary<Guid, (Color Color, List<int> RelatedOpers)> ApsModellingsInfo = new();

        // А это доп. дата, которая была введена для того, чтобы апс разграничила выполнение Fixed-операций и других
        // Благодаря этой сохраненной дате АПС даёт простою
        public DateTime? FIXED_START_TIME { get; private set; }

        //public DateTime MIN_DATE_TIME_AFTER_FIX { get; set; } = DateTime.MaxValue; //  Это всегда было временем начала

        public TimeSpan OneDay { get; private set; } = TimeSpan.FromDays(1);

        public DateTime? PrevTimeDelta { get; private set; }

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
            if (SessionLink.SettingsController.CurrentSettings.Demos_AllPrevDatesAreNull) stage.PrevDateTime = null;
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
            var parrMasterData = masterDatas.ParallelPlanningMasterData;
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
                    if (stage.Center.EmptyCheck())
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
                    stage.ParallelPlanningIDs.Add(rc, parrMasterData.GetParPlanID(rc, stage.OperationVid, stage));
                }
            }
            else
            {
                stage.ParallelPlanningIDs.Add(stage.OriginalCenter, parrMasterData.GetParPlanID(stage.OriginalCenter, stage.OperationVid, stage));
            }
            #endregion
        }

        public void AddDependenciesConnection(DependencyDataLoaderModel model)
        {
            var guid = model.ThisGuid;
            var guidNext = model.NextGuid;
            if (guid != Guid.Empty && guidNext != Guid.Empty)
            {
                bool thisYes = model.ThisIsInPlan;
                bool nextYes = model.NextIsInPlan;
                int places = model.PlacesCount;

                if (GUIDToIndex.TryGetValue(guid, out var PrevID) &&
                    GUIDToIndex.TryGetValue(guidNext, out var ID) &&
                    AllStages.ContainsKey(PrevID) &&
                    AllStages.ContainsKey(ID))
                {
                    AllStages[ID].Prevs.Add(PrevID);

                    // Если этот предок не вспомогательный, то прибавляем счетчик норм детей
                    if (!AllStages[PrevID].IsVspom) AllStages[ID].PrevsNormCount++;

                    AllStages[PrevID].Nexts.Add(ID);
                }


                if (!thisYes && nextYes)
                {
                    if (GUIDToIndex.TryGetValue(guidNext, out var nID) && AllStages.ContainsKey(nID))
                    {
                        if (model.VspomTara)
                            AllStages[nID].GhostVspomPrevsPlacesCount += places;
                        else
                            AllStages[nID].GhostNormPrevsPlacesCount += places;
                    }
                }
            }
        }

        public void FixVPZDependencies()
        {

            Queue<(int id, bool vpz, bool fact)> toCheck = new();
            Queue<(int id, DateTime startTime)> toCheckFixed = new();

            foreach (var op in AllStages)
            {
                if (op.Value.Fixed) toCheckFixed.Enqueue((op.Key, op.Value.StartTimeFact.Value));
                if (!op.Value.Fixed && (op.Value.VPZ || op.Value.IsFact)) toCheck.Enqueue((op.Value.Index, op.Value.VPZ, op.Value.IsFact));
            }

            while (toCheck.Count > 0)
            {
                var el = toCheck.Dequeue();
                AllStages[el.id].VPZ |= el.vpz;
                AllStages[el.id].IsFact |= el.fact;
                foreach (var prev in AllStages[el.id].Prevs)
                {
                    if (!AllStages[prev].Fixed) toCheck.Enqueue((prev, el.vpz, el.fact));
                }
            }

            while (toCheckFixed.Count > 0)
            {
                var el = toCheckFixed.Dequeue();
                AllStages[el.id].Fixed |= true;
                AllStages[el.id].StartTimeFact ??= el.startTime;
                foreach (var prev in AllStages[el.id].Prevs)
                {
                    /*if (!AllStages[prev].Fixed)*/ toCheckFixed.Enqueue((prev, el.startTime));
                }
            }
        }

        public void AddNewStage(Stage stage, OrderBaseModel orderModel, IDataReader data, DataLoaderOptions options)
        {
            var masterDatas = SessionLink.MasterDataLinks;
            var PropsMasterData = masterDatas.PropsMasterData;
            #region Проверка на дубликат
            if (GUIDToIndex.ContainsKey(stage.GUID))
            {
                SessionLink.AssignedLogger.LogQueue.Enqueue($"{stage.GUID} уже есть в списке этапов!");
                return;
            }
            #endregion

            stage.Index = GUIDToIndex.Count;
            GUIDToIndex.Add(stage.GUID, stage.Index);
            AllStages.Add(stage.Index, stage);
            stage.Prevs = new();
            stage.Nexts = new();

            #region Реквизиты, или ПРОПСЫ

            stage.NumberProps = new float?[PropsMasterData.OpersPropNumberToIndex.Count];
            foreach (var prop in PropsMasterData.OpersPropNumberToIndex) stage.NumberProps[prop.Value] = data[prop.Key].ConvertToFloatByAnyCost();

            stage.GuidProps = new Guid?[PropsMasterData.OpersPropGuidToIndex.Count][];
            foreach (var prop in PropsMasterData.OpersPropGuidToIndex)
            {
                //stage.GuidProps[prop.Value] = data[prop.Key].ProcessGuid(options.GuidArrrangeType, true);

                var str = data[prop.Key] == null ? "" : data[prop.Key].ToString();
                stage.GuidProps[prop.Value] = string.IsNullOrEmpty(str) ? new Guid?[] { null } : str.Split(',').Select(x => (Guid?)x.ProcessGuid(options.GuidArrrangeType, true)).ToArray();
            }

            stage.StringProps = new string[PropsMasterData.OpersPropStringToIndex.Count];
            foreach (var prop in PropsMasterData.OpersPropStringToIndex) stage.StringProps[prop.Value] = data[prop.Key].ConvertToOrDefault(string.Empty);
            #endregion

            if (SessionLink.SettingsController.CurrentSettings.Demos_NoOneRC)
            {
                stage.OnOneRC = false;
            }

            ProcessRCData(stage);

            ProcessTimeData(stage);

            if (SessionLink.SettingsController.CurrentSettings.Demos_AllPartiasAreFree)
            {
                stage.Fixed = stage.IsFact = stage.IsUserHandled = stage.VPZ = false;
                orderModel.PartiaPF = null;
            }

            stage.OrderLink = masterDatas.OrdersMasterData.ProcessOrderData(stage, orderModel);

            if (SessionLink.SettingsController.CurrentSettings.Demos_AllDurationsAreOne)
            {
                stage.DurationNorm = 1;
            }

            if (stage.DurationNorm == 0)
            {
                // Инфы нет! Придётся делать по другому
                stage.SetStaticDuration(null, stage.Count);
            }
            else
            {
                // Умножаем ДО количества мест, потому что нужно время ЭТАПА, а не одной КАТУШКИ.
                stage.SetStaticDuration(stage.DurationNorm, stage.Count);
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
            if (SessionLink.SettingsController.CurrentSettings.Demos_AllNaladLengthsAreClear)
            {
                stage.NeededNaladLengthTableValue = -1;
                stage.NeededNaladIsFirstInLaunch = false;
                stage.NeededNaladLength = null;
            }
            if (stage.NeededNaladLengthTableValue != -1) // решено, из-за работы экселя, сделать так, что -1 это пустота, а пустая клетка в экселе означает 0.
            {
                stage.NeededNaladLength = (new NaladLengthValue(stage.NeededNaladLengthTableValue, stage.EdIzmClassificator), stage.NeededNaladIsFirstInLaunch);
            }

            // Моделирование
            if (SessionLink.SettingsController.CurrentSettings.Demos_AllPartiasAreFree)
            {
                stage.MesModelling = Uco.EmptyObject;
                stage.EasyFlagOriginal = Guid.Empty;
            }

            if (!stage.MesModelling.IsEmpty)
            {
                MesModellingsInfo.TryAdd(stage.MesModelling.Code, (Colors.NewRandomColor(), new List<int>()));
                MesModellingsInfo[stage.MesModelling.Code].RelatedOpers.Add(stage.Index);
            }

            // Общий этап
            if (stage.EasyFlagOriginal == Guid.Empty) stage.EasyFlagOriginal = null;
            if (stage.EasyFlagOriginal.HasValue)
            {
                //stage.EasyFlagIsFromMes = true;
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


        public int LivensteinRechangeParam = 0; // В ЗМК-версии это напряжение
        public Dictionary<int, string> SavedOpersLivenCode = new();
        public void SetLivenCodes(string rawCriteriaName)
        {
            LivensteinRechangeParam = SessionLink.MasterDataLinks.RechangesMasterData.GetCriteriaIndexByName(rawCriteriaName);

            LevenshteinDistanceCashChecked = new System.Collections.BitArray[AllStages.Count];
            for (var i = 0; i < AllStages.Count; i++)
            {
                LevenshteinDistanceCashChecked[i] = new System.Collections.BitArray(AllStages.Count);
            }

            LevenshteinDistanceCash = new byte[AllStages.Count][];
            for (var i = 0; i < AllStages.Count; i++)
            {
                LevenshteinDistanceCash[i] = new byte[AllStages.Count];
            }

            SavedOpersLivenCode = new(AllStages.Count);


            var keys = AllStages.Where(x => x.Value.Prevs.Count > 0 && x.Value.Nexts.Count == 0);

            Dictionary<string, int> locConv = new(50);
            var curr = 0;

            foreach (var key in keys)
            {
                if (SavedOpersLivenCode.ContainsKey(key.Key))
                {
                    continue;
                }

                var code = "";
                var op = AllStages[key.Key];
                goPrev(op, code);
            }

            void goPrev(Stage oper, string currCode)
            {
                var s = oper.Nomenklature + ";" + (LivensteinRechangeParam == -1 ? "" : oper.RechangeParameters[LivensteinRechangeParam].GetString(0)) + ";" + (oper.StageMaterialsInfo?.AsLongString() ?? "");//oper.MaterialLink?.AsLongString() ?? "";

                if (!locConv.ContainsKey(s))
                {
                    locConv.Add(s, curr);
                    curr++;
                }
                currCode += (char)locConv[s];//Merger.SYMBOLS[locConv[s]];
                if (SavedOpersLivenCode.ContainsKey(oper.Index))
                {
                    SessionLink.DataErrors.AddNewPartiaError(
                        oper.Index
                        , NewDataErrorType.LivenshteinBadOper, new DataErrorStruct());
                }
                else
                {
                    SavedOpersLivenCode.Add(oper.Index, currCode);
                    foreach (var prev in oper.Prevs)
                    {
                        goPrev(AllStages[prev], currCode);
                    }
                }
            }
        }

        static int Minimum(int a, int b, int c) => (a = a < b ? a : b) < c ? a : c;
        public byte[][] LevenshteinDistanceCash;
        public BitArray[] LevenshteinDistanceCashChecked;
        public byte LevenshteinDistance(int index1, int index2, int id1, int id2, bool WithUI)
        {
            if (LevenshteinDistanceCashChecked[index1][index2])
            {
                return LevenshteinDistanceCash[index1][index2];
            }

            if (LevenshteinDistanceCashChecked[index2][index1])
            {
                return LevenshteinDistanceCash[index2][index1];
            }

            if (SavedOpersLivenCode.TryGetValue(id1, out var code1))
            {
                if (SavedOpersLivenCode.TryGetValue(id2, out var code2))
                {
                    var res = (byte)LevenshteinDistanceMain(code1, code2);
                    LevenshteinDistanceCashChecked[index1][index2] = true;
                    LevenshteinDistanceCashChecked[index2][index1] = true;
                    LevenshteinDistanceCash[index1][index2] = res;
                    LevenshteinDistanceCash[index2][index1] = res;
                    return res;
                }
                if (WithUI)
                {
                    SessionLink.AssignedLogger.PartiasLogQueue.Enqueue($"{AllStages[id2]} не содержит Кода Ливенштейна.\r\n");
                }

                return 255;
            }
            if (WithUI)
            {
                SessionLink.AssignedLogger.PartiasLogQueue.Enqueue($"{AllStages[id1]} не содержит Кода Ливенштейна.\r\n");
            }

            return 255;
        }
        public static byte LevenshteinDistanceMain(string firstWord, string secondWord)
        {

            var n = firstWord.Length + 1;
            var m = secondWord.Length + 1;
            var matrixD = new int[n, m];
            const int deletionCost = 1;
            const int insertionCost = 1;
            for (var i = 0; i < n; i++)
            {
                matrixD[i, 0] = i;
            }

            for (var j = 0; j < m; j++)
            {
                matrixD[0, j] = j;
            }

            for (var i = 1; i < n; i++)
            {
                for (var j = 1; j < m; j++)
                {
                    var substitutionCost = firstWord[i - 1] == secondWord[j - 1] ? 0 : 1;
                    matrixD[i, j] = Minimum(matrixD[i - 1, j] + deletionCost,          // удаление
                                            matrixD[i, j - 1] + insertionCost,         // вставка
                                            matrixD[i - 1, j - 1] + substitutionCost); // замена
                }
            }
            return (byte)(matrixD[n - 1, m - 1]);
        }

    }
}

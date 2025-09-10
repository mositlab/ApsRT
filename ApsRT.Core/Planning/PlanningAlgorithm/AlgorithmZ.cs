using ApsRT.Core.PlanSimulator;
using ApsRT.Core.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Planning.PlanningAlgorithm
{
    internal class AlgorithmZ
    {
        public PlanningAlgorithm basisLink;

        public AlgorithmZ(PlanningAlgorithm basisLink)
        {
            this.basisLink = basisLink;
        }

        public void Start(PlanningAlgorithmWorkflow workflow)
        {
            // Выбираем те операции, у которых нет предков. Ну или последнюю, которую можно юзать
            int[] possibleFirstIndexes;

            // Сет операций, которые ВПЗ, и которые надо выполнить ПОСЛЕ фиксированных, и ДО остальных
            //HashSet<int> VPZOpersOpenedList = basisLink.BestBlocksList.Where(x => x.Lists[0].OpersList[0].VPZ).Select(x => x.Lists[0].OpersList[0].ID).ToHashSet();

            var OpersCopy = basisLink.UsedPool.CopyBlocksDeep(basisLink, basisLink.UsedPool.BestPoolIndex, true);
            var poolCopy = new PlanSimulatorPool(basisLink.SessionLink, OpersCopy, basisLink.MinAcceptableDate, basisLink.UsedPool.PoolSize);

            basisLink.CurrentStageChanged?.Invoke(basisLink.CurrentStage, OpersCopy.Count);

            var centers_choosen = poolCopy.CentersData; // Каждый раз копию делать не нужно, потому что считаются все итерации друг за другом

            poolCopy.PrepareOnOneRCData();

            poolCopy.PrepareAllRCsOnBlocks();

            var OpersCopyEEL40 = OpersCopy.Where(x => x.Lists[0].FirstOperJust.Center.Label.Contains("EEL-40")).ToList();
            //var copyGlobal = PlanSimulatorPool.CopyBlocksSwallow(basisLink.UsedPool.Blocks);

            // Не имеет значения, по какому индексу моделирования искать операцию. Этот массив нужен только для последующего поиска и вставки.
            //if (basisLink.GLOBAL_FIXED_MIN == 0)
            //{
            //    possibleFirstIndexes = copyGlobal.Where(x => x.Lists[0].StagesAsNormal.All(y => y.Prevs.Count == 0)).Select((x, index) => x.Lists[0].StagesAsNormal[0].Index).ToArray();
            //}
            //else
            //{
            //    possibleFirstIndexes = new int[1] { basisLink.UsedPool.Blocks.First(x => x.Lists.All(y => y.PrevMods.Count == 0)).Lists[0].StagesAsNormal[0].Index }; // Ну она же всё равно первая
            //}

            int bestReches = int.MaxValue;

            // Пройдем по всем этапчикам
            for (int i = 0; i < OpersCopyEEL40.Count; i++)
            {

                if (basisLink.WannaStop)
                {
                    break;
                }

                var localCopy = PlanSimulatorPool.CopyBlocksSwallow(OpersCopyEEL40);

                List<PlanSimulatorBlock> finalList = new();

                finalList.Add(localCopy[i]);

                localCopy.RemoveAt(i);

                while (localCopy.Count > 0)
                {
                    var minIndex = -1;
                    var minValue = int.MaxValue;
                    for (int j = 0; j < localCopy.Count; j++)
                    {
                        var center = localCopy[j].GetCenterData(0);
                        var result = basisLink.SessionLink.MasterDataLinks.RechangesMasterData.FindRechanges(new Rechanges.RechangesEngine.RechangesEngineData(center.CenterDataLink, localCopy[j].Lists[0].MinDatesData[0], finalList[^1].Lists[0].FirstOperJust, localCopy[j].Lists[0].FirstOperJust, center.Center));
                        if (result.Duration < minValue)
                        {
                            minValue = result.Duration;
                            minIndex = j;
                        }
                    }
                    finalList.Add(localCopy[minIndex]);
                    localCopy.RemoveAt(minIndex);


                    if (basisLink.WannaStop)
                    {
                        break;
                    }
                }


                if (basisLink.WannaStop)
                {
                    break;
                }

                var listNext = finalList.SelectMany(x => x.Lists[0].Stages.SelectMany(x => x.NextStages)).Distinct().ToList();
                var listPrev = finalList.SelectMany(x => x.Lists[0].Stages.SelectMany(x => x.PreviousStages)).Distinct().ToList();

                if (true)
                {
                    Queue<PlanSimulatorStage> nextStagesToSet = new();
                    foreach (var listItem in listNext)
                        nextStagesToSet.Enqueue(listItem);

                    while (nextStagesToSet.Count > 0)
                    {
                        var nextStage = nextStagesToSet.Dequeue();
                        var block = nextStage.ModLink.BlockLink;
                        if (finalList.Contains(block))
                        {
                            ;
                        }
                        else
                        {
                            finalList.Add(block); // Добавляем в список
                        }
                        var nexts = nextStage.NextStages/*.Where(x => !finalList.Contains(x.ModLink.BlockLink))*/.ToList(); // Добавляем следующих
                        foreach (var listItem in nexts)
                            nextStagesToSet.Enqueue(listItem);
                    }
                }

                if (true)
                {
                    Queue<PlanSimulatorStage> nextStagesToSet = new();
                    foreach (var listItem in listPrev)
                        nextStagesToSet.Enqueue(listItem);

                    while (nextStagesToSet.Count > 0)
                    {
                        var nextStage = nextStagesToSet.Dequeue();
                        var block = nextStage.ModLink.BlockLink;
                        if (finalList.Contains(block))
                        {
                            ;
                        }
                        else
                        {
                            finalList.Insert(0, block); // Добавляем в список
                        }
                        var nexts = nextStage.PreviousStages/*.Where(x => !finalList.Contains(x.ModLink.BlockLink))*/.ToList(); // Добавляем следующих
                        foreach (var listItem in nexts)
                            nextStagesToSet.Enqueue(listItem);
                    }
                }

                if (true)
                {
                    var OpersCopyOther = OpersCopy.Where(x => !finalList.Contains(x)).ToList();
                    finalList.AddRange(OpersCopyOther);
                }

                poolCopy.Blocks = finalList;

                var resultBig = PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(finalList, 0, basisLink.MinAcceptableDate, null);

                if (bestReches > resultBig.RechangesSeconds.TotalSeconds)
                {
                    workflow.BestValue = resultBig;
                    basisLink.UsedPool = poolCopy;
                    basisLink.UpdateBcsSuccess(resultBig, finalList, 0);
                    bestReches = (int)resultBig.RechangesSeconds.TotalSeconds;
                }

                basisLink.AlgorithmCallback?.Invoke(basisLink.CurrentStage, i);

            }

        }
    }
}

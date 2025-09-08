using ApsRT.Core.PlanSimulator;
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


            var copyGlobal = PlanSimulatorPool.CopyBlocksSwallow(basisLink.UsedPool.Blocks).Where(x => x.Lists[0].FirstOperJust.Center.Label.Contains("EEL-40")).ToList();

            // Не имеет значения, по какому индексу моделирования искать операцию. Этот массив нужен только для последующего поиска и вставки.
            //if (basisLink.GLOBAL_FIXED_MIN == 0)
            //{
            //    possibleFirstIndexes = copyGlobal.Where(x => x.Lists[0].StagesAsNormal.All(y => y.Prevs.Count == 0)).Select((x, index) => x.Lists[0].StagesAsNormal[0].Index).ToArray();
            //}
            //else
            //{
            //    possibleFirstIndexes = new int[1] { basisLink.UsedPool.Blocks.First(x => x.Lists.All(y => y.PrevMods.Count == 0)).Lists[0].StagesAsNormal[0].Index }; // Ну она же всё равно первая
            //}

            // Пройдем по всем этапчикам
            for (int i = 0; i < copyGlobal.Count; i++)
            {
                var OpersCopy1 = PlanSimulatorPool.CopyBlocksSwallow(copyGlobal);
                var poolCopy = new PlanSimulatorPool(basisLink.SessionLink, OpersCopy1, basisLink.MinAcceptableDate, basisLink.UsedPool.PoolSize);

                var centers_choosen = poolCopy.CentersData; // Каждый раз копию делать не нужно, потому что считаются все итерации друг за другом

                poolCopy.PrepareOnOneRCData();

                poolCopy.PrepareAllRCsOnBlocks();

                List<PlanSimulatorBlock> finalList = new();

                finalList.Add(copyGlobal[i]);

                OpersCopy1.Remove(copyGlobal[i]);

                while (OpersCopy1.Count > 0)
                {
                    var minIndex = -1;
                    var minValue = int.MaxValue;
                    for (int j = 0; j < OpersCopy1.Count; j++)
                    {
                        var center = OpersCopy1[j].GetCenterData(0);
                        var result = basisLink.SessionLink.MasterDataLinks.RechangesMasterData.FindRechanges(new Rechanges.RechangesEngine.RechangesEngineData(center.CenterDataLink, OpersCopy1[j].Lists[0].MinDatesData[0], finalList[^1].Lists[0].FirstOperJust, OpersCopy1[j].Lists[0].FirstOperJust, center.Center));
                        if (result.Duration < minValue)
                        {
                            minValue = result.Duration;
                            minIndex = j;
                        }
                    }
                    finalList.Add(OpersCopy1[minIndex]);
                    OpersCopy1.RemoveAt(minIndex);
                }

                poolCopy.Blocks = finalList;

                var resultBig = PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(finalList, 0, basisLink.MinAcceptableDate, null);

                workflow.BestValue = resultBig;
                //workflow.LastRechangesForCombo = t.RechangesSeconds;
                //workflow.LastProductionForCombo = t.GlobalPlanEndDate - basisLink.MinAcceptableDate;
                //workflow.LastProstoiForCombo = t.ProstoiSeconds.TotalMinutes;
                //workflow.LastDaysForCombo = t.DaysRuinedValue;
                //workflow.LastBadOffsetsForCombo = t.BadOffsets;
                basisLink.UsedPool = poolCopy;
                //basisLink.UsedPool.indexes();
                basisLink.UpdateBcsSuccess(resultBig, finalList, 0);

            }

        }
    }
}

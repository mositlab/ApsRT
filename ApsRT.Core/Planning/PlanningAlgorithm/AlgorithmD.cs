using ApsRT.Core.PlanSimulator;
using ApsRT.Core.PlanSimulator.PlanSimulatorEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsRT.Core.Planning.PlanningAlgorithm
{
    internal class AlgorithmD
    {
        public PlanningAlgorithm basisLink;

        public AlgorithmD(PlanningAlgorithm basisLink)
        {
            this.basisLink = basisLink;
        }


        class DKoeff
        {
            public float RechangesK = 1;
        }

        class DNodeInfo
        {
            public DNode LastBestNode;
            public int IsPassed_Flag; // Если флаг равен текущему, то точка уже была пройдена, и её не следует рассматривать.
        }

        class DNode
        {
            public List<DNode> NeededParents { get; } = new();

            public int Index { get; }

            public PlanSimulatorMod ModLink { get; }

            public List<DNode> PossibleNeibhours { get; } = new();

            public DNodeInfo[] SolversData { get; }

            public DNode(int index, PlanSimulatorMod modLink, int solversCount)
            {
                Index = index;
                ModLink = modLink;
                SolversData = new DNodeInfo[solversCount];
                for (int i = 0; i < solversCount; i++) SolversData[i] = new DNodeInfo();
            }
        }

        class DGraph
        {

            Dictionary<PlanSimulatorMod, DNode> ModsToNodes;

            Dictionary<int, DNode> Nodes;

            DNode StartDummyNode, EndDummyNode;

            void AddNode(PlanSimulatorMod mod, int asIndex, int solversCount)
            {
                var newNode = new DNode(asIndex, mod, solversCount);
                Nodes.Add(asIndex, newNode);
                ModsToNodes.Add(mod, newNode);
                foreach (var node in Nodes)
                {
                    if (node.Value.Index < 0) continue; // это пустышка
                    var indexTo = node.Key;
                    if (asIndex == indexTo) continue; // В саму себя не идём
                    //    if (Matrix_Adjacency[asIndex, indexTo]) newNode.PossibleNeibhours.Add(node.Value);
                    //if (Matrix_Adjacency[indexTo, asIndex]) node.Value.PossibleNeibhours.Add(newNode);
                }
                newNode.PossibleNeibhours.Add(EndDummyNode);

                StartDummyNode.PossibleNeibhours.Add(newNode);
                EndDummyNode.NeededParents.Add(newNode); // Для завершения нужно больше теперь
            }

            public DGraph(int solversCount, List<PlanSimulatorMod> mods)
            {
                Nodes = new Dictionary<int, DNode>();
                ModsToNodes = new Dictionary<PlanSimulatorMod, DNode>();
                StartDummyNode = new DNode(-1, null, solversCount);
                Nodes.Add(-1, StartDummyNode);
                EndDummyNode = new DNode(-2, null, solversCount);
                Nodes.Add(-2, EndDummyNode);




            }
        
            struct DNodeStructData
            {
                public int PassedHere;
            }
            
            public List<PlanSimulatorBlock> Solve(int SolverIndex)
            {
                int currentFlag = Random.Shared.Next();
                Queue<(DNode from, DNode to, DNodeStructData data)> NodesQueue = new();
                NodesQueue.Enqueue((null, StartDummyNode, new DNodeStructData() { PassedHere = -1 }));

                //int totalPassed = 0;

                bool Success = false;

                while (NodesQueue.Count > 0)
                {
                    var currentNodeData = NodesQueue.Dequeue();
                    var lastNode = currentNodeData.from;
                    var currentNode = currentNodeData.to;
                    var data = currentNodeData.data;
                    var solverData = currentNode.SolversData[SolverIndex];
                    if (solverData.IsPassed_Flag == currentFlag && currentNode.SolversData[SolverIndex].LastBestNode != StartDummyNode) continue; // Точка уже пройдена, не трогаем.

                    if (data.PassedHere != -1 && data.PassedHere < currentNode.NeededParents.Count)
                        continue; // Для точки не пройдено достаточно, не трогаем.


                    solverData.LastBestNode = lastNode;
                    solverData.IsPassed_Flag = currentFlag; // Помечаем как пройденную
                    data.PassedHere++;


                    if (currentNode == EndDummyNode)
                    {
                        Success = true;
                        break;
                    }

                    //if (currentNode.Index >= 0) totalPassed++; // Если это не пустышка, помечаем, что какая-то точка пройдена

                    foreach (var nexts in currentNode.PossibleNeibhours) NodesQueue.Enqueue((currentNode, nexts, data));
                }

                if (Success)
                {
                    List<PlanSimulatorMod> mods = new();

                    Queue<DNode> nodes = new Queue<DNode>();
                    nodes.Enqueue(EndDummyNode);

                    while (nodes.Count > 0)
                    {
                        var current = nodes.Dequeue();
                        if (current.ModLink != null) mods.Add(current.ModLink);
                        var last = current.SolversData[SolverIndex].LastBestNode;
                        if (last != null) nodes.Enqueue(last);
                    }

                    mods.Reverse();

                    var blocks = mods.Select(mod => mod.BlockLink).ToList();
                    ;
                    return blocks;
                }
                else
                {
                    return null;
                }
            }
        }

        public void Start(PlanningAlgorithmWorkflow workflow)
        {
            var OpersCopy = basisLink.UsedPool.CopyBlocksDeep(basisLink, basisLink.UsedPool.BestPoolIndex, true);
            var poolCopy = new PlanSimulatorPool(basisLink.SessionLink, OpersCopy, basisLink.MinAcceptableDate, basisLink.UsedPool.PoolSize);

            //var centers_choosen = poolCopy.CentersData; // Каждый раз копию делать не нужно, потому что считаются все итерации друг за другом

            //poolCopy.PrepareOnOneRCData();

            //poolCopy.PrepareAllRCsOnBlocks();

            var OpersCopyEEL40 = OpersCopy.Where(x => x.Lists[0].FirstOperJust.Center.Label.Contains("EEL-40")).ToList();

            /// <summary>
            /// 1.1 Таблица смежности (куда можно переходить)
            /// </summary>
            bool[,] Matrix_Adjacency;

            /// <summary>
            /// 1.2 Таблица коэффициентов (данные по переходам)
            /// </summary>
            DKoeff[,] Matrix_DKoeffs;

            int opersCount = OpersCopyEEL40.Count;
            Matrix_Adjacency = new bool[opersCount, opersCount];
            Matrix_DKoeffs = new DKoeff[opersCount, opersCount];

            var dt0 = DateTime.Now;
            for (int i = 0; i < opersCount; i++)
            {
                for (int j = 0; j < opersCount; j++)
                {
                    Matrix_Adjacency[i, j] = true;
                    Matrix_DKoeffs[i, j] = new DKoeff();
                }
            }
            var dt1 = DateTime.Now;
            Console.WriteLine($"{dt1 - dt0} <=== matrix created");
            //for (int i = 0; i < opersCount; i++)
            //{
            //    AddNode(mods[i], i, solversCount);
            //}
            //var dt2 = DateTime.Now;
            //Console.WriteLine($"{dt2 - dt1} <=== nodes created");



            //var graph = new DGraph(16, OpersCopyEEL40.SelectMany(block => block.Lists).ToList());

            //var blocks = graph.Solve(0);

            //if (blocks != null)
            //{
            //    var result = PlanSimulatorEngine.CalculatePlan(blocks, 0, basisLink.StartOfPlanningDate, null);
            //}
            ;
            basisLink.Stop();
        }
    }
}

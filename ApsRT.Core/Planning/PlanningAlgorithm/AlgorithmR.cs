using ApsRT.Core.Leftovers;
using ApsRT.Core.PlanSimulator;
using ApsRT.Core.Rechanges.MasterData;
using Azure;
using Google.OrTools.Sat;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;

namespace ApsRT.Core.Planning.PlanningAlgorithm
{
    internal class AlgorithmR
    {

        public PlanningAlgorithm basisLink;

        public AlgorithmR(PlanningAlgorithm basisLink)
        {
            this.basisLink = basisLink;
        }


        List<PlanSimulatorBlock> OpersCopy;

        FrozenDictionary<Uco, List<PlanSimulatorBlock>> BlocksDependingOnCenter;

        List<PlanSimulatorBlock> LinearPlan = new();

        HashSet<PlanSimulatorBlock> ExistingBlocks = new();


        void AddBlockToPlan(PlanSimulatorBlock block, Uco center)
        {
            block.SetCenter(center, 0);
            BlocksDependingOnCenter[center].Add(block);
            LinearPlan.Add(block);
            ExistingBlocks.Add(block);
            OpersCopy.Remove(block);
        }

        void Step1_FirstStages()
        {
            Console.WriteLine("R: getting first ops...");
            var OpersForFirstLevelArray = OpersCopy.Where(block => block.Lists[0].FirstOperJust.Prevs.Count == 0).ToArray(); // Получаем те этапы, которые подходят именно в качестве первых.

            Dictionary<Uco, int> CenterToIndex = new();

            // Подготовим список кубиков и на какие РЦ они встанут
            List<(PlanSimulatorBlock block, Uco Center)> OpersForFirstLevel = new();
            for (int i = 0; i < OpersForFirstLevelArray.Length; i++)
            {
                var block = OpersForFirstLevelArray[i];
                foreach (var rc in block.PossibleRCsHere)
                {
                    OpersForFirstLevel.Add((block, rc));
                }
            }

            Console.WriteLine($"R: preparing solver for {OpersForFirstLevel.Count}...");

            double[] deltas = new double[OpersForFirstLevel.Count]; // Тут будут дельты

            int[] values = new int[OpersForFirstLevel.Count]; // Получаем значения

            int[] weights = new int[OpersForFirstLevel.Count]; // Веса

            double minDelta = double.MaxValue;
            for (int i = 0; i < OpersForFirstLevel.Count; i++)
            {
                var minDeadline = OpersForFirstLevel[i].block.Lists[0].StagesAsNormal.Min(x => x.OrderLink.Deadline);
                var desiredRC = OpersForFirstLevel[i].Center;
                CenterToIndex.TryAdd(desiredRC, CenterToIndex.Count);

                var blockDuration = OpersForFirstLevel[i].block.MaxTime(desiredRC);
                DateTime blockEnd = basisLink.StartOfPlanningDate + blockDuration;

                deltas[i] = (blockEnd - minDeadline).TotalDays; // Получаем дельту. Если дедлайн был меньше, то и дельта будет положительная.
                if (minDelta > deltas[i]) minDelta = deltas[i];
            }

            for (int i = 0; i < OpersForFirstLevel.Count; i++)
            {
                if (minDelta < 0) deltas[i] -= minDelta; // Увеличиваем на эту самую дельту, чтобы избавиться от нулей.
                values[i] = (int)(deltas[i] * 1000) + 1; // Ну это чтобы не было 0, если пришёл в эту точку.
            }

            Console.WriteLine("R: creating solver...");

            // Создадим решатель
            CpModel model = new CpModel();

            // Сначала просто создаём переменную
            ILiteral[,] xNew = new ILiteral[OpersForFirstLevel.Count, CenterToIndex.Count];
            for (int i = 0; i < OpersForFirstLevel.Count; i++)
            {
                for (int b = 0; b < CenterToIndex.Count; b++)
                {
                    xNew[i, b] = model.NewBoolVar($"Element: {OpersForFirstLevel[i].block.ToString()} to bin: {CenterToIndex.ElementAt(b).Value}");
                }
            }

            Console.WriteLine("R: creating table...");

            // Создаём таблицу для данных
            int[,] IBtable = new int[OpersForFirstLevel.Count, CenterToIndex.Count];
            // Заполняем ее данными по весу
            for (int i = 0; i < OpersForFirstLevel.Count; i++)
            {
                for (int b = 0; b < CenterToIndex.Count; b++)
                {
                    if (OpersForFirstLevel[i].Center == CenterToIndex.ElementAt(b).Key) // ТОЛЬКО ДЛЯ ТЕХ, КУДА ЭТО ОПРЕДЕЛЕНО!!!
                    {
                        IBtable[i, b] = values[i];
                    }
                }
            }

            Console.WriteLine("R: creating restriction 1...");

            // Ограничение: каждый предмет можно только в одну корзину, и при том не в каждую
            for (int i = 0; i < OpersForFirstLevel.Count; i++)
            {
                List<ILiteral> literals = new List<ILiteral>();
                List<ILiteral> literalsRestricted = new List<ILiteral>();
                for (int b = 0; b < CenterToIndex.Count; b++)
                {
                    if (IBtable[i, b] == 0)
                    {
                        // Это значит, что ни аналога, ни обычного совпадения по номенклатуре не было (иначе бы заполнилась), поместить нельзя
                        literalsRestricted.Add(xNew[i, b]);
                    }
                    else
                    {
                        // Поместить можно.
                        literals.Add(xNew[i, b]);
                    }
                }
                model.Add(Google.OrTools.Sat.LinearExpr.Sum(literalsRestricted) == 0); // В сумме запрещенных литератов быть не должно! Должен быть 0!
                model.AddAtMostOne(literals); // А разрешенных - лишь 1!
            }

            Console.WriteLine("R: creating restriction 2...");

            // Больше чем вместимость рюкзака - нельзя вместить
            for (int b = 0; b < CenterToIndex.Count; b++)
            {
                List<ILiteral> items = new List<ILiteral>();
                List<int> Weights = new();
                for (int i = 0; i < OpersForFirstLevel.Count; i++)
                {
                    items.Add(xNew[i, b]);
                    Weights.Add(1); // У НЕГО ВЕС 1
                }
                if (items.Count > 0)
                {
                    model.Add(Google.OrTools.Sat.LinearExpr.WeightedSum(items, Weights) <= 1); // не больше 1 собственно
                }
            }

            Console.WriteLine("R: creating restriction 3...");

            // На основании таблицы задаём удельную ценность
            LinearExprBuilder obj = Google.OrTools.Sat.LinearExpr.NewBuilder();
            for (int i = 0; i < OpersForFirstLevel.Count; i++)
            {
                for (int b = 0; b < CenterToIndex.Count; b++)
                {
                    obj.AddTerm(xNew[i, b], IBtable[i, b]); // Если не то подразделение, то этот остаток не настолько хорош, как другие.
                }
            }
            model.Minimize(obj); // Не максимум, а МИНИМУМ ищи!

            Console.WriteLine("R: solving...");

            // Решалка
            var dt0 = DateTime.Now;
            CpSolver solverNew = new CpSolver();
            CpSolverStatus status = CpSolverStatus.Unknown;
            CustomCpSolverSolutionCallback cb = new CustomCpSolverSolutionCallback(500);
            solverNew.StringParameters = "max_time_in_seconds:20.0";// enumerate_all_solutions:true; - было в документации, но и без нее всё работает
            status = solverNew.Solve(model, cb);
            var dt1 = DateTime.Now;
            var delta = dt1 - dt0;
            Console.WriteLine($"total time wasted on R solver is {delta}, writing results...");

            bool success = status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible;

            bool any = false;

            if (success)
            {
                for (int i = 0; i < OpersForFirstLevel.Count; i++)
                {
                    for (int b = 0; b < CenterToIndex.Count; b++)
                    {
                        if (solverNew.BooleanValue(xNew[i, b]))
                        {
                            var element = OpersForFirstLevel[i];
                            var center = CenterToIndex.ElementAt(b).Key;

                            AddBlockToPlan(element.block, center);

                            any = true;
                        }
                    }
                }
            }
            if (!success || !any)
            {
                Console.WriteLine($"solver R ended with {status}");
                basisLink.SessionLink.AssignedLogger.LogQueue.Enqueue($"solver for R ended with {status}");


                HashSet<Uco> passedCenters = new();
                HashSet<PlanSimulatorBlock> passedBlocks = new();

                var OpersForFirstLevelForSort = OpersForFirstLevel.Select((x, index) => (element: x, value: values[index])).OrderBy(x => x.value).ToArray();

                for (int i = 0; i < OpersForFirstLevelForSort.Length; i++)
                {
                    var element = OpersForFirstLevelForSort[i].element;
                    var center = element.Center;
                    if (passedCenters.Contains(center) || passedBlocks.Contains(element.block)) continue;

                    AddBlockToPlan(element.block, center);

                    passedCenters.Add(center);
                    passedBlocks.Add(element.block);
                }

            }
        }

        public void Start(PlanningAlgorithmWorkflow workflow)
        {
            var start = DateTime.Now;

            OpersCopy = basisLink.UsedPool.CopyBlocksDeep(basisLink, basisLink.UsedPool.BestPoolIndex, true);

            Console.WriteLine("R, beg, " + OpersCopy.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Count());


            //.Where(x => x.Lists[0].FirstOperJust.Center.Label.Contains("EEL-40")).ToList();
            var poolCopy = new PlanSimulatorPool(basisLink.SessionLink, OpersCopy, basisLink.MinAcceptableDate, basisLink.UsedPool.PoolSize);
            poolCopy.PrepareOnOneRCData();
            poolCopy.PrepareAllRCsOnBlocks();
            BlocksDependingOnCenter = poolCopy.CentersData.ToFrozenDictionary(x => x.Key, x => new List<PlanSimulatorBlock>());

            // Сейчас лучший индекс - 0.

            // Получаем самый максимальный уровень, по которому только можно ходить.

            Step1_FirstStages(); // Поставим сначала первые этапы

            bool success = Step2_Process();

            if (success)
            {
                poolCopy.Blocks = LinearPlan;
                var ALL_COUNT = basisLink.UsedPool.Blocks.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Count();
                Console.WriteLine("R, end, " + ALL_COUNT);
                ALL_COUNT = basisLink.UsedPool.Blocks.SelectMany(x => x.Lists.SelectMany(y => y.StagesAsNormal.Select(z => z.Index))).Distinct().Count();
                Console.WriteLine("R, end, " + ALL_COUNT);
                ;
            }
            else
            {
                ;
            }

            var end = DateTime.Now;

            Console.WriteLine($"R total time = {end - start}");
        }

        bool Step2_Process()
        {
            Console.WriteLine("R: get max level...");
            int MaxLevel = OpersCopy.Max(x => x.Lists.Max(y => y.StagesAsNormal.Max(z => z.Level)));


            PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngineResult LastResult = PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(LinearPlan, 0, basisLink.StartOfPlanningDate, null);

            basisLink.UpdateBcsSuccess(LastResult, LinearPlan, 0);

            int CurrentLevel = 2; // Потому что мы первый уровень уже прошли, полагается.

            int ErroredIterationsCount = 0;

            bool Success = false;


            object[] lockers = new object[basisLink.UsedPool.PoolSize];
            for (int i = 0; i < lockers.Length; i++) lockers[i] = new();
            ConcurrentQueue<int> AvailableIndexes = new ConcurrentQueue<int>();
            for (int i = 1; i < basisLink.UsedPool.PoolSize - 3; i++) // -1 потому что последний пул нужен для ручного анализа.
            {
                //if (i == bestIndex) continue;
                AvailableIndexes.Enqueue(i);
            }

            while (OpersCopy.Count > 0)
            {
                var PossibleOpersHere = OpersCopy.Where(x => x.Lists[0].FirstOperJust.Level == CurrentLevel

                &&

                x.Lists[0].PrevMods.All(mod => ExistingBlocks.Contains(mod.BlockLink)) // Проверяем на хронологию

                &&

                !ExistingBlocks.Contains(x)

                ).ToArray();

                if (PossibleOpersHere.Length == 0)
                {
                    ErroredIterationsCount++;
                    if (ErroredIterationsCount == MaxLevel * 2)
                    {
                        break;
                    }
                }

                if (PossibleOpersHere.Length > 0)
                {
                    if (basisLink.WannaStop)
                    {
                        break;
                    }

                    float minResult = float.MaxValue;
                    Uco ChoosenCenter = Uco.EmptyObject;
                    PlanSimulatorMod ChoosenMod = null;
                    object locker = new object();

                    var process = Parallel.For(0, PossibleOpersHere.Length, i =>
                    {
                        var mod = PossibleOpersHere[i].Lists[0];
                        var possibleRCs = mod.PossibleRCsHere;
                        var subProcess = Parallel.For(0, possibleRCs.Count, j =>
                        {

                            while (AvailableIndexes.Count == 0)
                            {
                                ;
                            }
                            int IndexToWorkWith = 0;
                            while (!AvailableIndexes.TryDequeue(out IndexToWorkWith))
                            {
                                ;
                            }
                            lock (lockers[IndexToWorkWith])
                            {
                                float localResult = Step2_1_CheckBlock(mod, possibleRCs[j], IndexToWorkWith);
                                lock (locker)
                                {
                                    if (minResult > localResult)
                                    {
                                        minResult = localResult;
                                        ChoosenMod = mod;
                                        ChoosenCenter = possibleRCs[j];
                                    }
                                }
                                AvailableIndexes.Enqueue(IndexToWorkWith);
                            }

                        });
                        while (!subProcess.IsCompleted) ;
                    });
                    while (!process.IsCompleted) ;

                    AddBlockToPlan(ChoosenMod.BlockLink, ChoosenCenter);

                    if (OpersCopy.Count == 0) Success = true;

                    ErroredIterationsCount = 0;

                    LastResult = PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(LinearPlan, 0, basisLink.StartOfPlanningDate, null); // Вызывем симулятор плана
                    
                    basisLink.UpdateBcsSuccess(LastResult, LinearPlan, 0);
                    basisLink.UsedWorkflow.Generation++;

                    basisLink.AlgorithmCallback?.Invoke(basisLink.CurrentStage, LinearPlan.Count);
                }


                CurrentLevel++; // Обновляем уровень
                if (CurrentLevel > MaxLevel) CurrentLevel = 0;
            }

            return Success;

        }

        float Step2_1_CheckBlock(PlanSimulatorMod mod, Uco choosenCenter, int poolIndex)
        {
            var swallowCopy = PlanSimulatorPool.CopyBlocksSwallow(LinearPlan);
            PlanSimulatorPool.MoveCentersInAllBlocks(swallowCopy, 0, poolIndex);

            PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngineResult localResult = PlanSimulator.PlanSimulatorEngine.PlanSimulatorEngine.CalculatePlan(swallowCopy, poolIndex, basisLink.StartOfPlanningDate, null);

            var list = BlocksDependingOnCenter[choosenCenter];

            var lastBlock = list.Count == 0 ? null : list[^1];
            var operationA = list.Count == 0 ? null : lastBlock.Lists[0]?.FirstOper;

            var minDate = mod.PrevMods.Count == 0 ? basisLink.StartOfPlanningDate : mod.PrevMods.Max(x => x.DateOfEnd[0]);
            if (minDate == DateTime.MinValue) minDate = basisLink.StartOfPlanningDate; // Получаем минимальную дату

            var lastCenterDate = lastBlock == null ? basisLink.StartOfPlanningDate : lastBlock.Lists[0].DateOfEnd[0];

            TimeSpan prostoiDur = minDate - lastCenterDate;

            var rechangesResult = operationA == null ? null : basisLink.SessionLink.MasterDataLinks.RechangesMasterData.FindRechanges(new(lastBlock.GetCenterData(0).CenterDataLink, new PlanSimulatorDatesData(minDate, 0, -1, -1), operationA.Stage, mod.FirstOperJust, choosenCenter));

            TimeSpan rechesDur = rechangesResult == null ? new TimeSpan() : TimeSpan.FromSeconds(rechangesResult.Duration);

            prostoiDur -= rechesDur;

            if (prostoiDur.Ticks < 0) prostoiDur = TimeSpan.Zero;

            var lastDate = lastCenterDate + prostoiDur + rechesDur + mod.AllTime[choosenCenter];

            return (float)/*rechesDur.TotalSeconds;//*/(lastDate - basisLink.MinAcceptableDate).TotalSeconds;

        }

    }
}

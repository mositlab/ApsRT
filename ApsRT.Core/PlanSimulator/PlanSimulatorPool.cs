using ApsRT.Core.NaladLengths;
using ApsRT.Core.Partias.PartiasClasses;
using ApsRT.Core.Planning;
using ApsRT.Core.Planning.PlanningAlgorithm;
using ApsRT.Core.Stages;
using ApsRT.Core.Taras.TaraSearchEngine;
using Google.OrTools.ConstraintSolver;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UniversalPack;
using ZLinq;
using static System.Reflection.Metadata.BlobBuilder;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;

namespace ApsRT.Core.PlanSimulator
{
    public class PlanSimulatorPool
    {

        public override string ToString()
        {
            return string.Format("#{0:X}", GetHashCode());
        }

        public Session.Session SessionLink { get; }

        public int PoolSize { get; }

        // Собственно блоки
        public List<PlanSimulatorBlock> Blocks;

        // Тут хранится инфа о центрах
        public FrozenDictionary<Uco, PlanSimulatorCenterData[]> CentersData;

        public int BestPoolIndex = 0;

        public FrozenDictionary<OrderID, PlanSimulatorOrderData[]> OrdersData;

        public FrozenDictionary<Guid, PlanSimulatorMod> ModLinks;

        public FrozenDictionary<int, PlanSimulatorStage> StagesLinks;

        public FrozenDictionary<Guid, PlanSimulatorPartiaRecalcData[]> PartiasToRecalc;

        //public FrozenDictionary<Guid, PlanSimulatorPartiaRottingData[]> PartiasRottingData;

        public PlanSimulatorPool(Session.Session session, List<PlanSimulatorBlock> blocks, DateTime dateOfStart, int PoolSize)
        {
            this.PoolSize = PoolSize;
            SessionLink = session;

            var alreadyCreatedMods = blocks.AsValueEnumerable().SelectMany(x => x.Lists);

            var innerData = alreadyCreatedMods.SelectMany(y => y.Stages);

            StagesLinks = innerData.ToFrozenDictionary(x => x.Stage.Index, x => x);

            var allCents = alreadyCreatedMods.SelectMany(x => x.PossibleRCsHere).Distinct();
            var centersData = new Dictionary<Uco, PlanSimulatorCenterData[]>(allCents.Count());
            foreach (var rcID in allCents)
            {
                var array = new PlanSimulatorCenterData[PoolSize];
                for (int i = 0; i <  array.Length; i++)
                {
                    array[i] = PlanSimulatorEngine.PlanSimulatorEngine.FillCenterWithSomeData(null, rcID, dateOfStart);
                }
                centersData.Add(rcID, array);
            }
            CentersData = centersData.ToFrozenDictionary(); 

            HashSet<Guid> partiasCodes = innerData.Select(z => z.Stage.GetOrderOrPartiaCode()).ToHashSet();
            HashSet<OrderID> ordersCodes = innerData.Select(z => z.Stage.OrderLink.OrderID).ToHashSet();

            // Тут хранится инфа о заказах
            OrdersData = ordersCodes.ToFrozenDictionary(x => x, x =>
            {
                var array = new PlanSimulatorOrderData[PoolSize];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new PlanSimulatorOrderData(DateTime.MinValue, -1, 0, 0);
                }
                return array;
            });

            PartiasToRecalc = partiasCodes.ToFrozenDictionary(x => x, x =>
            {
                var array = new PlanSimulatorPartiaRecalcData[PoolSize];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new PlanSimulatorPartiaRecalcData(false, -1);
                }
                return array;
            });



            FrozenDictionary<Guid, PlanSimulatorLastSubsData[]> LastSubs = null;

            LastSubs = partiasCodes.ToFrozenDictionary(x => x, x =>
            {
                var array = new PlanSimulatorLastSubsData[PoolSize];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new PlanSimulatorLastSubsData(Uco.EmptyObject, -1);
                }
                return array;
            });


            Blocks = blocks;

            ConcurrentDictionary<Guid, PlanSimulatorMod> modLinks = new();

            var res0 = Parallel.For(0, blocks.Count, index =>
            {
                var block = blocks[index];
                block.PoolLink = this;
                for (int m = 0; m < block.Lists.Count; m++)
                {
                    var mod = block.Lists[m];

                    var partiaCode = mod.FirstOperJust.GetOrderOrPartiaCode();

                    mod.BlockLink = block;
                    mod.NaladLengthData = new PlanSimulatorNaladLengthData[PoolSize];
                    for (int i = 0; i < PoolSize; i++)
                    {
                        var length = new GottenNaladLengthStruct();
                        mod.NaladLengthData[i] = new PlanSimulatorNaladLengthData(-1, length);
                    }

                    if (mod.EasyLifeData != null)
                    {
                        foreach (var easy in mod.EasyLifeData)
                        {
                            modLinks.TryAdd(easy.Key, mod);
                        }
                    }
                    else
                    {
                        modLinks.TryAdd(mod.FirstOperJust.CurrentModelling/*IsCustomModelling ? mod.FirstOperJust.CustomModelling : mod.FirstOperJust.MesModelling.Code*/, mod);
                    }

                    mod.WasPassed = new bool[PoolSize];

                    mod.MinDatesData = new PlanSimulatorDatesData[PoolSize];
                    for (int i = 0; i < PoolSize; i++)
                    {
                        mod.MinDatesData[i] = new PlanSimulatorDatesData(DateTime.MinValue, -1, -1, -1);
                    }

                    mod.Ranks = new int[PoolSize];

                    //mod.PrevMods = mod.StagesAsNormal.AsValueEnumerable().SelectMany(x => x.Prevs).Where(x => StagesLinks.ContainsKey(x)).Select(x => StagesLinks[x].ModLink).Distinct().ToList();
                    //mod.NextMods = mod.StagesAsNormal.AsValueEnumerable().SelectMany(x => x.Nexts).Where(x => StagesLinks.ContainsKey(x)).Select(x => StagesLinks[x].ModLink).Distinct().ToList();


                    // Гниение тары
                    mod.PartiaRottingData = new PlanSimulatorPartiaRottingData[PoolSize];
                    for (int i = 0; i < PoolSize; i++)
                    {
                        mod.PartiaRottingData[i] = new PlanSimulatorPartiaRottingData(null, -1);
                    }

                    mod.RottingTimeInDays = new double[PoolSize];

                    mod.LastSubsData = LastSubs[partiaCode];

                    for (int s = 0; s < mod.Stages.Count; s++)
                    {
                        var stage = mod.Stages[s];
                        stage.ModLink = mod;
                        stage.OrderData = OrdersData[stage.Stage.OrderLink.OrderID];
                        stage.NaladLengthData = new PlanSimulatorNaladLengthData[PoolSize];
                        for (int i = 0; i < PoolSize; i++)
                        {
                            var length = new GottenNaladLengthStruct();
                            stage.NaladLengthData[i] = new PlanSimulatorNaladLengthData(-1, length);
                        }
                        stage.PartiaRecalcData = PartiasToRecalc[partiaCode];
                    }
                }
                (var center, _, _) = block.GetCenterData(0);
                for (int i = 0; i < PoolSize; i++)
                {
                    block.SetCenter(center, i);
                }
            });
            while (!res0.IsCompleted)
            {
                ;
            }
            var res1 = Parallel.For(0, blocks.Count, index =>
            {
                var block = blocks[index];
                block.PoolLink = this;
                for (int m = 0; m < block.Lists.Count; m++)
                {
                    var mod = block.Lists[m];
                    mod.PrevMods = mod.StagesAsNormal.AsValueEnumerable().SelectMany(x => x.Prevs).Where(x => StagesLinks.ContainsKey(x)).Select(x => StagesLinks[x].ModLink).Distinct().ToList();
                    mod.NextMods = mod.StagesAsNormal.AsValueEnumerable().SelectMany(x => x.Nexts).Where(x => StagesLinks.ContainsKey(x)).Select(x => StagesLinks[x].ModLink).Distinct().ToList();

                    for (int s = 0; s < mod.Stages.Count; s++)
                    {
                        var stage = mod.Stages[s];
                        stage.PreviousStages = stage.Stage.Prevs.AsValueEnumerable().Where(x => StagesLinks.ContainsKey(x)).Select(x => StagesLinks[x]).ToList();
                        stage.NextStages = stage.Stage.Nexts.AsValueEnumerable().Where(x => StagesLinks.ContainsKey(x)).Select(x => StagesLinks[x]).ToList();
                    }
                }
            });
            while (!res1.IsCompleted)
            {
                ;
            }
            ModLinks = modLinks.ToFrozenDictionary();
        }



        public Dictionary<int, int> ModIDToOnOneRCGroupID = new();
        public Dictionary<int, (List<PlanSimulatorMod> ModIDs, List<Uco> PossibleRCs)> OnOneRCGroupData = new();


        public void PrepareOnOneRCData()
        {
            ModIDToOnOneRCGroupID.Clear();
            OnOneRCGroupData.Clear();

            var modsToGroup = Blocks.SelectMany(x => x.Lists).Where(x => x.StagesAsNormal.Any(s => s.OnOneRC)); // Выберем все нужные моды

            var groups = modsToGroup.GroupBy(mod => (mod.FirstOperJust.GetOrderOrPartiaCode(), /*mod.FirstOperJust.Level,*/ mod.FirstOperJust.OperationVid, mod.FirstOperJust.Nomenklature, mod.FirstOperJust.Characteristics/*, mod.FirstOperJust.Poviv*/));
            foreach (var group in groups) // каждая группа определена по номеру партии, уровню, виду операции, номенклатуры
            {
                var groupID = OnOneRCGroupData.Count;
                var modList = group/*.Select(x => x.ModID)*/.Where(x => x.EasyLifeData == null).ToList();

                if (modList.Count == 0) continue;

                List<Uco> rcs = null;
                foreach (var mod in group)
                {
                    if (rcs == null)
                        rcs = mod.PossibleRCsHere;
                    else
                        rcs = rcs.Intersect(mod.PossibleRCsHere).ToList();
                }

                var rcsCopy = rcs.Where(rc => 
                
                modList.All(mod => Taras.TaraSearchEngine.TaraSearchEngine.SumIsPossible(mod, out _, out _,
                    new SumIsPossibleOptions(false)
                    {
                        LogSomeErrors = false, // ошибки не фиксируем
                        RCMustBeThis = new(1) { rc } // РЦ должен быть именно такой
                    })
                )
                );

                if (rcsCopy.Count() > 0)
                {
                    rcs = rcsCopy.ToList();
                }
                else
                {
                    // ЭТО ОШИБКА
                    ;
                }

                    OnOneRCGroupData.Add(groupID, (modList, rcs));
                foreach (var modID in modList)
                {
                    ModIDToOnOneRCGroupID.Add(modID.ModID, groupID); // Заполняем ссылки на группы
                }
            }
        }

        public void UpdateOpsOnOneRC(PlanSimulatorBlock blockToStart, int PoolIndex)
        {
            (var settedRC, _, _) = blockToStart.GetCenterData(PoolIndex);
            foreach (var mod in blockToStart.Lists)
            {
                if (mod.FirstOperJust.OnOneRC
                    && ModIDToOnOneRCGroupID.TryGetValue(mod.ModID, out var dataID)
                    && OnOneRCGroupData.TryGetValue(dataID, out var data))
                {
                    foreach (var neededMod in data.ModIDs)
                    {
                        if (neededMod.ModID == mod.ModID) continue; // текущий мод не нужно искать и менять РЦ
                        neededMod.BlockLink.SetCenter(settedRC, PoolIndex);
                        //var index = pool.Blocks.FindIndex(x => x.Lists.Any(y => y.ModID == neededMod));
                        //if (index != -1) pool.Blocks[index].ActualRC = settedRC;
                    }
                }
            }
        }

        public void PrepareAllRCsOnBlocks()
        {
            foreach (var block in Blocks)
            {
                block.UpdatePossibleRCsHere();
            }
        }

        static int moveLeftIt(List<PlanSimulatorBlock> Blocks, PlanSimulatorBlock block, int PoolIndex)
        {
            var myIndex = Blocks.IndexOf(block);
            var firstLeftBlock = Blocks.FindLastIndex(myIndex - 1, blockL => blockL.GetCenterData(PoolIndex).Center == block.GetCenterData(PoolIndex).Center);


            var depends = block.Lists
                     .SelectMany(mod => mod.PrevMods)
                     .Select(mod => mod.BlockLink)
                     .Distinct()
                     .OrderBy(block => block.BlockIndex[PoolIndex]);

            int maxPrevIndex = -1;
            foreach (var dep in depends)
            {
                int prev = moveLeftIt(Blocks, dep, PoolIndex);
                maxPrevIndex = Math.Max(prev, maxPrevIndex);
            }


            if (maxPrevIndex > firstLeftBlock) firstLeftBlock = maxPrevIndex;

            firstLeftBlock++;

            Blocks.RemoveAt(myIndex);
            Blocks.Insert(firstLeftBlock, block);
            return firstLeftBlock; // Теперь это его индекс

        }

        static void adder(List<PlanSimulatorBlock> finalResult, List<PlanSimulatorBlock> blocksToSet, bool toBack, int PoolReadIndex, int PoolWriteIndex)
        {
            blocksToSet = blocksToSet.Where(block => !finalResult.Contains(block)).ToList();
            //for (int i = 0; i < blocksToSet.Count; i++)
            //{
            //    var opsHere = blocksToSet[i].Lists.SelectMany(mod => mod.StagesAsNormal).ToHashSet();
            //    var suchkaIndex = finalResult.FindIndex(block => block.Lists.SelectMany(mod => mod.StagesAsNormal).Any(op => opsHere.Contains(op)));
            //    if (suchkaIndex > -1)
            //    {
            //        var getThisblock = finalResult[suchkaIndex];
            //        ;
            //    }
            //}

            if (toBack)
                finalResult.InsertRange(0, blocksToSet);
            else
                finalResult.AddRange(blocksToSet);

            var listToPrev = blocksToSet
                .SelectMany(block => block.Lists)
                .SelectMany(mod => mod.PrevMods)
                .Select(mod => mod.BlockLink)
                .Distinct()
                .Where(block => !finalResult.Contains(block))
                .OrderBy(block => block.BlockIndex[PoolReadIndex])
                .ToList();

            var listToNext = blocksToSet
                .SelectMany(block => block.Lists)
                .SelectMany(mod => mod.NextMods)
                .Select(mod => mod.BlockLink)
                .Distinct()
                .Where(block => !finalResult.Contains(block) && !listToPrev.Contains(block))
                .OrderBy(block => block.BlockIndex[PoolReadIndex])
                .ToList();

            if (listToPrev.Count > 0) adder(finalResult, listToPrev, true, PoolReadIndex, PoolWriteIndex);
            if (listToNext.Count > 0) adder(finalResult, listToNext, false, PoolReadIndex, PoolWriteIndex);
        }

        static void addThis(List<PlanSimulatorBlock> finalResult, List<PlanSimulatorBlock> blocksToSet, List<PlanSimulatorBlock> Blocks)
        {
            blocksToSet = blocksToSet.Where(block => !finalResult.Contains(block)).ToList();
            if (blocksToSet.Count == 0) return;

            finalResult.AddRange(blocksToSet);

            var listToNext = blocksToSet
                .SelectMany(block => block.Lists)
                .SelectMany(mod => mod.NextMods)
                .Select(mod => mod.BlockLink)
                .Distinct()
                .Where(block => !finalResult.Contains(block))
                .OrderBy(block => Blocks.IndexOf(block))
                .ToList();

            if (listToNext.Count > 0) addThis(finalResult, listToNext, Blocks);
        }

        public static List<PlanSimulatorBlock> Refragmentate(List<PlanSimulatorBlock> Blocks, int PoolReadIndex, int PoolWriteIndex)
        {
            //Dictionary<>

            List<PlanSimulatorBlock> finalResult = new();
            addThis(finalResult, Blocks.Where(block => block.Lists
                .SelectMany(mod => mod.PrevMods)
                .Select(mod => mod.BlockLink)
                .Count() == 0)
                .OrderBy(block => Blocks.IndexOf(block))
                .ToList(), Blocks);



            //List<PlanSimulatorBlock> finalResult = new();


            //List<PlanSimulatorBlock> blocksToSet = Blocks.Where(x => x.GetCenterData(PoolReadIndex).Center == NeededCenter)
            //    .OrderBy(x => x.BlockIndex[PoolReadIndex])
            //    .ToList();

            //adder(finalResult, blocksToSet, true, PoolReadIndex, PoolWriteIndex);

            //Blocks = Blocks.Where(block => !finalResult.Contains(block)).ToList(); // Осталась часть тех, что не вошли в план, будучи на других РЦ

            ////var groupedByLevel = Blocks.

            //// Остались вообще независимые. Просто суём их в самый конец.
            //Blocks = Blocks.Where(block => !finalResult.Contains(block)).ToList();
            //finalResult.AddRange(Blocks);

            for (int i = 0; i < finalResult.Count; i++)
            {
                finalResult[i].BlockIndex[PoolWriteIndex] = i;
            }

            return finalResult;


            //moveLeftIt(Blocks, target, PoolIndex);




        }

        public static int InsertBlockSafe(List<PlanSimulatorBlock> Blocks, PlanSimulatorBlock block, int IndexToInsert, int PoolIndex, int indexToGetInfo)
        {
            int Prev = -1;
            var center = block.GetCenterData(PoolIndex).Center;
            var prevBlock = IndexToInsert == 0 ? -1 : Blocks.FindLastIndex(IndexToInsert - 1, x => x.GetCenterData(PoolIndex).Center == center);
            if (prevBlock != -1)
            {
                Prev = prevBlock;
                var prevBlockLink = Blocks[prevBlock];
                var nextBlockWas = prevBlockLink.NextBlockOnThisRC[PoolIndex];//CenterDataLocalCopy[PoolIndex].l
                if (nextBlockWas != null)
                {
                    nextBlockWas.PreviousBlockOnThisRC[PoolIndex] = block;
                    //Next = nextBlockWas.BlockIndex;
                }
                prevBlockLink.NextBlockOnThisRC[PoolIndex] = block;

                block.NextBlockOnThisRC[PoolIndex] = nextBlockWas;
                block.PreviousBlockOnThisRC[PoolIndex] = prevBlockLink;
            }
            else
            {
                var nextBlock = IndexToInsert == Blocks.Count ? -1 : Blocks.FindIndex(IndexToInsert, x => x.GetCenterData(PoolIndex).Center == center);
                if (nextBlock != -1)
                {
                    var nextBlockLink = Blocks[nextBlock];
                    var prevBlockWas = nextBlockLink.PreviousBlockOnThisRC[PoolIndex];
                    if (prevBlockWas != null)
                    {
                        prevBlockWas.NextBlockOnThisRC[PoolIndex] = block;
                        Prev = prevBlockWas.BlockIndex[indexToGetInfo];
                    }
                    nextBlockLink.PreviousBlockOnThisRC[PoolIndex] = block;

                    block.NextBlockOnThisRC[PoolIndex] = nextBlockLink;
                    block.PreviousBlockOnThisRC[PoolIndex] = prevBlockWas;
                }
                else
                {
                    block.NextBlockOnThisRC[PoolIndex] = null;
                    block.PreviousBlockOnThisRC[PoolIndex] = null;
                }
            }
            Blocks.Insert(IndexToInsert, block);

            //if (Prev != -1)
            //{
            //    Blocks[Prev].CenterDataLocalCopy[PoolIndex] = Blocks[Prev].CenterDataLocalCopy[indexToGetInfo];
            //}

            return Prev;
        }

        public static List<PlanSimulatorBlock> CopyBlocksSwallow(List<PlanSimulatorBlock> Blocks)
        {
            return new List<PlanSimulatorBlock>(Blocks);
        }

        public static void MoveBlockIndexesInAllBlocks(List<PlanSimulatorBlock> Blocks, int from, int to)
        {
            var res0 = Parallel.For(0, Blocks.Count, i =>
            {
                Blocks[i].BlockIndex[to] = Blocks[i].BlockIndex[from];// (from, to);
                //Blocks[i].CenterDataLocalCopy[to] = Blocks[i].CenterDataLocalCopy[from];
            });
            while (!res0.IsCompleted)
            {
                ;
            }
        }
        public static void MoveCentersInAllBlocks(List<PlanSimulatorBlock> Blocks, int from, int to)
        {
            var res0 = Parallel.For(0, Blocks.Count, i =>
            {
                Blocks[i].MoveCenterData(from, to);
                //Blocks[i].CenterDataLocalCopy[to] = Blocks[i].CenterDataLocalCopy[from];
            });
            while (!res0.IsCompleted)
            {
                ;
            }
        }
        public static void MovePassedInfoInAllBlocks(List<PlanSimulatorBlock> Blocks, int from, int to)
        {
            var res0 = Parallel.For(0, Blocks.Count, i =>
            {
                for (int j = 0; j < Blocks[i].Lists.Count; j++)
                {
                    Blocks[i].Lists[j].WasPassed[to] = Blocks[i].Lists[j].WasPassed[from];
                }
            });
            while (!res0.IsCompleted)
            {
                ;
            }
        }
        public static void MoveMinDatesInAllBlocks(List<PlanSimulatorBlock> Blocks, int from, int to)
        {
            var res0 = Parallel.For(0, Blocks.Count, i =>
            {
                for (int j = 0; j < Blocks[i].Lists.Count; j++)
                {
                    Blocks[i].Lists[j].MinDatesData[to] = new PlanSimulatorDatesData(Blocks[i].Lists[j].MinDatesData[from]);
                    Blocks[i].Lists[j].DateOfStart[to] = Blocks[i].Lists[j].DateOfStart[from];
                    Blocks[i].Lists[j].DateOfEnd[to] = Blocks[i].Lists[j].DateOfEnd[from];
                }
            });
            while (!res0.IsCompleted)
            {
                ;
            }
        }

        //public static void MoveIndexesInAllBlocks(List<PlanSimulatorBlock> Blocks, int from, int to)
        //{
        //    var res0 = Parallel.For(0, Blocks.Count, i =>
        //    {
        //        Blocks[i].BlockIndex[to] = Blocks[i].BlockIndex[from];
        //        Blocks[i].PreviousBlockOnThisRC[to] = Blocks[i].PreviousBlockOnThisRC[from];
        //        Blocks[i].NextBlockOnThisRC[to] = Blocks[i].NextBlockOnThisRC[from];
        //    });
        //    while (!res0.IsCompleted)
        //    {
        //        ;
        //    }
        //}

        public List<PlanSimulatorBlock> CopyBlocksDeep(PlanningAlgorithm basisLink, int bestIndex, bool WithTara)
        {
            //var dt0 = DateTime.Now;
            var opersCopy = new PlanSimulatorBlock[Blocks.Count];//new List<OpersBlock>(Blocks.Count);
            //for (var i = 0; i < Blocks.Count; i++)
            var res0 = Parallel.For(0, Blocks.Count, i =>
            {
                var mod = new List<PlanSimulatorMod>(Blocks[i].Lists.Count); // Создаём список моделирований (заготовку)
                for (var j = 0; j < Blocks[i].Lists.Count; j++)
                {

                    var NEWLIST = new PlanSimulatorMod(Blocks[i].Lists[j].StagesAsNormal, Blocks[i].Lists[j].ParallelRC, Blocks[i].Lists[j].ModType, Blocks[i].Lists[j].ModID, PoolSize); //Blocks[i].Lists[j];// new ModellingOperWithTime(new List<Operation>(Blocks[i].Lists[j].OpersList), Blocks[i].Lists[j].AllTime); // Копируем список операций и создаем моделирование
                    mod.Add(NEWLIST);
                }
                var block = new PlanSimulatorBlock(mod, Blocks[i].GetCenterData(bestIndex).Center, PoolSize, Blocks[i].FillKoeff, WithTara/*, Blocks[i].CanBeBroken*/);
                if (!WithTara)
                {
                    block.ExistingTaraHere = Blocks[i].ExistingTaraHere;
                    block.HowMuchPlaceHere = Blocks[i].HowMuchPlaceHere;
                    block.FillKoeff = Blocks[i].FillKoeff;
                }
                //opersCopy.Add(block); // Добавляем блок, у которого за основу взяты параметры
                opersCopy[i] = block;
            });
            while (!res0.IsCompleted)
            {
                ;
            }
            //var dt1 = DateTime.Now;
            var res = opersCopy.ToList();
            //Console.WriteLine($"copy length: {dt1-dt0}");
            return res;

            //var opersCopy = new OpersBlock[Blocks.Count];
            //Parallel.For(0, Blocks.Count, i =>
            //{
            //    var mod = new List<ModellingOper>(Blocks[i].Lists.Count); // Создаём список моделирований (заготовку)
            //    for (int j = 0; j < Blocks[i].Lists.Count; j++)
            //    {
            //        var NEWLIST = Blocks[i].Lists[j];// new ModellingOperWithTime(new List<Operation>(Blocks[i].Lists[j].OpersList), Blocks[i].Lists[j].AllTime); // Копируем список операций и создаем моделирование
            //        mod.Add(NEWLIST);
            //    }
            //    opersCopy[i] = new OpersBlock(mod, Blocks[i].ActualRC/*, Blocks[i].CanBeBroken*/); // Добавляем блок, у которого за основу взяты параметры
            //});
            //return new(opersCopy);
        }


        public static int MoveLeft(List<PlanSimulatorBlock> Blocks, PlanSimulatorBlock block, int wasHere, int Where, bool wasInPool, int ReadPoolIndex, PlanSimulatorBlock Awas, PlanSimulatorBlock Bwas, out HashSet<PlanSimulatorBlock> movedElements)
        {
            int movesCount = 1;
            HashSet<PlanSimulatorBlock> movedElements1 = new();
            var center00 = block.GetCenterData(ReadPoolIndex).Center;
            foreach (var listHere in block.Lists)
            {
                var partia = listHere.FirstOperJust.GetOrderOrPartiaCode();
                var foundPrev = Blocks.FindLastIndex(wasHere, x => x.Lists.Any(list => 
                    list.FirstOperJust.GetOrderOrPartiaCode() == partia
                    && list.FirstOperJust.Level < listHere.FirstOperJust.Level)
                    && x.GetCenterData(ReadPoolIndex).Center == center00);
                if (foundPrev >= Where)
                {
                    movesCount += MoveLeft(Blocks, Blocks[foundPrev], foundPrev, Where - 1, true, ReadPoolIndex, Awas, Bwas, out movedElements1);
                }
            }

            if (wasInPool) Blocks.RemoveAt(wasHere);

            HashSet<PlanSimulatorBlock> collected = new();
            List<PlanSimulatorBlock> collectedList = new();
            List<PlanSimulatorBlock> queue = new();
            var dependsBegin = block.Lists
                .SelectMany(mod => mod.PrevMods)
                .OrderByDescending(mod => mod.BlockLink.BlockIndex[ReadPoolIndex])
                .Select(mod => mod.BlockLink)
                .Distinct();
            foreach (var dep in dependsBegin) queue.Insert(0, dep);
            var foundPrevThis = Where == 0 ? -1 : Blocks.FindLastIndex(Where - 1, x => x.GetCenterData(ReadPoolIndex).Center == center00);
            if (foundPrevThis != -1) queue.Insert(0, Blocks[foundPrevThis]);

            while (queue.Count > 0)
            {
                var current = queue[0];
                var center = current.GetCenterData(ReadPoolIndex).Center;
                queue.RemoveAt(0);
                if (collected.Add(current))
                {
                    collectedList.Add(current);
                    var depends = current.Lists
                        .SelectMany(mod => mod.PrevMods)
                        .OrderByDescending(mod => mod.BlockLink.BlockIndex[ReadPoolIndex])
                        .Select(mod => mod.BlockLink)
                        .Distinct();
                    foreach (var dep in depends) queue.Insert(0, dep);
                    var myIndex = Blocks.IndexOf(current);
                    if (myIndex == -1)
                    {
                        ;
                    }
                    var foundPrevThis2 = myIndex <= 0 ? -1 : Blocks.FindLastIndex(myIndex - 1, x => x.GetCenterData(ReadPoolIndex).Center == center);
                    if (foundPrevThis2 != -1) queue.Insert(0, Blocks[foundPrevThis2]);
                }
            }

            var movingSequence = Blocks.Where(b => collected.Contains(b))/*.OrderByDescending(b => collectedList.IndexOf(b))*/.ToList();
            Blocks.RemoveAll(b => collected.Contains(b));

            var BIndex = movingSequence.IndexOf(Bwas);
            if (BIndex != -1)
            {
                collected = new();
                collectedList = new();
                queue = new();
                queue.Insert(0, movingSequence[BIndex]);
                while (queue.Count > 0)
                {
                    var current = queue[0];
                    var center = current.GetCenterData(ReadPoolIndex).Center;
                    queue.RemoveAt(0);
                    if (collected.Add(current))
                    {
                        collectedList.Add(current);
                        var depends = current.Lists
                            .SelectMany(mod => mod.NextMods)
                            .OrderByDescending(mod => mod.BlockLink.BlockIndex[ReadPoolIndex])
                            .Select(mod => mod.BlockLink)
                            .Where(block => movingSequence.Contains(block))
                            .Distinct();
                        foreach (var dep in depends) queue.Add(dep);
                        var myIndex = movingSequence.IndexOf(current);
                        if (myIndex == -1)
                        {
                            ;
                        }
                        var foundPrevThis2 = myIndex <= 0 ? -1 : movingSequence.FindIndex(myIndex + 1, x => x.GetCenterData(ReadPoolIndex).Center == center);
                        if (foundPrevThis2 != -1) queue.Add(movingSequence[foundPrevThis2]);
                    }
                }

                var movingSequence2 = movingSequence.Where(b => collected.Contains(b))/*.OrderByDescending(b => collectedList.IndexOf(b))*/.ToList();
                movingSequence.RemoveAll(b => collected.Contains(b));

                if (block.Lists.Any(listHere =>
                {
                    var partia = listHere.FirstOperJust.GetOrderOrPartiaCode();
                    return movingSequence2.Any(blockHere => blockHere.Lists.Any(list => list.FirstOperJust.GetOrderOrPartiaCode() == partia
                            && list.FirstOperJust.Level < listHere.FirstOperJust.Level));
                }))
                {
                    Blocks.Insert(0, block);
                    Blocks.InsertRange(0, movingSequence2);
                    Blocks.InsertRange(0, movingSequence);
                }
                else
                {
                    Blocks.InsertRange(0, movingSequence2);
                    Blocks.Insert(0, block);
                    Blocks.InsertRange(0, movingSequence);
                }
                    /*
                    foreach (var listHere in block.Lists)
                    {
                        var partia = 
                        var foundPrev = Blocks.FindLastIndex(wasHere, x => x.Lists.Any(list =>
                            )
                            && x.GetCenterData(ReadPoolIndex).Center == center00);
                        if (foundPrev >= Where)
                        {
                            movesCount += MoveLeft(Blocks, Blocks[foundPrev], foundPrev, Where - 1, true, ReadPoolIndex, Awas, Bwas, out movedElements1);
                        }
                    }
                    */

            }
            else
            {
                Blocks.Insert(0, block);
                Blocks.InsertRange(0, movingSequence);
            }





            movedElements = collected.Union(movedElements1).ToHashSet();

            return movesCount;
        }

        internal int Find(int Index, int Start, bool Back)
        {
            for (var i = Start; Back ? i >= 0 : i < Blocks.Count;)
            {
                for (var j = 0; j < Blocks[i].Lists.Count; j++)
                {
                    for (var jj = 0; jj < Blocks[i].Lists[j].StagesAsNormal.Count; jj++)
                    {
                        if (Blocks[i].Lists[j].StagesAsNormal[jj].Index == Index)
                        {
                            return i;
                        }
                    }
                }

                if (Back)
                {
                    i--;
                }
                else
                {
                    i++;
                }
            }
            return -1;
        }

        /// <summary>
        ///  Перенос
        /// </summary>
        /// <returns>Минимальный и максимальные индексы пула, куда можно поставить блок (включительно, в рандомайзерах надо будет прибавлять к max единицу).</returns>
        internal (int MIN, int MAX, int Prev, int Next) FindDiapazons(PlanSimulatorBlock opertoIns, int PoolIndex, Uco onRC, int wannaThisPlace)
        {
            var MIN = opertoIns.Lists[0].StagesAsNormal[0].Fixed ? 0 : (opertoIns.Lists[0].StagesAsNormal[0].VPZ || opertoIns.Lists[0].StagesAsNormal[0].IsFact ? SessionLink.AssignedPlanningAlgorithm.FixedBlocks : SessionLink.AssignedPlanningAlgorithm.VPZBlocks); // минимум - никаких операций нет, поэтому -1. Если будет хоть одна операция, то вставляем строго после нее.
            var MAX = opertoIns.Lists[0].StagesAsNormal[0].Fixed ? SessionLink.AssignedPlanningAlgorithm.FixedBlocks : (opertoIns.Lists[0].StagesAsNormal[0].VPZ || opertoIns.Lists[0].StagesAsNormal[0].IsFact ? SessionLink.AssignedPlanningAlgorithm.VPZBlocks : Blocks.Count); // максимум - включительно, включая последний же индекс. Insert вставит его ПЕРЕД этим индексом, поэтому всё норм.

            // Определяем границы
            int Prev = opertoIns.PreviousBlockOnThisRC[PoolIndex]?.BlockIndex[PoolIndex] ?? -1;
            int Next = opertoIns.NextBlockOnThisRC[PoolIndex]?.BlockIndex[PoolIndex] ?? -1;

            MIN = Math.Max(MIN, (opertoIns.Lists.Max(list => list.PrevMods.Count == 0 ? -1 : list.PrevMods.Max(mod => mod.BlockLink.BlockIndex[PoolIndex]))) + 1);
            MAX = Math.Min(MAX, (opertoIns.Lists.Min(list => list.NextMods.Count == 0 ? Blocks.Count : list.NextMods.Max(mod => mod.BlockLink.BlockIndex[PoolIndex]))));

            return (MIN, MAX, Prev, Next);
        }

        #region УЙМА всяких методов, которые по хорошему надо разгрести и убрать
        /// <summary>
        /// Словарь, связывающий моделирование и класс с инфой про даты занятой моделированием тары.
        /// Нужен для быстрого доступа к информации моделирования во время построения плана (так как оперирует номерами моделирования).
        /// НЕ ОБНОВЛЯЕТСЯ ПРИ ИЗМЕНЕНИИ ПЛАНА. В CalculateIt и G (17.05.2024) - бесполезен.
        /// </summary>
        Dictionary<int, OperationLineTime> OperToLineTime = new();

        /// <summary>
        /// Словарь, который позволяет хранить списки с временными отрезками, с фильтрацией по таре.
        /// В отличие от OperToLineTime, не оперирует номерами моделирований, так как используется методом GetTaraCountAt,
        /// которому не нужны номера, но нужен быстрый подсчёт количества временных отрезков.
        /// НЕ ОБНОВЛЯЕТСЯ ПРИ ИЗМЕНЕНИИ ПЛАНА. В CalculateIt и G (17.05.2024) - бесполезен.
        /// </summary>
        Dictionary<Uco, List<OperationLineTime>> StructuredTaraDataLine = new();
        internal void ClearTaraTimeLineDics()
        {
            OperToLineTime.Clear();
            StructuredTaraDataLine.Clear();
        }
        /// <summary>
        /// Добавляет новую запись о временном отрезке тары в словари.
        /// </summary>
        /// <param name="mod">Моделирование, которое надо добавить и обновить записи по таре.</param>
        /// <param name="start">Дата начала, его нельзя узнать из структуры мода, надо передавать.</param>
        /// <returns></returns>
        internal void AddInfoTaraTimeLine(PlanSimulatorMod mod, Uco center, DateTime start, string comment)
        {
            // 1. Обновляем в словаре информацию о предках, если таковые есть.
            // TODO: а если предков нет, то что тогда? уточнить у алены. Проблема бдует в том, что у меня нет инфы об отдающей таре.

            var end = start + mod.AllTime[center];
            if (mod.PrevMods.Count > 0)
            {
                foreach (var prev in mod.PrevMods)
                {
                    if (OperToLineTime.TryGetValue(prev.ModID, out var cl)) // класс такой есть? проверяем на всякий случай - если тара пустая, то ее может и не оказаться.
                    {
                        cl.uncheckedKids--;
                        cl.end = cl.end > end ? cl.end : end; // TODO: кажется у андрея была сравнивающая хрень для дат
                    }
                }
            }

            // 2. Добавляем инфу
            var line = new OperationLineTime(start, mod.NextMods.Count, comment); // создаём новый класс временного отрезка
            if (mod.NextMods.Count == 0)
            {
                line.end = end; // Если у меня нет детей (значит я последняя), то тут же и задаём последнюю дату.
            }

            var tara = mod.FirstOperJust.CurrentTara; // получаем тару текущего мода (достаточно глянуть на первый этап)
            if (!tara.IsEmpty)
            {
                OperToLineTime.Add(mod.ModID, line); // в один словарь добавляем
                if (!StructuredTaraDataLine.ContainsKey(tara))
                {
                    StructuredTaraDataLine.Add(tara, new());
                }

                StructuredTaraDataLine[tara].Add(line); // ну и в другой словарь - для метода "GetTaraCountAt"
            }
        }

        internal int GetTaraCountAt(Uco tara, DateTime time)
        {
            // TODO: оптимизировать ее так, чтобы она не весь список шерстила, а лишь N раз, где N - макс количество тары из базы данных.
            // взять у андрея.
            if (StructuredTaraDataLine.TryGetValue(tara, out var list))
            {
                if (list.Count == 0)
                {
                    return 0;
                }

                var count = list
                        .Where(x =>

                        time >= x.start // больше или равно старту
                        &&
                        (x.uncheckedKids > 0 || time <= x.end) // не все дети проверены (значит не снята тара), ЛИБО - меньше или равно концу (в диапазоне)

                        )
                        .Count();
                return count;
            }
            return 0;
        }
        #endregion
    }
}

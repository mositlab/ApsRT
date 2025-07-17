using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using UcoDU = UniversalPack.Common.CodedObject<System.Guid, UniversalPack.DimensionUnit>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;
using System.Collections.Frozen;
using System.Collections;
using ZLinq;

namespace ApsRT.Core.PlanSimulator
{
    public class PlanSimulatorTable
    {
        /*
        public PlanSimulatorTable(Session.Session sessionLink, DateTime startDate, List<Stages.Stage> stages, IEnumerable<Uco> possibleCenters): this(sessionLink, startDate, GenerateBlocks(stages), possibleCenters)
        {

        }

        static List<PlanSimulatorBlock> GenerateBlocks(List<Stages.Stage> stages)
        {
            return stages.AsValueEnumerable().OrderBy(x => x.Level).Select(stageBig =>
            {
                var mod = new PlanSimulatorMod();
                mod.Stages = new List<PlanSimulatorStage>();
                var stage = new PlanSimulatorStage();
                stage.Stage = stageBig;
                stage.ModLink = mod;
                mod.Stages.Add(stage);
                var block = new PlanSimulatorBlock();
                block.Modellings = new List<PlanSimulatorMod>();
                block.Modellings.Add(mod);
                block.ActualCenter = stage.Stage.Center;
                mod.BlockLink = block;
                return block;
            }).ToList();
        }

        public PlanSimulatorTable(Session.Session sessionLink, DateTime startDate, IEnumerable<PlanSimulatorBlock> blocks, IEnumerable<Uco> possibleCenters)
        {
            foreach (var stage in blocks) stage.TableLink = this;

            var dt0 = DateTime.Now;

            SessionLink = sessionLink;

            CentersRows = possibleCenters.AsValueEnumerable().Select((x, index) => (x, index)).ToFrozenDictionary(x => x.x, x => x.index);

            CentersLineRows = new List<PlanSimulatorBlock>[CentersRows.Count];

            Stages = blocks.AsValueEnumerable().SelectMany(x => x.Modellings.SelectMany(y => y.Stages)).ToFrozenDictionary(x => x.Stage.Index, x => x);

            var groups = blocks.GroupBy(x => x.ActualCenter);

            var res = Parallel.ForEach(groups, x =>
            {
                CentersLineRows[CentersRows[x.Key]] = x.AsValueEnumerable().ToList();
            });
            while (!res.IsCompleted) ;

            UpdateIndexes();

            RememberStagesEverywhere();

            SetPrevsAndNexts();

            var dt1 = DateTime.Now;
            Console.WriteLine($"bf {dt1 - dt0}");

            SetNewStartDate(startDate);

            var dt2 = DateTime.Now;
            Console.WriteLine($"af {dt2 - dt1}");
        }

        public Session.Session SessionLink { get; }

        public FrozenDictionary<Uco, int> CentersRows;

        public List<PlanSimulatorBlock>[] CentersLineRows;

        public FrozenDictionary<int, PlanSimulatorStage> Stages;


        public DateTime StartDate { get; private set; }

        /// <summary>
        /// Установить новую дату начала и пересчитать тем самым график.
        /// </summary>
        /// <param name="startDate">Дата начала</param>
        public void SetNewStartDate(DateTime startDate)
        {
            StartDate = startDate;
            RecalcFromFirstOnes();
        }

        public void UpdateIndexes()
        {
            var res = Parallel.For(0, CentersLineRows.Length, rowIndex =>//Parallel.ForEach(CentersLineRows, (row) =>
            {
                CentersLineRows[rowIndex] ??= new List<PlanSimulatorBlock>();
                var row = CentersLineRows[rowIndex];
                var res0 = Parallel.For(0, row.Count, i =>
                {
                    var block = row[i];
                    block.BlockIndexInLine = i;
                    block.PreviousBlockInLine = (i == 0) ? null : row[i - 1];
                    block.NextBlockInLine = (i == row.Count - 1) ? null : row[i + 1];
                });
                while (!res0.IsCompleted) ;
            });
            while (!res.IsCompleted) ;
        }

        public void RememberStagesEverywhere()
        {
            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            var res = Parallel.ForEach(CentersLineRows, (row) =>
            {
                var res0 = Parallel.For(0, row.Count, i =>
                {
                    var block = row[i];
                    block.RememberAllStagesHere();
                });
                while (!res0.IsCompleted) ;
            });
            while (!res.IsCompleted) ;
        }

        public void SetPrevsAndNexts()
        {
            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            var res = Parallel.ForEach(CentersLineRows, (row) =>
            {
                var res0 = Parallel.For(0, row.Count, i =>
                {
                    var block = row[i];
                    block.PreviousBlocksInChronology = block.AllStagesHere.AsValueEnumerable().SelectMany(x => ALL[x].Prevs).Where(x => Stages.ContainsKey(x)).Select(x => Stages[x].ModLink.BlockLink).Distinct().ToList();
                    block.NextBlocksInChronology = block.AllStagesHere.AsValueEnumerable().SelectMany(x => ALL[x].Nexts).Where(x => Stages.ContainsKey(x)).Select(x => Stages[x].ModLink.BlockLink).Distinct().ToList();
                });
                while (!res0.IsCompleted) ;
            });
            while (!res.IsCompleted) ;
        }
    
        public void RecalcFromFirstOnes()
        {
            for (int i = 0; i < CentersLineRows.Length; i++)
            {
                var row = CentersLineRows[i];
                if (row.Count == 0) continue;
                AddNeedToRecalc(row[0], true, true, true);
                //row[0].NeedToRecalc(true, true, true);
            }
            RecalcAllNeeded();
        }

        public class PlanSimulatorRecalcData()
        {
            public bool time, rech, addi;

            public PlanSimulatorRecalcData(bool time, bool rech, bool addi): this()
            {
                this.time = time;
                this.rech = rech;
                this.addi = addi;
            }
        }

        SortedDictionary<int, Dictionary<PlanSimulatorBlock, PlanSimulatorRecalcData>> NeedToRecalcIt = new ();
        public void AddNeedToRecalc(PlanSimulatorBlock block, bool time, bool rech, bool addi)
        {
            var level = block.Modellings[0].Stages[0].Stage.Level;
            NeedToRecalcIt.TryAdd(level, new());
            var dic = NeedToRecalcIt[level];
            dic.TryAdd(block, new PlanSimulatorRecalcData(time, rech, addi));
            var clas = dic[block];
            clas.time |= time;
            clas.rech |= rech;
            clas.addi |= addi;
        }
        public void RecalcAllNeeded()
        {
            //HashSet<PlanSimulatorBlock> block = new();
            while (NeedToRecalcIt.Count > 0)
            {
                var current = NeedToRecalcIt.First();
                if (current.Value.Count == 0)
                {
                    NeedToRecalcIt.Remove(current.Key);
                    continue;
                }
                var dic = NeedToRecalcIt[current.Key];
                var gotten = dic.First();
                gotten.Key.NeedToRecalc(gotten.Value.time, gotten.Value.rech, gotten.Value.addi);
                dic.Remove(gotten.Key);
                if (dic.Count == 0)
                {
                    NeedToRecalcIt.Remove(current.Key);
                }
                //Console.WriteLine(NeedToRecalcIt.Count);
                //block.Add(current.Key);
            }
            //Console.WriteLine(block.Count.ToString());
        }
        */
    }
}

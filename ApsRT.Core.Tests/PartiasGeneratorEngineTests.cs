using System.Drawing;
using System.Reflection;
using ApsRT.Core.Centers;
using ApsRT.Core.Orders;
using ApsRT.Core.Partias.PartiasAlgorithm;
using ApsRT.Core.Partias.PartiasClasses;
using ApsRT.Core.Partias.PartiasConnectorEngine;
using ApsRT.Core.Partias.PartiasGeneratorEngine;
using ApsRT.Core.Planning.PlanningAlgorithm;
using ApsRT.Core.PlanSimulator;
using ApsSession = ApsRT.Core.Session.Session;
using ApsRT.Core.Stages;
using ApsRT.Core.Taras.TaraSearchEngine;
using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using StageID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, int>;
using OrderID = UniversalPack.Common.CodedObjectVersioned<System.Guid, string, string>;

namespace ApsRT.Core.Tests;

public class PartiasGeneratorEngineTests
{
    static Uco Object(string label) => new(Guid.NewGuid(), label);

    static PartiaModelling Modelling(List<Stage> stages, Uco center, Uco tara = default) =>
        new(stages, tara, 0, DimensionUnit.Unknown, center, null, 0, null!);

    static Stage NewStage(ApsSession session, int index, Uco center)
    {
        var stage = new Stage(session)
        {
            Index = index,
            ID = new StageID(new Uco(Guid.NewGuid(), $"и.5852.1.{index}"), 1),
            OrderLink = new OrderBaseModel { OrderID = new OrderID(new Uco(Guid.NewGuid(), "005852"), "1") },
            OriginalCenter = center,
            Center = center,
            PossibleCenters = new() { center },
            Prevs = new(),
            Nexts = new(),
            Count = 1,
            OriginalCount = 1,
            PlacesCount = 1,
            OriginalPlacesCount = 1,
            RechangeParameters = Array.Empty<ApsRT.Core.Rechanges.StageRechangeParam>()
        };
        session.MasterDataLinks.StagesMasterData.AllStages.Add(index, stage);
        session.MasterDataLinks.CentersMasterData.CentersData.TryAdd(center, new CenterBaseModel { CenterName = center });
        return stage;
    }

    static Dictionary<int, PartiasGeneratorEngine.DeepCheckData> States(List<List<PartiaModelling>> tree) =>
        tree.SelectMany(x => x).SelectMany(x => x.Opers)
            .ToDictionary(x => x.Index, _ => new PartiasGeneratorEngine.DeepCheckData());

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void TerminalFailureKeepsComputedResultAndDoesNotExportSentinel(int predecessor)
    {
        var session = new ApsSession();
        var center = Object("RC");
        var stages = new List<Stage> { NewStage(session, 1, center), NewStage(session, 2, center) };
        var initial = Modelling(stages, Uco.EmptyObject);
        var completed = Modelling(stages, center);
        var tree = new List<List<PartiaModelling>> { new() { initial } };
        var data = new PartiasGeneratorEngineData(stages);
        var states = States(tree);
        var byStage = PartiasConnectorEngine.SeeSdvigsAsDic(tree);
        int calls = 0;

        PartiasGeneratorEngine.CompleteDeepChecks(tree, data, states, byStage, _ =>
        {
            calls++;
            return new(completed, false, predecessor, true);
        });

        Assert.Equal(1, calls);
        Assert.Same(completed, tree[0][0]);
        Assert.All(stages, stage => Assert.Same(completed, byStage[stage.Index]));
        Assert.False(completed.RC.IsEmpty);
        Assert.False(states.ContainsKey(-1));
        Assert.False(data.ExceptTheseTarasGlobal.ContainsKey(-1));
        if (predecessor >= 0) Assert.False(states[predecessor].CanSearchTara);
    }

    [Fact]
    public void RetryRevisitsPredecessorAndPublishesOnlyFinalResult()
    {
        var session = new ApsSession();
        var center = Object("RC");
        var root = NewStage(session, 1, center);
        var child = NewStage(session, 2, center);
        var oldTara = Object("Old tara");
        var newTara = Object("New tara");
        var originalChild = Modelling(new() { child }, Uco.EmptyObject);
        var tree = new List<List<PartiaModelling>>
        {
            new() { Modelling(new() { root }, Uco.EmptyObject) }, new() { originalChild }
        };
        var data = new PartiasGeneratorEngineData(new() { root, child });
        var states = States(tree);
        var byStage = PartiasConnectorEngine.SeeSdvigsAsDic(tree);
        var visits = new List<int>();
        var finalChild = Modelling(new() { child }, center, newTara);
        int rootVisits = 0, childVisits = 0;

        PartiasGeneratorEngine.CompleteDeepChecks(tree, data, states, byStage, current =>
        {
            var index = current.Opers[0].Index;
            visits.Add(index);
            if (index == root.Index)
            {
                rootVisits++;
                var tara = rootVisits == 1 ? oldTara : newTara;
                data.prevs[root.Index] = tara;
                return new(Modelling(new() { root }, center, tara), true, -1, false);
            }
            childVisits++;
            if (childVisits == 1)
                return new(Modelling(new() { child }, Object("Tentative RC")), false, root.Index, false);

            Assert.Same(originalChild, current);
            Assert.Same(originalChild, byStage[child.Index]);
            Assert.Equal(newTara, data.prevs[root.Index]);
            return new(finalChild, true, -1, false);
        });

        Assert.Equal(new[] { 1, 2, 1, 2 }, visits);
        Assert.Same(finalChild, tree[1][0]);
        Assert.Same(finalChild, byStage[child.Index]);
        Assert.Contains(oldTara, states[root.Index].ExceptThisTara);
        Assert.Equal(1, states[root.Index].TryCount);
    }

    [Fact]
    public void TerminalResultSurvivesRetryRequestedByLaterModelling()
    {
        var session = new ApsSession();
        var center = Object("RC");
        var first = NewStage(session, 1, center);
        var second = NewStage(session, 2, center);
        var firstFinal = Modelling(new() { first }, center);
        var tree = new List<List<PartiaModelling>>
        {
            new() { Modelling(new() { first }, Uco.EmptyObject), Modelling(new() { second }, Uco.EmptyObject) }
        };
        var data = new PartiasGeneratorEngineData(new() { first, second });
        var states = States(tree);
        var byStage = PartiasConnectorEngine.SeeSdvigsAsDic(tree);
        int firstCalls = 0, secondCalls = 0;

        PartiasGeneratorEngine.CompleteDeepChecks(tree, data, states, byStage, current =>
        {
            if (current.Opers[0].Index == first.Index)
            {
                if (++firstCalls > 1) Assert.Same(firstFinal, current);
                return new(firstFinal, false, -1, true);
            }
            return ++secondCalls == 1
                ? new(Modelling(new() { second }, center), false, first.Index, false)
                : new(Modelling(new() { second }, center), true, -1, false);
        });

        Assert.Equal(2, firstCalls);
        Assert.Equal(2, secondCalls);
        Assert.Same(firstFinal, tree[0][0]);
        Assert.False(data.ExceptTheseTarasGlobal.ContainsKey(-1));
    }

    sealed class IncompatibleTaras
    {
        public ApsSession Session = new();
        public Stage Previous;
        public Stage Current;
        public Uco OldTara = Object("Incompatible predecessor tara");
        public Uco AcceptedTara = Object("Accepted predecessor tara");
        public Uco FinalTara = Object("Final tara");
        public List<List<PartiaModelling>> Tree;
        public PartiasGeneratorEngineData Data;
        public Dictionary<int, PartiasGeneratorEngine.DeepCheckData> CheckStates;
        public Dictionary<int, PartiaModelling> ByStage;
        public PartiasConnectorEngineResult FinalResult = new(0, 0, 0, int.MaxValue, 0, 0, 0);

        public IncompatibleTaras()
        {
            Session.SettingsController.CurrentSettings.LogErrors = true;
            Session.SettingsController.CurrentSettings.OriginalDurationTime = 1;
            Previous = NewStage(Session, 1, Object("Previous RC"));
            Current = NewStage(Session, 2, Object("Current RC"));
            Current.Prevs.Add(Previous.Index);
            Current.EndTara = FinalTara;
            Session.MasterDataLinks.TarasMasterData.FromAndToTara[Current.OriginalCenter] = new()
            {
                [Uco.EmptyObject] = (
                    new Dictionary<bool, List<Uco>> { [false] = new() { AcceptedTara } },
                    new Dictionary<bool, List<Uco>> { [false] = new() { FinalTara } })
            };
            var previousMod = Modelling(new() { Previous }, Previous.OriginalCenter, OldTara);
            previousMod.ZadelCode = 1; // У предшественника фиксированный задел.
            Tree = new() { new() { previousMod }, new() { Modelling(new() { Current }, Uco.EmptyObject) } };
            Data = new(new() { Previous, Current }) { Color = Color.White, PossibleRCsGlobal = new() };
            Data.prevs[Previous.Index] = OldTara;
            CheckStates = States(Tree);
            CheckStates[Previous.Index].CanSearchTara = false;
            ByStage = PartiasConnectorEngine.SeeSdvigsAsDic(Tree);
            PartiasConnectorEngine.PrepareResultForOneRCData(FinalResult,
                Tree.SelectMany(x => x).Cast<IAbleForTaraCalcInterface>().ToList(), new(), false);
        }

        public PartiasGeneratorEngine.DeepCheckResult Check(PartiaModelling modelling) =>
            PartiasGeneratorEngine.checkThisList(modelling, Data, false, FinalResult, CheckStates, ByStage, Data.PossibleRCsGlobal);

        public void Complete() =>
            PartiasGeneratorEngine.CompleteDeepChecks(Tree, Data, CheckStates, ByStage, Check);
    }

    [Fact]
    public void RealTerminalFailureClearsStaleTaraAndKeepsCenterThroughSessionAndPlanner()
    {
        var fixture = new IncompatibleTaras();
        fixture.Data.prevs[fixture.Current.Index] = Object("Stale current tara");
        fixture.Complete();

        var completed = fixture.Tree[1][0];
        Assert.Equal(fixture.Current.OriginalCenter, completed.RC);
        Assert.True(completed.Tara.IsEmpty);
        Assert.False(fixture.Data.prevs.ContainsKey(fixture.Current.Index));
        Assert.Contains(fixture.Current.Index, fixture.Data.EmptyErroredTarasInfo);
        Assert.True(fixture.Session.DataErrors.NewDataErrors.ContainsKey(fixture.Current.Index));

        var partia = new PartiaClass(fixture.Tree, DateTime.Today, false, false, 0, Guid.NewGuid(), int.MaxValue);
        var algorithm = new PartiasAlgorithm(fixture.Session);
        typeof(PartiasAlgorithm).GetMethod("WriteToSession", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(algorithm, new object?[] { new[] { partia }, null });

        Assert.Equal(completed.RC, fixture.Current.Center);
        Assert.True(algorithm.PartiaHasErrors[partia.MyGuid]);

        var planner = new PlanningAlgorithm(fixture.Session);
        var blocks = (List<PlanSimulatorBlock>)typeof(PlanningAlgorithm)
            .GetMethod("ConvertToModellings", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(planner, new object?[] { new List<Stage> { fixture.Previous, fixture.Current }, null, 1 })!;
        Assert.Equal(2, blocks.Count);
        Assert.Equal(completed.RC, blocks[1].GetCenterData(0).Center);
    }

    [Fact]
    public void PublicDeepCheckLoopCompletesAfterPredecessorSearchIsExhausted()
    {
        var fixture = new IncompatibleTaras();
        fixture.Data.prevs[fixture.Current.Index] = Object("Stale current tara");

        PartiasGeneratorEngine.CheckListWithDeepChecks(fixture.Tree, fixture.Data, false, fixture.FinalResult);

        Assert.Equal(fixture.Current.OriginalCenter, fixture.Tree[1][0].RC);
        Assert.True(fixture.Tree[1][0].Tara.IsEmpty);
        Assert.Contains(fixture.OldTara, fixture.Data.ExceptTheseTarasGlobal[fixture.Previous.Index]);
        Assert.False(fixture.Data.ExceptTheseTarasGlobal.ContainsKey(-1));
        Assert.False(fixture.Data.prevs.ContainsKey(fixture.Current.Index));
    }

    [Fact]
    public void RealRetryDoesNotPublishTentativeTaraOrErrorState()
    {
        var fixture = new IncompatibleTaras();
        fixture.CheckStates[fixture.Previous.Index].CanSearchTara = true;
        var oldCurrentTara = Object("Previously committed current tara");
        fixture.Data.prevs[fixture.Current.Index] = oldCurrentTara;

        var result = fixture.Check(fixture.Tree[1][0]);

        Assert.False(result.success);
        Assert.False(result.last);
        Assert.Equal(fixture.Previous.Index, result.firstPrevIndex);
        Assert.Equal(oldCurrentTara, fixture.Data.prevs[fixture.Current.Index]);
        Assert.DoesNotContain(fixture.Current.Index, fixture.Data.EmptyErroredTarasInfo);
    }

    [Fact]
    public void RealSuccessfulRecheckClearsPreviouslyErroredTaraState()
    {
        var fixture = new IncompatibleTaras();
        fixture.Complete();
        fixture.Data.prevs[fixture.Previous.Index] = fixture.AcceptedTara;

        fixture.Complete();

        Assert.Equal(fixture.FinalTara, fixture.Tree[1][0].Tara);
        Assert.Equal(fixture.Current.OriginalCenter, fixture.Tree[1][0].RC);
        Assert.Equal(fixture.FinalTara, fixture.Data.prevs[fixture.Current.Index]);
        Assert.DoesNotContain(fixture.Current.Index, fixture.Data.EmptyErroredTarasInfo);
    }
}

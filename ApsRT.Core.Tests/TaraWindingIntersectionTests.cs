using ApsRT.Core.Taras.TaraSearchEngine;
using System.Collections.Concurrent;
using UniversalPack;
using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;

namespace ApsRT.Core.Tests;

public class TaraWindingIntersectionTests
{
    private static readonly Guid Cat27Code =
        new("c423f3d0-997e-11e9-9ad4-003048d2cebb");

    private static (Uco Tara, DimensionUnit Format, double Namot) Item(
        Guid code, double capacity, string name = "Катушка 27")
        => (new Uco(code, name), DimensionUnit.m, capacity);

    [Fact]
    public void DuplicateCat27InBothStages_DoesNotThrowAndRestrictsCapacity()
    {
        var previous = new[] { Item(Cat27Code, 100), Item(Cat27Code, 80) };
        var current = new[] { Item(Cat27Code, 60), Item(Cat27Code, 90) };

        var actual = TaraSearchEngine.IntersectAllowedWindings(previous, current);

        // min(max(100,80), max(60,90)) = 90;
        // the previous 100 is rejected, the three other records stay.
        Assert.Equal(new[] { 80d, 60d, 90d }, actual.Select(x => x.Namot));
        Assert.All(actual, x => Assert.Equal(Cat27Code, x.Tara.Code));
    }

    [Fact]
    public void SameGuidDifferentLabels_IsOneTara()
    {
        var previous = new[] { Item(Cat27Code, 100, "Катушка 27") };
        var current = new[] { Item(Cat27Code, 50, "Катушка 27 (1C)") };

        var actual = TaraSearchEngine.IntersectAllowedWindings(previous, current);

        Assert.Single(actual);
        Assert.Equal(50d, actual[0].Namot);
        Assert.Equal(Cat27Code, actual[0].Tara.Code);
    }

    [Fact]
    public void NoCommonTara_ProducesEmptyIntersection()
    {
        var previous = new[] { Item(Cat27Code, 100) };
        var current = new[] { Item(Guid.NewGuid(), 90, "Катушка другая") };

        Assert.Empty(TaraSearchEngine.IntersectAllowedWindings(previous, current));
    }

    [Fact]
    public void MultiStageIntersection_CannotIncreaseAllowedMaximum()
    {
        var previous = new[] { Item(Cat27Code, 100), Item(Cat27Code, 80) };
        var current = new[] { Item(Cat27Code, 90), Item(Cat27Code, 60) };
        var next = new[] { Item(Cat27Code, 75) };

        var afterSecond = TaraSearchEngine.IntersectAllowedWindings(previous, current);
        var afterThird = TaraSearchEngine.IntersectAllowedWindings(afterSecond, next);

        Assert.Equal(new[] { 60d, 75d }, afterThird.Select(x => x.Namot));
    }

    [Fact]
    public void ParallelStages_WithDuplicateCat27_AreIndependent()
    {
        var previous = new[] { Item(Cat27Code, 100), Item(Cat27Code, 80) };
        var current = new[] { Item(Cat27Code, 90), Item(Cat27Code, 60) };

        // Matches the concurrent call context in AlgorithmR.Step2_1_Parallel.
        Parallel.For(0, 1000, _ =>
        {
            var actual = TaraSearchEngine.IntersectAllowedWindings(previous, current);
            Assert.Equal(new[] { 80d, 90d, 60d }, actual.Select(x => x.Namot).Order());
        });
    }
}

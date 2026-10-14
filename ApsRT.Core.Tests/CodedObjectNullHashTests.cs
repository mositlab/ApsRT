using UniversalPack.Common;

namespace ApsRT.Core.Tests;

public class CodedObjectNullHashTests
{
    [Fact]
    public void NullCode_HasStableHashAndDoesNotThrowDuringEquality()
    {
        var first = new CodedObject<string, string>(null!, "Пустой код");
        var second = new CodedObject<string, string>(null!, "Другой ярлык");

        Assert.True(first.IsEmpty);
        Assert.Equal(0, first.GetHashCode());
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.Equal(first, second);
    }
}

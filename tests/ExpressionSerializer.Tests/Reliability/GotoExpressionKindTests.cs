using System;
using System.Linq.Expressions;
using Xunit;

namespace ExpressionSerializer.Tests.Reliability;

public sealed class GotoExpressionKindTests
{
    [Fact]
    public void TestGotoExpressionKindEnumVariants()
    {
        // Since `System.Linq.Expressions` is effectively archived, we can be confident that very few changes will take place in the future.
        // See https://github.com/dotnet/runtime/blob/20761d0beaefce8c3de502bc6059ec62e984ebf5/src/libraries/System.Linq.Expressions/README.md.
        // This Fact verifies that no breaking changes have occurred.
        Assert.Equal(4, Enum.GetValues(typeof(GotoExpressionKind)).Length);

        // This checks if the order in the enum has remained the same. This is important because our protocol is fixed.
        Assert.Equal(0, (int)GotoExpressionKind.Goto);
        Assert.Equal(1, (int)GotoExpressionKind.Return);
        Assert.Equal(2, (int)GotoExpressionKind.Break);
        Assert.Equal(3, (int)GotoExpressionKind.Continue);
    }
}
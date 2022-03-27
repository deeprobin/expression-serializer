using System.Linq.Expressions;
using ExpressionSerializer.Tests.Infrastructure;
using Xunit;

namespace ExpressionSerializer.Tests.SerializationHandlers;

public sealed class ConstantExpressionHandlerTests : SerializationTestBase
{
    public ConstantExpressionHandlerTests(SerializerFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void TestPrimitiveSerialization()
    {
        var expression = Expression.Constant(10);

        var actual = Serialize(expression);
        var expected = new byte[] {
            (int)ExpressionType.Constant, // Node Type: 9 
            5, // Type Id: 5
            1, // Type `HasValue` 
            10, 0, 0, 0 // Binary Little-Endian representation of 10
        };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestPrimitiveDeserialization()
    {
        var bytes = new byte[] {
            (int)ExpressionType.Constant, // Node Type: 9 
            5, // Type Id: 5
            1, // Type `HasValue` 
            12, 0, 0, 0 // Binary Little-Endian representation of 12
        };

        var actual = Deserialize(bytes);
        var expected = Expression.Constant(12);

        Assert.Equal(expected, actual, ExpressionEqualityComparer.Instance);
    }
}
using System.IO;
using System.Linq.Expressions;
using ExpressionSerializer.Tests.Infrastructure;
using Xunit;

namespace ExpressionSerializer.Tests.SerializationHandlers;

public sealed class BinaryExpressionHandlerTests : SerializationTestBase
{
    public BinaryExpressionHandlerTests(SerializerFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void TestSerialization()
    {
        var expression = Expression.Add(Expression.Constant(10), Expression.Constant(20));

        var actual = Serialize(expression);
        var expected = new byte[]
        {
            (int)ExpressionType.Add, // Node Type: 0
            
            // Left Sub-Expression
            (int)ExpressionType.Constant, // Node Type: 9 
            5, // Type Id: 5
            1, // Type `HasValue` 
            10, 0, 0, 0, // Binary Little-Endian representation of 10

            // Right Sub-Expression
            (int)ExpressionType.Constant, // Node Type: 9 
            5, // Type Id: 5
            1, // Type `HasValue` 
            20, 0, 0, 0 // Binary Little-Endian representation of 20
        };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestDeserialization()
    {
        var bytes = new byte[]
        {
            (int)ExpressionType.Subtract, // Node Type: 42
            
            // Left Sub-Expression
            (int)ExpressionType.Constant, // Node Type: 9 
            5, // Type Id: 5
            1, // Type `HasValue` 
            3, 0, 0, 0, // Binary Little-Endian representation of 3

            // Right Sub-Expression
            (int)ExpressionType.Constant, // Node Type: 9 
            5, // Type Id: 5
            1, // Type `HasValue` 
            2, 0, 0, 0 // Binary Little-Endian representation of 2
        };

        var expected = Expression.Subtract(Expression.Constant(3), Expression.Constant(2));
        var actual = Deserialize(bytes);

        Assert.Equal(expected, actual, ExpressionEqualityComparer.Instance);
    }
}
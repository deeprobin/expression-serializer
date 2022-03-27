using System.Linq.Expressions;
using ExpressionSerializer.Tests.Infrastructure;
using Xunit;

namespace ExpressionSerializer.Tests;

public sealed class ExpressionSerializerTests : SerializationTestBase
{
    public ExpressionSerializerTests(SerializerFixture fixture) : base(fixture)
    {
    }

    public static readonly TheoryData<Expression> ExpressionMemberData = new()
    {
        Expression.Constant(1),
        Expression.Add(Expression.Constant(10), Expression.Constant(200))
    };

    [Theory]
    [MemberData(nameof(ExpressionMemberData), MemberType = typeof(ExpressionSerializerTests))]
    public void TestSerializationWorkflow(Expression initialExpression)
    {
        var serializedBytes = Serialize(initialExpression);
        var deserialized = Deserialize(serializedBytes);

        Assert.Equal(initialExpression, deserialized, ExpressionEqualityComparer.Instance);
    }
}
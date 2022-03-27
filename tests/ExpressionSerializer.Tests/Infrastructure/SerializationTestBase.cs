using System.IO;
using System.Linq.Expressions;
using Xunit;

namespace ExpressionSerializer.Tests.Infrastructure;

public abstract class SerializationTestBase : IClassFixture<SerializerFixture>
{
    protected readonly SerializerFixture Fixture;

    protected SerializationTestBase(SerializerFixture fixture)
    {
        Fixture = fixture;
    }

    protected byte[] Serialize(Expression expression)
    {
        using var stream = new MemoryStream();
        Fixture.ExpressionSerializer.Serialize(expression, stream);
        return stream.ToArray();
    }

    protected Expression Deserialize(byte[] array)
    {
        using var stream = new MemoryStream(array);
        return Fixture.ExpressionSerializer.Deserialize(stream);
    }
}
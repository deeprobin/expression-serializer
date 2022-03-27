namespace ExpressionSerializer.Tests.Infrastructure;

public sealed class SerializerFixture
{
    public ExpressionSerializer ExpressionSerializer { get; }

    public SerializerFixture()
    {
        ExpressionSerializer = new ExpressionSerializer();
    }
}
using System.Linq.Expressions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class DynamicExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Dynamic;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        throw new NotSupportedException();
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        throw new NotSupportedException();
    }
}
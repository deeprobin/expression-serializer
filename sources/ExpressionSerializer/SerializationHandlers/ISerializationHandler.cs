using System.Linq.Expressions;

namespace ExpressionSerializer.SerializationHandlers;

internal interface ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type);

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream);

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream);
}
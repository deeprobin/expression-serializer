using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class DefaultExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Default;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var defaultType = stream.ReadType();
        return Expression.Default(defaultType);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is DefaultExpression);
        var defaultExpression = (DefaultExpression)expression;
        stream.WriteType(defaultExpression.Type);
    }
}
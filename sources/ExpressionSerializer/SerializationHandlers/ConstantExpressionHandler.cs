using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class ConstantExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Constant;

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is ConstantExpression);
        var constantExpression = (ConstantExpression)expression;
        stream.WriteValue(constantExpression.Type, constantExpression.Value);
    }

    Expression ISerializationHandler.ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var value = stream.ReadValue();
        return Expression.Constant(value);
    }
}
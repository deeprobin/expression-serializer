using System.Diagnostics;
using System.Linq.Expressions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class ConditionalExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Conditional;

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is ConditionalExpression);
        var conditionalExpression = (ConditionalExpression)expression;
        handler.Serialize(conditionalExpression.Test, stream);
        handler.Serialize(conditionalExpression.IfTrue, stream);
        handler.Serialize(conditionalExpression.IfFalse, stream);
    }

    Expression ISerializationHandler.ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var testExpression = handler.Deserialize(stream);
        var ifTrueExpression = handler.Deserialize(stream);
        var ifFalseExpression = handler.Deserialize(stream);

        return Expression.Condition(testExpression, ifTrueExpression, ifFalseExpression);
    }
}
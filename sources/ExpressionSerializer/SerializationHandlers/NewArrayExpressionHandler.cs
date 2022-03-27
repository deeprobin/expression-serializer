using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class NewArrayExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) =>
        type is ExpressionType.NewArrayInit or ExpressionType.NewArrayBounds;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var t = stream.ReadType();
        var expressionAmount = (int)stream.ReadUInt32();
        var expressions = new Expression[expressionAmount];
        for (var i = 0; i < expressions.Length; i++)
        {
            expressions[i] = handler.Deserialize(stream);
        }

        return type switch
        {
            ExpressionType.NewArrayInit => Expression.NewArrayInit(t, expressions),
            ExpressionType.NewArrayBounds => Expression.NewArrayBounds(t, expressions),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is NewArrayExpression);
        var newArrayExpression = (NewArrayExpression)expression;

        stream.WriteType(newArrayExpression.Type);

        var expressions = newArrayExpression.Expressions;
        stream.Write((ushort)expressions.Count);
        foreach (var subExpression in expressions)
        {
            handler.Serialize(subExpression, stream);
        }
    }
}
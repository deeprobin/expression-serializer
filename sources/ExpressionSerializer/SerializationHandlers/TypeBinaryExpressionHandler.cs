using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class TypeBinaryExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.TypeIs or ExpressionType.TypeEqual;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var expression = handler.Deserialize(stream);
        var t = stream.ReadType();

        return type switch
        {
            ExpressionType.TypeIs => Expression.TypeIs(expression, t),
            ExpressionType.TypeAs => Expression.TypeAs(expression, t),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is TypeBinaryExpression);
        var typeBinaryExpression = (TypeBinaryExpression)expression;

        handler.Serialize(typeBinaryExpression.Expression, stream);
        stream.WriteType(typeBinaryExpression.TypeOperand);
    }
}
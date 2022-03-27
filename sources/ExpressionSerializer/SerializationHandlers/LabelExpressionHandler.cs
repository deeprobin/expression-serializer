using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class LabelExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Label;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var label = stream.ReadLabel();
        var defaultValuePresent = (byte)stream.ReadByte() != 0;
        Expression? defaultValue = default;
        if (defaultValuePresent)
        {
            defaultValue = handler.Deserialize(stream);
        }

        return Expression.Label(label, defaultValue);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is LabelExpression);
        var labelExpression = (LabelExpression)expression;

        stream.WriteLabel(labelExpression.Target);
        if (labelExpression.DefaultValue is { } defaultValue)
        {
            stream.WriteByte(1);
            handler.Serialize(defaultValue, stream);
        }
        else
        {
            stream.WriteByte(0);
        }
    }
}
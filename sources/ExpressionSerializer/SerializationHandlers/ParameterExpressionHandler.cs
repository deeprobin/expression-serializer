using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class ParameterExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Parameter;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var t = stream.ReadType();
        var hasName = (byte)stream.ReadByte() != 0;

        string? name = default;
        if (hasName)
        {
            name = stream.Read16BitPrefixed().ToString();
        }

        return Expression.Parameter(t, name);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is ParameterExpression);
        var parameterExpression = (ParameterExpression)expression;

        stream.WriteType(parameterExpression.Type);
        if (parameterExpression.Name is { } name)
        {
            stream.WriteByte(1);
            stream.Write16BitPrefixed(name);
        } else
        {
            stream.WriteByte(0);
        }
    }
}
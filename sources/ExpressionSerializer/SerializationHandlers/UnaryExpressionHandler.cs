using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class UnaryExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type
        is ExpressionType.Negate
        or ExpressionType.NegateChecked
        or ExpressionType.Not
        or ExpressionType.IsFalse
        or ExpressionType.IsTrue
        or ExpressionType.OnesComplement
        or ExpressionType.ArrayLength
        or ExpressionType.Convert
        or ExpressionType.ConvertChecked
        or ExpressionType.Throw
        or ExpressionType.TypeAs
        or ExpressionType.Quote
        or ExpressionType.UnaryPlus
        or ExpressionType.Unbox
        or ExpressionType.Increment
        or ExpressionType.Decrement
        or ExpressionType.PreIncrementAssign
        or ExpressionType.PostIncrementAssign
        or ExpressionType.PreDecrementAssign
        or ExpressionType.PostDecrementAssign;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var t = stream.ReadType();

        var hasMethod = (byte)stream.ReadByte() != 0;
        MethodInfo? methodInfo = default;
        if (hasMethod)
        {
            methodInfo = stream.ReadMethodInfo();
        }

        var operand = handler.Deserialize(stream);
        return Expression.MakeUnary(type, operand, t, methodInfo);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is UnaryExpression);
        var unaryExpression = (UnaryExpression)expression;

        stream.WriteType(unaryExpression.Type);

        if (unaryExpression.Method is { } method)
        {
            stream.WriteByte(1);
            stream.WriteMethodInfo(method);
        }
        else
        {
            stream.WriteByte(0);
        }

        handler.Serialize(unaryExpression.Operand, stream);
    }
}
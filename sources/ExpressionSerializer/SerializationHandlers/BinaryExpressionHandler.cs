using System.Diagnostics;
using System.Linq.Expressions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class BinaryExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type
        is ExpressionType.Add
        or ExpressionType.AddChecked
        or ExpressionType.Subtract
        or ExpressionType.SubtractChecked
        or ExpressionType.Multiply
        or ExpressionType.MultiplyChecked
        or ExpressionType.Divide
        or ExpressionType.Modulo
        or ExpressionType.Power
        or ExpressionType.And
        or ExpressionType.AndAlso
        or ExpressionType.Or
        or ExpressionType.OrElse
        or ExpressionType.LessThan
        or ExpressionType.LessThanOrEqual
        or ExpressionType.GreaterThan
        or ExpressionType.GreaterThanOrEqual
        or ExpressionType.Equal
        or ExpressionType.NotEqual
        or ExpressionType.ExclusiveOr
        or ExpressionType.Coalesce
        or ExpressionType.ArrayIndex
        or ExpressionType.RightShift
        or ExpressionType.LeftShift
        or ExpressionType.Assign
        or ExpressionType.AddAssign
        or ExpressionType.AndAssign
        or ExpressionType.DivideAssign
        or ExpressionType.ExclusiveOrAssign
        or ExpressionType.LeftShiftAssign
        or ExpressionType.ModuloAssign
        or ExpressionType.MultiplyAssign
        or ExpressionType.OrAssign
        or ExpressionType.PowerAssign
        or ExpressionType.RightShiftAssign
        or ExpressionType.SubtractAssign
        or ExpressionType.AddAssignChecked
        or ExpressionType.SubtractAssignChecked
        or ExpressionType.MultiplyAssignChecked;

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is BinaryExpression);
        var binaryExpression = (BinaryExpression)expression;

        handler.Serialize(binaryExpression.Left, stream);
        handler.Serialize(binaryExpression.Right, stream);
    }

    Expression ISerializationHandler.ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var left = handler.Deserialize(stream);
        var right = handler.Deserialize(stream);

        return Expression.MakeBinary(type, left, right);
    }
}
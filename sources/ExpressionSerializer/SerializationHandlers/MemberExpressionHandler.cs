using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class MemberExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.MemberAccess;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var hasExpression = (byte)stream.ReadByte() != 0;
        Expression? expression = default;
        if (hasExpression)
        {
            expression = handler.Deserialize(stream);
        }

        var memberInfo = stream.ReadMemberInfo();
        return Expression.MakeMemberAccess(expression, memberInfo);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is MemberExpression);
        var memberExpression = (MemberExpression)expression;

        if (memberExpression.Expression is { } subExpression)
        {
            stream.WriteByte(1);
            handler.Serialize(subExpression, stream);
        } else
        {
            stream.WriteByte(0);
        }

        stream.WriteMemberInfo(memberExpression.Member);
    }
}
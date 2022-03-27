using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class GotoExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Goto;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var kind = (GotoExpressionKind)(byte)stream.ReadByte();
        var hasSubExpr = stream.ReadByte() != 0;

        Expression? subExpr = default;
        if (hasSubExpr)
        {
            subExpr = handler.Deserialize(stream);
        }

        var target = stream.ReadLabel();
        var gotoType = stream.ReadType();

        return Expression.MakeGoto(kind, target, subExpr, gotoType);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is GotoExpression);
        var gotoExpression = (GotoExpression)expression;
        
        stream.WriteByte((byte)(int)gotoExpression.Kind);
        if (gotoExpression.Value is { } subExpression)
        {
            stream.WriteByte(1);
            handler.Serialize(subExpression, stream);
        }
        else
        {
            stream.WriteByte(0);
        }
        stream.WriteLabel(gotoExpression.Target);
        stream.WriteType(gotoExpression.Type);
    }
}
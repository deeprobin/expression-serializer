using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class LoopExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Loop;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var hasBreakLabel = (byte)stream.ReadByte() != 0;
        LabelTarget? breakLabel = default;

        if (hasBreakLabel)
        {
            breakLabel = stream.ReadLabel();
        }

        var hasContinueLabel = (byte)stream.ReadByte() != 0;
        LabelTarget? continueLabel = default;

        if (hasContinueLabel)
        {
            continueLabel = stream.ReadLabel();
        }

        var body = handler.Deserialize(stream);
        return Expression.Loop(body, breakLabel, continueLabel);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is LoopExpression);
        var loopExpression = (LoopExpression)expression;

        if (loopExpression.BreakLabel is { } breakLabel)
        {
            stream.WriteByte(1);
            stream.WriteLabel(breakLabel);
        }
        else
        {
            stream.WriteByte(0);
        }

        if (loopExpression.ContinueLabel is { } continueLabel)
        {
            stream.WriteByte(1);
            stream.WriteLabel(continueLabel);
        }
        else
        {
            stream.WriteByte(0);
        }

        handler.Serialize(loopExpression.Body, stream);
    }
}
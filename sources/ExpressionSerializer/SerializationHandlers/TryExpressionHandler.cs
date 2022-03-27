using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class TryExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Try;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var body = handler.Deserialize(stream);
        var hasFaultBlock = (byte)stream.ReadByte() != 0;
        Expression? faultBlock = default;
        if (hasFaultBlock)
        {
            faultBlock = handler.Deserialize(stream);
        }

        var hasFinallyBlock = (byte)stream.ReadByte() != 0;
        Expression? finallyBlock = default;
        if (hasFinallyBlock)
        {
            finallyBlock = handler.Deserialize(stream);
        }

        var catchBlockAmount = (int)stream.ReadUInt32();
        var catchBlocks = new ReadOnlyCollectionBuilder<CatchBlock>(catchBlockAmount);
        for (var i = 0; i < catchBlockAmount; i++)
        {
            var catchBlock = ReadCatchBlock(handler, stream);
            catchBlocks.Add(catchBlock);
        }

        return Expression.MakeTry(null, body, finallyBlock, faultBlock, catchBlocks);
    }

    private static CatchBlock ReadCatchBlock(ExpressionSerializer handler, Stream stream)
    {
        var test = stream.ReadType();
        var body = handler.Deserialize(stream);

        var hasVariable = (byte)stream.ReadByte() != 0;
        ParameterExpression? variable = default;
        if (hasVariable)
        {
            variable = (ParameterExpression) handler.DeserializeCore(ExpressionType.Parameter, stream);
        }

        var hasFilter = (byte)stream.ReadByte() != 0;
        Expression? filter = default;
        if (hasFilter)
        {
            filter = handler.Deserialize(stream);
        }

        return Expression.MakeCatchBlock(test, variable, body, filter);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is TryExpression);
        var tryExpression = (TryExpression)expression;

        handler.Serialize(tryExpression.Body, stream);

        if (tryExpression.Fault is { } faultBlock)
        {
            stream.WriteByte(1);
            handler.Serialize(faultBlock, stream);
        }
        else
        {
            stream.WriteByte(0);
        }

        if (tryExpression.Finally is { } finallyBlock)
        {
            stream.WriteByte(1);
            handler.Serialize(finallyBlock, stream);
        }
        else
        {
            stream.WriteByte(0);
        }

        var catchBlocks = tryExpression.Handlers;
        stream.Write((ushort)catchBlocks.Count);
        foreach (var catchBlock in catchBlocks)
        {
            WriteCatchBlock(handler, catchBlock, stream);
        }
    }

    private static void WriteCatchBlock(ExpressionSerializer handler, CatchBlock catchBlock, Stream stream)
    {
        stream.WriteType(catchBlock.Test);
        handler.Serialize(catchBlock.Body, stream);

        if (catchBlock.Variable is { } variable)
        {
            stream.WriteByte(1);
            handler.SerializeCore(variable, stream);
        }
        else
        {
            stream.WriteByte(0);
        }

        if (catchBlock.Filter is { } filter)
        {
            stream.WriteByte(1);
            handler.Serialize(filter, stream);
        }
        else
        {
            stream.WriteByte(0);
        }
    }
}
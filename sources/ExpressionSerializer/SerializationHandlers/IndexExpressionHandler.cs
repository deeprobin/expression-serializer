using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class IndexExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Index;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var obj = handler.Deserialize(stream);

        var hasIndexer = (byte)stream.ReadByte() != 0;
        PropertyInfo? indexer = default;
        if (hasIndexer)
        {
            indexer = stream.ReadPropertyInfo();
        }

        var argumentCount = (int)stream.ReadUInt16();
        var arguments = new Expression[argumentCount];
        for (var i = 0; i < argumentCount; i++)
        {
            arguments[i] = handler.Deserialize(stream);
        }

        return Expression.MakeIndex(obj, indexer, arguments);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is IndexExpression);
        var indexExpression = (IndexExpression)expression;

        handler.Serialize(indexExpression.Object!, stream);
        if (indexExpression.Indexer is { } indexer)
        {
            stream.WriteByte(1);
            stream.WritePropertyInfo(indexer);
        } else
        {
            stream.WriteByte(0);
        }

        var arguments = indexExpression.Arguments;
        stream.Write((ushort)arguments.Count);
        foreach (var argumentExpression in arguments)
        {
            handler.Serialize(argumentExpression, stream);
        }
    }
}
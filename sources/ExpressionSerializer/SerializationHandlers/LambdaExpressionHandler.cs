using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class LambdaExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Lambda;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var hasName = (byte)stream.ReadByte() != 0;
        string? name = default;

        if (hasName)
        {
            name = stream.Read16BitPrefixed().ToString();
        }

        var tailCall = (byte)stream.ReadByte() != 0;

        var parameterCount = (int)stream.ReadUInt16();
        var collectionBuilder = new ReadOnlyCollectionBuilder<ParameterExpression>(parameterCount);
        for (var i = 0; i < parameterCount; i++)
        {
            var parameterExpression = handler.DeserializeCore(ExpressionType.Parameter, stream);
            Debug.Assert(parameterExpression is ParameterExpression);
            collectionBuilder.Add((ParameterExpression)parameterExpression);
        }

        var body = handler.Deserialize(stream);

        return Expression.Lambda(body, name, tailCall, collectionBuilder.ToReadOnlyCollection());
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is LambdaExpression);
        var lambdaExpression = (LambdaExpression)expression;

        if (lambdaExpression.Name is { } name)
        {
            stream.WriteByte(1);
            stream.Write16BitPrefixed(name);
        }
        else
        {
            stream.WriteByte(0);
        }
        stream.WriteByte(lambdaExpression.TailCall ? (byte)1 : (byte)0);

        var parameters = lambdaExpression.Parameters;
        stream.Write((ushort)parameters.Count);
        foreach (var parameterExpression in parameters)
        {
            // Serialize core is enough as we know this is a `ParameterExpression`
            handler.SerializeCore(parameterExpression, stream);
        }

        handler.Serialize(lambdaExpression.Body, stream);
    }
}
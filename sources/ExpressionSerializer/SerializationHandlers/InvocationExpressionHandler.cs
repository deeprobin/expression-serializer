using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class InvocationExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Invoke;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var expression = handler.Deserialize(stream);
        var argumentCount = (int)stream.ReadUInt16();

        var collectionBuilder = new ReadOnlyCollectionBuilder<Expression>(argumentCount);
        for (var i = 0; i < argumentCount; i++)
        {
            var argumentExpression = handler.Deserialize(stream);
            collectionBuilder.Add(argumentExpression);
        }

        return Expression.Invoke(expression, collectionBuilder.ToReadOnlyCollection());
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is InvocationExpression);
        var invocationExpression = (InvocationExpression)expression;
        handler.Serialize(invocationExpression.Expression, stream);

        var arguments = invocationExpression.Arguments;
        stream.Write((ushort)arguments.Count);

        foreach (var argumentExpression in arguments)
        {
            handler.Serialize(argumentExpression, stream);
        }
    }
}
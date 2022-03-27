using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class BlockExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) 
        => type is ExpressionType.Block;

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is BlockExpression);
        var blockExpression = (BlockExpression)expression;

        var variables = blockExpression.Variables;
        stream.Write((uint)variables.Count);
        foreach (var parameterExpression in variables)
        {
            // We know that this is a `ParameterExpression` so we don't need to serialize the full Expression inclusive Type-Prefix
            handler.SerializeCore(parameterExpression, stream);
        }

        var expressions = blockExpression.Expressions;
        stream.Write((uint)expressions.Count);
        foreach (var exp in expressions)
        {
            handler.Serialize(exp, stream);
        }
    }

    Expression ISerializationHandler.ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var variableLength = stream.ReadUInt32();
        var variableBuilder = new ReadOnlyCollectionBuilder<ParameterExpression>((int)variableLength);
        for (var i = 0; i < variableLength; i++)
        {
            var parameterExpression = (ParameterExpression)handler.DeserializeCore(ExpressionType.Parameter, stream);
            variableBuilder.Add(parameterExpression);
        }

        var variables = variableBuilder.ToReadOnlyCollection();

        var expressionLength = stream.ReadUInt32();
        var expressionBuilder = new ReadOnlyCollectionBuilder<Expression>((int)expressionLength);
        for (var i = 0; i < variableLength; i++)
        {
            var expression = handler.Deserialize(stream);
            expressionBuilder.Add(expression);
        }

        var expressions = expressionBuilder.ToReadOnlyCollection();

        return Expression.Block(variables, expressions);
    }
}
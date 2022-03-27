using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class ListInitExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.ListInit;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var newExpression = (NewExpression) handler.DeserializeCore(ExpressionType.New, stream);
        var initializersCount = (int)stream.ReadUInt16();

        var initializers = new ReadOnlyCollectionBuilder<ElementInit>(initializersCount);
        for (var i = 0; i < initializersCount; i++)
        {
            var methodInfo = stream.ReadMethodInfo();

            var argumentCount = (int)stream.ReadUInt16();
            var arguments = new ReadOnlyCollectionBuilder<Expression>(argumentCount);
            for (var j = 0; j < argumentCount; j++)
            {
                var argument = handler.Deserialize(stream);
                arguments.Add(argument);
            }

            var initializer = Expression.ElementInit(methodInfo, arguments);
            initializers.Add(initializer);
        }

        return Expression.ListInit(newExpression, initializers);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is ListInitExpression);
        var listInitExpression = (ListInitExpression) expression;

        handler.SerializeCore(listInitExpression.NewExpression, stream);
        var initializers = listInitExpression.Initializers;
        stream.Write((ushort)initializers.Count);

        foreach (var initializer in initializers)
        {
            stream.WriteMethodInfo(initializer.AddMethod);

            var arguments = initializer.Arguments;
            stream.Write((ushort)arguments.Count);
            foreach (var argument in arguments)
            {
                handler.Serialize(argument, stream);
            }
        }
    }
}
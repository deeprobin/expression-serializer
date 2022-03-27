using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class MethodCallExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Call;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var hasObject = (byte)stream.ReadByte() != 0;
        Expression? obj = default;
        if (hasObject)
        {
            obj = handler.Deserialize(stream);
        }

        var methodInfo = stream.ReadMethodInfo();

        var argumentCount = (int)stream.ReadUInt16();
        var arguments = new ReadOnlyCollectionBuilder<Expression>(argumentCount);
        for (var i = 0; i < argumentCount; i++)
        {
            var argumentExpression = handler.Deserialize(stream);
            arguments.Add(argumentExpression);
        }

        return Expression.Call(obj, methodInfo, arguments.ToReadOnlyCollection());
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is MethodCallExpression);
        var methodCallExpression = (MethodCallExpression)expression;

        if (methodCallExpression.Object is { } obj)
        {
            stream.WriteByte(1);
            handler.Serialize(obj, stream);
        }
        else
        {
            stream.WriteByte(0);
        }

        stream.WriteMethodInfo(methodCallExpression.Method);

        var arguments = methodCallExpression.Arguments;
        stream.Write((ushort)arguments.Count);
        foreach (var argument in arguments)
        {
            handler.Serialize(argument, stream);
        }
    }
}
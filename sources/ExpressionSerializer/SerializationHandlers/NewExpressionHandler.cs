using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class NewExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.New;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var ctorInfo = stream.ReadConstructorInfo();

        var argumentCount = (int)stream.ReadUInt16();
        var arguments = new Expression[argumentCount];
        for (var i = 0; i < argumentCount; i++)
        {
            arguments[i] = handler.Deserialize(stream);
        }

        return Expression.New(ctorInfo, arguments);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is NewExpression);
        var newExpression = (NewExpression)expression;

        stream.WriteConstructorInfo(newExpression.Constructor!);

        var arguments = newExpression.Arguments;
        stream.Write((ushort)arguments.Count);
        foreach (var argument in arguments)
        {
            handler.Serialize(argument, stream);
        }
    }
}
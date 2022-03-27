using System.Diagnostics;
using System.Linq.Expressions;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class RuntimeVariablesExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.RuntimeVariables;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var variableAmount = (int)stream.ReadUInt16();
        var variables = new ParameterExpression[variableAmount];
        for (var i = 0; i < variableAmount; i++)
        {
            variables[i] = (ParameterExpression)handler.DeserializeCore(ExpressionType.Parameter, stream);
        }

        return Expression.RuntimeVariables(variables);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is RuntimeVariablesExpression);
        var runtimeVariablesExpression = (RuntimeVariablesExpression)expression;
        var variables = runtimeVariablesExpression.Variables;

        stream.Write((ushort)variables.Count);
        foreach (var parameterExpression in variables)
        {
            handler.SerializeCore(parameterExpression, stream);
        }
    }
}
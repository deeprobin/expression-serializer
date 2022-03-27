using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class SwitchExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.Switch;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var switchValue = handler.Deserialize(stream);

        var hasDefaultBody = (byte)stream.ReadByte() != 0;
        Expression? defaultBody = default;
        if (hasDefaultBody)
        {
            defaultBody = handler.Deserialize(stream);
        }

        var hasComparison = (byte)stream.ReadByte() != 0;
        MethodInfo? comparison = default;
        if (hasComparison)
        {
            comparison = stream.ReadMethodInfo();
        }

        var casesAmount = (int)stream.ReadUInt16();
        var cases = new SwitchCase[casesAmount];
        for (var i = 0; i < casesAmount; i++)
        {
            cases[i] = ReadSwitchCase(handler, stream);
        }

        return Expression.Switch(switchValue, defaultBody, comparison, cases);
    }

    private static SwitchCase ReadSwitchCase(ExpressionSerializer handler, Stream stream)
    {
        var testValuesLength = (int)stream.ReadUInt16();
        var testValues = new Expression[testValuesLength];
        for (var i = 0; i < testValuesLength; i++)
        {
            testValues[i] = handler.Deserialize(stream);
        }

        var body = handler.Deserialize(stream);
        return Expression.SwitchCase(body, testValues);
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is SwitchExpression);
        var switchExpression = (SwitchExpression)expression;

        handler.Serialize(switchExpression.SwitchValue, stream);
        if (switchExpression.DefaultBody is { } defaultBody)
        {
            stream.WriteByte(1);
            handler.Serialize(defaultBody, stream);
        }
        else
        {
            stream.WriteByte(0);
        }

        if (switchExpression.Comparison is { } comparision)
        {
            stream.WriteByte(1);
            stream.WriteMethodInfo(comparision);
        }
        else
        {
            stream.WriteByte(0);
        }

        var cases = switchExpression.Cases;
        stream.Write((ushort)cases.Count);
        foreach (var switchCase in cases)
        {
            WriteSwitchCase(handler, switchCase, stream);
        }
    }

    private static void WriteSwitchCase(ExpressionSerializer handler, SwitchCase switchCase, Stream stream)
    {
        var testValues = switchCase.TestValues;
        stream.Write((ushort)testValues.Count);
        foreach (var testValue in testValues)
        {
            handler.Serialize(testValue, stream);
        }

        handler.Serialize(switchCase.Body, stream);
    }
}
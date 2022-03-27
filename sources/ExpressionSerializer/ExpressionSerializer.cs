using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ExpressionSerializer.SerializationHandlers;

namespace ExpressionSerializer;

public sealed class ExpressionSerializer
{
    private readonly IReadOnlyCollection<ISerializationHandler> _serializationHandlers;

    public ExpressionSerializer()
    {
        _serializationHandlers = new ReadOnlyCollectionBuilder<ISerializationHandler>
            {
                new BinaryExpressionHandler(),
                new BlockExpressionHandler(),
                new ConditionalExpressionHandler(),
                new ConstantExpressionHandler(),
                new DefaultExpressionHandler(),
                new DebugInfoExpressionHandler(),
                new DynamicExpressionHandler(),
                new IndexExpressionHandler(),
                new InvocationExpressionHandler(),
                new LabelExpressionHandler(),
                new LambdaExpressionHandler(),
                new ListInitExpressionHandler(),
                new LoopExpressionHandler(),
                new MemberExpressionHandler(),
                new MemberInitExpressionHandler(),
                new MethodCallExpressionHandler(),
                new NewArrayExpressionHandler(),
                new NewExpressionHandler(),
                new ParameterExpressionHandler(),
                new RuntimeVariablesExpressionHandler(),
                new SwitchExpressionHandler(),
                new TryExpressionHandler(),
                new TypeBinaryExpressionHandler(),
                new UnaryExpressionHandler()
            }
            .ToReadOnlyCollection();
    }

    public Expression Deserialize(Stream stream)
    {
        var expressionType = (ExpressionType)(byte)stream.ReadByte();
        return DeserializeCore(expressionType, stream);
    }

    public void Serialize(Expression expression, Stream stream)
    {
        stream.WriteByte((byte)(int)expression.NodeType);
        SerializeCore(expression, stream);
    }

    private ISerializationHandler GetRightHandlerForType(ExpressionType expressionType)
    {
        foreach (var serializationHandler in _serializationHandlers)
        {
            if (serializationHandler.IsTypeApplicable(expressionType))
            {
                return serializationHandler;
            }
        }

        throw new InvalidOperationException(
            $"No {nameof(ISerializationHandler)} is registered for {expressionType}");
    }

    public void SerializeCore(Expression expression, Stream stream)
    {
        var serializationHandler = GetRightHandlerForType(expression.NodeType);
        serializationHandler.WriteExpression(this, expression, stream);
    }

    public Expression DeserializeCore(ExpressionType expressionType, Stream stream)
    {
        var serializationHandler = GetRightHandlerForType(expressionType);
        return serializationHandler.ReadExpression(this, expressionType, stream);
    }
}
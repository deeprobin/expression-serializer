using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ExpressionSerializer.Extensions;

namespace ExpressionSerializer.SerializationHandlers;

internal sealed class MemberInitExpressionHandler : ISerializationHandler
{
    public bool IsTypeApplicable(ExpressionType type) => type is ExpressionType.MemberInit;

    public Expression ReadExpression(ExpressionSerializer handler, ExpressionType type, Stream stream)
    {
        var newExpression = (NewExpression)handler.DeserializeCore(ExpressionType.New, stream);

        var bindingsCount = (int)stream.ReadUInt16();
        var bindings = new ReadOnlyCollectionBuilder<MemberBinding>(bindingsCount);
        for (var i = 0; i < bindingsCount; i++)
        {
            var memberBinding = ReadMemberBinding(handler, stream);
            bindings.Add(memberBinding);
        }

        return Expression.MemberInit(newExpression, bindings.ToReadOnlyCollection());
    }

    public void WriteExpression(ExpressionSerializer handler, Expression expression, Stream stream)
    {
        Debug.Assert(expression is MemberInitExpression);
        var memberInitExpression = (MemberInitExpression)expression;

        handler.SerializeCore(memberInitExpression.NewExpression, stream);
        var bindings = memberInitExpression.Bindings;

        stream.Write((ushort)bindings.Count);
        foreach (var memberBinding in bindings)
        {
            WriteMemberBinding(handler, stream, memberBinding);
        }
    }

    private static MemberBinding ReadMemberBinding(ExpressionSerializer handler, Stream stream)
    {
        var type = (MemberBindingType)stream.ReadByte();
        var memberInfo = stream.ReadMemberInfo();

        switch (type)
        {
            case MemberBindingType.Assignment:
                var expression = handler.Deserialize(stream);
                return Expression.Bind(memberInfo, expression);
            case MemberBindingType.MemberBinding:
                var subBindingsCount = (int)stream.ReadUInt16();
                var subBindings = new ReadOnlyCollectionBuilder<MemberBinding>(subBindingsCount);
                for (var i = 0; i < subBindingsCount; i++)
                {
                    var binding = ReadMemberBinding(handler, stream);
                    subBindings.Add(binding);
                }

                return Expression.MemberBind(memberInfo, subBindings);
            case MemberBindingType.ListBinding:
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

                    initializers.Add(Expression.ElementInit(methodInfo, arguments.ToReadOnlyCollection()));
                }

                return Expression.ListBind(memberInfo, initializers);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void WriteMemberBinding(ExpressionSerializer handler, Stream stream, MemberBinding memberBinding)
    {
        stream.WriteByte((byte)(int)memberBinding.BindingType);
        stream.WriteMemberInfo(memberBinding.Member);
        switch (memberBinding.BindingType)
        {
            case MemberBindingType.Assignment:
                {
                    var memberAssignment = (MemberAssignment)memberBinding;
                    handler.Serialize(memberAssignment.Expression, stream);
                    break;
                }
            case MemberBindingType.MemberBinding:
                {
                    var memberMemberBinding = (MemberMemberBinding)memberBinding;
                    var subBindings = memberMemberBinding.Bindings;

                    stream.Write((ushort)subBindings.Count);
                    foreach (var subBinding in subBindings)
                    {
                        WriteMemberBinding(handler, stream, subBinding);
                    }

                    break;
                }
            case MemberBindingType.ListBinding:
                {
                    var listBinding = (MemberListBinding)memberBinding;
                    var initializers = listBinding.Initializers;
                    stream.Write((ushort)initializers.Count);
                    foreach (var initializer in initializers)
                    {
                        stream.WriteMethodInfo(initializer.AddMethod);

                        var arguments = initializer.Arguments;
                        stream.Write((ushort)arguments.Count);
                        foreach (var argumentExpression in arguments)
                        {
                            handler.Serialize(argumentExpression, stream);
                        }
                    }

                    break;
                }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
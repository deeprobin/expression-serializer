using System;
using System.Linq.Expressions;
using Xunit;

namespace ExpressionSerializer.Tests.Reliability;

public sealed class ExpressionTypeTests
{
    [Fact]
    public void TestExpressionTypeEnumVariants()
    {
        // Since `System.Linq.Expressions` is effectively archived, we can be confident that very few changes will take place in the future.
        // See https://github.com/dotnet/runtime/blob/20761d0beaefce8c3de502bc6059ec62e984ebf5/src/libraries/System.Linq.Expressions/README.md.
        // This Fact verifies that no breaking changes have occurred.
        Assert.Equal(85, Enum.GetValues(typeof(ExpressionType)).Length);

        // This checks if the order in the enum has remained the same. This is important because our protocol is fixed.
        Assert.Equal(0, (int)ExpressionType.Add);
        Assert.Equal(1, (int)ExpressionType.AddChecked);
        Assert.Equal(2, (int)ExpressionType.And);
        Assert.Equal(3, (int)ExpressionType.AndAlso);
        Assert.Equal(4, (int)ExpressionType.ArrayLength);
        Assert.Equal(5, (int)ExpressionType.ArrayIndex);
        Assert.Equal(6, (int)ExpressionType.Call);
        Assert.Equal(7, (int)ExpressionType.Coalesce);
        Assert.Equal(8, (int)ExpressionType.Conditional);
        Assert.Equal(9, (int)ExpressionType.Constant);
        Assert.Equal(10, (int)ExpressionType.Convert);
        Assert.Equal(11, (int)ExpressionType.ConvertChecked);
        Assert.Equal(12, (int)ExpressionType.Divide);
        Assert.Equal(13, (int)ExpressionType.Equal);
        Assert.Equal(14, (int)ExpressionType.ExclusiveOr);
        Assert.Equal(15, (int)ExpressionType.GreaterThan);
        Assert.Equal(16, (int)ExpressionType.GreaterThanOrEqual);
        Assert.Equal(17, (int)ExpressionType.Invoke);
        Assert.Equal(18, (int)ExpressionType.Lambda);
        Assert.Equal(19, (int)ExpressionType.LeftShift);
        Assert.Equal(20, (int)ExpressionType.LessThan);
        Assert.Equal(21, (int)ExpressionType.LessThanOrEqual);
        Assert.Equal(22, (int)ExpressionType.ListInit);
        Assert.Equal(23, (int)ExpressionType.MemberAccess);
        Assert.Equal(24, (int)ExpressionType.MemberInit);
        Assert.Equal(25, (int)ExpressionType.Modulo);
        Assert.Equal(26, (int)ExpressionType.Multiply);
        Assert.Equal(27, (int)ExpressionType.MultiplyChecked);
        Assert.Equal(28, (int)ExpressionType.Negate);
        Assert.Equal(29, (int)ExpressionType.UnaryPlus);
        Assert.Equal(30, (int)ExpressionType.NegateChecked);
        Assert.Equal(31, (int)ExpressionType.New);
        Assert.Equal(32, (int)ExpressionType.NewArrayInit);
        Assert.Equal(33, (int)ExpressionType.NewArrayBounds);
        Assert.Equal(34, (int)ExpressionType.Not);
        Assert.Equal(35, (int)ExpressionType.NotEqual);
        Assert.Equal(36, (int)ExpressionType.Or);
        Assert.Equal(37, (int)ExpressionType.OrElse);
        Assert.Equal(38, (int)ExpressionType.Parameter);
        Assert.Equal(39, (int)ExpressionType.Power);
        Assert.Equal(40, (int)ExpressionType.Quote);
        Assert.Equal(41, (int)ExpressionType.RightShift);
        Assert.Equal(42, (int)ExpressionType.Subtract);
        Assert.Equal(43, (int)ExpressionType.SubtractChecked);
        Assert.Equal(44, (int)ExpressionType.TypeAs);
        Assert.Equal(45, (int)ExpressionType.TypeIs);
        Assert.Equal(46, (int)ExpressionType.Assign);
        Assert.Equal(47, (int)ExpressionType.Block);
        Assert.Equal(48, (int)ExpressionType.DebugInfo);
        Assert.Equal(49, (int)ExpressionType.Decrement);
        Assert.Equal(50, (int)ExpressionType.Dynamic);
        Assert.Equal(51, (int)ExpressionType.Default);
        Assert.Equal(52, (int)ExpressionType.Extension);
        Assert.Equal(53, (int)ExpressionType.Goto);
        Assert.Equal(54, (int)ExpressionType.Increment);
        Assert.Equal(55, (int)ExpressionType.Index);
        Assert.Equal(56, (int)ExpressionType.Label);
        Assert.Equal(57, (int)ExpressionType.RuntimeVariables);
        Assert.Equal(58, (int)ExpressionType.Loop);
        Assert.Equal(59, (int)ExpressionType.Switch);
        Assert.Equal(60, (int)ExpressionType.Throw);
        Assert.Equal(61, (int)ExpressionType.Try);
        Assert.Equal(62, (int)ExpressionType.Unbox);
        Assert.Equal(63, (int)ExpressionType.AddAssign);
        Assert.Equal(64, (int)ExpressionType.AndAssign);
        Assert.Equal(65, (int)ExpressionType.DivideAssign);
        Assert.Equal(66, (int)ExpressionType.ExclusiveOrAssign);
        Assert.Equal(67, (int)ExpressionType.LeftShiftAssign);
        Assert.Equal(68, (int)ExpressionType.ModuloAssign);
        Assert.Equal(69, (int)ExpressionType.MultiplyAssign);
        Assert.Equal(70, (int)ExpressionType.OrAssign);
        Assert.Equal(71, (int)ExpressionType.PowerAssign);
        Assert.Equal(72, (int)ExpressionType.RightShiftAssign);
        Assert.Equal(73, (int)ExpressionType.SubtractAssign);
        Assert.Equal(74, (int)ExpressionType.AddAssignChecked);
        Assert.Equal(75, (int)ExpressionType.MultiplyAssignChecked);
        Assert.Equal(76, (int)ExpressionType.SubtractAssignChecked);
        Assert.Equal(77, (int)ExpressionType.PreIncrementAssign);
        Assert.Equal(78, (int)ExpressionType.PreDecrementAssign);
        Assert.Equal(79, (int)ExpressionType.PostIncrementAssign);
        Assert.Equal(80, (int)ExpressionType.PostDecrementAssign);
        Assert.Equal(81, (int)ExpressionType.TypeEqual);
        Assert.Equal(82, (int)ExpressionType.OnesComplement);
        Assert.Equal(83, (int)ExpressionType.IsTrue);
        Assert.Equal(84, (int)ExpressionType.IsFalse);
    }
}
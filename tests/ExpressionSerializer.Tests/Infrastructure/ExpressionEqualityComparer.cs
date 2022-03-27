using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionSerializer.Tests.Infrastructure;

public sealed class ExpressionEqualityComparer : IEqualityComparer<Expression>
{
    private static readonly Lazy<ExpressionEqualityComparer> InstanceLazy = new(static () => new ExpressionEqualityComparer());

    public static ExpressionEqualityComparer Instance => InstanceLazy.Value;

    private ExpressionEqualityComparer() {}

    public bool Equals(Expression? x, Expression? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.CanReduce == y.CanReduce && x.NodeType == y.NodeType && x.Type == y.Type && PropertyEquals(x, y);
    }

    private bool PropertyEquals(Expression x, Expression y)
    {
        var type = x.GetType();
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        foreach (var property in properties)
        {
            var xValue = property.GetValue(x);
            var yValue = property.GetValue(y);

            if (ReferenceEquals(x, y)) continue;
            if (xValue is null) return false;
            if (yValue is null) return false;
            if (xValue.GetType() != yValue.GetType()) return false;

            if (xValue is not Expression || yValue is not Expression) continue;
            if (!Equals(x, y)) return false;
        }

        return true;
    }

    public int GetHashCode(Expression obj)
    {
        var hashCode = HashCode.Combine(obj.CanReduce, (int)obj.NodeType, obj.Type);

        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        return properties
            .Select(property => property.GetValue(obj))
            .Aggregate(hashCode, HashCode.Combine);
    }
}
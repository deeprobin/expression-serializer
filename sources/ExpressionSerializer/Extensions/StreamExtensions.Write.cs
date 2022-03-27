using System.Buffers.Binary;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ExpressionSerializer.Extensions;

internal static partial class StreamExtensions
{
    public static void Write(this Stream stream, bool state) => stream.WriteByte(state ? (byte)1 : (byte)0);

    public static void Write(this Stream stream, short value)
    {
        Span<byte> lengthBytes = stackalloc byte[sizeof(short)];
        BinaryPrimitives.WriteInt16LittleEndian(lengthBytes, value);
        stream.Write(lengthBytes);
    }

    public static void Write(this Stream stream, ushort value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, value);
        stream.Write(bytes);
    }
    public static void Write(this Stream stream, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        stream.Write(bytes);
    }
    public static void Write(this Stream stream, uint value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        stream.Write(bytes);
    }
    public static void Write(this Stream stream, long value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
        stream.Write(bytes);
    }
    public static void Write(this Stream stream, ulong value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
        stream.Write(bytes);
    }
    public static void Write(this Stream stream, Half value)
    {
        unsafe
        {
            Span<byte> bytes = stackalloc byte[sizeof(Half)];
            BinaryPrimitives.WriteHalfLittleEndian(bytes, value);
            stream.Write(bytes);
        }
    }

    public static void Write(this Stream stream, float value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(float)];
        BinaryPrimitives.WriteSingleLittleEndian(bytes, value);
        stream.Write(bytes);
    }

    public static void Write(this Stream stream, double value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleLittleEndian(bytes, value);
        stream.Write(bytes);
    }

    public static void Write(this Stream stream, decimal value)
    {
        Span<int> ints = stackalloc int[4];
        decimal.GetBits(value, ints);
        var bytes = MemoryMarshal.AsBytes(ints);
        stream.Write(bytes);
    }
    public static void Write(this Stream stream, char value) => stream.Write((int)value);

    public static void Write(this Stream stream, Rune value)
    {
        Span<byte> runeBytes = stackalloc byte[4];
        var bytesWritten = value.EncodeToUtf8(runeBytes);
        Debug.Assert(bytesWritten < byte.MaxValue);

        stream.WriteByte((byte)bytesWritten);
        var bytes = runeBytes[..bytesWritten];

        stream.Write(bytes);
    }

    public static void Write(this Stream stream, Guid value)
    {
        unsafe
        {
            Span<byte> bytes = stackalloc byte[sizeof(Guid)];
            var writeResult = value.TryWriteBytes(bytes);
            Debug.Assert(!writeResult);
            stream.Write(bytes);
        }
    }
    public static void Write8BitPrefixed(this Stream stream, ReadOnlySpan<char> chars)
    {
        var bytes = MemoryMarshal.AsBytes(chars);
        var length = bytes.Length;
        Debug.Assert(length <= byte.MaxValue);

        stream.WriteByte((byte)length);
        stream.Write(bytes);
    }

    public static void Write16BitPrefixed(this Stream stream, ReadOnlySpan<char> chars)
    {
        var bytes = MemoryMarshal.AsBytes(chars);
        var length = bytes.Length;
        Debug.Assert(length <= ushort.MaxValue);

        stream.Write((ushort)length);
        stream.Write(bytes);
    }

    public static void Write32BitPrefixed(this Stream stream, ReadOnlySpan<char> chars)
    {
        var bytes = MemoryMarshal.AsBytes(chars);
        var length = bytes.Length;

        stream.Write((uint)length);
        stream.Write(bytes);
    }

    public static void WriteType(this Stream stream, Type type)
    {
        var typeId = TypeIdProvider.GetTypeId(type);
        if (typeId is { } t)
        {
            stream.WriteByte(t);
        }
        else
        {
            // Non-specified type
            stream.WriteByte(254);

            var typeName = type.AssemblyQualifiedName ?? "";
            stream.Write32BitPrefixed(typeName);
        }
    }

    public static void WriteMethodInfo(this Stream stream, MethodInfo methodInfo)
    {
        stream.WriteType(methodInfo.DeclaringType!);

        stream.WriteByte(methodInfo.IsStatic ? (byte)1 : (byte)0);
        
        stream.Write16BitPrefixed(methodInfo.Name);

        var parameters = methodInfo.GetParameters();
        stream.Write((ushort)parameters.Length);
        foreach (var parameterInfo in parameters)
        {
            stream.WriteType(parameterInfo.ParameterType);
        }
    }

    public static void WriteConstructorInfo(this Stream stream, ConstructorInfo constructorInfo)
    {
        stream.WriteType(constructorInfo.DeclaringType!);
        
        var parameters = constructorInfo.GetParameters();
        stream.Write((ushort)parameters.Length);
        foreach (var parameterInfo in parameters)
        {
            stream.WriteType(parameterInfo.ParameterType);
        }
    }

    public static void WritePropertyInfo(this Stream stream, PropertyInfo propertyInfo)
    {
        stream.WriteType(propertyInfo.DeclaringType!);
        stream.Write16BitPrefixed(propertyInfo.Name);
    }

    public static void WriteMemberInfo(this Stream stream, MemberInfo memberInfo)
    {
        stream.WriteType(memberInfo.DeclaringType!);
        stream.Write16BitPrefixed(memberInfo.Name);
    }

    public static void WriteLabel(this Stream stream, LabelTarget labelTarget)
    {
        stream.WriteType(labelTarget.Type);
        if (labelTarget.Name is {} name)
        {
            stream.WriteByte(1);
            stream.Write32BitPrefixed(name);
        }
        else
        {
            stream.WriteByte(0);
        }
    }

    public static void WriteValue(this Stream stream, Type type, object? obj)
    {
        Debug.Assert(obj is null || obj.GetType() == type);

        // Write TypeId
        stream.WriteType(type);

        // Write `HasValue`
        if (obj is null)
        {
            stream.WriteByte(0);
            return;
        }
        stream.WriteByte(1);

        switch (obj)
        {
            case byte b:
                stream.WriteByte(b);
                break;
            case sbyte sb:
                stream.WriteByte(Unsafe.As<sbyte, byte>(ref sb));
                break;

            case ushort us:
                stream.Write(us);
                break;
            case short s:
                stream.Write(s);
                break;

            case uint ui:
                stream.Write(ui);
                break;
            case int i:
                stream.Write(i);
                break;

            case ulong ul:
                stream.Write(ul);
                break;
            case long l:
                stream.Write(l);
                break;

            // Serialize platform-dependent size as 64-bit value
            case nuint nui:
                stream.Write(nui);
                break;
            case nint ni:
                stream.Write(ni);
                break;

            case Half h:
                stream.Write(h);
                break;
            case float f:
                stream.Write(f);
                break;
            case double d:
                stream.Write(d);
                break;
            case decimal m:
                stream.Write(m);
                break;

            case char c:
                stream.Write(c);
                break;

            case Rune r:
                stream.Write(r);
                break;

            case string s:
                stream.Write32BitPrefixed(s);
                break;

            case Guid g:
                stream.Write(g);
                break;

            case DateTime dt:
                stream.Write(dt.ToBinary());
                break;
            case DateTimeOffset dto:
                stream.Write(dto.DateTime.ToBinary());
                stream.Write(dto.Offset.Ticks);
                break;
            case DateOnly dateOnly:
                stream.Write(dateOnly.DayNumber);
                break;
            case TimeOnly timeOnly:
                stream.Write(timeOnly.Ticks);
                break;
            case TimeSpan timeSpan:
                stream.Write(timeSpan.Ticks);
                break;
        }
    }
}
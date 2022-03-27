using System.Buffers;
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
    public static Half ReadHalf(this Stream stream)
    {
        unsafe
        {
            Span<byte> bytes = stackalloc byte[sizeof(Half)];
            _ = stream.Read(bytes);
            return BinaryPrimitives.ReadHalfLittleEndian(bytes);
        }
    }

    public static float ReadSingle(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(float)];
        _ = stream.Read(bytes);
        return BinaryPrimitives.ReadSingleLittleEndian(bytes);
    }

    public static double ReadDouble(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(double)];
        _ = stream.Read(bytes);
        return BinaryPrimitives.ReadDoubleLittleEndian(bytes);
    }

    public static decimal ReadDecimal(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int) * 4];
        _ = stream.Read(bytes);
        unsafe
        {
            var ints = new Span<int>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(bytes)), sizeof(int) * 4);
            return new decimal(ints);
        }
    }

    public static char ReadChar(this Stream stream) => (char)stream.ReadInt32();

    public static Rune ReadRune(this Stream stream)
    {
        Span<byte> runeBytes = stackalloc byte[4];
        _ = stream.Read(runeBytes);
        var status = Rune.DecodeFromUtf8(runeBytes, out var result, out _);
        Debug.Assert(status == OperationStatus.Done);

        return result;
    }

    public static Guid ReadGuid(this Stream stream)
    {
        unsafe
        {
            Span<byte> bytes = stackalloc byte[sizeof(Guid)];
            _ = stream.Read(bytes);
            return new Guid(bytes);
        }
    }

    public static short ReadInt16(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(short)];
        _ = stream.Read(bytes);
        return BinaryPrimitives.ReadInt16LittleEndian(bytes);
    }
    public static ushort ReadUInt16(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        _ = stream.Read(bytes);
        return BinaryPrimitives.ReadUInt16LittleEndian(bytes);
    }
    public static int ReadInt32(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        _ = stream.Read(bytes);
        return BinaryPrimitives.ReadInt32LittleEndian(bytes);
    }
    public static uint ReadUInt32(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        _ = stream.Read(bytes);
        return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
    }
    public static long ReadInt64(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        _ = stream.Read(bytes);
        return BinaryPrimitives.ReadInt64LittleEndian(bytes);
    }
    public static ulong ReadUInt64(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        _ = stream.Read(bytes);
        return BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    }

    public static ReadOnlySpan<char> Read16BitPrefixed(this Stream stream)
    {
        var length = (int)stream.ReadUInt16();

        Span<byte> buffer = stackalloc byte[length];
        _ = stream.Read(buffer);

        unsafe
        {
            return new Span<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), length);
        }
    }

    public static ReadOnlySpan<char> Read32BitPrefixed(this Stream stream)
    {
        var length = (int)stream.ReadUInt32();

        Span<byte> buffer = stackalloc byte[length];
        _ = stream.Read(buffer);

        unsafe
        {
            return new Span<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), length);
        }
    }

    public static Type ReadType(this Stream stream)
    {
        var typeId = (byte)stream.ReadByte();
        if (typeId != 254) return TypeIdProvider.GetTypeById(typeId);
        
        var typeName = stream.Read32BitPrefixed();
        return typeof(ExpressionSerializer).Assembly.GetType(typeName.ToString())!;
    }

    public static MethodInfo ReadMethodInfo(this Stream stream)
    {
        var declaringType = stream.ReadType();

        var isStatic = (byte)stream.ReadByte() != 0;
        var name = stream.Read16BitPrefixed().ToString();

        var parameterCount = (int) stream.ReadUInt16();
        var parameters = new Type[parameterCount];

        for (var i = 0; i < parameterCount; i++)
        {
            var parameterType = stream.ReadType();
            parameters[i] = parameterType;
        }

        var flags = BindingFlags.Public;
        if (isStatic)
        {
            flags |= BindingFlags.Static;
        }

        return declaringType.GetMethod(name, flags, parameters.ToArray())!;
    }

    public static ConstructorInfo ReadConstructorInfo(this Stream stream)
    {
        var declaringType = stream.ReadType();
        var parameterCount = (int)stream.ReadUInt16();
        var parameters = new Type[parameterCount];

        for (var i = 0; i < parameterCount; i++)
        {
            var parameterType = stream.ReadType();
            parameters[i] = parameterType;
        }

        return declaringType.GetConstructor(parameters)!;
    }

    public static PropertyInfo ReadPropertyInfo(this Stream stream)
    {
        var declaringType = stream.ReadType();
        var name = stream.Read16BitPrefixed().ToString();

        return declaringType.GetProperty(name, BindingFlags.Public)!;
    }

    public static MemberInfo ReadMemberInfo(this Stream stream)
    {
        var declaringType = stream.ReadType();
        var memberName = stream.Read16BitPrefixed().ToString();

        return declaringType.GetMember(memberName, BindingFlags.Public)[0];
    }

    public static LabelTarget ReadLabel(this Stream stream)
    {
        var type = stream.ReadType();
        string? name = default;
        var nameHasValue = (byte)stream.ReadByte() != 0;
        if (nameHasValue)
        {
            name = stream.Read32BitPrefixed().ToString();
        }

        return Expression.Label(type, name);
    }

    public static object? ReadValue(this Stream stream)
    {
        var type = stream.ReadType();
        var hasValue = (byte) stream.ReadByte();
        if (hasValue == 0) return null;

        if (type == typeof(byte))
        {
            return (byte)stream.ReadByte();
        }

        if (type == typeof(sbyte))
        {
            var b = (byte)stream.ReadByte();
            return Unsafe.As<byte, sbyte>(ref b);
        }

        if (type == typeof(short))
        {
            return stream.ReadInt16();
        }
        if (type == typeof(ushort))
        {
            return stream.ReadUInt16();
        }
        if (type == typeof(int))
        {
            return stream.ReadInt32();
        }
        if (type == typeof(uint))
        {
            return stream.ReadUInt32();
        }
        if (type == typeof(long))
        {
            return stream.ReadInt64();
        }
        if (type == typeof(ulong))
        {
            return stream.ReadUInt64();
        }
        if (type == typeof(nint))
        {
            return (nint)stream.ReadInt64();
        }
        if (type == typeof(nuint))
        {
            return (nuint)stream.ReadUInt64();
        }
        if (type == typeof(Half))
        {
            return stream.ReadHalf();
        }
        if (type == typeof(float))
        {
            return stream.ReadSingle();
        }
        if (type == typeof(double))
        {
            return stream.ReadDouble();
        }
        if (type == typeof(decimal))
        {
            return stream.ReadDecimal();
        }
        if (type == typeof(char))
        {
            return stream.ReadChar();
        }
        if (type == typeof(Rune))
        {
            return stream.ReadRune();
        }
        if (type == typeof(Rune))
        {
            return stream.ReadRune();
        }
        if (type == typeof(string))
        {
            return stream.Read32BitPrefixed().ToString();
        }
        if (type == typeof(Guid))
        {
            return stream.ReadGuid();
        }

        if (type == typeof(DateTime))
        {
            var binary = stream.ReadInt64();
            return DateTime.FromBinary(binary);
        }

        if (type == typeof(DateTimeOffset))
        {
            var dtBinary = stream.ReadInt64();
            var offsetTicks = stream.ReadInt64();
            return new DateTimeOffset(DateTime.FromBinary(dtBinary), new TimeSpan(offsetTicks));
        }

        if (type == typeof(DateOnly))
        {
            var dayNumber = stream.ReadInt32();
            return DateOnly.FromDayNumber(dayNumber);
        }

        if (type == typeof(TimeOnly))
        {
            var ticks = stream.ReadInt64();
            return new TimeOnly(ticks);
        }

        if (type == typeof(TimeSpan))
        {
            var ticks = stream.ReadInt64();
            return new TimeSpan(ticks);
        }

        throw new ArgumentOutOfRangeException(nameof(type));
    }
}
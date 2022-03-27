using System.Text;

namespace ExpressionSerializer;

internal static class TypeIdProvider
{
    public static byte? GetTypeId(Type type)
    {
        if (typeof(byte) == type)
            return 0;
        if (typeof(sbyte) == type)
            return 1;

        if (typeof(ushort) == type)
            return 2;
        if (typeof(short) == type)
            return 3;

        if (typeof(uint) == type)
            return 4;
        if (typeof(int) == type)
            return 5;

        if (typeof(ulong) == type)
            return 6;
        if (typeof(long) == type)
            return 7;

        if (typeof(nuint) == type)
            return 8;
        if (typeof(nint) == type)
            return 9;

        if (typeof(Half) == type)
            return 10;
        if (typeof(float) == type)
            return 11;
        if (typeof(double) == type)
            return 12;
        if (typeof(decimal) == type)
            return 13;

        if (typeof(char) == type)
            return 14;
        if (typeof(Rune) == type)
            return 15;
        if (typeof(string) == type)
            return 16;

        if (typeof(Guid) == type)
            return 17;

        if (typeof(DateTime) == type)
            return 18;
        if (typeof(DateTimeOffset) == type)
            return 19;
        if (typeof(DateOnly) == type)
            return 20;
        if (typeof(TimeOnly) == type)
            return 21;
        if (typeof(TimeSpan) == type)
            return 22;

        return typeof(void) == type ? (byte?)255 : default;
    }

    public static Type GetTypeById(byte id)
    {
        return id switch
        {
            0 => typeof(byte),
            1 => typeof(sbyte),
            2 => typeof(ushort),
            3 => typeof(short),
            4 => typeof(uint),
            5 => typeof(int),
            6 => typeof(ulong),
            7 => typeof(long),
            8 => typeof(nuint),
            9 => typeof(nint),
            10 => typeof(Half),
            11 => typeof(float),
            12 => typeof(double),
            13 => typeof(decimal),
            14 => typeof(char),
            15 => typeof(Rune),
            16 => typeof(string),
            17 => typeof(Guid),
            18 => typeof(DateTime),
            19 => typeof(DateTimeOffset),
            20 => typeof(DateOnly),
            21 => typeof(TimeOnly),
            22 => typeof(TimeSpan),
            255 => typeof(void),
            _ => throw new ArgumentOutOfRangeException(nameof(id))
        };
    }
}
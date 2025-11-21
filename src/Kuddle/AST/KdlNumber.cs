using System;
using System.Linq;
using System.Numerics;

namespace Kuddle.AST;

public sealed record KdlNumber(string RawValue) : KdlValue
{
    private const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;

    public NumberBase Base =>
        RawValue.StartsWith("0x") ? NumberBase.Hex
        : RawValue.StartsWith("0o") ? NumberBase.Octal
        : RawValue.StartsWith("0b") ? NumberBase.Binary
        : NumberBase.Decimal;

    private static (string sanitised, int radix) Sanitise(string raw, NumberBase baseKind)
    {
        var sanitised = raw.Replace("_", "");
        return baseKind switch
        {
            NumberBase.Binary => (sanitised.Replace("0b", "", IgnoreCase), 2),
            NumberBase.Hex => (sanitised.Replace("0x", "", IgnoreCase), 16),
            NumberBase.Octal => (sanitised.Replace("0o", "", IgnoreCase), 8),
            _ => (sanitised, 10),
        };
    }

    public long ToInt64()
    {
        if (RawValue.ContainsAny(['.', 'e', 'E']) || RawValue.StartsWith('#'))
            throw new InvalidOperationException();
        var cleaned = Sanitise(RawValue, Base);
        return Convert.ToInt64(cleaned.sanitised, cleaned.radix);
    }

    public int ToInt32()
    {
        return checked((int)ToInt64());
    }

    public short ToInt16()
    {
        return checked((short)ToInt64());
    }

    public sbyte ToSByte()
    {
        return checked((sbyte)ToInt64());
    }

    public ulong ToUInt64()
    {
        if (RawValue.StartsWith('#'))
            throw new InvalidOperationException();
        var (sanitised, radix) = Sanitise(RawValue, Base);
        return Convert.ToUInt64(sanitised, radix);
    }

    public uint ToUInt32()
    {
        return checked((uint)ToUInt64());
    }

    public ushort ToUInt16()
    {
        return checked((ushort)ToUInt64());
    }

    public byte ToByte()
    {
        return checked((byte)ToUInt64());
    }

    public double ToDouble()
    {
        return RawValue.StartsWith('#')
            ? RawValue switch
            {
                "#inf" => double.PositiveInfinity,
                "#-inf" => double.NegativeInfinity,
                "#nan" => double.NaN,
                _ => throw new NotSupportedException(),
            }
            : Convert.ToDouble(Sanitise(RawValue, Base).sanitised);
    }

    public float ToFloat()
    {
        return checked((float)ToDouble());
    }

    public decimal ToDecimal()
    {
        var cleaned = Sanitise(RawValue, Base);
        return RawValue.StartsWith('#')
            ? throw new NotSupportedException()
            : Decimal.Parse(cleaned.sanitised, System.Globalization.NumberStyles.Float);
    }

    public BigInteger ToBigInteger()
    {
        if (RawValue.ContainsAny(['.', 'e', 'E']) || RawValue.StartsWith('#'))
            throw new InvalidOperationException();
        var cleaned = Sanitise(RawValue, Base);
        var actualInt = Convert.ToInt64(cleaned.sanitised, 2);
        var test = cleaned.sanitised.Aggregate(BigInteger.Zero, (s, a) => (s << 1) + a - '0');
        return BigInteger.Parse(cleaned.sanitised, System.Globalization.NumberStyles.BinaryNumber);
    }
}

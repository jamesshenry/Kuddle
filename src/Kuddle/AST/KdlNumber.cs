using System;
using System.Collections.Generic;
using System.Numerics;

namespace Kuddle.AST;

public sealed record KdlNumber(string RawValue, NumberKind Kind) : KdlValue
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
        var cleaned = Sanitise(RawValue, Base);
        return Convert.ToInt64(cleaned.sanitised, cleaned.radix);
    }

    public int ToInt32()
    {
        var cleaned = Sanitise(RawValue, Base);
        return int.Parse(cleaned.sanitised);
    }

    public short ToInt16()
    {
        throw new NotImplementedException();
    }

    public sbyte ToSByte()
    {
        throw new NotImplementedException();
    }

    public ulong ToUInt64()
    {
        var (sanitised, radix) = Sanitise(RawValue, Base);
        return Convert.ToUInt64(sanitised, radix);
    }

    public uint ToUInt32()
    {
        return checked((uint)ToUInt64());
    }

    public ushort ToUInt16()
    {
        throw new NotImplementedException();
    }

    public byte ToByte()
    {
        throw new NotImplementedException();
    }

    public double ToDouble()
    {
        if (Kind != NumberKind.Decimal)
            throw new InvalidOperationException();
        return 2;
    }

    public float ToFloat()
    {
        throw new NotImplementedException();
    }

    public decimal ToDecimal()
    {
        throw new NotImplementedException();
    }

    public BigInteger ToBigInteger()
    {
        var cleaned = Sanitise(RawValue, Base);

        return BigInteger.Parse(cleaned.sanitised);
    }
}

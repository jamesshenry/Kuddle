using System;
using System.Linq;
using System.Numerics;

namespace Kuddle.AST;

public sealed record KdlNumber(string RawValue) : KdlValue
{
    private const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;

    public NumberBase Base
    {
        get
        {
            ReadOnlySpan<char> span = RawValue.AsSpan();
            if (span.IsEmpty)
                return NumberBase.Decimal;

            // Skip sign
            if (span[0] == '+' || span[0] == '-')
            {
                if (span.Length == 1)
                    return NumberBase.Decimal;
                span = span[1..];
            }

            // Check prefix
            if (span.Length >= 2 && span[0] == '0')
            {
                char c = span[1];
                if (c == 'x' || c == 'X')
                    return NumberBase.Hex;
                if (c == 'o' || c == 'O')
                    return NumberBase.Octal;
                if (c == 'b' || c == 'B')
                    return NumberBase.Binary;
            }

            return NumberBase.Decimal;
        }
    }

    public long ToInt64()
    {
        if (RawValue.ContainsAny(['.', 'e', 'E']) || RawValue.StartsWith('#'))
            throw new FormatException($"Value '{RawValue}' is not a valid Integer.");
        var (magnitudeString, radix, isNegative) = Sanitise(RawValue, Base);

        try
        {
            // 1. Parse as Unsigned first (Handles full 64-bit magnitude)
            ulong magnitude = Convert.ToUInt64(magnitudeString, radix);

            // 2. Apply Sign and Check Bounds manually
            if (isNegative)
            {
                // Edge case: Int64.MinValue (9223372036854775808)
                // fits in ulong, but is > long.MaxValue.
                if (magnitude == (ulong)long.MaxValue + 1)
                {
                    return long.MinValue;
                }

                if (magnitude > (ulong)long.MaxValue)
                {
                    throw new OverflowException();
                }

                return -(long)magnitude;
            }
            else
            {
                if (magnitude > (ulong)long.MaxValue)
                {
                    throw new OverflowException();
                }
                return (long)magnitude;
            }
        }
        catch (FormatException)
        {
            throw new FormatException($"Value '{RawValue}' is not a valid {Base} integer.");
        }
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
        if (RawValue.ContainsAny(['.', 'e', 'E']) || RawValue.StartsWith('#'))
            throw new FormatException($"Value '{RawValue}' is not a valid Integer.");

        var (magnitudeString, radix, isNegative) = Sanitise(RawValue, Base);

        if (isNegative)
            throw new OverflowException("Cannot convert negative value to UInt64.");

        return Convert.ToUInt64(magnitudeString, radix);
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
        if (RawValue.StartsWith('#'))
        {
            return (double)(
                RawValue switch
                {
                    "#inf" => double.PositiveInfinity,
                    "#-inf" => double.NegativeInfinity,
                    "#nan" => double.NaN,
                    _ => throw new NotSupportedException(),
                }
            );
        }
        var (magnitudeString, radix, isNegative) = Sanitise(RawValue, Base);

        return (double)Convert.ToDouble(magnitudeString);
    }

    public float ToFloat()
    {
        return checked((float)ToDouble());
    }

    public decimal ToDecimal()
    {
        var (cleaned, radix, isNegative) = Sanitise(RawValue, Base);
        return RawValue.StartsWith('#')
            ? throw new NotSupportedException()
            : Decimal.Parse(cleaned, System.Globalization.NumberStyles.Float);
    }

    // public BigInteger ToBigInteger()
    // {
    //     if (RawValue.ContainsAny(['.', 'e', 'E']) || RawValue.StartsWith('#'))
    //         throw new InvalidOperationException();
    //     var cleaned = Sanitise(RawValue, Base);
    //     var actualInt = Convert.ToInt64(cleaned.sanitised, 2);
    //     var test = cleaned.sanitised.Aggregate(BigInteger.Zero, (s, a) => (s << 1) + a - '0');
    //     return BigInteger.Parse(cleaned.sanitised, System.Globalization.NumberStyles.BinaryNumber);
    // }

    private static (string cleaned, int radix, bool isNegative) Sanitise(
        string raw,
        NumberBase baseKind
    )
    {
        string s = raw.Replace("_", "");
        if (string.IsNullOrEmpty(s))
            return ("0", 10, false);

        bool isNegative = s.StartsWith('-');
        bool isPositive = s.StartsWith('+');

        // Strip Sign
        string unsigned = (isNegative || isPositive) ? s.Substring(1) : s;

        // Strip Prefix (0x, 0b, 0o)
        int radix = 10;
        if (baseKind == NumberBase.Hex)
        {
            radix = 16;
            if (unsigned.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                unsigned = unsigned.Substring(2);
        }
        else if (baseKind == NumberBase.Octal)
        {
            radix = 8;
            if (unsigned.StartsWith("0o", StringComparison.OrdinalIgnoreCase))
                unsigned = unsigned.Substring(2);
        }
        else if (baseKind == NumberBase.Binary)
        {
            radix = 2;
            if (unsigned.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                unsigned = unsigned.Substring(2);
        }

        return (unsigned, radix, isNegative);
    }
}

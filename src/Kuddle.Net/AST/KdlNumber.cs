using System;
using System.Globalization;

namespace Kuddle.AST;

public sealed record KdlNumber(string RawValue) : KdlValue
{
    public NumberBase GetBase()
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

    public long ToInt64()
    {
        if (RawValue.ContainsAny(['.', 'e', 'E']) || RawValue.StartsWith('#'))
            throw new FormatException($"Value '{RawValue}' is not a valid Integer.");
        var (sanitised, radix, isNegative) = Sanitise(RawValue, GetBase());

        try
        {
            ulong magnitude = Convert.ToUInt64(sanitised, radix);

            if (isNegative)
            {
                if (magnitude == (ulong)long.MaxValue + 1)
                {
                    return long.MinValue;
                }

                if (magnitude > long.MaxValue)
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
            throw new FormatException($"Value '{RawValue}' is not a valid {GetBase()} integer.");
        }
    }

    public int ToInt32() => checked((int)ToInt64());

    public short ToInt16() => checked((short)ToInt64());

    public sbyte ToSByte() => checked((sbyte)ToInt64());

    public ulong ToUInt64()
    {
        if (RawValue.ContainsAny(['.', 'e', 'E']) || RawValue.StartsWith('#'))
            throw new FormatException($"Value '{RawValue}' is not a valid Integer.");

        var (magnitudeString, radix, isNegative) = Sanitise(RawValue, GetBase());

        if (isNegative)
            throw new OverflowException("Cannot convert negative value to UInt64.");

        return Convert.ToUInt64(magnitudeString, radix);
    }

    public uint ToUInt32() => checked((uint)ToUInt64());

    public ushort ToUInt16() => checked((ushort)ToUInt64());

    public byte ToByte() => checked((byte)ToUInt64());

    public double ToDouble()
    {
        var numberBase = GetBase();

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
        var (sanitised, radix, isNegative) = Sanitise(RawValue, numberBase);

        double result;
        if (radix != 10)
        {
            result = Convert.ToUInt64(sanitised, radix);
        }
        else
        {
            result = Convert.ToDouble(sanitised);
        }
        return isNegative ? result * -1 : result;
    }

    public float ToFloat() => checked((float)ToDouble());

    public decimal ToDecimal()
    {
        var numberBase = GetBase();
        if (RawValue.StartsWith('#'))
        {
            throw new NotSupportedException();
        }
        var (sanitised, radix, isNegative) = Sanitise(RawValue, numberBase);

        decimal result;
        if (radix != 10)
        {
            result = Convert.ToUInt64(sanitised, radix);
        }
        else
        {
            result = decimal.Parse(sanitised, NumberStyles.Float, CultureInfo.InvariantCulture);
        }
        return isNegative ? result * -1 : result;
    }

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

        string sanitised = (isNegative || isPositive) ? s.Substring(1) : s;

        int radix = 10;
        if (baseKind == NumberBase.Hex)
        {
            radix = 16;
            if (sanitised.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                sanitised = sanitised.Substring(2);
        }
        else if (baseKind == NumberBase.Octal)
        {
            radix = 8;
            if (sanitised.StartsWith("0o", StringComparison.OrdinalIgnoreCase))
                sanitised = sanitised.Substring(2);
        }
        else if (baseKind == NumberBase.Binary)
        {
            radix = 2;
            if (sanitised.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
                sanitised = sanitised.Substring(2);
        }

        return (sanitised, radix, isNegative);
    }

    public string ToCanonicalString()
    {
        if (RawValue.StartsWith('#'))
            return RawValue;

        var numberBase = GetBase();
        var (clean, radix, isNegative) = Sanitise(RawValue, numberBase);

        if (numberBase == NumberBase.Decimal)
        {
            return isNegative ? '-' + clean : clean;
        }
        else
        {
            return TryDecimal() ?? TryBigInteger() ?? RawValue;
        }

        string? TryDecimal()
        {
            try
            {
                var dec = ToDecimal();
                return dec.ToString(CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                return null;
            }
        }
        string? TryBigInteger()
        {
            try
            {
                var bi = System.Numerics.BigInteger.Parse(
                    "0" + clean,
                    radix switch
                    {
                        2 => NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                        8 => NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                        16 => NumberStyles.AllowHexSpecifier,
                        _ => NumberStyles.Integer,
                    }
                );

                if (isNegative)
                    bi = -bi;

                return bi.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }
    }
}

using Kuddle.AST;

namespace Kuddle.Tests;

public class KdlNumberTests
{
    #region Successful Conversions - All Number Bases

    [Test]
    [Arguments("42", 42)]
    [Arguments("0x2A", 42)]
    [Arguments("0o52", 42)]
    [Arguments("0b101010", 42)]
    [Arguments("-42", -42)]
    [Arguments("-0x2A", -42)]
    [Arguments("-0o52", -42)]
    [Arguments("-0b101010", -42)]
    public async Task ToInt32_ConvertsAllBases(string input, int expected)
    {
        var sut = new KdlNumber(input);
        int result = sut.ToInt32();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (uint)42)]
    [Arguments("0x2A", (uint)42)]
    [Arguments("0o52", (uint)42)]
    [Arguments("0b101010", (uint)42)]
    public async Task ToUInt32_ConvertsAllBases(string input, uint expected)
    {
        var sut = new KdlNumber(input);
        uint result = sut.ToUInt32();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (long)42)]
    [Arguments("0x2A", (long)42)]
    [Arguments("0o52", (long)42)]
    [Arguments("0b101010", (long)42)]
    [Arguments("-42", (long)-42)]
    [Arguments("-0x2A", (long)-42)]
    [Arguments("-0o52", (long)-42)]
    [Arguments("-0b101010", (long)-42)]
    public async Task ToInt64_ConvertsAllBases(string input, long expected)
    {
        var sut = new KdlNumber(input);
        var result = sut.ToInt64();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (ulong)42)]
    [Arguments("0x2A", (ulong)42)]
    [Arguments("0o52", (ulong)42)]
    [Arguments("0b101010", (ulong)42)]
    public async Task ToUInt64_ConvertsAllBases(string input, ulong expected)
    {
        var sut = new KdlNumber(input);
        ulong result = sut.ToUInt64();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (short)42)]
    [Arguments("0x2A", (short)42)]
    [Arguments("0o52", (short)42)]
    [Arguments("0b101010", (short)42)]
    [Arguments("-42", (short)-42)]
    [Arguments("-0x2A", (short)-42)]
    [Arguments("-0o52", (short)-42)]
    [Arguments("-0b101010", (short)-42)]
    public async Task ToInt16_ConvertsAllBases(string input, short expected)
    {
        var sut = new KdlNumber(input);
        short result = sut.ToInt16();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (ushort)42)]
    [Arguments("0x2A", (ushort)42)]
    [Arguments("0o52", (ushort)42)]
    [Arguments("0b101010", (ushort)42)]
    public async Task ToUInt16_ConvertsAllBases(string input, ushort expected)
    {
        var sut = new KdlNumber(input);
        ushort result = sut.ToUInt16();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (byte)42)]
    [Arguments("0x2A", (byte)42)]
    [Arguments("0o52", (byte)42)]
    [Arguments("0b101010", (byte)42)]
    public async Task ToByte_ConvertsAllBases(string input, byte expected)
    {
        var sut = new KdlNumber(input);
        byte result = sut.ToByte();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (sbyte)42)]
    [Arguments("0x2A", (sbyte)42)]
    [Arguments("0o52", (sbyte)42)]
    [Arguments("0b101010", (sbyte)42)]
    [Arguments("-42", (sbyte)-42)]
    [Arguments("-0x2A", (sbyte)-42)]
    [Arguments("-0o52", (sbyte)-42)]
    [Arguments("-0b101010", (sbyte)-42)]
    public async Task ToSByte_ConvertsAllBases(string input, sbyte expected)
    {
        var sut = new KdlNumber(input);
        sbyte result = sut.ToSByte();
        await Assert.That(result).IsEqualTo(expected);
    }

    #endregion

    #region Overflow and Invalid Conversion Tests

    [Test]
    [Arguments("256")]
    [Arguments("-1")]
    public async Task ToByte_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToByte()).Throws<OverflowException>();
    }

    [Test]
    [Arguments("65536")]
    [Arguments("-1")]
    public async Task ToUInt16_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToUInt16()).Throws<OverflowException>();
    }

    [Test]
    [Arguments("50000")]
    [Arguments("-50000")]
    [Arguments("32768")]
    [Arguments("-32769")]
    public async Task ToInt16_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToInt16()).Throws<OverflowException>();
    }

    [Test]
    [Arguments("-42")]
    [Arguments("4294967296")]
    [Arguments("-1")]
    public async Task ToUInt32_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToUInt32()).Throws<OverflowException>();
    }

    [Test]
    [Arguments("-42")]
    [Arguments("18446744073709551616")]
    [Arguments("-1")]
    public async Task ToUInt64_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToUInt64()).Throws<OverflowException>();
    }

    [Test]
    [Arguments("2147483648")]
    [Arguments("-2147483649")]
    public async Task ToInt32_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToInt32()).Throws<OverflowException>();
    }

    [Test]
    [Arguments("9_223_372_036_854_775_808")]
    [Arguments("-9_223_372_036_854_775_809")]
    public async Task ToInt64_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToInt64()).Throws<OverflowException>();
    }

    #endregion

    #region Special Number Handling Tests

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToInt32_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToInt32()).Throws<FormatException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToUInt32_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToUInt32()).Throws<FormatException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToInt64_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToInt64()).Throws<FormatException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToUInt64_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToUInt64()).Throws<FormatException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToInt16_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToInt16()).Throws<FormatException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToUInt16_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToUInt16()).Throws<FormatException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToByte_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToByte()).Throws<FormatException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToSByte_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToSByte()).Throws<FormatException>();
    }

    [Test]
    public async Task ToDouble_HandlesInfinity()
    {
        var posInf = new KdlNumber("#inf");
        var negInf = new KdlNumber("#-inf");
        var nan = new KdlNumber("#nan");

        await Assert.That(posInf.ToDouble()).IsEqualTo(double.PositiveInfinity);
        await Assert.That(negInf.ToDouble()).IsEqualTo(double.NegativeInfinity);
        await Assert.That(double.IsNaN(nan.ToDouble())).IsTrue();
    }

    [Test]
    public async Task ToFloat_HandlesInfinity()
    {
        var posInf = new KdlNumber("#inf");
        var negInf = new KdlNumber("#-inf");
        var nan = new KdlNumber("#nan");

        await Assert.That(posInf.ToFloat()).IsEqualTo(float.PositiveInfinity);
        await Assert.That(negInf.ToFloat()).IsEqualTo(float.NegativeInfinity);
        await Assert.That(float.IsNaN(nan.ToFloat())).IsTrue();
    }

    #endregion

    #region Edge Case Tests

    [Test]
    [Arguments("0")]
    [Arguments("0x0")]
    [Arguments("0o0")]
    [Arguments("0b0")]
    public async Task ToInt32_HandlesZero(string input)
    {
        var sut = new KdlNumber(input);
        var result = sut.ToInt32();
        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    [Arguments("2147483647")]
    [Arguments("-2147483648")]
    public async Task ToInt32_HandlesBoundaries(string input)
    {
        var sut = new KdlNumber(input);
        var result = sut.ToInt32();
        await Assert.That(result).IsEqualTo(int.Parse(input));
    }

    [Test]
    [Arguments("4294967295")]
    public async Task ToUInt32_HandlesMaxBoundary(string input)
    {
        var sut = new KdlNumber(input);
        var result = sut.ToUInt32();
        await Assert.That(result).IsEqualTo(uint.MaxValue);
    }

    [Test]
    [Arguments("9223372036854775807")]
    [Arguments("-9223372036854775808")]
    public async Task ToInt64_HandlesBoundaries(string input)
    {
        var sut = new KdlNumber(input);
        var result = sut.ToInt64();
        await Assert.That(result).IsEqualTo(long.Parse(input));
    }

    [Test]
    [Arguments("18446744073709551615")]
    public async Task ToUInt64_HandlesMaxBoundary(string input)
    {
        var sut = new KdlNumber(input);
        var result = sut.ToUInt64();
        await Assert.That(result).IsEqualTo(ulong.MaxValue);
    }

    [Test]
    [Arguments("32767")]
    [Arguments("-32768")]
    public async Task ToInt16_HandlesBoundaries(string input)
    {
        var sut = new KdlNumber(input);
        var result = sut.ToInt16();
        await Assert.That(result).IsEqualTo(short.Parse(input));
    }

    [Test]
    [Arguments("65535")]
    public async Task ToUInt16_HandlesMaxBoundary(string input)
    {
        var sut = new KdlNumber(input);
        var result = sut.ToUInt16();
        await Assert.That(result).IsEqualTo(ushort.MaxValue);
    }

    [Test]
    [Arguments("127")]
    [Arguments("-128")]
    public async Task ToSByte_HandlesBoundaries(string input)
    {
        var sut = new KdlNumber(input);
        var result = sut.ToSByte();
        await Assert.That(result).IsEqualTo(sbyte.Parse(input));
    }

    [Test]
    [Arguments("255")]
    public async Task ToByte_HandlesMaxBoundary(string input)
    {
        var sut = new KdlNumber(input);
        var result = sut.ToByte();
        await Assert.That(result).IsEqualTo(byte.MaxValue);
    }

    #endregion

    #region Underscore Separator Tests

    [Test]
    [Arguments("1_000_000", 1000000)]
    [Arguments("0xFF_FF", 65535)]
    [Arguments("0o777_777", 262143)]
    [Arguments("0b1111_0000_1010_0101", 61605)]
    public async Task ToInt32_HandlesUnderscores(string input, int expected)
    {
        var sut = new KdlNumber(input);
        var result = sut.ToInt32();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("1_000_000_000_000", 1000000000000)]
    [Arguments("0xFFFF_FFFF_FFFF", 281474976710655)]
    public async Task ToInt64_HandlesUnderscores(string input, long expected)
    {
        var sut = new KdlNumber(input);
        var result = sut.ToInt64();
        await Assert.That(result).IsEqualTo(expected);
    }

    #endregion

    #region Floating-Point Number Tests

    [Test]
    [Arguments("3.14159", 3.14159)]
    [Arguments("1.23e-4", 0.000123)]
    [Arguments("6.02E23", 6.02e23)]
    [Arguments("-2.5", -2.5)]
    [Arguments("0.0", 0.0)]
    public async Task ToDouble_ConvertsDecimalNumbers(string input, double expected)
    {
        var sut = new KdlNumber(input);
        double result = sut.ToDouble();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("3.14159", 3.14159f)]
    [Arguments("1.23e-4", 0.000123f)]
    [Arguments("6.02E23", 6.02e23f)]
    [Arguments("-2.5", -2.5f)]
    [Arguments("0.0", 0.0f)]
    public async Task ToFloat_ConvertsDecimalNumbers(string input, float expected)
    {
        var sut = new KdlNumber(input);
        float result = sut.ToFloat();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("3.14159", 3.14159)]
    [Arguments("1.23e-4", 0.000123)]
    [Arguments("6.02E23", 6.02e23)]
    [Arguments("-2.5", -2.5)]
    [Arguments("0.0", 0.0)]
    public async Task ToDecimal_ConvertsDecimalNumbers(string input, decimal expected)
    {
        var sut = new KdlNumber(input);
        decimal result = sut.ToDecimal();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (double)42)]
    [Arguments("0x2A", (double)42)]
    [Arguments("0o52", (double)42)]
    [Arguments("0b101010", (double)42)]
    [Arguments("-42", (double)-42)]
    [Arguments("-0x2A", (double)-42)]
    [Arguments("-0o52", (double)-42)]
    [Arguments("-0b101010", (double)-42)]
    public async Task ToDouble_ConvertsAllBases(string input, double expected)
    {
        var sut = new KdlNumber(input);
        double result = sut.ToDouble();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (float)42)]
    [Arguments("0x2A", (float)42)]
    [Arguments("0o52", (float)42)]
    [Arguments("0b101010", (float)42)]
    [Arguments("-42", (float)-42)]
    [Arguments("-0x2A", (float)-42)]
    [Arguments("-0o52", (float)-42)]
    [Arguments("-0b101010", (float)-42)]
    public async Task ToFloat_ConvertsAllBases(string input, float expected)
    {
        var sut = new KdlNumber(input);
        double result = sut.ToFloat();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", 42)]
    [Arguments("0x2A", 42)]
    [Arguments("0o52", 42)]
    [Arguments("0b101010", 42)]
    [Arguments("-42", -42)]
    [Arguments("-0x2A", -42)]
    [Arguments("-0o52", -42)]
    [Arguments("-0b101010", -42)]
    public async Task ToDecimal_ConvertsAllBases(string input, decimal expected)
    {
        var sut = new KdlNumber(input);
        decimal result = sut.ToDecimal();
        await Assert.That(result).IsEqualTo(expected);
    }

    #endregion

    #region Float to Integer Conversion Tests

    [Test]
    [Arguments("3.14159")]
    [Arguments("1.23e-4")]
    [Arguments("6.02E23")]
    [Arguments("-2.5")]
    public async Task ToInt32_ThrowsOnFloatNumbers(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToInt32()).Throws<FormatException>();
    }

    [Test]
    [Arguments("3.14159")]
    [Arguments("1.23e-4")]
    [Arguments("6.02E23")]
    [Arguments("-2.5")]
    public async Task ToInt64_ThrowsOnFloatNumbers(string input)
    {
        var sut = new KdlNumber(input);
        await Assert.That(() => sut.ToInt64()).Throws<FormatException>();
    }

    #endregion
}

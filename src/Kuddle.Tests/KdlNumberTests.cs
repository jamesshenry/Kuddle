using System;
using System.Numerics;
using Kuddle.AST;

namespace Kuddle.Tests;

public class KdlNumberTests
{
    #region Successful Conversions - All Number Bases

    [Test]
    [Arguments("42", 42)] // Decimal
    [Arguments("0x2A", 42)] // Hex
    [Arguments("0o52", 42)] // Octal
    [Arguments("0b101010", 42)] // Binary
    public async Task ToInt32_ConvertsAllBases(string input, int expected)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        int result = sut.ToInt32();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (uint)42)] // Decimal
    [Arguments("0x2A", (uint)42)] // Hex
    [Arguments("0o52", (uint)42)] // Octal
    [Arguments("0b101010", (uint)42)] // Binary
    public async Task ToUInt32_ConvertsAllBases(string input, uint expected)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        uint result = sut.ToUInt32();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (long)42)] // Decimal
    [Arguments("0x2A", (long)42)] // Hex
    [Arguments("0o52", (long)42)] // Octal
    [Arguments("0b101010", (long)42)] // Binary
    public async Task ToInt64_ConvertsAllBases(string input, long expected)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToInt64();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (ulong)42)] // Decimal
    [Arguments("0x2A", (ulong)42)] // Hex
    [Arguments("0o52", (ulong)42)] // Octal
    [Arguments("0b101010", (ulong)42)] // Binary
    public async Task ToUInt64_ConvertsAllBases(string input, ulong expected)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        ulong result = sut.ToUInt64();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (short)42)] // Decimal
    [Arguments("0x2A", (short)42)] // Hex
    [Arguments("0o52", (short)42)] // Octal
    [Arguments("0b101010", (short)42)] // Binary
    public async Task ToInt16_ConvertsAllBases(string input, short expected)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        short result = sut.ToInt16();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (ushort)42)] // Decimal
    [Arguments("0x2A", (ushort)42)] // Hex
    [Arguments("0o52", (ushort)42)] // Octal
    [Arguments("0b101010", (ushort)42)] // Binary
    public async Task ToUInt16_ConvertsAllBases(string input, ushort expected)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        ushort result = sut.ToUInt16();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (byte)42)] // Decimal
    [Arguments("0x2A", (byte)42)] // Hex
    [Arguments("0o52", (byte)42)] // Octal
    [Arguments("0b101010", (byte)42)] // Binary
    public async Task ToByte_ConvertsAllBases(string input, byte expected)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        byte result = sut.ToByte();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", (sbyte)42)] // Decimal
    [Arguments("0x2A", (sbyte)42)] // Hex
    [Arguments("0o52", (sbyte)42)] // Octal
    [Arguments("0b101010", (sbyte)42)] // Binary
    public async Task ToSByte_ConvertsAllBases(string input, sbyte expected)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        sbyte result = sut.ToSByte();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42", 42)] // Decimal
    [Arguments("0x2A", 42)] // Hex
    [Arguments("0o52", 42)] // Octal
    [Arguments("0b101010", 42)] // Binary
    public async Task ToBigInteger_ConvertsAllBases(string input, BigInteger expected)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToBigInteger();
        await Assert.That(result).IsEqualTo(expected);
    }

    #endregion

    #region Overflow and Invalid Conversion Tests

    [Test]
    [Arguments("-42")] // Negative to unsigned
    [Arguments("300")] // Too large for byte
    [Arguments("-200")] // Too small for sbyte
    [Arguments("256")] // Too large for byte (boundary)
    public async Task ToByte_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        await Assert.That(() => sut.ToByte()).Throws<OverflowException>();
    }

    [Test]
    [Arguments("-42")] // Negative to unsigned
    [Arguments("200_000")] // Too large for ushort
    [Arguments("-200_000")] // Too small for sbyte
    [Arguments("65_536")] // Too large for ushort (boundary)
    public async Task ToUInt16_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        await Assert.That(() => sut.ToUInt16()).Throws<OverflowException>();
    }

    [Test]
    [Arguments("-42")] // Negative to unsigned
    [Arguments("50_000")] // Too large for short
    [Arguments("-50_000")] // Too small for short
    [Arguments("32_768")] // Too large for short (boundary)
    [Arguments("-32_769")] // Too small for short (boundary)
    public async Task ToInt16_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        await Assert.That(() => sut.ToInt16()).Throws<OverflowException>();
    }

    [Test]
    [Arguments("-42")] // Negative to unsigned
    [Arguments("4_294_967_296")] // Larger than uint.MaxValue
    [Arguments("-1")] // Negative to unsigned
    public async Task ToUInt32_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        await Assert.That(() => sut.ToUInt32()).Throws<OverflowException>();
    }

    [Test]
    [Arguments("-42")] // Negative to unsigned
    [Arguments("18_446_744_073_709_551_616")] // Larger than ulong.MaxValue
    [Arguments("-1")] // Negative to unsigned
    public async Task ToUInt64_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        await Assert.That(() => sut.ToUInt64()).Throws<OverflowException>();
    }

    [Test]
    [Arguments("2_147_483_648")] // Larger than int.MaxValue
    [Arguments("-2_147_483_649")] // Smaller than int.MinValue
    public async Task ToInt32_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        await Assert.That(() => sut.ToInt32()).Throws<OverflowException>();
    }

    [Test]
    [Arguments("9_223_372_036_854_775_808")] // Larger than long.MaxValue
    [Arguments("-9_223_372_036_854_775_809")] // Smaller than long.MinValue
    public async Task ToInt64_ThrowsOnInvalidValues(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
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
        var sut = new KdlNumber(input, NumberKind.Special);
        await Assert.That(() => sut.ToInt32()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToUInt32_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Special);
        await Assert.That(() => sut.ToUInt32()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToInt64_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Special);
        await Assert.That(() => sut.ToInt64()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToUInt64_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Special);
        await Assert.That(() => sut.ToUInt64()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToInt16_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Special);
        await Assert.That(() => sut.ToInt16()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToUInt16_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Special);
        await Assert.That(() => sut.ToUInt16()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToByte_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Special);
        await Assert.That(() => sut.ToByte()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToSByte_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Special);
        await Assert.That(() => sut.ToSByte()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToBigInteger_ThrowsOnSpecialNumbers(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Special);
        await Assert.That(() => sut.ToBigInteger()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ToDouble_HandlesInfinity()
    {
        var posInf = new KdlNumber("#inf", NumberKind.Special);
        var negInf = new KdlNumber("#-inf", NumberKind.Special);
        var nan = new KdlNumber("#nan", NumberKind.Special);

        await Assert.That(posInf.ToDouble()).IsEqualTo(double.PositiveInfinity);
        await Assert.That(negInf.ToDouble()).IsEqualTo(double.NegativeInfinity);
        await Assert.That(double.IsNaN(nan.ToDouble())).IsTrue();
    }

    [Test]
    public async Task ToFloat_HandlesInfinity()
    {
        var posInf = new KdlNumber("#inf", NumberKind.Special);
        var negInf = new KdlNumber("#-inf", NumberKind.Special);
        var nan = new KdlNumber("#nan", NumberKind.Special);

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
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToInt32();
        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    [Arguments("2_147_483_647")] // int.MaxValue
    [Arguments("-2_147_483_648")] // int.MinValue
    public async Task ToInt32_HandlesBoundaries(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToInt32();
        await Assert.That(result).IsEqualTo(int.Parse(input));
    }

    [Test]
    [Arguments("4_294_967_295")] // uint.MaxValue
    public async Task ToUInt32_HandlesMaxBoundary(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToUInt32();
        await Assert.That(result).IsEqualTo(uint.MaxValue);
    }

    [Test]
    [Arguments("9_223_372_036_854_775_807")] // long.MaxValue
    [Arguments("-9_223_372_036_854_775_808")] // long.MinValue
    public async Task ToInt64_HandlesBoundaries(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToInt64();
        await Assert.That(result).IsEqualTo(long.Parse(input));
    }

    [Test]
    [Arguments("18_446_744_073_709_551_615")] // ulong.MaxValue
    public async Task ToUInt64_HandlesMaxBoundary(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToUInt64();
        await Assert.That(result).IsEqualTo(ulong.MaxValue);
    }

    [Test]
    [Arguments("32_767")] // short.MaxValue
    [Arguments("-32_768")] // short.MinValue
    public async Task ToInt16_HandlesBoundaries(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToInt16();
        await Assert.That(result).IsEqualTo(short.Parse(input));
    }

    [Test]
    [Arguments("65_535")] // ushort.MaxValue
    public async Task ToUInt16_HandlesMaxBoundary(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToUInt16();
        await Assert.That(result).IsEqualTo(ushort.MaxValue);
    }

    [Test]
    [Arguments("127")] // sbyte.MaxValue
    [Arguments("-128")] // sbyte.MinValue
    public async Task ToSByte_HandlesBoundaries(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToSByte();
        await Assert.That(result).IsEqualTo(sbyte.Parse(input));
    }

    [Test]
    [Arguments("255")] // byte.MaxValue
    public async Task ToByte_HandlesMaxBoundary(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToByte();
        await Assert.That(result).IsEqualTo(byte.MaxValue);
    }

    [Test]
    [Arguments("123_456_789_012_345_678_901_234_567_890")] // Very large number
    public async Task ToBigInteger_HandlesVeryLargeNumbers(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToBigInteger();
        await Assert.That(result.ToString()).IsEqualTo(input);
    }

    #endregion

    #region Underscore Separator Tests

    [Test]
    [Arguments("1_000_000", 1000000)]
    [Arguments("0xFF_FF", 65535)]
    [Arguments("0o777_777", 262143)]
    [Arguments("0b1111_0000_1010_0101", 61621)]
    public async Task ToInt32_HandlesUnderscores(string input, int expected)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToInt32();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("1_000_000_000_000", 1000000000000)]
    [Arguments("0xFFFF_FFFF_FFFF", 281474976710655)]
    public async Task ToInt64_HandlesUnderscores(string input, long expected)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        var result = sut.ToInt64();
        await Assert.That(result).IsEqualTo(expected);
    }

    // [Test]
    // [Arguments("1_234_567_890_123_456_789", BigInteger.Parse("1234567890123456789"))]
    // [Arguments(
    //     "0xDE_AD_BE_EF",
    //     BigInteger.Parse("0xDEADBEEF", System.Globalization.NumberStyles.HexNumber)
    // )]
    // public async Task ToBigInteger_HandlesUnderscores(string input, BigInteger expected)
    // {
    //     var sut = new KdlNumber(input, NumberKind.Integer);
    //     var result = sut.ToBigInteger();
    //     await Assert.That(result).IsEqualTo(expected);
    // }

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
        var sut = new KdlNumber(input, NumberKind.Decimal);
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
        var sut = new KdlNumber(input, NumberKind.Decimal);
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
        var sut = new KdlNumber(input, NumberKind.Decimal);
        decimal result = sut.ToDecimal();
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments("42")] // Integer format
    [Arguments("0xFF")] // Hex format
    [Arguments("0o52")] // Octal format
    [Arguments("0b101010")] // Binary format
    public async Task ToDouble_ThrowsOnIntegerFormat(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        await Assert.That(() => sut.ToDouble()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("42")] // Integer format
    [Arguments("0xFF")] // Hex format
    [Arguments("0o52")] // Octal format
    [Arguments("0b101010")] // Binary format
    public async Task ToFloat_ThrowsOnIntegerFormat(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        await Assert.That(() => sut.ToFloat()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("42")] // Integer format
    [Arguments("0xFF")] // Hex format
    [Arguments("0o52")] // Octal format
    [Arguments("0b101010")] // Binary format
    public async Task ToDecimal_ThrowsOnIntegerFormat(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        await Assert.That(() => sut.ToDecimal()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToDouble_ThrowsOnSpecialNumbersFromInteger(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        await Assert.That(() => sut.ToDouble()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToFloat_ThrowsOnSpecialNumbersFromInteger(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        await Assert.That(() => sut.ToFloat()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("#inf")]
    [Arguments("#-inf")]
    [Arguments("#nan")]
    public async Task ToDecimal_ThrowsOnSpecialNumbersFromInteger(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Integer);
        await Assert.That(() => sut.ToDecimal()).Throws<InvalidOperationException>();
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
        var sut = new KdlNumber(input, NumberKind.Decimal);
        await Assert.That(() => sut.ToInt32()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("3.14159")]
    [Arguments("1.23e-4")]
    [Arguments("6.02E23")]
    [Arguments("-2.5")]
    public async Task ToInt64_ThrowsOnFloatNumbers(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Decimal);
        await Assert.That(() => sut.ToInt64()).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments("3.14159")]
    [Arguments("1.23e-4")]
    [Arguments("6.02E23")]
    [Arguments("-2.5")]
    public async Task ToBigInteger_ThrowsOnFloatNumbers(string input)
    {
        var sut = new KdlNumber(input, NumberKind.Decimal);
        await Assert.That(() => sut.ToBigInteger()).Throws<InvalidOperationException>();
    }

    #endregion
}

using Kuddle.Parser;

namespace Kuddle.Tests;

public class NumberParsersTests
{
    // Note: These tests are stubs until NumberParsers is implemented
    // They represent the number parsing rules from the KDL grammar:
    // number := keyword-number | hex | octal | binary | decimal
    // decimal := sign? integer ('.' integer)? exponent?
    // hex := sign? '0x' hex-digit (hex-digit | '_')*
    // octal := sign? '0o' [0-7] [0-7_]*
    // binary := sign? '0b' ('0' | '1') ('0' | '1' | '_')*
    // keyword-number := '#inf' | '#-inf' | '#nan'

    [Test]
    public async Task Decimal_ParsesPositiveInteger()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "42";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_ParsesNegativeInteger()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "-42";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_ParsesFractionalNumber()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "3.14159";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_ParsesScientificNotation()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "1.23e-4";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_ParsesScientificNotationUppercase()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "6.02E23";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_ParsesWithUnderscoreSeparators()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "1_000_000";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_ParsesFractionalWithUnderscores()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "12_34.56_78";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Hex_ParsesHexNumbers()
    {
        var sut = KuddleGrammar.Hex;

        var input = "0xFF";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Hex_ParsesHexWithUnderscores()
    {
        var sut = KuddleGrammar.Hex;

        var input = "0x123_ABC";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Hex_ParsesNegativeHex()
    {
        var sut = KuddleGrammar.Hex;

        var input = "-0x42";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Octal_ParsesOctalNumbers()
    {
        var sut = KuddleGrammar.Octal;

        var input = "0o777";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Octal_ParsesOctalWithUnderscores()
    {
        var sut = KuddleGrammar.Octal;

        var input = "0o123_456";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Octal_ParsesNegativeOctal()
    {
        var sut = KuddleGrammar.Octal;

        var input = "-0o42";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Binary_ParsesBinaryNumbers()
    {
        var sut = KuddleGrammar.Binary;

        var input = "0b1010";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Binary_ParsesBinaryWithUnderscores()
    {
        var sut = KuddleGrammar.Binary;

        var input = "0b1111_0000";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Binary_ParsesNegativeBinary()
    {
        var sut = KuddleGrammar.Binary;

        var input = "-0b101";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task KeywordNumber_ParsesInfinity()
    {
        var sut = KuddleGrammar.KeywordNumber;

        var input = "#inf";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value).IsEqualTo(input);
    }

    [Test]
    public async Task KeywordNumber_ParsesNegativeInfinity()
    {
        var sut = KuddleGrammar.KeywordNumber;

        var input = "#-inf";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value).IsEqualTo(input);
    }

    [Test]
    public async Task KeywordNumber_ParsesNaN()
    {
        var sut = KuddleGrammar.KeywordNumber;

        var input = "#nan";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value).IsEqualTo(input);
    }

    [Test]
    public async Task Number_ParsesDecimal()
    {
        var sut = KuddleGrammar.Number;

        var input = "42";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Number_ParsesHex()
    {
        var sut = KuddleGrammar.Number;

        var input = "0xFF";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Number_ParsesKeywordNumber()
    {
        var sut = KuddleGrammar.Number;

        var input = "#inf";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_RejectsDoubleDots()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "12.34.56";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsFalse();
    }

    [Test]
    public async Task Decimal_RejectsConsecutiveUnderscores()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "1__2";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsFalse();
    }
}

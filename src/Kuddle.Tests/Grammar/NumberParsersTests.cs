using Kuddle.Parser;

namespace Kuddle.Tests.Grammar;

public class NumberParsersTests
{
    [Test]
    public async Task Decimal_ParsesPositiveInteger()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "42";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_ParsesNegativeInteger()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "-42";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_ParsesFractionalNumber()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "3.14159";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_ParsesScientificNotation()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "1.23e-4";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_ParsesScientificNotationUppercase()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "6.02E23";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_ParsesWithUnderscoreSeparators()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "1_000_000";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_ParsesFractionalWithUnderscores()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "12_34.56_78";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Hex_ParsesHexNumbers()
    {
        var sut = KuddleGrammar.Hex;

        var input = "0xFF";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Hex_ParsesHexWithUnderscores()
    {
        var sut = KuddleGrammar.Hex;

        var input = "0x123_ABC";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Hex_ParsesNegativeHex()
    {
        var sut = KuddleGrammar.Hex;

        var input = "-0x42";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Octal_ParsesOctalNumbers()
    {
        var sut = KuddleGrammar.Octal;

        var input = "0o777";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Octal_ParsesOctalWithUnderscores()
    {
        var sut = KuddleGrammar.Octal;

        var input = "0o123_456";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Octal_ParsesNegativeOctal()
    {
        var sut = KuddleGrammar.Octal;

        var input = "-0o42";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Binary_ParsesBinaryNumbers()
    {
        var sut = KuddleGrammar.Binary;

        var input = "0b1010";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Binary_ParsesBinaryWithUnderscores()
    {
        var sut = KuddleGrammar.Binary;

        var input = "0b1111_0000";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Binary_ParsesNegativeBinary()
    {
        var sut = KuddleGrammar.Binary;

        var input = "-0b101";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task KeywordNumber_ParsesInfinity()
    {
        var sut = KuddleGrammar.KeywordNumber;

        var input = "#inf";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task KeywordNumber_ParsesNegativeInfinity()
    {
        var sut = KuddleGrammar.KeywordNumber;

        var input = "#-inf";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task KeywordNumber_ParsesNaN()
    {
        var sut = KuddleGrammar.KeywordNumber;

        var input = "#nan";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Number_ParsesDecimal()
    {
        var sut = KuddleGrammar.Decimal;

        var input = "42";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Number_ParsesHex()
    {
        var sut = KuddleGrammar.Hex;

        var input = "0xFF";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Number_ParsesKeywordNumber()
    {
        var sut = KuddleGrammar.KeywordNumber;

        var input = "#inf";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Decimal_RejectsDoubleDots()
    {
        var sut = KuddleGrammar.Decimal.Eof();

        var input = "12.34.56";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsFalse();
    }
}

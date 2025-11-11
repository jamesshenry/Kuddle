using Kuddle.Parser;

namespace Kuddle.Tests;

public class StringParsersTests
{
    // Note: These tests are stubs until StringParsers is implemented
    // They represent the string parsing rules from the KDL grammar:
    // string := identifier-string | quoted-string | raw-string
    // identifier-string := unambiguous-ident | signed-ident | dotted-ident
    // quoted-string := '"' single-line-string-body '"' | '"""' newline (multi-line-string-body newline)? (unicode-space | ws-escape)* '"""'
    // raw-string := '#' raw-string-quotes '#' | '#' raw-string '#'

    [Test]
    [Arguments("+positive")]
    [Arguments("-negative")]
    public async Task SignedIdent_ParsesSignedIdentifier(string input)
    {
        var sut = KuddleGrammar.SignedIdent;

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    [Arguments(".one")]
    [Arguments(".two")]
    [Arguments(".three")]
    public async Task DottedIdent_ParsesDottedIdentifier(string input)
    {
        var sut = KuddleGrammar.DottedIdent;

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    [Arguments(".12")]
    [Arguments(".01")]
    public async Task DottedIdent_DoesNotParseNumberDottedNumber(string input)
    {
        var sut = KuddleGrammar.DottedIdent;

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsFalse();
    }

    [Test]
    [Arguments("one")]
    [Arguments("two")]
    [Arguments("three")]
    [Arguments("world123")]
    [Arguments("test_case")]
    public async Task UnambiguousIdent_ParsesUnambiguousIdentifier(string input)
    {
        var sut = KuddleGrammar.UnambiguousIdent;

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    [Arguments("+positive")]
    [Arguments("-negative")]
    [Arguments(".one")]
    public async Task UnambiguousIdent_DoesNotParseInvalidIdentifier(string input)
    {
        var sut = KuddleGrammar.UnambiguousIdent;

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsFalse();
    }

    [Test]
    [Arguments("one")]
    [Arguments("two")]
    [Arguments("three")]
    [Arguments("world123")]
    [Arguments("test_case")]
    [Arguments(".one")]
    [Arguments(".two")]
    [Arguments(".three")]
    [Arguments("+positive")]
    [Arguments("-negative")]
    public async Task IdentifierString_ParsesIdentifierString(string input)
    {
        var sut = KuddleGrammar.IdentifierString;

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    [Arguments("true")]
    [Arguments("false")]
    public async Task IdentifierString_DoesNotParseDisallowedKeywordString(string input)
    {
        var sut = KuddleGrammar.UnambiguousIdent;

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsFalse();
    }

    // [Test]
    // public async Task IdentifierString_RejectsFalseKeyword()
    // {
    //     var sut = KuddleGrammar.IdentifierString;

    //     var input = "false";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsFalse();
    // }

    // [Test]
    // public async Task IdentifierString_RejectsNullKeyword()
    // {
    //     var sut = KuddleGrammar.IdentifierString;

    //     var input = "null";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsFalse();
    // }

    // [Test]
    // public async Task IdentifierString_RejectsInfKeyword()
    // {
    //     var sut = KuddleGrammar.IdentifierString;

    //     var input = "inf";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsFalse();
    // }

    // [Test]
    // public async Task IdentifierString_RejectsNegativeInfKeyword()
    // {
    //     var sut = KuddleGrammar.IdentifierString;

    //     var input = "-inf";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsFalse();
    // }

    // [Test]
    // public async Task IdentifierString_RejectsNanKeyword()
    // {
    //     var sut = KuddleGrammar.IdentifierString;

    //     var input = "nan";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsFalse();
    // }

    // [Test]
    // public async Task QuotedString_ParsesSingleLineString()
    // {
    //     var sut = KuddleGrammar.QuotedString;

    //     var input = "\"hello world\"";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsTrue();
    //     await Assert.That(value.ToString()).IsEqualTo(input);
    // }

    // [Test]
    // public async Task QuotedString_ParsesEmptyString()
    // {
    //     var sut = KuddleGrammar.QuotedString;

    //     var input = "\"\"";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsTrue();
    //     await Assert.That(value.ToString()).IsEqualTo(input);
    // }

    // [Test]
    // public async Task QuotedString_HandlesEscapeSequences()
    // {
    //     var sut = KuddleGrammar.QuotedString;

    //     var input = "\"\\n\\t\\\\\\\"\"";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsTrue();
    //     await Assert.That(value.ToString()).IsEqualTo(input);
    // }

    // [Test]
    // public async Task QuotedString_HandlesUnicodeEscapes()
    // {
    //     var sut = KuddleGrammar.QuotedString;

    //     var input = "\"\\u{1F600}\"";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsTrue();
    //     await Assert.That(value.ToString()).IsEqualTo(input);
    // }

    // [Test]
    // public async Task QuotedString_HandlesWhitespaceEscapes()
    // {
    //     var sut = KuddleGrammar.QuotedString;

    //     var input = "\"hello \\\nworld\"";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsTrue();
    //     await Assert.That(value.ToString()).IsEqualTo(input);
    // }

    // [Test]
    // public async Task RawString_ParsesSimpleRawString()
    // {
    //     var sut = KuddleGrammar.RawString;

    //     var input = "#\"hello world\"#";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsTrue();
    //     await Assert.That(value.ToString()).IsEqualTo(input);
    // }

    // [Test]
    // public async Task RawString_ParsesRawStringWithQuotes()
    // {
    //     var sut = KuddleGrammar.RawString;

    //     var input = "#\"content with \"quotes\"\"#";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsTrue();
    //     await Assert.That(value.ToString()).IsEqualTo(input);
    // }

    // [Test]
    // public async Task RawString_HandlesMultipleHashDelimiters()
    // {
    //     var sut = KuddleGrammar.RawString;

    //     var input = "##\"content with \" quotes\"##";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsTrue();
    //     await Assert.That(value.ToString()).IsEqualTo(input);
    // }

    // [Test]
    // public async Task String_ParsesIdentifierString()
    // {
    //     var sut = KuddleGrammar.String;

    //     var input = "hello";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsTrue();
    //     await Assert.That(value.ToString()).IsEqualTo(input);
    // }

    // [Test]
    // public async Task String_ParsesQuotedString()
    // {
    //     var sut = KuddleGrammar.String;

    //     var input = "\"hello world\"";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsTrue();
    //     await Assert.That(value.ToString()).IsEqualTo(input);
    // }

    // [Test]
    // public async Task String_ParsesRawString()
    // {
    //     var sut = KuddleGrammar.String;

    //     var input = "#\"hello world\"#";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsTrue();
    //     await Assert.That(value.ToString()).IsEqualTo(input);
    // }

    // [Test]
    // public async Task QuotedString_RejectsUnterminatedString()
    // {
    //     var sut = KuddleGrammar.QuotedString;

    //     var input = "\"unterminated string";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsFalse();
    // }

    // [Test]
    // public async Task RawString_RejectsMismatchedDelimiters()
    // {
    //     var sut = KuddleGrammar.RawString;

    //     var input = "#\"content\"##";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsFalse();
    // }
}

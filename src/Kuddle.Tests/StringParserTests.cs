using System.Diagnostics;
using Kuddle.Parser;

namespace Kuddle.Tests;

public class StringParserTests
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
    [Arguments("true")]
    [Arguments("false")]
    [Arguments("null")]
    [Arguments("inf")]
    [Arguments("-inf")]
    [Arguments("nan")]
    public async Task UnambiguousIdent_DoesNotParseDisallowedKeywordString(string input)
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
    [Arguments("\\   ")]
    public async Task WsEscape_ParsesWhiteSpace(string input)
    {
        var sut = KuddleGrammar.WsEscape;

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo("");
    }

    [Test]
    public async Task QuotedString_ParsesSingleLineString()
    {
        var sut = KuddleGrammar.QuotedString;

        const string input = """
"hello world"
""";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo("hello world");
    }

    [Test]
    public async Task QuotedString_ParsesEmptyString()
    {
        var sut = KuddleGrammar.QuotedString;

        const string input = """
""
""";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo("");
    }

    [Test]
    public async Task QuotedString_ParsesEmptyMultilineString()
    {
        var sut = KuddleGrammar.QuotedString;

        const string input = """"
"""
"""
"""";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo("");
    }

    [Test]
    [Arguments(
        """"
"""
Lorem ipsum canis canem edit
"""
""""
    )]
    //     [Arguments(
    //         """
    // "Lorem ipsum canis canem edit"
    // """
    //     )]
    public async Task QuotedString_HandlesEscapeSequences(string input)
    {
        var sut = KuddleGrammar.QuotedString;

        bool success = sut.TryParse(input, out var value);
        Debug.WriteLine(input);
        await Assert.That(success).IsTrue();
        await Assert
            .That(value.ToString())
            .IsEqualTo(
                """
Lorem ipsum canis canem edit
"""
            );
    }

    [Test]
    public async Task QuotedString_HandlesUnicodeEscapes()
    {
        var sut = KuddleGrammar.QuotedString;

        var input = "\"\\u{1F600}\"";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo("😀");
    }

    [Test]
    [Arguments(
        """
Hello \       World
"""
    )]
    [Arguments(
        """
            "Hello\nWorld"
            """
    )]
    [Arguments(
        """
"Hello\n\
    World"
"""
    )]
    public async Task QuotedString_HandlesWhitespaceEscapes(string input)
    {
        var sut = KuddleGrammar.QuotedString;

        bool success = sut.TryParse(input, out var value);

        var expected = """
Hello
World
""";
        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(expected);
    }

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

using System.Diagnostics;
using Kuddle.AST;
using Kuddle.Parser;

namespace Kuddle.Tests.Grammar;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

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
    [Arguments("\"\\   \"")]
    public async Task WsEscape_ParsesWhiteSpace(string input)
    {
        var sut = KuddleGrammar.QuotedString;

        bool success = sut.TryParse(input, out var value, out var error);

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
i
am
"""
"""",
        "i\nam"
    )]
    [Arguments(
        """"
"""
so am
                 I!
"""
"""",
        "so am\n                 I!"
    )]
    [Arguments(
        """"
"""
        foo
    This is the base indentation
            bar 
    """
"""",
        "    foo\nThis is the base indentation\n        bar "
    )]
    public async Task MultiLineStringBody_HandlesVarious(string input, string expected)
    {
        var sut = KuddleGrammar.MultiLineQuoted;

        bool success = sut.TryParse(input, out var value);
        Debug.WriteLine(input);
        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments(
        """"
"""
lorem ipsum
"""
"""",
        "lorem ipsum"
    )]
    [Arguments(
        """"
"""
Lorem ipsum
canis canem edit
"""
"""",
        "Lorem ipsum\ncanis canem edit"
    )]
    public async Task MultiLineQuotedString_CanParseMultiLine(string input, string expected)
    {
        var sut = KuddleGrammar.MultiLineQuoted;
        bool success = sut.TryParse(input, out var value, out var error);
        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments(
        """
"\u{1F600}"
""",
        "😀"
    )]
    public async Task QuotedString_HandlesUnicodeEscapes(string input, string expected)
    {
        var sut = KuddleGrammar.QuotedString;

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(expected);
    }

    [Test]
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

        bool success = sut.TryParse(input, out var value, out var error);

        var expected = "Hello\nWorld";
        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task RawString_ParsesSimpleRawString()
    {
        var sut = KuddleGrammar.RawString;

        var input = """
#"\n will be literal"#
""";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(@"\n will be literal");
    }

    [Test]
    public async Task RawString_ParsesRawStringWithQuotes()
    {
        var sut = KuddleGrammar.RawString;

        var input = "#\"content with \"quotes\"\"#";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo("content with \"quotes\"");
    }

    [Test]
    public async Task RawString_HandlesMultipleHashDelimiters()
    {
        var sut = KuddleGrammar.RawString;

        var input = """
##"hello\n\r\asd"#world"##
""";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert
            .That(value.ToString())
            .IsEqualTo(
                """
hello\n\r\asd"#world
"""
            );
    }

    [Test]
    public async Task RawString_ParsesMultiLineRawString()
    {
        var sut = KuddleGrammar.RawString;

        var input = """""
#"""
    Here's a """
        multiline string
        """
    without escapes.
    """#
""""";
        bool success = sut.TryParse(input, out var value);
        string expected = """"
Here's a """
    multiline string
    """
without escapes.
"""".Replace("\r\n", "\n");

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task IdentifierString_SetsStyleToBare()
    {
        var sut = KuddleGrammar.IdentifierString;
        var input = "bare_identifier";

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Value).IsEqualTo("bare_identifier");
        await Assert.That(value.Kind).IsEqualTo(StringKind.Bare);
    }

    [Test]
    public async Task QuotedString_SetsStyleToQuoted()
    {
        var sut = KuddleGrammar.QuotedString;
        var input = "\"quoted value\"";

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Value).IsEqualTo("quoted value");
        // Should be Quoted only
        await Assert.That(value.Kind).IsEqualTo(StringKind.Quoted);
    }

    [Test]
    public async Task MultiLineString_SetsStyleToMultiline()
    {
        var sut = KuddleGrammar.MultiLineQuoted;
        var input = """"
"""
content
"""
"""";

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Value).IsEqualTo("content");
        // Should be MultiLine only (not Raw)
        await Assert.That(value.Kind).IsEqualTo(StringKind.MultiLine);
    }

    [Test]
    public async Task RawString_SingleLine_SetsStyleToRawAndQuoted()
    {
        var sut = KuddleGrammar.RawString;
        var input = @"#""raw content""#";

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Value).IsEqualTo("raw content");

        // Use HasFlag to verify bitwise combination
        await Assert.That(value.Kind.HasFlag(StringKind.Raw)).IsTrue();
        await Assert.That(value.Kind.HasFlag(StringKind.Quoted)).IsTrue();
        await Assert.That(value.Kind.HasFlag(StringKind.MultiLine)).IsFalse();
    }

    [Test]
    public async Task RawString_MultiLine_SetsStyleToRawAndMultiline()
    {
        var sut = KuddleGrammar.RawString;
        var input = """"
#"""
multi
line
"""#
"""";

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Value).IsEqualTo("multi\nline");

        // Should be Raw AND MultiLine
        await Assert.That(value.Kind.HasFlag(StringKind.Raw)).IsTrue();
        await Assert.That(value.Kind.HasFlag(StringKind.MultiLine)).IsTrue();
        await Assert.That(value.Kind.HasFlag(StringKind.Quoted)).IsFalse();
    }

    [Test]
    public async Task String_UnifiedParser_DetectsBare()
    {
        var sut = KuddleGrammar.String;
        var input = "node_name";

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Kind).IsEqualTo(StringKind.Bare);
    }

    [Test]
    public async Task String_UnifiedParser_DetectsQuoted()
    {
        var sut = KuddleGrammar.String;
        var input = "\"node name\"";

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Kind).IsEqualTo(StringKind.Quoted);
    }

    [Test]
    public async Task String_UnifiedParser_DetectsRaw()
    {
        var sut = KuddleGrammar.String;
        var input = @"#""node name""#";

        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Kind.HasFlag(StringKind.Raw)).IsTrue();
    }
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
#pragma warning restore CS8602 // Dereference of a possibly null reference.

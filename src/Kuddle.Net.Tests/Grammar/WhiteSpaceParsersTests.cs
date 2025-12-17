using Kuddle.Parser;

namespace Kuddle.Tests.Grammar;

public class WhiteSpaceParsersTests
{
    // Note: These tests are stubs until WhiteSpaceParsers is implemented
    // They represent the whitespace parsing rules from the KDL grammar:
    // ws := unicode-space | multi-line-comment
    // escline := '\\' ws* (single-line-comment | newline | eof)
    // newline := See Table (All NewLine White_Space)
    // line-space := node-space | newline | single-line-comment
    // node-space := ws* escline ws* | ws+

    [Test]
    [Arguments('\u0009')]
    [Arguments('\u0020')]
    [Arguments('\u00A0')]
    [Arguments('\u1680')]
    [Arguments('\u2000')]
    [Arguments('\u2001')]
    [Arguments('\u2002')]
    [Arguments('\u2003')]
    [Arguments('\u2004')]
    [Arguments('\u2005')]
    [Arguments('\u2006')]
    [Arguments('\u2007')]
    [Arguments('\u2008')]
    [Arguments('\u2009')]
    [Arguments('\u200A')]
    [Arguments('\u202F')]
    [Arguments('\u205F')]
    [Arguments('\u3000')]
    public async Task Ws_ParsesUnicodeSpace(char input)
    {
        var sut = KdlGrammar.Ws;

        bool success = sut.TryParse(input.ToString(), out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input.ToString());
    }

    [Test]
    public async Task Ws_ParsesMultiLineComment()
    {
        var sut = KdlGrammar.Ws;

        var input = "/* comment */";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task EscLine_ParsesBackslashContinuation()
    {
        var sut = KdlGrammar.EscLine;

        var input =
            @"\
";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
    }

    [Test]
    public async Task EscLine_ParsesBackslashWithComment()
    {
        var sut = KdlGrammar.EscLine;

        var input = @"\    // comment";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task LineSpace_ParsesWhitespace()
    {
        var sut = KdlGrammar.LineSpace;

        var input = "   ";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task LineSpace_ParsesNewLine()
    {
        var sut = KdlGrammar.LineSpace;

        var input = "\n";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task LineSpace_ParsesComment()
    {
        var sut = KdlGrammar.LineSpace;

        var input = "// comment";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task NodeSpace_ParsesSimpleWhitespace()
    {
        var sut = KdlGrammar.NodeSpace;

        var input = "   ";
        bool success = sut.TryParse(input, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.Span.ToString()).IsEqualTo(input);
    }

    // [Test]
    // public async Task NodeSpace_ParsesEscapedNewLine()
    // {
    //     var sut = KuddleGrammar.NodeSpace;

    //     var input = "  \\\n  ";
    //     bool success = sut.TryParse(input, out var value);

    //     await Assert.That(success).IsTrue();
    //     await Assert.That(value.Span.ToString()).IsEqualTo(input);
    // }
}

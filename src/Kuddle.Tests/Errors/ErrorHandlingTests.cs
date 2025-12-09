using Kuddle.Exceptions;
using Kuddle.Parser;

namespace Kuddle.Tests.ErrorHandling;

public class KdlErrorTests
{
    private static async Task AssertParseFails(
        string kdl,
        string expectedMessagePart,
        int? expectedLine = null
    )
    {
        var ex = await Assert.ThrowsAsync<KdlParseException>(async () => KdlReader.Parse(kdl));

        await Assert
            .That(ex.Message)
            .Contains(expectedMessagePart, StringComparison.OrdinalIgnoreCase);

        if (expectedLine.HasValue)
        {
            await Assert.That(ex.Line).IsEqualTo(expectedLine.Value);
        }
    }

    #region Reserved Keywords (Your Custom Logic)

    [Test]
    public async Task ReservedKeyword_AsNodeName_ThrowsSpecificError()
    {
        var input = "true \"value\"";

        await AssertParseFails(input, "keyword 'true' cannot be used");
    }

    [Test]
    public async Task ReservedKeyword_AsPropKey_ThrowsSpecificError()
    {
        var input = "node null=10";

        await AssertParseFails(input, "keyword 'null' cannot be used");
    }

    [Test]
    public async Task ReservedKeyword_AsTypeAnnotation_ThrowsSpecificError()
    {
        var input = "(false)node";

        await AssertParseFails(input, "keyword 'false' cannot be used");
    }

    #endregion

    #region Structure & Blocks

    [Test]
    public async Task Block_Unclosed_Throws()
    {
        var input =
            @"
node {
    child
";

        await AssertParseFails(input, "expected", expectedLine: 3);
    }

    [Test]
    public async Task Block_MissingSpaceBefore_IsAllowed_But_MissingSemiColon_Throws()
    {
        var input = "node{child}";

        var broken = "node { child";
        await AssertParseFails(broken, "expected");
    }

    [Test]
    public async Task Property_MissingValue_Throws()
    {
        var input = "node key=";

        await AssertParseFails(input, "expected");
    }

    [Test]
    public async Task TypeAnnotation_Unclosed_Throws()
    {
        var input = "node (u8 value";

        await AssertParseFails(input, "expected");
    }

    #endregion

    #region String Literals

    [Test]
    public async Task String_UnclosedQuote_Throws()
    {
        var input = "node \"oops";

        await AssertParseFails(input, "expected");
    }

    [Test]
    public async Task String_InvalidEscape_Throws()
    {
        var input = "node \"hello \\q world\"";

        await AssertParseFails(input, "expected");
    }

    [Test]
    public async Task RawString_MismatchHashes_Throws()
    {
        var input = "node r##\"content\"#";

        await AssertParseFails(input, "expected");
    }

    [Test]
    public async Task MultilineString_Dedent_Invalid_Throws()
    {
        var input = "\"\"\"\n content";
        await AssertParseFails(input, "expected");
    }

    #endregion

    #region Numbers

    [Test]
    public async Task Hex_InvalidDigit_Throws()
    {
        var input = "node 0xG";

        await AssertParseFails(input, "expected");
    }

    [Test]
    public async Task Number_DoubleDot_Throws()
    {
        var input = "node 1.2.3";

        await AssertParseFails(input, "expected");
    }

    #endregion
}

using Kuddle.Exceptions;

namespace Kuddle.Tests.Errors;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

public class ErrorHandlingTests
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
        const string input = "true \"value\"";

        await AssertParseFails(input, "keyword 'true' cannot be used");
    }

    [Test]
    public async Task ReservedKeyword_AsPropKey_ThrowsSpecificError()
    {
        const string input = "node null=10";

        await AssertParseFails(input, "keyword 'null' cannot be used");
    }

    [Test]
    public async Task ReservedKeyword_AsTypeAnnotation_ThrowsSpecificError()
    {
        const string input = "(false)node";

        await AssertParseFails(input, "keyword 'false' cannot be used");
    }

    #endregion

    #region Structure & Blocks

    [Test]
    public async Task Block_Unclosed_Throws()
    {
        const string input =
            @"
node {
    child
";

        await AssertParseFails(input, "expected", expectedLine: 3);
    }

    [Test]
    public async Task Block_MissingSpaceBefore_IsAllowed_But_MissingSemiColon_Throws()
    {
        const string input = "node { child";

        await AssertParseFails(input, "expected");
    }

    [Test]
    public async Task Property_MissingValue_Throws()
    {
        const string input = "node key=";

        await AssertParseFails(input, "expected");
    }

    [Test]
    public async Task TypeAnnotation_Unclosed_Throws()
    {
        const string input = "node (u8 value";

        await AssertParseFails(input, "expected");
    }

    #endregion

    #region String Literals

    [Test]
    public async Task String_UnclosedQuote_Throws()
    {
        const string input = "node \"oops";

        await AssertParseFails(input, "expected");
    }

    [Test]
    public async Task String_InvalidEscape_Throws()
    {
        const string input = "node \"hello \\q world\"";

        await AssertParseFails(input, "expected");
    }

    [Test]
    public async Task RawString_MismatchHashes_Throws()
    {
        const string input = "node ##\"content\"#";

        await AssertParseFails(input, "expected");
    }

    // [Test]
    // public async Task MultilineString_Dedent_Invalid_Throws()
    // {
    //     const string input = "\"\"\"\n content";
    //     await AssertParseFails(input, "expected");
    // }

    #endregion

    // #region Numbers

    // [Test]
    // public async Task Hex_InvalidDigit_Throws()
    // {
    //     const string input = "node 0xG";

    //     await AssertParseFails(input, "expected");
    // }

    // [Test]
    // public async Task Number_DoubleDot_Throws()
    // {
    //     const string input = "node 1.2.3";

    //     await AssertParseFails(input, "expected");
    // }

    // #endregion
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.

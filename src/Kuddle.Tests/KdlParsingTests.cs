using Kuddle;

namespace Kuddle.Tests;

public class KdlParsingTests
{
    [Test]
    public async Task TestBlockComment() => await RunTestCases("block_comment");

    [Test]
    public async Task TestEscline() => await RunTestCases("escline");

    [Test]
    public async Task TestMultilineRawString() => await RunTestCases("multiline_raw_string");

    [Test]
    public async Task TestMultilineString() => await RunTestCases("multiline_string");

    [Test]
    public async Task TestArg() => await RunTestCases("arg");

    [Test]
    public async Task TestBareIdent() => await RunTestCases("bare_ident");

    [Test]
    public async Task TestBinary() => await RunTestCases("binary");

    [Test]
    public async Task TestBlank() => await RunTestCases("blank");

    [Test]
    public async Task TestBoolean() => await RunTestCases("boolean");

    [Test]
    public async Task TestBom() => await RunTestCases("bom");

    [Test]
    public async Task TestComment() => await RunTestCases("comment");

    [Test]
    public async Task TestCommented() => await RunTestCases("commented");

    [Test]
    public async Task TestEmpty() => await RunTestCases("empty");

    [Test]
    public async Task TestEsc() => await RunTestCases("esc");

    [Test]
    public async Task TestFalse() => await RunTestCases("false");

    [Test]
    public async Task TestIllegal() => await RunTestCases("illegal");

    [Test]
    public async Task TestBareEmoji() => await RunTestCases("bare_emoji");

    [Test]
    public async Task TestAll() => await RunTestCases("all");

    [Test]
    public async Task TestAsterisk() => await RunTestCases("asterisk");

    [Test]
    public async Task TestBraces() => await RunTestCases("braces");

    [Test]
    public async Task TestChevrons() => await RunTestCases("chevrons");

    [Test]
    public async Task TestComma() => await RunTestCases("comma");

    [Test]
    public async Task TestCrlf() => await RunTestCases("crlf");

    [Test]
    public async Task TestDash() => await RunTestCases("dash");

    [Test]
    public async Task TestDot() => await RunTestCases("dot");

    [Test]
    public async Task TestEmoji() => await RunTestCases("emoji");

    [Test]
    public async Task TestEof() => await RunTestCases("eof");

    [Test]
    public async Task TestErr() => await RunTestCases("err");

    [Test]
    public async Task TestEscaped() => await RunTestCases("escaped");

    [Test]
    public async Task TestHex() => await RunTestCases("hex");

    [Test]
    public async Task TestInitial() => await RunTestCases("initial");

    [Test]
    public async Task TestInt() => await RunTestCases("int");

    [Test]
    public async Task TestJust() => await RunTestCases("just");

    private async Task RunTestCases(string prefix)
    {
        var inputDir = "test_cases/input";
        var expectedDir = "test_cases/expected_kdl";
        var inputFiles = Directory.GetFiles(inputDir, $"{prefix}*.kdl");
        foreach (var inputFile in inputFiles)
        {
            var input = await File.ReadAllTextAsync(inputFile);
            var fileName = Path.GetFileName(inputFile);
            var expectedFile = Path.Combine(expectedDir, fileName);
            if (File.Exists(expectedFile))
            {
                var expected = await File.ReadAllTextAsync(expectedFile);
                var doc = KdlParser.V2().Parse(input);
                var serialized = doc.ToString(); // Assumes KdlDocument has ToString() for serialization
                await Assert.That(serialized).IsEqualTo(expected);
            }
            else
            {
                Assert.Throws<Exception>(() => KdlParser.V2().Parse(input));
            }
        }
    }
}

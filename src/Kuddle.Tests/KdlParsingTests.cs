using Kuddle;

namespace Kuddle.Tests;

public class KdlParsingTests
{
    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.BlockCommentTestData)
    )]
    public async Task TestBlockComment(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.EsclineTestData)
    )]
    public async Task TestEscline(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.MultilineRawStringTestData)
    )]
    public async Task TestMultilineRawString(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.MultilineStringTestData)
    )]
    public async Task TestMultilineString(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.ArgTestData))]
    public async Task TestArg(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString(); // Assumes KdlDocument has ToString() for serialization
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.BareIdentTestData)
    )]
    public async Task TestBareIdent(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.BinaryTestData)
    )]
    public async Task TestBinary(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.BlankTestData))]
    public async Task TestBlank(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.BooleanTestData)
    )]
    public async Task TestBoolean(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.BomTestData))]
    public async Task TestBom(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.CommentTestData)
    )]
    public async Task TestComment(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.CommentedTestData)
    )]
    public async Task TestCommented(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.EmptyTestData))]
    public async Task TestEmpty(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.EscTestData))]
    public async Task TestEsc(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.FalseTestData))]
    public async Task TestFalse(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.IllegalTestData)
    )]
    public async Task TestIllegal(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.BareEmojiTestData)
    )]
    public async Task TestBareEmoji(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.AllTestData))]
    public async Task TestAll(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.AsteriskTestData)
    )]
    public async Task TestAsterisk(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.BracesTestData)
    )]
    public async Task TestBraces(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.ChevronsTestData)
    )]
    public async Task TestChevrons(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.CommaTestData))]
    public async Task TestComma(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.CrlfTestData))]
    public async Task TestCrlf(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.DashTestData))]
    public async Task TestDash(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.DotTestData))]
    public async Task TestDot(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.EmojiTestData))]
    public async Task TestEmoji(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.EofTestData))]
    public async Task TestEof(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.ErrTestData))]
    public async Task TestErr(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.EscapedTestData)
    )]
    public async Task TestEscaped(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.HexTestData))]
    public async Task TestHex(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(ParsingTestDataSources),
        nameof(ParsingTestDataSources.InitialTestData)
    )]
    public async Task TestInitial(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.IntTestData))]
    public async Task TestInt(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(ParsingTestDataSources), nameof(ParsingTestDataSources.JustTestData))]
    public async Task TestJust(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KdlReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            var serialized = doc.ToString();
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KdlReader.Parse(testData.InputFile));
        }
    }
}

public record ParsingTestData(string InputFile, string ExpectedFile);

public static class ParsingTestDataSources
{
    public static IEnumerable<Func<ParsingTestData>> ArgTestData() => GetTestData("arg");

    public static IEnumerable<Func<ParsingTestData>> BlockCommentTestData() =>
        GetTestData("block_comment");

    public static IEnumerable<Func<ParsingTestData>> EsclineTestData() => GetTestData("escline");

    public static IEnumerable<Func<ParsingTestData>> MultilineRawStringTestData() =>
        GetTestData("multiline_raw_string");

    public static IEnumerable<Func<ParsingTestData>> MultilineStringTestData() =>
        GetTestData("multiline_string");

    public static IEnumerable<Func<ParsingTestData>> BareIdentTestData() =>
        GetTestData("bare_ident");

    public static IEnumerable<Func<ParsingTestData>> BinaryTestData() => GetTestData("binary");

    public static IEnumerable<Func<ParsingTestData>> BlankTestData() => GetTestData("blank");

    public static IEnumerable<Func<ParsingTestData>> BooleanTestData() => GetTestData("boolean");

    public static IEnumerable<Func<ParsingTestData>> BomTestData() => GetTestData("bom");

    public static IEnumerable<Func<ParsingTestData>> CommentTestData() => GetTestData("comment");

    public static IEnumerable<Func<ParsingTestData>> CommentedTestData() =>
        GetTestData("commented");

    public static IEnumerable<Func<ParsingTestData>> EmptyTestData() => GetTestData("empty");

    public static IEnumerable<Func<ParsingTestData>> EscTestData() => GetTestData("esc");

    public static IEnumerable<Func<ParsingTestData>> FalseTestData() => GetTestData("false");

    public static IEnumerable<Func<ParsingTestData>> IllegalTestData() => GetTestData("illegal");

    public static IEnumerable<Func<ParsingTestData>> BareEmojiTestData() =>
        GetTestData("bare_emoji");

    public static IEnumerable<Func<ParsingTestData>> AllTestData() => GetTestData("all");

    public static IEnumerable<Func<ParsingTestData>> AsteriskTestData() => GetTestData("asterisk");

    public static IEnumerable<Func<ParsingTestData>> BracesTestData() => GetTestData("braces");

    public static IEnumerable<Func<ParsingTestData>> ChevronsTestData() => GetTestData("chevrons");

    public static IEnumerable<Func<ParsingTestData>> CommaTestData() => GetTestData("comma");

    public static IEnumerable<Func<ParsingTestData>> CrlfTestData() => GetTestData("crlf");

    public static IEnumerable<Func<ParsingTestData>> DashTestData() => GetTestData("dash");

    public static IEnumerable<Func<ParsingTestData>> DotTestData() => GetTestData("dot");

    public static IEnumerable<Func<ParsingTestData>> EmojiTestData() => GetTestData("emoji");

    public static IEnumerable<Func<ParsingTestData>> EofTestData() => GetTestData("eof");

    public static IEnumerable<Func<ParsingTestData>> ErrTestData() => GetTestData("err");

    public static IEnumerable<Func<ParsingTestData>> EscapedTestData() => GetTestData("escaped");

    public static IEnumerable<Func<ParsingTestData>> HexTestData() => GetTestData("hex");

    public static IEnumerable<Func<ParsingTestData>> InitialTestData() => GetTestData("initial");

    public static IEnumerable<Func<ParsingTestData>> IntTestData() => GetTestData("int");

    public static IEnumerable<Func<ParsingTestData>> JustTestData() => GetTestData("just");

    private static IEnumerable<Func<ParsingTestData>> GetTestData(string prefix)
    {
        var inputDir = "test_cases/input";
        var expectedDir = "test_cases/expected_kdl";
        var inputFiles = Directory.GetFiles(inputDir, $"{prefix}*.kdl");

        foreach (var inputFile in inputFiles)
        {
            var fileName = Path.GetFileName(inputFile);
            var expectedFile = Path.Combine(expectedDir, fileName);
            yield return () => new ParsingTestData(inputFile, expectedFile);
        }
    }
}

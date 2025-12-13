using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization;

public class KuddleParsingTests
{
    readonly KuddleWriterOptions _options = new KuddleWriterOptions() with { RoundTrip = false };

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.BlockCommentTestData)
    )]
    public async Task TestBlockComment(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.EsclineTestData)
    )]
    public async Task TestEscline(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.MultilineRawStringTestData)
    )]
    public async Task TestMultilineRawString(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.MultilineStringTestData)
    )]
    public async Task TestMultilineString(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.ArgTestData))]
    public async Task TestArg(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options); // Assumes KdlDocument has ToString() for serialization
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.BareIdentTestData)
    )]
    public async Task TestBareIdent(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.BinaryTestData)
    )]
    public async Task TestBinary(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.BlankTestData))]
    public async Task TestBlank(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.BooleanTestData)
    )]
    public async Task TestBoolean(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.BomTestData))]
    public async Task TestBom(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.CommentTestData)
    )]
    public async Task TestComment(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.CommentedTestData)
    )]
    public async Task TestCommented(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.EmptyTestData))]
    public async Task TestEmpty(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.EscTestData))]
    public async Task TestEsc(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.FalseTestData))]
    public async Task TestFalse(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.IllegalTestData)
    )]
    public async Task TestIllegal(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.BareEmojiTestData)
    )]
    public async Task TestBareEmoji(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.AllTestData))]
    public async Task TestAll(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.AsteriskTestData)
    )]
    public async Task TestAsterisk(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.BracesTestData)
    )]
    public async Task TestBraces(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.ChevronsTestData)
    )]
    public async Task TestChevrons(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.CommaTestData))]
    public async Task TestComma(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.CrlfTestData))]
    public async Task TestCrlf(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.DashTestData))]
    public async Task TestDash(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.DotTestData))]
    public async Task TestDot(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.EmojiTestData))]
    public async Task TestEmoji(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.EofTestData))]
    public async Task TestEof(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.ErrTestData))]
    public async Task TestErr(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.EscapedTestData)
    )]
    public async Task TestEscaped(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.HexTestData))]
    public async Task TestHex(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(
        typeof(KuddleParsingTestDataSources),
        nameof(KuddleParsingTestDataSources.InitialTestData)
    )]
    public async Task TestInitial(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.IntTestData))]
    public async Task TestInt(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }

    [Test]
    [MethodDataSource(typeof(KuddleParsingTestDataSources), nameof(KuddleParsingTestDataSources.JustTestData))]
    public async Task TestJust(ParsingTestData testData)
    {
        if (File.Exists(testData.ExpectedFile))
        {
            var inputKdl = await File.ReadAllTextAsync(testData.InputFile);
            var doc = KuddleReader.Parse(inputKdl);
            var expected = await File.ReadAllTextAsync(testData.ExpectedFile);
            expected = expected.Replace("\r\n", "\n");
            var serialized = doc.ToString(_options);
            await Assert.That(serialized).IsEqualTo(expected);
        }
        else
        {
            Assert.Throws<Exception>(() => KuddleReader.Parse(testData.InputFile));
        }
    }
}

public record ParsingTestData(string InputFile, string ExpectedFile);

public static class KuddleParsingTestDataSources
{
    public static IEnumerable<Func<ParsingTestData>> ArgTestData() => GetTestData("arg");

    public static IEnumerable<Func<ParsingTestData>> BlockCommentTestData() =>
        GetTestData("block_comment");

    public static IEnumerable<Func<ParsingTestData>> EsclineTestData() => GetTestData("escline_");

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

    public static IEnumerable<Func<ParsingTestData>> EscTestData() => GetTestData("esc_");

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
        inputFiles = inputFiles.Where(x => !x.EndsWith("_fail.kdl")).ToArray();
        foreach (var inputFile in inputFiles)
        {
            var fileName = Path.GetFileName(inputFile);
            var expectedFile = Path.Combine(expectedDir, fileName);
            yield return () => new ParsingTestData(inputFile, expectedFile);
        }
    }
}

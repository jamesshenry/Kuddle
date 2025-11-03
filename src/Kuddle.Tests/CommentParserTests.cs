using System.Runtime.CompilerServices;
using Kuddle.Parser;

namespace Kuddle.Tests;

public class CommentParserTests
{
    [Test]
    public async Task CanParseSimpleSingleLineComment()
    {
        var sut = CommentParsers.MultiLineComment;

        var comment = """// I am a single line comment""";

        bool success = sut.TryParse(comment, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(comment);
    }

    [Test]
    public async Task CanParseSimpleMultiLineComment()
    {
        var sut = CommentParsers.MultiLineComment;

        var comment = """
/*
some
comments
*/
""";

        bool success = sut.TryParse(comment, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(comment);
    }

    [Test]
    public async Task CanParseNestedMultiLineComment()
    {
        var sut = CommentParsers.MultiLineComment;

        var comment = """
/*
    i am the first comment
    /*
        i am the nested comment
    */
*/
""";

        bool success = sut.TryParse(comment, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(comment);
    }
}

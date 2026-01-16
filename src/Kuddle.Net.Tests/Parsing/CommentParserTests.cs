using Kuddle.Parser;

namespace Kuddle.Tests.Parsing;

public class CommentParserTests
{
    [Test]
    public async Task CanParseSimpleSingleLineComment()
    {
        var sut = KdlGrammar.SingleLineComment;

        var comment = """// I am a single line comment""";

        bool success = sut.TryParse(comment, out var value);

        await Assert.That(success).IsTrue();
        await Assert.That(value.ToString()).IsEqualTo(comment);
    }

    [Test]
    public async Task CanParseSimpleMultiLineComment()
    {
        var sut = KdlGrammar.MultiLineComment;

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
        var sut = KdlGrammar.MultiLineComment;

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

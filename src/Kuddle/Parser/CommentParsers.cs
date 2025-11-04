using System.Text.RegularExpressions;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Kuddle.Parser;

public static class CommentParsers
{
    private static readonly Parser<string> OpenComment = Literals.Text("/*");
    private static readonly Parser<string> CloseComment = Literals.Text("*/");
    private static readonly Parser<TextSpan> CommentTerminator = Literals
        .AnyOf(CharacterSets.NewLineWhitespaceChars, minSize: 1)
        .Or(Capture(Always().Eof()));

    public static readonly Parser<TextSpan> SingleLineComment = Literals.Comments("//");
    public static readonly Parser<TextSpan> MultiLineComment = Recursive<TextSpan>(commentParser =>
    {
        var nestedComment = commentParser;

        var otherContent = AnyCharBefore(OpenComment.Or(CloseComment));
        var fullContent = ZeroOrMany(nestedComment.Or(otherContent));
        var fullCommentParser = OpenComment.And(fullContent).And(CloseComment);

        return Capture(fullCommentParser);
    });
}

public static class WhiteSpaceParsers { }

public static class NumberParsers { }

public static class StringParsers { }

public static class NodeParsers { }

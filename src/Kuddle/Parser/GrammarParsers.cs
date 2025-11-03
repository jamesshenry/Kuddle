using System.Text.RegularExpressions;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

public static class GrammarParsers
{
    private static readonly Parser<string> OpenComment = Literals.Text("/*");
    private static readonly Parser<string> CloseComment = Literals.Text("*/");

    public static readonly Parser<TextSpan> MultiLineComment = Recursive<TextSpan>(mlc =>
    {
        var commentedBlock = Recursive<TextSpan>(cb =>
            Capture(CloseComment)
                .Or(OneOf(mlc, Capture(Literals.Char('*')), Capture(Literals.Char('/')), cb))
        );

        return Capture(OpenComment.And(commentedBlock));
    });
}

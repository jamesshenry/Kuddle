using System.Text.RegularExpressions;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Kuddle.Parser;

public static class KuddleGrammar
{
    static KuddleGrammar()
    {
        Decimal = Terms.Decimal(NumberOptions.AllowLeadingSign | NumberOptions.AllowExponent);
        Sign = Literals.Text("-").Or(Literals.Text("+"));
        Hex = Capture(Sign.Optional().And(Literals.Text("0x")).And(Literals.Pattern(IsHexChar)));
        Octal = Capture(
            Sign.Optional()
                .AndSkip(Literals.Text("0o"))
                .And(
                    Literals
                        .Pattern(IsOctalChar)
                        .And(ZeroOrMany(Literals.Pattern(c => c == '_' || IsOctalChar(c))))
                )
        );
        Binary = Capture(
            Sign.Optional()
                .AndSkip(Literals.Text("0b"))
                .And(Literals.Char('0').Or(Literals.Char('1')))
                .And(ZeroOrMany(Literals.Pattern(c => c == '_' || IsBinaryChar(c))))
        );
        Boolean = Literals.Text("#true").Or(Literals.Text("#false"));
        Keyword = Boolean.Or(Literals.Text("#null"));
        KeywordNumber = OneOf(Literals.Text("#inf"), Literals.Text("#-inf"), Literals.Text("#nan"));

        Number = Capture(OneOf(Capture(KeywordNumber), Hex, Octal, Binary, Capture(Decimal)));

        var lineSpace = Deferred<TextSpan>();
        var multiLineComment = Deferred<TextSpan>();

        var openComment = Literals.Text("/*");
        var closeComment = Literals.Text("*/");

        SingleLineComment = Literals.Comments("//");
        MultiLineComment = Recursive<TextSpan>(commentParser =>
        {
            var nestedComment = commentParser;

            var otherContent = AnyCharBefore(openComment.Or(closeComment));
            var fullContent = ZeroOrMany(nestedComment.Or(otherContent));
            var fullCommentParser = openComment.And(fullContent).And(closeComment);

            return Capture(fullCommentParser);
        });

        SlashDash = Capture(Literals.Text("/-").And(lineSpace));

        Ws = Literals
            .AnyOf(CharacterSets.UnicodeSpaceChars, minSize: 1, maxSize: 1)
            .Or(MultiLineComment);
        EscLine = Capture(
            Literals
                .Text(@"\\")
                .And(ZeroOrMany(Ws))
                .And(
                    OneOf(
                        SingleLineComment,
                        Literals.AnyOf(CharacterSets.NewLineChars, maxSize: 1),
                        Capture(Always().Eof())
                    )
                )
        );
        NewLine = Literals
            .AnyOf(CharacterSets.NewLineChars, minSize: 1, maxSize: 1)
            .Or(Capture(Literals.Text("\r\n")));

        NodeSpace = Capture(Ws.ZeroOrMany().And(EscLine).And(Ws.ZeroOrMany()))
            .Or(Capture(Ws.OneOrMany()));

        lineSpace.Parser = NodeSpace.Or(NewLine).Or(SingleLineComment);
        LineSpace = lineSpace;
    }

    #region Numbers
    public static readonly Parser<decimal> Decimal;
    public static readonly Parser<string> Sign;
    public static readonly Parser<TextSpan> Hex;
    public static readonly Parser<TextSpan> Octal;
    public static readonly Parser<TextSpan> Binary;
    public static readonly Parser<TextSpan> Number;
    #endregion

    #region Keywords and booleans
    public static readonly Parser<string> Boolean;
    public static readonly Parser<string> KeywordNumber;
    public static readonly Parser<string> Keyword;
    #endregion

    #region Specific code points
    public static readonly Parser<char> Bom = Literals.Char('\uFEFF');
    #endregion

    #region Comments
    public static readonly Parser<TextSpan> SingleLineComment;
    public static readonly Parser<TextSpan> MultiLineComment;
    public static readonly Parser<TextSpan> SlashDash;
    #endregion

    #region WhiteSpace
    public static readonly Parser<TextSpan> Ws;
    public static readonly Parser<TextSpan> EscLine;
    public static readonly Parser<TextSpan> NewLine;
    public static readonly Parser<TextSpan> NodeSpace;
    public static readonly Parser<TextSpan> LineSpace = Deferred<TextSpan>();
    #endregion

    private static bool IsBinaryChar(char c) => c == '0' || c == '1';

    private static bool IsOctalChar(char c) => c >= '0' && c <= '7';

    private static bool IsHexChar(char c) =>
        (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == '_';
}

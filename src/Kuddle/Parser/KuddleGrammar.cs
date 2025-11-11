using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Kuddle.Extensions;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Kuddle.Parser;

public static class KuddleGrammar
{
    static KuddleGrammar()
    {
        var nodeSpace = Deferred<TextSpan>();
        var @string = Deferred<TextSpan>();

        SingleNewLine = Literals
            .AnyOf(CharacterSets.NewLineChars, minSize: 1, maxSize: 1)
            .Or(Capture(Literals.Text("\r\n")));

        Type = Between(
            Literals.Char('('),
            ZeroOrMany(nodeSpace).SkipAnd(@string).AndSkip(ZeroOrMany(nodeSpace)),
            Literals.Char(')')
        );
        Sign = Literals.AnyOf(['+', '-'], 1, 1);

        //Strings

        var singleQuote = Literals.Char('"');
        var tripleQuote = Literals.Text("\"\"\"");
        var hash = Literals.Char('#');

        var openingHashes = Capture(OneOrMany(hash));

        RawString = openingHashes.Switch(
            (context, ts) =>
            {
                string hashString = ts.ToString();

                static Parser<TextSpan> CreateBodyParser(Parser<TextSpan> closingDelimiter) =>
                    AnyCharBefore(closingDelimiter)
                        .When(
                            (context, textspan) => textspan.Span.Any(IsDisallowedLiteralCodePoint)
                        );

                var closeSingleQuoteRaw = Capture(singleQuote.And(Literals.Text(hashString)));
                var singleQuoteRaw = Between(
                    singleQuote,
                    CreateBodyParser(closeSingleQuoteRaw),
                    closeSingleQuoteRaw
                );

                var closeTripleQuoteRaw = Capture(tripleQuote.And(Literals.Text(hashString)));
                var tripleQuoteRaw = Between(
                    tripleQuote,
                    CreateBodyParser(closeTripleQuoteRaw),
                    closeTripleQuoteRaw
                );

                return singleQuoteRaw.Or(tripleQuoteRaw);
            }
        );
        var identifierChar = Literals.Pattern(c => char.IsLetterOrDigit(c) || c == '_' || c == '.');
        var unambiguousStartChar = identifierChar.When(
            (_, c) => c.Span[^1] >= 'a' && c.Span[^1] <= 'z'
        );
        var literalCodePoint = Literals
            .NoneOf("", minSize: 1, maxSize: 1)
            .When((context, c) => !IsDisallowedLiteralCodePoint(c.Span[0]));

        var hexSequence = Literals.Pattern(IsHexChar, 1, 6);
        HexUnicode = hexSequence.When((a, b) => !IsLoneSurrogate(b.Span[0]));

        WsEscape = Literals
            .Char('\\')
            .And(
                Literals.Pattern(
                    c =>
                        CharacterSets.WhiteSpaceAndNewLineChars.Contains(c) || char.IsWhiteSpace(c),
                    minSize: 1,
                    maxSize: 0
                )
            )
            .Then(x => new TextSpan());
        var stringEscapeChars = Literals
            .AnyOf(
                """
nrt"\bfs
"""
            )
            .Then(ts =>
                ts.Span[0] switch
                {
                    'n' => new TextSpan(Environment.NewLine),
                    'r' => new TextSpan("\r"),
                    't' => new TextSpan("\t"),
                    '"' => new TextSpan("\""),
                    '\\' => new TextSpan(@"\"),
                    'b' => new TextSpan("\b"),
                    'f' => new TextSpan("\f"),
                    's' => new TextSpan(" "),
                    _ => throw new Exception(),
                }
            );
        var unicodeEscape = Between(Literals.Text("u{"), HexUnicode, Literals.Char('}'))
            .Then(hexSpan =>
            {
                int codePoint = int.Parse(
                    hexSpan.Span,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture
                );

                string unicodeChar = char.ConvertFromUtf32(codePoint);

                return new TextSpan(unicodeChar);
            });
        ;
        var escapeSequence = Literals.Char('\\').SkipAnd(stringEscapeChars.Or(unicodeEscape));
        var plainCharacter = Literals.Pattern(c =>
            c != '\\' && c != '"' && !IsDisallowedLiteralCodePoint(c)
        );
        StringCharacter = OneOf(WsEscape, escapeSequence, plainCharacter);

        SingleLineStringBody = ZeroOrMany(
                StringCharacter
            // SkipWhiteSpace(
            //         StringCharacter.When(
            //             (context, sc) => !sc.Span.ContainsAny(CharacterSets.NewLineChars)
            //         )
            //     )
            //     .WithWhiteSpaceParser(WsEscape)
            // OneOf(plainCharacter, WsEscape.Then(ts => new TextSpan()))
            )
            .Then(x =>
            {
                var sb = new StringBuilder();
                foreach (var item in x)
                {
                    sb.Append(item.Span);
                }
                return new TextSpan(sb.ToString());
            });

        MultiLineStringBody = Capture(
            ZeroOrOne(Literals.Text("\"").Or(Literals.Text("\"\"")))
                .And(ZeroOrMany(StringCharacter))
        );
        QuotedString = Between(Literals.Char('"'), SingleLineStringBody, Literals.Char('"'))
            .Or(
                Between(
                    Literals.Text("\"\"\""),
                    Capture(
                        SingleNewLine
                            .And(ZeroOrOne(MultiLineStringBody.And(SingleNewLine)))
                            .And(Literals.AnyOf(CharacterSets.WhitespaceChars, 1).Or(WsEscape))
                    ),
                    Literals.Text("\"\"\"")
                )
            );
        IdentifierChar = Literals.NoneOf(CharacterSets.IdentifierExcludedChars, 1, 1);

        DottedIdent = Capture(
            Sign.Optional()
                .And(Literals.Char('.'))
                .And(
                    ZeroOrOne(
                        IdentifierChar
                            .When(
                                (a, b) =>
                                {
                                    var c = !IsDigitChar(b.Span[0]);
                                    return c;
                                }
                            )
                            .ElseError("No numbers allowed")
                            .And(ZeroOrMany(IdentifierChar))
                    )
                )
        );

        SignedIdent = Capture(
            Sign.And(
                ZeroOrOne(
                    IdentifierChar
                        .When((a, b) => !IsDigitChar(b.Span[0]) && b.Span[0] != '.')
                        .And(ZeroOrMany(IdentifierChar))
                )
            )
        );

        UnambiguousIdent = Capture(
                IdentifierChar
                    .When(
                        (a, b) =>
                            !IsDigitChar(b.Span[0]) && !IsSigned(b.Span[0]) && b.Span[0] != '.'
                    )
                    .And(ZeroOrMany(IdentifierChar))
            )
            .When((context, span) => !ReservedKeywords.Contains(span.Buffer!))
            .ElseError("");

        IdentifierString = OneOf(DottedIdent, SignedIdent, UnambiguousIdent);

        String = OneOf(IdentifierString, QuotedString, RawString);
        // Numbers
        Integer = Literals
            .AnyOf(CharacterSets.DigitsAndUnderscore)
            .When((a, b) => b.Span[0] != '_');
        Exponent = Literals.Char('e').Or(Literals.Char('E')).And(Sign.Optional()).And(Integer);
        Decimal = Capture(
            Sign.Optional()
                .And(Integer)
                .And(ZeroOrOne(Literals.Char('.').And(Integer)))
                .And(Exponent.Optional())
        );
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

        Number = OneOf(Capture(KeywordNumber), Hex, Octal, Binary, Decimal);

        var lineSpace = Deferred<TextSpan>();
        var multiLineComment = Deferred<TextSpan>();

        // Comments
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
            .AnyOf(CharacterSets.WhitespaceChars, minSize: 1, maxSize: 1)
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

        nodeSpace.Parser = Capture(Ws.ZeroOrMany().And(EscLine).And(Ws.ZeroOrMany()))
            .Or(Capture(Ws.OneOrMany()));
        NodeSpace = nodeSpace;
        lineSpace.Parser = NodeSpace.Or(SingleNewLine).Or(SingleLineComment);
        LineSpace = lineSpace;
    }

    private static bool IsDisallowedKeywordString(ReadOnlySpan<char> value)
    {
        return value != "true" && value != "false";
    }

    private static bool IsLoneSurrogate(char codePoint) =>
        codePoint >= 0xD800 && codePoint <= 0xDFFF;

    #region Numbers
    public static readonly Parser<TextSpan> Decimal;
    private static readonly Parser<TextSpan> Integer;
    public static readonly Parser<TextSpan> Sign;
    private static readonly Sequence<char, Option<TextSpan>, TextSpan> Exponent;
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
    public static readonly Parser<TextSpan> SingleNewLine;
    public static readonly Parser<TextSpan> NodeSpace;
    public static readonly Parser<TextSpan> LineSpace = Deferred<TextSpan>();
    internal static readonly Parser<TextSpan> Type;
    internal static readonly Parser<TextSpan> RawString;
    internal static readonly Parser<TextSpan> IdentifierChar;
    internal static readonly Parser<TextSpan> UnambiguousIdent;
    internal static readonly Parser<TextSpan> SignedIdent;
    internal static readonly Parser<TextSpan> DottedIdent;
    internal static readonly Parser<TextSpan> HexUnicode;
    internal static readonly Parser<TextSpan> WsEscape;
    internal static readonly Parser<TextSpan> StringCharacter;
    internal static readonly Parser<TextSpan> SingleLineStringBody;
    internal static readonly Parser<TextSpan> MultiLineStringBody;
    internal static readonly Parser<TextSpan> IdentifierString;
    internal static readonly Parser<TextSpan> QuotedString;
    internal static readonly Parser<TextSpan> String;
    #endregion

    private static bool IsBinaryChar(char c) => c == '0' || c == '1';

    private static bool IsOctalChar(char c) => c >= '0' && c <= '7';

    private static bool IsHexChar(char c) =>
        (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == '_';

    private static bool IsDigitChar(char c) => c >= '0' && c <= '9';

    private static bool IsSigned(char c) => c == '+' || c == '-';

    private static bool IsDisallowedLiteralCodePoint(char c)
    {
        return (c >= '\u0000' && c <= '\u0008')
            || IsLoneSurrogate(c)
            || (c >= '\u200E' && c <= '\u200F')
            || (c >= '\u202A' && c <= '\u202E')
            || (c >= '\u2066' && c <= '\u2069')
            || c == '\uFEFF';
    }

    private static readonly HashSet<string> ReservedKeywords = new()
    {
        "inf",
        "-inf",
        "nan",
        "true",
        "false",
        "null",
    };
}

public abstract record KdlValue(string? TypeAnnotation);

public sealed record KdlString(string Value, string? TypeAnnotation = null)
    : KdlValue(TypeAnnotation);

public sealed record KdlBoolean(bool Value) : KdlValue(TypeAnnotation: null);

public sealed record KdlNull() : KdlValue(TypeAnnotation: null);

public sealed record KdlNumber(string RawValue, BaseNumber Base, string? TypeAnnotation)
    : KdlValue(TypeAnnotation);

public enum BaseNumber
{
    Decimal,
    Hex,
    Octal,
    Binary,
}

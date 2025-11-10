using System;
using System.Collections.Generic;
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
                            (context, textspan) =>
                            {
                                foreach (var c in textspan.Span)
                                {
                                    if (IsDisallowedLiteralCodePoint(c))
                                        return false;
                                }
                                return true;
                            }
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

        IdentifierChar = Literals.NoneOf(CharacterSets.IdentifierExcludedChars, 1, 1);
        UnambiguousIdent = Capture(
            IdentifierChar
                .When((a, b) => !IsDigitChar(b.Span[0]) && !IsSigned(b.Span[0]) && b.Span[0] != '.')
                .And(ZeroOrMany(IdentifierChar))
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
        DottedIdent = Capture(
            Sign.Optional()
                .And(Literals.Char('.'))
                .And(
                    ZeroOrOne(
                        IdentifierChar
                            .When((a, b) => !IsDigitChar(b.Span[0]))
                            .And(ZeroOrMany(IdentifierChar))
                    )
                )
        );
        HexSequence = Literals.Pattern(IsHexChar, 1, 1);
        HexUnicode = HexSequence.When((a, b) => !IsLoneSurrogate(b.Span[0]));
        WsEscape = Capture(
            Literals
                .Char('\\')
                .And(Literals.AnyOf(CharacterSets.WhiteSpaceAndNewLineChars, minSize: 1))
        );
        var stringEscapeChars = Literals.AnyOf("\"\\bfnrts");
        var unicodeEscape = Between(Literals.Text("u{"), HexUnicode, Literals.Char('}'));
        var escapeSequence = Literals.Char('\\').SkipAnd(stringEscapeChars.Or(unicodeEscape));
        var plainCharacter = Literals.Pattern(c =>
            c != '\\' && c != '"' && !IsDisallowedLiteralCodePoint(c)
        );

        StringCharacter = OneOf(escapeSequence, WsEscape, plainCharacter);

        SingleLineStringBody = Capture(
            ZeroOrMany(
                StringCharacter.When(
                    (context, sc) => !sc.Span.ContainsAny(CharacterSets.NewLineChars)
                )
            )
        );
        MultiLineStringBody = Capture(
            ZeroOrOne(Literals.Text("\"").Or(Literals.Text("\"\"")))
                .And(ZeroOrMany(StringCharacter))
        );
        IdentifierString = UnambiguousIdent.Or(SignedIdent).Or(DottedIdent);
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
        String = IdentifierString.Or(QuotedString).Or(RawString);
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
    private static readonly Parser<TextSpan> Type;
    private static readonly Parser<TextSpan> RawString;
    private static readonly Parser<TextSpan> IdentifierChar;
    private static readonly Parser<TextSpan> UnambiguousIdent;
    private static readonly Parser<TextSpan> SignedIdent;
    private static readonly Parser<TextSpan> DottedIdent;
    private static readonly Parser<TextSpan> HexSequence;
    private static readonly Parser<TextSpan> HexUnicode;
    private static readonly Parser<TextSpan> WsEscape;
    private static readonly Parser<TextSpan> StringCharacter;
    private static readonly Parser<TextSpan> SingleLineStringBody;
    private static readonly Parser<TextSpan> MultiLineStringBody;
    private static readonly Parser<TextSpan> IdentifierString;
    private static readonly Parser<TextSpan> QuotedString;
    private static readonly Parser<TextSpan> String;
    #endregion

    private static bool IsBinaryChar(char c) => c == '0' || c == '1';

    private static bool IsOctalChar(char c) => c >= '0' && c <= '7';

    private static bool IsHexChar(char c) =>
        (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F') || c == '_';

    private static bool IsDigitChar(char c) => c >= '0' && c <= '9';

    private static bool IsSigned(char c) => c != '+' && c != '-';

    private static bool IsDisallowedLiteralCodePoint(char c)
    {
        return (c >= '\u0000' && c <= '\u0008')
            || IsLoneSurrogate(c)
            || (c >= '\u200E' && c <= '\u200F')
            || (c >= '\u202A' && c <= '\u202E')
            || (c >= '\u2066' && c <= '\u2069')
            || c == '\uFEFF';
    }
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

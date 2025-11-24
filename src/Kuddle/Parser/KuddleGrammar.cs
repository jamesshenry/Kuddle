using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using Kuddle.AST;
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

        var singleNewLine = Capture(Literals.Text("\r\n").Or(Literals.Text("\n")));

        Sign = Literals.AnyOf(['+', '-'], 1, 1);

        //Strings

        var singleQuote = Literals.Char('"');
        var tripleQuote = Literals.Text("\"\"\"");
        var hash = Literals.Char('#');

        var openingHashes = Capture(OneOrMany(hash));

        RawString = openingHashes
            .Switch(
                (context, hashes) =>
                {
                    string delimiter = hashes.ToString();

                    static Parser<TextSpan> BodyUntil(Parser<TextSpan> closer) =>
                        AnyCharBefore(closer)
                            .When((_, span) => !span.Span.Any(IsDisallowedLiteralCodePoint));

                    var closeSingle = Capture(singleQuote.And(Literals.Text(delimiter)));
                    var singleRaw = Between(singleQuote, BodyUntil(closeSingle), closeSingle);

                    var closeTriple = Capture(tripleQuote.And(Literals.Text(delimiter)));
                    var tripleRaw = Between(
                            tripleQuote.And(singleNewLine),
                            BodyUntil(closeTriple),
                            closeTriple
                        )
                        .Then(ts => new TextSpan(Dedent(ts.ToString())));

                    return tripleRaw.Or(singleRaw);
                }
            )
            .Then(ts => new KdlString(ts.Span.ToString(), StringKind.Raw));

        var identifierChar = Literals.Pattern(
            c =>
                !CharacterSets.IsNewline(c)
                && !CharacterSets.IsWhiteSpace(c)
                && !"\\/(){};[]\"#=".Contains(c),
            1,
            1
        );
        var unambiguousStartChar = identifierChar.When(
            (_, c) => c.Span[^1] >= 'a' && c.Span[^1] <= 'z'
        );
        var literalCodePoint = Literals
            .NoneOf("", minSize: 1, maxSize: 1)
            .When((context, c) => !IsDisallowedLiteralCodePoint(c.Span[0]));

        var hexSequence = Literals.Pattern(IsHexChar, 1, 6);
        HexUnicode = hexSequence
            .When((a, b) => !IsLoneSurrogate(b.Span[0]))
            .Then(ts => new TextSpan(Regex.Unescape(ts.Span.ToString())));

        WsEscape = Literals
            .Char('\\')
            .And(
                Literals.Pattern(
                    c => CharacterSets.IsNewline(c) || char.IsWhiteSpace(c),
                    minSize: 1,
                    maxSize: 0
                )
            )
            .Then(x => new TextSpan());
        var stringEscapeChars = Literals
            .AnyOf(
                """
nrt"\bfs
""",
                minSize: 1,
                maxSize: 1
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

        var escapeSequence = OneOf(
            Literals.Text(@"\").SkipAnd(stringEscapeChars),
            Literals
                .Text("\\u{")
                .SkipAnd(HexUnicode)
                .AndSkip(Literals.Char('}'))
                .Then(ts => new TextSpan(char.ConvertFromUtf32(Convert.ToInt32(ts.Buffer, 16))))
        );
        var plainCharacter = Literals
            .Pattern(c => c != '\\' && c != '"' && !IsDisallowedLiteralCodePoint(c), 1, 1)
            .Then(
                (_, x) =>
                {
                    if (x.Span[0] == '\r')
                    {
                        return new TextSpan();
                    }
                    return x;
                }
            );
        StringCharacter = OneOf(escapeSequence, WsEscape, plainCharacter);

        var singleLineStringBody = ZeroOrMany(StringCharacter)
            .Then(x =>
            {
                var sb = new StringBuilder();
                foreach (var item in x)
                {
                    sb.Append(item.Span);
                }
                return new TextSpan(sb.ToString());
            });

        MultiLineQuoted = tripleQuote
            .SkipAnd(singleNewLine)
            .SkipAnd(AnyCharBefore(tripleQuote))
            .When(
                (_, ts) =>
                {
                    var trimmed = ts.Span.TrimEnd(CharacterSets.WhiteSpaceChars);

                    if (trimmed.IsEmpty)
                        return true;
                    char lastChar = trimmed[^1];
                    return lastChar == '\n' || lastChar == '\r';
                }
            )
            .Then(ts =>
            {
                var dedented = Dedent(ts.ToString());

                return new KdlString(UnescapeKdl(dedented), StringKind.MultiLine);
            })
            .AndSkip(tripleQuote);
        SingleLineQuoted = Between(Literals.Char('"'), singleLineStringBody, Literals.Char('"'))
            .Then(ts => new KdlString(ts.Span.ToString(), StringKind.Quoted));
        QuotedString = OneOf(MultiLineQuoted, SingleLineQuoted);

        DottedIdent = Capture(
            Sign.Optional()
                .And(Literals.Char('.'))
                .And(
                    ZeroOrOne(
                        identifierChar
                            .When((a, b) => !IsDigitChar(b.Span[0]))
                            .ElseError("No numbers allowed")
                            .And(ZeroOrMany(identifierChar))
                    )
                )
        );

        SignedIdent = Capture(
            Sign.And(
                ZeroOrOne(
                    identifierChar
                        .When((a, b) => !IsDigitChar(b.Span[0]) && b.Span[0] != '.')
                        .And(ZeroOrMany(identifierChar))
                )
            )
        );

        UnambiguousIdent = Capture(
                identifierChar
                    .When(
                        (a, b) =>
                            !IsDigitChar(b.Span[0]) && !IsSigned(b.Span[0]) && b.Span[0] != '.'
                    )
                    .And(ZeroOrMany(identifierChar))
            )
            .When((context, span) => !ReservedKeywords.Contains(span.Buffer!))
            .ElseError("");

        IdentifierString = OneOf(DottedIdent, SignedIdent, UnambiguousIdent)
            .Then(ts => new KdlString(ts.Span.ToString(), StringKind.Identifier))
            .ElseError("Failed to parse identifier string");
        String = OneOf(IdentifierString, RawString, QuotedString)
            .ElseError("Failed to parse string")
            .Then((context, ks) => ks);

        Integer = Literals
            .Pattern(c => char.IsDigit(c) || c == '_')
            .When((a, b) => b.Span[0] != '_');
        var exponent = Literals.Char('e').Or(Literals.Char('E')).And(Sign.Optional()).And(Integer);
        Decimal = Capture(
            Sign.Optional()
                .And(Integer)
                .And(ZeroOrOne(Literals.Char('.').And(Integer)))
                .And(exponent.Optional())
        );
        Hex = Capture(
                Sign.Optional()
                    .And(Literals.Text("0x"))
                    .And(Literals.Pattern(IsHexChar, 1, 1))
                    .And(ZeroOrMany(Literals.Pattern(c => c == '_' || IsHexChar(c))))
            )
            .When((context, x) => x.Span[^1] != '_');
        Octal = Capture(
                Sign.Optional()
                    .AndSkip(Literals.Text("0o"))
                    .And(
                        Literals
                            .Pattern(IsOctalChar)
                            .And(ZeroOrMany(Literals.Pattern(c => c == '_' || IsOctalChar(c))))
                    )
            )
            .When((context, x) => x.Span[^1] != '_');

        Binary = Capture(
                Sign.Optional()
                    .AndSkip(Literals.Text("0b"))
                    .And(Literals.Char('0').Or(Literals.Char('1')))
                    .And(ZeroOrMany(Literals.Pattern(c => c == '_' || IsBinaryChar(c))))
            )
            .When((context, x) => x.Span[^1] != '_');
        Boolean = Literals
            .Text("#true")
            .Or(Literals.Text("#false"))
            .Then(value =>
                value switch
                {
                    "#true'" => new KdlBool(true),
                    "#false'" => new KdlBool(false),
                    _ => throw new NotSupportedException(),
                }
            );
        Keyword = Boolean.Or<KdlBool, KdlNull, KdlValue>(
            Literals.Text("#null").Then(_ => new KdlNull())
        );
        KeywordNumber = Capture(
            OneOf(Literals.Text("#inf"), Literals.Text("#-inf"), Literals.Text("#nan"))
        );

        Number = OneOf(KeywordNumber, Hex, Octal, Binary, Decimal)
            .Then((context, value) => new KdlNumber(value.Span.ToString()));

        // Comments
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

        var lineSpace = Deferred<TextSpan>();
        SlashDash = Capture(Literals.Text("/-").And(lineSpace));

        // Whitespace
        Ws = Literals
            .Pattern(c => CharacterSets.IsWhiteSpace(c), minSize: 1, maxSize: 1)
            .Or(MultiLineComment);
        EscLine = Capture(
            Literals
                .Text(@"\\")
                .And(ZeroOrMany(Ws))
                .And(
                    OneOf(
                        SingleLineComment,
                        Literals.Pattern(CharacterSets.IsNewline, maxSize: 1),
                        Capture(Always().Eof())
                    )
                )
        );

        nodeSpace.Parser = Capture(Ws.ZeroOrMany().And(EscLine).And(Ws.ZeroOrMany()))
            .Or(Capture(Ws.OneOrMany()));
        NodeSpace = nodeSpace;
        lineSpace.Parser = NodeSpace.Or(singleNewLine).Or(SingleLineComment);
        LineSpace = lineSpace;

        // Entries

        Type = Between(
            Literals.Char('('),
            ZeroOrMany(NodeSpace).SkipAnd(String).AndSkip(ZeroOrMany(NodeSpace)),
            Literals.Char(')')
        );

        Value = Type.Optional()
            .AndSkip(ZeroOrMany(NodeSpace))
            .And(String.Or<KdlString, KdlNumber, KdlValue>(Number).Or(Keyword))
            .Then(x =>
            {
                if (x.Item1.HasValue)
                {
                    return x.Item2 with { TypeAnnotation = x.Item1.Value.Value };
                }
                return x.Item2;
            });

        var Property = String
            .AndSkip(ZeroOrMany(NodeSpace))
            .AndSkip(Literals.Char('='))
            .AndSkip(ZeroOrMany(NodeSpace))
            .And(Value);
    }

    #region Numbers
    public static readonly Parser<TextSpan> Decimal;
    internal static readonly Parser<TextSpan> Integer;
    public static readonly Parser<TextSpan> Sign;
    public static readonly Parser<TextSpan> Hex;
    public static readonly Parser<TextSpan> Octal;
    public static readonly Parser<TextSpan> Binary;
    public static readonly Parser<KdlNumber> Number;
    #endregion

    #region Keywords and booleans
    public static readonly Parser<KdlBool> Boolean;
    public static readonly Parser<TextSpan> KeywordNumber;
    public static readonly Parser<KdlValue> Keyword;
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
    internal static readonly Parser<TextSpan> Ws;
    internal static readonly Parser<TextSpan> EscLine;
    internal static readonly Parser<TextSpan> NodeSpace;
    internal static readonly Parser<TextSpan> LineSpace = Deferred<TextSpan>();
    internal static readonly Parser<KdlString> Type;
    private static readonly Parser<KdlValue> Value;

    // internal static readonly Parser<TextSpan> IdentifierChar;
    internal static readonly Parser<TextSpan> UnambiguousIdent;
    internal static readonly Parser<TextSpan> SignedIdent;
    internal static readonly Parser<TextSpan> DottedIdent;
    internal static readonly Parser<TextSpan> HexUnicode;
    internal static readonly Parser<TextSpan> WsEscape;
    internal static readonly Parser<TextSpan> StringCharacter;
    internal static readonly Parser<KdlString> MultiLineQuoted;
    internal static readonly Parser<KdlString> SingleLineQuoted;
    internal static readonly Parser<KdlString> RawString;
    internal static readonly Parser<KdlString> IdentifierString;
    internal static readonly Parser<KdlString> QuotedString;
    internal static readonly Parser<KdlString> String;
    #endregion

    private static bool IsBinaryChar(char c) => c == '0' || c == '1';

    private static bool IsOctalChar(char c) => c >= '0' && c <= '7';

    private static bool IsLoneSurrogate(char codePoint) =>
        codePoint >= 0xD800 && codePoint <= 0xDFFF;

    private static bool IsHexChar(char c) =>
        (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

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

    private static readonly HashSet<string> ReservedKeywords =
    [
        "inf",
        "-inf",
        "nan",
        "true",
        "false",
        "null",
    ];

    static string UnescapeKdl(string input)
    {
        var parser = ZeroOrMany(StringCharacter);
        if (parser.TryParse(input, out var result))
        {
            var sb = new StringBuilder();
            foreach (var part in result!)
                sb.Append(part.Span);
            return sb.ToString();
        }
        return input;
    }

    static string Dedent(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return "";

        string text = raw.Replace("\r\n", "\n").Replace("\r", "\n");

        int lastNewLineIndex = text.LastIndexOf('\n');

        if (lastNewLineIndex == -1)
            return "";

        string indentation = text.Substring(lastNewLineIndex + 1);

        var lines = text.Split('\n');
        var sb = new StringBuilder();

        for (int i = 0; i < lines.Length - 1; i++)
        {
            var line = lines[i];

            if (line.StartsWith(indentation))
            {
                sb.Append(line.AsSpan(indentation.Length));
            }
            else if (string.IsNullOrWhiteSpace(line)) { }
            else
            {
                sb.Append(line);
            }

            if (i < lines.Length - 2)
            {
                sb.Append('\n');
            }
        }

        return sb.ToString();
    }
}

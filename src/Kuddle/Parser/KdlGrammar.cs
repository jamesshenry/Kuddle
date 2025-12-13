using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kuddle.AST;
using Kuddle.Extensions;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Kuddle.Parser;

public static class KdlGrammar
{
    internal static readonly Parser<KdlDocument> Document;

    #region Numbers
    internal static readonly Parser<TextSpan> Decimal;
    internal static readonly Parser<TextSpan> Integer;
    internal static readonly Parser<TextSpan> Sign;
    internal static readonly Parser<TextSpan> Hex;
    internal static readonly Parser<TextSpan> Octal;
    internal static readonly Parser<TextSpan> Binary;
    internal static readonly Parser<KdlNumber> Number;
    #endregion

    #region Keywords and booleans
    internal static readonly Parser<KdlBool> Boolean;
    internal static readonly Parser<TextSpan> KeywordNumber;
    internal static readonly Parser<KdlValue> Keyword;
    #endregion

    #region Specific code points
    internal static readonly Parser<char> Bom = Literals.Char('\uFEFF');
    #endregion

    #region Comments
    internal static readonly Parser<TextSpan> SingleLineComment;
    internal static readonly Parser<TextSpan> MultiLineComment;
    internal static readonly Parser<TextSpan> SlashDash;
    #endregion

    #region WhiteSpace
    internal static readonly Parser<TextSpan> Ws;
    internal static readonly Parser<TextSpan> EscLine;
    internal static readonly Parser<TextSpan> NodeSpace;
    internal static readonly Parser<TextSpan> LineSpace = Deferred<TextSpan>();
    internal static readonly Parser<KdlString> Type;
    private static readonly Parser<KdlValue> Value;
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

    internal static readonly Parser<KdlNode> FinalNode = Deferred<KdlNode>();
    internal static readonly Parser<KdlNode?> Node;
    internal static readonly Parser<IReadOnlyList<KdlNode>> Nodes = Deferred<
        IReadOnlyList<KdlNode>
    >();

    static KdlGrammar()
    {
        var nodeSpace = Deferred<TextSpan>();

        var singleNewLine = Capture(Literals.Text("\r\n").Or(Literals.Text("\n")))
            .Debug("SingleNewLine");
        var eof = Capture(Always().Eof());
        Sign = Literals.AnyOf(['+', '-'], 1, 1);

        //Strings

        var singleQuote = Literals.Char('"');
        var tripleQuote = Literals.Text("\"\"\"").Debug("tripleQuote");
        var hash = Literals.Char('#');

        var openingHashes = Capture(OneOrMany(hash));

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
            .Then(x => new TextSpan())
            .Debug("WsEscape");

        var escapeSequence = Literals
            .Char('\\')
            .SkipAnd(
                OneOf(
                        Literals.Char('n').Then(_ => "\n"),
                        Literals.Char('r').Then(_ => "\r"),
                        Literals.Char('t').Then(_ => "\t"),
                        Literals.Char('\\').Then(_ => "\\"),
                        Literals.Char('"').Then(_ => "\""),
                        Literals.Char('b').Then(_ => "\b"),
                        Literals.Char('f').Then(_ => "\f"),
                        Literals.Char('s').Then(_ => " "),
                        Literals
                            .Text("u{")
                            .SkipAnd(HexUnicode)
                            .AndSkip(Literals.Char('}'))
                            .Then(ts => char.ConvertFromUtf32(Convert.ToInt32(ts.Buffer, 16)))
                    )
                    .Then(s => new TextSpan(s))
            );

        var plainCharacter = Literals
            .Pattern(c => c != '\\' && c != '"' && !IsDisallowedLiteralCodePoint(c), 1, 1)
            .Then((_, x) => x.Span[0] == '\r' ? new TextSpan() : x);
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
        MultiLineQuoted = new MultiLineStringParser().Debug("MultiLineQuoted");
        SingleLineQuoted = Between(
                Literals.Char('"'),
                singleLineStringBody.Debug("singleLineStringBody"),
                Literals.Char('"').ElseError("Expected a closing quote on string")
            )
            .Debug("SingleLineQuoted")
            .Then(ts => new KdlString(ts.Span.ToString(), StringKind.Quoted));
        QuotedString = OneOf(MultiLineQuoted, SingleLineQuoted);

        DottedIdent = Capture(
                Sign.Optional()
                    .And(
                        Literals
                            .Char('.')
                            .Debug("First identifierChar")
                            .And(
                                Literals
                                    .Pattern(
                                        c =>
                                            !CharacterSets.IsNewline(c)
                                            && !CharacterSets.IsWhiteSpace(c)
                                            && !"\\/(){};[]\"#=".Contains(c),
                                        0
                                    )
                                    .Optional()
                                    .Debug("Remaining identifierChars")
                            )
                    )
            )
            .When(
                (ctx, b) =>
                {
                    var span = b.Span;
                    if (span.IsEmpty)
                        return false;

                    char c0 = span[0];
                    bool isSign = c0 == '+' || c0 == '-';

                    if (isSign && span.Length == 1)
                        return true;

                    ReadOnlySpan<char> slice = isSign ? span[1..] : span;

                    if (char.IsDigit(slice[0]))
                        return false;

                    if (slice[0] == '.' && slice.Length > 1 && char.IsDigit(slice[1]))
                        return false;

                    return true;
                }
            )
            .Debug("DottedIdent");

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
            .Then(
                (context, span) =>
                {
                    return ReservedKeywords.Contains(span.ToString())
                        ? throw new ParseException(
                            $"The keyword '{span}' cannot be used as an unquoted identifier. Wrap it in quotes: \"{span}\".",
                            context.Scanner.Cursor.Position
                        )
                        : span;
                }
            );

        IdentifierString = OneOf(DottedIdent, SignedIdent, UnambiguousIdent)
            .Then(ts => new KdlString(ts.Span.ToString(), StringKind.Bare));
        RawString = new RawStringParser();
        String = OneOf(IdentifierString, RawString, QuotedString).Then((context, ks) => ks);

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
        // .When((context, x) => x.Span[^1] != '_')
        ;
        Boolean = Literals
            .Text("#true")
            .Or(Literals.Text("#false"))
            .Then(value =>
                value switch
                {
                    "#true" => new KdlBool(true),
                    "#false" => new KdlBool(false),
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

        SingleLineComment = Literals.Comments("//").Debug("SingleLineComment");
        MultiLineComment = Recursive<TextSpan>(commentParser =>
        {
            var nestedComment = commentParser;

            var otherContent = AnyCharBefore(openComment.Or(closeComment));
            var fullContent = ZeroOrMany(nestedComment.Or(otherContent));
            var fullCommentParser = openComment.And(fullContent).And(closeComment);

            return Capture(fullCommentParser);
        });

        var lineSpace = Deferred<TextSpan>();
        SlashDash = Capture(Literals.Text("/-").And(ZeroOrMany(lineSpace))).Debug("SlashDash");

        // Whitespace
        Ws = Literals
            .Pattern(c => CharacterSets.IsWhiteSpace(c), minSize: 1, maxSize: 1)
            .Or(MultiLineComment)
            .Debug("Ws");
        EscLine = Capture(
            Literals
                .Text(@"\")
                .And(ZeroOrMany(Ws))
                .And(
                    OneOf(SingleLineComment.AndSkip(OneOf(singleNewLine, eof)), singleNewLine, eof)
                )
                .Debug("EscLine")
        );

        nodeSpace.Parser = Capture(Ws.ZeroOrMany().And(EscLine).And(Ws.ZeroOrMany()))
            .Or(Capture(Ws.OneOrMany()));
        NodeSpace = nodeSpace.Debug("NodeSpace");
        lineSpace.Parser = NodeSpace.Or(singleNewLine).Or(SingleLineComment);
        LineSpace = lineSpace;

        // Entries

        Type = Between(
            Literals.Char('('),
            ZeroOrMany(NodeSpace).SkipAnd(String).AndSkip(ZeroOrMany(NodeSpace)),
            Literals.Char(')').ElseError("Expected closing brace on type annotation")
        );

        Value = Type.Optional()
            .AndSkip(ZeroOrMany(NodeSpace))
            .And(OneOf(Keyword, Number, String))
            .Then(x =>
                x.Item1.HasValue ? (x.Item2 with { TypeAnnotation = x.Item1.Value.Value }) : x.Item2
            );

        var prop = String
            .AndSkip(ZeroOrMany(NodeSpace))
            .AndSkip(Literals.Char('='))
            .AndSkip(ZeroOrMany(NodeSpace))
            .And(Value.ElseError("Expected a value at end of input"))
            .Then(x => new KdlProperty(x.Item1, x.Item2) as KdlEntry);
        var arg = Value.Then(v => new KdlArgument(v) as KdlEntry);
        var nodePropOrArg = OneOf(prop, arg);

        var nodeTerminator = OneOf(
                SingleLineComment,
                singleNewLine,
                Capture(Literals.AnyOf(";")),
                eof
            )
            .Debug("NodeTerminator");

        var skippedEntry = SlashDash
            .And(Capture(nodePropOrArg))
            .Then(x => new KdlSkippedEntry(x.Item2.ToString()) as KdlEntry)
            .Debug("skippedEntry");

        var entryParser = OneOrMany(NodeSpace)
            .SkipAnd(OneOf(skippedEntry, nodePropOrArg))
            .Debug("EntryParser");

        var nodeChildren = Literals
            .Char('{')
            .SkipAnd(ZeroOrMany(LineSpace))
            .SkipAnd(Nodes.And(FinalNode.Optional()))
            .AndSkip(
                Literals.Char('}').ElseError("Expected closing brace '}' for node children block.") // <--- COMMITMENT
            )
            .Then(x =>
            {
                var block = new KdlBlock { Nodes = [.. x.Item1] };
                if (x.Item2.HasValue)
                    block.Nodes.Add(x.Item2.Value!);
                return block;
            })
            .Debug("nodeChildren");

        var childrenValueParser = OneOf(
                SlashDash.And(nodeChildren).Then(_ => (KdlBlock?)null),
                nodeChildren.Then(b => (KdlBlock?)b)
            )
            .Debug("ChildrenValueParser");

        var baseNode = SlashDash
            .Optional()
            .And(Type.Optional())
            .AndSkip(ZeroOrMany(NodeSpace))
            .And(String.Debug("NodeName"))
            .And(ZeroOrMany(entryParser))
            .And(
                ZeroOrMany(
                    OneOf(
                        OneOrMany(NodeSpace).SkipAnd(childrenValueParser),
                        OneOf(Capture(Literals.Char('{')), Capture(Literals.Text("/-")))
                            .Then<KdlBlock?>(
                                (context, _) =>
                                    throw new ParseException(
                                        "Nodes must be separated from their children block by whitespace.",
                                        context.Scanner.Cursor.Position
                                    )
                            )
                    )
                )
            )
            .AndSkip(ZeroOrMany(NodeSpace))
            .Then(result =>
            {
                if (result.Item1.HasValue)
                    return null;
                return new KdlNode(result.Item3)
                {
                    Entries = result.Item4.Where(e => e is not KdlSkippedEntry).ToList(),
                    Children = result.Item5.FirstOrDefault(b => b != null),
                    TerminatedBySemicolon = false,
                    TypeAnnotation = result.Item2.HasValue ? result.Item2.Value.ToString() : null,
                };
            })
            .Debug("BaseNode");

        Node = baseNode
            .And(nodeTerminator)
            .Then(x =>
                x.Item1 is null
                    ? null
                    : x.Item1 with
                    {
                        TerminatedBySemicolon = x.Item2.Span.Contains(';'),
                    }
            )
            .Debug("Node");

        (FinalNode as Deferred<KdlNode?>)!.Parser = baseNode.AndSkip(nodeTerminator.Optional());

        (Nodes as Deferred<IReadOnlyList<KdlNode>>)!.Parser = ZeroOrMany(
                ZeroOrMany(LineSpace).SkipAnd(Node)
            )
            .AndSkip(ZeroOrMany(LineSpace))
            .Then(list =>
                list.Where(x => x != null).Select(n => n!).ToList() as IReadOnlyList<KdlNode>
            );

        Document = Literals
            .Char('\uFEFF')
            .Optional()
            .SkipAnd(Nodes)
            .AndSkip(Always().Eof())
            .ElseError(
                "Unconsumed content at end of file. Syntax error likely occurred before this point."
            )
            .Then(nodes => new KdlDocument { Nodes = nodes.ToList() });
    }

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
}

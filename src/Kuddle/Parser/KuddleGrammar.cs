using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kuddle.AST;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Kuddle.Parser;

public static class KuddleGrammar
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

    internal static readonly Parser<KdlNode> FinalNode = Deferred<KdlNode>();
    internal static readonly Parser<KdlNode?> Node;
    internal static readonly Parser<IReadOnlyList<KdlNode>> Nodes = Deferred<
        IReadOnlyList<KdlNode>
    >();

    static KuddleGrammar()
    {
        var nodeSpace = Deferred<TextSpan>();

        var singleNewLine = Capture(Literals.Text("\r\n").Or(Literals.Text("\n")));
        var eof = Capture(Always().Eof());
        Sign = Literals.AnyOf(['+', '-'], 1, 1);

        //Strings

        var singleQuote = Literals.Char('"');
        var tripleQuote = Literals.Text("\"\"\"");
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
            .Then(x => new TextSpan());

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
            .AndSkip(tripleQuote.ElseError("Expected a closing triple quote on multiline string"));
        SingleLineQuoted = Between(
                Literals.Char('"'),
                singleLineStringBody,
                Literals.Char('"').ElseError("Expected a closing quote on string")
            )
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
            .Then(ts => new KdlString(ts.Span.ToString(), StringKind.Identifier));
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
                    .And(Literals.Pattern(IsHexChar, 1, 1).ElseError())
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
                        eof
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
        );

        var skippedEntry = SlashDash
            .And(Capture(nodePropOrArg))
            .Then(x => new KdlSkippedEntry(x.Item2.ToString()) as KdlEntry);

        var entryParser = OneOrMany(NodeSpace).SkipAnd(OneOf(skippedEntry, nodePropOrArg));

        var nodeChildren = /*  Between(
                Literals.Char('{'),
                Nodes.And(FinalNode.Optional()),
                Literals.Char('}')
            ) */
        Literals
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
            });

        var baseNode = SlashDash
            .Optional()
            .And(Type.Optional())
            .AndSkip(ZeroOrMany(NodeSpace))
            .And(String)
            .And(ZeroOrMany(entryParser))
            .And(
                ZeroOrMany(
                    OneOrMany(NodeSpace)
                        .SkipAnd(
                            OneOf(
                                SlashDash.And(nodeChildren).Then(_ => (KdlBlock?)null),
                                nodeChildren.Then(b => (KdlBlock?)b)
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
            });

        Node = baseNode
            .And(nodeTerminator)
            .Then(x =>
                x.Item1 is null
                    ? null
                    : x.Item1 with
                    {
                        TerminatedBySemicolon = x.Item2.Span.Contains(';'),
                    }
            );

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

    public static string Dedent(string raw)
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

public class RawStringParser : Parser<KdlString>
{
    public override bool Parse(ParseContext context, ref ParseResult<KdlString> result)
    {
        var cursor = context.Scanner.Cursor;
        var buffer = context.Scanner.Buffer;

        // 1. Must start with 'r' or '#'
        // Spec 2.0: just '#' or 'r#'? Spec says `raw-string := '#' ...`
        if (cursor.Current != '#')
            return false;

        // 2. Count opening hashes
        int hashCount = 0;
        while (cursor.Current == '#')
        {
            hashCount++;
            cursor.Advance();
        }

        // 3. Must be followed by quote
        if (cursor.Current != '"')
        {
            // Backtrack if not a raw string

            return false;
        }
        cursor.Advance(); // Skip open quote

        // 4. Read until quote + hashCount
        var start = cursor.Position;

        while (!cursor.Eof)
        {
            if (cursor.Current == '"')
            {
                // Potential end
                cursor.Advance();
                int closingHashes = 0;
                while (cursor.Current == '#' && closingHashes < hashCount)
                {
                    closingHashes++;
                    cursor.Advance();
                }

                if (closingHashes == hashCount)
                {
                    // Match found!
                    // Extract content (excluding the surrounding quotes/hashes)
                    var length = (cursor.Position.Offset - hashCount - 1) - start.Offset;
                    var content = buffer.Substring(start.Offset, length);
                    content = KuddleGrammar.Dedent(content);
                    result.Set(
                        start.Offset,
                        cursor.Position.Offset,
                        new KdlString(content, StringKind.Raw)
                    );
                    return true;
                }

                // If we didn't match enough hashes, we continue loop (it was part of the content)
            }
            else
            {
                cursor.Advance();
            }
        }

        return false;
    }
}

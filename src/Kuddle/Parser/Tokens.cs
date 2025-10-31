using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;
using static Kuddle.Parser.SymbolToken;

namespace Kuddle.Parser;

public static class Atoms
{
    public static readonly Parser<char> OpenParen = Literals.Char('(');
    public static readonly Parser<char> CloseParen = Terms.Char(')');
    public static readonly Parser<char> OpenBrace = Terms.Char('{');
    public static readonly Parser<char> CloseBrace = Terms.Char('}');
    public static readonly Parser<char> SingleQuote = Terms.Char('"');
}

public static class CharacterSets
{
    public static ReadOnlySpan<char> NonNewLineWhitespaceChars =>
        [
            // Character Tabulation
            '\u0009',
            // Space
            '\u0020',
            // No-Break Space
            '\u00A0',
            // Ogham Space Mark
            '\u1680',
            // En Quad
            '\u2000',
            // Em Quad
            '\u2001',
            // En Space
            '\u2002',
            // Em Space
            '\u2003',
            // Three-Per-Em Space
            '\u2004',
            // Four-Per-Em Space
            '\u2005',
            // Six-Per-Em Space
            '\u2006',
            // Figure Space
            '\u2007',
            // Punctuation Space
            '\u2008',
            // Thin Space
            '\u2009',
            // Hair Space
            '\u200A',
            // Narrow No-Break Space
            '\u202F',
            // Medium Mathematical Space
            '\u205F',
            // Ideographic Space
            '\u3000',
        ];
}

public static class Tokens
{
    public static readonly Parser<TextSpan> UnicodeSpace = Capture(
        Literals.AnyOf(CharacterSets.NonNewLineWhitespaceChars).Or(Literals.WhiteSpace())
    );

    public static readonly Parser<TextSpan> MultiLineComment = SlashStar.And(CommentedBlock)
    public static readonly Parser<TextSpan> CommentedBlock = StarSlash.Or(MultiLineComment)
}

public abstract record Token(int Line, int Column);

public abstract record KeywordToken(int Line, int Column) : Token(Line, Column);

public sealed record IdentifierToken(string Name, int Line, int Column) : Token(Line, Column);

public sealed record IntegerLiteralToken(long Value, int Line, int Column) : Token(Line, Column);

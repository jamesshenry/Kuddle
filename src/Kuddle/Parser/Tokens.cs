using System;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Kuddle.Parser;

public static class CharacterSets
{
    public static ReadOnlySpan<char> Digits => "0123456789";
    public static ReadOnlySpan<char> DigitsAndUnderscore => "0123456789_";
    public static ReadOnlySpan<char> StringExcludedChars =>
        ['[', '"', '\\', 'b', 'f', 'n', 't', 'r', 's'];

    internal static bool IsWhiteSpace(char c)
    {
        return c switch
        {
            '\u0009' => true, // Character Tabulation
            '\u0020' => true, // Space
            '\u00A0' => true, // No-Break Space
            '\u1680' => true, // Ogham Space Mark
            '\u2000' => true, // En Quad
            '\u2001' => true, // Em Quad
            '\u2002' => true, // En Space
            '\u2003' => true, // Em Space
            '\u2004' => true, // Three-Per-Em Space
            '\u2005' => true, // Four-Per-Em Space
            '\u2006' => true, // Six-Per-Em Space
            '\u2007' => true, // Figure Space
            '\u2008' => true, // Punctuation Space
            '\u2009' => true, // Thin Space
            '\u200A' => true, // Hair Space
            '\u202F' => true, // Narrow No-Break Space
            '\u205F' => true, // Medium Mathematical Space
            '\u3000' => true, // Ideographic Space
            _ => false,
        };
    }

    internal static bool IsNewline(char c)
    {
        return c switch
        {
            '\u000D' => true,
            '\u000A' => true,
            '\u0085' => true,
            '\u000B' => true,
            '\u000C' => true,
            '\u2028' => true,
            '\u2029' => true,
            _ => false,
        };
    }

    // internal static bool IsDisallowedLiteralCodePoint(char c)
    // {
    //     return c switch
    //     {
    //         >= '\u0000' and <= '\u0008' => true, // various control characters
    //         '\u007F' => true,
    //         >= '\uD800' and <= '\uDFFF' => true, // unicode scalar value
    //         (>= '\u200E' and <= '\u200F')
    //         or (>= '\u202A' and <= '\u202E')
    //         or (>= '\u2066' and <= '\u2069') => true, // unicode 'direction control' characters
    //         '\uFEFF' => true, // unicode BOM
    //         _ => false,
    //     };
    // }
}

public static class Tokens
{
    public static Parser<TextSpan> Sign => Literals.AnyOf(['+', '-'], 1, 1);
    public static Parser<string> Boolean => Literals.Text("#true").Or(Literals.Text("#false"));
}

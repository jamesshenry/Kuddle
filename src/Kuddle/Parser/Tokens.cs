using System;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Kuddle.Parser;

public static class CharacterSets
{
    public static ReadOnlySpan<char> WhitespaceChars =>
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

    public static ReadOnlySpan<char> NewLineChars =>
        ['\u000D', '\u000A', '\u0085', '\u000B', '\u000C', '\u2028', '\u2029'];
    public static ReadOnlySpan<char> WhiteSpaceAndNewLineChars
    {
        get
        {
            var buffer = new char[WhitespaceChars.Length + NewLineChars.Length];
            WhitespaceChars.CopyTo(buffer);
            NewLineChars.CopyTo(buffer);
            return buffer;
        }
    }

    public static ReadOnlySpan<char> Digits => "0123456789";
    public static ReadOnlySpan<char> DigitsAndUnderscore => "0123456789_";
    public static ReadOnlySpan<char> StringExcludedChars =>
        ['[', '"', '\\', 'b', 'f', 'n', 't', 'r', 's'];
    public static ReadOnlySpan<char> IdentifierExcludedChars
    {
        get
        {
            // combine manually once at startup
            var s1 = WhitespaceChars;
            var s2 = NewLineChars;
            var extras = "\\/(){};[]\"#=";

            var buffer = new char[s1.Length + s2.Length + extras.Length];
            s1.CopyTo(buffer);
            s2.CopyTo(buffer.AsSpan(s1.Length));
            extras.AsSpan().CopyTo(buffer.AsSpan(s1.Length + s2.Length));

            return buffer;
        }
    }
}

public static class Tokens
{
    public static Parser<TextSpan> Sign => Literals.AnyOf(['+', '-'], 1, 1);
    public static Parser<string> Boolean => Literals.Text("#true").Or(Literals.Text("#false"));
}

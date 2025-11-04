using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace Kuddle.Parser;

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

    public static ReadOnlySpan<char> NewLineWhitespaceChars =>
        ['\u000D', '\u000A', '\u0085', '\u000B', '\u000C', '\u2028', '\u2029'];
}

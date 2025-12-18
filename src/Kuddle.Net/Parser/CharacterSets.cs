using System;
using System.Collections.Generic;

namespace Kuddle.Parser;

public static class CharacterSets
{
    public static ReadOnlySpan<char> Digits => "0123456789";
    public static ReadOnlySpan<char> DigitsAndUnderscore => "0123456789_";
    public static ReadOnlySpan<char> StringExcludedChars =>
        ['[', '"', '\\', 'b', 'f', 'n', 't', 'r', 's'];
    public static ReadOnlySpan<char> WhiteSpaceChars =>
        [
            '\u0009',
            '\u0020',
            '\u00A0',
            '\u1680',
            '\u2000',
            '\u2001',
            '\u2002',
            '\u2003',
            '\u2004',
            '\u2005',
            '\u2006',
            '\u2007',
            '\u2008',
            '\u2009',
            '\u200A',
            '\u202F',
            '\u205F',
            '\u3000',
        ];

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

    public static readonly HashSet<string> ReservedTypes =
    [
        "i8",
        "i16",
        "i32",
        "i64",
        "u8",
        "u16",
        "u32",
        "u64",
        "f32",
        "f64",
        "decimal64",
        "decimal128",
        "date-time",
        "time",
        "date",
        "duration",
        "decimal",
        "currency",
        "country-2",
        "country-3",
        "ipv4",
        "ipv6",
        "url",
        "uuid",
        "regex",
        "base64",
    ];

    /// <summary>
    /// Maps KDL type annotations to their corresponding CLR types.
    /// Not all reserved types have CLR equivalents (e.g., country codes, IP addresses as validated strings).
    /// </summary>
    public static readonly Dictionary<string, Type> TypeAnnotationToClrType = new()
    {
        ["i8"] = typeof(sbyte),
        ["i16"] = typeof(short),
        ["i32"] = typeof(int),
        ["i64"] = typeof(long),
        ["u8"] = typeof(byte),
        ["u16"] = typeof(ushort),
        ["u32"] = typeof(uint),
        ["u64"] = typeof(ulong),
        ["f32"] = typeof(float),
        ["f64"] = typeof(double),
        ["decimal64"] = typeof(decimal),
        ["decimal128"] = typeof(decimal),
        ["decimal"] = typeof(decimal),
        ["date-time"] = typeof(DateTimeOffset),
        ["time"] = typeof(TimeOnly),
        ["date"] = typeof(DateOnly),
        ["duration"] = typeof(TimeSpan),
        ["uuid"] = typeof(Guid),
        ["url"] = typeof(Uri),
        ["base64"] = typeof(byte[]),
        // These remain as strings with semantic meaning:
        // "currency", "country-2", "country-3", "ipv4", "ipv6", "regex"
    };

    /// <summary>
    /// Gets the CLR type for a KDL type annotation, or null if no mapping exists.
    /// </summary>
    public static Type? GetClrType(string? typeAnnotation) =>
        typeAnnotation is not null
        && TypeAnnotationToClrType.TryGetValue(typeAnnotation, out var type)
            ? type
            : null;
}

namespace Kuddle.AST;

public abstract record KdlValue
{
    public string? TypeAnnotation { get; init; }
}

public sealed record KdlString(string Value) : KdlValue;

public sealed record KdlBoolean(bool Value) : KdlValue;

public sealed record KdlNull() : KdlValue;

// public enum NumberKind
// {
//     Integer,
//     Decimal,
//     Special,
// }

public enum NumberBase
{
    Decimal,
    Hex,
    Octal,
    Binary,
}

public static class ASTExtensions { }

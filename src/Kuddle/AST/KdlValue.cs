namespace Kuddle.AST;

public abstract record KdlValue
{
    public string? TypeAnnotation { get; init; }
}

public sealed record KdlString(string Value) : KdlValue;

public sealed record KdlBoolean(bool Value) : KdlValue;

public sealed record KdlNull() : KdlValue;

public sealed record KdlNumber(string RawValue, NumberBase Base) : KdlValue;

public enum NumberBase
{
    Decimal,
    Hex,
    Octal,
    Binary,
}

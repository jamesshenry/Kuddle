namespace Kuddle.AST;

public enum StringKind
{
    Identifier,
    Quoted,
    Raw,
    MultiLine,
}

public sealed record KdlString(string Value, StringKind Kind) : KdlValue
{
    public override string ToString() => Value;
}

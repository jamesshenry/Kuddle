namespace Kuddle.AST;

public sealed record KdlProperty(KdlString Key, KdlValue Value) : KdlEntry
{
    public string EqualsTrivia { get; init; } = "=";
}

namespace Kuddle.AST;

public record KdlObject
{
    public string LeadingTrivia { get; init; } = string.Empty;
    public string TrailingTrivia { get; init; } = string.Empty;
}

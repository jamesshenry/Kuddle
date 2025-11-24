using System.Collections.Generic;

namespace Kuddle.AST;

public record KdlObject
{
    public string LeadingTrivia { get; init; } = string.Empty;
    public string TrailingTrivia { get; init; } = string.Empty;
}

public sealed record KdlDocument : KdlObject
{
    public List<KdlNode> Nodes { get; init; } = [];
}

public sealed record KdlBlock : KdlObject
{
    public List<KdlNode> Nodes { get; init; } = [];
}

public sealed record KdlNode(KdlIdentifier Name) : KdlObject
{
    public List<KdlEntry> Entries { get; init; } = [];

    public KdlBlock? Children { get; init; }

    public bool TerminatedBySemicolon { get; init; }
}

public abstract record KdlEntry : KdlObject;

public sealed record KdlArgument(KdlValue Value) : KdlEntry;

public sealed record KdlProperty(KdlIdentifier Key, KdlValue Value) : KdlEntry
{
    public string EqualsTrivia { get; init; } = "=";
}

public sealed record KdlSkippedEntry(string RawText) : KdlEntry;

public sealed record KdlIdentifier(string Name) : KdlObject
{
    public string RawText { get; init; } = Name;

    public string? TypeAnnotation { get; init; }
}

public abstract record KdlValue : KdlObject
{
    public string? TypeAnnotation { get; init; }
}

public enum StringKind
{
    Quoted,
    Raw,
    MultiLine,
}

public sealed record KdlString(string Value, StringKind Kind) : KdlValue
{
    public override string ToString() => Value;
}

public sealed record KdlBool(bool Value) : KdlValue;

public sealed record KdlNull : KdlValue;

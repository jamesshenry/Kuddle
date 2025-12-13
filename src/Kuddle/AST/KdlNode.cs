using System.Collections.Generic;

namespace Kuddle.AST;

public sealed record KdlNode(KdlString Name) : KdlObject
{
    public List<KdlEntry> Entries { get; init; } = [];

    public KdlBlock? Children { get; init; }

    public bool TerminatedBySemicolon { get; init; }
    public string? TypeAnnotation { get; init; }
}

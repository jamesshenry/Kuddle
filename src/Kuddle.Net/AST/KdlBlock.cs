using System.Collections.Generic;

namespace Kuddle.AST;

public sealed record KdlBlock : KdlObject
{
    public List<KdlNode> Nodes { get; init; } = [];
}

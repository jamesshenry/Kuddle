using System.Collections.Generic;
using Kuddle.Serialization;

namespace Kuddle.AST;

public sealed record KdlDocument : KdlObject
{
    public List<KdlNode> Nodes { get; init; } = [];

    public string ToString(KdlWriterOptions? options = null)
    {
        return KdlWriter.Write(this, options);
    }

    public override string ToString()
    {
        return KdlWriter.Write(this);
    }
}

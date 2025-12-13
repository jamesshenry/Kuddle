using System.Collections.Generic;
using Kuddle.Serialization;

namespace Kuddle.AST;

public sealed record KdlDocument : KdlObject
{
    public List<KdlNode> Nodes { get; init; } = [];

    public string ToString(KuddleWriterOptions? options = null)
    {
        return KuddleWriter.Write(this, options);
    }

    public override string ToString()
    {
        return KuddleWriter.Write(this);
    }
}

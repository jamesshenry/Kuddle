using System.Collections.Generic;

namespace Kuddle.AST;

public sealed record KdlNode(KdlString Name) : KdlObject
{
    public List<KdlEntry> Entries { get; init; } = [];

    public KdlBlock? Children { get; init; }

    public bool TerminatedBySemicolon { get; init; }
    public string? TypeAnnotation { get; init; }

    /// <summary>
    /// Gets the value of the last property with the specified name (per KDL spec, last wins).
    /// Returns null if no property with that name exists.
    /// </summary>
    public KdlValue? this[string key]
    {
        get
        {
            for (var i = Entries.Count - 1; i >= 0; i--)
            {
                if (
                    Entries[i] is KdlProperty { Key.Value: var propKey, Value: var value }
                    && propKey == key
                )
                {
                    return value;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Gets all arguments (positional values) for this node.
    /// </summary>
    public IEnumerable<KdlValue> Arguments
    {
        get
        {
            foreach (var entry in Entries)
            {
                if (entry is KdlArgument arg)
                {
                    yield return arg.Value;
                }
            }
        }
    }

    /// <summary>
    /// Gets all properties (key-value pairs) for this node.
    /// </summary>
    public IEnumerable<KdlProperty> Properties
    {
        get
        {
            foreach (var entry in Entries)
            {
                if (entry is KdlProperty prop)
                {
                    yield return prop;
                }
            }
        }
    }

    public bool HasChildren => Children != null && Children.Nodes!.Count > 0;
}

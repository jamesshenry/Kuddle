using System;

namespace Kuddle.Serialization;

/// <summary>
/// Maps a collection to a child node (container) that holds the items.
/// </summary>
/// <param name="nodeName">The name of the wrapper/container node.</param>
/// <param name="elementName">The node name for items inside the container. If null, uses the type's default.</param>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class KdlNodeCollectionAttribute(string nodeName, string? elementName = null)
    : KdlEntryAttribute
{
    public string NodeName { get; } = nodeName;
    public string? ElementName { get; } = elementName;
}

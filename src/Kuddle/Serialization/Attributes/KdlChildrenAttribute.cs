using System;

namespace Kuddle.Serialization;

/// <summary>
/// Marks a property to receive child nodes of a specific type from the KDL node's children block.
/// </summary>
/// <param name="childNodeName">The KDL node name to match for children (e.g., "dependency").</param>
[AttributeUsage(AttributeTargets.Property)]
public class KdlChildrenAttribute(string childNodeName) : Attribute
{
    /// <summary>Gets the child node name to match.</summary>
    public string ChildNodeName { get; } = childNodeName;
}

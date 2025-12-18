using System;

namespace Kuddle.Serialization;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Property,
    AllowMultiple = false,
    Inherited = false
)]
public sealed class KdlTypeAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}

using System;

namespace Kuddle.Serialization;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class KdlPropertyAttribute(string? key = null) : Attribute
{
    public string? Key { get; } = key;
}

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Property,
    AllowMultiple = false,
    Inherited = false
)]
public sealed class KdlTypeAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}

using System;

namespace Kuddle.Serialization;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class KdlNodeAttribute(string? name = null) : KdlEntryAttribute
{
    public string? Name { get; } = name;
    public bool Flatten { get; set; }
}

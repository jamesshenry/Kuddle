using System;

namespace Kuddle.Serialization;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class KdlNodeAttribute(string? name = null, string? typeAnnotation = null)
    : KdlEntryAttribute(typeAnnotation)
{
    public string? Name { get; } = name;
}

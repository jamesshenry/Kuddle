using System;

namespace Kuddle.Serialization;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class KdlPropertyAttribute(string? key = null) : Attribute
{
    public string? Key { get; } = key;
}

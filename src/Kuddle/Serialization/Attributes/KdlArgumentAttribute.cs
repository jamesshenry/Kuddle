using System;

namespace Kuddle.Serialization;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class KdlArgumentAttribute(int index) : Attribute
{
    public int Index { get; } = index;
}

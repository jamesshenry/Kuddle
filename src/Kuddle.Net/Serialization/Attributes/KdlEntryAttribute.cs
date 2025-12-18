using System;

namespace Kuddle.Serialization;

/// <summary>
/// Base class for KDL entry attributes. Only one entry attribute can be applied per property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class KdlEntryAttribute : Attribute
{
    /// <summary>
    /// Optional KDL type annotation (e.g., "uuid", "date-time", "i32").
    /// </summary>
    public string? TypeAnnotation { get; }

    protected KdlEntryAttribute(string? typeAnnotation = null)
    {
        TypeAnnotation = typeAnnotation;
    }
}

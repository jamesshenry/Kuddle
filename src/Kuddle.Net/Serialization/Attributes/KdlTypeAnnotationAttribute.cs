using System;

namespace Kuddle.Serialization;

/// <summary>
/// Specifies a KDL type annotation to be applied when serializing this property or argument.
/// When deserializing, the type annotation is used to guide conversion (e.g., parsing UUID strings).
/// </summary>
/// <remarks>
/// Examples:
/// <code>
/// [KdlTypeAnnotation("uuid")]
/// public Guid Id { get; set; }
///
/// [KdlTypeAnnotation("date-time")]
/// public DateTimeOffset CreatedAt { get; set; }
///
/// [KdlTypeAnnotation("i32")]
/// public int Count { get; set; }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class KdlTypeAnnotationAttribute(string annotation) : Attribute
{
    /// <summary>
    /// The type annotation string (e.g., "uuid", "date-time", "i32").
    /// </summary>
    public string Annotation { get; } = annotation;
}

namespace Kuddle.Serialization;

/// <summary>
/// Options for KDL serialization and deserialization.
/// </summary>
public record KdlSerializerOptions
{
    /// <summary>
    /// Whether to ignore null values when serializing. Default is true.
    /// </summary>
    public bool IgnoreNullValues { get; init; } = true;

    /// <summary>
    /// Whether property/node name comparison is case-insensitive. Default is true.
    /// </summary>
    public bool CaseInsensitiveNames { get; init; } = true;

    /// <summary>
    /// Whether to include type annotations in output. Default is true.
    /// </summary>
    public bool WriteTypeAnnotations { get; init; } = true;

    /// <summary>
    /// Default options instance.
    /// </summary>
    public static KdlSerializerOptions Default { get; } = new();
}

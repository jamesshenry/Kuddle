namespace Kuddle.Serialization;

public record KdlSerializerOptions
{
    public static KdlSerializerOptions Default { get; } = new KdlSerializerOptions();

    public bool IgnoreNullValues { get; init; } = true;

    public bool SimpleCollectionNodeNames { get; init; } = true;

    public KdlRootMapping RootMapping { get; init; } = KdlRootMapping.AsNode;

    public KdlReaderOptions Reader { get; init; } = KdlReaderOptions.Default;
    public KdlWriterOptions Writer { get; init; } = KdlWriterOptions.Default;
    public KdlStringStyle StringStyle
    {
        get => Writer.StringStyle;
        init => Writer = Writer with { StringStyle = value };
    }
}

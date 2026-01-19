namespace Kuddle.Serialization;

public record KdlWriterOptions
{
    public static KdlWriterOptions Default { get; } = new();
    public KdlWriterIndentType IndentType { get; init; } = KdlWriterIndentType.Spaces;
    public KdlWriterIndentSize IndentSize { get; init; } = KdlWriterIndentSize.Four;
    internal string IndentChar =>
        IndentType == KdlWriterIndentType.Tabs ? "\t" : new string(' ', (int)IndentSize);
    internal string NewLine { get; } = "\n";
    internal string SpaceAfterProp { get; } = " ";
    public bool EscapeUnicode { get; init; } = false;
    public KdlStringStyle StringStyle { get; init; } = KdlStringStyle.Default;
}

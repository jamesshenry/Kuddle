namespace Kuddle.Serialization;

public record KdlWriterOptions
{
    public static KdlWriterOptions Default => new();

    public string IndentChar { get; init; } = "    ";
    public string NewLine { get; init; } = "\n";
    public string SpaceAfterProp { get; init; } = " ";
    public bool EscapeUnicode { get; init; } = false;
    public bool RoundTrip { get; set; } = true;
}

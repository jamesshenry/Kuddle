namespace Kuddle.Serialization;

public record KuddleWriterOptions
{
    public static KuddleWriterOptions Default => new();

    public string IndentChar { get; init; } = "    ";
    public string NewLine { get; init; } = "\n";
    public string SpaceAfterProp { get; init; } = " ";
    public bool EscapeUnicode { get; init; } = false;
    public bool RoundTrip { get; set; } = true;
}

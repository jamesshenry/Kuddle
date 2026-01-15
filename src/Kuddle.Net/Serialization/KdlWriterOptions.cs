namespace Kuddle.Serialization;

public record KdlWriterOptions
{
    public static KdlWriterOptions Default => new();

    public string IndentChar { get; init; } = "    ";
    public string NewLine { get; init; } = "\n";
    public string SpaceAfterProp { get; init; } = " ";
    public bool EscapeUnicode { get; init; } = false;
    public KdlStringStyle StringStyle { get; init; } = KdlStringStyle.Default;
}

[System.Flags]
public enum KdlStringStyle
{
    AlwaysQuoted = 0,
    AllowBare = 1 << 0, // 1
    Preserve = 1 << 1, // 2

    RawPaths = 1 << 2, // 4 (Slashes trigger Raw)
    PreferRaw = 1 << 3, // 8 (Any escape triggers Raw)

    AllowMultiline = 1 << 4, // 16
    Default = AllowBare | RawPaths,
}

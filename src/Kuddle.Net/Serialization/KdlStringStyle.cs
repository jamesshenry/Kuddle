namespace Kuddle.Serialization;

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

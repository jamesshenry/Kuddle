namespace Kuddle.Serialization;

public record KdlReaderOptions
{
    public static KdlReaderOptions Default { get; } = new();
    public bool ValidateReservedTypes { get; init; } = true;
}

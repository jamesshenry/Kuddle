namespace Kuddle.Serialization;

public record KdlReaderOptions
{
    public static KdlReaderOptions Default => new() { ValidateReservedTypes = true };
    public bool ValidateReservedTypes { get; init; } = true;
}

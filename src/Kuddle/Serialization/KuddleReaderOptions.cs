namespace Kuddle;

public record KuddleReaderOptions
{
    public static KuddleReaderOptions Default => new() { ValidateReservedTypes = true };
    public bool ValidateReservedTypes { get; init; } = true;
}

namespace Kuddle.Tests.Serialization.Models;

public class TemporalStressModel
{
    public DateTime UtcTime { get; set; }
    public DateTime LocalTime { get; set; }
    public DateTime UnspecifiedTime { get; set; }

    public DateTimeOffset OffsetTime { get; set; }
    public DateTimeOffset NewYorkTime { get; set; }

    public DateOnly JustDate { get; set; }
    public TimeOnly JustTime { get; set; }
    public TimeSpan Duration { get; set; }

    // Nullable versions
    public DateTime? NullableDate { get; set; }
    public AppSettings? AppSettings { get; set; }
}

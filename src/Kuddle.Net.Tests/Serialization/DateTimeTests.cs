using System.Diagnostics;
using Kuddle.Serialization;
using Kuddle.Tests.Serialization.Models;

namespace Kuddle.Tests.Serialization;

public class DateTimeStressTests
{
    [Test]
    public async Task TemporalTypes_FullRoundTrip_MaintainsAccuracy()
    {
        var original = new TemporalStressModel
        {
            UtcTime = new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Utc),
            LocalTime = new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Local),
            UnspecifiedTime = new DateTime(2025, 1, 1, 10, 30, 0, DateTimeKind.Unspecified),

            OffsetTime = DateTimeOffset.UtcNow,
            NewYorkTime = new DateTimeOffset(2025, 1, 1, 10, 30, 0, TimeSpan.FromHours(-5)),

            JustDate = new DateOnly(2025, 12, 25),
            JustTime = new TimeOnly(23, 59, 59),
            Duration = TimeSpan.FromDays(1.5),
            NullableDate = null,
            AppSettings = new AppSettings(),
        };
        var options = KdlSerializerOptions.Default with { RootMapping = KdlRootMapping.AsDocument };
        // Act
        var kdl = KdlSerializer.Serialize(original, options);
        Debug.WriteLine(kdl);
        var deserialized = KdlSerializer.Deserialize<TemporalStressModel>(kdl, options);

        // Assert
        await Assert.That(deserialized.UtcTime).IsEqualTo(original.UtcTime);
        await Assert.That(deserialized.OffsetTime).IsEqualTo(original.OffsetTime);
        await Assert.That(deserialized.NewYorkTime.Offset).IsEqualTo(TimeSpan.FromHours(-5));
        await Assert.That(deserialized.JustDate).IsEqualTo(original.JustDate);
        await Assert.That(deserialized.Duration).IsEqualTo(original.Duration);
        await Assert.That(deserialized.NullableDate).IsNull();
    }

    [Test]
    public async Task DateTime_Coaxing_FromOffsetString()
    {
        // KDL contains a specific offset, but C# target is a simple DateTime
        var kdl = "temporal-stress-model utc-time=(date-time)\"2024-01-01T12:00:00+05:00\"";

        var result = KdlSerializer.Deserialize<TemporalStressModel>(kdl);

        // It should successfully strip the offset and give us the local-equivalent DateTime
        await Assert.That(result.UtcTime.Year).IsEqualTo(2024);
        await Assert.That(result.UtcTime.Hour).IsEqualTo(12);
    }

    [Test]
    public async Task Temporal_BoundaryValues_ShouldWork()
    {
        var original = new TemporalStressModel
        {
            UtcTime = DateTime.MinValue,
            LocalTime = DateTime.MaxValue,
            OffsetTime = DateTimeOffset.MinValue,
            NewYorkTime = DateTimeOffset.MaxValue,
        };

        var kdl = KdlSerializer.Serialize(original);
        var deserialized = KdlSerializer.Deserialize<TemporalStressModel>(kdl);

        await Assert.That(deserialized.UtcTime).IsEqualTo(DateTime.MinValue);
        await Assert.That(deserialized.LocalTime).IsEqualTo(DateTime.MaxValue);
    }

    [Test]
    public async Task SubMillisecondPrecision_IsPreserved()
    {
        // Many serializers fail on Ticks precision. ISO "O" format should handle this.
        var highPrecision = new DateTime(2025, 1, 1, 10, 30, 0, 500, DateTimeKind.Utc).AddTicks(
            1234
        );

        var kdl = KdlSerializer.Serialize(new TemporalStressModel { UtcTime = highPrecision });
        var result = KdlSerializer.Deserialize<TemporalStressModel>(kdl);

        await Assert.That(result.UtcTime.Ticks).IsEqualTo(highPrecision.Ticks);
    }

    [Test]
    public async Task DateOnly_TimeOnly_SpecificMapping()
    {
        var kdl = """
            temporal-stress-model {
                utc-time "2028-02-29" // Leap year
                just-date (date)"2028-02-29" // Leap year
                just-time (time)"13:45:00"
            }
            """;
        var options = KdlSerializerOptions.Default; // with { RootMapping = KdlRootMapping.AsDocument };

        var result = KdlSerializer.Deserialize<TemporalStressModel>(kdl, options);
        await Assert.That(result.UtcTime.Year).IsEqualTo(2028);
        await Assert.That(result.UtcTime.Month).IsEqualTo(2);
        await Assert.That(result.UtcTime.Day).IsEqualTo(29);
        await Assert.That(result.JustDate.Year).IsEqualTo(2028);
        await Assert.That(result.JustDate.Month).IsEqualTo(2);
        await Assert.That(result.JustDate.Day).IsEqualTo(29);
        await Assert.That(result.JustTime.Hour).IsEqualTo(13);
    }

    [Test]
    public async Task TimeSpan_HandlesVariousFormats()
    {
        // Testing that your KdlValueConverter can handle TimeSpan via string parsing
        var kdl = "temporal-stress-model duration=(duration)\"12:30:00\"";
        var result = KdlSerializer.Deserialize<TemporalStressModel>(kdl);
        await Assert.That(result.Duration.Hours).IsEqualTo(12);
        await Assert.That(result.Duration.Minutes).IsEqualTo(30);

        var kdlDays = "temporal-stress-model duration=(duration)\"5.10:00:00\"";
        var resultDays = KdlSerializer.Deserialize<TemporalStressModel>(kdlDays);
        await Assert.That(resultDays.Duration.Days).IsEqualTo(5);
    }
}

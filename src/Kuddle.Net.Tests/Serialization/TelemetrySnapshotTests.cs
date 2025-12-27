using System.Diagnostics;
using Kuddle.Serialization;
using Kuddle.Tests.Serialization.Models;

namespace Kuddle.Tests.Serialization;

public class TelemetrySnapshotTests
{
    [Test]
    public async Task RoundTrip_TelemetrySnapshot_SerializesAndDeserializes()
    {
        var original = new TelemetrySnapshot
        {
            SnapshotId = Guid.NewGuid(),
            CapturedAt = DateTimeOffset.UtcNow,
            Services =
            {
                ["svc1"] = new ServiceInfo
                {
                    Name = "Inventory",
                    Status = ServiceStatus.Healthy,
                    Version = new VersionInfo
                    {
                        VersionString = "1.2.3",
                        Major = 1,
                        Minor = 2,
                        Patch = 3,
                    },
                    Metrics = { ["cpu"] = 0.75 },
                    Dependencies =
                    {
                        ["dep-type"] = new List<DependencyInfo>
                        {
                            new() { DependencyName = "db", Type = DependencyType.Database },
                        },
                    },
                    Endpoints = new List<EndpointInfo>
                    {
                        new()
                        {
                            Route = "/items",
                            Method = Models.HttpMethod.Get,
                            RequiresAuth = true,
                        },
                    },
                },
            },
            GlobalTags =
            {
                ["global"] = new Dictionary<string, string>
                {
                    ["region"] = "uk-south",
                    ["timezone"] = "gmt",
                },
            },
            Environment = new EnvironmentInfo
            {
                Name = "prod",
                Region = "uk-west",
                Machines =
                {
                    ["host1"] = new MachineInfo
                    {
                        Os = "linux",
                        CpuCores = 4,
                        MemoryBytes = 8L * 1024 * 1024 * 1024,
                    },
                },
            },
            Events = new List<EventRecord>
            {
                new EventRecord
                {
                    EventId = Guid.NewGuid(),
                    Timestamp = DateTimeOffset.UtcNow,
                    Severity = EventSeverity.Info,
                    Message = "Started",
                },
                new EventRecord
                {
                    EventId = Guid.NewGuid(),
                    Timestamp = DateTimeOffset.UtcNow.AddSeconds(10),
                    Severity = EventSeverity.Warning,
                    Message = "Slow start",
                },
            },
            Metadata = { ["a"] = "one", ["b"] = "two" },
        };

        var kdl = KdlSerializer.Serialize(original);

        Debug.WriteLine(kdl);

        var deserialized = KdlSerializer.Deserialize<TelemetrySnapshot>(kdl);

        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized.SnapshotId).IsEqualTo(original.SnapshotId);
        await Assert.That(deserialized.CapturedAt).IsEqualTo(original.CapturedAt);
        await Assert.That(deserialized.Services).ContainsKey("svc1");
        await Assert.That(deserialized.Services["svc1"].Name).IsEqualTo("Inventory");
        await Assert.That(deserialized.Services["svc1"].Metrics["cpu"]).IsEqualTo(0.75);
        await Assert.That(deserialized.GlobalTags["global"]["region"]).IsEqualTo("uk-south");
        await Assert.That(deserialized.GlobalTags["global"]["timezone"]).IsEqualTo("gmt");
        await Assert.That(deserialized.Environment.Name).IsEqualTo("prod");
        await Assert.That(deserialized.Environment.Region).IsEqualTo("uk-west");
        await Assert.That(deserialized.Environment.Machines).ContainsKey("host1");
        await Assert.That(deserialized.Events).Count().IsEqualTo(2);
    }
}

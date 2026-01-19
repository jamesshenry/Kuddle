using Kuddle.Exceptions;
using Kuddle.Serialization;
using Kuddle.Tests.Serialization.Models;
using HttpMethod = Kuddle.Tests.Serialization.Models.HttpMethod;

namespace Kuddle.Tests.Conversion;

/// <summary>
/// Tests for round-trip serialization: serialize then deserialize preserves data.
/// </summary>
public class RoundTripTests
{
    #region Simple Round-Trips

    [Test]
    public async Task RoundTrip_SimpleObject_PreservesData()
    {
        // Arrange
        var original = new Package
        {
            Name = "test-pkg",
            Version = "2.5.0",
            Description = "Test",
        };

        // Act
        var kdl = KdlSerializer.Serialize(original);
        var deserialized = KdlSerializer.Deserialize<Package>(kdl);

        // Assert
        await Assert.That(deserialized.Name).IsEqualTo(original.Name);
        await Assert.That(deserialized.Version).IsEqualTo(original.Version);
        await Assert.That(deserialized.Description).IsEqualTo(original.Description);
    }

    [Test]
    public async Task RoundTrip_NestedObject_PreservesData()
    {
        // Arrange
        var original = new Project
        {
            Name = "my-app",
            Version = "1.2.3",
            Dependencies =
            [
                new Dependency
                {
                    Package = "dep1",
                    Version = "1.0",
                    Optional = false,
                },
                new Dependency
                {
                    Package = "dep2",
                    Version = "2.0",
                    Optional = true,
                },
            ],
        };

        // Act
        var kdl = KdlSerializer.Serialize(original);
        var deserialized = KdlSerializer.Deserialize<Project>(kdl);

        // Assert
        await Assert.That(deserialized.Name).IsEqualTo(original.Name);
        await Assert.That(deserialized.Dependencies).Count().IsEqualTo(2);
        await Assert.That(deserialized.Dependencies[0].Package).IsEqualTo("dep1");
        await Assert.That(deserialized.Dependencies[1].Optional).IsTrue();
    }

    #endregion

    #region Collection Strategy Round-Trips

    [Test]
    public async Task RoundTrip_SerializeAndDeserialize_WithSimpleCollectionNodeNames_True()
    {
        // Arrange
        var originalModel = new CollectionModel
        {
            WrappedPlugins = [new() { Name = "Auth" }, new() { Name = "Logging" }],
            FlattenedServers = [new() { Host = "localhost" }, new() { Host = "remote" }],
        };

        var options = new KdlSerializerOptions
        {
            SimpleCollectionNodeNames = true,
            RootMapping = KdlRootMapping.AsDocument,
        };

        // Act
        var kdl = KdlSerializer.Serialize(originalModel, options);
        var deserializedModel = KdlSerializer.Deserialize<CollectionModel>(kdl, options);

        // Assert
        await Assert.That(deserializedModel.WrappedPlugins).Count().IsEqualTo(2);
        await Assert.That(deserializedModel.WrappedPlugins[0].Name).IsEqualTo("Auth");
        await Assert.That(deserializedModel.WrappedPlugins[1].Name).IsEqualTo("Logging");
        await Assert.That(deserializedModel.FlattenedServers).Count().IsEqualTo(2);
        await Assert.That(deserializedModel.FlattenedServers[0].Host).IsEqualTo("localhost");
        await Assert.That(deserializedModel.FlattenedServers[1].Host).IsEqualTo("remote");
    }

    [Test]
    public async Task RoundTrip_SerializeAndDeserialize_WithSimpleCollectionNodeNames_False()
    {
        // Arrange
        var originalModel = new CollectionModel
        {
            WrappedPlugins = [new() { Name = "Auth" }, new() { Name = "Logging" }],
            FlattenedServers = [new() { Host = "localhost" }, new() { Host = "remote" }],
        };

        var options = new KdlSerializerOptions
        {
            SimpleCollectionNodeNames = false,
            RootMapping = KdlRootMapping.AsNode,
        };

        // Act
        var kdl = KdlSerializer.Serialize(originalModel, options);
        var deserializedModel = KdlSerializer.Deserialize<CollectionModel>(kdl, options);

        // Assert
        await Assert.That(deserializedModel.WrappedPlugins).Count().IsEqualTo(2);
        await Assert.That(deserializedModel.WrappedPlugins[0].Name).IsEqualTo("Auth");
        await Assert.That(deserializedModel.WrappedPlugins[1].Name).IsEqualTo("Logging");
        await Assert.That(deserializedModel.FlattenedServers).Count().IsEqualTo(2);
        await Assert.That(deserializedModel.FlattenedServers[0].Host).IsEqualTo("localhost");
        await Assert.That(deserializedModel.FlattenedServers[1].Host).IsEqualTo("remote");
    }

    #endregion

    #region Complex Round-Trips

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
                        BuildDate = DateTime.UtcNow,
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
                            Method = HttpMethod.Get,
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

    #endregion

    #region Error Handling

    [Test]
    public async Task DeserializeObject_WithInvalidNumericValue_ThrowsException()
    {
        // Arrange
        var kdl = """
            settings timeout="not-a-number"
            """;

        // Act & Assert
        await Assert
            .That(async () => KdlSerializer.Deserialize<Settings>(kdl))
            .Throws<KuddleSerializationException>();
    }

    #endregion

    #region Test Models

    public class Package
    {
        [KdlArgument(0)]
        public string Name { get; set; } = string.Empty;

        [KdlProperty("version")]
        public string? Version { get; set; }

        [KdlProperty("description")]
        public string? Description { get; set; }
    }

    public class Project
    {
        [KdlArgument(0)]
        public string Name { get; set; } = string.Empty;

        [KdlProperty("version")]
        public string Version { get; set; } = "1.0.0";

        [KdlNode("dependency", Flatten = true)]
        public List<Dependency> Dependencies { get; set; } = [];
    }

    public class Dependency
    {
        [KdlArgument(0)]
        public string Package { get; set; } = string.Empty;

        [KdlProperty("version")]
        public string Version { get; set; } = "*";

        [KdlProperty("optional")]
        public bool Optional { get; set; }
    }

    public class Settings
    {
        [KdlProperty("timeout")]
        public int Timeout { get; set; }
    }

    public class CollectionModel
    {
        [KdlNode("plugins", Flatten = false)]
        public List<PluginInfo> WrappedPlugins { get; set; } = [];

        [KdlNode("server", Flatten = true)]
        public List<ServerInfo> FlattenedServers { get; set; } = [];
    }

    public class PluginInfo
    {
        [KdlArgument(0)]
        public string Name { get; set; } = "";
    }

    public class ServerInfo
    {
        [KdlProperty]
        public string Host { get; set; } = "";
    }

    #endregion
}

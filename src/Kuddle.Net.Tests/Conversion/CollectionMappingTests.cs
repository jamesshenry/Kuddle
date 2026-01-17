using System.Diagnostics;
using Kuddle.Exceptions;
using Kuddle.Serialization;
using Kuddle.Tests.Serialization.Models;

namespace Kuddle.Tests.Conversion;

/// <summary>
/// Tests for collection mapping: lists, dictionaries, flatten strategies.
/// </summary>
public class CollectionMappingTests
{
    #region Dictionary Mapping

    [Test]
    public async Task DeserializeToDictionary_MapsCorrectly()
    {
        // Arrange
        var kdl = """
            themes {
                dark-mode {
                    window {
                        align v="Top" h="Left"
                        border color="#FFFFFF" style="solid"
                    }
                    button {
                        header text="Click Me"
                        align v="Middle" h="Center"
                    }
                }
                high-contrast {
                    window {
                        border color="#FFFF00"
                    }
                }
            }
            layouts {
                dashboard section="main-view" size=1 split="rows" {
                    child section="top-bar" size=1
                    child section="content-area" size=4 split="columns" {
                        child section="sidebar" size=1
                        child section="grid" size=3
                    }
                }
            }
            """;
        var options = KdlSerializerOptions.Default with { RootMapping = KdlRootMapping.AsDocument };

        // Act
        var result = KdlSerializer.Deserialize<AppSettings>(kdl, options);

        // Assert
        var dashboard = result.Layouts["dashboard"];
        await Assert.That(dashboard).IsNotNull();
        await Assert.That(dashboard.Section).IsEqualTo("main-view");
        await Assert.That(dashboard.Ratio).IsEqualTo(1);
        await Assert.That(dashboard.SplitDirection).IsEqualTo("rows");

        var contentArea = dashboard.Children.FirstOrDefault(c => c.Section == "grid");
        await Assert.That(contentArea!.Ratio).IsEqualTo(3);
    }

    [Test]
    public async Task DeserializeToEnumerable_WithDifferentNodeNames_ThrowsException()
    {
        // Arrange
        var kdl = """
            package "my-lib" version="1.0.0"
            reference "my-dep1" version="2.1.0"
            node "my-dep2" version="3.2.1"
            """;

        // Act & Assert
        await Assert
            .That(async () => KdlSerializer.Deserialize<List<Package>>(kdl))
            .Throws<KuddleSerializationException>();
    }

    [Test]
    public async Task Serialize_PropertyDictionary_WritesFlatProperties()
    {
        // Arrange
        var model = new PropertyDictModel();
        model.Tags["env"] = "production";
        model.Tags["region"] = "us-east-1";
        model.Settings["timeout"] = 5000;
        model.Settings["retries"] = 3;

        // Act
        var kdl = KdlSerializer.Serialize(model);

        // Assert
        await Assert.That(kdl).Contains("env=production");
        await Assert.That(kdl).Contains("region=us-east-1");
        await Assert.That(kdl).Contains("setting:timeout=5000");
        await Assert.That(kdl).Contains("setting:retries=3");
        await Assert.That(kdl).DoesNotContain("{");
    }

    [Test]
    public async Task Deserialize_PropertyDictionary_MapsFlatPropertiesBack()
    {
        // Arrange
        var kdl = "property-dict-model env=\"dev\" setting:port=8080";

        // Act
        var result = KdlSerializer.Deserialize<PropertyDictModel>(kdl);

        // Assert
        await Assert.That(result.Tags["env"]).IsEqualTo("dev");
        await Assert.That(result.Settings["port"]).IsEqualTo(8080);
    }

    [Test]
    public async Task Mapping_InvalidPropertyDictionary_ThrowsConfigurationException()
    {
        await Assert
            .That(() => KdlTypeMapping.For<InvalidPropertyDictModel>())
            .Throws<KdlConfigurationException>();
    }

    #endregion

    #region Collection Strategies

    [Test]
    public async Task Serialize_CollectionStrategies_WritesCorrectStructure()
    {
        // Arrange
        var model = new CollectionModel
        {
            WrappedPlugins = [new() { Name = "Auth" }],
            FlattenedServers = [new() { Host = "localhost" }, new() { Host = "127.0.0.1" }],
        };

        // Act
        var kdl = KdlSerializer.Serialize(model);

        // Assert Wrapped: plugins { plugininfo "Auth" }
        await Assert.That(kdl).Contains("plugins {");

        // Assert Flattened: server host="localhost"
        await Assert.That(kdl).Contains("server host=localhost");
        await Assert.That(kdl).Contains("server host=\"127.0.0.1\"");

        // Double check there isn't a wrapper node for servers
        await Assert.That(kdl).DoesNotContain("flattenedservers");
    }

    [Test]
    public async Task Deserialize_FlattenedCollection_CollectsAllMatchingNodes()
    {
        // Arrange
        var kdl = """
            plugins {
                plugininfo "Auth"
            }
            server host="localhost"
            server host="remote"
            """;
        var options = KdlSerializerOptions.Default with { RootMapping = KdlRootMapping.AsDocument };

        // Act
        var result = KdlSerializer.Deserialize<CollectionModel>(kdl, options);

        // Assert
        await Assert.That(result.WrappedPlugins).Count().IsEqualTo(1);
        await Assert.That(result.FlattenedServers).Count().IsEqualTo(2);
        await Assert.That(result.FlattenedServers[1].Host).IsEqualTo("remote");
    }

    [Test]
    public async Task Serialize_WithSimpleCollectionNodeNames_True_UseDashNodeNames()
    {
        // Arrange
        var model = new CollectionModel
        {
            WrappedPlugins = [new() { Name = "Auth" }],
            FlattenedServers = [new() { Host = "localhost" }],
        };
        var options = new KdlSerializerOptions { SimpleCollectionNodeNames = true };

        // Act
        var kdl = KdlSerializer.Serialize(model, options);

        // Assert - should use dash (-) for collection items
        await Assert.That(kdl).Contains("plugins {");
        await Assert.That(kdl).Contains("- Auth");
    }

    [Test]
    public async Task Serialize_WithSimpleCollectionNodeNames_False_UseTypedNodeNames()
    {
        // Arrange
        var model = new CollectionModel
        {
            WrappedPlugins = [new() { Name = "Auth" }],
            FlattenedServers = [new() { Host = "localhost" }],
        };
        var options = new KdlSerializerOptions { SimpleCollectionNodeNames = false };

        // Act
        var kdl = KdlSerializer.Serialize(model, options);

        // Assert - should use typed node names
        await Assert.That(kdl).Contains("plugins {");
        await Assert.That(kdl).Contains("plugin-info Auth");
    }

    #endregion

    #region Hybrid Models

    [Test]
    public async Task RoundTrip_HybridType_PreservesPropertiesAndDictionary()
    {
        var model = new HybridModel { Version = "2.5" };
        model["timeout"] = "5000";
        model["retry"] = "true";

        var kdl = KdlSerializer.Serialize(model);

        var deserialized = KdlSerializer.Deserialize<HybridModel>(kdl);

        await Assert.That(deserialized.Version).IsEqualTo("2.5");
        await Assert.That(deserialized["timeout"]).IsEqualTo("5000");
        await Assert.That(deserialized.Count).IsEqualTo(2);
    }

    #endregion

    #region Test Models

    public class Package
    {
        [KdlArgument(0)]
        public string Name { get; set; } = string.Empty;

        [KdlProperty("version")]
        public string? Version { get; set; }
    }

    public class PropertyDictModel
    {
        [KdlProperty]
        public Dictionary<string, string> Tags { get; set; } = new();

        [KdlProperty("setting")]
        public Dictionary<string, int> Settings { get; set; } = new();
    }

    public class InvalidPropertyDictModel
    {
        [KdlProperty]
        public Dictionary<string, ComplexValue> Items { get; set; } = new();
    }

    public class ComplexValue
    {
        public string? Name { get; set; } = default;
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

    public class HybridModel : Dictionary<string, string>
    {
        [KdlProperty("version")]
        public string Version { get; set; } = "1.0";
    }

    #endregion
}

using System.Diagnostics;
using Kuddle.AST;
using Kuddle.Exceptions;
using Kuddle.Serialization;
using Kuddle.Tests.Serialization.Models;

namespace Kuddle.Tests.Serialization;

/// <summary>
/// Tests for KdlSerializer.Deserialize<T>() and KdlSerializer.Serialize<T>()
/// which map between KDL documents and strongly-typed C# objects.
/// </summary>
public class ObjectMapperTests
{
    #region Basic Mapping Tests

    [Test]
    public async Task DeserializeSimpleObject_WithArgument_MapsCorrectly()
    {
        // Arrange
        var kdl = """
            package "my-lib"
            """;

        // Act
        var result = KdlSerializer.Deserialize<Package>(kdl);

        // Assert
        await Assert.That(result.Name).IsEqualTo("my-lib");
    }

    [Test]
    public async Task DeserializeSimpleObject_WithProperties_MapsCorrectly()
    {
        // Arrange
        var kdl = """
            package "my-lib" version="1.0.0" description="A cool library"
            """;

        // Act
        var result = KdlSerializer.Deserialize<Package>(kdl);

        // Assert
        await Assert.That(result.Name).IsEqualTo("my-lib");
        await Assert.That(result.Version).IsEqualTo("1.0.0");
        await Assert.That(result.Description).IsEqualTo("A cool library");
    }

    [Test]
    public async Task DeserializeObject_WithMissingOptionalProperty_UsesDefault()
    {
        // Arrange
        var kdl = """
            package "my-lib" version="1.0.0"
            """;

        // Act
        var result = KdlSerializer.Deserialize<Package>(kdl);

        // Assert
        await Assert.That(result.Description).IsNull();
        await Assert.That(result.Name).IsEqualTo("my-lib");
        await Assert.That(result.Version).IsEqualTo("1.0.0");
    }

    #endregion

    #region Nested Children Tests

    [Test]
    public async Task DeserializeObject_WithChildren_MapsChildListCorrectly()
    {
        // Arrange
        var kdl = """
            project "my-app" version="2.0.0" {
                dependency "lodash" version="4.17.21"
                dependency "react" version="18.0.0"
            }
            """;

        // Act
        var result = KdlSerializer.Deserialize<Project>(kdl);

        // Assert
        await Assert.That(result.Name).IsEqualTo("my-app");
        await Assert.That(result.Version).IsEqualTo("2.0.0");
        await Assert.That(result.Dependencies).Count().IsEqualTo(2);
        await Assert.That(result.Dependencies[0].Package).IsEqualTo("lodash");
        await Assert.That(result.Dependencies[1].Package).IsEqualTo("react");
    }

    [Test]
    public async Task DeserializeObject_WithMultipleChildTypes_MapsEachTypeToCorrectList()
    {
        // Arrange
        var kdl = """
            project "my-app" {
                dependency "lodash" version="4.0"
                devDependency "jest" version="27.0"
                dependency "react" version="18.0"
                devDependency "typescript" version="4.0"
            }
            """;

        // Act
        var result = KdlSerializer.Deserialize<Project>(kdl);

        // Assert
        await Assert.That(result.Dependencies).Count().IsEqualTo(2);
        await Assert.That(result.DevDependencies).Count().IsEqualTo(2);
        await Assert.That(result.Dependencies[0].Package).IsEqualTo("lodash");
        await Assert.That(result.DevDependencies[0].Package).IsEqualTo("jest");
    }

    [Test]
    public async Task DeserializeObject_WithNoChildren_InitializesEmptyLists()
    {
        // Arrange
        var kdl = """
            project "standalone"
            """;

        // Act
        var result = KdlSerializer.Deserialize<Project>(kdl);

        // Assert
        await Assert.That(result.Dependencies).IsEmpty();
        await Assert.That(result.DevDependencies).IsEmpty();
    }

    #endregion

    #region Numeric Type Tests

    [Test]
    public async Task DeserializeObject_WithIntProperty_MapsCorrectly()
    {
        // Arrange
        var kdl = """
            settings timeout=5000
            """;

        // Act
        var result = KdlSerializer.Deserialize<Settings>(kdl);

        // Assert
        await Assert.That(result.Timeout).IsEqualTo(5000);
    }

    [Test]
    public async Task DeserializeObject_WithLongProperty_MapsCorrectly()
    {
        // Arrange
        var kdl = """
            settings retries=9223372036854775807
            """;

        // Act
        var result = KdlSerializer.Deserialize<Settings>(kdl);

        // Assert
        await Assert.That(result.Retries).IsEqualTo(long.MaxValue);
    }

    [Test]
    public async Task DeserializeObject_WithDoubleProperty_MapsCorrectly()
    {
        // Arrange
        var kdl = """
            settings ratio=3.14159
            """;

        // Act
        var result = KdlSerializer.Deserialize<Settings>(kdl);

        // Assert
        await Assert.That(result.Ratio).IsEqualTo(3.14159).Within(0.00001);
    }

    [Test]
    public async Task DeserializeObject_WithBoolProperty_MapsCorrectly()
    {
        // Arrange
        var kdl = """
            settings enabled=#true
            """;

        // Act
        var result = KdlSerializer.Deserialize<Settings>(kdl);

        // Assert
        await Assert.That(result.Enabled).IsTrue();
    }

    [Test]
    public async Task DeserializeObject_WithAllNumericTypes_MapsAllCorrectly()
    {
        // Arrange
        var kdl = """
            settings timeout=500 retries=10 ratio=0.95 enabled=#true
            """;

        // Act
        var result = KdlSerializer.Deserialize<Settings>(kdl);

        // Assert
        await Assert.That(result.Timeout).IsEqualTo(500);
        await Assert.That(result.Retries).IsEqualTo(10);
        await Assert.That(result.Ratio).IsEqualTo(0.95).Within(0.0001);
        await Assert.That(result.Enabled).IsTrue();
    }

    #endregion

    #region Type Annotation Tests

    [Test]
    public async Task DeserializeObject_WithGuidProperty_ParsesUuidTypeAnnotation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var kdl = $"""
            user "alice" id=(uuid)"{id:D}"
            """;

        // Act
        var result = KdlSerializer.Deserialize<User>(kdl);

        // Assert
        await Assert.That(result.Username).IsEqualTo("alice");
        await Assert.That(result.Id).IsEqualTo(id);
    }

    [Test]
    public async Task DeserializeObject_WithDateTimeProperty_ParsesDateTimeTypeAnnotation()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var kdl = $"""
            user "bob" createdAt=(date-time)"{now:O}"
            """;

        // Act
        var result = KdlSerializer.Deserialize<User>(kdl);

        // Assert
        await Assert.That(result.CreatedAt).IsEqualTo(now);
    }

    #endregion

    #region Serialization Tests

    [Test]
    public async Task SerializeObject_WithSimpleProperties_GeneratesValidKdl()
    {
        // Arrange
        var obj = new Package
        {
            Name = "my-lib",
            Version = "1.0.0",
            Description = "A library",
        };

        // Act
        var kdl = KdlSerializer.Serialize(obj);

        // Assert
        await Assert.That(kdl).Contains("package");
        await Assert.That(kdl).Contains("my-lib");
        await Assert.That(kdl).Contains("version");
        await Assert.That(kdl).Contains("1.0.0");
    }

    [Test]
    public async Task SerializeObject_WithNestedChildren_GeneratesValidKdl()
    {
        // Arrange
        var obj = new Project
        {
            Name = "my-app",
            Version = "1.0.0",
            Dependencies =
            [
                new Dependency { Package = "lodash", Version = "4.17" },
                new Dependency { Package = "react", Version = "18.0" },
            ],
        };

        // Act
        var kdl = KdlSerializer.Serialize(obj);

        // Assert
        await Assert.That(kdl).Contains("project");
        await Assert.That(kdl).Contains("my-app");
        await Assert.That(kdl).Contains("dependency");
        await Assert.That(kdl).Contains("lodash");
        await Assert.That(kdl).Contains("react");
    }

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

    #region Edge Cases

    [Test]
    public async Task DeserializeObject_WithEmptyString_MapsCorrectly()
    {
        // Arrange
        var kdl = """
            package "" version=""
            """;

        // Act
        var result = KdlSerializer.Deserialize<Package>(kdl);

        // Assert
        await Assert.That(result.Name).IsEqualTo("");
        await Assert.That(result.Version).IsEqualTo("");
    }

    [Test]
    public async Task DeserializeObject_WithSpecialCharactersInString_MapsCorrectly()
    {
        // Arrange
        var kdl = """
            package "my-lib@1.0" description="Unicode: 你好世界 🚀"
            """;

        // Act
        var result = KdlSerializer.Deserialize<Package>(kdl);

        // Assert
        await Assert.That(result.Name).IsEqualTo("my-lib@1.0");
        await Assert.That(result.Description).Contains("世界");
    }

    [Test]
    public async Task DeserializeObject_WithNegativeNumbers_MapsCorrectly()
    {
        // Arrange
        var kdl = """
            settings timeout=-1000 ratio=-3.14
            """;

        // Act
        var result = KdlSerializer.Deserialize<Settings>(kdl);

        // Assert
        await Assert.That(result.Timeout).IsEqualTo(-1000);
        await Assert.That(result.Ratio).IsEqualTo(-3.14).Within(0.0001);
    }

    [Test]
    public async Task DeserializeObject_WithHexNumbers_MapsCorrectly()
    {
        // Arrange
        var kdl = """
            settings timeout=0xFF retries=0x10
            """;

        // Act
        var result = KdlSerializer.Deserialize<Settings>(kdl);

        // Assert
        await Assert.That(result.Timeout).IsEqualTo(255);
        await Assert.That(result.Retries).IsEqualTo(16);
    }

    #endregion

    #region Error Handling Tests

    [Test]
    public async Task DeserializeObject_WithWrongNodeName_ThrowsException()
    {
        // Arrange
        var kdl = """
            application "wrong-node"
            """;

        // Act & Assert
        await Assert
            .That(async () => KdlSerializer.Deserialize<Package>(kdl))
            .Throws<KuddleSerializationException>();
    }

    [Test]
    public async Task DeserializeToDictionary_MapsCorrectly()
    {
        // Arrange
        var kdl = """
// 1. The "themes" dictionary (Key = Theme Name)
themes {
    // Key: "dark-mode" -> Value: Theme (which is also a Dictionary)
    dark-mode {
        // Key: "window" -> Value: ElementStyle
        window {
            align v="Top" h="Left"
            border color="#FFFFFF" style="solid"
        }

        // Key: "button" -> Value: ElementStyle
        button {
            header text="Click Me"
            align v="Middle" h="Center"
        }
    }

    // Key: "high-contrast"
    high-contrast {
        window {
            border color="#FFFF00"
        }
    }
}

// 2. The "layouts" dictionary (Key = Layout Name)
layouts {
    // Key: "dashboard" -> Value: LayoutDefinition
    dashboard section="main-view" size=1 split="rows" {
        
        // Recursive List<LayoutDefinition> (Children)
        child section="top-bar" size=1
        
        child section="content-area" size=4 split="columns" {
            child section="sidebar" size=1
            child section="grid" size=3
        }
    }
}
""";

        // Act
        var result = KdlSerializer.Deserialize<AppSettings>(kdl);

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

    // [Test]
    // public async Task DeserializeObject_WithMissingRequiredArgument_ThrowsException()
    // {
    //     // Arrange
    //     var kdl = """
    //         package
    //         """;

    //     // Act & Assert
    //     await Assert
    //         .That(async () => KdlSerializer.Deserialize<Package>(kdl))
    //         .Throws<KuddleSerializationException>();
    // }

    [Test]
    public async Task RoundTrip_HybridType_PreservesPropertiesAndDictionary()
    {
        var model = new HybridModel { Version = "2.5" };
        model["timeout"] = "5000";
        model["retry"] = "true";

        var kdl = KdlSerializer.Serialize(model);

        // Should look like:
        // hybridmodel version="2.5" {
        //   timeout "5000"
        //   retry "true"
        // }

        var deserialized = KdlSerializer.Deserialize<HybridModel>(kdl);

        await Assert.That(deserialized.Version).IsEqualTo("2.5");
        await Assert.That(deserialized["timeout"]).IsEqualTo("5000");
        await Assert.That(deserialized.Count).IsEqualTo(2);
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

    [Test]
    public async Task Serialize_PropertyDictionary_WritesFlatProperties()
    {
        // Arrange
        var model = new PropertyDictModel();
        model.Tags["env"] = "production";
        model.Tags["region"] = "us-east-1";

        // Using a prefix "setting"
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
        Debug.WriteLine(kdl);
        // Assert Wrapped: plugins { plugininfo "Auth" }
        await Assert.That(kdl).Contains("plugins {");

        // Assert Flattened: server host="localhost"
        await Assert.That(kdl).Contains("server host=localhost");
        await Assert.That(kdl).Contains("server host=\"127.0.0.1\"");

        // Double check there isn't a wrapper node for servers
        // (Assuming the class property was named FlattenedServers)
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

        // Act
        var result = KdlSerializer.Deserialize<CollectionModel>(kdl);

        // Assert
        await Assert.That(result.WrappedPlugins).Count().IsEqualTo(1);
        await Assert.That(result.FlattenedServers).Count().IsEqualTo(2);
        await Assert.That(result.FlattenedServers[1].Host).IsEqualTo("remote");
    }

    #endregion

    #region Test Models
    public class HybridModel : Dictionary<string, string>
    {
        [KdlProperty("version")]
        public string Version { get; set; } = "1.0";
    }

    /// <summary>Simple class with properties and arguments.</summary>
    public class Package
    {
        [KdlArgument(0)]
        public string Name { get; set; } = string.Empty;

        [KdlProperty("version")]
        public string? Version { get; set; }

        [KdlProperty("description")]
        public string? Description { get; set; }
    }

    /// <summary>Class with nested children (list of child nodes).</summary>
    public class Project
    {
        [KdlArgument(0)]
        public string Name { get; set; } = string.Empty;

        [KdlProperty("version")]
        public string Version { get; set; } = "1.0.0";

        [KdlNode("dependency", Flatten = true)]
        public List<Dependency> Dependencies { get; set; } = [];

        [KdlNode("devDependency", Flatten = true)]
        public List<Dependency> DevDependencies { get; set; } = [];
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

    /// <summary>Class with numeric properties.</summary>
    public class Settings
    {
        [KdlProperty("timeout")]
        public int Timeout { get; set; }

        [KdlProperty("retries")]
        public long Retries { get; set; }

        [KdlProperty("ratio")]
        public double Ratio { get; set; }

        [KdlProperty("enabled")]
        public bool Enabled { get; set; }
    }

    /// <summary>Class with type annotations (uuid, date-time).</summary>
    public class User
    {
        [KdlArgument(0)]
        public string Username { get; set; } = string.Empty;

        [KdlProperty("id")]
        public Guid Id { get; set; }

        [KdlProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }
    }

    /// <summary>Class demonstrating polymorphism with type discriminators.</summary>
    public class Resource
    {
        [KdlArgument(0)]
        public string Name { get; set; } = string.Empty;

        [KdlProperty("type")]
        public string Type { get; set; } = string.Empty; // Used as discriminator
    }

    public class FileResource : Resource
    {
        [KdlProperty("path")]
        public string Path { get; set; } = string.Empty;
    }

    public class UrlResource : Resource
    {
        [KdlProperty("url")]
        public string Url { get; set; } = string.Empty;
    }

    #endregion
}

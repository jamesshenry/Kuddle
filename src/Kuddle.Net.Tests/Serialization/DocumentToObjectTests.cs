using Kuddle.Exceptions;
using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization;

public class DocumentToObjectTests
{
    class AppConfig
    {
        [KdlNode("plugin")]
        public List<Plugin> Plugins { get; set; } = new();

        [KdlNode("logging")]
        public LogSettings? Logging { get; set; }

        [KdlNode("experiments")]
        public Experiments? Experiments { get; set; }
    }

    class Plugin
    {
        [KdlArgument(0)]
        public string Name { get; set; } = "";
    }

    class LogSettings
    {
        [KdlNode("level")]
        public string LogLevel { get; set; } = "info";
    }

    class Experiments
    {
        [KdlProperty("enabled")]
        public bool Enabled { get; set; }
    }

    // --- Tests ---

    [Test]
    public async Task Deserialize_Document_MapsListsAndSingleObjects()
    {
        var kdl = """
            plugin "Analytics"
            plugin "Authentication"

            logging {
                level "debug"
            }
            """;

        var result = KdlSerializer.Deserialize<AppConfig>(kdl);

        // Assert
        await Assert.That(result.Plugins).Count().IsEqualTo(2);
        await Assert.That(result.Plugins[0].Name).IsEqualTo("Analytics");
        await Assert.That(result.Plugins[1].Name).IsEqualTo("Authentication");

        await Assert.That(result.Logging).IsNotNull();

        await Assert.That(result.Logging!.LogLevel).IsEqualTo("debug");
    }

    [Test]
    public async Task Deserialize_EmptyDocument_ReturnsInitializedObject()
    {
        var kdl = "";

        var result = KdlSerializer.Deserialize<AppConfig>(kdl);

        await Assert.That(result).IsNotNull();
        await Assert.That(result.Plugins).IsEmpty();
        await Assert.That(result.Logging).IsNull();
    }

    [Test]
    public async Task Deserialize_PartialMatch_IgnoresUnmappedNodes()
    {
        var kdl = """
            plugin "Core"

            // This node is not in AppConfig
            garbage_data {
                ignore me
            }
            """;

        var result = KdlSerializer.Deserialize<AppConfig>(kdl);

        await Assert.That(result.Plugins).Count().IsEqualTo(1);
        await Assert.That(result.Plugins[0].Name).IsEqualTo("Core");

        await Assert.That(result.Logging).IsNull();
    }

    [Test]
    public async Task Deserialize_NestedStructure_WithProperties()
    {
        var kdl =
            @"
            experiments enabled=#true
        ";

        var result = KdlSerializer.Deserialize<AppConfig>(kdl);

        await Assert.That(result.Experiments).IsNotNull();
        await Assert.That(result.Experiments!.Enabled).IsTrue();
    }

    [Test]
    public async Task Deserialize_AmbiguousSingleNode_ThrowsOrHandles()
    {
        var kdl =
            @"
            logging { level ""info"" }
            logging { level ""error"" }
        ";
        var config = KdlSerializer.Deserialize<AppConfig>(kdl);

        await Assert.That(config.Logging!.LogLevel).IsEqualTo("error");
    }
}

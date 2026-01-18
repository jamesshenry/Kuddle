using Kuddle.Serialization;

namespace Kuddle.Tests.Conversion;

/// <summary>
/// Tests for nested object mapping: children, hierarchies, and complex structures.
/// </summary>
public class NestedObjectTests
{
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

    #endregion

    #region Test Models

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

    #endregion
}

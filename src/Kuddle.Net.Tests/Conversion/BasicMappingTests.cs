using Kuddle.Serialization;

namespace Kuddle.Tests.Conversion;

/// <summary>
/// Tests for basic object mapping: simple properties, arguments, and type conversions.
/// </summary>
public class BasicMappingTests
{
    #region Deserialization Tests

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

    public class User
    {
        [KdlArgument(0)]
        public string Username { get; set; } = string.Empty;

        [KdlProperty("id")]
        public Guid Id { get; set; }

        [KdlProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }
    }

    #endregion
}

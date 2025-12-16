using Kuddle.Exceptions;
using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization;

public class NodeToObjectTests
{
    // 1. Explicit Naming via [KdlType]
    [KdlType("database")]
    public class DbConfig
    {
        [KdlArgument(0)]
        public string Name { get; set; } = "";

        [KdlProperty("port")]
        public int Port { get; set; }

        [KdlProperty("enabled")]
        public bool Enabled { get; set; } = true; // Default value
    }

    // 2. Implicit Naming (Class Name fallback)
    public class Server
    {
        [KdlArgument(0)]
        public string Host { get; set; } = "";
    }

    [Test]
    public async Task Deserialize_ExplicitName_MapsArgumentsAndProperties()
    {
        var kdl = "database production port=5432 enabled=#false";

        var result = KdlSerializer.Deserialize<DbConfig>(kdl);

        await Assert.That(result).IsNotNull();
        await Assert.That(result.Name).IsEqualTo("production");
        await Assert.That(result.Port).IsEqualTo(5432);
        await Assert.That(result.Enabled).IsFalse();
    }

    [Test]
    public async Task Deserialize_ImplicitName_UsesClassName()
    {
        // Class is "Server", so it expects node "server" (case-insensitive usually, or exact match)
        // Assuming your logic defaults to exact or lowercase.
        // Let's assume case-insensitive or exact match "Server".
        // If your logic uses .ToLowerInvariant(), input should be "server".
        var kdl = "Server \"localhost\"";

        var result = KdlSerializer.Deserialize<Server>(kdl);

        await Assert.That(result).IsNotNull();
        await Assert.That(result.Host).IsEqualTo("localhost");
    }

    [Test]
    public async Task Deserialize_MissingProperty_UsesDefault()
    {
        // "enabled" is missing, should stay true
        var kdl = "database \"local\" port=3000";

        var result = KdlSerializer.Deserialize<DbConfig>(kdl);

        await Assert.That(result.Name).IsEqualTo("local");
        await Assert.That(result.Enabled).IsTrue();
    }

    [Test]
    public async Task Deserialize_MismatchNodeName_Throws()
    {
        // Expecting "database", got "table"
        var kdl = "table \"production\" port=5432";

        // This asserts that the Serializer enforces the [KdlType] name
        await Assert.ThrowsAsync<KuddleSerializationException>(async () =>
        {
            KdlSerializer.Deserialize<DbConfig>(kdl);
        });
    }

    [Test]
    public async Task Deserialize_MultipleRootNodes_Throws()
    {
        // Node-to-Object strategy implies the Type represents A SINGLE node.
        // A document with two nodes is ambiguous/invalid for this mapping.
        var kdl =
            @"
            database ""primary"" port=5432
            database ""replica"" port=5433
        ";

        await Assert.ThrowsAsync<KuddleSerializationException>(async () =>
        {
            KdlSerializer.Deserialize<DbConfig>(kdl);
        });
    }

    [Test]
    public async Task Deserialize_TypeMismatch_Throws()
    {
        // Port expects int, got string identifier
        var kdl = "database \"db\" port=\"not-a-number\"";

        await Assert.ThrowsAsync<KuddleSerializationException>(async () =>
        {
            KdlSerializer.Deserialize<DbConfig>(kdl);
        });
    }

    [Test]
    public async Task Deserialize_DocumentRoot_RejectsProperties()
    {
        // If the serializer logic correctly identifies this as a Node
        // (because it has [KdlArgument]), it handles it.
        // But if we tried to put a property at the top level in the FILE that doesn't
        // belong to a node (which is syntactically impossible in KDL anyway), the Parser would catch it.

        // However, we can test that extraneous properties on the node are simply ignored
        // (unless you implemented strict mode).
        var kdl = "database \"db\" port=5432 unknown_prop=123";

        // Should succeed and ignore unknown_prop
        var result = KdlSerializer.Deserialize<DbConfig>(kdl);
        await Assert.That(result.Port).IsEqualTo(5432);
    }
}

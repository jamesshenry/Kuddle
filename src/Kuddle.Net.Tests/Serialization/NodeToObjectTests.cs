using System.Diagnostics;
using Kuddle.Exceptions;
using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization;

public class NodeToObjectTests
{
    [KdlType("database")]
    class DbConfig
    {
        [KdlArgument(0)]
        public string Name { get; set; } = "";

        [KdlProperty("port")]
        public int Port { get; set; }

        [KdlProperty("enabled")]
        public bool Enabled { get; set; } = true;
    }

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
        var kdl = "Server \"localhost\"";

        var result = KdlSerializer.Deserialize<Server>(kdl);

        await Assert.That(result).IsNotNull();
        await Assert.That(result.Host).IsEqualTo("localhost");
    }

    [Test]
    public async Task Deserialize_MissingProperty_UsesDefault()
    {
        var kdl = "database \"local\" port=3000";

        var result = KdlSerializer.Deserialize<DbConfig>(kdl);

        await Assert.That(result.Name).IsEqualTo("local");
        await Assert.That(result.Enabled).IsTrue();
    }

    [Test]
    public async Task Deserialize_MultipleRootNodes_Throws()
    {
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
        var kdl = "database \"db\" port=\"not-a-number\"";

        await Assert.ThrowsAsync<KuddleSerializationException>(async () =>
        {
            KdlSerializer.Deserialize<DbConfig>(kdl);
        });
    }

    [Test]
    public async Task Deserialize_DocumentRoot_RejectsProperties()
    {
        var kdl = "database \"db\" port=5432 unknown_prop=123";

        var result = KdlSerializer.Deserialize<DbConfig>(kdl);
        await Assert.That(result.Port).IsEqualTo(5432);
    }
}

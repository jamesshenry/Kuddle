using System.Diagnostics;
using Kuddle.Serialization;
using Microsoft.Extensions.Configuration;

namespace Kuddle.Extensions.Configuration.Tests;

public class ConfigurationTests
{
    [Test]
    public async Task AddKdlFile_ShouldNotThrow()
    {
        await Assert
            .That(() => new ConfigurationBuilder().AddKdlFile("test.kdl").Build())
            .ThrowsNothing();
    }

    [Test]
    public async Task ReflectionBased_ConfigurationBind_ShouldBindToDeserializedKdl()
    {
        var sample = new Sample();
        var configuration = new ConfigurationBuilder().AddKdlFile("sample.kdl").Build();
        configuration.Bind(sample);
        var expected = new Sample
        {
            Title = "TOML Example",
            Owner = new Owner { Name = "Tom Preston-Werner", DoB = new DateTime(1979, 05, 27) },
            Database = new Database
            {
                Enabled = true,
                Ports = [8000, 8001, 8002],
                Temp_Targets = new Dictionary<string, decimal> { ["cpu"] = 79.5m, ["case"] = 72m },
            },
            Servers = new Dictionary<string, Server>
            {
                ["alpha"] = new() { Ip = "10.0.0.1", Role = Role.Frontend },
                ["beta"] = new() { Ip = "10.0.0.2", Role = Role.Backend },
            },
        };

        var serialized = KdlSerializer.Serialize(expected);

        Debug.WriteLine(serialized);

        var retrieved = KdlSerializer.Deserialize<Sample>(serialized);

        await Assert.That(serialized).IsNotNull();
    }
}

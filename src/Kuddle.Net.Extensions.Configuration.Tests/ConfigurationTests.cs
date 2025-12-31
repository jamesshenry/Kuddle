using System.Diagnostics;
using Kuddle.Serialization;
using Microsoft.Extensions.Configuration;

namespace Kuddle.Extensions.Configuration.Tests;

public class ConfigurationTests
{
    [Test]
    public async Task AddKdlFile_ShouldNotThrow()
    {
        var serialized = KdlSerializer.Serialize(new Sample());
        File.WriteAllText("sample.kdl", serialized);

        await Assert
            .That(() => new ConfigurationBuilder().AddKdlFile("sample.kdl").Build())
            .ThrowsNothing();
    }

    [Test]
    public async Task ReflectionBased_ConfigurationBind_ShouldBindToDeserializedKdl()
    {
        var result = new Sample();
        var expected = new Sample
        {
            Title = "kdl example",
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

        // 1. Serialize the object
        var serialized = KdlSerializer.Serialize(
            expected,
            new KdlSerializerOptions { UnwrapRoot = true }
        );

        // 2. YOU MUST WRITE THE FILE TO DISK
        File.WriteAllText("sample.kdl", serialized);

        var configuration = new ConfigurationBuilder().AddKdlFile("sample.kdl").Build();
        configuration.Bind(result);

        await Assert.That(result.Title).IsEqualTo(expected.Title);
        await Assert.That(result.Database.Ports).Count().IsEqualTo(3);
        await Assert.That(result.Database.Ports[0]).IsEqualTo((ushort)8000);
        await Assert.That(result.Servers["alpha"].Ip).IsEqualTo("10.0.0.1");
    }

    [Test]
    public async Task Configuration_ShouldFlattenHierarchy()
    {
        var kdl = """
            server {
                host "localhost"
                port 8080
                ssl #true
            }
            """;
        File.WriteAllText("appsettings.kdl", kdl);

        var config = new ConfigurationBuilder().AddKdlFile("appsettings.kdl").Build();

        await Assert.That(config["server:host"]).IsEqualTo("localhost");
        await Assert.That(config["server:port"]).IsEqualTo("8080");
        await Assert.That(config["server:ssl"]).IsEqualTo("true");
    }

    [Test]
    public async Task Configuration_ShouldHandleDuplicateNodesAsArrays()
    {
        var kdl = """
            user name="Alice"
            user name="Bob"
            """;
        File.WriteAllText("users.kdl", kdl);

        var config = new ConfigurationBuilder().AddKdlFile("users.kdl").Build();

        await Assert.That(config["user:0:name"]).IsEqualTo("Alice");
        await Assert.That(config["user:1:name"]).IsEqualTo("Bob");
    }

    [Test]
    public async Task Configuration_ShouldHandlePositionalArguments()
    {
        var kdl = """
            endpoints "10.0.0.1" "10.0.0.2" "10.0.0.3"
            """;
        File.WriteAllText("network.kdl", kdl);

        var config = new ConfigurationBuilder().AddKdlFile("network.kdl").Build();

        await Assert.That(config["endpoints:0"]).IsEqualTo("10.0.0.1");
        await Assert.That(config["endpoints:1"]).IsEqualTo("10.0.0.2");
    }

    [Test]
    public async Task Configuration_ShouldOverrideValuesFromSubsequentFiles()
    {
        File.WriteAllText("base.kdl", "logging level=\"info\"");
        File.WriteAllText("override.kdl", "logging level=\"debug\"");

        var config = new ConfigurationBuilder()
            .AddKdlFile("base.kdl")
            .AddKdlFile("override.kdl")
            .Build();

        await Assert.That(config["logging:level"]).IsEqualTo("debug");
    }

    [Test]
    public async Task OptionalFile_Missing_ShouldNotThrow()
    {
        await Assert
            .That(() =>
                new ConfigurationBuilder().AddKdlFile("missing.kdl", optional: true).Build()
            )
            .ThrowsNothing();
    }

    [Test]
    public async Task RequiredFile_Missing_ShouldThrow()
    {
        // The base Microsoft.Extensions.Configuration.FileExtensions handles this,
        // but it's good to ensure your provider doesn't swallow the error.
        await Assert
            .That(() =>
                new ConfigurationBuilder().AddKdlFile("missing.kdl", optional: false).Build()
            )
            .Throws<FileNotFoundException>();
    }

    [Test]
    public async Task Configuration_ShouldConvertKdlTypesToStrings()
    {
        var kdl = """
            flags {
                enabled #true
                disabled #false
                missing #null
            }
            """;
        File.WriteAllText("flags.kdl", kdl);

        var config = new ConfigurationBuilder().AddKdlFile("flags.kdl").Build();

        await Assert.That(config["flags:enabled"]).IsEqualTo("true");
        await Assert.That(config["flags:disabled"]).IsEqualTo("false");
        await Assert.That(config["flags:missing"]).IsNull();
    }
}

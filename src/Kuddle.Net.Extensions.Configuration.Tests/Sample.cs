using Kuddle.Serialization;

namespace Kuddle.Extensions.Configuration.Tests;

public class Sample
{
    public string Title { get; init; } = "";
    public Owner Owner { get; init; } = new();
    public Database Database { get; init; } = new();
    public Dictionary<string, Server> Servers { get; init; } = [];
}

public class Owner
{
    public string Name { get; init; } = "";

    [KdlProperty("dob")]
    public DateTimeOffset DoB { get; init; }
}

public class Database
{
    public bool Enabled { get; init; }
    public ushort[] Ports { get; init; } = [];
    public Dictionary<string, decimal> Temp_Targets { get; init; } = [];
}

public class Server
{
    public string Ip { get; init; } = "";
    public Role Role { get; init; }
}

public enum Role
{
    Unknown,
    Frontend,
    Backend,
}

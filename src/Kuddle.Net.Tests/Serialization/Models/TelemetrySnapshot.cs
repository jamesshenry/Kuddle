using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization.Models;

public class TelemetrySnapshot
{
    // [KdlNode("id")]
    public Guid SnapshotId { get; set; }
    public DateTimeOffset CapturedAt { get; set; }

    // Dictionary with complex values
    // [KdlNodeDictionary("services")]
    public Dictionary<string, ServiceInfo> Services { get; set; } = new();

    // Dictionary of dictionaries
    // [KdlNodeDictionary("tags")]
    public Dictionary<string, Dictionary<string, string>> GlobalTags { get; set; } = new();

    // [KdlNode("env")]
    public EnvironmentInfo Environment { get; set; } = new();

    [KdlNodeCollection("events", "event")]
    public List<EventRecord> Events { get; set; } = new();

    // Arbitrary metadata bucket
    public Dictionary<string, object?> Metadata { get; set; } = new();
}

// [KdlType("info")]
public class ServiceInfo
{
    // [KdlNode]
    public string Name { get; set; } = string.Empty;

    // [KdlNode]
    public ServiceStatus Status { get; set; }

    // [KdlNode]
    public VersionInfo Version { get; set; } = new();

    // Dictionary with primitive values
    public Dictionary<string, double> Metrics { get; set; } = new();

    // Dictionary with list values
    public Dictionary<string, List<DependencyInfo>> Dependencies { get; set; } = new();

    public List<EndpointInfo> Endpoints { get; set; } = new();
}

public class VersionInfo
{
    public string VersionString { get; set; } = string.Empty;
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }

    public DateTime? BuildDate { get; set; }
}

public class DependencyInfo
{
    public string DependencyName { get; set; } = string.Empty;
    public DependencyType Type { get; set; }

    // Nullable to test optional fields
    public TimeSpan? Latency { get; set; }

    public Dictionary<string, string> Properties { get; set; } = new();
}

public class EndpointInfo
{
    public string Route { get; set; } = string.Empty;
    public HttpMethod Method { get; set; }

    public bool RequiresAuth { get; set; }

    // Dictionary keyed by status code
    public Dictionary<int, ResponseProfile> ResponseProfiles { get; set; } = new();
}

public class ResponseProfile
{
    public int StatusCode { get; set; }
    public string Description { get; set; } = string.Empty;

    public Dictionary<string, string> Headers { get; set; } = new();

    // Nested complex object
    public PayloadSchema? Payload { get; set; }
}

public class PayloadSchema
{
    public string ContentType { get; set; } = string.Empty;

    // Dictionary representing schema-like data
    public Dictionary<string, FieldDefinition> Fields { get; set; } = new();
}

public class FieldDefinition
{
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; }

    // Recursive-ish structure
    public Dictionary<string, FieldDefinition>? SubFields { get; set; }
}

public class EventRecord
{
    // [KdlProperty]
    public Guid EventId { get; set; }

    // [KdlProperty]
    public DateTimeOffset Timestamp { get; set; }

    // [KdlProperty]
    public EventSeverity Severity { get; set; }

    // [KdlProperty]
    public string Message { get; set; } = string.Empty;

    // Heterogeneous dictionary
    // [KdlNode]
    public Dictionary<string, object?>? Context { get; set; }
}

[KdlType("env-info")]
public class EnvironmentInfo
{
    // [KdlProperty]
    public string Name { get; set; } = string.Empty;

    // [KdlProperty]
    public string Region { get; set; } = string.Empty;

    // Dictionary keyed by machine name
    // [KdlNode]
    public Dictionary<string, MachineInfo> Machines { get; set; } = new();
}

public class MachineInfo
{
    public string Os { get; set; } = string.Empty;
    public int CpuCores { get; set; }
    public long MemoryBytes { get; set; }

    public Dictionary<string, string> Labels { get; set; } = new();
}

public enum ServiceStatus
{
    Unknown,
    Healthy,
    Degraded,
    Unavailable,
}

public enum DependencyType
{
    Database,
    HttpService,
    MessageQueue,
    Cache,
}

public enum EventSeverity
{
    Trace,
    Info,
    Warning,
    Error,
    Critical,
}

public enum HttpMethod
{
    Get,
    Post,
    Put,
    Delete,
    Patch,
}

# Kuddle.Net

Kuddle.Net is a .NET implementation of a [KDL](https://kdl.dev) parser/serializer targeting [v2](https://kdl.dev/spec/) of the spec.
KDL is a concise, human-readable language built for configuration and data exchange. Head to <https://kdl.dev> for more specifics on the KDL document language itself.

## Installation

```text
dotnet add package Kuddle.Net
```

## Quick Start: Serialization & Deserialization

For most use cases, `KdlSerializer` provides the easiest way to work with KDL data by mapping it directly to C# classes.

### Deserializing KDL to Objects

```csharp
using Kuddle.Serialization;

var kdl = """
    server "production" {
        host "10.0.0.1"
        port 8080
    }
    """;

// Deserialize a single root node
var config = KdlSerializer.Deserialize<ServerConfig>(kdl);
```

### Serializing Objects to KDL

```csharp
var myConfig = new ServerConfig { Host = "localhost", Port = 3000 };
string kdl = KdlSerializer.Serialize(myConfig);
```

### Document-Level Deserialization

If your KDL file contains multiple top-level nodes of the same type, use `DeserializeMany`:

```csharp
var kdl = """
    user "alice" role="admin"
    user "bob" role="user"
    """;

var users = KdlSerializer.DeserializeMany<User>(kdl);
```

---

## Mapping with Attributes

To control how C# properties map to KDL arguments, properties, and child nodes, use the provided attributes.

| Attribute              | Target   | Purpose                                       |
| ---------------------- | -------- | --------------------------------------------- |
| `[KdlArgument(index)]` | Property | Maps to a positional argument                 |
| `[KdlProperty(key)]`   | Property | Maps to a `key="value"` property              |
| `[KdlNode(name)]`      | Property | Maps to a child node (or collection of nodes) |
| `[KdlType(name)]`      | Class    | Overrides the default node name               |

**[Detailed Attribute Documentation](docs/serialization-attributes.md)**

---

## Advanced Usage

### Lower-Level AST Access

If you need full control over the KDL structure, you can use `KdlReader` to get a `KdlDocument` AST.

```csharp
KdlDocument doc = KdlReader.Read(kdlString);
```

**[Lower-Level API Documentation](docs/low-level-api.md)**

---

## License

Kuddle.Net is licensed under the MIT License.

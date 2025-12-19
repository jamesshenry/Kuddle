# Attribute Usage

This document describes how to use Kuddle's serialization attributes to map between KDL documents and C# types.

## Quick Reference

| Attribute              | Target   | Purpose                         |
| ---------------------- | -------- | ------------------------------- |
| `[KdlType]`            | Class    | Override the expected node name |
| `[KdlArgument(index)]` | Property | Map to a positional argument    |
| `[KdlProperty(key?)]`  | Property | Map to a named property         |
| `[KdlNode(name?)]`     | Property | Map to child nodes              |
| `[KdlIgnore]`          | Property | Exclude from serialization      |

---

## Node Name Matching

### Default Convention

By default, the class name (lowercased) must match the KDL node name:

```csharp
// Matches: package "my-lib"
public class Package { ... }
```

### Custom Node Name with `[KdlType]`

Override the expected node name:

```csharp
// Matches: db "primary"  (not "database")
[KdlType("db")]
public class Database { ... }
```

---

## `[KdlArgument(index)]` — Positional Arguments

Maps a property to a **positional argument** by zero-based index.

### KDL

```kdl
point 10 20 30 label="origin"
```

### C# Model

```csharp
public class Point
{
    [KdlArgument(0)]
    public int X { get; set; }
    
    [KdlArgument(1)]
    public int Y { get; set; }
    
    [KdlArgument(2)]
    public int Z { get; set; }
    
    [KdlProperty("label")]
    public string? Label { get; set; }
}
```

### Rules

1. **Index is required** — Each `[KdlArgument]` must specify its position
2. **Indices should be contiguous** — Gaps (0, 2 without 1) may cause errors
3. **Order matters** — During serialization, arguments are written in index order
4. **Scalar types only** — Arguments cannot be complex objects

### Supported Types

- `string`
- `int`, `long`, `double`, `decimal`
- `bool`
- `Guid` (with `(uuid)` type annotation)
- `DateTimeOffset` (with `(date-time)` type annotation)
- Nullable variants of all above

---

## `[KdlProperty(key?)]` — Named Properties

Maps a property to a **KDL property** (key=value pair).

### KDL

```kdl
dependency lodash version="4.17.21" optional=#false
```

### C# Model

```csharp
public class Dependency
{
    [KdlArgument(0)]
    public string Package { get; set; } = "";
    
    [KdlProperty("version")]
    public string Version { get; set; } = "*";
    
    [KdlProperty("optional")]
    public bool Optional { get; set; }
}
```

### Key Name Resolution

1. **Explicit key**: `[KdlProperty("my-key")]` → matches `my-key=...`
2. **Implicit key**: `[KdlProperty]` → uses property name lowercased

```csharp
[KdlProperty]           // Matches "timeout=..."
public int Timeout { get; set; }

[KdlProperty("max-retries")]  // Matches "max-retries=..."
public int MaxRetries { get; set; }
```

### Rules

1. **Last value wins** — Per KDL spec, if `key=1 key=2`, the value is `2`
2. **Missing properties use defaults** — No error if property absent in KDL
3. **Scalar types only** — Properties cannot be complex objects

---

## `[KdlNode(name?)]` — Child Nodes

Maps a property to **child nodes** within the parent's `{ }` block.

### Basic Usage — Collection of Child Nodes

#### KDL

```kdl
project web-app version="1.0.0" {
    dependency lodash version="4.17.21"
    dependency react version="18.2.0"
    devDependency jest version="29.0.0"
}
```

#### C# Model

```csharp
public class Project
{
    [KdlArgument(0)]
    public string Name { get; set; } = "";
    
    [KdlProperty("version")]
    public string Version { get; set; } = "1.0.0";
    
    [KdlNode("dependency")]
    public List<Dependency> Dependencies { get; set; } = [];
    
    [KdlNode("devDependency")]
    public List<Dependency> DevDependencies { get; set; } = [];
}
```

### Single Complex Child

When the property type is a **non-collection complex type**, it maps to a single child node:

#### KDL

```kdl
application myapp {
    database {
        host "localhost"
        port 5432
    }
}
```

#### C# Model

```csharp
public class Application
{
    [KdlArgument(0)]
    public string Name { get; set; } = "";
    
    [KdlNode("database")]
    public DatabaseConfig? Database { get; set; }
}

public class DatabaseConfig
{
    [KdlNode("host")]   // Maps child node's Arg(0)
    public string Host { get; set; } = "";
    
    [KdlNode("port")]   // Maps child node's Arg(0)
    public int Port { get; set; }
}
```

### Scalar Child Node

When the property type is a **scalar type**, it extracts `Arg(0)` from the child node:

#### KDL

```kdl
config {
    timeout 5000
    enabled #true
}
```

#### C# Model

```csharp
public class Config
{
    [KdlNode("timeout")]
    public int Timeout { get; set; }
    
    [KdlNode("enabled")]
    public bool Enabled { get; set; }
}
```

### Node Name Resolution

1. **Explicit name**: `[KdlNode("my-items")]` → matches child nodes named `my-items`
2. **Implicit name**: `[KdlNode]` → uses property name lowercased

### Rules

1. **Collection types** → Collects all matching child nodes into the list/array
2. **Complex types** → Expects exactly one matching child node (throws if multiple)
3. **Scalar types** → Extracts `Arg(0)` from the single matching child node
4. **Supported collection types**: `List<T>`, `T[]`, `IEnumerable<T>`, `IList<T>`

---

## `[KdlIgnore]` — Exclude Properties

Excludes a property from both serialization and deserialization:

```csharp
public class User
{
    [KdlArgument(0)]
    public string Username { get; set; } = "";
    
    [KdlIgnore]
    public string ComputedDisplayName => Username.ToUpperInvariant();
    
    [KdlIgnore]
    public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
}
```

---

## Type Conversion

### Automatic Type Mapping

| C# Type             | KDL Representation                  |
| ------------------- | ----------------------------------- |
| `string`            | `"value"` or `bare-string`          |
| `int`, `long`       | `123`, `0xFF`, `0o77`, `0b1010`     |
| `double`, `decimal` | `3.14`, `1.5e-10`                   |
| `bool`              | `#true`, `#false`                   |
| `Guid`              | `(uuid)"550e8400-..."`              |
| `DateTimeOffset`    | `(date-time)"2024-01-15T10:30:00Z"` |
| Nullable `T?`       | Value or `#null`                    |

### Type Annotations

Type annotations in KDL provide parsing hints. The serializer recognizes:

- `(uuid)` → `Guid`
- `(date-time)` → `DateTimeOffset`

```kdl
user alice {
    id (uuid)"550e8400-e29b-41d4-a716-446655440000"
    createdAt (date-time)"2024-01-15T10:30:00Z"
}
```

---

## Document-Level vs Node-Level Deserialization

### Single Node → Object

When your type has `[KdlArgument]` or `[KdlProperty]` attributes, the deserializer expects **exactly one root node**:

```csharp
var kdl = "package my-lib version=\"1.0\"";
var pkg = KdlSerializer.Deserialize<Package>(kdl);
```

### Multiple Nodes → Collection

Use `DeserializeMany<T>` for documents with multiple top-level nodes:

```kdl
server web-1 host="10.0.0.1"
server web-2 host="10.0.0.2"
server api-1 host="10.0.1.1"
```

```csharp
var servers = KdlSerializer.DeserializeMany<Server>(kdl);
// Returns IEnumerable<Server> with 3 items
```

### Document as Container

For a document where each node type maps to a different property:

```kdl
name "my-project"
version "1.0.0"
author "Alice"
```

```csharp
public class Manifest
{
    [KdlNode("name")]
    public string Name { get; set; } = "";
    
    [KdlNode("version")]
    public string Version { get; set; } = "";
    
    [KdlNode("author")]
    public string Author { get; set; } = "";
}
```

---

## Complete Example

### KDL Document

```kdl
project web-app version="2.0.0" {
    dependency lodash version="4.17.21" optional=#false
    dependency react version="18.2.0" optional=#false
    
    devDependency jest version="29.0.0"
    devDependency typescript version="5.0.0"
    
    author "Alice" {
        email "alice@example.com"
        url "https://github.com/alice"
    }
    
    repository type="git" url="https://github.com/alice/web-app"
}
```

### C# Models

```csharp
public class Project
{
    [KdlArgument(0)]
    public string Name { get; set; } = "";
    
    [KdlProperty("version")]
    public string Version { get; set; } = "1.0.0";
    
    [KdlNode("dependency")]
    public List<Dependency> Dependencies { get; set; } = [];
    
    [KdlNode("devDependency")]
    public List<Dependency> DevDependencies { get; set; } = [];
    
    [KdlNode("author")]
    public Author? Author { get; set; }
    
    [KdlNode("repository")]
    public Repository? Repository { get; set; }
}

public class Dependency
{
    [KdlArgument(0)]
    public string Package { get; set; } = "";
    
    [KdlProperty("version")]
    public string Version { get; set; } = "*";
    
    [KdlProperty("optional")]
    public bool Optional { get; set; }
}

public class Author
{
    [KdlArgument(0)]
    public string Name { get; set; } = "";
    
    [KdlNode("email")]
    public string? Email { get; set; }
    
    [KdlNode("url")]
    public string? Url { get; set; }
}

public class Repository
{
    [KdlProperty("type")]
    public string Type { get; set; } = "";
    
    [KdlProperty("url")]
    public string Url { get; set; } = "";
}
```

## Limitations & Notes

1. **No dictionary support** — `Dictionary<K,V>` types are not currently supported
2. **No polymorphism** — Cannot deserialize to derived types based on discriminator
3. **Case sensitivity** — Node/property name matching is case-insensitive by default
4. **Argument gaps** — Missing argument indices will throw; ensure contiguous indices
5. **Round-trip fidelity** — Comments, formatting, and slashdash elements are not preserved

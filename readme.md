# Kuddle.Net

Kuddle.Net is a .NET implementation of a [KDL](https://kdl.dev) parser/serializer targeting [v2](https://kdl.dev/spec/) of the spec.
KDL is concise, human-readable language built for configuration and data exchange. Head to <https://kdl.dev> for more specifics on the KDL document language itself.

## Installation

```text
dotnet add package Kuddle.Net
```

## Usage

There are a few ways of using the library, the lower level `KdlReader` and `KdlWriter` classes, and the utility `KdlSerializer` class. For most use cases `KdlSerializer.Serialize<T>`/`KdlSerializer.Deserialize<T>` will be sufficient.

```cs
var dbKdl = """
database main port=5432
""";

var dbConfig = KdlSerializer.Deserialize<DbConfig>(dbKdl);
```

KDL differs from other configuration languages like
yaml or toml in that it is node-based. The top-level document can consist of a collection of nodes, not args (e.g. `#true`, `#false`, `0xAF`) or properties (`key=#false`). To adhere to this, the serialization API depends on the use of several provided attributes to facilitate mapping `KDL` constructs to user defined types:

### Attributes

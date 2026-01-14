# Lower-Level API: Reader, Writer, and AST

For scenarios where the high-level `KdlSerializer` is too restrictive, Kuddle.Net provides direct access to the KDL AST (Abstract Syntax Tree) via `KdlReader` and `KdlWriter`.

## Reading KDL (KdlReader)

`KdlReader.Read` parses a KDL string and returns a `KdlDocument`.

```csharp
using Kuddle.AST;
using Kuddle.Serialization;

string kdl = "node 1 2 key=\"val\"";
KdlDocument doc = KdlReader.Read(kdl);

foreach (KdlNode node in doc.Nodes)
{
    Console.WriteLine($"Node name: {node.Name.Value}");
}
```

### Options

`KdlReaderOptions` allows you to customize the reading process:

```csharp
var options = new KdlReaderOptions
{
    ValidateReservedTypes = true // Validates (uuid), (date-time), etc. format
};

KdlDocument doc = KdlReader.Read(kdl, options);
```

---

## Writing KDL (KdlWriter)

`KdlWriter.Write` takes a `KdlDocument` (or any `KdlObject`) and returns its KDL string representation.

```csharp
var doc = new KdlDocument();
// ... build doc ...

string kdl = KdlWriter.Write(doc);
// Or use doc.ToString() which uses default options
```

### Options

`KdlWriterOptions` controls the output formatting:

```csharp
var options = new KdlWriterOptions
{
    IndentChar = "\t",
    NewLine = "\r\n",
    SpaceAfterProp = "  ",
    EscapeUnicode = true
};

string kdl = KdlWriter.Write(doc, options);
```

---

## The KDL AST

The AST is composed of records representing KDL constructs.

### `KdlDocument`

The root of a KDL file.

- `Nodes`: `List<KdlNode>`

### `KdlNode`

A single KDL node.

- `Name`: `KdlString`
- `Entries`: `List<KdlEntry>` (Arguments or Properties)
- `Children`: `KdlBlock?` (Nested nodes)
- `TypeAnnotation`: `string?`

### `KdlEntry`

Base class for entries within a node.

- `KdlArgument`: Positional value (`KdlValue`)
- `KdlProperty`: Key-value pair (`KdlString Key`, `KdlValue Value`)

### `KdlValue`

Base class for all constants.

- `KdlString`: Represents strings. Support for varieties via `StringKind`:
  - `StringKind.Bare`: `bare-string`
  - `StringKind.Quoted`: `"quoted string"`
  - `StringKind.Raw`: `r#"raw string"#`
  - `StringKind.MultiLine`: `"""multi-line string"""`
- `KdlNumber`: Represents numeric values. Stores the `RawValue` string to preserve precision and formatting (e.g., `0xFF` vs `255`).
- `KdlBool`: `#true` or `#false`.
- `KdlNull`: `#null`.

---

## Serialization Options

When using `KdlSerializer`, you can pass `KdlSerializerOptions` to control the behavior:

```csharp
var options = new KdlSerializerOptions
{
    IgnoreNullValues = true,      // Don't write properties with null values
    CaseInsensitiveNames = true,  // Match KDL names to C# properties case-insensitively
    WriteTypeAnnotations = true   // Include (uuid), (date-time) etc. in output
};

string kdl = KdlSerializer.Serialize(myObj, options);
```

---

## Extension Methods

Kuddle.Net provides helpful extension methods in `Kuddle.Extensions` for working with the AST:

```csharp
using Kuddle.Extensions;

KdlNode node = ...;

// Get property value
KdlValue? val = node.Prop("my-key");

// Get argument by index
KdlValue? arg = node.Arg(0);

// Try to get typed values
if (node.TryGetProp<int>("port", out int port))
{
    // ...
}
```

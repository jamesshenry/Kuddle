# Kuddle.Net

- [Kuddle.Net](#kuddlenet)
  - [Quick Start](#quick-start)
  - [Mapping C# Members](#mapping-c-members)
  - [Advanced Composition](#advanced-composition)
  - [Type Annotations and Validation](#type-annotations-and-validation)
  - [Output Control and Formatting](#output-control-and-formatting)
  - [Integrations](#integrations)

Kuddle.Net is a .NET implementation of a [KDL](https://kdl.dev) parser/serializer targeting [v2](https://kdl.dev/spec/) of the spec.
KDL is a concise, human-readable language built for configuration and data exchange. Head to <https://kdl.dev> for more specifics on the KDL document language itself.

## Quick Start

Implement KDL v2 serialization in a .NET project.

### Install Kuddle.Net

Run the installation command in your project directory:

```bash
dotnet add package Kuddle.Net
```

### Define a Model

Create a class with a parameterless constructor. Kuddle.Net uses kebab-case for KDL node names by default.

```csharp
using Kuddle.Serialization;

public class Plugin
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
}
```

### Serialize and Deserialize

Use the **KdlSerializer** static class for string-based operations.

```csharp
using Kuddle.Serialization;

// Initialize data
var plugin = new Plugin { Name = "Kuddle", Version = "2.0.0" };

// 1. Convert Object to KDL String
string kdl = KdlSerializer.Serialize(plugin);
// Result: plugin name="Kuddle" version="2.0.0"

// 2. Convert KDL String back to Object
var result = KdlSerializer.Deserialize<Plugin>(kdl);
```

### Understand KDL Structure

KDL utilizes a node-based hierarchy. Use the following table to map KDL concepts to .NET types.

| KDL Concept | .NET Equivalent | Example |
| :--- | :--- | :--- |
| **Node** | Class / POCO | `server { ... }` |
| **Argument** | Positional Value | `node "value"` |
| **Property** | Key-Value Pair | `node key="value"` |
| **Children** | Nested Objects/Collections | `node { child_node }` |
| **Annotation** | Type metadata | `(uuid)"..."` |

---

## Mapping C# Members

Control how C# properties map to KDL structures using attributes.

### Configure Naming Conventions

Kuddle.Net mandates **kebab-case** for implicit names. A property `SystemSettings` maps to node or key `system-settings`.

Override naming by passing a string argument to mapping attributes:

```csharp
[KdlProperty("serial_NO")]
public string SerialNumber { get; set; }
```

### Map Entry Types

KDL nodes store data in three slots. Use attributes to assign properties to specific slots:

| Attribute | KDL Target | Mapping Logic |
| :--- | :--- | :--- |
| **[KdlProperty]** | Property | Key-value pairs: `key="value"`. |
| **[KdlArgument]** | Argument | Positional values: `node "value"`. |
| **[KdlNode]** | Child Node | Nested nodes or blocks: `node { child }`. |

**Default Inference:**

- **Scalars** (int, string, bool, DateTime): Maps to **Properties**.
- **Complex Types / Collections**: Maps to **Child Nodes**.

### Implement Positional Arguments

Specify the 0-based index for positional values.

```csharp
public record User(
    [property: KdlArgument(0)] int Id,
    [property: KdlArgument(1)] string Role
);
// Output: user 1 "admin"
```

**The "Rest" Argument Constraint:**
Map a collection to an argument to capture all remaining values.

- **Requirement:** The collection argument must possess the highest index in the class.
- **Uniqueness:** Only one collection argument is permitted per node.

### Manage Null and Boolean Values

Kuddle.Net follows KDL v2 strict type requirements for booleans and nulls.

**Null Fidelity:**
Toggle `IgnoreNullValues` in `KdlSerializerOptions`:

- **True (Default):** Omit null properties from output.
- **False:** Emit the `#null` literal.

**Boolean Explicitness:**
KDL requires `#true` or `#false`. Bare identifiers like `true` are parsed as **KdlString**, not **KdlBool**. Kuddle.Net handles this conversion automatically for `System.Boolean` types.

---

## Advanced Composition

Manage complex document hierarchies through collection strategies, and unmapped data capture.

### Configure Collection Mapping

Kuddle.Net provides two strategies for mapping `IEnumerable<T>` properties.

**Wrapped Collections (Default):**
The property name defines a parent node, and items appear as children.

```csharp
[KdlNode("items")]
public List<string> Tags { get; set; } = ["net10", "kdl"];
/* Output:
items {
    - "net10"
    - "kdl"
}
*/
```

**Flattened Collections:**
Set `Flatten = true` to omit the container node and emit items as siblings.

```csharp
[KdlNode("tag", Flatten = true)]
public List<string> Tags { get; set; } = ["net10"];
/* Output:
tag "net10"
*/
```

### Implement Member Hoisting

Flatten complex objects to merge their properties into the parent node's scope.

```csharp
public class Root {
    [KdlNode(Flatten = true)]
    public Metadata Info { get; set; }
}

public class Metadata {
    [KdlProperty] public string Author { get; set; }
}
// Result: root author="name"
```

**Constraint:** Flattening is restricted to collections and complex objects. Applying `Flatten = true` to a scalar type (e.g., `int`, `string`) throws `KdlConfigurationException`.

### Capture Unmapped Data

Use `[KdlExtensionData]` to preserve KDL elements that do not match existing class members.

**Requirements:**

- Property type must be `IDictionary<string, object>` or `IDictionary<string, KdlValue>`.
- Unmapped properties are stored as native CLR types (`string`, `double`, `bool`).
- Unmapped nodes are stored as raw `KdlNode` AST objects.

```csharp
public class Config {
    [KdlExtensionData]
    public Dictionary<string, object> CatchAll { get; set; }
}
```

*Note: Elements prefixed with the slashdash `/-` are ignored by the parser and are not captured.*

### Select Root Mapping Strategy

Set the `RootMapping` property in `KdlSerializerOptions` to define top-level structure.

| Strategy | Description | Best Use Case |
| :--- | :--- | :--- |
| **AsNode** (Default) | Maps the object to one root node. | Data exchange / Storage. |
| **AsDocument** | Maps properties to top-level nodes. | Config files (e.g., `appsettings.kdl`). |

---

## Type Annotations and Validation

Enforce type safety and data integrity using KDL v2 type annotations and reserved type validators.

### Utilize Standard Type Annotations

Kuddle.Net automatically emits and resolves reserved KDL annotations for standard .NET types.

| .NET Type | KDL Annotation | Output Example |
| :--- | :--- | :--- |
| **Guid** | `(uuid)` | `(uuid)"550e...4000"` |
| **DateTimeOffset** | `(date-time)` | `(date-time)"2023-10-05T14:48:00Z"` |
| **DateOnly** | `(date)` | `(date)"2023-10-05"` |
| **TimeOnly** | `(time)` | `(time)"14:48:00"` |
| **TimeSpan** | `(duration)` | `(duration)"PT1H30M"` |

### Enforce Numeric Precision

Specify bit-widths for numeric entries using the `TypeAnnotation` property on mapping attributes. This ensures cross-platform compatibility for integer and floating-point types.

```csharp
public class Metrics
{
    [KdlProperty(TypeAnnotation = "u8")]
    public byte Priority { get; set; }

    [KdlProperty(TypeAnnotation = "f64")]
    public double Velocity { get; set; }
}
// Output: priority=(u8)10 velocity=(f64)120.5
```

### Configure Reserved Type Validation

Enable `KdlReservedTypeValidator` to ensure values for specific identifiers (e.g., `ipv4`, `regex`, `base64`) conform to their format specifications.

**Enable/Disable Validation:**
Modify `KdlReaderOptions` before parsing. Validation is enabled by default.

```csharp
var options = new KdlReaderOptions { ValidateReservedTypes = true };
var doc = KdlReader.Read(kdlText, options);
```

**Handle Validation Failures:**
Catch `KuddleValidationException` to inspect specific failures. This exception contains an `Errors` collection referencing the failing node and a descriptive message.

```csharp
try {
    KdlReader.Read(kdlText);
} catch (KuddleValidationException ex) {
    foreach (var err in ex.Errors) Console.WriteLine(err.Message);
}
```

### Map Enums

Enums serialize as **bare strings** (unquoted identifiers).

- **Serialization:** Emits the exact member name string.
- **Deserialization:** Performs case-insensitive matching against member names.

```csharp
public enum Status { Active, Inactive }

public class Account {
    public Status State { get; set; }
}
// KDL: account state=Active
```

---

## Output Control and Formatting

Manage KDL output structure and string representation via **KdlWriterOptions** and **KdlStringStyle**.

### String Style Selection

Kuddle.Net selects string formats based on character content and configured flags.

| Style | Result | Selection Criteria |
| :--- | :--- | :--- |
| **Bare** | `name` | No spaces or reserved characters `()[]{}/\"#;=`. Cannot start with a digit. |
| **Quoted** | `"name"` | Contains spaces or reserved characters. Standard escape sequences applied. |
| **Multi-line** | `"""..."""` | Contains newlines. Requires `AllowMultiline` flag. |

### Raw String Formatting

Raw strings disable escape sequence processing. Use raw strings for Regex patterns, Windows paths, or content with heavy quotation marks.

**Delimiter Calculation:**
The writer identifies the longest consecutive sequence of `#` characters in the source string. It then wraps the string in `n+1` hashes to prevent premature termination.

**Style Flags:**

- **RawPaths:** Employs raw strings if the value contains `/` or `\`.
- **PreferRaw:** Employs raw strings if the content requires escaping (e.g., internal quotes).

### Indentation and Style Settings

Configure document appearance through the **KdlWriterOptions** record.

| Option | Values | Description |
| :--- | :--- | :--- |
| **IndentType** | `Spaces`, `Tabs` | Sets the character used for nesting. |
| **IndentSize** | `Two`, `Four` | Sets the count of spaces per level (ignored for Tabs). |
| **EscapeUnicode** | `true`, `false` | If `true`, non-ASCII characters emit as `\u{XXXX}`. |
| **NewLine** | `\n` | Internal constant. All output uses LF line endings. |

### Code Example: Formatting Configuration

Apply custom formatting by passing options to the serializer.

```csharp
var options = new KdlSerializerOptions
{
    StringStyle = KdlStringStyle.RawPaths | KdlStringStyle.AllowBare,
    Writer = new KdlWriterOptions
    {
        IndentType = KdlWriterIndentType.Spaces,
        IndentSize = KdlWriterIndentSize.Two,
        EscapeUnicode = true
    }
};

string kdl = KdlSerializer.Serialize(myObject, options);
```

## Integrations

### Microsoft.Extensions.Configuration

The `Kuddle.Net.Extensions.Configuration` package enables KDL as a configuration source.

**Installation:**

```bash
dotnet add package Kuddle.Net.Extensions.Configuration
```

**Implementation:**
Add the KDL provider to the `ConfigurationBuilder`.

```csharp
using Kuddle.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddKdlFile("appsettings.kdl", optional: false, reloadOnChange: true)
    .Build();

string connection = config["database:connection-string"];
```

### Configuration Key Mapping

KDL document structures map to flattened .NET configuration keys using the following logic:

| KDL Structure | Configuration Key | Example |
| :--- | :--- | :--- |
| **Nested Nodes** | Colon Separator | `server { port 80 }` → `server:port` |
| **Anonymous Nodes (`-`)** | Numeric Index | `- "val"` → `:0`, `:1` |
| **Node Arguments** | Numeric Index | `endpoints "a" "b"` → `endpoints:0`, `endpoints:1` |
| **Properties** | Key Name | `node key="val"` → `node:key` |

### Exception Reference

Kuddle.Net uses specific exceptions for syntax and mapping failures.

| Exception | Root Cause | Critical Properties |
| :--- | :--- | :--- |
| **KuddleParseException** | Syntax error in KDL source. | `Line`, `Column`, `Offset` |
| **KuddleSerializationException** | CLR/KDL type mismatch. | `Message` |
| **KuddleValidationException** | Reserved type format failure. | `Errors` (Collection) |
| **KdlConfigurationException** | Invalid attribute configuration. | `Message` |

### Diagnostic Coordinates

`KuddleParseException` provides exact locations for syntax correction:

- **Line:** 1-based line number.
- **Column:** 1-based column number.
- **Offset:** 0-based character position from document start.

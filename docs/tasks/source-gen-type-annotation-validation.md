# Task: Source Generator for Type Annotation Validation

## Summary

Add a Roslyn analyzer/source generator to validate that KDL type annotations are compatible with their property types at compile time.

## Problem

Currently, a user can write:

```csharp
[KdlProperty("stock", "u32")]
public int StockCount { get; set; }
```

This compiles but is semantically incorrect—`u32` implies `uint` (0 to 4,294,967,295) but `int` only holds values up to 2,147,483,647. This could cause overflow at runtime.

## Solution

Create a diagnostic analyzer that:

1. Finds properties with `[KdlArgument]`, `[KdlProperty]`, or `[KdlNode]` attributes
2. Extracts the type annotation parameter (if present)
3. Compares the implied CLR type against the property's actual type
4. Reports an error if they're incompatible

## Diagnostic

| ID | Severity | Message |
|----|----------|---------|
| `KDL001` | Error | Type annotation '{0}' ({1}) is not compatible with property type '{2}'. Consider using '{1}' or a larger compatible type. |

## Type Annotation Mappings

| Annotation | CLR Type | Compatible With |
|------------|----------|-----------------|
| `i8` | `sbyte` | `sbyte`, `short`, `int`, `long` |
| `i16` | `short` | `short`, `int`, `long` |
| `i32` | `int` | `int`, `long` |
| `i64` | `long` | `long` |
| `u8` | `byte` | `byte`, `ushort`, `uint`, `ulong`, `short`, `int`, `long` |
| `u16` | `ushort` | `ushort`, `uint`, `ulong`, `int`, `long` |
| `u32` | `uint` | `uint`, `ulong`, `long` |
| `u64` | `ulong` | `ulong` |
| `f32` | `float` | `float`, `double` |
| `f64` | `double` | `double` |
| `decimal64` | `decimal` | `decimal` |
| `decimal128` | `decimal` | `decimal` |
| `decimal` | `decimal` | `decimal` |
| `date-time` | `DateTimeOffset` | `DateTimeOffset` |
| `date` | `DateOnly` | `DateOnly` |
| `time` | `TimeOnly` | `TimeOnly` |
| `duration` | `TimeSpan` | `TimeSpan` |
| `uuid` | `Guid` | `Guid` |
| `url` | `Uri` | `Uri`, `string` |
| `base64` | `byte[]` | `byte[]` |

## Compatibility Rules

1. **Exact match** is always valid
2. **Widening conversions** are valid (e.g., `i32` → `long`)
3. **Unsigned to signed** requires larger target (e.g., `u32` → `long` is valid, `u32` → `int` is not)
4. **Unknown annotations** are allowed (no diagnostic)

## Implementation Steps

1. [ ] Create analyzer class `KdlTypeAnnotationAnalyzer`
2. [ ] Register syntax node action for `PropertyDeclarationSyntax`
3. [ ] Extract type annotation from attribute arguments
4. [ ] Get property type from semantic model
5. [ ] Check compatibility using mapping table
6. [ ] Report diagnostic if incompatible
7. [ ] Add tests for all annotation types
8. [ ] Add code fix suggestion to change property type

## Optional: Code Fix Provider

Offer a quick fix to change the property type:

```csharp
[KdlProperty("stock", "u32")]
public int StockCount { get; set; }
//     ~~~
//     Quick fix: Change type to 'uint'
```

## Files to Create

- `src/Kuddle.Net.SourceGen/Analyzers/KdlTypeAnnotationAnalyzer.cs`
- `src/Kuddle.Net.SourceGen/CodeFixes/KdlTypeAnnotationCodeFixProvider.cs` (optional)
- `src/Kuddle.Net.SourceGen.Tests/KdlTypeAnnotationAnalyzerTests.cs`

## References

- [Roslyn Analyzers Tutorial](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix)
- [CharacterSets.cs](../src/Kuddle.Net/Parser/CharacterSets.cs) - source of truth for type annotations

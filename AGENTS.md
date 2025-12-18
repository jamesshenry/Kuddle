# Agent Guidelines for Kuddle.Net

## Build Commands

- **Build**: `dotnet run .build/targets.cs build`
- **Test**: `dotnet run .build/targets.cs test`
- **Clean**: `dotnet run .build/targets.cs clean`
- **Pack**: `dotnet run .build/targets.cs pack`
- **Single test**: `dotnet test --filter "FullyQualifiedName~TestName"` (TUnit supports standard .NET test filtering)

## Code Style Guidelines

### Project Setup

- **Target Framework**: .NET 10.0
- **Language Version**: C# preview
- **Nullable**: Enabled
- **Implicit Usings**: Enabled
- **Treat Warnings as Errors**: True

### Naming Conventions

- **Types/Namespaces**: PascalCase
- **Interfaces**: IPascalCase
- **Methods/Properties/Events**: PascalCase
- **Local variables/Parameters**: camelCase
- **Private fields**: _camelCase
- **Private static fields**: s_camelCase
- **Constants**: PascalCase

### Formatting

- **Indentation**: 4 spaces
- **Braces**: Required for all control structures
- **New lines**: Before open braces for all constructs
- **Using directives**: Outside namespace
- **File-scoped namespaces**: Preferred

### Code Analysis & Formatting

- **Analyzer**: Roslynator.Analyzers, Roslynator.CodeAnalysis.Analyzers, Roslynator.Formatting.Analyzers
- **Style enforcement**: Via .editorconfig rules
- **Expression-bodied members**: Preferred for accessors, indexers, properties
- **Pattern matching**: Preferred over is/as checks
- **Null checking**: Use null propagation and coalescing operators

### Error Handling

- **Exceptions**: Throw descriptive exceptions with context
- **Null checks**: Leverage nullable reference types
- **Validation**: Use argument validation where appropriate

### Dependencies

- **Parsing**: Parlot library
- **Testing**: TUnit with Microsoft.Testing.Platform

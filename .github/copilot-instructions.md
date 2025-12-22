# Najlot.Map Project Instructions

## Project Overview
`Najlot.Map` is a high-performance object mapping library for .NET, designed to be fast, flexible, and memory-efficient. It supports both manual mapping registration and automated code generation via Roslyn source generators.

## Core Library (`Najlot.Map`)

### Key Concepts
- **IMap Interface**: The central interface for mapping operations.
- **Fluent API**: Usage follows the pattern `map.From(source).To<Destination>()`.
- **Registration**: Mappings must be registered before use.
  - `Register<TFrom, TTo>((from, to) => ...)`: Registers a specific mapping delegate.
  - `Register<T>()`: Registers all compatible mapping methods found in a class.
- **Collections**: Native support for `IEnumerable<T>` and `IAsyncEnumerable<T>` via `.From(collection).To<Destination>()`.
- **Factories**: Supports custom object creation via `RegisterFactory`.

### Attributes
- `[MapIgnoreProperty]`: Apply to properties to exclude them from mapping.
- `[MapIgnoreMethod]`: Apply to methods to exclude them from automatic registration.

### Validation
- **`Validate()`**: Checks all registered mappings to ensure all destination properties are either mapped or explicitly ignored.
- **Usage**: Call `map.Validate()` after registering all mappings, typically in a unit test or application startup.
- **Exception**: Throws `MapMissPropertiesException` if unmapped properties are found.

### Usage Example
```csharp
var map = new Map();
map.Register<Source, Destination>((src, dest) => dest.Value = src.Value);
var result = map.From(source).To<Destination>();
```

## Source Generator (`Najlot.Map.SourceGenerator`)

### Features
- **Incremental Generation**: Uses Roslyn's incremental generator API for performance.
- **Smart Caching**: Regenerates code only when necessary.
- **Zero Runtime Overhead**: Mapping logic is generated at compile time.

### Usage Pattern
1.  **Mark Class**: Apply `[Mapping]` to a `partial class`.
2.  **Define Methods**: Create partial methods for the mappings you need.
    - `partial void Map(Source from, Destination to)`
    - `partial void Map(IMap map, Source from, Destination to)` (allows nested mappings)
3.  **Custom Converters**: Define methods like `ReturnType Convert(InputType input)` to handle specific type conversions automatically.

### Example
```csharp
using Najlot.Map.Attributes;

[Mapping]
public partial class UserMapper
{
    // Basic mapping
    public partial void MapUser(UserEntity from, UserDto to);

    // Mapping with IMap for nested objects
    public partial void MapOrder(IMap map, OrderEntity from, OrderDto to);

    // Custom type converter
    public string DateToString(DateTime date) => date.ToString("O");
}
```

## Development Guidelines
- **Null Safety**: Ensure generated and manual mappings handle `null` inputs gracefully.
- **Performance**: When contributing, prioritize zero-allocation patterns where possible.
- **Testing**: Add tests in `Najlot.Map.Tests` for core logic and `Najlot.Map.SourceGenerator.Tests` for generator logic.

## Project Structure
- `src/Najlot.Map`: The core runtime library containing `IMap`, the fluent API, and attributes.
- `src/Najlot.Map.SourceGenerator`: The Roslyn source generator project.
- `src/Najlot.Map.Attributes`: (If applicable) Separate project for attributes to avoid referencing the generator at runtime.
- `tests/Najlot.Map.Tests`: Unit tests for the runtime library.
- `tests/Najlot.Map.SourceGenerator.Tests`: Integration tests for the source generator.

## Prerequisites
- .NET 10.0 SDK (tests target .NET 10.0)
- Visual Studio 2026 or compatible IDE

# GitHub Copilot Instructions for Najlot.Map

## Repository Overview

This repository contains **Najlot.Map**, a lightweight and efficient .NET mapping library that provides controlled, debuggable object-to-object mapping functionality. Unlike "magical" mapping solutions, this library emphasizes explicit control and transparency in mapping operations.

## Key Principles

- **Explicit over implicit**: All mappings are explicitly defined through methods
- **Performance-focused**: Designed for efficiency and speed
- **Debuggable**: Mappings are clear and easy to debug
- **Validation support**: Built-in validation to ensure complete property mapping
- **Flexible**: Supports complex mapping scenarios

## Project Structure

```
src/
├── Najlot.Map/              # Main library project
│   ├── Attributes/          # Mapping attributes
│   ├── Exceptions/          # Custom exceptions
│   ├── IMap.cs             # Core interface
│   ├── Map.cs              # Main implementation
│   └── *.cs                # Supporting interfaces and classes
└── Najlot.Map.Tests/       # Unit tests
    ├── TestTypes/          # Test model classes
    └── *Tests.cs           # Test files
```

## Target Frameworks

- **Main Library**: .NET Standard 2.0 and .NET 8.0
- **Tests**: .NET 9.0 (currently incompatible with .NET 8.0 SDK in CI)

## Core Concepts and Patterns

### 1. Mapping Registration

There are two primary ways to register mappings:

#### Inline Registration
```csharp
IMap map = new Map();
map.Register<SourceType, DestinationType>((from, to) => {
    to.Property = from.Property;
});
```

#### Class-based Registration
```csharp
internal class UserMapMethods
{
    public void MapModelToUser(UserModel from, User to)
    {
        to.Id = from.Id;
        to.Username = from.Username;
    }
    
    public UserModel MapUserToNewModel(User from) => new()
    {
        Id = from.Id,
        Username = from.Username
    };
}

// Register the mapping class
map.Register<UserMapMethods>();
```

### 2. Mapping Execution

```csharp
// Map to new object
var result = map.From(sourceObject).To<DestinationType>();

// Map to existing object
var existingObject = new DestinationType();
map.From(sourceObject).To(existingObject);

// Map collections
var results = map.From<SourceType>(sourceCollection).To<DestinationType>();
```

### 3. Attributes

#### `[MapIgnoreProperty]`
- Used on mapping methods to indicate which destination properties should be ignored during validation
- Can be applied multiple times to ignore multiple properties
- Takes the property name as a parameter

```csharp
[MapIgnoreProperty(nameof(to.Password))]
public void MapModelToUser(UserModel from, User to)
{
    // Password property will be ignored in validation
}
```

#### `[MapIgnoreMethod]`
- Applied to methods that should be ignored during automatic registration
- Useful for utility methods in mapping classes

```csharp
[MapIgnoreMethod]
public Guid SomeUtilityMethod(UserModel from) => from.Id;
```

### 4. Factory Registration

```csharp
map.RegisterFactory(type => IocContainer.Instance.Resolve(type));
```

### 5. Validation

```csharp
// Validates that all destination properties are covered by mapping methods
// Should only be used in unit tests
map.Validate();
```

## Coding Conventions

### Method Naming
- Mapping methods should clearly indicate the transformation: `MapUserToModel`, `MapModelToUser`
- Factory methods should end with "ToNew": `MapUserToNewModel`

### Class Organization
- Group related mappings in dedicated classes (e.g., `UserMapMethods`)
- Use internal visibility for mapping classes unless they need to be public
- Only public methods are automatically registered

### Property Mapping
- Always map all required properties explicitly
- Use `[MapIgnoreProperty]` for properties that should not be mapped (e.g., passwords, calculated fields)
- Prefer object initializer syntax when creating new objects

### Error Handling
- The library uses custom exceptions in the `Exceptions` namespace
- Validation failures will throw exceptions indicating missing property mappings

## Testing Patterns

### Test Structure
- Tests use xUnit framework
- Test classes follow the pattern: `Map[Feature]Tests`
- Use descriptive test method names: `Test_Simple_Map_To_New_Object`

### Arrange-Act-Assert Pattern
```csharp
[Fact]
public void Test_Simple_Map()
{
    // Arrange
    IMap map = new Map();
    map.Register<Source, Destination>((from, to) => { /* mapping */ });
    var source = new Source { /* properties */ };
    
    // Act
    var result = map.From(source).To<Destination>();
    
    // Assert
    Assert.Equal(expectedValue, result.Property);
}
```

## Build and Development

### Prerequisites
- .NET 8.0 SDK (tests currently target .NET 9.0)
- Visual Studio 2022 or compatible IDE

### Building
```bash
cd src
dotnet build
```

### Testing
```bash
cd src
dotnet test
```

### Package Information
- NuGet package: `Najlot.Map`
- Current version: 0.0.5
- Package generation is enabled on build

## Dependencies

### Main Library
- `Microsoft.Bcl.AsyncInterfaces` (9.0.3) - for async enumerable support

### Test Project
- `xunit` (2.9.3) - testing framework
- `Microsoft.NET.Test.Sdk` (17.13.0) - test SDK
- `coverlet.collector` (6.0.4) - code coverage

## Common Use Cases

1. **Simple Object Mapping**: Direct property-to-property mapping
2. **Collection Mapping**: Mapping arrays, lists, and enumerables
3. **Async Enumerable Support**: Support for `IAsyncEnumerable<T>`
4. **Complex Transformations**: Custom logic in mapping methods
5. **Validation**: Ensuring all properties are mapped correctly

## Performance Considerations

- The library is designed for high performance
- Prefer method-based mappings over reflection-heavy alternatives
- Use validation only in development/testing environments
- Consider factory registration for dependency injection scenarios

## Anti-patterns to Avoid

- Don't use reflection-based mapping when explicit mapping is needed
- Avoid skipping validation in development (only skip in production)
- Don't mix mapping logic with business logic
- Avoid overly complex mapping methods (split into smaller methods)

When contributing code or suggesting mappings, follow these established patterns and maintain the library's focus on explicit, performant, and debuggable mapping operations.
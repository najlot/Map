# Najlot.Map.SourceGenerator

A high-performance incremental source generator for Najlot.Map that provides automatic mapping code generation with intelligent caching.

## Features

- **Incremental Generation**: Uses Roslyn's incremental generator API for optimal performance
- **Smart Caching**: Only regenerates code when necessary, providing fast compilation times
- **Class-Level Mapping**: Apply `[Mapping]` to a class to generate implementations for all partial methods
- **Method-Level Mapping**: Apply `[Mapping]` to individual partial methods
- **Flexible Method Names**: Use any method name - not restricted to "MapFrom"
- **IMap Integration**: Automatically uses `IMap` for complex types and collections
- **Factory Support**: Respects factories registered with `IMap.RegisterFactory`
- **Custom Type Converters**: Detects and uses custom mapping methods for type conversions
- **Property Ignoring**: Use `[MapIgnoreProperty]` to skip specific properties
- **Smart Null Handling**: Proper null checking for both source and target properties
- **Zero Runtime Overhead**: All code generation happens at compile time

## Installation

```bash
dotnet add package Najlot.Map.SourceGenerator
```

## Usage

### Class-Level Mapping with IMap

Apply the `[Mapping]` attribute to a partial class to generate implementations for all partial methods.

Note: The `[Mapping]` attribute is defined in the core package (`Najlot.Map`) in the `Najlot.Map.Attributes` namespace.

```csharp
using Najlot.Map;
using Najlot.Map.Attributes;

[Mapping]
public partial class UserMappings
{
    // Partial methods with IMap parameter
    public partial void MapUser(IMap map, UserModel from, User to);
    public partial void MapAddress(IMap map, AddressModel from, Address to);
    
    // Custom type converter - automatically detected and used
    public DateTime ConvertDate(DateTimeOffset offset) => offset.UtcDateTime;
}

// Register and use
var map = new Map()
    .Register<UserMappings>()
    .RegisterFactory(t => /* your factory logic */);

var userModel = new UserModel { /* ... */ };
var user = new User();
map.From(userModel).To(user);
```

The source generator will create implementations that:
- Map simple properties directly (int, string, etc.)
- Use `IMap.From().To()` for complex objects
- Use `IMap.From().ToList()` for `List<T>` properties
- Use `IMap.From().ToArray()` for arrays
- Call custom converter methods when property types don't match
- Handle null sources and targets properly with factory support

### Ignoring Properties

Use `[MapIgnoreProperty]` to skip specific properties:

```csharp
using Najlot.Map.Attributes;

[Mapping]
public partial class UserMappings
{
    [MapIgnoreProperty(nameof(to.Password))]
    [MapIgnoreProperty(nameof(to.Salt))]
    public partial void MapUser(IMap map, UserModel from, User to);
}
```

### Method-Level Mapping

Apply `[Mapping]` to individual partial methods for simple mappings:

```csharp
using Najlot.Map.Attributes;

public partial class UserMapper
{
    [Mapping]
    public partial UserDto MapToDto(User user);
    
    [Mapping]
    public partial void MapUserDetails(IMap map, UserModel from, User to);
}
```

## How It Works

1. **Compilation Time**: The source generator runs during compilation
2. **Syntax Analysis**: It finds all partial classes and methods decorated with `[Mapping]`
3. **Code Generation**: For each target, it generates appropriate mapping code:
   - **Class-level**: Generates implementations for all partial void methods with signature `(IMap map, TSource from, TTarget to)`
   - **Method-level**: Generates implementations for individual partial methods
4. **Smart Mapping**:
   - Direct assignment for matching simple types
   - Custom converter method detection for type mismatches
   - `IMap` usage for complex objects and collections
   - Factory-aware object creation
5. **Caching**: Uses incremental generation to avoid regenerating unchanged code

## Generated Code Examples

### For Complex Types with Nested Objects

```csharp
// Input
[Mapping]
public partial class UserMappings
{
    public partial void MapUser(IMap map, UserModel from, User to);
}

// Generated
public partial void MapUser(IMap map, UserModel from, User to)
{
    to.Id = from.Id;
    to.Name = from.Name;
    
    // Nested object with null handling and factory support
    if (from.Address != null)
    {
        if (to.Address == null)
        {
            to.Address = map.From(from.Address).To<Address>();
        }
        else
        {
            map.From(from.Address).To(to.Address);
        }
    }
    else
    {
        to.Address = null;
    }
    
    // Collection mapping
    to.Features = map.From<FeatureModel>(from.Features).ToList<Feature>();
}
```

## Performance Characteristics

- **Compile Time**: Incremental generation ensures minimal impact on build time
- **Runtime**: Zero overhead - all code is generated at compile time
- **Memory**: No reflection or runtime code generation needed

## Requirements

- .NET Standard 2.0 or higher
- C# 9.0 or higher (for partial method support with return types)

## Supported Scenarios

### Method Signatures

The generator supports two types of partial methods:

1. **Void methods with 3 parameters** (for complex mappings with IMap):
   ```csharp
   public partial void AnyMethodName(IMap map, TSource from, TTarget to);
   ```

2. **Methods with return type and 1 parameter** (for simple mappings):
   ```csharp
   public partial TTarget AnyMethodName(TSource source);
   ```

### Mapping Behavior

- **Simple Types**: Direct property assignment (int, string, bool, DateTime, etc.)
- **Complex Objects**: Uses `IMap.From().To<T>()` or `IMap.From().To(existing)`
- **Collections**: 
  - `List<T>` → uses `ToList<T>()`
  - Arrays → uses `ToArray<T>()`
  - `IEnumerable<T>` → uses `To<T>()`
- **Custom Conversions**: Automatically detects and uses methods with signature `TTarget MethodName(TSource source)`
- **Null Handling**: 
  - Checks source for null before mapping
  - Uses factory when target is null
  - Maps to existing object when target is not null
  - Sets target to null when source is null

## Limitations

- Only properties with both getter and setter are mapped
- Property mapping is based on name matching only
- Partial methods must be in partial classes
- Custom converter methods must be in the same class

## Advanced Scenarios

### Using Custom Type Converters

The generator automatically detects and uses custom converter methods:

```csharp
using Najlot.Map.Attributes;

[Mapping]
public partial class UserMappings
{
    public partial void MapUser(IMap map, UserModel from, User to);
    
    // This will be automatically used when mapping DateTimeOffset to DateTime
    public DateTime ConvertToUtc(DateTimeOffset offset) => offset.UtcDateTime;
    
    // This will be used for Guid to string conversions
    public string GuidToString(Guid id) => id.ToString("N");
}
```

### Working with Factories

The generator respects factories registered with `IMap.RegisterFactory`:

```csharp
var map = new Map()
    .Register<UserMappings>()
    .RegisterFactory(type =>
    {
        if (type == typeof(Address)) return new Address();
        if (type == typeof(Feature)) return new Feature();
        throw new InvalidOperationException($"No factory for {type}");
    });

// When mapping, the factory will be used to create new instances
```

### Combining with Manual Mapping

For complex scenarios, combine generated code with manual logic:

```csharp
using Najlot.Map.Attributes;

[Mapping]
public partial class UserMappings
{
    [MapIgnoreProperty(nameof(to.FullName))]
    public partial void MapUser(IMap map, UserModel from, User to);
}

// Then manually handle the ignored property
public void MapUserComplete(IMap map, UserModel from, User to)
{
    MapUser(map, from, to);
    to.FullName = $"{from.FirstName} {from.LastName}";
}
```

## Troubleshooting

### Generated Code Not Appearing

1. Ensure your class/method is marked as `partial`
2. Verify the `[Mapping]` attribute is applied (from `Najlot.Map.Attributes`)
3. Clean and rebuild your project
4. Check the `obj/GeneratedFiles` folder to see generated code

### Build Errors

- Ensure you're using C# 9.0 or higher
- Verify all properties have compatible types
- Check that partial method signatures are correct

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## License

This project is licensed under the MIT license.

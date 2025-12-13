# Complete Working Example

This is a complete, working example demonstrating the Najlot.Map.SourceGenerator with IMap integration.

## Project Setup

```bash
# Create a new console application
dotnet new console -n MapGeneratorDemo
cd MapGeneratorDemo

# Add both packages
dotnet add package Najlot.Map
dotnet add package Najlot.Map.SourceGenerator
```

## Models.cs

```csharp
using Najlot.Map.SourceGenerator;

namespace MapGeneratorDemo;

// Domain entities
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public bool IsActive { get; set; }
    public Address? Address { get; set; }
    public List<Feature> Features { get; set; } = new();
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

public class Feature
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

// Models with different types
public class UserModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset RegisteredAt { get; set; }
    public bool IsActive { get; set; }
    public AddressModel? Address { get; set; }
    public List<FeatureModel> Features { get; set; } = new();
}

public class AddressModel
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

public class FeatureModel
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
```

## Mappers.cs

```csharp
using Najlot.Map;
using Najlot.Map.SourceGenerator;

namespace MapGeneratorDemo;

// Class-level mapping - generates implementations for all partial methods
[Mapping]
public partial class UserMappings
{
    // Use meaningful names - any name works!
    public partial void MapUser(IMap map, UserModel from, User to);
    public partial void MapAddress(IMap map, AddressModel from, Address to);
    public partial void MapFeature(IMap map, FeatureModel from, Feature to);
    
    // Custom type converter - automatically detected and used
    public DateTime ConvertDateTimeOffset(DateTimeOffset offset) => offset.UtcDateTime;
}
```

## Program.cs

```csharp
using MapGeneratorDemo;
using Najlot.Map;

// Setup the map with factory registration
var map = new Map()
    .Register<UserMappings>()
    .RegisterFactory(type =>
    {
        if (type == typeof(Address)) return new Address();
        if (type == typeof(Feature)) return new Feature();
        throw new InvalidOperationException($"No factory for {type}");
    });

// Example 1: Map complex object with nested properties
Console.WriteLine("=== Example 1: Complex Mapping with Nested Objects ===");
var userModel = new UserModel
{
    Id = Guid.NewGuid(),
    Username = "johndoe",
    Email = "john@example.com",
    RegisteredAt = DateTimeOffset.UtcNow,
    IsActive = true,
    Address = new AddressModel
    {
        Street = "123 Main St",
        City = "Springfield",
        ZipCode = "12345"
    },
    Features = new List<FeatureModel>
    {
        new() { Code = "F001", Name = "Premium" },
        new() { Code = "F002", Name = "Analytics" }
    }
};

var user = new User();
map.From(userModel).To(user);

Console.WriteLine($"User: {user.Username} ({user.Email})");
Console.WriteLine($"  Registered: {user.RegisteredAt}");
Console.WriteLine($"  Active: {user.IsActive}");
Console.WriteLine($"  Address: {user.Address?.Street}, {user.Address?.City}");
Console.WriteLine($"  Features: {string.Join(", ", user.Features.Select(f => f.Name))}");

// Example 2: Factory usage demonstration
Console.WriteLine("\n=== Example 2: Factory Usage ===");
var userModel2 = new UserModel
{
    Id = Guid.NewGuid(),
    Username = "janedoe",
    Email = "jane@example.com",
    RegisteredAt = DateTimeOffset.UtcNow,
    Address = new AddressModel { Street = "456 Oak Ave", City = "Shelbyville" }
};

var user2 = new User();
map.From(userModel2).To(user2);
Console.WriteLine($"Address created via factory: {user2.Address?.Street}");

// Example 3: Null handling
Console.WriteLine("\n=== Example 3: Null Handling ===");
var userModel3 = new UserModel
{
    Id = Guid.NewGuid(),
    Username = "bob",
    Email = "bob@example.com",
    RegisteredAt = DateTimeOffset.UtcNow,
    Address = null // Null address
};

var user3 = new User();
map.From(userModel3).To(user3);
Console.WriteLine($"User with null address: {user3.Username}");
Console.WriteLine($"  Address is null: {user3.Address == null}");
```

## Generated Code

The source generator will create these files during compilation:

### UserMappings_MapUser.g.cs
```csharp
// <auto-generated/>
#nullable enable

namespace MapGeneratorDemo
{
    partial class UserMappings
    {
        public partial void MapUser(IMap map, UserModel from, User to)
        {
            to.Id = from.Id;
            to.Username = from.Username;
            to.Email = from.Email;
            to.IsActive = from.IsActive;
            
            // Custom converter automatically used
            to.RegisteredAt = ConvertDateTimeOffset(from.RegisteredAt);
            
            // Smart null handling with factory support
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
    }
}
```

### UserMappings_MapAddress.g.cs
```csharp
// <auto-generated/>
#nullable enable

namespace MapGeneratorDemo
{
    partial class UserMappings
    {
        public partial void MapAddress(IMap map, AddressModel from, Address to)
        {
            to.Street = from.Street;
            to.City = from.City;
            to.ZipCode = from.ZipCode;
        }
    }
}
```

## Expected Output

```
=== Example 1: Complex Mapping with Nested Objects ===
User: johndoe (john@example.com)
  Registered: 12/13/2025 9:00:00 PM
  Active: True
  Address: 123 Main St, Springfield
  Features: Premium, Analytics

=== Example 2: Factory Usage ===
Address created via factory: 456 Oak Ave

=== Example 3: Null Handling ===
User with null address: bob
  Address is null: True
```

## Key Takeaways

1. **Flexible Method Names**: Use any names that make sense for your domain
2. **IMap Integration**: Automatic use of `IMap` for complex types and collections
3. **Factory Support**: Respects factories registered with `IMap.RegisterFactory`
4. **Custom Converters**: Automatic detection and use of type conversion methods
5. **Smart Null Handling**: Proper null checking for both source and target
6. **Type Safety**: All code is generated and type-checked at compile time
7. **Performance**: No reflection, no runtime overhead
8. **Debuggable**: Step into generated code just like handwritten code
9. **Transparent**: Generated files visible in obj/GeneratedFiles folder
10. **Incremental**: Fast compilation with intelligent caching

## Viewing Generated Files

Generated files are located at:
```
obj/Debug/net8.0/GeneratedFiles/Najlot.Map.SourceGenerator/Najlot.Map.SourceGenerator.MappingGenerator/
```

Enable this in your .csproj to see generated files in IDE:
```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)\GeneratedFiles</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

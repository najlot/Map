# Najlot.Map.SourceGenerator Examples

This document provides a complete working example and various usage scenarios for the Najlot.Map.SourceGenerator.

## Complete Working Example

This is a complete, working example demonstrating the Najlot.Map.SourceGenerator with IMap integration.

### Project Setup

```bash
# Create a new console application
dotnet new console -n MapGeneratorDemo
cd MapGeneratorDemo

# Add both packages
dotnet add package Najlot.Map
dotnet add package Najlot.Map.SourceGenerator
```

### Models.cs

```csharp
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

### Mappers.cs

```csharp
using Najlot.Map;
using Najlot.Map.Attributes;

namespace MapGeneratorDemo;

// Class-level mapping - generates implementations for all partial methods
[Mapping]
public partial class UserMappings
{
    // Use meaningful names - any name works!
    // You can also use [MapIgnoreProperty(nameof(to.Property))] to skip properties
    public partial void MapUser(IMap map, UserModel from, User to);
    public partial void MapAddress(IMap map, AddressModel from, Address to);
    public partial void MapFeature(IMap map, FeatureModel from, Feature to);
    
    // Custom type converter - automatically detected and used
    public DateTime ConvertDateTimeOffset(DateTimeOffset offset) => offset.UtcDateTime;
}
```

### Program.cs

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

### Generated Code

The source generator will create these files during compilation:

#### UserMappings_MapUser.g.cs
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

#### UserMappings_MapAddress.g.cs
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

### Expected Output

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

### Viewing Generated Files

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

## Additional Scenarios

### Method-Level Mapping

You can define mappings for specific methods without using the `IMap` interface if you don't need recursive mapping capabilities.

```csharp
using Najlot.Map.Attributes;

public partial class OrderMapper
{
    [Mapping]
    public partial OrderDto ConvertToDto(Order order);
    
    [Mapping]
    public partial OrderSummaryDto CreateSummary(Order order);
    
    [Mapping]
    public partial void UpdateOrderDetails(IMap map, OrderModel from, Order to);
}

// Usage
var mapper = new OrderMapper();
var order = GetOrder();
var dto = mapper.ConvertToDto(order);
var summary = mapper.CreateSummary(order);
```

### Request/Response Models

Common pattern for API controllers.

```csharp
using Najlot.Map.Attributes;

public partial class ProductMapper
{
    [Mapping]
    public partial Product MapFromRequest(CreateProductRequest request);
    
    [Mapping]
    public partial ProductResponse MapToResponse(Product product);
}

// In your controller
[HttpPost]
public IActionResult CreateProduct(CreateProductRequest request)
{
    var mapper = new ProductMapper();
    var product = mapper.MapFromRequest(request);
    
    _repository.Add(product);
    _repository.SaveChanges();
    
    var response = mapper.MapToResponse(product);
    return Ok(response);
}
```

## Common Patterns

### Service Layer Integration

Encapsulate the mapper configuration within your service.

```csharp
public class UserService
{
    private readonly IMap _map;
    
    public UserService()
    {
        _map = new Map()
            .Register<UserMappings>()
            .RegisterFactory(type =>
            {
                // Use a DI container for this
                if (type == typeof(Address)) return new Address();
                if (type == typeof(Feature)) return new Feature();
                throw new InvalidOperationException($"No factory for {type}");
            });
    }
    
    public void UpdateUser(UserModel model)
    {
        var user = new User();
        _map.From(model).To(user);
        SaveToDatabase(user);
    }
}
```

### Repository Pattern with IMap

```csharp
using Najlot.Map;
using Najlot.Map.Attributes;

[Mapping]
public partial class UserMappings
{
    public partial void UpdateFromDto(IMap map, UserDto from, User to);
}

public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IMap _map;
    
    public UserService(IUserRepository repository)
    {
        _repository = repository;
        _map = new Map().Register<UserMappings>();
    }
    
    public void UpdateUser(Guid id, UserDto dto)
    {
        var user = _repository.GetById(id);
        _map.From(dto).To(user);
        _repository.Update(user);
    }
}
```

### CQRS Pattern

```csharp
using Najlot.Map.Attributes;

[Mapping]
public partial class UserCommandMappings
{
    public partial void MapCreateCommand(IMap map, CreateUserCommand from, User to);
    public partial void MapUpdateCommand(IMap map, UpdateUserCommand from, User to);
}

[Mapping]
public partial class UserQueryMappings
{
    [Mapping]
    public partial UserReadModel CreateReadModel(User from);
    
    [Mapping]
    public partial UserListItemModel CreateListItem(User from);
}
```

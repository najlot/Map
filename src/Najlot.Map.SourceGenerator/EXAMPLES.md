# Example: Using Najlot.Map.SourceGenerator

This example demonstrates how to use the source generator in real applications.

## Scenario 1: Complex Entity Mapping with IMap

```csharp
using Najlot.Map;
using Najlot.Map.SourceGenerator;
using Najlot.Map.Attributes;

// Domain entity
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredAt { get; set; }
    public Address Address { get; set; }
    public List<Feature> Features { get; set; } = new();
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}

public class Feature
{
    public string Code { get; set; }
    public string Name { get; set; }
}

// Models
public class UserModel
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
    public AddressModel Address { get; set; }
    public List<FeatureModel> Features { get; set; }
}

public class AddressModel
{
    public string Street { get; set; }
    public string City { get; set; }
}

public class FeatureModel
{
    public string Code { get; set; }
    public string Name { get; set; }
}

// Mapping class
[Mapping]
public partial class UserMappings
{
    [MapIgnoreProperty(nameof(to.SessionId))]
    public partial void MapUser(IMap map, UserModel from, User to);
    public partial void MapAddress(IMap map, AddressModel from, Address to);
    public partial void MapFeature(IMap map, FeatureModel from, Feature to);
    
    // Custom type converter - automatically used
    public DateTime ConvertToUtc(DateTimeOffset offset) => offset.UtcDateTime;
}

// Usage in a service
public class UserService
{
    private readonly IMap _map;
    
    public UserService()
    {
        _map = new Map()
            .Register<UserMappings>()
            .RegisterFactory(type =>
            {
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

## Scenario 2: Method-Level Mapping

```csharp
using Najlot.Map.SourceGenerator;

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

## Scenario 3: Request/Response Models

```csharp
using Najlot.Map.SourceGenerator;

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

## Performance Comparison

### With Source Generator
- No runtime reflection
- No expression compilation
- Zero allocation overhead
- Fully debuggable code

### Traditional Mapping Libraries
- Runtime reflection (slower)
- Expression tree compilation (memory overhead)
- Less debuggable

## Best Practices

1. **Use class-level [Mapping] for related mappings** - Group related mapping methods in one class
2. **Use method-level [Mapping] for simple conversions** - Individual methods for straightforward mappings
3. **Combine with IMap** for complex scenarios - Let the generator handle nested objects and collections
4. **Use custom converters** for type mismatches - Define simple conversion methods
5. **Register factories** for object creation - Control how new instances are created
6. **Use [MapIgnoreProperty]** for sensitive data - Skip passwords, tokens, etc.
7. **Use meaningful method names** - Name methods based on what they do, not required patterns

## Common Patterns

### Repository Pattern with IMap

```csharp
using Najlot.Map;
using Najlot.Map.SourceGenerator;

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
using Najlot.Map.SourceGenerator;

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

## Performance Comparison

### With Source Generator
- No runtime reflection
- No expression compilation
- Zero allocation overhead for mapping logic
- Fully debuggable code
- Uses registered factories efficiently

### Traditional Mapping Libraries
- Runtime reflection (slower)
- Expression tree compilation (memory overhead)
- Less debuggable
- May not integrate with factory patterns

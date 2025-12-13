# Example: Using Najlot.Map.SourceGenerator

This example demonstrates how to use the source generator in a real application.

## Scenario 1: User Entity Mapping

```csharp
using Najlot.Map.SourceGenerator;

// Domain entity
[Mapping]
public partial class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

// DTO for API responses
[Mapping]
public partial class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Usage in a service
public class UserService
{
    public void UpdateUser(UserDto dto)
    {
        var user = new User();
        user.MapFrom(dto); // Generated method
        
        // Save to database
        SaveToDatabase(user);
    }
}
```

## Scenario 2: Mapper Class Pattern

```csharp
using Najlot.Map.SourceGenerator;

public partial class OrderMapper
{
    [Mapping]
    public partial OrderDto MapToDto(Order order);
    
    [Mapping]
    public partial OrderSummaryDto MapToSummary(Order order);
}

// Usage
var mapper = new OrderMapper();
var order = GetOrder();
var dto = mapper.MapToDto(order);
var summary = mapper.MapToSummary(order);
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

1. **Use partial classes** for entities that need self-mapping
2. **Use partial methods** for cross-type mappings
3. **Combine with manual mapping** for complex scenarios
4. **Keep DTOs simple** - complex logic belongs in business layer
5. **Use multiple mappers** for different contexts (API, Database, etc.)

## Common Patterns

### Repository Pattern

```csharp
public interface IUserRepository
{
    User GetById(Guid id);
    void Update(User user);
}

[Mapping]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class UserService
{
    private readonly IUserRepository _repository;
    
    public void UpdateUser(Guid id, UserDto dto)
    {
        var user = _repository.GetById(id);
        user.MapFrom(dto); // Generated
        _repository.Update(user);
    }
}
```

### CQRS Pattern

```csharp
public partial class UserCommandMapper
{
    [Mapping]
    public partial User MapFromCreateCommand(CreateUserCommand command);
    
    [Mapping]
    public partial User MapFromUpdateCommand(UpdateUserCommand command);
}

public partial class UserQueryMapper
{
    [Mapping]
    public partial UserReadModel MapToReadModel(User user);
}
```

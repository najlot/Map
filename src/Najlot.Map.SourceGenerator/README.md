# Najlot.Map.SourceGenerator

A high-performance incremental source generator for Najlot.Map that provides automatic mapping code generation with intelligent caching.

## Features

- **Incremental Generation**: Uses Roslyn's incremental generator API for optimal performance
- **Smart Caching**: Only regenerates code when necessary, providing fast compilation times
- **Partial Class Support**: Generate mapping methods for partial classes
- **Partial Method Support**: Implement partial methods with automatic property mapping
- **Zero Runtime Overhead**: All code generation happens at compile time

## Installation

```bash
dotnet add package Najlot.Map.SourceGenerator
```

## Usage

### Partial Class Mapping

Apply the `[Mapping]` attribute to a partial class to generate a `MapFrom` method:

```csharp
using Najlot.Map.SourceGenerator;

[Mapping]
public partial class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// Usage
var source = new User { Id = 1, Name = "John", Email = "john@example.com" };
var target = new User();
target.MapFrom(source);
```

The source generator will create:

```csharp
public partial class User
{
    /// <summary>
    /// Maps properties from another instance of User.
    /// </summary>
    public void MapFrom(User source)
    {
        if (source == null)
        {
            throw new System.ArgumentNullException(nameof(source));
        }

        this.Id = source.Id;
        this.Name = source.Name;
        this.Email = source.Email;
    }
}
```

### Partial Method Mapping

Apply the `[Mapping]` attribute to a partial method to generate the implementation:

```csharp
using Najlot.Map.SourceGenerator;

public partial class UserMapper
{
    [Mapping]
    public partial UserDto MapToDto(User user);
}

// Usage
var mapper = new UserMapper();
var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
var dto = mapper.MapToDto(user);
```

The source generator will create:

```csharp
public partial class UserMapper
{
    public partial UserDto MapToDto(User user)
    {
        var result = new UserDto();
        
        result.Id = user.Id;
        result.Name = user.Name;
        result.Email = user.Email;
        
        return result;
    }
}
```

## How It Works

1. **Compilation Time**: The source generator runs during compilation
2. **Syntax Analysis**: It finds all partial classes and methods decorated with `[Mapping]`
3. **Code Generation**: For each target, it generates appropriate mapping code
4. **Caching**: Uses incremental generation to avoid regenerating unchanged code

## Performance Characteristics

- **Compile Time**: Incremental generation ensures minimal impact on build time
- **Runtime**: Zero overhead - all code is generated at compile time
- **Memory**: No reflection or runtime code generation needed

## Requirements

- .NET Standard 2.0 or higher
- C# 9.0 or higher (for partial method support with return types)

## Limitations

- Only properties with both getter and setter are mapped
- Property mapping is based on name matching only
- Only supports partial classes and partial methods
- Partial methods must have a single parameter and return a non-void type

## Advanced Scenarios

### Customizing Generated Code

Currently, the generator creates simple property-to-property mappings. For more complex scenarios, you can:

1. Use the main Najlot.Map library for custom mapping logic
2. Manually implement partial methods for complex transformations
3. Combine generated code with custom mapping extensions

## Troubleshooting

### Generated Code Not Appearing

1. Ensure your class/method is marked as `partial`
2. Verify the `[Mapping]` attribute is applied
3. Clean and rebuild your project
4. Check the `obj/GeneratedFiles` folder to see generated code

### Build Errors

- Ensure you're using C# 9.0 or higher
- Verify all properties have compatible types
- Check that partial method signatures are correct

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## License

This project is licensed under the same license as Najlot.Map.

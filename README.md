# ![M](images/icon.png) MAP
Map is a simple library to manage map methods.

## NuGet Packages
This library is distributed as NuGet packages.

### Core Library
```bash
dotnet add package Najlot.Map
```

### Source Generator (NEW!)
```bash
dotnet add package Najlot.Map.SourceGenerator
```

## Features
- Efficient and fast.
- Debuggable mappings.
- Supports complex mappings.
- Flexible and easy to use.
- Mapping validation and map assistance with the Map.Validate method.
- **NEW**: Compile-time code generation with incremental source generator.

## Source Generator Quickstart

The source generator provides compile-time code generation for mapping with zero runtime overhead:

```csharp
using Najlot.Map.SourceGenerator;

// Apply [Mapping] to partial classes for self-mapping
[Mapping]
public partial class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// Usage - MapFrom method is generated at compile time
var source = new User { Id = 1, Name = "John", Email = "john@example.com" };
var target = new User();
target.MapFrom(source); // Generated method

// Apply [Mapping] to partial methods for cross-type mapping
public partial class UserMapper
{
    [Mapping]
    public partial UserDto MapToDto(User user);
}

// Usage - method implementation is generated at compile time
var mapper = new UserMapper();
var dto = mapper.MapToDto(user); // Generated implementation
```

For more information about the source generator, see [Najlot.Map.SourceGenerator README](src/Najlot.Map.SourceGenerator/README.md).

## Runtime Mapping Quickstart
Following classes will give you an idea of how this library can be used.
For more information, see the unit tests or open an issue.

```csharp
/// <summary>
/// Class with methods for mapping User to UserModel and back.
/// Only public methods are automatically registered.
/// Password property should be ignored by "map.Validate()" method.
/// </summary>
internal class UserMapMethods
{
	[MapIgnoreProperty(nameof(to.Password))]
	public void MapModelToUser(UserModel from, User to)
	{
		to.Id = from.Id;
		to.Username = from.Username;
	}

	[MapIgnoreProperty(nameof(to.Password))]
	public UserModel MapUserToNewModel(User from) => new()
	{
		Id = from.Id,
		Username = from.Username
	};

	[MapIgnoreMethod]
	public Guid SomeMapToBeIgnored(UserModel from) => from.Id;
}

public class UserService
{
	private readonly IMap map;
	private readonly IUser _repository = GetUserRepositoryFromSomewhere();

	public UserService()
	{
		map = new Map();

		// Register mapping methods.
		// TODO: Move this code to application startup.
		map.Register<UserMapMethods>();
		map.RegisterFactory(type => IocContainer.Instance.Resolve(type));

		// Validate mapping methods.
		// It compares all properties in destination class and the properties used in map methods
		// and throws an exception when some of them are missing.
		// This method should be used only in unit tests.
		map.Validate();
	}

	public void UpdateUserData(User user)
	{
		// Map a single object.
		var model = map.From(user).To<UserModel>();
		_repository.UpdateUserData(model);
	}

	public void UpdateUserData(User[] users)
	{
		// Map an IEnumerable of objects.
		var models = map.From<User>(users).To<UserModel>();
		_repository.UpdateUserData(models);
	}
}
```

## When to use
- When you want full control over your mappings.  
- When other mappers approach is too "magical" for your needs.  
- When you need a fast and lightweight but flexible solution.
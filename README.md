# ![M](images/icon.png) MAP
Map is a simple library to manage mapping methods.

## NuGet Packages
This library is distributed as NuGet packages.

### Core Library
```bash
dotnet add package Najlot.Map
```

### Source Generator
```bash
dotnet add package Najlot.Map.SourceGenerator
```

## Features
- Efficient and fast.
- Debuggable mappings.
- Supports complex mappings.
- Flexible and easy to use.
- Mapping validation and map assistance with the Map.Validate method.
- Compile-time code generation with incremental source generator.

## Source Generator Quickstart

The source generator provides compile-time code generation for mapping with zero runtime overhead:

```csharp
using Najlot.Map;
using Najlot.Map.SourceGenerator;

// Class-level [Mapping] generates implementations for all partial methods
[Mapping]
public partial class UserMappings
{
    // You can use any method name - not restricted to "MapFrom"
    public partial void MapUser(IMap map, UserModel from, User to);
    public partial void MapFeature(IMap map, FeatureModel from, Feature to);
    
	// Generator detects private partial methods too, but the IMap.Register call won't register them
	private partial void PartialMapAddress(IMap map, AddressModel from, AddressViewModel to);
	// That may you can do something like this:
	public void MapAddress(IMap map, AddressModel from, AddressViewModel to)
	{
		// Before mapping logic (or custom mapping) here...
		PartialMapAddress(map, from, to);
		// After mapping logic (or custom mapping) here...
	}

    // Custom converter - automatically detected and used
    public DateTime ConvertToUtc(DateTimeOffset offset) => offset.UtcDateTime;
}

// Register and use with IMap
var map = new Map()
    .Register<UserMappings>()
    .RegisterFactory(type => /* your factory logic */);

var userModel = new UserModel { /* ... */ };
var user = new User();
map.From(userModel).To(user); // Uses generated mapping

// Method-level [Mapping] for simple mappings
public partial class UserMapper
{
    [Mapping]
    public partial UserDto ConvertToDto(User user);
}

var mapper = new UserMapper();
var dto = mapper.ConvertToDto(user); // Uses generated implementation
```

### Key Features

- **Flexible naming**: Use any method names that make sense for your domain
- **IMap integration**: Automatically uses `IMap` for complex types and collections
- **Factory support**: Respects factories registered with `IMap.RegisterFactory`
- **Custom converters**: Detects and uses custom type conversion methods
- **Smart null handling**: Proper null checking with factory-aware object creation
- **Property ignoring**: Use `[MapIgnoreProperty]` to skip specific properties

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
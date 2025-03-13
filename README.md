# ![M](images/icon.png) MAP
Map is a simple library to manage map methods.

## NuGet Package
This library is distributed as a NuGet package.
```
dotnet add package Najlot.Map
```

## Features
- Efficient and fast.
- Debuggable mappings.
- Supports complex mappings.
- Flexible and easy to use.
- Mapping validation and map assistance with the Map.Validate method.

## Quickstart
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
		var model = map.From(user).To<UserModel>();
		UserRepository.Instance.UpdateUserData(model);
	}
}
```

## When to use
- When you want full control over your mappings.  
- When other mappers approach is too "magical" for your needs.  
- When you need a fast and lightweight but flexible solution.
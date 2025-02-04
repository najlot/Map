# MAP
Map is a simple library to manage map methods.

## NuGet Package
This library is distributed as an NuGet package.
dotnet add package Najlot.Map

### Quickstart
Following classes should give you an idea how this library can be used.
For more see unit tests or open an issue.

´cs
/// <summary>
/// Class with methods for mapping User to UserModel and back.
/// Only public methods are automatically registered.
/// Password property should be ignored by "map.Validate()" method.
/// </summary>
internal class UserMapMethods
{
	[MapIgnoreProperty(nameof(User.Password))]
	public void MapModelToUser(UserModel from, User to)
	{
		to.Id = from.Id;
		to.Username = from.Username;
	}

	[MapIgnoreProperty(nameof(UserModel.Password))]
	public UserModel MapUserToNewModel(User from) => new()
	{
		Id = from.Id,
		Username = from.Username
	};
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
´
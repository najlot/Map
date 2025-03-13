using Najlot.Map.Exceptions;
using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapCheckFactoryClassTests
{
	internal class ValidUserMapMethods
	{
		public User MapModelToUser(UserModel from) => new()
		{
			Username = from.Username
		};
	}

	internal class InvalidUserMapMethods
	{
		public User MapModelToUser(UserModel from) => new();
	}

	internal class ValidSessionMapMethods
	{
		public static SessionModel MapSessionToModel(IMap map, Session from) => new()
		{
			Id = from.Id,
			User = map.FromNullable(from.User)?.To<UserModel>()
		};
	}

	internal class InvalidSessionMapMethods
	{
		public static SessionModel MapSessionToModel(IMap map, Session from) => new();
	}

	[Fact]
	public void Map_Check_Must_Recognize_Simple_Map_From_Class()
	{
		// Arrange
		IMap map = new Map();

		map.Register<ValidUserMapMethods>();

		// Act & Assert
		map.Validate();
	}

	[Fact]
	public void Map_Check_Must_Recognize_Map_From_Class()
	{
		// Arrange
		IMap map = new Map();

		map.Register<ValidSessionMapMethods>();

		// Act & Assert
		map.Validate();
	}

	[Fact]
	public void Map_Check_Must_Recognize_Missing_Simple_Map_From_Class()
	{
		// Arrange
		IMap map = new Map();

		map.Register<InvalidUserMapMethods>();

		// Act & Assert
		var exception = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains(nameof(UserModel.Username), exception.Message);
	}

	[Fact]
	public void Map_Check_Must_Recognize_Missing_Map_From_Class()
	{
		// Arrange
		IMap map = new Map();

		map.Register<InvalidSessionMapMethods>();

		// Act & Assert
		var exception = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains(nameof(SessionModel.Id), exception.Message);
		Assert.Contains(nameof(SessionModel.User), exception.Message);
	}
}
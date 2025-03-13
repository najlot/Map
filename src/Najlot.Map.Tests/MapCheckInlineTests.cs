using Najlot.Map.Exceptions;
using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapCheckInlineTests
{
	[Fact]
	public void Map_Check_Must_Recognize_Map()
	{
		// Arrange
		IMap map = new Map();

		map.Register<Session, SessionModel>((map, from, to) =>
		{
			to.Id = from.Id;
			to.User = new UserModel();
		});

		// Act & Assert
		map.Validate();
	}

	[Fact]
	public void Map_Check_Must_Recognize_Missing_Map_Properties()
	{
		// Arrange
		IMap map = new Map();

		map.Register<Session, SessionModel>((map, from, to) =>
		{
		});

		// Act & Assert
		var exception = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains(nameof(SessionModel.Id), exception.Message);
		Assert.Contains(nameof(SessionModel.User), exception.Message);
	}

	[Fact]
	public void Map_Check_Must_Recognize_Simple_Map()
	{
		// Arrange
		IMap map = new Map();

		map.Register<User, UserModel>((from, to) =>
		{
			to.Username = from.Username;
		});

		// Act & Assert
		map.Validate();
	}

	[Fact]
	public void Map_Check_Must_Recognize_Missing_Simple_Map_Properties()
	{
		// Arrange
		IMap map = new Map();

		map.Register<User, UserModel>((from, to) =>
		{
		});

		// Act & Assert
		var exception = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains(nameof(UserModel.Username), exception.Message);
	}

	[Fact]
	public void Map_Check_Must_Recognize_Correct_Properties_From_Map()
	{
		// Arrange
		IMap map = new Map();

		map.Register<Session, SessionModel>((map, from, to) =>
		{
			from.Id = to.Id;
			from.User = map.FromNullable(to.User)?.To<User>();
		});

		// Act & Assert
		var exception = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains(nameof(SessionModel.Id), exception.Message);
		Assert.Contains(nameof(SessionModel.User), exception.Message);
	}

	[Fact]
	public void Map_Check_Must_Recognize_Correct_Properties_From_Simple_Map()
	{
		// Arrange
		IMap map = new Map();

		map.Register<Session, SessionModel>((from, to) =>
		{
			from.Id = Guid.NewGuid();
			from.User = new User { Username = "test" };
		});

		// Act & Assert
		var exception = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains(nameof(SessionModel.Id), exception.Message);
		Assert.Contains(nameof(SessionModel.User), exception.Message);
	}
}
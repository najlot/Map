using Najlot.Map.Exceptions;
using Najlot.Map.Tests.TestTypes;
using System.Linq;
using Xunit;

namespace Najlot.Map.Tests;

public class MapProjectToTests
{
	[Fact]
	public void Map_ProjectTo_Should_Work_With_Registered_Expression()
	{
		// Arrange
		IMap map = new Map();
		map.RegisterExpression<User, UserModel>(u => new UserModel { Username = u.Username });

		var users = new[]
		{
			new User { Username = "User1" },
			new User { Username = "User2" }
		}.AsQueryable();

		// Act
		var result = map.From(users).To<UserModel>();

		// Assert
		var list = result.ToList();
		Assert.Equal(2, list.Count);
		Assert.Equal("User1", list[0].Username);
		Assert.Equal("User2", list[1].Username);
	}

	[Fact]
	public void Map_ProjectTo_Should_Throw_When_Not_Registered()
	{
		// Arrange
		IMap map = new Map();
		var users = new[] { new User() }.AsQueryable();

		// Act & Assert
		Assert.Throws<MapNotRegisteredException>(() => map.From(users).To<UserModel>());
	}
}

using Najlot.Map.Tests.TestTypes;
using System.Linq.Expressions;
using Xunit;

namespace Najlot.Map.Tests;

public class MapExpressionRegistrationTests
{
	internal class UserExpressionMap
	{
		public Expression<Func<User, UserModel>> MapUserToModel()
		{
			return u => new UserModel { Username = u.Username };
		}

		public static Expression<Func<UserModel, User>> MapModelToUser()
		{
			return u => new User { Username = u.Username };
		}
	}

	[Fact]
	public void Map_Should_Register_Instance_Expression_From_Class()
	{
		// Arrange
		IMap map = new Map();
		map.Register(new UserExpressionMap());

		var users = new[] { new User { Username = "TestUser" } }.AsQueryable();

		// Act
		var result = map.From(users).To<UserModel>().ToList();

		// Assert
		Assert.Single(result);
		Assert.Equal("TestUser", result[0].Username);
	}

	[Fact]
	public void Map_Should_Register_Static_Expression_From_Class()
	{
		// Arrange
		IMap map = new Map();
		map.Register(new UserExpressionMap());

		var users = new[] { new UserModel { Username = "TestUser" } }.AsQueryable();

		// Act
		var result = map.From(users).To<User>().ToList();

		// Assert
		Assert.Single(result);
		Assert.Equal("TestUser", result[0].Username);
	}
}

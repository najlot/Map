using Najlot.Map.Exceptions;
using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapSimpleMethodRetrievalTests
{
	[Fact]
	public void Test_GetSimpleMapMethod_WithInlineRegistration()
	{
		// Arrange
		IMap map = new Map();
		
		SimpleMapMethod<User, UserModel> expectedMethod = (from, to) =>
		{
			to.Username = from.Username;
		};
		
		map.Register(expectedMethod);
		
		// Act
		var mapMethod = map.GetMethod<User, UserModel>();
		var user = new User { Username = "test" };
		var userModel = new UserModel { Username = "old test" };
		mapMethod(user, userModel);

		// Assert
		Assert.Equal("test", userModel.Username);
	}
	
	[Fact]
	public void Test_GetSimpleMapMethod_WithClassRegistration()
	{
		// Arrange
		IMap map = new Map();
		var mapMethods = new UserMapMethods();
		
		map.Register(mapMethods);
		
		// Act
		var retrievedMethod = map.GetMethod<User, UserModel>();
		
		// Assert
		var user = new User { Username = "test" };
		var userModel = new UserModel();
		retrievedMethod(user, userModel);
		
		Assert.Equal("test", userModel.Username);
	}
	
	[Fact]
	public void Test_GetSimpleMapMethod_NotRegistered_throws_MapNotRegisteredException()
	{
		// Arrange
		IMap map = new Map();

		// Act & Assert
		Assert.Throws<MapNotRegisteredException>(() => map.GetMethod<User, UserModel>());
	}
	
	[Fact]
	public void Test_GetSimpleMapFactoryMethod_WithInlineRegistration()
	{
		// Arrange
		IMap map = new Map();
		
		SimpleMapFactoryMethod<User, UserModel> expectedMethod = (from) => new UserModel
		{
			Username = from.Username
		};
		
		map.Register(expectedMethod);
		
		// Act
		var mapFactoryMethod = map.GetFactoryMethod<User, UserModel>();
		var user = new User { Username = "test" };
		UserModel userModel = mapFactoryMethod(user);

		// Assert
		Assert.Equal("test", userModel.Username);
	}
	
	[Fact]
	public void Test_GetSimpleMapFactoryMethod_WithClassRegistration()
	{
		// Arrange
		IMap map = new Map();
		var mapMethods = new UserMapMethods();
		
		map.Register(mapMethods);
		
		// Act
		var retrievedMethod = map.GetFactoryMethod<int, UserModel>();
		
		// Assert
		Assert.NotNull(retrievedMethod);
		
		// Test that the retrieved method works correctly
		var result = retrievedMethod(123);
		
		Assert.Equal("123", result.Username);
	}
	
	[Fact]
	public void Test_GetSimpleMapFactoryMethod_NotRegistered_throws_MapNotRegisteredException()
	{
		// Arrange
		IMap map = new Map();

		// Act & Assert
		Assert.Throws<MapNotRegisteredException>(() => map.GetFactoryMethod<User, UserModel>());
	}
	
	[Fact]
	public void Test_RetrievedMethod_CanBeUsedDirectly()
	{
		// Arrange
		IMap map = new Map();
		
		map.Register<User, UserModel>((from, to) =>
		{
			to.Username = from.Username + "_mapped";
		});
		
		var mapMethod = map.GetMethod<User, UserModel>();
		
		// Act
		var user = new User { Username = "original" };
		var userModel = new UserModel();
		mapMethod(user, userModel);
		
		// Assert
		Assert.Equal("original_mapped", userModel.Username);
	}
	
	[Fact]
	public void Test_RetrievedFactoryMethod_CanBeUsedDirectly()
	{
		// Arrange
		IMap map = new Map();
		
		map.Register<User, UserModel>((from) => new UserModel
		{
			Username = from.Username + "_factory"
		});
		
		var mapFactoryMethod = map.GetFactoryMethod<User, UserModel>();
		
		// Act
		var user = new User { Username = "original" };
		var result = mapFactoryMethod(user);
		
		// Assert
		Assert.Equal("original_factory", result.Username);
	}
	
	internal class UserMapMethods
	{
		public void MapUserToModel(User from, UserModel to)
		{
			to.Username = from.Username;
		}
		
		public UserModel MapStringToModel(int from) => new() { Username = from.ToString() };
	}
}
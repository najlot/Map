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
		var retrievedMethod = map.GetSimpleMapMethod<User, UserModel>();
		
		// Assert
		Assert.NotNull(retrievedMethod);
		Assert.Same(expectedMethod, retrievedMethod);
	}
	
	[Fact]
	public void Test_GetSimpleMapMethod_WithClassRegistration()
	{
		// Arrange
		IMap map = new Map();
		var mapMethods = new UserMapMethods();
		
		map.Register(mapMethods);
		
		// Act
		var retrievedMethod = map.GetSimpleMapMethod<User, UserModel>();
		
		// Assert
		Assert.NotNull(retrievedMethod);
		
		// Test that the retrieved method works correctly
		var user = new User { Username = "test" };
		var userModel = new UserModel();
		retrievedMethod(user, userModel);
		
		Assert.Equal("test", userModel.Username);
	}
	
	[Fact]
	public void Test_GetSimpleMapMethod_NotRegistered_ReturnsNull()
	{
		// Arrange
		IMap map = new Map();
		
		// Act
		var retrievedMethod = map.GetSimpleMapMethod<User, UserModel>();
		
		// Assert
		Assert.Null(retrievedMethod);
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
		var retrievedMethod = map.GetSimpleMapFactoryMethod<User, UserModel>();
		
		// Assert
		Assert.NotNull(retrievedMethod);
		Assert.Same(expectedMethod, retrievedMethod);
	}
	
	[Fact]
	public void Test_GetSimpleMapFactoryMethod_WithClassRegistration()
	{
		// Arrange
		IMap map = new Map();
		var mapMethods = new UserMapMethods();
		
		map.Register(mapMethods);
		
		// Act
		var retrievedMethod = map.GetSimpleMapFactoryMethod<int, UserModel>();
		
		// Assert
		Assert.NotNull(retrievedMethod);
		
		// Test that the retrieved method works correctly
		var result = retrievedMethod(123);
		
		Assert.Equal("123", result.Username);
	}
	
	[Fact]
	public void Test_GetSimpleMapFactoryMethod_NotRegistered_ReturnsNull()
	{
		// Arrange
		IMap map = new Map();
		
		// Act
		var retrievedMethod = map.GetSimpleMapFactoryMethod<User, UserModel>();
		
		// Assert
		Assert.Null(retrievedMethod);
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
		
		var retrievedMethod = map.GetSimpleMapMethod<User, UserModel>();
		
		// Act
		var user = new User { Username = "original" };
		var userModel = new UserModel();
		retrievedMethod!(user, userModel);
		
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
		
		var retrievedMethod = map.GetSimpleMapFactoryMethod<User, UserModel>();
		
		// Act
		var user = new User { Username = "original" };
		var result = retrievedMethod!(user);
		
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
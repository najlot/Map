using Najlot.Map.Exceptions;
using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapSimpleMethodRetrievalEdgeCaseTests
{
	[Fact]
	public void Test_GetSimpleMapMethod_AfterMultipleRegistrations_ReturnsLast()
	{
		// Arrange
		IMap map = new Map();
		
		// Register first method
		SimpleMapMethod<User, UserModel> firstMethod = (from, to) =>
		{
			to.Username = "first_" + from.Username;
		};
		map.Register(firstMethod);
		
		// Register second method (should overwrite first)
		SimpleMapMethod<User, UserModel> secondMethod = (from, to) =>
		{
			to.Username = "second_" + from.Username;
		};
		map.Register(secondMethod);
		
		// Act
		var retrievedMethod = map.GetMethod<User, UserModel>();
		
		// Assert
		Assert.NotNull(retrievedMethod);
		
		var user = new User { Username = "test" };
		var userModel = new UserModel();
		retrievedMethod(user, userModel);
		
		Assert.Equal("second_test", userModel.Username);
	}
	
	[Fact]
	public void Test_GetSimpleMapMethod_MixedRegistration_InlineAndClass()
	{
		// Arrange
		IMap map = new Map();
		
		// Register via inline method
		map.Register<User, UserModel>((from, to) =>
		{
			to.Username = "inline_" + from.Username;
		});
		
		// Register via class (should overwrite inline)
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
	public void Test_GetSimpleMapMethod_DifferentTypes_Independent()
	{
		// Arrange
		IMap map = new Map();
		
		// Register User -> UserModel
		SimpleMapMethod<User, UserModel> userToModel = (from, to) =>
		{
			to.Username = from.Username;
		};
		map.Register(userToModel);
		
		// Register UserModel -> User  
		SimpleMapMethod<UserModel, User> modelToUser = (from, to) =>
		{
			to.Username = from.Username;
		};
		map.Register(modelToUser);
		
		// Act & Assert
		var method1 = map.GetMethod<User, UserModel>();
		var method2 = map.GetMethod<UserModel, User>();
		Assert.Throws<MapNotRegisteredException>(() => map.GetMethod<User, User>()); // Not registered
		
		Assert.NotNull(method1);
		Assert.NotNull(method2);
	}
	
	[Fact]
	public void Test_GetSimpleMapMethod_OnlyComplexMethodRegistered()
	{
		// Arrange
		IMap map = new Map();
		
		// Register only MapMethod (not SimpleMapMethod)
		map.Register<User, UserModel>((map, from, to) =>
		{
			to.Username = from.Username;
		});

		// Act
		var retrievedMethod = map.GetMethod<User, UserModel>();

		// Assert
		var user = new User { Username = "test" };
		var userModel = new UserModel();
		retrievedMethod(user, userModel);

		Assert.Equal("test", userModel.Username);
	}
	
	internal class UserMapMethods
	{
		public void MapUserToModel(User from, UserModel to)
		{
			to.Username = from.Username;
		}
	}
}
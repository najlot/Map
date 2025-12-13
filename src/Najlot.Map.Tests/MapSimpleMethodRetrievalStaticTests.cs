using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapSimpleMethodRetrievalStaticTests
{
	[Fact]
	public void Test_GetSimpleMapMethod_WithStaticMethod()
	{
		// Arrange
		IMap map = new Map();
		var mapMethods = new StaticMapMethods();
		
		map.Register(mapMethods);
		
		// Act
		var retrievedMethod = map.GetMethod<int, User>();
		
		// Assert
		Assert.NotNull(retrievedMethod);
		
		// Test that the retrieved method works correctly
		var user = new User();
		retrievedMethod(42, user);
		
		Assert.Equal("42", user.Username);
	}
	
	[Fact]
	public void Test_GetSimpleMapFactoryMethod_WithStaticMethod()
	{
		// Arrange
		IMap map = new Map();
		var mapMethods = new StaticMapMethods();
		
		map.Register(mapMethods);
		
		// Act
		var retrievedMethod = map.GetFactoryMethod<int, User>();
		
		// Assert
		Assert.NotNull(retrievedMethod);
		
		// Test that the retrieved method works correctly
		var result = retrievedMethod(42);
		
		Assert.Equal("42", result.Username);
	}
	
	internal class StaticMapMethods
	{
		public static void MapIntToUser(int from, User to)
		{
			to.Username = from.ToString();
		}
		
		public static User MapIntToUser(int from) => new() { Username = from.ToString() };
	}
}
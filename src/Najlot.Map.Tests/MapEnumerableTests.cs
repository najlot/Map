using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapEnumerableTests
{
	private readonly string _usernamePrefix = "test";

	private IEnumerable<User> GetUsers()
	{
		yield return new User()
		{
			Username = _usernamePrefix + " 1"
		};

		yield return new User()
		{
			Username = _usernamePrefix + " 2"
		};
	}

	[Fact]
	public void Test_Simple_Map_To_New_IEnumerable()
	{
		// Arrange
		IMap map = new Map().Register<User, UserModel>(static (from, to) =>
		{
			to.Username = from.Username;
		});

		// Act
		var result = map.From(GetUsers()).To<UserModel>();

		// Assert
		Assert.Equal(2, result.Count());
		Assert.Contains(result, x => x.Username == "test 1");
		Assert.Contains(result, x => x.Username == "test 2");
	}

	[Fact]
	public void Test_Simple_Map_To_New_List()
	{
		// Arrange
		IMap map = new Map().Register<User, UserModel>((from, to) =>
		{
			to.Username = from.Username;
		});

		// Act
		var result = map.From(GetUsers()).ToList<UserModel>();

		// Assert
		Assert.Equal(2, result.Count);
		Assert.Contains(result, x => x.Username == "test 1");
		Assert.Contains(result, x => x.Username == "test 2");
	}

	[Fact]
	public void Test_Simple_Map_To_New_Array()
	{
		// Arrange
		IMap map = new Map().Register<User, UserModel>((from, to) =>
		{
			to.Username = from.Username;
		});

		// Act
		var result = map.From(GetUsers()).ToArray<UserModel>();

		// Assert
		Assert.Equal(2, result.Length);
		Assert.Contains(result, x => x.Username == "test 1");
		Assert.Contains(result, x => x.Username == "test 2");
	}

	[Fact]
	public void Test_Simple_Map_To_Existing_List()
	{
		// Arrange
		IMap map = new Map().Register<User, UserModel>((from, to) =>
		{
			to.Username = from.Username;
		});

		var existingModel = new UserModel() { Username = "test 3" };
		var list = new List<UserModel>() { existingModel };

		// Act
		var result = map.From(GetUsers()).ToList(list);

		// Assert
		Assert.Same(list, result);
		Assert.Equal(2, result.Count);
		Assert.Contains(result, x => x.Username == "test 1");
		Assert.Contains(result, x => x.Username == "test 2");

		Assert.Equal("test 1", existingModel.Username);
	}

	[Fact]
	public void Test_Simple_Map_To_Existing_List_With_Too_Much_Items()
	{
		// Arrange
		IMap map = new Map().Register<User, UserModel>((from, to) =>
		{
			to.Username = from.Username;
		});

		var list = new List<UserModel>() { new(), new(), new(), };

		// Act
		var result = map.From(GetUsers()).ToList(list);

		// Assert
		Assert.Same(list, result);
		Assert.Equal(2, result.Count);
		Assert.Contains(result, x => x.Username == "test 1");
		Assert.Contains(result, x => x.Username == "test 2");
	}
}
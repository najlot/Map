using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapEnumerableTests
{
	private IEnumerable<User> GetUsers()
	{
		yield return new User()
		{
			Username = "test 1"
		};

		yield return new User()
		{
			Username = "test 2"
		};
	}

	[Fact]
	public void Test_Simple_Map_To_New_IEnumerable()
	{
		// Arrange
		IMap map = new Map();
		map.Register<User, UserModel>(static (from, to) =>
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
		IMap map = new Map();
		map.Register<User, UserModel>((from, to) =>
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
		IMap map = new Map();
		map.Register<User, UserModel>((from, to) =>
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
}

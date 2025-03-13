using Najlot.Map.Tests.TestTypes;
using System.Collections.Generic;

namespace Najlot.Map.Tests;

public class MapAsyncEnumerableTests
{
	private async IAsyncEnumerable<User> GetUsers()
	{
		yield return new User()
		{
			Username = "test 1"
		};

		await Task.Delay(1);

		yield return new User()
		{
			Username = "test 2"
		};
	}

	[Fact]
	public void Test_Simple_Map_To_New_AsyncEnumerable()
	{
		// Arrange
		IMap map = new Map();
		map.Register<User, UserModel>((from, to) =>
		{
			to.Username = from.Username;
		});

		// Act
		var result = map.From(GetUsers()).To<UserModel>();

		// Assert
		Assert.Contains(result, x => x.Username == "test 1");
		Assert.Contains(result, x => x.Username == "test 2");
	}

	private async IAsyncEnumerable<User> GetEmptyUsers()
	{
		await Task.CompletedTask;
		yield break;
	}

	[Fact]
	public void Test_Map_With_Empty_AsyncEnumerable()
	{
		// Arrange
		IMap map = new Map();
		map.Register<User, UserModel>((from, to) =>
		{
			to.Username = from.Username;
		});

		// Act
		var result = map.From(GetEmptyUsers()).To<UserModel>();

		// Assert
		Assert.Empty(result);
	}

	private async IAsyncEnumerable<User?> GetNullableUsers()
	{
		yield return null;

		yield return new User()
		{
			Username = "test 1"
		};

		await Task.Delay(1);
	}

	[Fact]
	public void Test_Map_With_Nullable_AsyncEnumerable()
	{
		// Arrange
		IMap map = new Map();
		map.Register<User, UserModel>((from, to) =>
		{
			to.Username = from.Username;
		});

		// Act
		var result = map.FromNullable(GetNullableUsers()).To<UserModel>();

		// Assert
		Assert.Contains(result, x => x == null);
		Assert.Contains(result, x => x != null);
	}
}
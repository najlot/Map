using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapInlineTests
{
	[Fact]
	public void Test_Simple_Map_To_New_Object()
	{
		// Arrange
		IMap map = new Map();

		map.Register<User, UserModel>((from, to) =>
		{
			to.Username = from.Username;
		});

		var user = new User()
		{
			Username = "test"
		};

		// Act
		var result = map.From(user).To<UserModel>();

		// Assert
		Assert.Equal("test", result.Username);
	}

	[Fact]
	public void Test_Simple_Map_To_Existing_Object()
	{
		// Arrange
		IMap map = new Map();

		map.Register<User, UserModel>((from, to) =>
		{
			to.Username = from.Username;
		});

		var user = new User()
		{
			Username = "test"
		};

		var usermodel = new UserModel();

		// Act
		var result = map.From(user).To(usermodel);

		// Assert
		Assert.Same(usermodel, result);
		Assert.Equal("test", result.Username);
	}

	[Fact]
	public void Test_Map_To_New_Object()
	{
		// Arrange
		IMap map = new Map();

		map.Register<User, UserModel>((m, from, to) =>
		{
			to.Username = from.Username;
		});

		map.Register<Session, SessionModel>((m, from, to) =>
		{
			to.Id = from.Id;

			if (from.User != null)
			{
				to.User = m.From(from.User).To<UserModel>();
			}
		});

		var session = new Session()
		{
			Id = Guid.NewGuid(),
			User = new User()
			{
				Username = "test"
			}
		};

		// Act
		var result = map.From(session).To<SessionModel>();

		// Assert
		Assert.Equal(session.Id, result.Id);
		Assert.Equal("test", result.User?.Username);
	}

	[Fact]
	public void Test_Map_To_Existing_Object()
	{
		// Arrange
		IMap map = new Map();

		map.Register<UserModel, User>((m, from, to) =>
		{
			to.Username = from.Username;
		});

		map.Register<SessionModel, Session>((m, from, to) =>
		{
			to.Id = from.Id;

			if (from.User != null)
			{
				to.User = m.From(from.User).To<User>();
			}
		});

		var sessionModel = new SessionModel()
		{
			Id = Guid.NewGuid(),
			User = new UserModel()
			{
				Username = "test"
			}
		};

		var session = new Session();

		// Act
		var result = map.From(sessionModel).To(session);

		// Assert
		Assert.Equal(sessionModel.Id, result.Id);
		Assert.Equal("test", result.User?.Username);
	}

	[Fact]
	public void Test_Map_Simple_Types()
	{
		// Arrange
		IMap map = new Map();
		map.Register((int from) => from.ToString());

		// Act
		var result = map.From(123).To<string>();

		// Assert
		Assert.Equal("123", result);
	}
}
using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapStaticFunctionsTests
{
	private static void MapModelToUser(UserModel from, User to)
	{
		to.Username = from.Username;
	}

	private static void MapSessionToModel(IMap map, Session from, SessionModel to)
	{
		to.Id = from.Id;
		if (from.User != null)
		{
			to.User = map.From(from.User).To<UserModel>();
		}
	}

	private static void MapUserToModel(User from, UserModel to)
	{
		to.Username = from.Username;
	}

	private static void MapModelToSession(IMap map, SessionModel from, Session to)
	{
		to.Id = from.Id;
		if (from.User != null)
		{
			to.User = map.From(from.User).To<User>();
		}
	}

	[Fact]
	public void Test_Simple_Map_To_New_Object()
	{
		// Arrange
		IMap map = new Map();

		map.Register<User, UserModel>(MapUserToModel);

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

		map.Register<User, UserModel>(MapUserToModel);

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

		map.Register<User, UserModel>(MapUserToModel);
		map.Register<Session, SessionModel>(MapSessionToModel);

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

		map.Register<UserModel, User>(MapModelToUser);
		map.Register<SessionModel, Session>(MapModelToSession);

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
}

using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapClassFunctionsTests
{
	internal class UserMapMethods
	{
		public void MapModelToUser(UserModel from, User to)
		{
			to.Username = from.Username;
		}

		public void MapUserToModel(User from, UserModel to)
		{
			to.Username = from.Username;
		}

		public void MapStringToModel(int from, UserModel to)
		{
			to.Username = from.ToString();
		}

		public UserModel MapStringToModel(int from) => new() { Username = from.ToString() };

		public static void MapStringToUser(int from, User to)
		{
			to.Username = from.ToString();
		}

		public static User MapStringToUser(int from) => new() { Username = from.ToString() };
	}

	internal class SessionMapMethods
	{
		public static void MapSessionToModel(IMap map, Session from, SessionModel to)
		{
			to.Id = from.Id;
			if (from.User != null)
			{
				to.User = map.From(from.User).To<UserModel>();
			}
		}

		public static void MapModelToSession(IMap map, SessionModel from, Session to)
		{
			to.Id = from.Id;
			if (from.User != null)
			{
				to.User = map.From(from.User).To<User>();
			}
		}
	}

	[Fact]
	public void Test_Simple_Map_To_New_Object()
	{
		// Arrange
		IMap map = new Map();

		map.Register<UserMapMethods>();
		map.Register<SessionMapMethods>();

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

		map.Register(new UserMapMethods());
		map.Register(new SessionMapMethods());

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

		map.Register<UserMapMethods>();
		map.Register<SessionMapMethods>();

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

		map.Register(new UserMapMethods());
		map.Register(new SessionMapMethods());

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
	public void Test_Map_Int_To_User()
	{
		// Arrange
		IMap map = new Map();

		map.Register(new UserMapMethods());

		// Act
		var result = map.From(123).To<User>();

		// Assert
		Assert.Equal("123", result.Username);
	}

	[Fact]
	public void Test_Map_Int_To_Existing_User()
	{
		// Arrange
		IMap map = new Map();

		map.Register(new UserMapMethods());

		// Act
		var result = map.From(133).To(new User());

		// Assert
		Assert.Equal("133", result.Username);
	}

	[Fact]
	public void Test_Map_Int_To_UserModel()
	{
		// Arrange
		IMap map = new Map();

		map.Register(new UserMapMethods());

		// Act
		var result = map.From(122).To<UserModel>();

		// Assert
		Assert.Equal("122", result.Username);
	}

	[Fact]
	public void Test_Map_Int_To_Existing_UserModel()
	{
		// Arrange
		IMap map = new Map();

		map.Register(new UserMapMethods());

		// Act
		var result = map.From(321).To(new UserModel());

		// Assert
		Assert.Equal("321", result.Username);
	}
}
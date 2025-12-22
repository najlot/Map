using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapFactoryTests
{
	internal class UserMapMethods
	{
		public UserModel MapModelToUser(User from) => new()
		{
			Username = from.Username
		};
	}

	internal class SessionMapMethods
	{
		public static SessionModel MapSessionToModel(IMap map, Session from) => new()
		{
			Id = from.Id,
			User = map.From(from.User).To<UserModel>()
		};
	}

	[Fact]
	public void Test_Class_Map_Factory()
	{
		// Arrange
		IMap map = new Map()
			.Register<UserMapMethods>()
			.Register<SessionMapMethods>();

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
	public void Test_Inline_Map_Factory()
	{
		// Arrange
		IMap map = new Map();
		map.Register<User, UserModel>(static from
			=> new UserModel
			{
				Username = from.Username
			});

		map.Register<Session, SessionModel>(static (map, from)
			=> new SessionModel()
			{
				Id = from.Id,
				User = map.From(from.User).To<UserModel>()
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
	public void Test_Custom_Map_Factory_when_alwaysUseFactory()
	{
		// Arrange
		IMap map = new Map();
		map.Register<User, UserModel>((from, to) => { });
		map.RegisterFactory(type =>
		{
			if (type == typeof(UserModel))
			{
				return new UserModel()
				{
					Username = "Factory set username"
				};
			}

			throw new InvalidOperationException("Unknown type: " + type);
		}, alwaysUseFactory: true);

		// Act
		var result = map.From(new User() { Username = "test" }).To<UserModel>();

		// Assert
		Assert.Equal("Factory set username", result.Username);
	}

	[Fact]
	public void Test_Custom_Map_Factory_when_not_alwaysUseFactory()
	{
		// Arrange
		IMap map = new Map();
		map.Register<User, UserModel>((from, to) => { });
		map.RegisterFactory(type =>
		{
			if (type == typeof(UserModel))
			{
				return new UserModel()
				{
					Username = "Factory set username"
				};
			}

			throw new InvalidOperationException("Unknown type: " + type);
		});

		// Act
		var result = map.From(new User() { Username = "test" }).To<UserModel>();

		// Assert
		Assert.NotEqual("Factory set username", result.Username);
	}

	[Fact]
	public void Test_Interface_Factory()
	{
		// Arrange
		bool factoryCalled = false;
		IMap map = new Map();

		void ClassToInterface(TitleAndDescription from, ITitleAndDescription to)
		{
			to.Title = from.Title;
			to.Description = from.Description;
		}

		object TitleAndDescriptionFactory(Type type)
		{
			factoryCalled = true;

			if (type == typeof(ITitleAndDescription))
			{
				return new TitleAndDescription();
			}

			throw new NotSupportedException();
		}

		map.Register(ClassToInterface);
		map.RegisterFactory(TitleAndDescriptionFactory, alwaysUseFactory: true);

		// Act & Assert
		ITitleAndDescription result = map
			.From(new TitleAndDescription()
			{
				Title = "My Title",
				Description = "My Description"
			})
			.To<ITitleAndDescription>();

		// Assert
		Assert.True(factoryCalled);
		Assert.Equal("My Title", result.Title);
		Assert.Equal("My Description", result.Description);
	}
}
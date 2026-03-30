using Najlot.Map.Attributes;

namespace Najlot.Map.SourceGenerator.Tests;

public class PostMapTests
{
	[Fact]
	public void Test_Post_Map_instance_mapping()
	{
		var map = new Map().Register<PostMapMappings>();
		var source = new TestUserModel
		{
			Id = 7,
			Name = "Ada",
			Email = "ada@example.com"
		};
		var target = new TestUserViewModel(new TestUserService())
		{
			NotifyEnabled = false,
			Email = string.Empty
		};

		map.From(source).To(target);

		Assert.Equal(source.Id, target.Id);
		Assert.Equal(source.Name, target.Name);
		Assert.Equal("post@map.email", target.Email);
		Assert.True(target.NotifyEnabled);
	}

	[Fact]
	public void Test_Post_Map_instance_factory()
	{
		var map = new Map()
			.Register<PostMapMappings>()
			.RegisterFactory(type =>
			{
				if (type == typeof(TestUserViewModel))
				{
					return new TestUserViewModel(new TestUserService());
				}

				throw new InvalidOperationException($"No factory registered for type {type.FullName}");
			});
		var source = new TestUser
		{
			Id = 5,
			Name = "Grace",
			Email = "grace@example.com"
		};

		var result = map.From(source).To<TestUserViewModel>();

		Assert.Equal(source.Id, result.Id);
		Assert.Equal(source.Name, result.Name);
		Assert.Equal("post@map.email", result.Email);
		Assert.True(result.NotifyEnabled);
	}

	[Fact]
	public void Test_Post_Map_static_mapping()
	{
		var map = new Map().Register<PostMapMappings>();
		var source = new TestUserViewModel(new TestUserService())
		{
			Id = 11,
			Name = "Linus",
			Email = "linus@example.com"
		};

		var result = map.From(source).To<TestUser>();

		Assert.Equal(source.Id, result.Id);
		Assert.Equal(source.Name, result.Name);
		Assert.Equal("post@map.email", result.Email);
	}

	[Fact]
	public void Test_Post_Map_static_factory()
	{
		var map = new Map().Register<PostMapMappings>();
		var source = new TestUserModel
		{
			Id = 13,
			Name = "Margaret",
			Email = "margaret@example.com"
		};

		var result = map.From(source).To<TestUser>();

		Assert.Equal(source.Id, result.Id);
		Assert.Equal(source.Name, result.Name);
		Assert.Equal("post@map.email", result.Email);
	}
}

[Mapping]
public partial class PostMapMappings
{
	[MapIgnoreProperty(nameof(to.NotifyEnabled))]
	[MapIgnoreProperty(nameof(to.Email))]
	[PostMap(nameof(PostMapToViewModel))]
	public partial void MapToViewModel(IMap map, TestUserModel from, TestUserViewModel to);

	private void PostMapToViewModel(TestUserModel from, TestUserViewModel to)
	{
		to.Email = "post@map.email";
		to.NotifyEnabled = true;
	}

	[MapIgnoreProperty(nameof(TestUserViewModel.NotifyEnabled))]
	[PostMap(nameof(PostMapToViewModel))]
	public partial TestUserViewModel ToViewModel(IMap map, TestUser from);

	private void PostMapToViewModel(IMap map, TestUser from, TestUserViewModel to)
	{
		to.Email = "post@map.email";
		to.NotifyEnabled = true;
	}

	[PostMap(nameof(PostMapToContract))]
	public static partial TestUser MapToContract(IMap map, TestUserViewModel from);

	private static void PostMapToContract(IMap map, TestUserViewModel from, TestUser to)
	{
		to.Email = "post@map.email";
	}

	private static DateTime MapDateRegistered(DateTimeOffset from) => from.UtcDateTime;

	private static TestUserStatus MapStatus(TestUserModelStatus from) => from switch
	{
		TestUserModelStatus.Active => TestUserStatus.Active,
		TestUserModelStatus.Inactive => TestUserStatus.Inactive,
		TestUserModelStatus.Banned => TestUserStatus.Banned,
		_ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
	};

	public static partial TestUserFeature MapFeature(TestUserFeatureModel from);

	[MapIgnoreProperty(nameof(TestUser.DateRegistered))]
	[MapIgnoreProperty(nameof(TestUser.Status))]
	[PostMap(nameof(PostMapToContract))]
	public static partial TestUser MapToContract(IMap map, TestUserModel from);

	private static void PostMapToContract(TestUserModel from, TestUser to)
	{
		to.Email = "post@map.email";
	}
}
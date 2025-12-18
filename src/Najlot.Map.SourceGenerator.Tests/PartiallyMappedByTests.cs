using Najlot.Map.Attributes;

namespace Najlot.Map.SourceGenerator.Tests;

public class PartiallyMappedByTests
{
	[Fact]
	public void Test_Partial_Class_With_Mapping_Attribute_Generates_Map_Methods()
	{
		// Arrange
		var registered = new DateTimeOffset(2025, 10, 12, 14, 30, 0, TimeSpan.Zero);

		var source = new TestUserModel
		{
			Id = 1,
			Name = "Test User",
			Email = "test@example.com",
		};

		var userService = new TestUserService();

		var map = new Map()
			.Register<TestUserViewModelMappings>()
			.RegisterFactory(t =>
			{
				if (t == typeof(TestUserViewModel)) return new TestUserViewModel(userService);
				throw new InvalidOperationException($"No factory registered for type {t.FullName}");
			});

		// Act
		var target = map.From(source).To<TestUserViewModel>();

		// Assert
		Assert.Equal(1, target.Id);
		Assert.Equal("Test User", target.Name);
		Assert.Equal("test@example.com", target.Email);
		Assert.True(target.NotifyEnabled);
	}

	[Fact]
	public void MapShouldBeValid()
	{
		// Arrange
		var map = new Map().Register<TestUserViewModelMappings>();

		// Act: Validate should not throw exception
		map.Validate();
	}
}

[Mapping]
public partial class TestUserViewModelMappings
{
	[MapIgnoreProperty(nameof(to.NotifyEnabled))]
	private partial void GeneratedMapToViewModel(IMap map, TestUserModel from, TestUserViewModel to);
	public void MapToViewModel(IMap map, TestUserModel from, TestUserViewModel to)
	{
		using (to.StopNotify()) // Prevent NotifyEnabled property change notifications during mapping
		{
			GeneratedMapToViewModel(map, from, to);
		}
	}

	[MapIgnoreProperty(nameof(TestUserViewModel.NotifyEnabled))]
	public partial TestUserViewModel TestUserModelToTestUserViewModel(IMap map, TestUser from);
}

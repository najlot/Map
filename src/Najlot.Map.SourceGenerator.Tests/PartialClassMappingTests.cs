using Najlot.Map.Attributes;
using Najlot.Map.SourceGenerator;

namespace Najlot.Map.SourceGenerator.Tests;

/// <summary>
/// Tests for partial class mapping generation.
/// </summary>
public class PartialClassMappingTests
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
			DateRegistered = registered,
			Address = new TestUserAddressModel
			{
				Street = "Main St",
				HouseNumber = 123,
				City = "Testville",
				ZipCode = "12345"
			},
			Features =
			[
				new TestUserFeatureModel
				{
					FeatureCode = "F001",
					FeatureName = "Feature 1"
				},
				new TestUserFeatureModel
				{
					FeatureCode = "F002",
					FeatureName = "Feature 2"
				}
			]
		};

		var sessionId = Guid.NewGuid();
		var target = new TestUser() { CurrentSessionId = sessionId };

		var map = new Map()
			.Register<UserMappings>()
			.RegisterFactory(t =>
			{
				if (t == typeof(TestUser)) return new TestUser();
				if (t == typeof(TestUserFeature)) return new TestUserFeature();
				if (t == typeof(TestUserAddress)) return new TestUserAddress();

				throw new InvalidOperationException($"No factory registered for type {t.FullName}");
			});

		// Act
		map.From(source).To(target);

        // Assert
        Assert.Equal(1, target.Id);
        Assert.Equal("Test User", target.Name);
        Assert.Equal("test@example.com", target.Email);
        Assert.Equal(2, target.Features.Count);
        Assert.Equal("F001", target.Features[0].FeatureCode);
        Assert.Equal("Feature 1", target.Features[0].FeatureName);
        Assert.Equal("F002", target.Features[1].FeatureCode);
        Assert.Equal("Feature 2", target.Features[1].FeatureName);
		Assert.NotNull(target.Address);
		Assert.Equal("Main St", target.Address!.Street);
		Assert.Equal(123, target.Address.HouseNumber);
		Assert.Equal("Testville", target.Address.City);
		Assert.Equal("12345", target.Address.ZipCode);
		Assert.Equal("12345", target.Address.ZipCode);
		Assert.Equal(sessionId, target.CurrentSessionId);
		Assert.Equal(registered.UtcDateTime, target.DateRegistered);
	}

	[Fact]
	public void MapShouldBeValid()
	{
		// Arrange
		var map = new Map().Register<UserMappings>();

		// Act: Validate should not throw exception
		map.Validate();
	}
}

/// <summary>
/// Test class with Mapping attribute for source generator.
/// </summary>
[Mapping]
public partial class UserMappings
{
	[MapIgnoreProperty(nameof(to.CurrentSessionId))]
	public partial void MapFrom(IMap map, TestUserModel from, TestUser to);
	public partial void MapFrom(IMap map, TestUserFeatureModel from, TestUserFeature to);
	public partial void MapFrom(IMap map, TestUserAddressModel from, TestUserAddress to);

	// Additional mapping method for DateTimeOffset to DateTime
	public DateTime MapFrom(DateTimeOffset from) => from.UtcDateTime;
}
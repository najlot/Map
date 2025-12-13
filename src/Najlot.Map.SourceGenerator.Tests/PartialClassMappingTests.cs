using Najlot.Map.SourceGenerator;

namespace Najlot.Map.SourceGenerator.Tests;

/// <summary>
/// Tests for partial class mapping generation.
/// </summary>
public class PartialClassMappingTests
{
    [Fact]
    public void Test_Partial_Class_With_Mapping_Attribute_Generates_MapFrom_Method()
    {
        // Arrange
        var source = new TestUserModel
		{
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
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

        var target = new TestUser();

		var map = new Map()
			.Register<UserMappings>()
			.RegisterFactory(t =>
			{
				if (t == typeof(TestUser)) return new TestUser();
				if (t == typeof(TestUserFeature)) return new TestUserFeature();

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
	public partial void MapFrom(IMap map, TestUserModel from, TestUser to);
	public partial void MapFrom(IMap map, TestUserFeatureModel from, TestUserFeature to);
}

public class TestUser
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;

	public List<TestUserFeature> Features { get; set; } = [];
}

public class TestUserFeature
{
	public string FeatureCode { get; set; } = string.Empty;
	public string FeatureName { get; set; } = string.Empty;
}

public class TestUserModel
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;

	public List<TestUserFeatureModel> Features { get; set; } = [];
}

public class TestUserFeatureModel
{
	public string FeatureCode { get; set; } = string.Empty;
	public string FeatureName { get; set; } = string.Empty;
}
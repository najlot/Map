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
        var source = new TestUser
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com"
        };

        var target = new TestUser();

        // Act
        target.MapFrom(source);

        // Assert
        Assert.Equal(1, target.Id);
        Assert.Equal("Test User", target.Name);
        Assert.Equal("test@example.com", target.Email);
    }

    [Fact]
    public void Test_Partial_Class_MapFrom_Throws_On_Null()
    {
        // Arrange
        var target = new TestUser();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => target.MapFrom(null!));
    }

    [Fact]
    public void Test_Partial_Class_MapFrom_Copies_All_Properties()
    {
        // Arrange
        var source = new TestUser
        {
            Id = 42,
            Name = "John Doe",
            Email = "john@example.com"
        };

        var target = new TestUser
        {
            Id = 0,
            Name = "",
            Email = ""
        };

        // Act
        target.MapFrom(source);

        // Assert
        Assert.Equal(source.Id, target.Id);
        Assert.Equal(source.Name, target.Name);
        Assert.Equal(source.Email, target.Email);
    }
}

/// <summary>
/// Test class with Mapping attribute for source generator.
/// </summary>
[Mapping]
public partial class TestUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

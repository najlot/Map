using Najlot.Map.SourceGenerator;

namespace Najlot.Map.SourceGenerator.Tests;

/// <summary>
/// Tests for partial method mapping generation.
/// </summary>
public class PartialMethodMappingTests
{
    [Fact]
    public void Test_Partial_Method_With_Mapping_Attribute_Maps_Properties()
    {
        // Arrange
        var mapper = new TestMapper();
        var source = new SourceModel
        {
            Id = 123,
            Name = "Test Name",
            Description = "Test Description"
        };

        // Act
        var result = mapper.MapToTarget(source);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(123, result.Id);
        Assert.Equal("Test Name", result.Name);
        Assert.Equal("Test Description", result.Description);
    }

    [Fact]
    public void Test_Partial_Method_Creates_New_Instance()
    {
        // Arrange
        var mapper = new TestMapper();
        var source = new SourceModel
        {
            Id = 456,
            Name = "Another Name",
            Description = "Another Description"
        };

        // Act
        var result1 = mapper.MapToTarget(source);
        var result2 = mapper.MapToTarget(source);

        // Assert
        Assert.NotSame(result1, result2);
    }
}

/// <summary>
/// Test mapper class with partial method for source generator.
/// </summary>
public partial class TestMapper
{
    [Mapping]
    public partial TargetModel MapToTarget(SourceModel source);
}

/// <summary>
/// Source model for testing.
/// </summary>
public class SourceModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Target model for testing.
/// </summary>
public class TargetModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

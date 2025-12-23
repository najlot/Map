using Najlot.Map.Attributes;
using System.Linq.Expressions;

namespace Najlot.Map.SourceGenerator.Tests;

public class ExpressionProjectionMappingTests
{
	[Fact]
	public void Test_Expression_Projection_Method_Is_Generated()
	{
		var expr = ProjectionMappings.UserModelToUserListItem();
		Assert.NotNull(expr);

		var source = new ProjectionUserModel { Id = 123, Name = "Alice", Ignored = "x" };
		var compiled = expr.Compile();
		var result = compiled(source);

		Assert.Equal(123, result.Id);
		Assert.Equal("Alice", result.Name);
	}
}

public class ProjectionUserModel
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Ignored { get; set; } = string.Empty;
}

public class ProjectionUserListItem
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
}

[Mapping]
public partial class ProjectionMappings
{
	public static partial Expression<Func<ProjectionUserModel, ProjectionUserListItem>> UserModelToUserListItem();
}

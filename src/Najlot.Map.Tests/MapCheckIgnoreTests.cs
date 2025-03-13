using Najlot.Map.Attributes;
using Najlot.Map.Exceptions;
using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapCheckIgnoreTests
{
	internal class EmptyMapMethods
	{
		public static void SimpleMapToModel(EmptyClass from, UserModel to)
		{
		}
	}

	internal class IgnoreMapMethods
	{
		[MapIgnoreProperty(nameof(to.Id))]
		[MapIgnoreProperty(nameof(to.User))]
		public static void IgnoreMapToModel(EmptyClass from, SessionModel to)
		{
		}

		[MapIgnoreMethod]
		public static void IgnoreMethod(Session from, SessionModel to)
		{
		}
	}

	[Fact]
	public void Map_Check_Must_Recognize_Ignore()
	{
		// Arrange
		IMap map = new Map();

		map.Register<IgnoreMapMethods>();

		// Act & Assert
		map.Validate();
	}

	[Fact]
	public void Map_Check_Must_Suggest_Ignore()
	{
		// Arrange
		IMap map = new Map();

		map.Register<EmptyMapMethods>();

		// Act & Assert
		var exception = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains($"[MapIgnoreProperty(nameof(to.Username))]", exception.Message);
	}
}
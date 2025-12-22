using Najlot.Map.Exceptions;
using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapCheckClassTests
{
	internal class ValidUserMapMethods
	{
		public void MapModelToUser(UserModel from, User to)
		{
			to.Username = from.Username;
		}
	}

	internal class InvalidUserMapMethods
	{
		public void MapModelToUser(UserModel from, User to)
		{
		}
	}

	internal class ValidSessionMapMethods
	{
		public static void MapSessionToModel(IMap map, Session from, SessionModel to)
		{
			to.Id = from.Id;
			if (from.User != null)
			{
				to.User = map.From(from.User).To<UserModel>();
			}
		}
	}

	internal class InvalidSessionMapMethods
	{
		public static void MapSessionToModel(IMap map, Session from, SessionModel to)
		{ }
	}

	internal struct StructMapMethods
	{
		public static StructData CreateAndMapIntToStructData(int value) => new StructData
		{
			Value = value
		};

		public static void MapIntToStructData(int value, StructData data)
		{
			data.Value = value;
		}
	}

	internal class InvalidStructMapMethods
	{
		public static void MapIntToStructData(int value, StructData data)
		{
		}
	}

	internal class InvalidStructMapFactoryMethods
	{
		public static StructData CreateAndMapIntToStructData(int value) => new StructData
		{
		};
	}

	internal class InvalidInterfaceMapMethods
	{
		public static void ClassToInterface(TitleAndDescription from, ITitleAndDescription to)
		{
		}
	}

	internal class InterfaceMapMethods
	{
		public static void ClassToInterface(TitleAndDescription from, ITitleAndDescription to)
		{
			to.Title = from.Title;
			to.Description = from.Description;
		}
	}

	internal class MapMethodsWithConstructor
	{
		public MapMethodsWithConstructor(bool something)
		{
		}
	}

	internal class MapMethodsWithParameterlessConstructor
	{
		public static bool ContructorCalled { get; private set; } = false;

		public MapMethodsWithParameterlessConstructor()
		{
			ContructorCalled = true;
		}
	}

	[Fact]
	public void Register_Should_Invoke_Parameterless_Constructor()
	{
		// Arrange
		IMap map = new Map();

		// Act
		Assert.False(MapMethodsWithParameterlessConstructor.ContructorCalled);
		map.Register<MapMethodsWithParameterlessConstructor>();

		// Assert
		Assert.True(MapMethodsWithParameterlessConstructor.ContructorCalled);
	}

	[Fact]
	public void Register_Should_Throw_Exception_With_Constructor_Parameters()
	{
		// Arrange
		IMap map = new Map();

		// Act & Assert
		Assert.Throws<MissingMethodException>(() => map.Register<MapMethodsWithConstructor>());
	}

	[Fact]
	public void Map_Check_Must_Call_Interface_Factory()
	{
		// Arrange
		IMap map = new Map();

		map.Register<InterfaceMapMethods>();

		// Act & Assert
		map.Validate();
	}

	[Fact]
	public void Map_Check_Must_Recognize_Invalid_Map_Interface()
	{
		// Arrange
		IMap map = new Map();

		map.Register<InvalidInterfaceMapMethods>();

		// Act & Assert
		var exception = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains($"{nameof(InvalidInterfaceMapMethods)}.{nameof(InvalidInterfaceMapMethods.ClassToInterface)}", exception.Message);
		Assert.Contains(nameof(ITitleAndDescription.Title), exception.Message);
		Assert.Contains(nameof(ITitleAndDescription.Description), exception.Message);
	}

	[Fact]
	public void Map_Check_Must_Recognize_Invalid_Map_Struct()
	{
		// Arrange
		IMap map = new Map();

		map.Register<InvalidStructMapMethods>();

		// Act & Assert
		var exception = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains(nameof(StructData.Value), exception.Message);
	}

	[Fact]
	public void Map_Check_Must_Recognize_Invalid_Map_Factory_Struct()
	{
		// Arrange
		IMap map = new Map();

		map.Register<InvalidStructMapFactoryMethods>();

		// Act & Assert
		var exception = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains(nameof(StructData.Value), exception.Message);
	}

	[Fact]
	public void Map_Check_Must_Recognize_Map_Struct()
	{
		// Arrange
		IMap map = new Map();

		map.Register<StructMapMethods>();

		// Act & Assert
		map.Validate();
	}

	[Fact]
	public void Map_Check_Must_Recognize_Simple_Map_From_Class()
	{
		// Arrange
		IMap map = new Map();

		map.Register<ValidUserMapMethods>();

		// Act & Assert
		map.Validate();
	}

	[Fact]
	public void Map_Check_Must_Recognize_Map_From_Class()
	{
		// Arrange
		IMap map = new Map();

		map.Register<ValidSessionMapMethods>();

		// Act & Assert
		map.Validate();
	}

	[Fact]
	public void Map_Check_Must_Recognize_Missing_Simple_Map_From_Class()
	{
		// Arrange
		IMap map = new Map();

		map.Register<InvalidUserMapMethods>();

		// Act & Assert
		var exception = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains(nameof(UserModel.Username), exception.Message);
	}

	[Fact]
	public void Map_Check_Must_Recognize_Missing_Map_From_Class()
	{
		// Arrange
		IMap map = new Map();

		map.Register<InvalidSessionMapMethods>();

		// Act & Assert
		var exception = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains(nameof(SessionModel.Id), exception.Message);
		Assert.Contains(nameof(SessionModel.User), exception.Message);
	}
}
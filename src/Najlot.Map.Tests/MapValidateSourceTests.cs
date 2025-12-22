using Najlot.Map.Attributes;
using Najlot.Map.Exceptions;
using Najlot.Map.Tests.TestTypes;

namespace Najlot.Map.Tests;

public class MapValidateSourceTests
{
	internal class ValidateSourceMethods
	{
		[MapValidateSource] // EmptyClass has no properties, but this should not throw error
		public static void SimpleMapToModel(EmptyClass from, UserModel to)
		{
		}

		[MapValidateSource] // IsPaid should not throw error because it's missing in source
		public static void MapToModel(IMap map, Invoice from, InvoiceModel to)
		{
			to.Id = from.Id;
			to.Recipient = from.Recipient;
			to.Amount = from.Amount;
		}
	}

	internal class InvalidValidateSourceMethods
	{
		[MapValidateSource] // This should throw an error
		public static void SimpleMapToModel(UserModel from, EmptyClass to)
		{
		}

		[MapValidateSource] // This should throw an error
		public static void MapToModel(IMap map, InvoiceModel from, Invoice to)
		{
			to.Id = from.Id;
			to.Recipient = from.Recipient;
			to.Amount = from.Amount;
		}
	}

	internal class IgnoredValidateSourceMethods
	{
		[MapValidateSource]
		[MapIgnoreProperty(nameof(from.Empty))]
		[MapIgnoreProperty(nameof(from.LowUsername))]
		[MapIgnoreProperty(nameof(from.Username))]
		public static void SimpleMapToModel(UserModel from, EmptyClass to)
		{
		}

		[MapValidateSource]
		[MapIgnoreProperty(nameof(from.IsPaid))]
		public static void MapToModel(IMap map, InvoiceModel from, Invoice to)
		{
			to.Id = from.Id;
			to.Recipient = from.Recipient;
			to.Amount = from.Amount;
		}
	}

	[Fact]
	public void Map_Check_Must_Recognize_Validate_Source()
	{
		// Arrange
		IMap map = new Map();

		map.Register<ValidateSourceMethods>();

		// Act & Assert
		map.Validate();
	}

	[Fact]
	public void Map_Check_Must_Recognize_Validate_Source_errors()
	{
		// Arrange
		IMap map = new Map();

		map.Register<InvalidValidateSourceMethods>();

		// Act & Assert
		var ex = Assert.Throws<MapMissPropertiesException>(() => map.Validate());
		Assert.Contains("InvalidValidateSourceMethods.SimpleMapToModel", ex.Message);
		Assert.Contains("InvalidValidateSourceMethods.MapToModel", ex.Message);
	}

	[Fact]
	public void Map_Check_Must_Recognize_Validate_Ignore_Source_errors()
	{
		// Arrange
		IMap map = new Map();

		map.Register<IgnoredValidateSourceMethods>();

		// Act & Assert
		map.Validate();
	}
}

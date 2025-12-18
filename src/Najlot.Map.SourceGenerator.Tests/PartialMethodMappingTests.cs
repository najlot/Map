using Najlot.Map.Attributes;
using Najlot.Map.SourceGenerator;

namespace Najlot.Map.SourceGenerator.Tests;

/// <summary>
/// Tests for partial method mapping generation.
/// </summary>
public class PartialMethodMappingTests
{
	[Fact]
	public void Test_Partial_Methods_With_Mapping_Attribute_Generates_Map_Method()
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
			.Register<UserMappingMethods>()
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
		Assert.Equal(sessionId, target.CurrentSessionId);
		Assert.Equal(registered.UtcDateTime, target.DateRegistered);
	}

	[Fact]
	public void Test_Booking_Mappings()
	{
		// Arrange
		var source = new TestBookingRecord
		{
			Id = Guid.NewGuid(),
			Currency = "EUR",
			TotalAmount = 123.45,
			Positions =
			[
				new TestBookingPosition
				{
					Id = 1,
					Description = "Pos 1",
					Price = 10.0,
					Quantity = 1
				}
			]
		};

		var map = new Map().Register<BookingMappingMethods>();

		// Act
		var result = map.From(source).To<CreateTestBookingRecord>();

		// Assert
		Assert.Equal(source.Id, result.Id);
		Assert.Equal(source.Currency, result.Currency);
		Assert.Equal(source.TotalAmount, result.TotalAmount);
		Assert.Single(result.Positions);
		Assert.Equal(source.Positions[0].Id, result.Positions[0].Id);

		// Test Update mapping
		var posSource = source.Positions[0];
		
		// Since MapToUpdate returns a new object, we use To<T>()
		var posTarget = map.From(posSource).To<TestBookingPositionUpdate>();

		Assert.Equal(posSource.Id, posTarget.Id);
		Assert.Equal(posSource.Description, posTarget.Description);
		Assert.Equal(posSource.Price, posTarget.Price);
		Assert.Equal(posSource.Quantity, posTarget.Quantity);
	}

	[Fact]
	public void MapShouldBeValid()
	{
		// Arrange
		var map = new Map()
			.Register<UserMappingMethods>()
			.Register<BookingMappingMethods>();

		// Act: Validate should not throw exception
		map.Validate();
	}
}

/// <summary>
/// Test mapper class with partial method for source generator.
/// </summary>
public partial class UserMappingMethods
{
	[Mapping]
	[MapIgnoreProperty(nameof(to.CurrentSessionId))]
	public partial void UserModelToUser(IMap map, TestUserModel from, TestUser to);

	[Mapping]
	public partial void FeatureModelToFeature(TestUserFeatureModel from, TestUserFeature to);

	[Mapping]
	public static partial void AddressModelToAddress(IMap map, TestUserAddressModel from, TestUserAddress to);

	// Additional mapping method for DateTimeOffset to DateTime
	public DateTime MapOffsetToUtcDateTime(DateTimeOffset from) => from.UtcDateTime;
}

public partial class BookingMappingMethods
{
	[Mapping]
	public partial CreateTestBookingRecord MapToCreate(TestBookingRecord from);

	[Mapping]
	public partial TestBookingPositionUpdate MapToUpdate(IMap map, TestBookingPosition from);
}
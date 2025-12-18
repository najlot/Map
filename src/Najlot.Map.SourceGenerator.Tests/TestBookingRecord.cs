namespace Najlot.Map.SourceGenerator.Tests;

public class TestBookingRecord
{
	public Guid Id { get; set; }
	public string Currency { get; set; } = string.Empty;
	public double TotalAmount { get; set; }
	public List<TestBookingPosition> Positions { get; set; } = [];
}

public class CreateTestBookingRecord
{
	public Guid Id { get; set; }
	public string Currency { get; set; } = string.Empty;
	public double TotalAmount { get; set; }
	public List<TestBookingPosition> Positions { get; set; } = [];
}

public class TestBookingPosition
{
	public long Id { get; set; }
	public string Description { get; set; } = string.Empty;
	public double Price { get; set; }
	public int Quantity { get; set; }
}

public class TestBookingPositionUpdate
{
	public long Id { get; set; }
	public string Description { get; set; } = string.Empty;
	public double Price { get; set; }
	public int Quantity { get; set; }
}

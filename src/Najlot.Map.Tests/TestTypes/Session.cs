namespace Najlot.Map.Tests.TestTypes;

internal struct StructData
{
	public int Value { get; set; }
}

internal class Session
{
	public Guid Id { get; set; } = Guid.Empty;
	public User? User { get; set; }

	public static Session Empty => new();

	public string IdString => Id.ToString();

	public static Guid StaticGuid { get; set; }
}
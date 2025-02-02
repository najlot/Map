namespace Najlot.Map.Tests.TestTypes;

internal class SessionModel
{
	public Guid Id { get; set; } = Guid.Empty;
	public UserModel? User { get; set; }

	public static SessionModel Empty => new();

	public string IdString => Id.ToString();

	public static Guid StaticGuid { get; set; }
}
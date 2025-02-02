namespace Najlot.Map.Tests.TestTypes;

internal class UserModel
{
	public string Username { get; set; } = string.Empty;

	public static UserModel Empty => new();

	public string LowUsername => Username.ToLower();

	public static string StaticString { get; set; } = "";
}
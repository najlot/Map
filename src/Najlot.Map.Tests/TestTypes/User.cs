namespace Najlot.Map.Tests.TestTypes;

internal class User
{
	public string Username { get; set; } = string.Empty;

	public static User Empty => new();

	public string UsernameLow => Username.ToLower();

	public static string StaticString { get; set; } = "";
}

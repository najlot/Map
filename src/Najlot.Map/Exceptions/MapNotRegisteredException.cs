namespace Najlot.Map.Exceptions;

/// <summary>
/// Exception thrown when map is not registered.
/// </summary>
public sealed class MapNotRegisteredException : Exception
{
	public MapNotRegisteredException() : base()
	{
	}

	public MapNotRegisteredException(Type from)
		: base($"Map from {from.FullName} is not registered.")
	{
	}

	public MapNotRegisteredException(Type from, Type to)
		: base($"Map from {from.FullName} to {to.FullName} is not registered.")
	{
	}

	public MapNotRegisteredException(Type from, Type to, Exception innerException)
		: base($"Map from {from.FullName} to {to.FullName} is not registered.", innerException)
	{
	}
}
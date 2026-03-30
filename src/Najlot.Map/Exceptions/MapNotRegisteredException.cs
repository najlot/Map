namespace Najlot.Map.Exceptions;

/// <summary>
/// Exception thrown when map is not registered.
/// </summary>
public sealed class MapNotRegisteredException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MapNotRegisteredException"/> class.
	/// </summary>
	public MapNotRegisteredException() : base()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MapNotRegisteredException"/> class for an unregistered source type.
	/// </summary>
	/// <param name="from">Source type that is not registered.</param>
	public MapNotRegisteredException(Type from)
		: base($"Map from {from.FullName} is not registered.")
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MapNotRegisteredException"/> class for an unregistered source and destination pair.
	/// </summary>
	/// <param name="from">Source type that is not registered.</param>
	/// <param name="to">Destination type that is not registered.</param>
	public MapNotRegisteredException(Type from, Type to)
		: base($"Map from {from.FullName} to {to.FullName} is not registered.")
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MapNotRegisteredException"/> class for an unregistered source and destination pair.
	/// </summary>
	/// <param name="from">Source type that is not registered.</param>
	/// <param name="to">Destination type that is not registered.</param>
	/// <param name="innerException">The exception that caused the current exception.</param>
	public MapNotRegisteredException(Type from, Type to, Exception innerException)
		: base($"Map from {from.FullName} to {to.FullName} is not registered.", innerException)
	{
	}
}
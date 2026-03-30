namespace Najlot.Map.Exceptions;

/// <summary>
/// Exception thrown when mapping validation detects unmapped properties.
/// </summary>
public sealed class MapMissPropertiesException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MapMissPropertiesException"/> class.
	/// </summary>
	public MapMissPropertiesException() : base()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MapMissPropertiesException"/> class with a validation message.
	/// </summary>
	/// <param name="message">Validation failure details.</param>
	public MapMissPropertiesException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MapMissPropertiesException"/> class with a validation message and inner exception.
	/// </summary>
	/// <param name="message">Validation failure details.</param>
	/// <param name="innerException">The exception that caused the current exception.</param>
	public MapMissPropertiesException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
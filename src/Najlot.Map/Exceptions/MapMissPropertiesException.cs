namespace Najlot.Map.Exceptions;

public sealed class MapMissPropertiesException : Exception
{
	public MapMissPropertiesException() : base()
	{
	}

	public MapMissPropertiesException(string message)
		: base(message)
	{
	}

	public MapMissPropertiesException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
namespace Najlot.Map;

/// <summary>
/// Maps from a class.
/// </summary>
public interface IMapFrom
{
	/// <summary>
	/// Maps to a new class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T To<T>();

	/// <summary>
	/// Maps to an existing class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="to"></param>
	/// <returns></returns>
	public T To<T>(T to);

	/// <summary>
	/// Maps to an existing nullable class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="to"></param>
	/// <returns></returns>
	public T? ToNullable<T>(T? to);
}
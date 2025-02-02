namespace Najlot.Map;

/// <summary>
/// Maps from a nullable enumerable.
/// </summary>
public interface IMapFromNullableEnumerable
{
	/// <summary>
	/// Maps provided IEnumerable to a new IEnumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public IEnumerable<T?> To<T>();

	/// <summary>
	/// Maps provided IEnumerable to a List.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public List<T?> ToList<T>();

	/// <summary>
	/// Maps provided IEnumerable to an array.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T?[] ToArray<T>();
}

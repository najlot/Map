namespace Najlot.Map;

/// <summary>
/// Maps from an enumerable.
/// </summary>
public interface IMapFromEnumerable
{
	/// <summary>
	/// Maps provided IEnumerable to a new IEnumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public IEnumerable<T> To<T>();

	/// <summary>
	/// Maps provided IEnumerable to a List.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public List<T> ToList<T>();

	/// <summary>
	/// Maps provided IEnumerable to an array.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T[] ToArray<T>();

	/// <summary>
	/// Maps provided IEnumerable into an existing List.
	/// Does not work with map factories and modifies existing items.
	/// </summary>
	/// <typeparam name="T">Type of the elements in the list</typeparam>
	/// <param name="to">List to map into</param>
	/// <returns></returns>
	List<T> ToList<T>(List<T> to);
}

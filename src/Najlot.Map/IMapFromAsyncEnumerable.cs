namespace Najlot.Map;

public interface IMapFromAsyncEnumerable
{
	/// <summary>
	/// Maps provided IAsyncEnumerable to a new IAsyncEnumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public IAsyncEnumerable<T> To<T>();
}
namespace Najlot.Map;

public interface IMapFromNullableAsyncEnumerable
{
	/// <summary>
	/// Maps provided IAsyncEnumerable to a new IAsyncEnumerable of nullable class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public IAsyncEnumerable<T?> To<T>();
}
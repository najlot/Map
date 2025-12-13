namespace Najlot.Map;

/// <summary>
/// Map interface.
/// </summary>
public interface IMap
{
	/// <summary>
	/// Maps from a class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="from"></param>
	/// <returns></returns>
	IMapFrom From<T>(T from);

	/// <summary>
	/// Maps from a nullable class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="from"></param>
	/// <returns></returns>
	IMapFrom? FromNullable<T>(T? from);

	/// <summary>
	/// Maps from an async enumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="from"></param>
	/// <returns></returns>
	IMapFromAsyncEnumerable From<T>(IAsyncEnumerable<T> from);

	/// <summary>
	/// Maps from a nullable async enumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="from"></param>
	/// <returns></returns>
	IMapFromNullableAsyncEnumerable FromNullable<T>(IAsyncEnumerable<T?> from);

	/// <summary>
	/// Maps from an enumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="from"></param>
	/// <returns></returns>
	IMapFromEnumerable From<T>(IEnumerable<T> from);

	/// <summary>
	/// Maps from a nullable enumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="from"></param>
	/// <returns></returns>
	IMapFromNullableEnumerable FromNullable<T>(IEnumerable<T?> from);

	/// <summary>
	/// Registers a map delegate.
	/// </summary>
	/// <typeparam name="TFrom"></typeparam>
	/// <typeparam name="TTo"></typeparam>
	/// <param name="method"></param>
	/// <returns></returns>
	IMap Register<TFrom, TTo>(MapMethod<TFrom, TTo> method);

	/// <summary>
	/// Registers a simple map delegate.
	/// </summary>
	/// <typeparam name="TFrom"></typeparam>
	/// <typeparam name="TTo"></typeparam>
	/// <param name="method"></param>
	/// <returns></returns>
	IMap Register<TFrom, TTo>(SimpleMapMethod<TFrom, TTo> method);

	IMap Register<TFrom, TTo>(SimpleMapFactoryMethod<TFrom, TTo> method);

	IMap Register<TFrom, TTo>(MapFactoryMethod<TFrom, TTo> method);

	/// <summary>
	/// Registers all map delegates in a new class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	IMap Register<T>();

	/// <summary>
	/// Registers all map delegates in a class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="maps"></param>
	/// <returns></returns>
	IMap Register<T>(T maps);

	/// <summary>
	/// Registers a factory method to create object instances.
	/// </summary>
	IMap RegisterFactory(FactoryMethod factory);

	/// <summary>
	/// Gets a registered simple map method for the specified types.
	/// </summary>
	/// <typeparam name="TFrom">Source type</typeparam>
	/// <typeparam name="TTo">Destination type</typeparam>
	/// <returns>The simple map method if registered, otherwise null</returns>
	SimpleMapMethod<TFrom, TTo> GetMethod<TFrom, TTo>();

	/// <summary>
	/// Gets a registered simple map factory method for the specified types.
	/// </summary>
	/// <typeparam name="TFrom">Source type</typeparam>
	/// <typeparam name="TTo">Destination type</typeparam>
	/// <returns>The simple map factory method if registered, otherwise null</returns>
	SimpleMapFactoryMethod<TFrom, TTo> GetFactoryMethod<TFrom, TTo>();

	/// <summary>
	/// Validates map methods and throws an exception if any of them miss some properties.
	/// </summary>
	void Validate();
}
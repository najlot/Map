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
	MapFrom<T> From<T>(T from);

	/// <summary>
	/// Maps from a nullable class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="from"></param>
	/// <returns></returns>
	MapFrom<T>? FromNullable<T>(T? from);

	/// <summary>
	/// Maps from an async enumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="from"></param>
	/// <returns></returns>
	MapFromAsyncEnumerable<T> From<T>(IAsyncEnumerable<T> from);

	/// <summary>
	/// Maps from a nullable async enumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="from"></param>
	/// <returns></returns>
	MapFromNullableAsyncEnumerable<T> FromNullable<T>(IAsyncEnumerable<T?> from);

	/// <summary>
	/// Maps from an enumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="from"></param>
	/// <returns></returns>
	MapFromEnumerable<T> From<T>(IEnumerable<T> from);

	/// <summary>
	/// Maps from a nullable enumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="from"></param>
	/// <returns></returns>
	MapFromNullableEnumerable<T> FromNullable<T>(IEnumerable<T?> from);

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
	/// Registers all public map delegates in a new class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	IMap Register<T>();

	/// <summary>
	/// Registers all public map delegates in a class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="maps"></param>
	/// <returns></returns>
	IMap Register<T>(T maps);

	/// <summary>
	/// Registers a factory method to create object instances.
	/// </summary>
	/// <param name="factory">Factory method to create objects</param>
	/// <param name="alwaysUseFactory">Whether to use the factory method for all object creations. If left false, then internal object factory will be used</param>
	/// <returns>This instance</returns>
	IMap RegisterFactory(FactoryMethod factory, bool alwaysUseFactory = false);

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
	/// Creates a new instance of the specified type.
	/// Uses the registered factory method.
	/// Used by map methods to create destination objects.
	/// </summary>
	/// <typeparam name="T">The type of object to create.</typeparam>
	/// <returns>A new instance of type <typeparamref name="T"/>.</returns>
	T Create<T>();

	/// <summary>
	/// Validates map methods and throws an exception if any of them miss some properties.
	/// </summary>
	void Validate();
}
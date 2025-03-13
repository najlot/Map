using Najlot.Map.Exceptions;

namespace Najlot.Map;

/// <summary>
/// Map class. It is used to map objects from one class to another.
/// </summary>
public partial class Map : IMap
{
	private readonly Dictionary<Type, Dictionary<Type, Delegate>> _mapRegistrations = [];
	private readonly Dictionary<Type, Dictionary<Type, Delegate>> _mapFactoryRegistrations = [];
	private readonly IReadOnlyDictionary<Type, Delegate> _emptyDictionary = new Dictionary<Type, Delegate>();

	private readonly List<Delegate> _mapDelegates = [];
	private readonly List<Delegate> _mapFactoryDelegates = [];

	private FactoryMethod _factory = DefaultFactory;
	private static object DefaultFactory(Type type) => Activator.CreateInstance(type)
		?? throw new NullReferenceException($"Activator.CreateInstance of type {type.FullName} returns null.");

	/// <summary>
	/// Maps from a class.
	/// </summary>
	/// <typeparam name="T">Type of the class</typeparam>
	/// <param name="from">Instance of the class</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered</exception>
	public IMapFrom From<T>(T from)
	{
		return new MapFrom<T>(this, from, _factory,
			_mapRegistrations.TryGetValue(typeof(T), out var registrations) ? registrations : _emptyDictionary,
			_mapFactoryRegistrations.TryGetValue(typeof(T), out var factoryRegistrations) ? factoryRegistrations : _emptyDictionary);
	}

	/// <summary>
	/// Maps from a nullable class.
	/// </summary>
	/// <typeparam name="T">Type of the class</typeparam>
	/// <param name="from">Instance of the class</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered</exception>
	public IMapFrom? FromNullable<T>(T? from)
	{
		if (from is null)
		{
			return null;
		}

		return new MapFrom<T>(this, from, _factory,
			_mapRegistrations.TryGetValue(typeof(T), out var registrations) ? registrations : _emptyDictionary,
			_mapFactoryRegistrations.TryGetValue(typeof(T), out var factoryRegistrations) ? factoryRegistrations : _emptyDictionary);
	}

	/// <summary>
	/// Maps from an enumerable.
	/// </summary>
	/// <typeparam name="T">Type of source class</typeparam>
	/// <param name="from">Enumerable containing source classes</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered.</exception>
	public IMapFromEnumerable From<T>(IEnumerable<T> from)
	{
		return new MapFromEnumerable<T>(this, from, _factory,
			_mapRegistrations.TryGetValue(typeof(T), out var registrations) ? registrations : _emptyDictionary,
			_mapFactoryRegistrations.TryGetValue(typeof(T), out var factoryRegistrations) ? factoryRegistrations : _emptyDictionary);
	}

	/// <summary>
	/// Maps from a nullable enumerable.
	/// </summary>
	/// <typeparam name="T">Type of source class</typeparam>
	/// <param name="from">Enumerable containing source classes</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered.</exception>
	public IMapFromNullableEnumerable FromNullable<T>(IEnumerable<T?> from)
	{
		return new MapFromNullableEnumerable<T>(this, from, _factory,
			_mapRegistrations.TryGetValue(typeof(T), out var registrations) ? registrations : _emptyDictionary,
			_mapFactoryRegistrations.TryGetValue(typeof(T), out var factoryRegistrations) ? factoryRegistrations : _emptyDictionary);
	}

	/// <summary>
	/// Maps from an async enumerable.
	/// </summary>
	/// <typeparam name="T">Type of source class</typeparam>
	/// <param name="from">Async enumerable containing source classes</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered.</exception>
	public IMapFromAsyncEnumerable From<T>(IAsyncEnumerable<T> from)
	{
		return new MapFromAsyncEnumerable<T>(this, from, _factory,
			_mapRegistrations.TryGetValue(typeof(T), out var registrations) ? registrations : _emptyDictionary,
			_mapFactoryRegistrations.TryGetValue(typeof(T), out var factoryRegistrations) ? factoryRegistrations : _emptyDictionary);
	}

	/// <summary>
	/// Maps from a nullable async enumerable.
	/// </summary>
	/// <typeparam name="T">Type of source class</typeparam>
	/// <param name="from">Async enumerable containing source classes</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered.</exception>
	public IMapFromNullableAsyncEnumerable FromNullable<T>(IAsyncEnumerable<T?> from)
	{
		return new MapFromNullableAsyncEnumerable<T>(this, from, _factory,
			_mapRegistrations.TryGetValue(typeof(T), out var registrations) ? registrations : _emptyDictionary,
			_mapFactoryRegistrations.TryGetValue(typeof(T), out var factoryRegistrations) ? factoryRegistrations : _emptyDictionary);
	}

	public IMap Register<TFrom, TTo>(SimpleMapFactoryMethod<TFrom, TTo> method)
	{
		if (method is null)
		{
			throw new ArgumentNullException(nameof(method));
		}

		TTo Map(IMap map, TFrom from) => method(from);

		RegisterFactoryMapInternal<TFrom, TTo>(Map);
		_mapFactoryDelegates.Add(method);
		return this;
	}

	public IMap Register<TFrom, TTo>(MapFactoryMethod<TFrom, TTo> method)
	{
		if (method is null)
		{
			throw new ArgumentNullException(nameof(method));
		}

		RegisterIntoDictionary(_mapFactoryRegistrations, typeof(TFrom), typeof(TTo), method);
		_mapFactoryDelegates.Add(method);
		return this;
	}

	private void RegisterFactoryMapInternal<TFrom, TTo>(MapFactoryMethod<TFrom, TTo> method)
	{
		RegisterIntoDictionary(_mapFactoryRegistrations, typeof(TFrom), typeof(TTo), method);
	}

	/// <summary>
	/// Registers a map delegate.
	/// </summary>
	/// <typeparam name="TFrom">From type</typeparam>
	/// <typeparam name="TTo">To type</typeparam>
	/// <param name="method">Simple map delegate</param>
	/// <returns></returns>
	public IMap Register<TFrom, TTo>(SimpleMapMethod<TFrom, TTo> method)
	{
		if (method is null)
		{
			throw new ArgumentNullException(nameof(method));
		}

		void Map(IMap map, TFrom from, TTo to)
		{
			method(from, to);
		}

		RegisterInternal<TFrom, TTo>(Map);
		_mapDelegates.Add(method);
		return this;
	}

	private void RegisterInternal<TFrom, TTo>(MapMethod<TFrom, TTo> method)
	{
		RegisterIntoDictionary(_mapRegistrations, typeof(TFrom), typeof(TTo), method);
	}

	/// <summary>
	/// Registers a map delegate.
	/// </summary>
	/// <typeparam name="TFrom">From type</typeparam>
	/// <typeparam name="TTo">To type</typeparam>
	/// <param name="method">Map delegate</param>
	/// <returns></returns>
	public IMap Register<TFrom, TTo>(MapMethod<TFrom, TTo> method)
	{
		RegisterIntoDictionary(_mapRegistrations, typeof(TFrom), typeof(TTo), method);
		_mapDelegates.Add(method);
		return this;
	}

	private void RegisterIntoDictionary(
		Dictionary<Type, Dictionary<Type, Delegate>> dictionary,
		Type from,
		Type to,
		Delegate method)
	{
		if (!dictionary.TryGetValue(from, out var registrations))
		{
			registrations = [];
			dictionary[from] = registrations;
		}

		registrations[to] = method;
	}

	/// <summary>
	/// Registers a factory method to create object instances.
	/// </summary>
	/// <param name="factory"></param>
	/// <returns>This instance</returns>
	public IMap RegisterFactory(FactoryMethod factory)
	{
		_factory = factory;
		return this;
	}
}
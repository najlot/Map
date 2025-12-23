using Najlot.Map.Exceptions;
using System.Linq.Expressions;

namespace Najlot.Map;

/// <summary>
/// Map class. It is used to map objects from one class to another.
/// </summary>
public partial class Map : IMap
{
	private class TypeRegistrations
	{
		public Dictionary<Type, Delegate>? Maps;
		public Dictionary<Type, Delegate>? Factories;
		public Dictionary<Type, Expression>? Expressions;
	}

	private readonly Dictionary<Type, TypeRegistrations> _registrations = [];

	private readonly List<Delegate> _mapDelegates = [];
	private readonly List<Delegate> _mapFactoryDelegates = [];

	private FactoryMethod? _factory = null;
	private bool _alwaysUseFactory = false;

	/// <summary>
	/// Creates a new instance of the specified type.
	/// Uses the registered factory method.
	/// Used by map methods to create destination objects.
	/// </summary>
	/// <typeparam name="T">The type of object to create.</typeparam>
	/// <returns>A new instance of type <typeparamref name="T"/>.</returns>
	public T Create<T>()
	{
		if (_factory is not null && (_alwaysUseFactory || !Cache<T>.HasPublicParameterlessCtor))
		{
			return (T)_factory(typeof(T));
		}

		return Cache<T>.Factory();
	}

	/// <summary>
	/// Maps from a class.
	/// </summary>
	/// <typeparam name="T">Type of the class</typeparam>
	/// <param name="from">Instance of the class</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered</exception>
	public MapFrom<T> From<T>(T from)
	{
		if (_registrations.TryGetValue(typeof(T), out var regs))
		{
			return new MapFrom<T>(this, from, regs.Maps, regs.Factories);
		}

		return new MapFrom<T>(this, from, null, null);
	}

	/// <summary>
	/// Maps from a nullable class.
	/// </summary>
	/// <typeparam name="T">Type of the class</typeparam>
	/// <param name="from">Instance of the class</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered</exception>
	public MapFrom<T>? FromNullable<T>(T? from)
	{
		if (from is null)
		{
			return null;
		}

		if (_registrations.TryGetValue(typeof(T), out var regs))
		{
			return new MapFrom<T>(this, from, regs.Maps, regs.Factories);
		}

		return new MapFrom<T>(this, from, null, null);
	}

	/// <summary>
	/// Maps from an enumerable.
	/// </summary>
	/// <typeparam name="T">Type of source class</typeparam>
	/// <param name="from">Enumerable containing source classes</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered.</exception>
	public MapFromEnumerable<T> From<T>(IEnumerable<T> from)
	{
		if (_registrations.TryGetValue(typeof(T), out var regs))
		{
			return new MapFromEnumerable<T>(this, from, regs.Maps, regs.Factories);
		}

		return new MapFromEnumerable<T>(this, from, null, null);
	}

	/// <summary>
	/// Maps from a nullable enumerable.
	/// </summary>
	/// <typeparam name="T">Type of source class</typeparam>
	/// <param name="from">Enumerable containing source classes</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered.</exception>
	public MapFromNullableEnumerable<T> FromNullable<T>(IEnumerable<T?> from)
	{
		if (_registrations.TryGetValue(typeof(T), out var regs))
		{
			return new MapFromNullableEnumerable<T>(this, from, regs.Maps, regs.Factories);
		}

		return new MapFromNullableEnumerable<T>(this, from, null, null);
	}

	/// <summary>
	/// Maps from an async enumerable.
	/// </summary>
	/// <typeparam name="T">Type of source class</typeparam>
	/// <param name="from">Async enumerable containing source classes</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered.</exception>
	public MapFromAsyncEnumerable<T> From<T>(IAsyncEnumerable<T> from)
	{
		if (_registrations.TryGetValue(typeof(T), out var regs))
		{
			return new MapFromAsyncEnumerable<T>(this, from, regs.Maps, regs.Factories);
		}

		return new MapFromAsyncEnumerable<T>(this, from, null, null);
	}

	/// <summary>
	/// Maps from a nullable async enumerable.
	/// </summary>
	/// <typeparam name="T">Type of source class</typeparam>
	/// <param name="from">Async enumerable containing source classes</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered.</exception>
	public MapFromNullableAsyncEnumerable<T> FromNullable<T>(IAsyncEnumerable<T?> from)
	{
		if (_registrations.TryGetValue(typeof(T), out var regs))
		{
			return new MapFromNullableAsyncEnumerable<T>(this, from, regs.Maps, regs.Factories);
		}

		return new MapFromNullableAsyncEnumerable<T>(this, from, null, null);
	}

	/// <summary>
	/// Maps from a queryable.
	/// </summary>
	/// <typeparam name="T">Type of source class</typeparam>
	/// <param name="from">Queryable containing source classes</param>
	/// <returns>Class to specify to which type to map</returns>
	/// <exception cref="MapNotRegisteredException">Thrown when map is not registered.</exception>
	public MapFromQueryable<T> From<T>(IQueryable<T> from)
	{
		if (_registrations.TryGetValue(typeof(T), out var regs))
		{
			return new MapFromQueryable<T>(from, regs.Expressions);
		}

		return new MapFromQueryable<T>(from, null);
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

		RegisterFactoryMapInternal(method);
		_mapFactoryDelegates.Add(method);
		return this;
	}

	public IMap RegisterExpression<TFrom, TTo>(Expression<Func<TFrom, TTo>> expression)
	{
		if (expression is null)
		{
			throw new ArgumentNullException(nameof(expression));
		}

		if (!_registrations.TryGetValue(typeof(TFrom), out var regs))
		{
			regs = new TypeRegistrations();
			_registrations[typeof(TFrom)] = regs;
		}

		regs.Expressions ??= [];
		regs.Expressions[typeof(TTo)] = expression;
		return this;
	}

	private void RegisterFactoryMapInternal<TFrom, TTo>(MapFactoryMethod<TFrom, TTo> method)
	{
		if (!_registrations.TryGetValue(typeof(TFrom), out var regs))
		{
			regs = new TypeRegistrations();
			_registrations[typeof(TFrom)] = regs;
		}

		regs.Factories ??= [];
		regs.Factories[typeof(TTo)] = method;
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
		if (!_registrations.TryGetValue(typeof(TFrom), out var regs))
		{
			regs = new TypeRegistrations();
			_registrations[typeof(TFrom)] = regs;
		}

		regs.Maps ??= [];
		regs.Maps[typeof(TTo)] = method;
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
		RegisterInternal(method);
		_mapDelegates.Add(method);
		return this;
	}

	/// <summary>
	/// Registers a factory method to create object instances.
	/// </summary>
	/// <param name="factory">Factory method to create objects</param>
	/// <param name="alwaysUseFactory">Whether to use the factory method for all object creations. If left false, then internal object factory will be used</param>
	/// <returns>This instance</returns>
	public IMap RegisterFactory(FactoryMethod factory, bool alwaysUseFactory = false)
	{
		_factory = factory;
		_alwaysUseFactory = alwaysUseFactory;
		return this;
	}

	/// <summary>
	/// Gets a registered simple map method for the specified types.
	/// </summary>
	/// <typeparam name="TFrom">Source type</typeparam>
	/// <typeparam name="TTo">Destination type</typeparam>
	/// <returns>The simple map method if registered, otherwise null</returns>
	public SimpleMapMethod<TFrom, TTo> GetMethod<TFrom, TTo>()
	{
		if (_registrations.TryGetValue(typeof(TFrom), out var regs)
			&& regs.Maps != null
			&& regs.Maps.TryGetValue(typeof(TTo), out var registration))
		{
			var method = (MapMethod<TFrom, TTo>)registration;
			return (TFrom from, TTo to) => method(this, from, to);
		}

		throw new MapNotRegisteredException(typeof(TFrom), typeof(TTo));
	}

	/// <summary>
	/// Gets a registered simple map factory method for the specified types.
	/// </summary>
	/// <typeparam name="TFrom">Source type</typeparam>
	/// <typeparam name="TTo">Destination type</typeparam>
	/// <returns>The simple map factory method if registered, otherwise null</returns>
	public SimpleMapFactoryMethod<TFrom, TTo> GetFactoryMethod<TFrom, TTo>()
	{
		if (_registrations.TryGetValue(typeof(TFrom), out var regs))
		{
			if (regs.Factories != null
				&& regs.Factories.TryGetValue(typeof(TTo), out var factoryRegistration))
			{
				var factoryMethod = (MapFactoryMethod<TFrom, TTo>)factoryRegistration;
				return (TFrom from) => factoryMethod(this, from);
			}

			if (regs.Maps != null
				&& regs.Maps.TryGetValue(typeof(TTo), out var registration))
			{
				var method = (MapMethod<TFrom, TTo>)registration;
				return (TFrom from) =>
				{
					var to = Create<TTo>();
					method(this, from, to);
					return to;
				};
			}
		}

		throw new MapNotRegisteredException(typeof(TFrom), typeof(TTo));
	}

	/// <summary>
	/// Gets a registered map expression for the specified types.
	/// </summary>
	/// <typeparam name="TFrom">Source type</typeparam>
	/// <typeparam name="TTo">Destination type</typeparam>
	/// <returns>The map expression if registered, otherwise null</returns>
	public Expression<Func<TFrom, TTo>> GetExpression<TFrom, TTo>()
	{
		if (_registrations.TryGetValue(typeof(TFrom), out var regs)
			&& regs.Expressions != null
			&& regs.Expressions.TryGetValue(typeof(TTo), out var registration))
		{
			return (Expression<Func<TFrom, TTo>>)registration;
		}

		throw new MapNotRegisteredException(typeof(TFrom), typeof(TTo));
	}
}
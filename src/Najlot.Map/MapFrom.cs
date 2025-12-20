using Najlot.Map.Exceptions;

namespace Najlot.Map;

/// <summary>
/// Maps from a class.
/// </summary>
public readonly struct MapFrom<TFrom>(
	Map map,
	TFrom from,
	IReadOnlyDictionary<Type, Delegate>? mapRegistrations,
	IReadOnlyDictionary<Type, Delegate>? mapFactoryRegistrations)
{
	/// <summary>
	/// Maps to a new class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public readonly T To<T>()
	{
		var targetType = typeof(T);

		if (mapFactoryRegistrations != null && mapFactoryRegistrations.TryGetValue(targetType, out var factoryRegistration))
		{
			var factoryMethod = (MapFactoryMethod<TFrom, T>)factoryRegistration;
			return factoryMethod(map, from);
		}

		var t = map.Create<T>();
		return To(t);
	}

	/// <summary>
	/// Maps to an existing class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="to"></param>
	/// <returns></returns>
	public readonly T To<T>(T to)
	{
		if (mapRegistrations == null || !mapRegistrations.TryGetValue(typeof(T), out var registration))
		{
			throw new MapNotRegisteredException(typeof(TFrom), typeof(T));
		}

		var method = (MapMethod<TFrom, T>)registration;
		method(map, from, to);
		return to;
	}

	/// <summary>
	/// Maps to an existing nullable class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="to"></param>
	/// <returns></returns>
	public readonly T? ToNullable<T>(T? to)
	{
		if (to is null)
		{
			return default;
		}

		return To(to);
	}
}
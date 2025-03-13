using Najlot.Map.Exceptions;

namespace Najlot.Map;

internal class MapFrom<TFrom>(
	IMap map,
	TFrom from,
	FactoryMethod factory,
	IReadOnlyDictionary<Type, Delegate> mapRegistrations,
	IReadOnlyDictionary<Type, Delegate> mapFactoryRegistrations) : IMapFrom
{
	public T To<T>()
	{
		var targetType = typeof(T);

		if (mapFactoryRegistrations.TryGetValue(targetType, out var factoryRegistration))
		{
			var factoryMethod = (MapFactoryMethod<TFrom, T>)factoryRegistration;
			return factoryMethod(map, from);
		}

		var t = (T)factory(targetType);
		return To(t);
	}

	public T To<T>(T to)
	{
		if (!mapRegistrations.TryGetValue(typeof(T), out var registration))
		{
			throw new MapNotRegisteredException(typeof(TFrom), typeof(T));
		}

		var method = (MapMethod<TFrom, T>)registration;
		method(map, from, to);
		return to;
	}

	public T? ToNullable<T>(T? to)
	{
		if (to is null)
		{
			return default;
		}

		return To(to);
	}
}
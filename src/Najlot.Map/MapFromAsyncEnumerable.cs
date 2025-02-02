using Najlot.Map.Exceptions;

namespace Najlot.Map;

internal class MapFromAsyncEnumerable<TFrom>(
	IMap map,
	IAsyncEnumerable<TFrom> from,
	FactoryMethod factory,
	IReadOnlyDictionary<Type, Delegate> mapRegistrations,
	IReadOnlyDictionary<Type, Delegate> mapFactoryRegistrations) : IMapFromAsyncEnumerable
{
	public async IAsyncEnumerable<T> To<T>()
	{
		var targetType = typeof(T);

		MapFactoryMethod<TFrom, T> factoryMethod;

		if (mapFactoryRegistrations.TryGetValue(targetType, out var factoryRegistration))
		{
			factoryMethod = (MapFactoryMethod<TFrom, T>)factoryRegistration;
		}
		else
		{
			if (!mapRegistrations.TryGetValue(targetType, out var registration))
			{
				throw new MapNotRegisteredException(typeof(TFrom), targetType);
			}

			var method = (MapMethod<TFrom, T>)registration;

			T CreateAndMap(IMap map, TFrom from)
			{
				var t = (T)factory(targetType);
				method(map, from, t);
				return t;
			}

			factoryMethod = CreateAndMap;
		}

		await foreach (var item in from)
		{
			yield return factoryMethod(map, item);
		}
	}
}

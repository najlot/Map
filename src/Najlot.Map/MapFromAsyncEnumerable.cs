using Najlot.Map.Exceptions;

namespace Najlot.Map;

public readonly struct MapFromAsyncEnumerable<TFrom>(
	Map map,
	IAsyncEnumerable<TFrom> from,
	IReadOnlyDictionary<Type, Delegate>? mapRegistrations,
	IReadOnlyDictionary<Type, Delegate>? mapFactoryRegistrations)
{
	/// <summary>
	/// Maps provided IAsyncEnumerable to a new IAsyncEnumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public async readonly IAsyncEnumerable<T> To<T>()
	{
		var targetType = typeof(T);

		if (mapFactoryRegistrations != null && mapFactoryRegistrations.TryGetValue(targetType, out var factoryRegistration))
		{
			var factoryMethod = (MapFactoryMethod<TFrom, T>)factoryRegistration;
			await foreach (var item in from)
			{
				yield return factoryMethod(map, item);
			}
		}
		else
		{
			if (mapRegistrations == null || !mapRegistrations.TryGetValue(targetType, out var registration))
			{
				throw new MapNotRegisteredException(typeof(TFrom), targetType);
			}

			var method = (MapMethod<TFrom, T>)registration;
			await foreach (var item in from)
			{
				var t = map.Create<T>();
				method(map, item, t);
				yield return t;
			}
		}
	}
}
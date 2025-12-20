using Najlot.Map.Exceptions;

namespace Najlot.Map;

public readonly struct MapFromNullableAsyncEnumerable<TFrom>(
	Map map,
	IAsyncEnumerable<TFrom?> from,
	IReadOnlyDictionary<Type, Delegate>? mapRegistrations,
	IReadOnlyDictionary<Type, Delegate>? mapFactoryRegistrations)
{
	/// <summary>
	/// Maps provided IAsyncEnumerable to a new IAsyncEnumerable of nullable class.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public async readonly IAsyncEnumerable<T?> To<T>()
	{
		var targetType = typeof(T);

		MapFactoryMethod<TFrom, T> factoryMethod;

		if (mapFactoryRegistrations != null && mapFactoryRegistrations.TryGetValue(targetType, out var factoryRegistration))
		{
			factoryMethod = (MapFactoryMethod<TFrom, T>)factoryRegistration;

			await foreach (var item in from)
			{
				if (item is null)
				{
					yield return default;
				}
				else
				{
					yield return factoryMethod(map, item);
				}
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
				if (item is null)
				{
					yield return default;
				}
				else
				{
					var t = map.Create<T>();
					method(map, item, t);
					yield return t;
				}
			}
		}
	}
}
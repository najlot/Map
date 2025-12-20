using Najlot.Map.Exceptions;

namespace Najlot.Map;

/// <summary>
/// Maps from a nullable enumerable.
/// </summary>
public readonly struct MapFromNullableEnumerable<TFrom>(
	Map map,
	IEnumerable<TFrom?> from,
	IReadOnlyDictionary<Type, Delegate>? mapRegistrations,
	IReadOnlyDictionary<Type, Delegate>? mapFactoryRegistrations)
{
	/// <summary>
	/// Maps provided IEnumerable to a new IEnumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public readonly IEnumerable<T?> To<T>()
	{
		var targetType = typeof(T);

		if (mapFactoryRegistrations != null && mapFactoryRegistrations.TryGetValue(targetType, out var factoryRegistration))
		{
			var factoryMethod = (MapFactoryMethod<TFrom, T>)factoryRegistration;

			foreach (var item in from)
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

			foreach (var item in from)
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

	/// <summary>
	/// Maps provided IEnumerable to an array.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T?[] ToArray<T>()
	{
		return To<T>().ToArray();
	}

	/// <summary>
	/// Maps provided IEnumerable to a List.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public List<T?> ToList<T>()
	{
		return To<T>().ToList();
	}
}
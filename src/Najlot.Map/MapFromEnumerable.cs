using Najlot.Map.Exceptions;

namespace Najlot.Map;

/// <summary>
/// Maps from an enumerable.
/// </summary>
public readonly struct MapFromEnumerable<TFrom>(
	Map map,
	IEnumerable<TFrom> from,
	IReadOnlyDictionary<Type, Delegate>? mapRegistrations,
	IReadOnlyDictionary<Type, Delegate>? mapFactoryRegistrations)
{
	/// <summary>
	/// Maps provided IEnumerable to a new IEnumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public readonly IEnumerable<T> To<T>()
	{
		var targetType = typeof(T);

		if (mapFactoryRegistrations != null && mapFactoryRegistrations.TryGetValue(targetType, out var factoryRegistration))
		{
			var factoryMethod = (MapFactoryMethod<TFrom, T>)factoryRegistration;
			foreach (var item in from)
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
			foreach (var item in from)
			{
				var t = map.Create<T>();
				method(map, item, t);
				yield return t;
			}
		}
	}

	/// <summary>
	/// Maps provided IEnumerable to an array.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public T[] ToArray<T>()
	{
		return To<T>().ToArray();
	}

	/// <summary>
	/// Maps provided IEnumerable to a List.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public List<T> ToList<T>()
	{
		return To<T>().ToList();
	}

	/// <summary>
	/// Maps provided IEnumerable into an existing List.
	/// Does not work with map factories and modifies existing items.
	/// </summary>
	/// <typeparam name="T">Type of the elements in the list</typeparam>
	/// <param name="to">List to map into</param>
	/// <returns></returns>
	public List<T> ToList<T>(List<T> to)
	{
		var targetType = typeof(T);
		int count;

		if (mapRegistrations == null || !mapRegistrations.TryGetValue(targetType, out var registration))
		{
			throw new MapNotRegisteredException(typeof(TFrom), targetType);
		}

		var mapMethod = (MapMethod<TFrom, T>)registration;

		if (from is System.Collections.ICollection collection)
		{
			count = collection.Count;
		}
		else
		{
			count = from.Count();
		}

		while (to.Count > count)
		{
			to.RemoveAt(to.Count - 1);
		}

		while (to.Count < count)
		{
			var t = map.Create<T>();
			to.Add(t);
		}

		int i = 0;
		foreach (var f in from)
		{
			mapMethod(map, f, to[i++]);
		}

		return to;
	}
}
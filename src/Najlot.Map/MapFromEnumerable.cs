﻿using Najlot.Map.Exceptions;

namespace Najlot.Map;

internal class MapFromEnumerable<TFrom>(
	IMap map,
	IEnumerable<TFrom> from,
	FactoryMethod factory,
	IReadOnlyDictionary<Type, Delegate> mapRegistrations,
	IReadOnlyDictionary<Type, Delegate> mapFactoryRegistrations) : IMapFromEnumerable
{
	public IEnumerable<T> To<T>()
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

		foreach (var item in from)
		{
			yield return factoryMethod(map, item);
		}
	}

	public T[] ToArray<T>()
	{
		return To<T>().ToArray();
	}

	public List<T> ToList<T>()
	{
		return To<T>().ToList();
	}

	public List<T> ToList<T>(List<T> to)
	{
		var targetType = typeof(T);
		int count;

		if (!mapRegistrations.TryGetValue(targetType, out var registration))
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
			to.Add((T)factory(targetType));
		}

		int i = 0;
		foreach (var f in from)
		{
			mapMethod(map, f, to[i++]);
		}

		return to;
	}
}
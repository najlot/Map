using Najlot.Map.Exceptions;
using System.Linq.Expressions;

namespace Najlot.Map;

/// <summary>
/// Maps from a queryable.
/// </summary>
public readonly struct MapFromQueryable<TFrom>(
	IQueryable<TFrom> from,
	IReadOnlyDictionary<Type, Expression>? expressionRegistrations)
{
	/// <summary>
	/// Projects provided IQueryable to a new IQueryable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public readonly IQueryable<T> To<T>()
	{
		var targetType = typeof(T);

		if (expressionRegistrations == null || !expressionRegistrations.TryGetValue(targetType, out var registration))
		{
			throw new MapNotRegisteredException(typeof(TFrom), targetType);
		}

		var expression = (Expression<Func<TFrom, T>>)registration;
		return from.Select(expression);
	}

	public readonly List<T> ToList<T>() => To<T>().ToList();
	public readonly T[] ToArray<T>() => To<T>().ToArray();
}

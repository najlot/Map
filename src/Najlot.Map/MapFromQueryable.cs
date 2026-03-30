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

	/// <summary>
	/// Projects the source queryable and materializes the results as a list.
	/// </summary>
	/// <typeparam name="T">Destination element type</typeparam>
	/// <returns>A list containing the projected results.</returns>
	public readonly List<T> ToList<T>() => To<T>().ToList();

	/// <summary>
	/// Projects the source queryable and materializes the results as an array.
	/// </summary>
	/// <typeparam name="T">Destination element type</typeparam>
	/// <returns>An array containing the projected results.</returns>
	public readonly T[] ToArray<T>() => To<T>().ToArray();
}

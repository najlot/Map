using System.Linq.Expressions;
using System.Reflection;

namespace Najlot.Map;

public partial class Map
{
	private static Func<T> CreateFactory<T>()
	{
		var type = typeof(T);

		if (type.IsValueType)
		{
			return static () => default!;
		}

		var ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, binder: null, Type.EmptyTypes, modifiers: null);
		if (ctor is null)
		{
			return static () => Activator.CreateInstance<T>();
		}

		var newExpr = Expression.New(ctor);
		var lambda = Expression.Lambda<Func<T>>(newExpr);
		return lambda.Compile();
	}

	// This static class is initialized once per type T.
	// We should not use static caching, but in this case the types should not change during runtime.
	private static class Cache<T>
	{
		public static readonly bool HasPublicParameterlessCtor =
			typeof(T).GetConstructor(
				BindingFlags.Public | BindingFlags.Instance,
				binder: null,
				Type.EmptyTypes,
				modifiers: null) is not null;

		public static readonly Func<T> Factory = CreateFactory<T>();
	}
}

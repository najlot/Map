using System.Linq.Expressions;
using System.Reflection;

namespace Najlot.Map;

internal interface IMapDelegateRegistrator
{
	void RegisterSimpleMap<T>(T maps, Map map, MethodInfo method);
	void RegisterMap<T>(T maps, Map map, MethodInfo method);
	void RegisterSimpleFactory<T>(T maps, Map map, MethodInfo method);
	void RegisterFactory<T>(T maps, Map map, MethodInfo method);
	void RegisterExpression<T>(T maps, Map map, MethodInfo method);
}

internal class MapDelegateRegistrator<TFrom, TTo> : IMapDelegateRegistrator
{
	public void RegisterSimpleMap<T>(T maps, Map map, MethodInfo method)
	{
		var methodType = typeof(SimpleMapMethod<TFrom, TTo>);
		var mapDelegate = method.IsStatic ? method.CreateDelegate(methodType) : method.CreateDelegate(methodType, maps);
		map.Register((SimpleMapMethod<TFrom, TTo>)mapDelegate);
	}

	public void RegisterMap<T>(T maps, Map map, MethodInfo method)
	{
		var methodType = typeof(MapMethod<TFrom, TTo>);
		var mapDelegate = method.IsStatic ? method.CreateDelegate(methodType) : method.CreateDelegate(methodType, maps);
		map.Register((MapMethod<TFrom, TTo>)mapDelegate);
	}

	public void RegisterSimpleFactory<T>(T maps, Map map, MethodInfo method)
	{
		var methodType = typeof(SimpleMapFactoryMethod<TFrom, TTo>);
		var mapDelegate = method.IsStatic ? method.CreateDelegate(methodType) : method.CreateDelegate(methodType, maps);
		map.Register((SimpleMapFactoryMethod<TFrom, TTo>)mapDelegate);
	}

	public void RegisterFactory<T>(T maps, Map map, MethodInfo method)
	{
		var methodType = typeof(MapFactoryMethod<TFrom, TTo>);
		var mapDelegate = method.IsStatic ? method.CreateDelegate(methodType) : method.CreateDelegate(methodType, maps);
		map.Register((MapFactoryMethod<TFrom, TTo>)mapDelegate);
	}

	public void RegisterExpression<T>(T maps, Map map, MethodInfo method)
	{
		var expression = (Expression<Func<TFrom, TTo>>)method.Invoke(maps, null)!;
		map.RegisterExpression(expression);
	}
}

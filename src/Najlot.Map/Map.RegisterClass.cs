using System.Reflection;

namespace Najlot.Map;

public partial class Map
{
	/// <summary>
	/// Registers all map delegates in a class.
	/// The delegates must have the signature of MapMethod or SimpleMapMethod.
	/// </summary>
	/// <typeparam name="T">Type of the class</typeparam>
	/// <returns>This instance</returns>
	public IMap Register<T>()
	{
		var maps = Create<T>();
		return Register(maps);
	}

	/// <summary>
	/// Registers all map delegates in a class.
	/// The delegates must have the signature void MethodName(IMap map, TFrom from, TTo to) or void MethodName(TFrom from, TTo to).
	/// </summary>
	/// <typeparam name="T">Type of the class</typeparam>
	/// <param name="maps">Instance of the class</param>
	/// <returns>This instance</returns>
	public IMap Register<T>(T maps)
	{
		var type = typeof(T);
		var methods = type.GetMethods();
		var convertToMapMethod = typeof(Map)
			.GetMethod(nameof(ConvertToMapMethod), BindingFlags.NonPublic | BindingFlags.Static)
			?? throw new Exception("Method " + nameof(ConvertToMapMethod) + " not found.");

		var convertToMapFactoryMethod = typeof(Map)
			.GetMethod(nameof(ConvertToMapFactoryMethod), BindingFlags.NonPublic | BindingFlags.Static)
			?? throw new Exception("Method " + nameof(ConvertToMapFactoryMethod) + " not found.");

		foreach (var method in methods)
		{
			if (method.DeclaringType == typeof(object))
			{
				continue;
			}

			if (method.ReturnType == typeof(void))
			{
				TryRegisterMapMethod(maps, method, convertToMapMethod);
			}
			else
			{
				TryRegisterMapFactoryMethod(maps, method, convertToMapFactoryMethod);
			}
		}

		return this;
	}

	private void TryRegisterMapFactoryMethod<T>(T maps, MethodInfo method, MethodInfo convertToMapFactoryMethod)
	{
		var parameters = method.GetParameters();

		if (parameters.Length == 1)
		{
			var parameterTypes = new Type[] { parameters[0].ParameterType, method.ReturnType };

			Delegate simpleMapDelegate;

			if (method.IsStatic)
			{
				simpleMapDelegate = method.CreateDelegate(typeof(SimpleMapFactoryMethod<,>).MakeGenericType(parameterTypes));
			}
			else
			{
				simpleMapDelegate = method.CreateDelegate(typeof(SimpleMapFactoryMethod<,>).MakeGenericType(parameterTypes), maps);
			}

			if (convertToMapFactoryMethod.MakeGenericMethod(parameterTypes).Invoke(null, [simpleMapDelegate]) is Delegate mapDelegate)
			{
				RegisterFactory(parameterTypes[0], parameterTypes[1], mapDelegate);
				_mapFactoryDelegates.Add(simpleMapDelegate);
			}
		}
		else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(IMap))
		{
			var parameterTypes = new Type[] { parameters[1].ParameterType, method.ReturnType };

			Delegate mapDelegate;

			if (method.IsStatic)
			{
				mapDelegate = method.CreateDelegate(typeof(MapFactoryMethod<,>).MakeGenericType(parameterTypes[0], parameterTypes[1]));
			}
			else
			{
				mapDelegate = method.CreateDelegate(typeof(MapFactoryMethod<,>).MakeGenericType(parameterTypes[0], parameterTypes[1]), maps);
			}

			RegisterFactory(parameterTypes[0], parameterTypes[1], mapDelegate);
			_mapFactoryDelegates.Add(mapDelegate);
		}
	}

	private void TryRegisterMapMethod<T>(T maps, MethodInfo method, MethodInfo convertToMapMethod)
	{
		var parameters = method.GetParameters();

		if (parameters.Length == 2)
		{
			var parameterTypes = new Type[] { parameters[0].ParameterType, parameters[1].ParameterType };

			Delegate simpleMapDelegate;

			if (method.IsStatic)
			{
				simpleMapDelegate = method.CreateDelegate(typeof(SimpleMapMethod<,>).MakeGenericType(parameterTypes));
			}
			else
			{
				simpleMapDelegate = method.CreateDelegate(typeof(SimpleMapMethod<,>).MakeGenericType(parameterTypes), maps);
			}

			if (convertToMapMethod.MakeGenericMethod(parameterTypes).Invoke(null, [simpleMapDelegate]) is Delegate mapDelegate)
			{
				RegisterMap(parameterTypes[0], parameterTypes[1], mapDelegate);
				_mapDelegates.Add(simpleMapDelegate);
			}
		}
		else if (parameters.Length == 3 && parameters[0].ParameterType == typeof(IMap))
		{
			var parameterTypes = new Type[] { parameters[1].ParameterType, parameters[2].ParameterType };

			Delegate mapDelegate;

			if (method.IsStatic)
			{
				mapDelegate = method.CreateDelegate(typeof(MapMethod<,>).MakeGenericType(parameterTypes[0], parameterTypes[1]));
			}
			else
			{
				mapDelegate = method.CreateDelegate(typeof(MapMethod<,>).MakeGenericType(parameterTypes[0], parameterTypes[1]), maps);
			}

			RegisterMap(parameterTypes[0], parameterTypes[1], mapDelegate);
			_mapDelegates.Add(mapDelegate);
		}
	}

	private void RegisterMap(Type from, Type to, Delegate method)
	{
		if (!_registrations.TryGetValue(from, out var regs))
		{
			regs = new TypeRegistrations();
			_registrations[from] = regs;
		}

		regs.Maps ??= [];
		regs.Maps[to] = method;
	}

	private void RegisterFactory(Type from, Type to, Delegate method)
	{
		if (!_registrations.TryGetValue(from, out var regs))
		{
			regs = new TypeRegistrations();
			_registrations[from] = regs;
		}

		regs.Factories ??= [];
		regs.Factories[to] = method;
	}

	private static Delegate ConvertToMapFactoryMethod<TFrom, TTo>(Delegate simpleMapDelegate)
	{
		var method = (SimpleMapFactoryMethod<TFrom, TTo>)simpleMapDelegate;

		TTo Map(IMap map, TFrom from)
		{
			return method(from);
		}

		return new MapFactoryMethod<TFrom, TTo>(Map);
	}

	private static Delegate ConvertToMapMethod<TFrom, TTo>(Delegate simpleMapDelegate)
	{
		var method = (SimpleMapMethod<TFrom, TTo>)simpleMapDelegate;

		void Map(IMap map, TFrom from, TTo to)
		{
			method(from, to);
		}

		return new MapMethod<TFrom, TTo>(Map);
	}
}
using System.Linq.Expressions;
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
	/// The delegates must be public and have one of the signatures:
	/// void MethodName(IMap map, TFrom from, TTo to)
	/// void MethodName(TFrom from, TTo to)
	/// TTo MethodName(IMap map, TFrom from)
	/// TTo MethodName(TFrom from)
	/// </summary>
	/// <typeparam name="T">Type of the class</typeparam>
	/// <param name="maps">Instance of the class</param>
	/// <returns>This instance</returns>
	public IMap Register<T>(T maps)
	{
		var methods = typeof(T)
			.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
		
		foreach (var method in methods)
		{
			if (method.ReturnType == typeof(void))
			{
				TryRegisterMapMethod(maps, method);
			}
			else if (IsExpressionType(method.ReturnType))
			{
				TryRegisterExpressionMethod(maps, method);
			}
			else
			{
				TryRegisterMapFactoryMethod(maps, method);
			}
		}

		return this;
	}

	private static bool IsExpressionType(Type type)
	{
		return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Expression<>);
	}

	private void TryRegisterExpressionMethod<T>(T maps, MethodInfo method)
	{
		var returnType = method.ReturnType;
		var funcType = returnType.GetGenericArguments()[0];

		if (!funcType.IsGenericType || funcType.GetGenericTypeDefinition() != typeof(Func<,>))
		{
			return;
		}

		var genericArgs = funcType.GetGenericArguments();
		var fromType = genericArgs[0];
		var toType = genericArgs[1];

		var parameters = method.GetParameters();
		if (parameters.Length == 0)
		{
			var registratorType = typeof(MapDelegateRegistrator<,>).MakeGenericType(fromType, toType);
			var registrator = (IMapDelegateRegistrator)Activator.CreateInstance(registratorType)!;
			registrator.RegisterExpression(maps, this, method);
		}
	}

	private void TryRegisterMapFactoryMethod<T>(T maps, MethodInfo method)
	{
		var parameters = method.GetParameters();

		if (parameters.Length == 1)
		{
			var fromType = parameters[0].ParameterType;
			var toType = method.ReturnType;
			var registratorType = typeof(MapDelegateRegistrator<,>).MakeGenericType(fromType, toType);
			var registrator = (IMapDelegateRegistrator)Activator.CreateInstance(registratorType)!;
			registrator.RegisterSimpleFactory(maps, this, method);
		}
		else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(IMap))
		{
			var fromType = parameters[1].ParameterType;
			var toType = method.ReturnType;
			var registratorType = typeof(MapDelegateRegistrator<,>).MakeGenericType(fromType, toType);
			var registrator = (IMapDelegateRegistrator)Activator.CreateInstance(registratorType)!;
			registrator.RegisterFactory(maps, this, method);
		}
	}

	private void TryRegisterMapMethod<T>(T maps, MethodInfo method)
	{
		var parameters = method.GetParameters();

		if (parameters.Length == 2)
		{
			var fromType = parameters[0].ParameterType;
			var toType = parameters[1].ParameterType;
			var registratorType = typeof(MapDelegateRegistrator<,>).MakeGenericType(fromType, toType);
			var registrator = (IMapDelegateRegistrator)Activator.CreateInstance(registratorType)!;
			registrator.RegisterSimpleMap(maps, this, method);
		}
		else if (parameters.Length == 3 && parameters[0].ParameterType == typeof(IMap))
		{
			var fromType = parameters[1].ParameterType;
			var toType = parameters[2].ParameterType;
			var registratorType = typeof(MapDelegateRegistrator<,>).MakeGenericType(fromType, toType);
			var registrator = (IMapDelegateRegistrator)Activator.CreateInstance(registratorType)!;
			registrator.RegisterMap(maps, this, method);
		}
	}
}
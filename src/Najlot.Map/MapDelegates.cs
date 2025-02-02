namespace Najlot.Map;

/// <summary>
/// Factory method delegate.
/// </summary>
/// <param name="type"></param>
/// <returns></returns>
public delegate object FactoryMethod(Type type);

/// <summary>
/// Simple method delegate to create and map an object.
/// </summary>
/// <typeparam name="TFrom"></typeparam>
/// <typeparam name="TTo"></typeparam>
/// <param name="from"></param>
/// <returns></returns>
public delegate TTo SimpleMapFactoryMethod<TFrom, TTo>(TFrom from);

/// <summary>
/// Method delegate to create and map an object.
/// </summary>
/// <typeparam name="TFrom"></typeparam>
/// <typeparam name="TTo"></typeparam>
/// <param name="map"></param>
/// <param name="from"></param>
/// <returns></returns>
public delegate TTo MapFactoryMethod<TFrom, TTo>(IMap map, TFrom from);

/// <summary>
/// Simple map delegate.
/// </summary>
/// <typeparam name="TFrom"></typeparam>
/// <typeparam name="TTo"></typeparam>
/// <param name="from"></param>
/// <param name="to"></param>
public delegate void SimpleMapMethod<TFrom, TTo>(TFrom from, TTo to);

/// <summary>
/// Map delegate.
/// </summary>
/// <typeparam name="TFrom"></typeparam>
/// <typeparam name="TTo"></typeparam>
/// <param name="map"></param>
/// <param name="from"></param>
/// <param name="to"></param>
public delegate void MapMethod<TFrom, TTo>(IMap map, TFrom from, TTo to);
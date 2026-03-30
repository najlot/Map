namespace Najlot.Map.Attributes;

/// <summary>
/// Instructs validation to verify which source properties are read instead of which target properties are written.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MapValidateSourceAttribute : Attribute
{
}

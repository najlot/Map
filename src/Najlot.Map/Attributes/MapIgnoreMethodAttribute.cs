namespace Najlot.Map.Attributes;

/// <summary>
/// Excludes a mapping method from validation and generator registration.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MapIgnoreMethodAttribute() : Attribute
{
}
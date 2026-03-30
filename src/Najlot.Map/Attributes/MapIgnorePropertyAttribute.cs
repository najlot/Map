namespace Najlot.Map.Attributes;

/// <summary>
/// Marks a property name to be ignored in mapping source generation
/// and as intentionally ignored during mapping validation.
/// </summary>
/// <param name="propertyName">The property name to ignore.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MapIgnorePropertyAttribute(string propertyName) : Attribute
{
	/// <summary>
	/// Gets the property name that should be ignored during validation.
	/// </summary>
	public string PropertyName { get; } = propertyName;
}
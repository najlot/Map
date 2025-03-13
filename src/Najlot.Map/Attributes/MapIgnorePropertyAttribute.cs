namespace Najlot.Map.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MapIgnorePropertyAttribute(string propertyName) : Attribute
{
	public string PropertyName { get; } = propertyName;
}